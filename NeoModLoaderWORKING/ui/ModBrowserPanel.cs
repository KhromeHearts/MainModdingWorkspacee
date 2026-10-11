using System.Text;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.ui.prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     The "Mod Browser" tab of <see cref="NewModListWindow" />: browse, install, update, enable and disable Steam
///     Workshop mods.
/// </summary>
internal class ModBrowserPanel : MonoBehaviour
{
    public static readonly Vector2 Size = new(522, 255);
    private const float refresh_interval = 1f;
    private const int max_description_length = 6000;

    private static readonly WorkshopBrowseSort[] sort_cycle =
    {
        WorkshopBrowseSort.MostSubscribed, WorkshopBrowseSort.Trending, WorkshopBrowseSort.TopRated,
        WorkshopBrowseSort.Newest, WorkshopBrowseSort.RecentlyUpdated, WorkshopBrowseSort.Name,
        WorkshopBrowseSort.Relevance
    };

    private readonly List<ModBrowserListItem> active_rows = new();
    private readonly Queue<Action> queued_dialogs = new();
    private SimpleButton apply_button;
    private bool busy;
    private SimpleButton cancel_button;
    private SimpleButton changelog_button;
    private Text details_author;
    private GameObject details_content;
    private Text details_dates;
    private Text details_dependencies;
    private Text details_description;
    private string details_description_source;
    private ScrollRect details_description_scroll;
    private Text details_hint;
    private Image details_icon;
    private Text details_state;
    private Text details_stats;
    private Text details_title;
    private SimpleButton dependencies_button;
    private GameObject dialog;
    private SimpleButton dialog_cancel;
    private SimpleButton dialog_confirm;
    private Text dialog_message;
    private Action dialog_on_cancel;
    private Action dialog_on_confirm;
    private General.UI.Prefabs.SwitchButton dialog_switch;
    private Text dialog_switch_label;
    private GameObject dialog_switch_row;
    private bool dialog_switch_value;
    private Text dialog_title;
    private SimpleButton download_all_button;
    private SimpleButton folder_button;
    private bool meta_dirty;
    private float meta_refresh_timer;
    private SimpleButton more_button;
    private RectTransform progress_fill;
    private GameObject progress_bar;
    private SimpleButton primary_button;
    private WorkshopBrowseQuery query;
    private float refresh_timer;
    private SimpleButton refresh_button;
    private SimpleButton reload_button;
    private RectTransform results_content;
    private GameObject results_message;
    private Text results_message_text;
    private SimpleButton results_retry_button;
    private ScrollRect results_scroll;
    private ObjectPoolGenericMono<ModBrowserListItem> row_pool;
    private SimpleButton scope_button;
    private TextInput search_input;
    private SimpleButton search_button;
    private SimpleButton clear_search_button;
    private WorkshopModInfo selected;
    private WorkshopModInfo selected_details;
    private WorkshopBrowseSession session;
    private SimpleButton sort_button;
    private Text status_text;
    private string busy_text;
    private bool steam_check_running;
    private string steam_error;
    private bool steam_ready;
    private SimpleButton toggle_button;
    private SimpleButton uninstall_button;
    private SimpleButton unverified_button;
    private SimpleButton update_all_button;
    private SimpleButton verified_button;
    private SimpleButton steam_page_button;
    private NewModListWindow window;

    public static ModBrowserPanel Create(Transform pParent, NewModListWindow pWindow, Vector2 pPosition)
    {
        var obj = new GameObject(nameof(ModBrowserPanel), typeof(RectTransform));
        obj.transform.SetParent(pParent, false);
        obj.transform.localPosition = pPosition;
        obj.GetComponent<RectTransform>().sizeDelta = Size;
        obj.SetActive(false);

        var panel = obj.AddComponent<ModBrowserPanel>();
        panel.window = pWindow;
        panel.query = ModBrowserService.Settings.ToQuery();
        panel.Build();
        return panel;
    }

    /// <summary>
    ///     Called when the tab becomes visible.
    /// </summary>
    public void Open()
    {
        RefreshTexts();
        SyncToolbar();
        if (!ModBrowserService.Settings.WarningAccepted)
        {
            // The warning may still be open from the last visit to this tab.
            if (!dialog.activeSelf) ShowWarning();
            return;
        }

        EnsureSteamAndLoad(false);
    }

    private void OnEnable()
    {
        ModBrowserService.ItemChanged += OnItemChanged;
        ModBrowserService.QueueFinished += OnQueueFinished;
        ModBrowserService.OwnerNameResolved += OnOwnerNameResolved;
        refresh_timer = 0;
    }

    private void OnDisable()
    {
        ModBrowserService.ItemChanged -= OnItemChanged;
        ModBrowserService.QueueFinished -= OnQueueFinished;
        ModBrowserService.OwnerNameResolved -= OnOwnerNameResolved;
    }

    private void Update()
    {
        // Windows may pause the game, so use unscaled time.
        refresh_timer -= Time.unscaledDeltaTime;
        meta_refresh_timer -= Time.unscaledDeltaTime;
        if (refresh_timer <= 0 || (meta_dirty && meta_refresh_timer <= 0))
        {
            refresh_timer = refresh_interval;
            meta_refresh_timer = 0.3f;
            meta_dirty = false;
            RefreshAll();
        }

        // Lazy loading: fetch the next page when the list is scrolled to the end.
        if (steam_ready && session != null && session.Query.IsServerPaged && session.HasMorePages &&
            !session.IsLoading && session.LastError == null && results_scroll.verticalNormalizedPosition <= 0.02f &&
            results_content.rect.height > ((RectTransform)results_scroll.transform).rect.height)
        {
            _ = session.LoadNextPageAsync();
        }
    }

    #region Loading

    private async void EnsureSteamAndLoad(bool pReload)
    {
        if (steam_check_running) return;
        steam_check_running = true;
        if (session == null || session.Items.Count == 0) ShowResultsMessage(LM.Get("mod_browser_status_connecting"), false);
        try
        {
            await ModWorkshopService.EnsureSteamAvailableAsync();
            steam_ready = true;
            steam_error = null;
            if (session == null)
            {
                session = ModBrowserService.CreateSession();
                session.Changed += () => MainThreadDispatcher.Run(OnSessionChanged);
            }

            if (pReload || (session.LoadedPages == 0 && !session.IsLoading))
            {
                ModBrowserService.InvalidateLocalCaches();
                await session.ResetAsync(query);
            }
            else
            {
                OnSessionChanged();
            }
        }
        catch (WorkshopException e)
        {
            steam_ready = false;
            steam_error = ErrorText(e);
            LogService.LogWarning($"Mod Browser: {e.Message}");
            ShowResultsMessage(steam_error, true);
        }
        catch (Exception e)
        {
            steam_ready = false;
            steam_error = LM.Get("mod_browser_error_query");
            LogService.LogError($"Mod Browser failed to load: {e.Message}");
            LogService.LogException(e);
            ShowResultsMessage(steam_error, true);
        }
        finally
        {
            steam_check_running = false;
            RefreshAll();
        }
    }

