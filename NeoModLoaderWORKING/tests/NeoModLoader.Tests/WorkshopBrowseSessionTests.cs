using NeoModLoader.services;

namespace NeoModLoader.Tests;

internal static class WorkshopBrowseSessionTests
{
    private static WorkshopModInfo Mod(ulong pId, string pTitle = null, int pUpdatedDay = 1, ulong pSubscribers = 0,
        params string[] pTags)
    {
        return new WorkshopModInfo
        {
            FileId = pId,
            Title = pTitle ?? $"Mod {pId}",
            Updated = new DateTime(2026, 1, 1).AddDays(pUpdatedDay),
            Created = new DateTime(2025, 1, 1).AddDays(pId % 1000),
            Subscriptions = pSubscribers,
            Tags = pTags.Length == 0 ? new[] { "mod" } : pTags
        };
    }

    private static WorkshopBrowsePage Page(int pPage, int pTotal, IEnumerable<WorkshopModInfo> pItems)
    {
        return new WorkshopBrowsePage { Page = pPage, TotalCount = pTotal, Items = pItems.ToList() };
    }

    [Test]
    public static async Task ServerOrderedQueriesLoadOnePageAtATimeAndDropDuplicates()
    {
        var requested = new List<int>();
        var session = new WorkshopBrowseSession((query, page) =>
        {
            requested.Add(page);
            // Page 2 repeats six items of page 1, as happens when rankings shift between requests.
            IEnumerable<ulong> ids = page == 1 ? Range(1, 50) : Range(45, 50);
            return Task.FromResult(Page(page, 94, ids.Select(id => Mod(id))));
        });

        await session.ResetAsync(new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.MostSubscribed });
        Assert.Equal(50, session.Items.Count);
        Assert.Equal(94, session.TotalCount);
        Assert.True(session.HasMorePages);

        await session.LoadNextPageAsync();
        Assert.Equal(94, session.Items.Count);
        Assert.Equal(session.Items.Count, session.Items.Select(item => item.FileId).Distinct().Count());
        Assert.False(session.HasMorePages);

