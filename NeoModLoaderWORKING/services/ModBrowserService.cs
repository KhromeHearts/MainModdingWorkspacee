using System.Runtime.ExceptionServices;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;

namespace NeoModLoader.services;

internal enum ModBrowserStatusKind
{
    NotInstalled,
    WaitingForSteam,
    Queued,
    Subscribing,
    Downloading,
    Installing,
    Failed,
    UpdateAvailable,
    Incompatible,
    MissingDependency,
    Enabled,
    EnabledNotLoaded,
    Disabled,
    LoadFailed,
    RestartRequired,
    InstalledLocalCopy,

    /// <summary>
    ///     Installed, but not known to NML this session (installed outside the browser after startup).
    /// </summary>
    Installed,

    /// <summary>
    ///     Unsubscribed; Steam removes the files later.
    /// </summary>
    UninstallPending
}

internal sealed class ModBrowserStatus
{
    public ModBrowserStatusKind Kind { get; set; }
    public float Progress { get; set; }

    /// <summary>
    ///     Why the item failed or is incompatible, missing dependencies, etc.
    /// </summary>
    public string Detail { get; set; }

    public WorkshopItemLocalState Local { get; set; }

    /// <summary>
    ///     mod.json of the installed Workshop copy.
    /// </summary>
    public ModDeclare InstalledMod { get; set; }

    /// <summary>
    ///     The declaration NML uses for this UID, possibly from another folder.
    /// </summary>
    public ModDeclare RecognizedMod { get; set; }

    public List<string> MissingDependencies { get; set; } = new();
    public List<string> IncompatibleMods { get; set; } = new();

    public bool IsBusy => Kind is ModBrowserStatusKind.Queued or ModBrowserStatusKind.Subscribing
        or ModBrowserStatusKind.WaitingForSteam or ModBrowserStatusKind.Downloading
        or ModBrowserStatusKind.Installing;
}

internal sealed class ModBrowserDependencyMatch
{
    public string Uid { get; set; }

    /// <summary>
    ///     Null when nothing on the Workshop matched.
    /// </summary>
    public WorkshopModInfo Candidate { get; set; }
}

internal sealed class ModBrowserApplyResult
{
    public List<ModDeclare> Loaded { get; } = new();
    public List<KeyValuePair<ModDeclare, string>> Failed { get; } = new();
}

/// <summary>
///     Mod Browser logic on top of <see cref="ModWorkshopService" /> and NML's mod services.
/// </summary>
/// <remarks>
///     Nothing here compiles or loads a mod on its own: installs are recorded as disabled unless the user opted in,
///     and loading always goes through an explicit Enable or Apply.
/// </remarks>
[Experimental]
internal static class ModBrowserService
{
    private static readonly Dictionary<string, ModDeclare> mod_json_cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<ulong, WorkshopModInfo> details_cache = new();
    private static readonly Dictionary<ulong, string> owner_names = new();
    private static readonly HashSet<ulong> owner_name_requests = new();
    private static readonly HashSet<string> restart_required = new();
    private static readonly HashSet<string> pending_apply = new();
    private static Dictionary<string, string> local_mod_index;
    private static WorkshopDownloadQueue queue;
    private static ModBrowserSettings settings;
    private static bool state_read_error_logged;

    /// <summary>
    ///     An item's queue entry or install state changed. Raised on the main thread.
    /// </summary>
    public static event Action<ulong> ItemChanged;

    public static event Action<WorkshopQueueSummary> QueueFinished;

    /// <summary>
    ///     A Steam persona name arrived (argument: Steam id).
    /// </summary>
    public static event Action<ulong> OwnerNameResolved;

    public static ModBrowserSettings Settings => settings ??= ModBrowserSettings.Load(SettingsPath);
    private static string SettingsPath => Path.Combine(Paths.NMLPath, "mod_browser_settings.json");

    /// <summary>
    ///     Created on first use; requires the Workshop backend, so check Steam first.
    /// </summary>
    public static WorkshopDownloadQueue Queue
    {
        get
        {
            if (queue != null) return queue;
            queue = new WorkshopDownloadQueue(ModWorkshopService.Backend, new InstallHandler());
            queue.EntryChanged += entry => ItemChanged?.Invoke(entry.FileId);
            queue.Finished += summary =>
            {
                InvalidateLocalCaches();
                QueueFinished?.Invoke(summary);
            };
            return queue;
        }
    }