    private void OnSessionChanged()
    {
        if (session == null) return;
        if (ItemsChanged()) RebuildList();

        if (session.Items.Count == 0)
        {
            if (session.IsLoading) ShowResultsMessage(LM.Get("mod_browser_status_loading"), false);
            else if (session.LastError != null) ShowResultsMessage(ErrorText(session.LastError), true);
            else ShowResultsMessage(LM.Get("mod_browser_empty"), false);
        }
        else
        {
            HideResultsMessage();
        }

        RefreshButtons();
        UpdateStatusText();
    }

    private bool ItemsChanged()
    {
        if (session.Items.Count != active_rows.Count) return true;
        for (int i = 0; i < active_rows.Count; i++)
        {
            if (!ReferenceEquals(active_rows[i].Item, session.Items[i])) return true;
        }

        return false;
    }

    private void RebuildList()
    {
        row_pool.clear();
        active_rows.Clear();
        foreach (WorkshopModInfo item in session.Items)
        {
            ModBrowserListItem row = row_pool.getNext();
            row.transform.localScale = Vector3.one;
            row.Setup(item, Select);
            row.SetSelected(selected != null && selected.FileId == item.FileId);
            row.RefreshMeta(ModBrowserService.GetStatus(item.FileId));
            active_rows.Add(row);
        }
    }

    private void ApplyQueryChange()
    {
        ModBrowserService.Settings.SetQuery(query);
        ModBrowserService.SaveSettings();
        SyncToolbar();
        results_scroll.verticalNormalizedPosition = 1;
        if (steam_ready && session != null)
        {
            _ = session.ResetAsync(query);
        }
        else
        {
            EnsureSteamAndLoad(true);
        }
    }

    #endregion

    #region Toolbar actions

    private void OnSearchSubmitted(string pText)
    {
        string text = (pText ?? "").Trim();
        if (text == query.SearchText) return;
        query.SearchText = text;
        if (text.Length == 0 && query.Sort == WorkshopBrowseSort.Relevance) query.Sort = WorkshopBrowseSort.MostSubscribed;
        ApplyQueryChange();
    }

    private void ClearSearch()
    {
        search_input.input.text = "";
        OnSearchSubmitted("");
    }

    private void CycleSort()
    {
        int index = Array.IndexOf(sort_cycle, query.Sort);
        do
        {
            index = (index + 1) % sort_cycle.Length;
        } while (sort_cycle[index] == WorkshopBrowseSort.Relevance && string.IsNullOrWhiteSpace(query.SearchText));

        query.Sort = sort_cycle[index];
        ApplyQueryChange();
    }

    private void ToggleTag(bool pVerified)
    {
        if (pVerified)
        {
            query.IncludeVerified = !query.IncludeVerified;
            if (!query.IncludeVerified && !query.IncludeUnverified) query.IncludeUnverified = true;
        }
        else
        {
            query.IncludeUnverified = !query.IncludeUnverified;
            if (!query.IncludeVerified && !query.IncludeUnverified) query.IncludeVerified = true;
        }

        ApplyQueryChange();
    }

    private void ToggleScope()
    {
        query.Scope = query.Scope == WorkshopBrowseScope.All ? WorkshopBrowseScope.Subscribed : WorkshopBrowseScope.All;
        ApplyQueryChange();
    }

    private void RefreshList()
    {
        ModBrowserService.InvalidateLocalCaches();
        if (steam_ready && session != null)
        {
            _ = session.ResetAsync(query);
        }
        else
        {
            EnsureSteamAndLoad(true);
        }
    }

    private void SyncToolbar()
    {
        if (!search_input.input.isFocused) search_input.input.text = query.SearchText;
        sort_button.Text.text = LM.Get(SortKey(query.Sort));
        verified_button.Background.color = query.IncludeVerified ? Color.white : Color.gray;
        unverified_button.Background.color = query.IncludeUnverified ? Color.white : Color.gray;
        scope_button.Text.text = LM.Get(query.Scope == WorkshopBrowseScope.All
            ? "mod_browser_scope_all"
            : "mod_browser_scope_subscribed");
    }

    private static string SortKey(WorkshopBrowseSort pSort)
    {
        return pSort switch
        {
            WorkshopBrowseSort.Trending => "mod_browser_sort_trending",
            WorkshopBrowseSort.TopRated => "mod_browser_sort_top_rated",
            WorkshopBrowseSort.Newest => "mod_browser_sort_newest",
            WorkshopBrowseSort.RecentlyUpdated => "mod_browser_sort_recently_updated",
            WorkshopBrowseSort.Name => "mod_browser_sort_name",
            WorkshopBrowseSort.Relevance => "mod_browser_sort_relevance",
            _ => "mod_browser_sort_most_subscribed"
        };
    }

    #endregion

    #region Selection and details

