extern alias winsteamwork;
using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.ui;
using NeoModLoader.utils;
using RSG;
using UnityEngine;
using winsteamwork::Steamworks;
using winsteamwork::Steamworks.Ugc;
using WorkshopItem = winsteamwork::Steamworks.Ugc.Item;

namespace NeoModLoader.services;

extern alias winsteamwork;

internal class ModWorkshopServiceWindows : IPlatformSpecificModWorkshopService
{
    private static FieldInfo item_details_field;
    private static FieldInfo details_file_size_field;
    private static bool file_size_unavailable;

    public ModWorkshopServiceWindows()
    {
        SteamUGC.OnDownloadItemResult += OnSteamDownloadItemResult;
    }

    public event Action<bool, string> DownloadItemResult;

    public void UploadModLoader(string changelog)
    {
        string workshopPath = SaveManager.generateWorkshopPath(CoreConstants.ModName);

        string previewImagePath = Path.Combine(workshopPath, "preview.png");
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }

        Directory.CreateDirectory(workshopPath);
        // Prepare files to upload
        File.Copy(Paths.NMLModPath, Path.Combine(workshopPath, "NeoModLoader.dll"));
        File.Copy(Paths.NMLModPath.Replace(".dll", ".pdb"), Path.Combine(workshopPath, "NeoModLoader.pdb"));

        using Stream icon_stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("NeoModLoader.resources.logo.png");
        using FileStream icon_file = File.Create(previewImagePath);
        icon_stream.Seek(0, SeekOrigin.Begin);
        icon_stream.CopyTo(icon_file);
        icon_file.Close();

        Editor editor = new Editor(CoreConstants.WorkshopFileId).WithContent(workshopPath).WithTag("Mod Loader")
            .WithPreviewFile(previewImagePath)
            .WithChangeLog(changelog);