    public static bool IsQueueRunning => queue?.IsRunning ?? false;

    /// <summary>
    ///     Mods installed with "Enable after install" that Apply can load now.
    /// </summary>
    public static int PendingApplyCount => pending_apply.Count;

    public static void SaveSettings()
    {
        Settings.Save(SettingsPath);
    }

    public static WorkshopBrowseSession CreateSession()
    {
        return new WorkshopBrowseSession(ModWorkshopService.QueryModsAsync);
    }

    /// <summary>
    ///     Drop cached mod.json reads and the local mod index, e.g. after installs or when the user refreshes.
    /// </summary>
    public static void InvalidateLocalCaches()
    {
        mod_json_cache.Clear();
        local_mod_index = null;
    }

    public static ModBrowserStatus GetStatus(ulong pFileId)
    {
        var status = new ModBrowserStatus();
        WorkshopQueueEntry entry = queue?.Find(pFileId);
        if (entry is { IsFinished: false })
        {
            status.Kind = entry.State switch
            {
                WorkshopQueueState.Subscribing => ModBrowserStatusKind.Subscribing,
                WorkshopQueueState.WaitingForSteam => ModBrowserStatusKind.WaitingForSteam,
                WorkshopQueueState.Downloading => ModBrowserStatusKind.Downloading,
                WorkshopQueueState.Installing => ModBrowserStatusKind.Installing,
                _ => ModBrowserStatusKind.Queued
            };
            status.Progress = entry.Progress;
            return status;
        }

        WorkshopItemLocalState local = GetLocalState(pFileId);
        status.Local = local;
        if (entry is { State: WorkshopQueueState.Failed })
        {
            status.Kind = ModBrowserStatusKind.Failed;
            status.Detail = entry.Error;
            return status;
        }

        if (local.Downloading || local.DownloadPending)
        {
            status.Kind = local.Downloading ? ModBrowserStatusKind.Downloading : ModBrowserStatusKind.WaitingForSteam;
            status.Progress = local.DownloadProgress;
            return status;
        }

        if (!local.Installed)
        {
            status.Kind = local.Subscribed ? ModBrowserStatusKind.WaitingForSteam : ModBrowserStatusKind.NotInstalled;
            return status;
        }

        if (!local.Subscribed)
        {
            status.Kind = ModBrowserStatusKind.UninstallPending;
            return status;
        }

        ModDeclare mod = ReadModJson(local.Directory);
        status.InstalledMod = mod;
        if (mod == null)
        {
            status.Kind = ModBrowserStatusKind.Incompatible;
            status.Detail = LM.Get("mod_browser_detail_no_mod_json");
            return status;
        }

        if (mod.ModType == ModTypeEnum.BEPINEX)
        {
            // NML never loads BepInEx mods itself; it links enabled ones into BepInEx at startup.
            bool bepinex_disabled = ModInfoUtils.isModDisabled(mod.UID);
            status.Kind = bepinex_disabled ? ModBrowserStatusKind.Disabled : ModBrowserStatusKind.Installed;
            status.Detail = LM.Get(bepinex_disabled
                ? "mod_browser_detail_bepinex_disabled"
                : "mod_browser_detail_bepinex");
            return status;
        }

        ModInfoUtils.TryGetRecognizedMod(mod.UID, out ModDeclare recognized);
        status.RecognizedMod = recognized;
        status.MissingDependencies = mod.Dependencies.Where(uid => !IsModAvailable(uid)).ToList();
        status.IncompatibleMods = mod.IncompatibleWith.Where(IsModWanted).ToList();

        if (local.NeedsUpdate)
        {
            status.Kind = ModBrowserStatusKind.UpdateAvailable;
        }
        else if (recognized != null && !SameFolder(recognized.FolderPath, mod.FolderPath))
        {
            status.Kind = ModBrowserStatusKind.InstalledLocalCopy;
            status.Detail = recognized.FolderPath;
        }
        else if (restart_required.Contains(mod.UID))
        {
            status.Kind = ModBrowserStatusKind.RestartRequired;
        }
        else if (status.MissingDependencies.Count > 0)
        {
            status.Kind = ModBrowserStatusKind.MissingDependency;
            status.Detail = string.Join(", ", status.MissingDependencies);
        }
        else if (status.IncompatibleMods.Count > 0)
        {
            status.Kind = ModBrowserStatusKind.Incompatible;
            status.Detail = string.Format(LM.Get("mod_browser_detail_incompatible"),
                string.Join(", ", status.IncompatibleMods));
        }
        else if (recognized == null)
        {
            status.Kind = ModBrowserStatusKind.Installed;
        }
        else
        {
            bool disabled = ModInfoUtils.isModDisabled(mod.UID);
            switch (WorldBoxMod.AllRecognizedMods[recognized])
            {
                case ModState.LOADED:
                    // Disabled while loaded: it stays active until the game restarts.
                    status.Kind = disabled ? ModBrowserStatusKind.RestartRequired : ModBrowserStatusKind.Enabled;
                    break;
                case ModState.DISABLED:
                    status.Kind = ModBrowserStatusKind.Disabled;
                    break;
                default:
                    string reason = recognized.FailReason.ToString().Trim();
                    if (disabled)
                    {
                        status.Kind = ModBrowserStatusKind.Disabled;
                    }
                    else if (reason.Length > 0)
                    {
                        status.Kind = ModBrowserStatusKind.LoadFailed;
                        status.Detail = reason;
                    }
                    else
                    {
                        status.Kind = ModBrowserStatusKind.EnabledNotLoaded;
                    }

                    break;
            }
        }

        return status;
    }