    private async void Select(WorkshopModInfo pItem)
    {
        selected = pItem;
        selected_details = null;
        details_description_source = null;
        details_description_scroll.verticalNormalizedPosition = 1;
        foreach (ModBrowserListItem row in active_rows) row.SetSelected(row.Item.FileId == pItem.FileId);
        ulong requested = pItem.FileId;
        WorkshopThumbnailLoader.Request(pItem.FileId, pItem.PreviewUrl, (file_id, sprite) =>
        {
            if (selected != null && selected.FileId == requested && file_id == requested) details_icon.sprite = sprite;
        });
        RefreshDetails();
        if (pItem.IsLongDescription || !steam_ready) return;

        try
        {
            WorkshopModInfo details = await ModBrowserService.GetDetailsAsync(pItem);
            if (selected == null || selected.FileId != requested) return;
            selected_details = details;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot load the description of Workshop item {requested}: {e.Message}");
            // Keep the short description and stop showing "loading".
            if (selected != null && selected.FileId == requested) selected_details = pItem;
        }

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (selected == null)
        {
            details_content.SetActive(false);
            details_hint.gameObject.SetActive(true);
            return;
        }

        details_content.SetActive(true);
        details_hint.gameObject.SetActive(false);
        WorkshopModInfo info = selected_details ?? selected;
        ModBrowserStatus status = ModBrowserService.GetStatus(info.FileId);
        ModDeclare mod = status.InstalledMod;

        details_title.text = ModBrowserText.Truncate(info.Title, 60);
        details_author.text = string.Format(LM.Get("mod_browser_by"), ModBrowserText.Truncate(ModBrowserText.Author(info), 30)) +
                              " | " + string.Format(LM.Get("mod_browser_workshop_id"), info.FileId);
        string unverified = info.IsVerified
            ? ""
            : $"  <color=#{ColorUtility.ToHtmlStringRGB(ModBrowserText.WarningColor)}>{LM.Get("mod_browser_unverified")}</color>";
        details_state.text = ModBrowserText.StatusBadge(status) + unverified;
        details_stats.text = string.Format(LM.Get("mod_browser_subscribers"), ModBrowserText.FormatCount(info.Subscriptions)) +
                             " | " + ModBrowserText.Rating(info) + " | " + ModBrowserText.FormatSize(info.FileSizeBytes);
        details_dates.text = string.Format(LM.Get("mod_browser_created"), ModBrowserText.FormatDate(info.Created)) + " | " +
                             string.Format(LM.Get("mod_browser_updated"), ModBrowserText.FormatDate(info.Updated)) +
                             (string.IsNullOrEmpty(mod?.Version)
                                 ? ""
                                 : " | " + string.Format(LM.Get("mod_browser_version"), ModBrowserText.Truncate(mod.Version, 20)));
        details_dependencies.text = DependencyText(status);

        string description = ModBrowserText.Description(info);
        if (selected_details == null && !info.IsLongDescription && steam_ready)
        {
            description += "\n\n" + LM.Get("mod_browser_loading_description");
        }
        if (!string.IsNullOrEmpty(status.Detail))
        {
            description = string.Format(LM.Get("mod_browser_problem"), status.Detail) + "\n\n" + description;
        }

        if (description.Length > max_description_length) description = description.Substring(0, max_description_length) + "...";
        if (description != details_description_source)
        {
            details_description_source = description;
            details_description.text = description;
        }

        RefreshDetailButtons(info, status);
    }

    private static string DependencyText(ModBrowserStatus pStatus)
    {
        ModDeclare mod = pStatus.InstalledMod;
        if (mod == null) return LM.Get("mod_browser_dependencies_unknown");
        if (mod.Dependencies.Length == 0) return LM.Get("mod_browser_dependencies_none");

        // UIDs only contain letters, digits, '_' and non-ASCII characters, so they are safe in rich text.
        string missing_color = ColorUtility.ToHtmlStringRGB(ModBrowserText.StatusColor(ModBrowserStatusKind.Failed));
        string ok_color = ColorUtility.ToHtmlStringRGB(ModBrowserText.StatusColor(ModBrowserStatusKind.Enabled));
        IEnumerable<string> parts = mod.Dependencies.Select(uid => pStatus.MissingDependencies.Contains(uid)
            ? $"<color=#{missing_color}>{uid} ({LM.Get("mod_browser_missing")})</color>"
            : $"<color=#{ok_color}>{uid}</color>");
        return string.Format(LM.Get("mod_browser_dependencies"), string.Join(", ", parts));
    }

