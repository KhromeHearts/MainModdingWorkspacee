using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using Newtonsoft.Json;
using RSG;
using UnityEngine;

namespace NeoModLoader.services;

[Experimental]
internal static class ModWorkshopService
{
    internal static Promise steamWorkshopPromise;
    private static IPlatformSpecificModWorkshopService workshopServiceBackend;

    public static void Init()
    {
        steamWorkshopPromise = RF.GetStaticField<Promise, SteamSDK>("steamInitialized");
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            workshopServiceBackend = new ModWorkshopServiceWindows();
        }
        else
        {
            workshopServiceBackend = new ModWorkshopServiceUnix();
        }
    }

    private static void UploadModLoader(string changelog)
    {
        workshopServiceBackend.UploadModLoader(changelog);
    }

    /// <summary>
    /// Try to Upload a mod to Steam Workshop
    /// </summary>
    public static Promise UploadMod(IMod mod, string changelog, bool verified = false)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string name = mod_decl.Name;
        string description = $"{name} Uploaded by NeoModLoader\n" +
                             $"{name} 由NeoModLoader上传\n\n" +
                             $"{mod_decl.Description}\n\n" +
                             $"ModLoader: {CoreConstants.RepoURL}\n\n" +
                             $"模组加载器: {CoreConstants.RepoURL}";
        string workshopPath = Path.Combine(SaveManager.generateMainPath("workshop_upload_mod") + mod_decl.UID);
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }

        if (!Directory.Exists(SaveManager.generateMainPath("workshop_upload_mod")))
        {
            Directory.CreateDirectory(SaveManager.generateMainPath("workshop_upload_mod"));
        }

        Directory.CreateDirectory(workshopPath);
        // Prepare files to upload
        List<string> files_to_upload = SystemUtils.SearchFileRecursive(mod_decl.FolderPath,
            (filename) =>
            {
                // To ignore .git and .vscode and so on files
                return !filename.StartsWith(".");
            },
            (dirname) =>
            {
                // To ignore .git and .vscode and so on files
                return !dirname.StartsWith(".") && !Paths.IgnoreSearchDirectories.Contains(dirname);
            });
        foreach (string file_full_path in files_to_upload)
        {
            string path = Path.Combine(workshopPath,
                file_full_path.Replace(mod_decl.FolderPath, "").Replace("\\", "/").Substring(1));

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            File.Copy(file_full_path, path);
        }

        string previewImagePath;
        if (string.IsNullOrEmpty(mod_decl.IconPath))
        {
            using Stream icon_stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("NeoModLoader.resources.logo.png");
            using FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
            icon_stream.Seek(0, SeekOrigin.Begin);
            icon_stream.CopyTo(icon_file);
            previewImagePath = Path.Combine(workshopPath, "preview.png");
        }
        else
        {
            previewImagePath = Path.Combine(workshopPath, mod_decl.IconPath);
        }

        // This works for BepInEx mods
        if (!File.Exists(Path.Combine(workshopPath, "mod.json")))
        {
            File.WriteAllText(Path.Combine(workshopPath, "mod.json"), JsonConvert.SerializeObject(mod_decl));
        }

        return workshopServiceBackend.UploadMod(name, description, previewImagePath, workshopPath, changelog, verified);
    }

    public static Promise TryEditMod(ulong fileID, IMod mod, string changelog)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string workshopPath = Path.Combine(SaveManager.generateMainPath("workshop_upload_mod") + mod_decl.UID);
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }

        if (!Directory.Exists(SaveManager.generateMainPath("workshop_upload_mod")))
        {
            Directory.CreateDirectory(SaveManager.generateMainPath("workshop_upload_mod"));
        }

        Directory.CreateDirectory(workshopPath);
        // Prepare files to upload
        List<string> files_to_upload = SystemUtils.SearchFileRecursive(mod_decl.FolderPath,
            (filename) =>
            {
                // To ignore .git and .vscode and so on files
                return !filename.StartsWith(".");
            },
            (dirname) =>
            {
                // To ignore .git and .vscode and so on files
                return !dirname.StartsWith(".") && !Paths.IgnoreSearchDirectories.Contains(dirname);
            });
        foreach (string file_full_path in files_to_upload)
        {
            string path = Path.Combine(workshopPath,
                file_full_path.Replace(mod_decl.FolderPath, "").Replace("\\", "/").Substring(1));

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            File.Copy(file_full_path, path);
        }

        string previewImagePath;
        if (string.IsNullOrEmpty(mod_decl.IconPath))
        {
            using Stream icon_stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("NeoModLoader.resources.logo.png");
            using FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
            icon_stream.Seek(0, SeekOrigin.Begin);
            icon_stream.CopyTo(icon_file);
            previewImagePath = Path.Combine(workshopPath, "preview.png");
        }
        else
        {
            previewImagePath = Path.Combine(workshopPath, mod_decl.IconPath);
        }

        // This works for BepInEx mods
        if (!File.Exists(Path.Combine(workshopPath, "mod.json")))
        {
            File.WriteAllText(Path.Combine(workshopPath, "mod.json"), JsonConvert.SerializeObject(mod_decl));
        }

        return workshopServiceBackend.EditMod(fileID, previewImagePath, workshopPath, changelog);
    }

    /// <summary>
    ///     The platform backend. Call <see cref="EnsureSteamAvailableAsync" /> before using it.
    /// </summary>
    internal static IPlatformSpecificModWorkshopService Backend => workshopServiceBackend;

    /// <summary>
    ///     Whether Steam initialized successfully. Does not wait.
    /// </summary>
    internal static bool IsSteamInitialized => steamWorkshopPromise?.CurState == PromiseState.Resolved;

    /// <summary>
    ///     Wait for Steam to initialize and check that it is connected.
    /// </summary>
    /// <exception cref="WorkshopException">Steam is unavailable or offline</exception>
    internal static async Task EnsureSteamAvailableAsync()
    {
        if (Others.is_editor)
        {
            throw new WorkshopException(WorkshopErrorKind.SteamUnavailable,
                "The Steam Workshop is not available in the Unity editor.");
        }

        if (steamWorkshopPromise == null || workshopServiceBackend == null)
        {
            throw new WorkshopException(WorkshopErrorKind.SteamUnavailable,
                "The Workshop service has not been initialized.");
        }

        var initialized = new TaskCompletionSource<bool>();
        steamWorkshopPromise.Then(() => { initialized.TrySetResult(true); }).Catch(e =>
        {
            initialized.TrySetException(new WorkshopException(WorkshopErrorKind.SteamUnavailable,
                $"Steam is not available: {e.Message}", e));
        });
        await initialized.Task;

        if (!workshopServiceBackend.IsSteamLoggedOn())
        {
            throw new WorkshopException(WorkshopErrorKind.Offline, "Steam is not connected.");
        }
    }

    /// <summary>
    ///     Query one page of mods, see <see cref="IPlatformSpecificModWorkshopService.QueryModsAsync" />.
    /// </summary>
    internal static async Task<WorkshopBrowsePage> QueryModsAsync(WorkshopBrowseQuery pQuery, int pPage)
    {
        await EnsureSteamAvailableAsync();
        try
        {
            return await workshopServiceBackend.QueryModsAsync(pQuery, pPage);
        }
        catch (WorkshopException e) when (e.Kind == WorkshopErrorKind.QueryFailed &&
                                          !workshopServiceBackend.IsSteamLoggedOn())
        {
            throw new WorkshopException(WorkshopErrorKind.Offline, "Steam lost its connection.", e);
        }
    }

    /// <summary>
    ///     Query one item with its full description, null when it does not exist.
    /// </summary>
    internal static async Task<WorkshopModInfo> QueryModDetailsAsync(ulong pFileId)
    {
        await EnsureSteamAvailableAsync();
        return await workshopServiceBackend.QueryModDetailsAsync(pFileId);
    }

    /// <summary>
    ///     Steam persona name of a user, null if it is not available.
    /// </summary>
    internal static async Task<string> RequestPersonaNameAsync(ulong pSteamId)
    {
        await EnsureSteamAvailableAsync();
        return await workshopServiceBackend.RequestPersonaNameAsync(pSteamId);
    }
}