        await session.LoadNextPageAsync();
        Assert.SequenceEqual(new[] { 1, 2 }, requested);
    }

    [Test]
    public static async Task NameAndRecentlyUpdatedFetchEveryPageAndSortLocally()
    {
        WorkshopModInfo[] all =
        {
            Mod(1, "beta", 5), Mod(2, "Alpha", 1), Mod(3, "gamma", 9), Mod(4, "Delta", 3)
        };
        var session = new WorkshopBrowseSession((query, page) =>
            Task.FromResult(Page(page, 4, all.Skip((page - 1) * 2).Take(2))));
        // A page size of 2 here, but the session assumes Steam's 50 per page, so it relies on empty pages too.
        var requested = 0;
        var counting = new WorkshopBrowseSession((query, page) =>
        {
            requested++;
            return Task.FromResult(page == 1 ? Page(1, 4, all) : Page(page, 4, Array.Empty<WorkshopModInfo>()));
        });

        await counting.ResetAsync(new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.Name });
        Assert.Equal(1, requested, "4 results fit on one Steam page");
        Assert.SequenceEqual(new[] { "Alpha", "beta", "Delta", "gamma" }, counting.Items.Select(item => item.Title));
        Assert.False(counting.HasMorePages);

        await counting.ResetAsync(new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.RecentlyUpdated });
        Assert.SequenceEqual(new ulong[] { 3, 1, 4, 2 }, counting.Items.Select(item => item.FileId));

        await session.ResetAsync(new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.Name });
        Assert.SequenceEqual(new[] { "Alpha", "beta" }, session.Items.Select(item => item.Title),
            "stops once page * 50 covers the total");
    }

    [Test]
    public static async Task FetchAllStopsAtThePageCap()
    {
        var requested = 0;
        var session = new WorkshopBrowseSession((query, page) =>
        {
            requested++;
            return Task.FromResult(Page(page, 100_000,
                Range((ulong)(page - 1) * 50 + 1, 50).Select(id => Mod(id))));
        });

        await session.ResetAsync(new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.RecentlyUpdated });

        Assert.Equal(WorkshopBrowseSession.MaxFetchAllPages, requested);
        Assert.True(session.ReachedPageCap);
        Assert.False(session.HasMorePages);
        Assert.Equal(WorkshopBrowseSession.MaxFetchAllPages * 50, session.Items.Count);
    }

    [Test]
    public static async Task SubscribedScopeFetchesEverythingWhateverTheSort()
    {
        var session = new WorkshopBrowseSession((query, page) => Task.FromResult(page == 1
            ? Page(1, 3, new[] { Mod(1, pSubscribers: 5), Mod(2, pSubscribers: 50), Mod(3, pSubscribers: 20) })
            : Page(page, 3, Array.Empty<WorkshopModInfo>())));

        await session.ResetAsync(new WorkshopBrowseQuery
        {
            Scope = WorkshopBrowseScope.Subscribed, Sort = WorkshopBrowseSort.MostSubscribed
        });

        Assert.SequenceEqual(new ulong[] { 2, 3, 1 }, session.Items.Select(item => item.FileId));
    }

    [Test]
    public static async Task ErrorsAreKeptAndLoadingStops()
    {
        var session = new WorkshopBrowseSession((query, page) =>
            Task.FromException<WorkshopBrowsePage>(new WorkshopException(WorkshopErrorKind.Offline, "offline")));

        await session.ResetAsync(new WorkshopBrowseQuery());

        Assert.False(session.IsLoading);
        Assert.True(session.LastError is WorkshopException { Kind: WorkshopErrorKind.Offline }, "offline error kept");
        Assert.Equal(0, session.Items.Count);
    }

    [Test]
    public static async Task ResultsOfAReplacedQueryAreDropped()
    {
        var slow_first_page = new TaskCompletionSource<WorkshopBrowsePage>();
        var session = new WorkshopBrowseSession((query, page) => query.SearchText == "old"
            ? slow_first_page.Task
            : Task.FromResult(Page(1, 1, new[] { Mod(2, "new result") })));

        Task stale = session.ResetAsync(new WorkshopBrowseQuery { SearchText = "old" });
        await session.ResetAsync(new WorkshopBrowseQuery { SearchText = "new" });
        slow_first_page.SetResult(Page(1, 1, new[] { Mod(1, "old result") }));
        await stale;

        Assert.SequenceEqual(new[] { "new result" }, session.Items.Select(item => item.Title));
        Assert.False(session.IsLoading);
    }

    [Test]
    public static async Task ModLoaderItemsAreHidden()
    {
        var session = new WorkshopBrowseSession((query, page) => Task.FromResult(Page(1, 3, new[]
        {
            Mod(1), Mod(2, pTags: new[] { "mod loader" }), Mod(3080294469UL)
        })));

        await session.ResetAsync(new WorkshopBrowseQuery());

        Assert.SequenceEqual(new ulong[] { 1 }, session.Items.Select(item => item.FileId));
    }

    [Test]
    public static void RelevanceNeedsSearchText()
    {
        var query = new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.Relevance };
        Assert.Equal(WorkshopBrowseSort.MostSubscribed, query.EffectiveSort);
        Assert.True(query.IsServerPaged);

        query.SearchText = "traits";
        Assert.Equal(WorkshopBrowseSort.Relevance, query.EffectiveSort);

        Assert.False(new WorkshopBrowseQuery { Sort = WorkshopBrowseSort.Name }.IsServerPaged);
        Assert.False(new WorkshopBrowseQuery { Scope = WorkshopBrowseScope.Subscribed }.IsServerPaged);
    }

    [Test]
    public static void VerificationComesFromTags()
    {
        Assert.True(Mod(1, pTags: new[] { "mod" }).IsVerified);
        Assert.False(Mod(1, pTags: new[] { "unverified mod" }).IsVerified);
        Assert.False(Mod(1, pTags: new[] { "mod", "unverified mod" }).IsVerified, "both tags count as unverified");
        Assert.False(Mod(1, pTags: new[] { "world" }).IsVerified);
    }

    private static IEnumerable<ulong> Range(ulong pStart, int pCount)
    {
        for (ulong i = 0; i < (ulong)pCount; i++) yield return pStart + i;
    }
}