    private void RefreshDetailButtons(WorkshopModInfo pInfo, ModBrowserStatus pStatus)
    {
        WorkshopItemLocalState local = pStatus.Local;
        ModDeclare mod = pStatus.RecognizedMod ?? pStatus.InstalledMod;

        // Install / Update / Retry / Cancel
        if (pStatus.IsBusy)
        {
            SetupButton(primary_button, "mod_browser_cancel", () => ModBrowserService.Queue.Cancel(pInfo.FileId));
        }
        else if (pStatus.Kind == ModBrowserStatusKind.UpdateAvailable)
        {
            SetupButton(primary_button, "mod_browser_update", () => Install(pInfo));
        }
        else if (pStatus.Kind == ModBrowserStatusKind.Failed)
        {
            SetupButton(primary_button, "mod_browser_retry", () => Install(pInfo));
        }
        else if (!local.Installed || !local.Subscribed)
        {
            SetupButton(primary_button, "mod_browser_install", () => Install(pInfo));
        }
        else
        {
            primary_button.gameObject.SetActive(false);
        }

        if (primary_button.gameObject.activeSelf) SetInteractable(primary_button, steam_ready);

        bool can_toggle = mod != null && local.Installed && local.Subscribed && !pStatus.IsBusy &&
                          pStatus.Kind is not (ModBrowserStatusKind.Incompatible or ModBrowserStatusKind.InstalledLocalCopy
                              or ModBrowserStatusKind.UninstallPending or ModBrowserStatusKind.Failed);
        if (can_toggle)
        {
            // BepInEx mods are never "recognized" by NML; their record alone decides whether startup links them.
            bool wanted = (pStatus.RecognizedMod != null || mod.ModType == ModTypeEnum.BEPINEX) &&
                          !ModInfoUtils.isModDisabled(mod.UID);
            ModDeclare target = mod;
            if (wanted)
            {
                SetupButton(toggle_button, "mod_browser_disable", () =>
                {
                    ModBrowserService.DisableMod(target);
                    RefreshAll();
                });
            }
            else
            {
                SetupButton(toggle_button, "mod_browser_enable", () =>
                {
                    ModBrowserService.EnableMod(target);
                    RefreshAll();
                });
            }
        }
        else
        {
            toggle_button.gameObject.SetActive(false);
        }

        if (local.Subscribed && !pStatus.IsBusy)
        {
            SetupButton(uninstall_button, "mod_browser_uninstall", () => Uninstall(pInfo));
            SetInteractable(uninstall_button, steam_ready);
        }
        else
        {
            uninstall_button.gameObject.SetActive(false);
        }

        if (pStatus.MissingDependencies.Count > 0 && !pStatus.IsBusy)
        {
            List<string> missing = pStatus.MissingDependencies.ToList();
            SetupButton(dependencies_button, "mod_browser_find_dependencies", () => FindDependencies(missing));
            SetInteractable(dependencies_button, steam_ready && !busy);
        }
        else
        {
            dependencies_button.gameObject.SetActive(false);
        }

        SetupButton(steam_page_button, "mod_browser_steam_page", () => Application.OpenURL(pInfo.Url));
        SetupButton(changelog_button, "mod_browser_changelog", () => Application.OpenURL(pInfo.ChangelogUrl));
        if (local.Installed && !string.IsNullOrEmpty(local.Directory))
        {
            string directory = local.Directory;
            SetupButton(folder_button, "mod_browser_open_folder", () => Application.OpenURL(directory));
        }
        else
        {
            folder_button.gameObject.SetActive(false);
        }

        if (pStatus.Kind == ModBrowserStatusKind.RestartRequired && pStatus.RecognizedMod != null &&
            ModReloadService.CanReload(pStatus.RecognizedMod) && !ModInfoUtils.isModDisabled(pStatus.RecognizedMod.UID))
        {
            ModDeclare target = pStatus.RecognizedMod;
            SetupButton(reload_button, "mod_browser_reload", () =>
            {
                if (!ModBrowserService.ReloadMod(target))
                {
                    ShowInfo(LM.Get("mod_browser_reload"), string.Format(LM.Get("mod_browser_reload_failed"), target.Name));
                }

                RefreshAll();
            });
        }
        else
        {
            reload_button.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Mod actions

    private void Install(WorkshopModInfo pItem)
    {
        if (pItem.IsVerified)
        {
            ModBrowserService.Enqueue(pItem, false);
            RefreshAll();
            return;
        }

        ShowDialog(LM.Get("mod_browser_install_unverified_title"),
            string.Format(LM.Get("mod_browser_install_unverified_text"), ModBrowserText.Truncate(pItem.Title, 60)),
            LM.Get("mod_browser_install"), () =>
            {
                ModBrowserService.Enqueue(pItem, false);
                RefreshAll();
            }, LM.Get("mod_browser_dialog_cancel"));
    }

    private void Uninstall(WorkshopModInfo pItem)
    {
        ShowDialog(LM.Get("mod_browser_uninstall"),
            string.Format(LM.Get("mod_browser_uninstall_text"), ModBrowserText.Truncate(pItem.Title, 60)),
            LM.Get("mod_browser_uninstall"), async () =>
            {
                try
                {
                    if (!await ModBrowserService.UninstallAsync(pItem))
                    {
                        ShowInfo(LM.Get("mod_browser_uninstall"), LM.Get("mod_browser_uninstall_failed"));
                    }
                }
                catch (Exception e)
                {
                    LogService.LogError($"Failed to uninstall Workshop item {pItem.FileId}: {e.Message}");
                    LogService.LogException(e);
                    ShowInfo(LM.Get("mod_browser_uninstall"), ErrorText(e));
                }

                RefreshAll();
            }, LM.Get("mod_browser_dialog_cancel"));
    }

    private async void DownloadAll()
    {
        if (busy || session == null) return;
        SetBusy(LM.Get("mod_browser_loading_list"));
        try
        {
            // Let a page that is already loading finish first.
            while (session.IsLoading) await Task.Delay(100);

            // "All" means every match of the current filters, not only the pages scrolled to so far.
            while (session.Query.IsServerPaged && session.HasMorePages &&
                   session.LoadedPages < WorkshopBrowseSession.MaxFetchAllPages)
            {
                int loaded = session.LoadedPages;
                await session.LoadNextPageAsync();
                if (session.LastError != null || session.LoadedPages == loaded) break;
            }

            if (session.LastError != null)
            {
                ShowInfo(LM.Get("mod_browser_download_all"), ErrorText(session.LastError));
                return;
            }

            List<WorkshopModInfo> candidates = ModBrowserService.GetInstallCandidates(session.Items);
            if (candidates.Count == 0)
            {
                ShowInfo(LM.Get("mod_browser_download_all"), LM.Get("mod_browser_nothing_to_download"));
                return;
            }

            string message = string.Format(LM.Get("mod_browser_download_all_text"), candidates.Count,
                TotalSize(candidates), candidates.Count(item => !item.IsVerified));
            ShowDialog(LM.Get("mod_browser_download_all"), message, LM.Get("mod_browser_install"), () =>
                {
                    ModBrowserService.EnqueueAll(candidates, dialog_switch_value);
                    RefreshAll();
                }, LM.Get("mod_browser_dialog_cancel"), null, LM.Get("mod_browser_enable_after_install"),
                false);
        }
        catch (Exception e)
        {
            LogService.LogError($"Download all failed: {e.Message}");
            LogService.LogException(e);
            ShowInfo(LM.Get("mod_browser_download_all"), ErrorText(e));
        }
        finally
        {
            ClearBusy();
        }
    }

    private async void UpdateAll()
    {
        if (busy) return;
        SetBusy(LM.Get("mod_browser_checking_updates"));
        try
        {
            List<WorkshopModInfo> updates = await ModBrowserService.FindUpdatesAsync();
            if (updates.Count == 0)
            {
                ShowInfo(LM.Get("mod_browser_update_all"), LM.Get("mod_browser_no_updates"));
                return;
            }

            ShowDialog(LM.Get("mod_browser_update_all"),
                string.Format(LM.Get("mod_browser_update_all_text"), updates.Count, TotalSize(updates)),
                LM.Get("mod_browser_update"), () =>
                {
                    ModBrowserService.EnqueueAll(updates, false);
                    RefreshAll();
                }, LM.Get("mod_browser_dialog_cancel"));
        }
        catch (Exception e)
        {
            LogService.LogError($"Update check failed: {e.Message}");
            if (e is not WorkshopException) LogService.LogException(e);
            ShowInfo(LM.Get("mod_browser_update_all"), ErrorText(e));
        }
        finally
        {
            ClearBusy();
        }
    }

    private async void FindDependencies(List<string> pMissing)
    {
        if (busy) return;
        SetBusy(LM.Get("mod_browser_deps_searching"));
        try
        {
            List<ModBrowserDependencyMatch> matches = await ModBrowserService.FindDependencyCandidatesAsync(pMissing);
            List<ModBrowserDependencyMatch> found = matches.Where(match => match.Candidate != null).ToList();
            string lines = string.Join("\n", matches.Select(match => match.Candidate != null
                ? string.Format(LM.Get("mod_browser_deps_found"), match.Uid, ModBrowserText.Truncate(match.Candidate.Title, 40))
                : string.Format(LM.Get("mod_browser_deps_not_found"), match.Uid)));
            if (found.Count == 0)
            {
                ShowInfo(LM.Get("mod_browser_deps_title"), lines + "\n\n" + LM.Get("mod_browser_deps_none_found"));
                return;
            }

            ShowDialog(LM.Get("mod_browser_deps_title"), lines + "\n\n" + LM.Get("mod_browser_deps_install_hint"),
                LM.Get("mod_browser_deps_install"), () =>
                {
                    ModBrowserService.EnqueueDependencies(found);
                    RefreshAll();
                }, LM.Get("mod_browser_dialog_cancel"));
        }
        catch (Exception e)
        {
            LogService.LogError($"Dependency search failed: {e.Message}");
            if (e is not WorkshopException) LogService.LogException(e);
            ShowInfo(LM.Get("mod_browser_deps_title"), ErrorText(e));
        }
        finally
        {
            ClearBusy();
        }
    }

    private void Apply()
    {
        int count = ModBrowserService.PendingApplyCount;
        if (count == 0) return;
        ShowDialog(LM.Get("mod_browser_apply"), string.Format(LM.Get("mod_browser_apply_text"), count),
            LM.Get("mod_browser_apply"), () =>
            {
                ModBrowserApplyResult result = ModBrowserService.ApplyPendingMods();
                var sb = new StringBuilder(string.Format(LM.Get("mod_browser_apply_result"), result.Loaded.Count,
                    result.Failed.Count));
                foreach (KeyValuePair<ModDeclare, string> failure in result.Failed.Take(5))
                {
                    sb.Append("\n- ").Append(ModBrowserText.Truncate(failure.Key.Name, 30)).Append(": ")
                        .Append(ModBrowserText.Truncate(failure.Value, 100));
                }

                RefreshAll();
                ShowInfo(LM.Get("mod_browser_apply"), sb.ToString());
            }, LM.Get("mod_browser_dialog_cancel"));
    }

    private void OnQueueFinished(WorkshopQueueSummary pSummary)
    {
        RefreshAll();
        if (pSummary.Entries.Count <= 1 && pSummary.Failed == 0) return;

        var sb = new StringBuilder(string.Format(LM.Get("mod_browser_summary_text"), pSummary.Completed, pSummary.Failed,
            pSummary.Cancelled));
        List<WorkshopQueueEntry> failures = pSummary.Entries.Where(entry => entry.State == WorkshopQueueState.Failed)
            .ToList();
        if (failures.Count > 0)
        {
            sb.Append("\n").Append(LM.Get("mod_browser_summary_failures"));
            foreach (WorkshopQueueEntry failure in failures.Take(5))
            {
                sb.Append("\n- ").Append(ModBrowserText.Truncate(failure.Title, 30)).Append(": ")
                    .Append(ModBrowserText.Truncate(failure.Error, 90));
            }

            if (failures.Count > 5) sb.Append("\n...");
        }

        foreach (string note in pSummary.Entries.Where(entry => entry.State == WorkshopQueueState.Completed)
                     .Select(entry => entry.Note).Where(note => !string.IsNullOrEmpty(note)).Distinct())
        {
            sb.Append("\n").Append(note);
        }

        int missing_dependencies = pSummary.Entries.Count(entry => entry.State == WorkshopQueueState.Completed &&
                                                                   ModBrowserService.GetStatus(entry.FileId).Kind ==
                                                                   ModBrowserStatusKind.MissingDependency);
        if (missing_dependencies > 0) sb.Append("\n").Append(LM.Get("mod_browser_summary_missing_deps"));

        ShowInfo(LM.Get("mod_browser_summary_title"), sb.ToString());
    }

    private static string TotalSize(List<WorkshopModInfo> pItems)
    {
        long known = pItems.Where(item => item.FileSizeBytes > 0).Sum(item => item.FileSizeBytes);
        int unknown = pItems.Count(item => item.FileSizeBytes <= 0);
        string size = ModBrowserText.FormatSize(known);
        return unknown == 0 ? size : size + " " + string.Format(LM.Get("mod_browser_size_partial"), unknown);
    }

    private static string ErrorText(Exception pException)
    {
        if (pException is WorkshopException workshop_exception)
        {
            return LM.Get(workshop_exception.Kind switch
            {
                WorkshopErrorKind.SteamUnavailable => "mod_browser_error_steam_unavailable",
                WorkshopErrorKind.Offline => "mod_browser_error_offline",
                _ => "mod_browser_error_query"
            });
        }

        return LM.Get("mod_browser_error_query") + "\n" + pException.Message;
    }

    #endregion

    #region Refresh

    private void OnItemChanged(ulong pFileId)
    {
        foreach (ModBrowserListItem row in active_rows)
        {
            if (row.Item.FileId == pFileId) row.RefreshMeta(ModBrowserService.GetStatus(pFileId));
        }

        if (selected != null && selected.FileId == pFileId) RefreshDetails();
        RefreshButtons();
        UpdateStatusText();
    }

    private void OnOwnerNameResolved(ulong pSteamId)
    {
        meta_dirty = true;
    }

    private void RefreshAll()
    {
        foreach (ModBrowserListItem row in active_rows) row.RefreshMeta(ModBrowserService.GetStatus(row.Item.FileId));
        RefreshDetails();
        RefreshButtons();
        UpdateStatusText();
    }

    private void RefreshButtons()
    {
        bool queue_running = ModBrowserService.IsQueueRunning;
        bool has_more = steam_ready && session != null && session.Query.IsServerPaged && session.HasMorePages;
        more_button.gameObject.SetActive(has_more);
        if (has_more) SetInteractable(more_button, !session.IsLoading);
        SetInteractable(download_all_button, steam_ready && !busy && session != null && session.Items.Count > 0);
        SetInteractable(update_all_button, steam_ready && !busy);
        cancel_button.gameObject.SetActive(queue_running);
        int pending = ModBrowserService.PendingApplyCount;
        apply_button.Text.text = pending > 0
            ? string.Format(LM.Get("mod_browser_apply_count"), pending)
            : LM.Get("mod_browser_apply");
        SetInteractable(apply_button, pending > 0 && !queue_running && !busy);
        SetInteractable(refresh_button, !busy);
    }

    private void UpdateStatusText()
    {
        WorkshopQueueEntry current = ModBrowserService.IsQueueRunning ? ModBrowserService.Queue.Current : null;
        progress_bar.SetActive(current != null);
        if (current != null)
        {
            int queued = ModBrowserService.Queue.Entries.Count(entry => entry.State == WorkshopQueueState.Queued);
            status_text.text = string.Format(LM.Get("mod_browser_status_queue"),
                ModBrowserText.Truncate(current.Title, 22), Mathf.RoundToInt(current.Progress * 100), queued);
            progress_fill.sizeDelta = new Vector2(68 * Mathf.Clamp01(current.Progress), 4);
            return;
        }

        if (busy)
        {
            status_text.text = busy_text;
            return;
        }

        if (!steam_ready || session == null)
        {
            status_text.text = steam_error ?? "";
            return;
        }

        if (session.IsLoading)
        {
            status_text.text = session.LoadedPages == 0
                ? LM.Get("mod_browser_status_loading")
                : string.Format(LM.Get("mod_browser_status_loading_page"), session.LoadedPages + 1);
        }
        else if (session.LastError != null)
        {
            status_text.text = ErrorText(session.LastError);
        }
        else
        {
            string text = string.Format(LM.Get("mod_browser_status_showing"), session.Items.Count,
                Math.Max(session.TotalCount, session.Items.Count));
            if (session.ReachedPageCap) text += " " + LM.Get("mod_browser_status_page_cap");
            status_text.text = text;
        }
    }

    private void SetBusy(string pText)
    {
        busy = true;
        busy_text = pText;
        RefreshButtons();
        UpdateStatusText();
    }

    private void ClearBusy()
    {
        busy = false;
        busy_text = null;
        RefreshAll();
    }

    private void ShowResultsMessage(string pText, bool pShowRetry)
    {
        results_message.SetActive(true);
        results_message_text.text = pText;
        results_retry_button.gameObject.SetActive(pShowRetry);
    }

    private void HideResultsMessage()
    {
        results_message.SetActive(false);
    }

    private void RefreshTexts()
    {
        search_button.Text.text = LM.Get("mod_browser_search");
        verified_button.Text.text = LM.Get("mod_browser_tag_verified");
        unverified_button.Text.text = LM.Get("mod_browser_tag_unverified");
        more_button.Text.text = LM.Get("mod_browser_more");
        download_all_button.Text.text = LM.Get("mod_browser_download_all");
        update_all_button.Text.text = LM.Get("mod_browser_update_all");
        cancel_button.Text.text = LM.Get("mod_browser_cancel");
        results_retry_button.Text.text = LM.Get("mod_browser_retry");
        details_hint.text = LM.Get("mod_browser_select_hint");
    }

    #endregion

    #region Dialog

    private void ShowWarning()
    {
        ShowDialog(LM.Get("mod_browser_warning_title"), LM.Get("mod_browser_warning_text"),
            LM.Get("mod_browser_warning_accept"), () =>
            {
                ModBrowserService.Settings.WarningAccepted = true;
                ModBrowserService.SaveSettings();
                EnsureSteamAndLoad(false);
            }, LM.Get("mod_browser_dialog_cancel"), () => window.ShowInstalled());
    }

    private void ShowInfo(string pTitle, string pMessage)
    {
        ShowDialog(pTitle, pMessage, LM.Get("mod_browser_dialog_ok"), null);
    }

    /// <summary>
    ///     Modal dialog over the panel. The message is plain text, so Workshop titles inside it cannot add markup.
    /// </summary>
    private void ShowDialog(string pTitle, string pMessage, string pConfirmLabel, Action pOnConfirm,
        string pCancelLabel = null, Action pOnCancel = null, string pSwitchLabel = null, bool pSwitchValue = false)
    {
        if (dialog.activeSelf)
        {
            // Never replace an open dialog (it may be waiting for a confirmation); show this one after it.
            queued_dialogs.Enqueue(() => ShowDialog(pTitle, pMessage, pConfirmLabel, pOnConfirm, pCancelLabel,
                pOnCancel, pSwitchLabel, pSwitchValue));
            return;
        }

        dialog_title.text = pTitle;
        dialog_message.text = pMessage;
        dialog_on_confirm = pOnConfirm;
        dialog_on_cancel = pOnCancel;
        dialog_confirm.Text.text = pConfirmLabel;
        dialog_cancel.gameObject.SetActive(pCancelLabel != null);
        if (pCancelLabel != null) dialog_cancel.Text.text = pCancelLabel;

        dialog_switch_row.SetActive(pSwitchLabel != null);
        dialog_switch_value = pSwitchValue;
        if (pSwitchLabel != null)
        {
            dialog_switch_label.text = pSwitchLabel;
            dialog_switch.Setup(pSwitchValue, () => dialog_switch_value = !dialog_switch_value);
        }

        dialog.SetActive(true);
        dialog.transform.SetAsLastSibling();
    }

    private void CloseDialog(bool pConfirmed)
    {
        Action callback = pConfirmed ? dialog_on_confirm : dialog_on_cancel;
        dialog_on_confirm = null;
        dialog_on_cancel = null;
        dialog.SetActive(false);
        try
        {
            callback?.Invoke();
        }
        catch (Exception e)
        {
            LogService.LogError($"Mod Browser dialog action failed: {e.Message}");
            LogService.LogException(e);
        }

        if (!dialog.activeSelf && queued_dialogs.Count > 0) queued_dialogs.Dequeue()();
    }

    #endregion

    #region Build

    private void Build()
    {
        BuildToolbar();
        BuildResults();
        BuildDetails();
        BuildBottomBar();
        BuildDialog();
    }

    private void BuildToolbar()
    {
        Transform toolbar = CreateRow(transform, "Toolbar", new Vector2(Size.x, 20), new Vector2(0, 115),
            TextAnchor.MiddleLeft, 3);

        search_input = Instantiate(TextInput.Prefab, toolbar);
        search_input.name = "Search";
        search_input.transform.localScale = Vector3.one;
        search_input.Setup(query.SearchText, OnSearchSubmitted);
        search_input.SetSize(new Vector2(140, 16));
        search_input.tip_button.enabled = false;

        clear_search_button = CreateTextButton(toolbar, "ClearSearch", new Vector2(16, 16), ClearSearch);
        clear_search_button.Text.text = "X";
        search_button = CreateTextButton(toolbar, "SearchButton", new Vector2(36, 16),
            () => OnSearchSubmitted(search_input.input.text));
        sort_button = CreateTextButton(toolbar, "Sort", new Vector2(96, 16), CycleSort);
        verified_button = CreateTextButton(toolbar, "Verified", new Vector2(42, 16), () => ToggleTag(true));
        unverified_button = CreateTextButton(toolbar, "Unverified", new Vector2(58, 16), () => ToggleTag(false));
        scope_button = CreateTextButton(toolbar, "Scope", new Vector2(60, 16), ToggleScope);

        refresh_button = Instantiate(SimpleButton.Prefab, toolbar);
        refresh_button.name = "Refresh";
        refresh_button.transform.localScale = Vector3.one;
        refresh_button.Setup(RefreshList, InternalResourcesGetter.GetReloadIcon(), pSize: new Vector2(16, 16));
    }

    private void BuildResults()
    {
        results_scroll = CreateScrollView(transform, "Results", new Vector2(256, 207), new Vector2(-133, -0.5f),
            out results_content, true);
        Image background = results_scroll.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/windowEmptyFrame");
        row_pool = new ObjectPoolGenericMono<ModBrowserListItem>(ModBrowserListItem.Prefab, results_content);

        results_message = new GameObject("Message", typeof(RectTransform));
        results_message.transform.SetParent(results_scroll.transform, false);
        results_message.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 90);
        results_message_text = ModBrowserText.CreateText(results_message.transform, "Text", new Vector2(230, 56),
            new Vector2(0, 14), 7, TextAnchor.MiddleCenter);
        results_retry_button = CreateTextButton(results_message.transform, "Retry", new Vector2(60, 16),
            () => EnsureSteamAndLoad(true));
        results_retry_button.transform.localPosition = new Vector3(0, -28);
        results_message.SetActive(false);
    }

    private void BuildDetails()
    {
        var details = new GameObject("Details", typeof(RectTransform), typeof(Image));
        details.transform.SetParent(transform, false);
        details.transform.localPosition = new Vector3(130.5f, -0.5f);
        details.GetComponent<RectTransform>().sizeDelta = new Vector2(261, 207);
        var background = details.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        background.type = Image.Type.Sliced;

        details_hint = ModBrowserText.CreateText(details.transform, "Hint", new Vector2(230, 40), Vector2.zero, 7,
            TextAnchor.MiddleCenter);

        details_content = new GameObject("Content", typeof(RectTransform));
        details_content.transform.SetParent(details.transform, false);
        details_content.GetComponent<RectTransform>().sizeDelta = new Vector2(261, 207);
        Transform root = details_content.transform;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(root, false);
        icon.transform.localPosition = new Vector3(-104.5f, 77.5f);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        details_icon = icon.GetComponent<Image>();
        details_icon.preserveAspect = true;
        details_icon.raycastTarget = false;
        var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(icon.transform, false);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(44, 44);
        var frame_image = frame.GetComponent<Image>();
        frame_image.sprite = InternalResourcesGetter.GetIconFrame();
        frame_image.type = Image.Type.Sliced;
        frame_image.raycastTarget = false;

        details_title = ModBrowserText.CreateText(root, "Title", new Vector2(206, 14), new Vector2(23, 93), 9,
            TextAnchor.UpperLeft);
        details_author = ModBrowserText.CreateText(root, "Author", new Vector2(206, 10), new Vector2(23, 80), 6,
            TextAnchor.UpperLeft);
        details_author.color = new Color(0.85f, 0.85f, 0.85f);
        details_state = ModBrowserText.CreateText(root, "State", new Vector2(206, 11), new Vector2(23, 68.5f), 7,
            TextAnchor.UpperLeft, true);
        details_stats = ModBrowserText.CreateText(root, "Stats", new Vector2(253, 10), new Vector2(0, 50), 6,
            TextAnchor.UpperLeft);
        details_dates = ModBrowserText.CreateText(root, "Dates", new Vector2(253, 10), new Vector2(0, 40), 6,
            TextAnchor.UpperLeft);
        details_dependencies = ModBrowserText.CreateText(root, "Dependencies", new Vector2(253, 18),
            new Vector2(0, 26), 6, TextAnchor.UpperLeft, true);

        details_description_scroll = CreateScrollView(root, "Description", new Vector2(253, 84),
            new Vector2(0, -26), out RectTransform description_content, false);
        details_description_scroll.GetComponent<Image>().color = new Color(1, 1, 1, 0.35f);
        description_content.sizeDelta = new Vector2(-8, 0);
        description_content.anchoredPosition = new Vector2(0, -3);
        details_description = description_content.gameObject.AddComponent<Text>();
        details_description.font = LocalizedTextManager.current_font;
        details_description.fontSize = 6;
        details_description.alignment = TextAnchor.UpperLeft;
        details_description.supportRichText = false;
        details_description.horizontalOverflow = HorizontalWrapMode.Wrap;
        details_description.verticalOverflow = VerticalWrapMode.Overflow;
        details_description.raycastTarget = false;

        Transform first_row = CreateRow(root, "Actions", new Vector2(253, 15), new Vector2(0, -77),
            TextAnchor.MiddleCenter, 3);
        primary_button = CreateTextButton(first_row, "Primary", new Vector2(60, 14), null);
        toggle_button = CreateTextButton(first_row, "Toggle", new Vector2(60, 14), null);
        uninstall_button = CreateTextButton(first_row, "Uninstall", new Vector2(60, 14), null);
        dependencies_button = CreateTextButton(first_row, "Dependencies", new Vector2(60, 14), null);

        Transform second_row = CreateRow(root, "Links", new Vector2(253, 15), new Vector2(0, -94),
            TextAnchor.MiddleCenter, 3);
        steam_page_button = CreateTextButton(second_row, "SteamPage", new Vector2(60, 14), null);
        changelog_button = CreateTextButton(second_row, "Changelog", new Vector2(60, 14), null);
        folder_button = CreateTextButton(second_row, "Folder", new Vector2(60, 14), null);
        reload_button = CreateTextButton(second_row, "Reload", new Vector2(60, 14), null);

        details_content.SetActive(false);
    }

    private void BuildBottomBar()
    {
        Transform bar = CreateRow(transform, "BottomBar", new Vector2(Size.x, 20), new Vector2(0, -116),
            TextAnchor.MiddleLeft, 3);
        status_text = ModBrowserText.CreateText(bar, "Status", new Vector2(190, 18), Vector2.zero, 6,
            TextAnchor.MiddleLeft);

        progress_bar = new GameObject("Progress", typeof(RectTransform), typeof(Image));
        progress_bar.transform.SetParent(bar, false);
        progress_bar.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 6);
        var progress_background = progress_bar.GetComponent<Image>();
        progress_background.color = new Color(0, 0, 0, 0.5f);
        progress_background.raycastTarget = false;
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(progress_bar.transform, false);
        progress_fill = fill.GetComponent<RectTransform>();
        progress_fill.anchorMin = new Vector2(0, 0.5f);
        progress_fill.anchorMax = new Vector2(0, 0.5f);
        progress_fill.pivot = new Vector2(0, 0.5f);
        progress_fill.anchoredPosition = new Vector2(1, 0);
        progress_fill.sizeDelta = new Vector2(0, 4);
        var fill_image = fill.GetComponent<Image>();
        fill_image.color = ModBrowserText.ProgressColor;
        fill_image.raycastTarget = false;
        progress_bar.SetActive(false);

        more_button = CreateTextButton(bar, "More", new Vector2(34, 16), () =>
        {
            if (session != null) _ = session.LoadNextPageAsync();
        });
        download_all_button = CreateTextButton(bar, "DownloadAll", new Vector2(58, 16), DownloadAll);
        update_all_button = CreateTextButton(bar, "UpdateAll", new Vector2(52, 16), UpdateAll);
        cancel_button = CreateTextButton(bar, "Cancel", new Vector2(40, 16),
            () => ModBrowserService.Queue.CancelAll());
        apply_button = CreateTextButton(bar, "Apply", new Vector2(46, 16), Apply);
    }