        editor.SubmitAsync(null).ContinueWith(delegate(Task<PublishResult> taskResult)
        {
            if (taskResult.Status != TaskStatus.RanToCompletion)
            {
                LogService.LogErrorConcurrent("!RanToCompletion");
                return;
            }

            PublishResult result = taskResult.Result;
            // Result process refer to: https://partner.steamgames.com/doc/api/steam_api#EResult
            if (!result.Success)
            {
                LogService.LogErrorConcurrent("!result.Success");
            }

            if (result.NeedsWorkshopAgreement)
            {
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }

            if (result.Result != Result.OK)
            {
                LogService.LogErrorConcurrent(result.Result.ToString());
            }
        }, TaskScheduler.Default);
    }

    public Promise UploadMod(string name, string description, string previewImagePath, string workshopPath,
        string changelog, bool verified)
    {
        // Create Upload Files Descriptor
        Editor editor = Editor.NewCommunityFile.WithTag(verified ? "Mod" : "Unverified Mod")
            .WithTitle(name).WithDescription(description).WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);

        Promise promise = new();
        ModUploadingProgressWindow.UploadProgress uploadProgress = ModUploadingProgressWindow.ShowWindow();
        editor.SubmitAsync(uploadProgress).ContinueWith(delegate(Task<PublishResult> taskResult)
        {
            if (taskResult.Status != TaskStatus.RanToCompletion)
            {
                promise.Reject(taskResult.Exception.GetBaseException());
                return;
            }

            PublishResult result = taskResult.Result;
            // Result process refer to: https://partner.steamgames.com/doc/api/steam_api#EResult
            if (!result.Success)
            {
                LogService.LogError("!result.Success");
            }

            if (result.NeedsWorkshopAgreement)
            {
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }

            if (result.Result != Result.OK)
            {
                promise.Reject(new Exception("Something went wrong: " + result.Result.ToString()));
                return;
            }

            ModUploadingProgressWindow.Instance.fileId = result.FileId;
            promise.Resolve();
        }, TaskScheduler.Default);

        return promise;
    }

    public Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog)
    {
        Promise promise = new();
        // Create Upload Files Descriptor
        Editor editor = new Editor(fileID)
            .WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);

        editor.SubmitAsync(ModUploadingProgressWindow.ShowWindow()).ContinueWith(
            delegate(Task<PublishResult> taskResult)
            {
                if (taskResult.Status != TaskStatus.RanToCompletion)
                {
                    promise.Reject(taskResult.Exception.GetBaseException());
                    return;
                }

                PublishResult result = taskResult.Result;
                // Result process refer to: https://partner.steamgames.com/doc/api/steam_api#EResult
                if (result.NeedsWorkshopAgreement)
                {
                    LogService.LogWarning("Needs Workshop Agreement");
                    // TODO: Open Workshop Agreement
                    Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
                }

                if (result.Result != Result.OK)
                {
                    promise.Reject(new Exception(result.Result.ToString()));
                    return;
                }

                // result.FileId;
                promise.Resolve();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        return promise;
    }

    public bool IsSteamLoggedOn()
    {
        try
        {
            return SteamClient.IsValid && SteamClient.IsLoggedOn;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot read Steam logon state: {e.Message}");
            return false;
        }
    }

    public async Task<WorkshopBrowsePage> QueryModsAsync(WorkshopBrowseQuery pQuery, int pPage)
    {
        var browse_page = new WorkshopBrowsePage { Page = pPage };
        if (!pQuery.IncludeVerified && !pQuery.IncludeUnverified)
        {
            return browse_page;
        }

        ResultPage? result_page = await BuildQuery(pQuery).GetPageAsync(pPage);
        if (!result_page.HasValue)
        {
            throw new WorkshopException(WorkshopErrorKind.QueryFailed,
                $"Steam returned no result for Workshop page {pPage}.");
        }

        // The page owns a Steam query handle, which is released on dispose.
        using ResultPage page = result_page.Value;
        browse_page.TotalCount = page.TotalCount;
        browse_page.CachedData = page.CachedData;
        foreach (WorkshopItem entry in page.Entries)
        {
            browse_page.Items.Add(ToModInfo(entry, false));
        }

        return browse_page;
    }

    public async Task<WorkshopModInfo> QueryModDetailsAsync(ulong pFileId)
    {
        ResultPage? result_page = await Query.ItemsReadyToUse.WithFileId(pFileId).WithLongDescription(true)
            .GetPageAsync(1);
        if (!result_page.HasValue)
        {
            throw new WorkshopException(WorkshopErrorKind.QueryFailed,
                $"Steam returned no details for Workshop item {pFileId}.");
        }

        using ResultPage page = result_page.Value;
        foreach (WorkshopItem entry in page.Entries)
        {
            if (entry.Result != Result.OK) return null;
            return ToModInfo(entry, true);
        }

        return null;
    }

    public WorkshopItemLocalState GetItemLocalState(ulong pFileId)
    {
        var item = new WorkshopItem(pFileId);
        bool installed = item.IsInstalled;
        bool downloading = item.IsDownloading;
        return new WorkshopItemLocalState
        {
            Subscribed = item.IsSubscribed,
            Installed = installed,
            NeedsUpdate = item.NeedsUpdate,
            Downloading = downloading,
            DownloadPending = item.IsDownloadPending,
            // Item.DownloadBytesDownloaded/Total report the installed size unless NeedsUpdate is set, so they read 0
            // during a first download; DownloadAmount asks Steam directly while downloading.
            DownloadProgress = downloading ? item.DownloadAmount : installed ? 1f : 0f,
            Directory = installed ? item.Directory : null
        };
    }

    public Task<bool> SubscribeModAsync(ulong pFileId)
    {
        return new WorkshopItem(pFileId).Subscribe();
    }

    public Task<bool> UnsubscribeModAsync(ulong pFileId)
    {
        return new WorkshopItem(pFileId).Unsubscribe();
    }

    public bool StartModDownload(ulong pFileId, bool pHighPriority)
    {
        return SteamUGC.Download(pFileId, pHighPriority);
    }

    public async Task<string> RequestPersonaNameAsync(ulong pSteamId)
    {
        var friend = new Friend(pSteamId);
        Task request = friend.RequestInfoAsync();
        if (await Task.WhenAny(request, Task.Delay(5000)) == request)
        {
            await request;
        }

        string name = friend.Name;
        return string.IsNullOrEmpty(name) || name == "[unknown]" ? null : name;
    }

    private void OnSteamDownloadItemResult(Result pResult)
    {
        DownloadItemResult?.Invoke(pResult == Result.OK, pResult.ToString());
    }

    private static Query BuildQuery(WorkshopBrowseQuery pQuery)
    {
        // Query is a struct whose builder methods mutate and copy it, so always start from a fresh one.
        Query query = Query.ItemsReadyToUse;
        if (pQuery.Scope == WorkshopBrowseScope.Subscribed)
        {
            query = query.WhereUserSubscribed().SortByCreationDateAsc();
        }

        if (pQuery.IncludeVerified && pQuery.IncludeUnverified)
        {
            query = query.WithTag(WorkshopTags.Mod).WithTag(WorkshopTags.UnverifiedMod).MatchAnyTag();
        }
        else if (pQuery.IncludeVerified)
        {
            query = query.WithTag(WorkshopTags.Mod).WithoutTag(WorkshopTags.UnverifiedMod);
        }
        else
        {
            query = query.WithTag(WorkshopTags.UnverifiedMod);
        }

        query = query.WithoutTag(WorkshopTags.ModLoader);
        if (!string.IsNullOrWhiteSpace(pQuery.SearchText))
        {
            query = query.WhereSearchText(pQuery.SearchText.Trim());
        }

        if (pQuery.Scope == WorkshopBrowseScope.Subscribed)
        {
            return query;
        }

        switch (pQuery.EffectiveSort)
        {
            case WorkshopBrowseSort.Trending:
                return query.RankedByTrend().WithTrendDays(30);
            case WorkshopBrowseSort.TopRated:
                return query.RankedByVote();
            case WorkshopBrowseSort.Relevance:
                return query.RankedByTextSearch();
            case WorkshopBrowseSort.Newest:
            // Steam has no all-items ranking by update date or title: every page is fetched and sorted locally,
            // so only a stable order matters here.
            case WorkshopBrowseSort.RecentlyUpdated:
            case WorkshopBrowseSort.Name:
                return query.RankedByPublicationDate();
            default:
                return query.RankedByTotalUniqueSubscriptions();
        }
    }

    private static WorkshopModInfo ToModInfo(WorkshopItem pItem, bool pLongDescription)
    {
        return new WorkshopModInfo
        {
            FileId = pItem.Id.Value,
            Title = pItem.Title ?? "",
            Description = pItem.Description ?? "",
            IsLongDescription = pLongDescription,
            OwnerSteamId = pItem.Owner.Id.Value,
            Tags = pItem.Tags ?? Array.Empty<string>(),
            PreviewUrl = pItem.PreviewImageUrl,
            VotesUp = pItem.VotesUp,
            VotesDown = pItem.VotesDown,
            Score = pItem.Score,
            Subscriptions = pItem.NumSubscriptions,
            Favorites = pItem.NumFavorites,
            FileSizeBytes = GetPublishedFileSize(pItem),
            Created = pItem.Created,
            Updated = pItem.Updated
        };
    }

    /// <summary>
    ///     Item.SizeBytes is the size on disk (0 when not installed). The published size only exists in the
    ///     internal SteamUGCDetails_t.FileSize, so read it through reflection and report -1 if that ever fails.
    /// </summary>
    private static long GetPublishedFileSize(WorkshopItem pItem)
    {
        if (file_size_unavailable) return -1;
        try
        {
            item_details_field ??= typeof(WorkshopItem).GetField("details", BindingFlags.Instance | BindingFlags.NonPublic);
            details_file_size_field ??= item_details_field?.FieldType.GetField("FileSize",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (item_details_field == null || details_file_size_field == null)
            {
                throw new MissingFieldException("Item.details.FileSize");
            }

            int size = (int)details_file_size_field.GetValue(item_details_field.GetValue(pItem));
            // A signed 32-bit field in Steam's struct: reinterpret to cover files between 2 and 4 GiB.
            long bytes = (uint)size;
            return bytes > 0 ? bytes : -1;
        }
        catch (Exception e)
        {
            file_size_unavailable = true;
            LogService.LogWarning($"Cannot read Workshop file sizes, they will show as unknown: {e.Message}");
            return -1;
        }
    }
}
