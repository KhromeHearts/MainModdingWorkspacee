using NeoModLoader.services;
using RSG;

namespace NeoModLoader.Tests;

/// <summary>
///     Simulated Steam item. Each state read advances a started download: pending, then downloading in
///     <see cref="DownloadPolls" /> steps, then installed.
/// </summary>
internal sealed class FakeItem
{
    private int download_polls_done;

    public FakeItem(ulong pId)
    {
        Id = pId;
    }

    public ulong Id { get; }
    public bool Subscribed { get; set; }
    public bool Installed { get; set; }
    public bool NeedsUpdate { get; set; }
    public bool Downloading { get; private set; }
    public bool Pending { get; private set; }
    public float Progress { get; private set; }
    public bool DownloadStarted { get; private set; }
    public int DownloadPolls { get; set; } = 4;

    public bool RefuseSubscribe { get; set; }
    public bool RefuseDownload { get; set; }

    /// <summary>
    ///     Stay pending forever.
    /// </summary>
    public bool Stall { get; set; }

    /// <summary>
    ///     Steam reports this DownloadItemResult instead of finishing.
    /// </summary>
    public string FailWith { get; set; }

    public bool ThrowOnStateAfterStart { get; set; }
    public string Directory => $"C:/fake/workshop/{Id}";

    public void StartDownload()
    {
        DownloadStarted = true;
        Pending = true;
        download_polls_done = 0;
    }

    public void Advance(FakeWorkshopBackend pBackend)
    {
        if (!DownloadStarted || Stall) return;
        if (Pending)
        {
            Pending = false;
            Downloading = true;
            Progress = 0;
            return;
        }

        if (!Downloading) return;
        if (FailWith != null)
        {
            Downloading = false;
            DownloadStarted = false;
            pBackend.RaiseDownloadResult(false, FailWith);
            return;
        }

        download_polls_done++;
        Progress = Math.Min(1f, download_polls_done / (float)DownloadPolls);
        if (download_polls_done < DownloadPolls) return;

        Downloading = false;
        DownloadStarted = false;
        Installed = true;
        NeedsUpdate = false;
        pBackend.RaiseDownloadResult(true, "OK");
    }

    public WorkshopItemLocalState Snapshot()
    {
        return new WorkshopItemLocalState
        {
            Subscribed = Subscribed,
            Installed = Installed,
            NeedsUpdate = NeedsUpdate,
            Downloading = Downloading,
            DownloadPending = Pending,
            DownloadProgress = Progress,
            Directory = Installed ? Directory : null
        };
    }
}

internal sealed class FakeWorkshopBackend : IPlatformSpecificModWorkshopService
{
    private readonly object sync = new();
    public Dictionary<ulong, FakeItem> Items { get; } = new();
    public List<string> Calls { get; } = new();
    public bool LoggedOn { get; set; } = true;
    public Func<WorkshopBrowseQuery, int, WorkshopBrowsePage> Pages { get; set; }

    public event Action<bool, string> DownloadItemResult;

    public void UploadModLoader(string changelog)
    {
        throw new NotSupportedException();
    }

    public Promise UploadMod(string name, string description, string previewImagePath, string workshopPath,
        string changelog, bool verified)
    {
        throw new NotSupportedException();
    }

    public Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog)
    {
        throw new NotSupportedException();
    }

    public bool IsSteamLoggedOn()
    {
        return LoggedOn;
    }

    public Task<WorkshopBrowsePage> QueryModsAsync(WorkshopBrowseQuery pQuery, int pPage)
    {
        return Task.FromResult(Pages(pQuery, pPage));
    }

    public Task<WorkshopModInfo> QueryModDetailsAsync(ulong pFileId)
    {
        return Task.FromResult<WorkshopModInfo>(null);
    }

    public WorkshopItemLocalState GetItemLocalState(ulong pFileId)
    {
        lock (sync)
        {
            FakeItem item = Items[pFileId];
            if (item.ThrowOnStateAfterStart && item.DownloadStarted)
            {
                throw new InvalidOperationException("simulated Steam failure");
            }

            item.Advance(this);
            return item.Snapshot();
        }
    }

    public Task<bool> SubscribeModAsync(ulong pFileId)
    {
        lock (sync)
        {
            Calls.Add($"subscribe {pFileId}");
            FakeItem item = Items[pFileId];
            if (item.RefuseSubscribe) return Task.FromResult(false);
            item.Subscribed = true;
            return Task.FromResult(true);
        }
    }

    public Task<bool> UnsubscribeModAsync(ulong pFileId)
    {
        lock (sync)
        {
            Calls.Add($"unsubscribe {pFileId}");
            Items[pFileId].Subscribed = false;
            return Task.FromResult(true);
        }
    }

    public bool StartModDownload(ulong pFileId, bool pHighPriority)
    {
        lock (sync)
        {
            Calls.Add($"download {pFileId}");
            FakeItem item = Items[pFileId];
            if (item.RefuseDownload) return false;
            if (item.Installed && !item.NeedsUpdate) return true;
            item.StartDownload();
            return true;
        }
    }

    public Task<string> RequestPersonaNameAsync(ulong pSteamId)
    {
        return Task.FromResult($"user {pSteamId}");
    }

    public FakeItem Add(ulong pId, Action<FakeItem> pConfigure = null)
    {
        var item = new FakeItem(pId);
        pConfigure?.Invoke(item);
        Items[pId] = item;
        return item;
    }

    public List<string> CallsFor(ulong pId)
    {
        lock (sync)
        {
            return Calls.Where(call => call.EndsWith($" {pId}")).ToList();
        }
    }

    internal void RaiseDownloadResult(bool pSucceeded, string pResult)
    {
        DownloadItemResult?.Invoke(pSucceeded, pResult);
    }
}

internal sealed class FakeInstallHandler : IWorkshopInstallHandler
{
    public List<ulong> Installed { get; } = new();
    public Func<WorkshopQueueEntry, WorkshopInstallResult> Result { get; set; } = _ => WorkshopInstallResult.Success();

    public WorkshopInstallResult OnItemInstalled(WorkshopQueueEntry pEntry, string pDirectory)
    {
        lock (Installed)
        {
            Installed.Add(pEntry.FileId);
        }

        return Result(pEntry);
    }
}

/// <summary>
///     Virtual clock: every queue delay advances it, then yields briefly so tests can interleave.
/// </summary>
internal sealed class FakeTime
{
    private long ticks = new DateTime(2026, 1, 1).Ticks;
    public DateTime Now => new(Interlocked.Read(ref ticks));

    public async Task Delay(int pMilliseconds, CancellationToken pToken)
    {
        Interlocked.Add(ref ticks, TimeSpan.FromMilliseconds(pMilliseconds).Ticks);
        pToken.ThrowIfCancellationRequested();
        await Task.Delay(1, pToken);
    }
}