    private void BuildDialog()
    {
        dialog = new GameObject("Dialog", typeof(RectTransform), typeof(Image));
        dialog.transform.SetParent(transform, false);
        dialog.GetComponent<RectTransform>().sizeDelta = Size;
        dialog.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);

        var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
        box.transform.SetParent(dialog.transform, false);
        box.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 170);
        var box_image = box.GetComponent<Image>();
        box_image.sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        box_image.type = Image.Type.Sliced;

        dialog_title = ModBrowserText.CreateText(box.transform, "Title", new Vector2(280, 16), new Vector2(0, 70), 9,
            TextAnchor.MiddleCenter);
        dialog_message = ModBrowserText.CreateText(box.transform, "Message", new Vector2(280, 92),
            new Vector2(0, 12), 7, TextAnchor.UpperLeft);
        dialog_message.resizeTextForBestFit = true;
        dialog_message.resizeTextMinSize = 4;
        dialog_message.resizeTextMaxSize = 7;

        dialog_switch_row = CreateRow(box.transform, "Switch", new Vector2(280, 18), new Vector2(0, -45),
            TextAnchor.MiddleLeft, 6).gameObject;
        dialog_switch = Instantiate(General.UI.Prefabs.SwitchButton.Prefab, dialog_switch_row.transform);
        dialog_switch.transform.localScale = Vector3.one;
        dialog_switch_label = ModBrowserText.CreateText(dialog_switch_row.transform, "Label", new Vector2(215, 16),
            Vector2.zero, 7, TextAnchor.MiddleLeft);

        Transform buttons = CreateRow(box.transform, "Buttons", new Vector2(280, 20), new Vector2(0, -68),
            TextAnchor.MiddleCenter, 12);
        dialog_confirm = CreateTextButton(buttons, "Confirm", new Vector2(90, 18), () => CloseDialog(true));
        dialog_cancel = CreateTextButton(buttons, "Cancel", new Vector2(90, 18), () => CloseDialog(false));
        dialog.SetActive(false);
    }

    private static Transform CreateRow(Transform pParent, string pName, Vector2 pSize, Vector2 pPosition,
        TextAnchor pAlignment, float pSpacing)
    {
        var row = new GameObject(pName, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(pParent, false);
        row.transform.localPosition = pPosition;
        row.GetComponent<RectTransform>().sizeDelta = pSize;
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = pAlignment;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = pSpacing;
        layout.padding = new RectOffset(3, 3, 2, 2);
        return row.transform;
    }

    private static ScrollRect CreateScrollView(Transform pParent, string pName, Vector2 pSize, Vector2 pPosition,
        out RectTransform pContent, bool pVerticalLayout)
    {
        var root = new GameObject(pName, typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
        root.transform.SetParent(pParent, false);
        root.transform.localPosition = pPosition;
        var root_rect = (RectTransform)root.transform;
        root_rect.sizeDelta = pSize;
        var background = root.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        background.type = Image.Type.Sliced;

        var content = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
        content.transform.SetParent(root.transform, false);
        pContent = (RectTransform)content.transform;
        pContent.anchorMin = new Vector2(0, 1);
        pContent.anchorMax = new Vector2(1, 1);
        pContent.pivot = new Vector2(0.5f, 1);
        pContent.anchoredPosition = Vector2.zero;
        pContent.sizeDelta = Vector2.zero;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        if (pVerticalLayout)
        {
            var layout = content.AddComponent<VerticalLayoutGroup>();
            OT.InitializeNoActionVerticalLayoutGroup(layout);
            layout.spacing = 2;
            layout.padding = new RectOffset(4, 4, 4, 4);
        }

        var scroll = root.GetComponent<ScrollRect>();
        scroll.content = pContent;
        scroll.viewport = root_rect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 12;
        return scroll;
    }

    private static SimpleButton CreateTextButton(Transform pParent, string pName, Vector2 pSize, UnityAction pAction)
    {
        SimpleButton button = Instantiate(SimpleButton.Prefab, pParent);
        button.name = pName;
        button.transform.localScale = Vector3.one;
        // Text and action of most buttons are set later; the name is only a placeholder.
        button.Setup(pAction ?? (() => { }), null, pName, pSize);
        return button;
    }

    private static void SetupButton(SimpleButton pButton, string pTextKey, UnityAction pAction)
    {
        pButton.gameObject.SetActive(true);
        pButton.Text.text = LM.Get(pTextKey);
        pButton.Button.onClick.RemoveAllListeners();
        pButton.Button.onClick.AddListener(pAction);
        SetInteractable(pButton, true);
    }

    private static void SetInteractable(SimpleButton pButton, bool pInteractable)
    {
        pButton.Button.interactable = pInteractable;
        pButton.Background.color = pInteractable ? Color.white : Color.gray;
    }

    #endregion
}
