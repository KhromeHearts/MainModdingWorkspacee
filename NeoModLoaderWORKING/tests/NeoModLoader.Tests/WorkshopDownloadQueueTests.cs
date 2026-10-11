using NeoModLoader.services;

namespace NeoModLoader.Tests;

internal static class WorkshopDownloadQueueTests
{
    private static (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) Create()
    {
        var backend = new FakeWorkshopBackend();
        var handler = new FakeInstallHandler();
        var time = new FakeTime();
        var queue = new WorkshopDownloadQueue(backend, handler, time.Delay, () => time.Now)
        {
            PollIntervalMs = 200,
            StallTimeout = TimeSpan.FromSeconds(2)
        };
        return (queue, backend, handler);
    }

    [Test]
    public static async Task InstallsAnItem()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1);
        var states = new List<WorkshopQueueState>();
        queue.EntryChanged += entry =>
        {
            lock (states)
            {
                if (states.Count == 0 || states[states.Count - 1] != entry.State) states.Add(entry.State);
            }
        };
        WorkshopQueueSummary summary = null;
        queue.Finished += finished => summary = finished;

        WorkshopQueueEntry entry = queue.Enqueue(1, "One", WorkshopQueueAction.Install, 1000, false);
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Completed, entry.State);
        Assert.True(entry.SubscribedByQueue);
        Assert.Equal("C:/fake/workshop/1", entry.InstallDirectory);
        Assert.SequenceEqual(new ulong[] { 1 }, handler.Installed);
        Assert.SequenceEqual(new[] { "subscribe 1", "download 1" }, backend.Calls);
        Assert.SequenceEqual(new[]
        {
            WorkshopQueueState.Queued, WorkshopQueueState.Subscribing, WorkshopQueueState.WaitingForSteam,
            WorkshopQueueState.Downloading, WorkshopQueueState.Installing, WorkshopQueueState.Completed
        }, states);
        Assert.NotNull(summary);
        Assert.Equal(1, summary.Completed);
        Assert.False(queue.IsRunning);
    }

    [Test]
    public static async Task OneFailureNeverStopsTheQueue()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1, item => item.RefuseSubscribe = true);
        backend.Add(2, item => item.RefuseDownload = true);
        backend.Add(3, item => item.Stall = true);
        backend.Add(4, item => item.FailWith = "Timeout");
        backend.Add(5, item => item.ThrowOnStateAfterStart = true);
        backend.Add(6);
        backend.Add(7);
        handler.Result = entry => entry.FileId == 6
            ? WorkshopInstallResult.Failure("broken mod.json", false)
            : WorkshopInstallResult.Success();
        WorkshopQueueSummary summary = null;
        queue.Finished += finished => summary = finished;

        // One batch: items 1 and 2 fail without ever awaiting, which must not end the run early.
        queue.Enqueue(Enumerable.Range(1, 7).Select(id =>
            new WorkshopQueueEntry((ulong)id, $"Mod {id}", WorkshopQueueAction.Install, -1, false, null)));
        await queue.RunTask;

        Dictionary<ulong, WorkshopQueueEntry> entries = queue.Entries.ToDictionary(entry => entry.FileId);
        Assert.Contains("refused the subscription", entries[1].Error);
        Assert.Contains("could not start the download", entries[2].Error);
        Assert.Contains("no download progress", entries[3].Error);
        Assert.Contains("Steam reported Timeout", entries[4].Error);
        Assert.Contains("simulated Steam failure", entries[5].Error);
        Assert.Equal("broken mod.json", entries[6].Error);
        for (ulong id = 1; id <= 6; id++) Assert.Equal(WorkshopQueueState.Failed, entries[id].State, $"item {id}");
        Assert.Equal(WorkshopQueueState.Completed, entries[7].State);

        // Subscriptions the queue made are rolled back unless the item ended up installed.
        Assert.SequenceEqual(new[] { "subscribe 1" }, backend.CallsFor(1));
        Assert.SequenceEqual(new[] { "subscribe 2", "download 2", "unsubscribe 2" }, backend.CallsFor(2));
        Assert.SequenceEqual(new[] { "subscribe 3", "download 3", "unsubscribe 3" }, backend.CallsFor(3));
        Assert.SequenceEqual(new[] { "subscribe 4", "download 4", "unsubscribe 4" }, backend.CallsFor(4));
        Assert.SequenceEqual(new[] { "subscribe 5", "download 5", "unsubscribe 5" }, backend.CallsFor(5));
        Assert.SequenceEqual(new[] { "subscribe 6", "download 6" }, backend.CallsFor(6));

        Assert.Equal(6, summary.Failed);
        Assert.Equal(1, summary.Completed);
        Assert.Equal(7, summary.Entries.Count);
    }

    [Test]
    public static async Task CancelAllStopsTheCurrentItemAndDropsTheRest()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1, item => item.DownloadPolls = 1000);
        backend.Add(2);
        backend.Add(3);
        WorkshopQueueSummary summary = null;
        queue.Finished += finished => summary = finished;

        WorkshopQueueEntry first = queue.Enqueue(1, "Slow", WorkshopQueueAction.Install, -1, false);
        queue.Enqueue(2, "Two", WorkshopQueueAction.Install, -1, false);
        queue.Enqueue(3, "Three", WorkshopQueueAction.Install, -1, false);
        await Assert.Eventually(() => first.State == WorkshopQueueState.Downloading, "first item downloading");

        queue.CancelAll();
        await queue.RunTask;

        Assert.True(queue.Entries.All(entry => entry.State == WorkshopQueueState.Cancelled), "all cancelled");
        Assert.SequenceEqual(new[] { "subscribe 1", "download 1", "unsubscribe 1" }, backend.CallsFor(1));
        Assert.SequenceEqual(Array.Empty<string>(), backend.CallsFor(2), "item 2 never starts");
        Assert.Equal(0, handler.Installed.Count);
        Assert.Equal(3, summary.Cancelled);
    }

    [Test]
    public static async Task CancellingAQueuedItemLeavesTheOthersRunning()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1, item => item.DownloadPolls = 20);
        backend.Add(2);
        backend.Add(3);

        WorkshopQueueEntry first = queue.Enqueue(1, "One", WorkshopQueueAction.Install, -1, false);
        WorkshopQueueEntry second = queue.Enqueue(2, "Two", WorkshopQueueAction.Install, -1, false);
        WorkshopQueueEntry third = queue.Enqueue(3, "Three", WorkshopQueueAction.Install, -1, false);
        await Assert.Eventually(() => first.State == WorkshopQueueState.Downloading, "first item downloading");
        queue.Cancel(2);
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Completed, first.State);
        Assert.Equal(WorkshopQueueState.Cancelled, second.State);
        Assert.Equal(WorkshopQueueState.Completed, third.State);
        Assert.SequenceEqual(new ulong[] { 1, 3 }, handler.Installed);
    }

    [Test]
    public static async Task CancellingAnUpdateKeepsTheExistingSubscription()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, _) = Create();
        backend.Add(1, item =>
        {
            item.Subscribed = true;
            item.Installed = true;
            item.NeedsUpdate = true;
            item.DownloadPolls = 1000;
        });

        WorkshopQueueEntry entry = queue.Enqueue(1, "One", WorkshopQueueAction.Update, -1, false);
        await Assert.Eventually(() => entry.State == WorkshopQueueState.Downloading, "update downloading");
        queue.Cancel(1);
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Cancelled, entry.State);
        Assert.False(entry.SubscribedByQueue);
        Assert.SequenceEqual(new[] { "download 1" }, backend.Calls);
        Assert.True(backend.Items[1].Subscribed);
    }

    [Test]
    public static async Task UpdatesAnInstalledItem()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1, item =>
        {
            item.Subscribed = true;
            item.Installed = true;
            item.NeedsUpdate = true;
        });

        WorkshopQueueEntry entry = queue.Enqueue(1, "One", WorkshopQueueAction.Update, -1, false);
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Completed, entry.State);
        Assert.False(backend.Items[1].NeedsUpdate);
        Assert.SequenceEqual(new[] { "download 1" }, backend.Calls);
        Assert.SequenceEqual(new ulong[] { 1 }, handler.Installed);
    }

    [Test]
    public static async Task UpToDateItemSkipsTheDownload()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1, item =>
        {
            item.Subscribed = true;
            item.Installed = true;
            item.RefuseDownload = true;
        });

        WorkshopQueueEntry entry = queue.Enqueue(1, "One", WorkshopQueueAction.Install, -1, false);
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Completed, entry.State);
        Assert.SequenceEqual(new ulong[] { 1 }, handler.Installed);
    }

    [Test]
    public static async Task WrongDependencyIsRolledBackEvenThoughInstalled()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, FakeInstallHandler handler) = Create();
        backend.Add(1);
        handler.Result = entry => WorkshopInstallResult.Failure($"provides OTHER, not {entry.ExpectedUid}", true);

        WorkshopQueueEntry entry =
            queue.Enqueue(1, "Candidate", WorkshopQueueAction.Install, -1, false, "AUTHOR_WANTED");
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Failed, entry.State);
        Assert.Equal("provides OTHER, not AUTHOR_WANTED", entry.Error);
        Assert.SequenceEqual(new[] { "subscribe 1", "download 1", "unsubscribe 1" }, backend.Calls);
    }

    [Test]
    public static async Task EnqueueingTwiceReturnsTheSameEntry()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, _) = Create();
        backend.Add(1, item => item.DownloadPolls = 10);

        WorkshopQueueEntry first = queue.Enqueue(1, "One", WorkshopQueueAction.Install, -1, false);
        WorkshopQueueEntry again = queue.Enqueue(1, "One", WorkshopQueueAction.Install, -1, true);
        await queue.RunTask;

        Assert.True(ReferenceEquals(first, again), "same entry");
        Assert.Equal(1, queue.Entries.Count);
        Assert.Equal(1, backend.CallsFor(1).Count(call => call.StartsWith("download")));
    }

    [Test]
    public static async Task ABrokenQueueFailsItsEntriesInsteadOfHanging()
    {
        var queue = new WorkshopDownloadQueue(null, new FakeInstallHandler());
        WorkshopQueueSummary summary = null;
        queue.Finished += finished => summary = finished;

        List<WorkshopQueueEntry> entries = queue.Enqueue(new[]
        {
            new WorkshopQueueEntry(1, "One", WorkshopQueueAction.Install, -1, false, null),
            new WorkshopQueueEntry(2, "Two", WorkshopQueueAction.Install, -1, false, null)
        });
        await queue.RunTask;

        Assert.True(entries.All(entry => entry.State == WorkshopQueueState.Failed), "all failed");
        Assert.False(queue.IsRunning);
        Assert.Equal(2, summary.Failed);
    }

    [Test]
    public static async Task RetryReplacesTheFailedEntry()
    {
        (WorkshopDownloadQueue queue, FakeWorkshopBackend backend, _) = Create();
        FakeItem item = backend.Add(1, fake => fake.RefuseSubscribe = true);

        WorkshopQueueEntry failed = queue.Enqueue(1, "One", WorkshopQueueAction.Install, -1, false);
        await queue.RunTask;
        Assert.Equal(WorkshopQueueState.Failed, failed.State);

        item.RefuseSubscribe = false;
        WorkshopQueueEntry retry = queue.Enqueue(1, "One", WorkshopQueueAction.Install, -1, false);
        await queue.RunTask;

        Assert.Equal(WorkshopQueueState.Completed, retry.State);
        Assert.Equal(1, queue.Entries.Count);
        Assert.True(ReferenceEquals(retry, queue.Find(1)), "latest entry is found");
    }
}