    /// <summary>
    ///     Steam persona name of the uploader, or null while it is being requested.
    /// </summary>
    public static string GetOwnerName(WorkshopModInfo pItem)
    {
        if (pItem.OwnerName != null) return pItem.OwnerName;
        if (owner_names.TryGetValue(pItem.OwnerSteamId, out string name))
        {
            pItem.OwnerName = name;
            return name;
        }

        if (owner_name_requests.Add(pItem.OwnerSteamId))
        {
            _ = ResolveOwnerNameAsync(pItem.OwnerSteamId);
        }

        return null;
    }

    /// <summary>
    ///     The item with its full description (one Steam query, cached).
    /// </summary>
    public static async Task<WorkshopModInfo> GetDetailsAsync(WorkshopModInfo pItem)
    {
        if (pItem.IsLongDescription) return pItem;
        if (details_cache.TryGetValue(pItem.FileId, out WorkshopModInfo cached)) return cached;

        WorkshopModInfo details = await ModWorkshopService.QueryModDetailsAsync(pItem.FileId) ?? pItem;
        details.OwnerName ??= pItem.OwnerName;
        details_cache[pItem.FileId] = details;
        return details;
    }

    /// <summary>
    ///     Items of <paramref name="pItems" /> that are not installed or need an update, and are not queued.
    /// </summary>
    public static List<WorkshopModInfo> GetInstallCandidates(IEnumerable<WorkshopModInfo> pItems)
    {
        var candidates = new List<WorkshopModInfo>();
        foreach (WorkshopModInfo item in pItems)
        {
            if (item.IsModLoader) continue;
            if (queue?.Find(item.FileId) is { IsFinished: false }) continue;
            WorkshopItemLocalState local = GetLocalState(item.FileId);
            if (!local.Installed || local.NeedsUpdate) candidates.Add(item);
        }

        return candidates;
    }

    /// <summary>
    ///     Queue an install, or an update if the item is installed.
    /// </summary>
    public static WorkshopQueueEntry Enqueue(WorkshopModInfo pItem, bool pEnableAfterInstall)
    {
        return Queue.Enqueue(new[] { CreateRequest(pItem, pEnableAfterInstall, null) })[0];
    }

    /// <summary>
    ///     Queue several items as one batch (Download All, Update All).
    /// </summary>
    public static List<WorkshopQueueEntry> EnqueueAll(IEnumerable<WorkshopModInfo> pItems, bool pEnableAfterInstall)
    {
        return Queue.Enqueue(pItems.Select(item => CreateRequest(item, pEnableAfterInstall, null)).ToList());
    }

