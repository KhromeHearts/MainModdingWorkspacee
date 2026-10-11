using RSG;

namespace NeoModLoader.services;

internal interface IPlatformSpecificModWorkshopService
{
    void UploadModLoader(string changelog);

    Promise UploadMod(string name, string description, string previewImagePath, string workshopPath, string changelog,
        bool verified);

    Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog);

    /// <summary>
    ///     Raised for every Steam DownloadItemResult callback: (succeeded, Steam result name). Steam does not say
    ///     which item it belongs to through Facepunch.
    /// </summary>
    event Action<bool, string> DownloadItemResult;

    bool IsSteamLoggedOn();

    /// <summary>
    ///     Query one page (1-based, up to 50 items) of mods. Throws <see cref="WorkshopException" /> when Steam
    ///     returns nothing.
    /// </summary>
    Task<WorkshopBrowsePage> QueryModsAsync(WorkshopBrowseQuery pQuery, int pPage);

    /// <summary>
    ///     Query a single item with its full description. Returns null when the item does not exist.
    /// </summary>
    Task<WorkshopModInfo> QueryModDetailsAsync(ulong pFileId);

    WorkshopItemLocalState GetItemLocalState(ulong pFileId);
    Task<bool> SubscribeModAsync(ulong pFileId);
    Task<bool> UnsubscribeModAsync(ulong pFileId);

    /// <summary>
    ///     Ask Steam to download or update an item. False when the download could not be started.
    /// </summary>
    bool StartModDownload(ulong pFileId, bool pHighPriority);

    /// <summary>
    ///     Steam persona name of a user, null if Steam does not provide it in time.
    /// </summary>
    Task<string> RequestPersonaNameAsync(ulong pSteamId);
}
