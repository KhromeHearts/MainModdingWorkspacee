using NeoModLoader.utils;

namespace NeoModLoader.services;

internal enum WorkshopQueueAction
{
    Install,
    Update
}

internal enum WorkshopQueueState
{
    Queued,
    Subscribing,
    WaitingForSteam,
    Downloading,
    Installing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
///     One requested install or update.
/// </summary>
internal sealed class WorkshopQueueEntry
{
    internal WorkshopQueueEntry(ulong pFileId, string pTitle, WorkshopQueueAction pAction, long pSizeBytes,
        bool pEnableAfterInstall, string pExpectedUid)
    {
        FileId = pFileId;
        Title = pTitle;
        Action = pAction;
        SizeBytes = pSizeBytes;
        EnableAfterInstall = pEnableAfterInstall;
        ExpectedUid = pExpectedUid;
    }

    public ulong FileId { get; }
    public string Title { get; }
    public WorkshopQueueAction Action { get; }

    /// <summary>
    ///     Published size for display, -1 when unknown.
    /// </summary>
    public long SizeBytes { get; }

    /// <summary>
    ///     Whether a new install should be marked as enabled instead of disabled.
    /// </summary>
    public bool EnableAfterInstall { get; }

    /// <summary>
    ///     For dependency installs: the mod UID the item must provide.
    /// </summary>
    public string ExpectedUid { get; }

    public WorkshopQueueState State { get; internal set; } = WorkshopQueueState.Queued;
    public float Progress { get; internal set; }
    public string Error { get; internal set; }

    /// <summary>
    ///     Extra information from the install step, e.g. that a restart is required.
    /// </summary>
    public string Note { get; internal set; }

    /// <summary>
    ///     The queue subscribed the item, so cancelling or failing before it is installed unsubscribes it again.
    /// </summary>
    public bool SubscribedByQueue { get; internal set; }

    public string InstallDirectory { get; internal set; }

    public bool IsFinished => State is WorkshopQueueState.Completed or WorkshopQueueState.Failed
        or WorkshopQueueState.Cancelled;

    internal CancellationTokenSource Cancellation { get; } = new();
}

/// <summary>
///     Result of <see cref="IWorkshopInstallHandler.OnItemInstalled" />.
/// </summary>
internal readonly struct WorkshopInstallResult
{
    private WorkshopInstallResult(string pError, bool pRollbackSubscription, string pNote)
    {
        Error = pError;
        RollbackSubscription = pRollbackSubscription;
        Note = pNote;
    }

    public string Error { get; }
    public bool RollbackSubscription { get; }
    public string Note { get; }

    public static WorkshopInstallResult Success(string pNote = null)
    {
        return new WorkshopInstallResult(null, false, pNote);
    }

    /// <param name="pError">Shown to the user</param>
    /// <param name="pRollbackSubscription">Unsubscribe again if the queue subscribed the item</param>
    public static WorkshopInstallResult Failure(string pError, bool pRollbackSubscription)
    {
        return new WorkshopInstallResult(pError, pRollbackSubscription, null);
    }
}

internal interface IWorkshopInstallHandler
{
    /// <summary>
    ///     Called on the main thread once Steam has installed the item.
    /// </summary>
    WorkshopInstallResult OnItemInstalled(WorkshopQueueEntry pEntry, string pDirectory);
}

internal sealed class WorkshopQueueSummary
{
    public List<WorkshopQueueEntry> Entries { get; } = new();
    public int Completed => Entries.Count(entry => entry.State == WorkshopQueueState.Completed);
    public int Failed => Entries.Count(entry => entry.State == WorkshopQueueState.Failed);
    public int Cancelled => Entries.Count(entry => entry.State == WorkshopQueueState.Cancelled);
}

/// <summary>
///     Subscribes to, downloads and installs Workshop items one at a time.
/// </summary>
/// <remarks>
///     Items run sequentially so progress is accurate and Steam's item-less DownloadItemResult callback can be
///     attributed to the item in flight. A failing item never stops the queue. All events are raised through
///     <see cref="MainThreadDispatcher" />.
/// </remarks>
internal sealed class WorkshopDownloadQueue
{
    private readonly IPlatformSpecificModWorkshopService backend;
    private readonly Func<DateTime> clock;
    private readonly Func<int, CancellationToken, Task> delay;
    private readonly List<WorkshopQueueEntry> entries = new();
    private readonly object entries_lock = new();
    private readonly IWorkshopInstallHandler install_handler;
    private string failed_download_result;
    private WorkshopQueueSummary running_summary;