    /// <summary>
    ///     Queue dependency candidates. Each must turn out to provide its UID, or it is rolled back.
    /// </summary>
    public static List<WorkshopQueueEntry> EnqueueDependencies(IEnumerable<ModBrowserDependencyMatch> pMatches)
    {
        return Queue.Enqueue(pMatches.Where(match => match.Candidate != null)
            .Select(match => CreateRequest(match.Candidate, false, match.Uid)).ToList());
    }

    private static WorkshopQueueEntry CreateRequest(WorkshopModInfo pItem, bool pEnableAfterInstall,
        string pExpectedUid)
    {
        details_cache.Remove(pItem.FileId);
        WorkshopQueueAction action = GetLocalState(pItem.FileId).Installed
            ? WorkshopQueueAction.Update
            : WorkshopQueueAction.Install;
        return new WorkshopQueueEntry(pItem.FileId, pItem.Title, action, pItem.FileSizeBytes, pEnableAfterInstall,
            pExpectedUid);
    }

    /// <summary>
    ///     Subscribed mods that Steam reports as needing an update.
    /// </summary>
    public static async Task<List<WorkshopModInfo>> FindUpdatesAsync()
    {
        WorkshopBrowseSession session = CreateSession();
        await session.ResetAsync(new WorkshopBrowseQuery { Scope = WorkshopBrowseScope.Subscribed });
        if (session.LastError != null) ExceptionDispatchInfo.Capture(session.LastError).Throw();

        return session.Items.Where(item =>
        {
            WorkshopItemLocalState local = GetLocalState(item.FileId);
            return local.Installed && local.NeedsUpdate;
        }).ToList();
    }

    /// <summary>
    ///     Search the Workshop for items that may provide each missing dependency UID.
    /// </summary>
    public static async Task<List<ModBrowserDependencyMatch>> FindDependencyCandidatesAsync(IEnumerable<string> pUids)
    {
        var matches = new List<ModBrowserDependencyMatch>();
        foreach (string uid in pUids.Distinct())
        {
            WorkshopModInfo candidate = null;
            foreach (string term in ModBrowserDependencyResolver.BuildSearchTerms(uid))
            {
                WorkshopBrowsePage page = await ModWorkshopService.QueryModsAsync(new WorkshopBrowseQuery
                {
                    SearchText = term,
                    Sort = WorkshopBrowseSort.Relevance
                }, 1);
                candidate = ModBrowserDependencyResolver.PickCandidate(uid, page.Items);
                if (candidate != null) break;
            }

            LogService.LogInfo(candidate == null
                ? $"No Workshop item found for missing dependency {uid}"
                : $"Workshop item {candidate.FileId} ({candidate.Title}) may provide dependency {uid}");
            matches.Add(new ModBrowserDependencyMatch { Uid = uid, Candidate = candidate });
        }

        return matches;
    }

    /// <summary>
    ///     Compile and load every mod installed with "Enable after install", through the existing runtime path.
    /// </summary>
    public static ModBrowserApplyResult ApplyPendingMods()
    {
        var result = new ModBrowserApplyResult();
        foreach (string uid in pending_apply.ToList())
        {
            if (!ModInfoUtils.TryGetRecognizedMod(uid, out ModDeclare mod) || ModCompileLoadService.IsModLoaded(uid) ||
                ModInfoUtils.isModDisabled(uid) || mod.ModType == ModTypeEnum.BEPINEX)
            {
                pending_apply.Remove(uid);
                continue;
            }

            if (ModCompileLoadService.TryEnableMod(mod, false))
            {
                result.Loaded.Add(mod);
                pending_apply.Remove(uid);
            }
            else
            {
                result.Failed.Add(new KeyValuePair<ModDeclare, string>(mod, mod.FailReason.ToString().Trim()));
            }
        }

        LogService.LogInfo($"Mod Browser apply: {result.Loaded.Count} loaded, {result.Failed.Count} failed");
        InvalidateLocalCaches();
        return result;
    }

