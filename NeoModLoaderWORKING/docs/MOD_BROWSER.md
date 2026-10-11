# Mod Browser: investigation and design

This document records how Steam Workshop support works in NeoModLoader (NML) today, what the bundled
Facepunch.Steamworks build actually provides, and how the in-game Mod Browser is built on top of it.
Facts marked **verified** were checked against the code or binaries in this repository. Everything else is
a design decision and is labelled as one.

Contents:

1. [How a Workshop mod reaches the game today](#1-how-a-workshop-mod-reaches-the-game-today)
2. [Facepunch.Steamworks verification](#2-facepunchsteamworks-verification)
3. [Design](#3-design)
4. [Blockers and the nearest viable alternatives](#4-blockers-and-the-nearest-viable-alternatives)
5. [Testing](#5-testing)
6. [Test log (2026-10-04)](#6-test-log-2026-10-04)
7. [Manual in-game test plan](#7-manual-in-game-test-plan)
8. [Known limitations and open decisions](#8-known-limitations-and-open-decisions)

---

## 1. How a Workshop mod reaches the game today

### 1.1 Folders

| Location | Path | Who writes it | Notes |
|---|---|---|---|
| Local mods | `<game>/Mods` (`Paths.ModsPath`) | The user | Zips found here are unpacked first (`ModInfoUtils.TryToUnzipModZip`). |
| Native mods | `<game>/worldbox_Data/StreamingAssets/mods` (`Paths.NativeModsPath`) | The user and NML | NML itself lives here, plus its cache in `NML/`. |
| Workshop content | `<library>/steamapps/workshop/content/1206560/<fileid>/` | **Steam only** | NML derives it as `Paths.CommonModsWorkshopPath = <game>/../../workshop/content/1206560`. Steam always puts Workshop content in the library that holds the game, so the derived path and `Item.Directory` (from `ISteamUGC::GetItemInstallInfo`) agree. NML must never write into this folder, because Steam can replace or delete it at any time. |

### 1.2 From download to "recognized" (verified in `utils/ModInfoUtils.cs`, `api/ModDeclare.cs`)

1. Steam downloads a subscribed item into `workshop/content/1206560/<fileid>/`. When the game is not running
   this happens on its own. While it runs, it depends on Steam's "allow downloads during gameplay" setting.
2. NML only looks at disk **once, at startup**, in the `Load Mods Info And Prepare Mods` step of
   `WorldBoxMod.Update`, through `ModInfoUtils.findAndPrepareMods()`:
   - It scans `Mods`, then `StreamingAssets/mods`, then every folder under the Workshop content path.
     **The first mod with a given UID wins**, so a local copy shadows the Workshop copy ("Repeat Mod" warning).
   - `recogMod(folder)` looks for `mod.json`. If there is none at the root it searches recursively and
     takes the first match. It returns `null` when no `mod.json` exists.
   - The `ModDeclare(string)` constructor sets `IsWorkshopLoaded = true` when the path matches
     `.../workshop/content/1206560/<digits>/`.
   - Workshop mods with an empty `RepoUrl` get the Steam `filedetails` URL (`SetRepoUrlToWorkshopPage`).
   - Only `ModType == NEOMOD` Workshop mods are added. `BEPINEX` ones become a symlink request
     (`LinkBepInExModToLocalRequest`, which runs an elevated `mklink` later). Any other declared type is
     ignored for Workshop folders.
   - `EnsureRecognizedMod` puts the mod into `WorldBoxMod.AllRecognizedMods` as `DISABLED` if
     `mod_compile_records.json` marks it disabled, and `FAILED` otherwise. `FAILED` here means "wanted but
     not loaded yet".
3. `ModDepenSolveService.InitializeGraph` builds the dependency graph. `BuildStartupEnablePlan` turns every
   mod that is not disabled into a requested root and pulls in its dependencies.
   `ModCompileLoadService.CompileModNodes` then compiles the plan (Roslyn, in parallel stages), and
   `loadMods` loads the compiled DLLs with `Assembly.Load(bytes)`.

The consequence: **every Workshop mod that is installed when the game starts and has no "disabled" record is
compiled and loaded automatically.** No record at all counts as enabled.

### 1.3 The "subscribed" flow (`ui/WorkshopModListWindow.cs`, retired by this work)

- When the window opens it calls `ModWorkshopService.FindSubscribedMods()`. The backend pages through
  `Query.ItemsReadyToUse.WhereUserSubscribed().WithTag("Mod").SortByCreationDateAsc()` and keeps only items
  where `IsInstalled && !IsDownloadPending && !IsDownloading`. Items that are still downloading, not yet
  installed, or **need an update** are dropped or listed with stale files, and nothing tells the user.
- The tag constraint uses Steam's default "match all tags". Subscribed **"Unverified Mod" items never appear in
  this window**, yet `findAndPrepareMods` loads them at startup, because the folder scan ignores tags.
- The items go through a static `Queue` that `Update()` drains every 15 ms. `GetNextModFromWorkshopItem`
  calls `recogMod(item.Directory)` and dereferences the result without a null check, so an item with no
  `mod.json` throws a `NullReferenceException`. Every open re-runs the query and appends to the same static
  list, and a `HashSet` of UIDs hides the duplicates.
- **Why nothing compiles until "Load" is clicked:** the startup scan in 1.2 has already loaded everything
  that was installed at launch. Anything subscribed or downloaded *after* startup is invisible to NML,
  because the disk is never rescanned and `AllRecognizedMods` is not updated. The window's Load button is
  the only path that brings such a mod in, through `ModCompileLoadService.TryCompileAndLoadModAtRuntime`
  (build a runtime enable plan, compile, load, commit the plan). There is no automatic compilation on
  subscribe, which is the right call for arbitrary code. Runtime loading also cannot be undone: NML can't
  unload an assembly, and only `IReloadable` mods can be hot-patched (`ModReloadService`).

### 1.4 Tags (verified in the upload code)

| Tag | Set by | Meaning |
|---|---|---|
| `Mod` | `ModWorkshopService.UploadMod` when `ModUploadAuthenticationService.Authed` | The uploader passed NML's Discord or GitHub-organisation check. This is **not** a code review. |
| `Unverified Mod` | `UploadMod` when authentication was skipped | Anyone can upload as unverified. |
| `Mod Loader` | `UploadModLoader` (NML, file id `3080294469`) | NML itself. Not a mod. |
| `World` | The game (`WorkshopMaps.uploadMap`) | Saved worlds, not mods. |

Public population on 2026-10-04, read from the Steam Workshop browse pages: 105 `Mod`, 38 `Unverified Mod`,
2 `Mod Loader`.

### 1.5 A pre-existing bug that matters for security

`ModCompileLoadService.CompileModNodes` truncates `mod_compile_records.json` (`File.WriteAllText(path, "")`)
whenever any compile fails. At startup this is harmless, because `SaveModRecords()` runs right afterwards and
rewrites the file from memory. On the **runtime** path (`TryCompileModAtRuntime` /
`TryCompileAndLoadModAtRuntime`) a failed compile returns without saving. If the game is closed afterwards,
the next start reads an empty file and treats **every** mod as enabled, including mods the user disabled and
mods the browser installed as disabled. The browser's "installed but not enabled" guarantee depends on that
file, so this work saves the records on those failure paths too (see 3.7).

---

## 2. Facepunch.Steamworks verification

### 2.1 Method

- The game loads `worldbox_Data/Managed/Facepunch.Steamworks.Win64.dll`. **It is byte-identical to
  `resources/assemblies/Facepunch.Steamworks.Win64.dll`** (MD5 `9b6881f3af33f662ee9a2a8f07016b98`). NML's
  `winsteamwork` alias therefore binds to exactly the API checked here.
- Both DLLs target .NET Framework 4.6 and run on the game's Mono (Unity 2022.3.60, `MonoBleedingEdge`).
  Win64 is `Version=1.0.0.0` (Debug build, product attributes set). Posix is `Version=0.0.0.0` with no
  product attributes, so it is a separate build.
- All public **and non-public** members of the 33 relevant types (`Steamworks.Ugc.*`, `SteamUGC`,
  `SteamClient`, `SteamFriends`, `Friend`, `PublishedFileId`, `SteamUGCDetails_t`, the UGC enums, and so on)
  were dumped through reflection for both DLLs and diffed. **The two surfaces are identical.** Behaviour was
  then read from the IL of the methods listed below.

### 2.2 What exists (identical in Win64 and Posix)

| API asked about | Exists | Notes from the IL |
|---|---|---|
| `Query.All` | yes | `new Query(UgcType.All)`. The browser uses `Query.ItemsReadyToUse`, as the existing code does. |
| `Query.WithTag` / `WithoutTag` / `MatchAnyTag` / `MatchAllTags` | yes | Applied in `ApplyConstraints` through `AddRequiredTag`, `AddExcludedTag` and `SetMatchAnyTag`. |
| `Query.WhereSearchText` | yes | `SetSearchText` is applied whenever the text is non-empty. |
| `SortByPopular` / `MostRecent` | **no** | Neither name exists. Use `RankedByTotalUniqueSubscriptions`, `RankedByTrend` (+`WithTrendDays`), `RankedByVote`, `RankedByPublicationDate` or `RankedByTextSearch`. |
| `SortByTitleAsc`, `SortByUpdateDate`, `SortByCreationDate`, ... | yes | **Only honoured by user-list queries** (`WhereUserSubscribed`, `WhereUserPublished`, ...), which go through `CreateQueryUserUGCRequest`. `Query.All`/`Items` ignore them. |
| "Ranked by last updated" | **no** | The `UGCQuery` enum stops at `RankedByLifetimePlaytimeSessions = 18`. Steam's `k_EUGCQuery_RankedByLastUpdatedDate = 19` is absent and `queryType` is a private field. |
| `Query.GetPageAsync(page)` | yes | Returns `null` whenever the call fails or `Result != OK`, with no error detail. The `ResultPage` it returns owns a query handle that **must be disposed** (`ReleaseQueryUGCRequest`). The existing code never disposes it. Steam returns at most 50 items per page. |
| `ResultPage.Entries` | yes | Fills stats (`NumSubscriptions`, `NumFavorites`, ...) and `PreviewImageUrl` for every entry. |
| `Item.Subscribe()` / `Unsubscribe()` | yes | `Task<bool>`, true when `Result == OK`. |
| `Item.Download(highPriority)` | yes | `ISteamUGC::DownloadItem`. Returns false when the download cannot start. |
| `Item.DownloadAsync(progress, delay, ct)` | yes, but **not used** | It forwards to `SteamUGC.DownloadAsync`, which: (a) **turns a default `CancellationToken` into a 60-second timeout**; (b) waits for the *first* `DownloadItemResult` from **any** item before polling the item itself; (c) reports progress as 0, then 0.2, then 1, so there is no real progress while bytes are downloading. |
| `Item.NeedsUpdate`, `IsInstalled`, `IsSubscribed`, `IsDownloading`, `IsDownloadPending` | yes | Each call is a live `ISteamUGC::GetItemState`, not a snapshot. |
| `Item.DownloadAmount` | yes | Calls `GetItemDownloadInfo` directly while `IsDownloading`. **Used for progress.** |
| `Item.DownloadBytesDownloaded` / `DownloadBytesTotal` | yes, but **misleading** | Return the *installed size* unless `NeedsUpdate` is set, so they read 0 during a first-time download. Not used. |
| `Item.SizeBytes` | yes, but **misleading** | This is the size on disk from `GetItemInstallInfo`, so it is 0 for items that are not installed. The published file size is `SteamUGCDetails_t.FileSize`, an internal field. The browser reads it through guarded reflection and shows "unknown" if that fails. |
| `Item.VotesUp`, `VotesDown`, `Score` | yes | Taken from the query details. `Score` is Steam's 0..1 rating. |
| `Item.Vote(bool)`, `GetUserVote()` | yes | Not needed for browsing. |
| `Item.Created`, `Updated`, `Tags`, `Owner`, `Directory`, `Url`, `ChangelogUrl` | yes | **`Tags` are lower-cased** by `Item.From` (`"mod"`, `"unverified mod"`). `HasTag` ignores case. `ChangelogUrl` is a web page, not changelog text. |
| `Item.GetAsync(id)` | yes | Uses the deprecated `RequestUGCDetails`, which returns no stats and no preview. The details panel uses `Query.WithFileId(id).WithLongDescription(true)` instead. |
| `SteamClient.IsLoggedOn`, `IsValid` | yes | Used to tell "offline" apart from "Steam missing". |
| `Friend.RequestInfoAsync()` / `Friend.Name` | yes | `CacheUserInformationAsync(id, nameOnly: true)` followed by `GetFriendPersonaName`. Used for author names, with a timeout. |

Threading: the game calls `SteamClient.Init(1206560, asyncCallbacks: true)` on the main thread, so Facepunch
dispatches callbacks from a loop that resumes on Unity's `UnitySynchronizationContext`. `await`ing Facepunch
tasks from the main thread therefore resumes on the main thread. The browser does not depend on that alone:
all UI notifications go through `MainThreadDispatcher`.

### 2.3 Differences between Win64 and Posix

None at the API level: the dumps, including private fields and compiler-generated state machines, are
identical. They differ only in assembly version and metadata (see 2.1) and in which native library they
P/Invoke. Code written against one compiles unchanged against the other, which is why both backends stay
line-for-line parallel.

---

## 3. Design

### 3.1 Layers

```
ui/NewModListWindow  ──tabs──►  "Installed" (existing parts)  |  "Mod Browser" (ui/ModBrowserPanel)
                                                                     │  uses
services/ModBrowserService (static, [Experimental]) ── status, installs, dependencies, Update All, Apply
   ├─ services/WorkshopBrowseSession     paging, client-side sorts, dedup        (pure, unit tested)
   ├─ services/WorkshopDownloadQueue     subscribe → download → install queue   (pure, unit tested)
   ├─ services/ModBrowserDependencyResolver  UID → Workshop candidate matching   (pure, unit tested)
   ├─ services/ModBrowserSettings        persisted filters, sort, warning flag
   └─ utils/WorkshopThumbnailLoader      HTTP + disk + memory cache, placeholders
services/ModWorkshopService (static facade, [Experimental]) ── waits for steamWorkshopPromise, raises typed errors
   └─ IPlatformSpecificModWorkshopService ── ModWorkshopServiceWindows / ModWorkshopServiceUnix (Facepunch)
utils/MainThreadDispatcher ── marshals callbacks onto Unity's main thread
```

Facepunch types never leave the two backends. Win64 and Posix declare identical type names under different
`extern alias`es, so the backends convert items into platform-neutral types (`services/WorkshopModels.cs`):
`WorkshopModInfo` (a query snapshot), `WorkshopItemLocalState` (live install and download state),
`WorkshopBrowseQuery`, `WorkshopBrowsePage`, and `WorkshopException` with a `WorkshopErrorKind`.

### 3.2 Backend additions (`IPlatformSpecificModWorkshopService`, implemented in both backends)

| Method | Facepunch implementation |
|---|---|
| `bool IsSteamLoggedOn()` | `SteamClient.IsValid && SteamClient.IsLoggedOn` |
| `Task<WorkshopBrowsePage> QueryModsAsync(query, page)` | `Query.ItemsReadyToUse` + tag constraints + search + ranking (3.3), `GetPageAsync`, convert, **dispose** |
| `Task<WorkshopModInfo> QueryModDetailsAsync(id)` | `WithFileId(id).WithLongDescription(true)` |
| `WorkshopItemLocalState GetItemLocalState(id)` | `new Item(id)`: state flags, `DownloadAmount`, `Directory` |
| `Task<bool> SubscribeModAsync(id)` / `UnsubscribeModAsync(id)` | `Item.Subscribe()` / `Unsubscribe()` |
| `bool StartModDownload(id, highPriority)` | `SteamUGC.Download(id, highPriority)` |
| `Task<string> RequestPersonaNameAsync(steamId)` | `Friend.RequestInfoAsync()` + `Name`, with a timeout |
| `event Action<bool, string> DownloadItemResult` | Forwards `SteamUGC.OnDownloadItemResult` |

`FindSubscribedMods` and `GetNextModFromWorkshopItem` are removed together with `WorkshopModListWindow`,
their only caller. The browser's "Subscribed" scope replaces them.

`ModWorkshopService` keeps its `[Experimental]` marker. Before any Steam call it awaits
`steamWorkshopPromise` through a Promise-to-Task adapter and throws:

- `WorkshopException(SteamUnavailable)` when the promise was rejected (Steam not running, or the game
  started without Steam), and also in the Unity editor;
- `WorkshopException(Offline)` when `IsSteamLoggedOn()` is false;
- `WorkshopException(QueryFailed)` when `GetPageAsync` returns `null`.

### 3.3 Browsing: search, sort, tags, paging

- **Tags** (decision): "Mod" and "Unverified Mod" are two independent toggles, both on by default, plus
  `WithoutTag("Mod Loader")`. Both on means `MatchAnyTag`. One on means a required tag, and verified-only
  also excludes `Unverified Mod`. NML's own item is hidden. Unverified items are **shown but flagged**: an
  orange `UNVERIFIED` badge on rows and in the details panel, an extra caution line in every install
  confirmation, and a count of unverified items in the Download All prompt. Hiding them would make 38 of
  143 mods invisible while Steam can still install them. Labelling them "verified" would overstate what the
  authentication proves.
- **Sorts**. Server-side, paged lazily (50 per page, the next page loads when the list is scrolled to the end
  or "More" is clicked):

  | UI label | Steam query |
  |---|---|
  | Most subscribed (default) | `RankedByTotalUniqueSubscriptions` |
  | Trending | `RankedByTrend().WithTrendDays(30)` |
  | Top rated | `RankedByVote` |
  | Newest | `RankedByPublicationDate` |
  | Relevance (only with search text) | `RankedByTextSearch` |

  Client-side, because Steam has no all-items equivalent in this Facepunch build (see 2.2):
  **Recently updated** and **Name**. The session fetches every page (capped at 40 pages, which is 2,000
  items, and says so if the cap is hit), removes duplicates by file id, and sorts locally by `Updated`
  descending or title (ordinal, case-insensitive). With about 143 matching items that is three queries.
- **Scope**: *All mods* or *Subscribed* (`WhereUserSubscribed()`). A user's subscriptions are few, so the
  Subscribed scope always fetches all pages and sorts locally. Every sort, including Name and Recently
  updated, is then exact.
- **Search**: `WhereSearchText` is applied server-side for every sort. "Relevance" becomes available while
  text is entered.
- `WorkshopBrowseSession` (pure logic) owns the pages loaded so far, the total count, the next page, a
  loading flag and the last error. A stale response (the query changed while a page was in flight) is
  dropped by comparing generation counters.

### 3.4 Per-mod state

`ModBrowserService.GetStatus(fileId)` combines the queue entry, Steam's live state and NML's recognized-mod
state, in this priority order:

| State | Condition |
|---|---|
| Queued / Subscribing / Waiting for Steam / Downloading NN% / Installing | Queue entry in that state |
| Failed: *reason* | Last queue attempt failed (cleared by the next attempt) |
| Downloading NN% | Steam is downloading it outside the queue (`IsDownloading`/`IsDownloadPending`) |
| Update available | `Installed && NeedsUpdate` |
| Incompatible | Installed but no `mod.json`, or `IncompatibleWith` names an enabled or loaded mod |
| Installed / Disabled (BepInEx) | A BepInEx mod: NML never loads it, but links enabled ones into BepInEx at startup. The details panel explains this |
| Missing dependency | A `Dependencies` UID that is neither recognized nor present in the Mods, native or Workshop folders (read without registering anything, unlike `ModInfoUtils.TryFindMod`) |
| Enabled / Disabled / Failed to load | `AllRecognizedMods` state, with `FailReason` in the details panel |
| Enabled, not loaded yet | Wanted but not loaded this session (Apply or restart) |
| Restart required | Updated or unsubscribed while loaded (NML cannot unload) |
| Installed (local copy wins) | The UID is recognized from another folder, such as `Mods/` |
| Not installed | Otherwise |

`mod.json` files are read with `recogMod` and cached per install directory. The cache is invalidated when the
queue finishes an item or when the panel refreshes.

### 3.5 Download queue (`WorkshopDownloadQueue`)

- Runs entries **one at a time**, which gives accurate per-item progress and lets a download result be
  attributed to the item in flight. For each entry:
  1. `Subscribing`: `SubscribeModAsync` unless already subscribed. Records `SubscribedByQueue`.
  2. `StartModDownload(id, highPriority: true)`. If it returns false and the item is installed and current,
     go straight to step 4. Otherwise the entry fails.
  3. Polls `GetItemLocalState` every 200 ms. Progress comes from `DownloadAmount`. The entry is done when
     `Installed && !NeedsUpdate && !Downloading && !DownloadPending`. A non-OK `DownloadItemResult` while
     the item is neither downloading nor pending fails it. **Stall detection:** no change in state or
     progress for 90 s fails it with "Steam made no download progress for 90 seconds. Is Steam offline?". A
     pending-but-stuck download is the typical offline symptom.
  4. `Installing`: calls `IWorkshopInstallHandler.OnItemInstalled` (3.6) on the main thread.
- **Error isolation:** each entry runs in its own `try/catch`. Failures are recorded on the entry and logged
  through `LogService`, and the queue moves on. **One failure never aborts the queue.** A failed item that the
  queue subscribed and Steam did not install is unsubscribed again, so failures leave no subscription behind.
- **Batches**: `Enqueue(IEnumerable<...>)` adds a whole batch before processing starts, so Download All runs and
  is summarized as one batch even when early items fail immediately.
- **Cancel**: `Cancel(id)` and `CancelAll()`. Cancelling the item in flight stops waiting and, **if the queue
  subscribed it and it is not installed yet, unsubscribes it** so Steam drops the download. An update of an
  item that was already subscribed is left alone, because unsubscribing would uninstall it. Entries still
  queued become `Cancelled`.
- Events (`EntryChanged`, and `Finished` with a summary of the batch) are raised through
  `MainThreadDispatcher`.
- Timing and delays are injected (`Func<DateTime>`, `Func<int, CancellationToken, Task>`), so tests run in
  milliseconds.

### 3.6 Installing, Download All, Update All, dependencies, Apply

- **Install handler** (in `ModBrowserService`), called once Steam reports the item installed:
  - `recogMod(directory)`. No `mod.json` means the item stays installed but is reported as "Not a NeoModLoader
    mod". The browser never fires the elevated BepInEx `mklink` itself: a new BepInEx install is recorded
    disabled (unless opted in), and startup linking now skips disabled BepInEx mods.
  - A **new install** is recorded as **disabled** (`ModInfoUtils.setModDisabled(uid, true)`, saved through
    `SaveModRecords`) unless the user opted into "Enable after install". It is then registered with
    `EnsureRecognizedMod` and `ModDepenSolveService.EnsureNode`. **Nothing is compiled or loaded.**
  - An **update** never changes the enabled state. If the mod is loaded, its compile timestamp is cleared and
    the entry is marked "Restart required". If it is not loaded, the recognized declaration is replaced with
    the new `mod.json`.
  - If an expected UID was set (dependency installs), the installed UID must match it. Otherwise the entry
    fails and the subscription is rolled back.
- **Download All**: queues every listed item that is not installed or needs an update. The confirmation shows
  the item count, the approximate total size (`FileSize`, "unknown" for any item where it couldn't be read),
  the number of unverified items, and an **"Enable after install" switch that is off every time** (the choice
  is deliberately not remembered). On a lazily paged query, the remaining pages are loaded first, so "all"
  means every match of the filters. The bottom bar shows the current item, its progress and how many are
  queued, plus Cancel. The summary dialog lists every failure.
- **Update All**: fetches the Subscribed scope (all pages) and queues every item with `NeedsUpdate`.
- **Dependencies** (via `ModDepenSolveService`). Before download, a Workshop item exposes no NML dependency
  data, because `mod.json` lives inside the content (see 4.3). After install:
  1. Missing UIDs are those that are neither recognized nor found in the local, native or Workshop folders.
  2. `ModBrowserDependencyResolver` searches the Workshop for each missing UID. It turns the UID into search
     words and accepts a candidate only if its mangled title (`ParseDepenNameToPreprocessSymbol`) equals the
     UID or is an `_`-separated suffix of it. NML uploads use the mod name as the Workshop title.
  3. The user sees one confirmation listing what was found and what wasn't. Confirmed candidates are queued
     with their expected UID, verified after download, and rolled back on a mismatch.
  4. Enabling a mod uses the existing `ModCompileLoadService.TryEnableMod`, whose
     `BuildRuntimeEnablePlan` enables the dependencies, reports missing or incompatible mods, and rolls back
     on failure.
- **Apply** (bottom bar): for every browser-installed mod that is wanted but not loaded this session, calls
  `ModCompileLoadService.TryEnableMod` (the existing compile and load path) through a new internal overload
  that suppresses the per-mod error popup. Failures are collected into one summary. Updated loaded mods that
  implement `IReloadable` get a "Reload" button in the details panel, which calls the existing
  `ModReloadService.ReloadMod`. All other updated loaded mods say "Restart required".

### 3.7 Persistence

- **Enabled state**: the existing `mod_compile_records.json`, through `ModInfoUtils.setModDisabled` and
  `SaveModRecords`. The runtime compile-failure paths now also call `SaveModRecords`, fixing the record wipe
  in 1.5.
- **Filters, sort, scope, warning acknowledged**:
  `NML/mod_browser_settings.json` (`ModBrowserSettings`). The search text is kept only for the current game
  session (the "X" next to the box clears it), so an old search never narrows the list after a restart;
  `search_text` in files from earlier builds is ignored, and a saved Relevance sort falls back to Most
  subscribed. This is a decision: the records file is a
  `Dictionary<string, ModCompilationCache>` keyed by mod UID. It is truncated on compile failure (1.5), and
  older NML versions deserialize it strictly. Mixing UI preferences into it would either break that schema
  for downgraded installs or put the preferences at risk of the wipe. Corrupt or missing settings fall back
  to defaults with a logged warning.
- **Thumbnails**: `NML/workshop_thumbnails/<fileid>_<urlhash>.img` on disk and an in-memory cache with an
  LRU cap of 512 sprites. A changed preview URL means a new hash, so the image is fetched again. Cached images
  still show while offline.

### 3.8 Thumbnails (`WorkshopThumbnailLoader`)

- The NML logo is shown immediately as a placeholder.
- The disk cache is tried first. Otherwise the loader downloads with the `HttpClient` NML already uses (no new
  Unity module references), at most 4 at a time, with a 15 s timeout, off the main thread.
- The bytes are decoded into a `Texture2D` on the main thread.
- **Verified on the live CDN:** appending `?imw=128&imh=128&ima=fit&impolicy=Letterbox` to a Steam preview
  URL returns a 128×128 JPEG (about 4.7 KB instead of the 71 KB original). That URL is tried first. If Unity
  cannot decode it, the loader tries the original, and after that keeps the placeholder. A failure is logged
  once per item.

### 3.9 UI

- `NewModListWindow` (window id `NMLMenu`) gains a third tab button in its left column. The icon is
  `ui/icons/iconSteam`, the tooltip "Mod Browser". The existing "Installed" parts are hidden while the
  browser tab is active, and `ModBrowserPanel` fills the same 522×255 area:
  - **Toolbar**: search `TextInput` + clear ("X") and search buttons, sort cycle button,
    "Verified"/"Unverified" tag toggles, scope toggle (All/Subscribed), refresh.
  - **Results** (left): a scroll list of `ModBrowserListItem` rows (`APrefab`, pooled with
    `ObjectPoolGenericMono`). Each row shows the icon, title, an `UNVERIFIED` badge, the author, subscribers,
    rating, the updated date, the version (installed mods only), a short description and a coloured state
    badge with progress.
  - **Details** (right): icon, title, author, tags, state, stats (subscribers, votes, rating, size, created,
    updated, Workshop id, installed version), dependencies with their resolution status, and a scrolling full
    description (the fixed lines NML's uploader adds around every description are hidden). Action buttons:
    Install / Update / Retry / Cancel / Enable / Disable / Uninstall / Find dependencies / Steam page /
    Changelog / Folder / Reload.
  - **Bottom bar**: status text ("Showing 50 of 143", errors, queue progress), a progress bar, and Download
    All / Update All / Cancel / Apply / More.
  - **Dialogs**: a modal overlay inside the panel with a message, an optional switch (`SwitchButton`) and
    confirm/cancel (`SimpleButton`). It is used for the security warning, install confirmations, Download All,
    dependency installs and summaries. A dialog never replaces an open one; later ones wait in a queue.
- A new power button, `NML_ModBrowser`, in the Main tab opens `NMLMenu` on the browser tab. The legacy
  `ModListWindow` Steam button now opens the browser too. `WorkshopModListWindow` is removed from
  `UIManager`, and its code is deleted.
- Locale keys are added to `en.json`, `cz.json` (Simplified Chinese, per WorldBox's language codes) and
  `ch.json` (Traditional Chinese). Missing languages fall back to English through `LM.ApplyLocale`.

### 3.10 Security

- **One-time warning** before the first browse: Workshop mods are arbitrary code with the user's
  permissions, NML neither reviews nor sandboxes them, and "unverified" means the uploader skipped NML's
  community check while "verified" does not mean reviewed. Acceptance is stored in `ModBrowserSettings`.
- Installs are disabled by default (3.6), including BepInEx mods. Download All enables nothing unless the switch
  is turned on, and even then it only marks the mods as wanted. They compile on Apply or at the next start.
- Unverified items need an extra confirmation to install, and Download All states how many it includes.
- Workshop titles, descriptions and author names are shown in texts with rich text disabled, so they cannot
  inject Unity markup.
- **No automatic compile or load anywhere in the browser.** Every compile goes through a button.
- Installing subscribes the user's Steam account to the item, which is how Workshop content persists. The
  confirmation says so. Uninstall unsubscribes.

### 3.11 Errors (all logged through `LogService`; nothing is swallowed)

| Situation | What the user sees | Installed tab |
|---|---|---|
| Steam not running or unavailable | "Steam is not available. Start Steam and restart the game to browse the Workshop." with Retry | unaffected |
| Steam offline | "You are offline. The Mod Browser needs a connection to Steam." with Retry. Cached thumbnails still show | unaffected |
| Query failed | "Steam Workshop query failed." with Retry | unaffected |
| Download stalled or failed | Failed state on the entry, with the reason. The queue continues | — |
| Not an NML mod | Incompatible state and explanation | — |
| BepInEx mod | Installed/Disabled state, the details explain the startup linking | — |
| Missing dependency | State plus a "Find dependencies" action | — |
| Compile or load failure | Existing `FailReason` in the details panel. Apply summarises them all | — |
| Incompatible NML or game version | Shown as a compile or load failure (see 4.4) | — |

---

## 4. Blockers and the nearest viable alternatives

1. **Changelog text.** `ISteamUGC` has no API for change notes, and Facepunch only exposes `ChangelogUrl`.
   *Alternative:* a "Changelog" button opens the Steam change-notes page, and the details panel shows the
   `Updated` date and the installed `mod.json` version.
2. **"Recently updated" and "Name" across all mods.** The Facepunch build has no `RankedByLastUpdatedDate`
   or title ranking for all-items queries (2.2). *Alternative:* fetch every page and sort locally. That is
   exact and cheap at today's scale (about 143 items, 3 queries) and capped at 2,000 items. The Subscribed
   scope sorts locally as well. Setting the private `queryType` to 19 through reflection was rejected,
   because it can't be tested offline and depends on Steam backend behaviour.
3. **Version and dependencies before download.** NML writes them only into `mod.json` inside the content.
   Steam metadata and key-value tags exist in Facepunch, but `Item` exposes neither, and NML's uploader
   doesn't set them. *Alternative:* show the version once installed (rows omit it until then), and resolve
   dependencies after download with UID verification and rollback (3.6). *Possible future step, needs your
   decision:* have the uploader add a key-value tag `nml_uid=<UID>`. The public `Query.AddRequiredKeyValueTag`
   could then find dependency items exactly. This touches publishing, which this work leaves unchanged.
4. **"Incompatible NML or game version".** `mod.json` has no NML version field. `targetGameBuild` is documented
   as "not implemented yet", and the NCMS layer defaults it to 444, so it cannot be used as a compatibility
   gate. *Alternative:* incompatibility shows up as a compile or load failure with Roslyn's errors in
   `FailReason`. A `minNmlVersion` field would need your decision (schema change).
5. **Unloading.** NML cannot unload assemblies. Disabling, uninstalling or updating a loaded mod takes effect
   after a restart, and the UI says so. Hot reload exists only for `IReloadable` mods.
6. **Steam "Required items".** `NumChildren` exists only in the internal details struct, and Facepunch does
   not expose `GetQueryUGCChildren`. NML uploads never set them either, so `mod.json` remains the only
   dependency source.
7. **Published size accuracy.** `SteamUGCDetails_t.FileSize` is a 32-bit field that Valve documents as
   possibly inaccurate for non-legacy items. It is read through reflection (internal) and labelled
   approximate.

---

## 5. Testing

### 5.1 Unit tests (`tests/NeoModLoader.Tests`)

A dependency-free net48 console runner (no test framework packages; run `dotnet run` in that folder, optionally
with part of a test name as a filter). It drives the Unity-free logic against an in-memory fake Steam backend
(`Fakes.cs`) whose items step through pending, downloading and installed on each state read, with a virtual
clock:

| Area | Tests |
|---|---|
| `WorkshopDownloadQueue` | install end to end (state sequence, calls, summary); one batch with six different forced failures (subscribe refused, download refused, stall timeout, Steam error result, exception while polling, install-handler error) plus one success, checking isolation, error texts and subscription rollback; CancelAll during a download (active item rolled back, queued items never started); cancelling one queued item; cancelling an update keeps the existing subscription; update of an installed item; up-to-date item skips the download; wrong dependency candidate rolled back although installed; duplicate enqueue; retry after failure; a broken queue fails its entries instead of hanging |
| `WorkshopBrowseSession` | lazy paging with duplicates across pages; fetch-all with local Name / Recently updated sorts; page cap; Subscribed scope sorted locally; errors kept; results of a replaced query dropped; NML's own item hidden; Relevance needs search text; verification from tags |
| `ModBrowserDependencyResolver` | search terms; title-to-UID matching (punctuation, explicit GUID, near misses); candidate ranking (longest match, verified, subscribers) |
| `ModBrowserSettings` | defaults, round trip, invalid files fall back to defaults |
| `ModBrowserText` | removal of NML's upload boilerplate (also with CRLF and a truncated footer), BBCode stripping, truncation |

Two of these tests caught real bugs during development: `CancelAll` could start the next queued item from
inside `CancellationTokenSource.Cancel()` (continuations run inline), and items that fail without awaiting
could end a run before the rest of a Download All batch was queued.

Not unit tested, because it needs Unity or Steam: the Facepunch backends, `ModBrowserService` (it uses
`ModInfoUtils`, `Paths` and the compile services), `WorkshopThumbnailLoader` and the UI.

### 5.2 Facepunch verification

See section 2: reflection dumps of both DLLs, an IL read of every method the backends call, and a check of the
live Steam image CDN. This verifies the API contract, not runtime behaviour against Steam.

## 6. Test log (2026-10-04)

- Build: `dotnet build NeoModLoader.csproj` succeeds with 0 errors. The 201 warnings are the existing
  missing-XML-comment warnings; the new code adds none.
- Unit tests: 30 passed, 0 failed (repeated runs stable).
- In game: **not tested.** The build was deployed with `prepare_debug.ps1` and the game was started from the
  Steam install, but `SteamAPI_Init` failed ("Steam isn't running, couldn't find Steam, AppId is unreleased,
  Don't own AppId") and WorldBox quit itself, as `SteamSDK` does when Steam is unavailable. Steam was running;
  the install's `appmanifest_1206560.acf` lists a different owner account than the one logged in, so this
  account has no usable WorldBox license. The ownership check was not worked around. The original NML files,
  the `NML` folder and Workshop subscriptions were restored afterwards; NML had not reached mod loading, so no
  records changed.
- Therefore unverified at runtime: real Workshop queries, paging, persona names, thumbnails, subscribe and
  download, progress, cancel, Download All / Update All, the install handler, Apply, the UI layout and all
  in-game error states (offline, Steam missing).

## 7. Manual in-game test plan

Run with a Steam account that owns WorldBox, after `dotnet build NeoModLoader.csproj` (which deploys the
Debug build):

1. Open the Mod Browser from the Main power tab. The security warning appears once; Cancel returns to the
   installed list, accepting loads results. Check row layout, thumbnails, author names and the details panel.
2. Search, each sort (Name and Recently updated load every page first), the Verified/Unverified toggles
   (turning both off is not possible), Subscribed scope, scrolling to the end and "More" (lazy paging),
   Refresh, the "X" that clears the search. Restart the game: filters and sort are remembered, the search box
   is empty.
3. Install a small verified mod: progress, then "Installed, disabled"; it appears disabled in the installed
   list; `mod_compile_records.json` marks it disabled. Enable: it compiles and loads; Disable: "Restart
   required".
4. Install an unverified mod: extra confirmation.
5. Download all on a narrow search: the confirmation shows count, size and unverified count; leave "Enable
   after install" off. Cancel during the second item: the active item is unsubscribed, the rest cancelled.
   Repeat with the switch on, then press Apply.
6. Uninstall: Steam unsubscribes; a loaded mod says "Restart required".
7. A mod with a dependency that is not installed shows "Missing dependency"; Find deps lists candidates;
   installing one verifies its UID.
8. Update all with nothing outdated shows "up to date". To test an update, revert an item with an older
   version (or wait for an author update) while the game runs.
9. Error states: start Steam in offline mode (offline message, installed tab unaffected); start the game
   without Steam if possible (Steam unavailable message).

## 8. Known limitations and open decisions

- Dependencies and versions are only known after download (4.3). Adding an `nml_uid` key-value tag in the
  uploader would allow exact dependency lookups; it changes publishing, so it needs a maintainer decision.
- No machine-readable NML or game version compatibility in `mod.json` (4.4); a `minNmlVersion` field needs a
  schema decision.
- Changelog text is a link to the Steam page (4.1).
- Recently updated and Name sorts fetch everything (up to 2,000 items) because Steam has no all-items ranking
  for them in this Facepunch build (4.2).
- Runtime-loaded mods do not get `IStagedLoad.Init`/`PostInit` (existing behaviour of the runtime load path,
  also before this work).
- Unsubscribing removes files only when Steam gets to it; until then the item shows "Uninstalled (restart)".
- The Simplified and Traditional Chinese strings were written without a native-speaker review.