    public WorkshopDownloadQueue(IPlatformSpecificModWorkshopService pBackend, IWorkshopInstallHandler pInstallHandler,
        Func<int, CancellationToken, Task> pDelay = null, Func<DateTime> pClock = null)
    {
        backend = pBackend;
        install_handler = pInstallHandler;
        delay = pDelay ?? Task.Delay;
        clock = pClock ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    ///     Fail an item when nothing about its download changes for this long.
    /// </summary>
    public TimeSpan StallTimeout { get; set; } = TimeSpan.FromSeconds(90);

    public int PollIntervalMs { get; set; } = 200;
    public IReadOnlyList<WorkshopQueueEntry> Entries
    {
        get
        {
            lock (entries_lock)
            {
                return entries.ToList();
            }
        }
    }
    public bool IsRunning { get; private set; }
    public WorkshopQueueEntry Current { get; private set; }

    /// <summary>
    ///     Task of the current run, completed when the queue drains. For tests.
    /// </summary>
    internal Task RunTask { get; private set; } = Task.CompletedTask;

    public event Action<WorkshopQueueEntry> EntryChanged;
    public event Action<WorkshopQueueSummary> Finished;

    /// <summary>
    ///     Latest entry for an item, finished or not.
    /// </summary>
    public WorkshopQueueEntry Find(ulong pFileId)
    {
        lock (entries_lock)
        {
            return entries.LastOrDefault(entry => entry.FileId == pFileId);
        }
    }

    /// <summary>
    ///     Queue an item and start processing. Returns the existing entry if the item is already queued or active.
    /// </summary>
    public WorkshopQueueEntry Enqueue(ulong pFileId, string pTitle, WorkshopQueueAction pAction, long pSizeBytes,
        bool pEnableAfterInstall, string pExpectedUid = null)
    {
        return Enqueue(new[]
        {
            new WorkshopQueueEntry(pFileId, pTitle, pAction, pSizeBytes, pEnableAfterInstall, pExpectedUid)
        })[0];
    }

    /// <summary>
    ///     Queue several items at once, so they run (and are summarized) as one batch even if early items finish
    ///     immediately. Items already queued or active keep their existing entry, which is returned instead.
    /// </summary>
    public List<WorkshopQueueEntry> Enqueue(IEnumerable<WorkshopQueueEntry> pRequests)
    {
        var result = new List<WorkshopQueueEntry>();
        var added = new List<WorkshopQueueEntry>();
        bool start;
        lock (entries_lock)
        {
            foreach (WorkshopQueueEntry request in pRequests)
            {
                WorkshopQueueEntry existing =
                    entries.FirstOrDefault(entry => entry.FileId == request.FileId && !entry.IsFinished);
                if (existing != null)
                {
                    result.Add(existing);
                    continue;
                }

                entries.RemoveAll(entry => entry.FileId == request.FileId);
                entries.Add(request);
                added.Add(request);
                result.Add(request);
            }

            start = added.Count > 0 && !IsRunning;
            if (start) IsRunning = true;
        }

        foreach (WorkshopQueueEntry entry in added) Notify(entry);
        if (start)
        {
            RunTask = RunAsync();
        }

        return result;
    }

    /// <summary>
    ///     Cancel an item. A queued item is dropped; the item in flight stops after rolling back the subscription
    ///     the queue made.
    /// </summary>
    public void Cancel(ulong pFileId)
    {
        List<WorkshopQueueEntry> targets;
        lock (entries_lock)
        {
            targets = entries.Where(entry => entry.FileId == pFileId && !entry.IsFinished).ToList();
        }

        foreach (WorkshopQueueEntry entry in targets) CancelEntry(entry);
    }

    public void CancelAll()
    {
        var dropped = new List<WorkshopQueueEntry>();
        var active = new List<WorkshopQueueEntry>();
        lock (entries_lock)
        {
            // Drop queued items before cancelling the active one: Cancel() can run the queue's continuation inline,
            // which would otherwise pick up the next still-queued item.
            foreach (WorkshopQueueEntry entry in entries.Where(entry => !entry.IsFinished))
            {
                if (entry.State == WorkshopQueueState.Queued && entry != Current)
                {
                    entry.State = WorkshopQueueState.Cancelled;
                    running_summary?.Entries.Add(entry);
                    dropped.Add(entry);
                }
                else
                {
                    active.Add(entry);
                }
            }
        }

        foreach (WorkshopQueueEntry entry in dropped) Notify(entry);
        foreach (WorkshopQueueEntry entry in active) entry.Cancellation.Cancel();
    }

    /// <summary>
    ///     Forget finished entries, e.g. after the user has seen the summary.
    /// </summary>
    public void ClearFinished()
    {
        lock (entries_lock)
        {
            entries.RemoveAll(entry => entry.IsFinished);
        }
    }

    private void CancelEntry(WorkshopQueueEntry pEntry)
    {
        bool dropped = false;
        lock (entries_lock)
        {
            if (pEntry.State == WorkshopQueueState.Queued && pEntry != Current)
            {
                pEntry.State = WorkshopQueueState.Cancelled;
                running_summary?.Entries.Add(pEntry);
                dropped = true;
            }
        }

        if (dropped)
        {
            Notify(pEntry);
            return;
        }

        // Outside the lock: Cancel() may run the queue's continuation inline.
        pEntry.Cancellation.Cancel();
    }

    private async Task RunAsync()
    {
        try
        {
            backend.DownloadItemResult += OnDownloadItemResult;
            while (true)
            {
                WorkshopQueueEntry next;
                lock (entries_lock)
                {
                    running_summary ??= new WorkshopQueueSummary();
                    next = entries.FirstOrDefault(entry => entry.State == WorkshopQueueState.Queued);
                    if (next == null) break;
                    Current = next;
                    running_summary.Entries.Add(next);
                }

                try
                {
                    await ProcessAsync(next);
                }
                catch (Exception e)
                {
                    // ProcessAsync handles its own errors; this keeps one broken item from stopping the queue.
                    LogService.LogError($"Workshop item {next.FileId} ({next.Title}) failed unexpectedly: {e.Message}");
                    LogService.LogException(e);
                    next.Error = e.Message;
                    SetState(next, WorkshopQueueState.Failed);
                }

                Current = null;
            }
        }
        catch (Exception e)
        {
            // Only reachable if the queue itself breaks (e.g. no backend); fail what is left instead of hanging.
            LogService.LogError($"Workshop download queue stopped: {e.Message}");
            LogService.LogException(e);
            foreach (WorkshopQueueEntry entry in Entries.Where(entry => !entry.IsFinished))
            {
                entry.Error = e.Message;
                SetState(entry, WorkshopQueueState.Failed);
                lock (entries_lock)
                {
                    running_summary ??= new WorkshopQueueSummary();
                    if (!running_summary.Entries.Contains(entry)) running_summary.Entries.Add(entry);
                }
            }
        }
        finally
        {
            if (backend != null) backend.DownloadItemResult -= OnDownloadItemResult;
            WorkshopQueueSummary summary;
            lock (entries_lock)
            {
                summary = running_summary ?? new WorkshopQueueSummary();
                running_summary = null;
                Current = null;
                IsRunning = false;
            }

            MainThreadDispatcher.Run(() => Finished?.Invoke(summary));
        }
    }

    private async Task ProcessAsync(WorkshopQueueEntry pEntry)
    {
        CancellationToken token = pEntry.Cancellation.Token;
        pEntry.Error = null;
        pEntry.Note = null;
        try
        {
            token.ThrowIfCancellationRequested();
            WorkshopItemLocalState state = backend.GetItemLocalState(pEntry.FileId);
            if (!state.Subscribed)
            {
                SetState(pEntry, WorkshopQueueState.Subscribing);
                bool subscribed = await backend.SubscribeModAsync(pEntry.FileId);
                if (subscribed) pEntry.SubscribedByQueue = true;
                token.ThrowIfCancellationRequested();
                if (!subscribed)
                {
                    await FailAsync(pEntry, "Steam refused the subscription.");
                    return;
                }
            }

            SetState(pEntry, WorkshopQueueState.WaitingForSteam);
            failed_download_result = null;
            bool started = backend.StartModDownload(pEntry.FileId, true);
            state = backend.GetItemLocalState(pEntry.FileId);
            if (!started && !state.IsUpToDate)
            {
                await FailAsync(pEntry, "Steam could not start the download.");
                return;
            }

            string last_signature = null;
            DateTime last_change = clock();
            while (!state.IsUpToDate)
            {
                token.ThrowIfCancellationRequested();
                if (state.Downloading)
                {
                    SetProgress(pEntry, WorkshopQueueState.Downloading, state.DownloadProgress);
                }
                else
                {
                    SetProgress(pEntry, WorkshopQueueState.WaitingForSteam, pEntry.Progress);
                }

                if (failed_download_result != null && !state.Downloading && !state.DownloadPending)
                {
                    await FailAsync(pEntry, $"Steam reported {failed_download_result}.");
                    return;
                }

                string signature =
                    $"{state.Downloading}|{state.DownloadPending}|{state.Installed}|{state.NeedsUpdate}|{state.DownloadProgress:F4}";
                if (signature != last_signature)
                {
                    last_signature = signature;
                    last_change = clock();
                }
                else if (clock() - last_change > StallTimeout)
                {
                    await FailAsync(pEntry,
                        $"Steam made no download progress for {StallTimeout.TotalSeconds:0} seconds. Is Steam offline?");
                    return;
                }

                await delay(PollIntervalMs, token);
                state = backend.GetItemLocalState(pEntry.FileId);
            }

            token.ThrowIfCancellationRequested();
            pEntry.InstallDirectory = state.Directory;
            SetProgress(pEntry, WorkshopQueueState.Installing, 1f);
            WorkshopInstallResult result = await MainThreadDispatcher.RunAsync(
                () => install_handler.OnItemInstalled(pEntry, state.Directory));
            if (result.Error != null)
            {
                await FailAsync(pEntry, result.Error, result.RollbackSubscription);
                return;
            }

            pEntry.Note = result.Note;
            SetState(pEntry, WorkshopQueueState.Completed);
            LogService.LogInfo($"Workshop item {pEntry.FileId} ({pEntry.Title}) installed at {state.Directory}");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            await RollbackSubscriptionAsync(pEntry);
            SetState(pEntry, WorkshopQueueState.Cancelled);
            LogService.LogInfo($"Workshop item {pEntry.FileId} ({pEntry.Title}) cancelled");
        }
        catch (Exception e)
        {
            LogService.LogException(e);
            await FailAsync(pEntry, e.Message);
        }
    }

    private async Task FailAsync(WorkshopQueueEntry pEntry, string pError, bool pRollbackInstalled = false)
    {
        LogService.LogError($"Workshop item {pEntry.FileId} ({pEntry.Title}) failed: {pError}");
        // Leave no subscription behind for an item the queue could not deliver. An installed item is only rolled
        // back when the install step asks for it (e.g. a dependency candidate that turned out to be the wrong mod).
        bool installed;
        try
        {
            installed = backend.GetItemLocalState(pEntry.FileId).Installed;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot read the state of Workshop item {pEntry.FileId}: {e.Message}");
            installed = false;
        }

        if (!installed || pRollbackInstalled)
        {
            await RollbackSubscriptionAsync(pEntry);
        }

        pEntry.Error = pError;
        SetState(pEntry, WorkshopQueueState.Failed);
    }

    private async Task RollbackSubscriptionAsync(WorkshopQueueEntry pEntry)
    {
        if (!pEntry.SubscribedByQueue) return;
        try
        {
            if (await backend.UnsubscribeModAsync(pEntry.FileId))
            {
                pEntry.SubscribedByQueue = false;
            }
            else
            {
                LogService.LogWarning($"Steam refused to unsubscribe Workshop item {pEntry.FileId}");
            }
        }
        catch (Exception e)
        {
            LogService.LogError($"Failed to unsubscribe Workshop item {pEntry.FileId}: {e.Message}");
            LogService.LogException(e);
        }
    }

    private void OnDownloadItemResult(bool pSucceeded, string pResult)
    {
        if (!pSucceeded) failed_download_result = pResult;
    }

    private void SetProgress(WorkshopQueueEntry pEntry, WorkshopQueueState pState, float pProgress)
    {
        // Notify on state changes and whole-percent steps only.
        bool changed = pEntry.State != pState || (int)(pEntry.Progress * 100) != (int)(pProgress * 100);
        pEntry.State = pState;
        pEntry.Progress = pProgress;
        if (changed) Notify(pEntry);
    }

    private void SetState(WorkshopQueueEntry pEntry, WorkshopQueueState pState)
    {
        pEntry.State = pState;
        Notify(pEntry);
    }

    private void Notify(WorkshopQueueEntry pEntry)
    {
        MainThreadDispatcher.Run(() => EntryChanged?.Invoke(pEntry));
    }
}