    /// <summary>
    ///     Enable a mod now (compiles and loads it if needed), showing NML's error window on failure.
    /// </summary>
    public static bool EnableMod(ModDeclare pMod)
    {
        pending_apply.Remove(pMod.UID);
        if (pMod.ModType == ModTypeEnum.BEPINEX)
        {
            // Only allows NML to link it into BepInEx on the next start (which asks for administrator rights).
            ModInfoUtils.setModDisabled(pMod.UID, false);
            return true;
        }

        bool enabled = ModCompileLoadService.TryEnableMod(pMod);
        InvalidateLocalCaches();
        return enabled;
    }

    /// <summary>
    ///     Disable a mod. A loaded mod stays active until restart; an existing BepInEx link is not removed.
    /// </summary>
    public static void DisableMod(ModDeclare pMod)
    {
        pending_apply.Remove(pMod.UID);
        if (pMod.ModType == ModTypeEnum.BEPINEX)
        {
            ModInfoUtils.setModDisabled(pMod.UID, true);
            return;
        }

        ModCompileLoadService.DisableMod(pMod);
    }

    /// <summary>
    ///     Hot reload an updated, loaded <see cref="IReloadable" /> mod through <see cref="ModReloadService" />.
    /// </summary>
    public static bool ReloadMod(ModDeclare pMod)
    {
        if (!ModReloadService.ReloadMod(pMod)) return false;
        restart_required.Remove(pMod.UID);
        return true;
    }

    /// <summary>
    ///     Unsubscribe; Steam deletes the files when it can.
    /// </summary>
    public static async Task<bool> UninstallAsync(WorkshopModInfo pItem)
    {
        ModDeclare mod = ReadModJson(GetLocalState(pItem.FileId).Directory);
        await ModWorkshopService.EnsureSteamAvailableAsync();
        bool unsubscribed = await ModWorkshopService.Backend.UnsubscribeModAsync(pItem.FileId);
        if (unsubscribed && mod != null)
        {
            pending_apply.Remove(mod.UID);
            if (ModCompileLoadService.IsModLoaded(mod.UID)) restart_required.Add(mod.UID);
        }

        LogService.LogInfo(unsubscribed
            ? $"Unsubscribed from Workshop item {pItem.FileId} ({pItem.Title})"
            : $"Steam refused to unsubscribe from Workshop item {pItem.FileId} ({pItem.Title})");
        InvalidateLocalCaches();
        ItemChanged?.Invoke(pItem.FileId);
        return unsubscribed;
    }

    private static WorkshopItemLocalState GetLocalState(ulong pFileId)
    {
        if (!ModWorkshopService.IsSteamInitialized) return default;
        try
        {
            return ModWorkshopService.Backend.GetItemLocalState(pFileId);
        }
        catch (Exception e)
        {
            if (!state_read_error_logged)
            {
                state_read_error_logged = true;
                LogService.LogError($"Cannot read Workshop item state from Steam: {e.Message}");
                LogService.LogException(e);
            }

            return default;
        }
    }

    private static ModDeclare ReadModJson(string pDirectory)
    {
        if (string.IsNullOrEmpty(pDirectory)) return null;
        if (mod_json_cache.TryGetValue(pDirectory, out ModDeclare cached)) return cached;

        ModDeclare mod = Directory.Exists(pDirectory) ? ModInfoUtils.recogMod(pDirectory, false) : null;
        mod_json_cache[pDirectory] = mod;
        return mod;
    }

    private static bool IsModAvailable(string pUid)
    {
        if (ModInfoUtils.TryGetRecognizedMod(pUid, out _)) return true;
        local_mod_index ??= BuildLocalModIndex();
        return local_mod_index.ContainsKey(pUid);
    }

    private static bool IsModWanted(string pUid)
    {
        return ModInfoUtils.TryGetRecognizedMod(pUid, out _) &&
               (ModCompileLoadService.IsModLoaded(pUid) || !ModInfoUtils.isModDisabled(pUid));
    }

    /// <summary>
    ///     UIDs of every mod on disk, read without registering anything (unlike <see cref="ModInfoUtils.TryFindMod" />).
    /// </summary>
    private static Dictionary<string, string> BuildLocalModIndex()
    {
        var index = new Dictionary<string, string>();
        var roots = new List<string> { Paths.ModsPath, Paths.NativeModsPath };
        if (!Others.is_editor) roots.Add(Paths.CommonModsWorkshopPath);
        foreach (string root in roots.Where(Directory.Exists))
        {
            foreach (string folder in Directory.GetDirectories(root))
            {
                ModDeclare mod = ReadModJson(folder);
                if (mod != null && !index.ContainsKey(mod.UID)) index[mod.UID] = mod.FolderPath;
            }
        }

        return index;
    }

