namespace NeoModLoader.services;

/// <summary>
///     Paging state of one Mod Browser query.
/// </summary>
/// <remarks>
///     Server-ordered queries load lazily, one Steam page at a time. Orders Steam cannot produce for all items
///     (<see cref="WorkshopBrowseSort.RecentlyUpdated" />, <see cref="WorkshopBrowseSort.Name" />) and the
///     subscribed scope fetch every page, capped at <see cref="MaxFetchAllPages" />, and sort locally.
/// </remarks>
internal sealed class WorkshopBrowseSession
{
    /// <summary>
    ///     Steam's fixed page size (kNumUGCResultsPerPage).
    /// </summary>
    public const int SteamPageSize = 50;

    public const int MaxFetchAllPages = 40;

    private readonly Func<WorkshopBrowseQuery, int, Task<WorkshopBrowsePage>> fetch_page;
    private readonly HashSet<ulong> known_items = new();
    private bool exhausted;
    private int generation;

    /// <param name="pFetchPage">Fetches one 1-based page; throws on failure.</param>
    public WorkshopBrowseSession(Func<WorkshopBrowseQuery, int, Task<WorkshopBrowsePage>> pFetchPage)
    {
        fetch_page = pFetchPage;
    }

    public WorkshopBrowseQuery Query { get; private set; } = new();
    public List<WorkshopModInfo> Items { get; } = new();

    /// <summary>
    ///     Items matching the query according to Steam, -1 before the first page arrives.
    /// </summary>
    public int TotalCount { get; private set; } = -1;

    public int LoadedPages { get; private set; }
    public bool IsLoading { get; private set; }

    /// <summary>
    ///     Set when a fetch-all query stopped at <see cref="MaxFetchAllPages" />.
    /// </summary>
    public bool ReachedPageCap { get; private set; }

    public Exception LastError { get; private set; }
    public bool HasMorePages => !exhausted && !ReachedPageCap;

    /// <summary>
    ///     Raised after every state change, on the thread that completed the fetch.
    /// </summary>
    public event Action Changed;

    /// <summary>
    ///     Start over with a new query. Results of requests still in flight for the previous query are dropped.
    /// </summary>
    public Task ResetAsync(WorkshopBrowseQuery pQuery)
    {
        generation++;
        Query = pQuery.Clone();
        Items.Clear();
        known_items.Clear();
        TotalCount = -1;
        LoadedPages = 0;
        exhausted = false;
        ReachedPageCap = false;
        LastError = null;
        IsLoading = false;
        Changed?.Invoke();
        return Query.IsServerPaged ? LoadNextPageAsync() : LoadAllPagesAsync();
    }

    /// <summary>
    ///     Load the next page of a server-ordered query. Does nothing while loading or when there is nothing left.
    /// </summary>
    public async Task LoadNextPageAsync()
    {
        if (IsLoading || !HasMorePages || !Query.IsServerPaged) return;

        int request_generation = generation;
        IsLoading = true;
        LastError = null;
        Changed?.Invoke();
        try
        {
            WorkshopBrowsePage page = await fetch_page(Query, LoadedPages + 1);
            if (request_generation != generation) return;
            Append(page);
        }
        catch (Exception e)
        {
            if (request_generation != generation) return;
            LastError = e;
            LogError(e);
        }
        finally
        {
            if (request_generation == generation)
            {
                IsLoading = false;
                Changed?.Invoke();
            }
        }
    }

    private async Task LoadAllPagesAsync()
    {
        int request_generation = generation;
        IsLoading = true;
        Changed?.Invoke();
        try
        {
            while (!exhausted)
            {
                if (LoadedPages >= MaxFetchAllPages)
                {
                    ReachedPageCap = true;
                    break;
                }

                WorkshopBrowsePage page = await fetch_page(Query, LoadedPages + 1);
                if (request_generation != generation) return;
                Append(page);
                SortLocally(Items, Query.EffectiveSort);
                Changed?.Invoke();
            }
        }
        catch (Exception e)
        {
            if (request_generation != generation) return;
            LastError = e;
            LogError(e);
        }
        finally
        {
            if (request_generation == generation)
            {
                IsLoading = false;
                Changed?.Invoke();
            }
        }
    }

    private static void LogError(Exception pException)
    {
        if (pException is WorkshopException)
        {
            LogService.LogWarning($"Workshop query failed: {pException.Message}");
            return;
        }

        LogService.LogError($"Workshop query failed: {pException.Message}");
        LogService.LogException(pException);
    }

    private void Append(WorkshopBrowsePage pPage)
    {
        LoadedPages++;
        TotalCount = pPage.TotalCount;
        foreach (WorkshopModInfo item in pPage.Items)
        {
            // Rankings can shift between page requests, so an item may show up twice.
            if (item.IsModLoader || !known_items.Add(item.FileId)) continue;
            Items.Add(item);
        }

        exhausted = pPage.Items.Count == 0 || LoadedPages * SteamPageSize >= pPage.TotalCount;
    }

    /// <summary>
    ///     Stable local sort. <see cref="WorkshopBrowseSort.Relevance" /> keeps Steam's order and
    ///     <see cref="WorkshopBrowseSort.Trending" /> falls back to subscribers, since trend data is not returned.
    /// </summary>
    public static void SortLocally(List<WorkshopModInfo> pItems, WorkshopBrowseSort pSort)
    {
        IEnumerable<WorkshopModInfo> sorted = pSort switch
        {
            WorkshopBrowseSort.RecentlyUpdated => pItems.OrderByDescending(item => item.Updated),
            WorkshopBrowseSort.Name => pItems.OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase),
            WorkshopBrowseSort.Newest => pItems.OrderByDescending(item => item.Created),
            WorkshopBrowseSort.TopRated => pItems.OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.VotesUp),
            WorkshopBrowseSort.Relevance => pItems,
            _ => pItems.OrderByDescending(item => item.Subscriptions)
        };
        List<WorkshopModInfo> result = sorted.ToList();
        pItems.Clear();
        pItems.AddRange(result);
    }
}
