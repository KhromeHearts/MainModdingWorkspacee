using NeoModLoader.constants;

namespace NeoModLoader.services;

/// <summary>
///     Workshop tags used by NeoModLoader items.
/// </summary>
internal static class WorkshopTags
{
    /// <summary>
    ///     Uploaded by an author who passed NML's upload authentication. Not a code review.
    /// </summary>
    public const string Mod = "Mod";

    /// <summary>
    ///     Uploaded without NML's upload authentication.
    /// </summary>
    public const string UnverifiedMod = "Unverified Mod";

    /// <summary>
    ///     NeoModLoader itself.
    /// </summary>
    public const string ModLoader = "Mod Loader";
}

/// <summary>
///     How Mod Browser results are ordered.
/// </summary>
internal enum WorkshopBrowseSort
{
    MostSubscribed,
    Trending,
    TopRated,
    Newest,
    RecentlyUpdated,
    Name,
    Relevance
}

/// <summary>
///     Which Workshop items the Mod Browser lists.
/// </summary>
internal enum WorkshopBrowseScope
{
    All,
    Subscribed
}

internal enum WorkshopErrorKind
{
    /// <summary>
    ///     Steam is not running, failed to initialize or is not supported in this environment.
    /// </summary>
    SteamUnavailable,

    /// <summary>
    ///     Steam runs but is not connected.
    /// </summary>
    Offline,

    /// <summary>
    ///     Steam returned no result for a query.
    /// </summary>
    QueryFailed
}

internal sealed class WorkshopException : Exception
{
    public WorkshopException(WorkshopErrorKind pKind, string pMessage, Exception pInnerException = null) :
        base(pMessage, pInnerException)
    {
        Kind = pKind;
    }

    public WorkshopErrorKind Kind { get; }
}

/// <summary>
///     Filters and ordering of a Mod Browser query.
/// </summary>
internal sealed class WorkshopBrowseQuery
{
    public WorkshopBrowseScope Scope { get; set; } = WorkshopBrowseScope.All;
    public WorkshopBrowseSort Sort { get; set; } = WorkshopBrowseSort.MostSubscribed;
    public string SearchText { get; set; } = "";
    public bool IncludeVerified { get; set; } = true;
    public bool IncludeUnverified { get; set; } = true;

    /// <summary>
    ///     <see cref="WorkshopBrowseSort.Relevance" /> only makes sense with search text.
    /// </summary>
    public WorkshopBrowseSort EffectiveSort =>
        Sort == WorkshopBrowseSort.Relevance && string.IsNullOrWhiteSpace(SearchText)
            ? WorkshopBrowseSort.MostSubscribed
            : Sort;

    /// <summary>
    ///     Whether Steam can order the results itself. Otherwise every page is fetched and sorted locally:
    ///     Steam has no all-items ranking by update date or title, and user lists are small.
    /// </summary>
    public bool IsServerPaged =>
        Scope == WorkshopBrowseScope.All &&
        EffectiveSort != WorkshopBrowseSort.RecentlyUpdated &&
        EffectiveSort != WorkshopBrowseSort.Name;

    public WorkshopBrowseQuery Clone()
    {
        return (WorkshopBrowseQuery)MemberwiseClone();
    }
}

/// <summary>
///     Snapshot of a Workshop item taken from a query.
/// </summary>
internal sealed class WorkshopModInfo
{
    public ulong FileId { get; set; }
    public string Title { get; set; } = "";

    /// <summary>
    ///     Short (up to 255 characters) unless <see cref="IsLongDescription" />.
    /// </summary>
    public string Description { get; set; } = "";

    public bool IsLongDescription { get; set; }
    public ulong OwnerSteamId { get; set; }

    /// <summary>
    ///     Steam persona name of the uploader, null until resolved.
    /// </summary>
    public string OwnerName { get; set; }

    /// <summary>
    ///     Tags as returned by Facepunch, which lower-cases them.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    public string PreviewUrl { get; set; }
    public uint VotesUp { get; set; }
    public uint VotesDown { get; set; }

    /// <summary>
    ///     Steam's rating score, 0 to 1.
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    ///     Current subscribers.
    /// </summary>
    public ulong Subscriptions { get; set; }

    public ulong Favorites { get; set; }

    /// <summary>
    ///     Published size in bytes, -1 when unknown. Steam documents it as approximate for modern items.
    /// </summary>
    public long FileSizeBytes { get; set; } = -1;

    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public bool IsVerified => HasTag(WorkshopTags.Mod) && !HasTag(WorkshopTags.UnverifiedMod);
    public bool IsModLoader => FileId == CoreConstants.WorkshopFileId || HasTag(WorkshopTags.ModLoader);
    public string Url => $"https://steamcommunity.com/sharedfiles/filedetails/?id={FileId}";
    public string ChangelogUrl => $"https://steamcommunity.com/sharedfiles/filedetails/changelog/{FileId}";

    public bool HasTag(string pTag)
    {
        return Tags.Any(tag => string.Equals(tag, pTag, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
///     Live local state of a Workshop item, read from Steam on every call.
/// </summary>
internal struct WorkshopItemLocalState
{
    public bool Subscribed;
    public bool Installed;
    public bool NeedsUpdate;
    public bool Downloading;
    public bool DownloadPending;

    /// <summary>
    ///     0 to 1, only meaningful while <see cref="Downloading" />.
    /// </summary>
    public float DownloadProgress;

    /// <summary>
    ///     Install folder, null unless installed.
    /// </summary>
    public string Directory;

    public bool IsUpToDate => Installed && !NeedsUpdate && !Downloading && !DownloadPending;
}

/// <summary>
///     One page of query results.
/// </summary>
internal sealed class WorkshopBrowsePage
{
    public int Page { get; set; }

    /// <summary>
    ///     Total items matching the query across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    public bool CachedData { get; set; }
    public List<WorkshopModInfo> Items { get; set; } = new();
}