    private static bool SameFolder(string pPathA, string pPathB)
    {
        if (pPathA == null || pPathB == null) return false;
        static string normalize(string pPath)
        {
            return Path.GetFullPath(pPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return string.Equals(normalize(pPathA), normalize(pPathB), StringComparison.OrdinalIgnoreCase);
    }

    private static async Task ResolveOwnerNameAsync(ulong pSteamId)
    {
        string name = null;
        try
        {
            name = await ModWorkshopService.RequestPersonaNameAsync(pSteamId);
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot get the Steam name of user {pSteamId}: {e.Message}");
        }

        MainThreadDispatcher.Run(() =>
        {
            owner_names[pSteamId] = name ?? "";
            OwnerNameResolved?.Invoke(pSteamId);
        });
    }

    private sealed class InstallHandler : IWorkshopInstallHandler
    {
        public WorkshopInstallResult OnItemInstalled(WorkshopQueueEntry pEntry, string pDirectory)
        {
            InvalidateLocalCaches();
            details_cache.Remove(pEntry.FileId);
            ModDeclare mod = ReadModJson(pDirectory);
            if (mod == null)
            {
                return WorkshopInstallResult.Failure(LM.Get("mod_browser_error_not_nml"),
                    pEntry.ExpectedUid != null);
            }

            if (pEntry.ExpectedUid != null && mod.UID != pEntry.ExpectedUid)
            {
                return WorkshopInstallResult.Failure(
                    string.Format(LM.Get("mod_browser_error_wrong_dependency"), mod.UID, pEntry.ExpectedUid), true);
            }

            if (mod.ModType == ModTypeEnum.BEPINEX)
            {
                // NML links BepInEx mods at startup (an elevated mklink); never trigger that from here, and keep new
                // installs disabled so startup does not link them either unless the user opted in.
                if (pEntry.Action == WorkshopQueueAction.Install && !ModInfoUtils.TryGetRecognizedMod(mod.UID, out _))
                {
                    ModInfoUtils.setModDisabled(mod.UID, !pEntry.EnableAfterInstall);
                }

                return WorkshopInstallResult.Success(LM.Get(pEntry.EnableAfterInstall
                    ? "mod_browser_note_bepinex"
                    : "mod_browser_note_bepinex_disabled"));
            }

            if (string.IsNullOrEmpty(mod.RepoUrl)) mod.SetRepoUrlToWorkshopPage(pEntry.FileId.ToString());

            if (ModInfoUtils.TryGetRecognizedMod(mod.UID, out ModDeclare recognized))
            {
                if (!SameFolder(recognized.FolderPath, mod.FolderPath))
                {
                    return WorkshopInstallResult.Success(LM.Get("mod_browser_note_local_copy"));
                }

                // Updated files: make sure the next compile does not reuse the old build.
                ModInfoUtils.clearModCompileTimestamp(mod.UID);
                ModInfoUtils.InvalidateModUpdateTimestamp(mod.FolderPath);
                if (ModCompileLoadService.IsModLoaded(mod.UID))
                {
                    restart_required.Add(mod.UID);
                    return WorkshopInstallResult.Success(LM.Get("mod_browser_note_restart"));
                }

                ModInfoUtils.ReplaceRecognizedMod(recognized, mod);
                ModDepenSolveService.EnsureNode(mod);
                return WorkshopInstallResult.Success();
            }

            // New to this session. Installs start disabled unless the user opted in; updates keep their state.
            if (pEntry.Action == WorkshopQueueAction.Install)
            {
                ModInfoUtils.setModDisabled(mod.UID, !pEntry.EnableAfterInstall);
                if (pEntry.EnableAfterInstall) pending_apply.Add(mod.UID);
            }

            ModDepenSolveService.EnsureNode(ModInfoUtils.EnsureRecognizedMod(mod));
            return WorkshopInstallResult.Success(pEntry.EnableAfterInstall ? LM.Get("mod_browser_note_apply") : null);
        }
    }
}
