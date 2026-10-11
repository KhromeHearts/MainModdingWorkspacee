using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     A Workshop item row in the Mod Browser.
/// </summary>
/// <remarks>
///     Workshop titles, descriptions and author names go into texts with rich text disabled, so they cannot inject
///     markup; only the state text (built from NML's own strings) uses rich text.
/// </remarks>
internal class ModBrowserListItem : APrefab<ModBrowserListItem>
{
    public const float Width = 246;
    public const float Height = 42;
    private static readonly Color selected_color = new(1f, 0.82f, 0.45f);

    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Text title;
    [SerializeField] private Text state;
    [SerializeField] private Text meta;
    [SerializeField] private Text unverified;
    [SerializeField] private Text description;
    [SerializeField] private RectTransform progress_fill;
    [SerializeField] private Button button;

    public WorkshopModInfo Item { get; private set; }

    public void Setup(WorkshopModInfo pItem, Action<WorkshopModInfo> pOnClick)
    {
        Init();
        Item = pItem;
        name = $"ModBrowserListItem {pItem.FileId}";
        title.text = ModBrowserText.Truncate(pItem.Title, 40);
        description.text = ModBrowserText.Truncate(ModBrowserText.Description(pItem), 130);
        unverified.gameObject.SetActive(!pItem.IsVerified);
        RefreshMeta(null);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => pOnClick?.Invoke(Item));

        icon.sprite = WorkshopThumbnailLoader.Placeholder;
        ulong requested = pItem.FileId;
        WorkshopThumbnailLoader.Request(pItem.FileId, pItem.PreviewUrl, (file_id, sprite) =>
        {
            if (this != null && Item != null && Item.FileId == requested && file_id == requested) icon.sprite = sprite;
        });
    }

    public void SetSelected(bool pSelected)
    {
        Init();
        background.color = pSelected ? selected_color : Color.white;
    }

    /// <summary>
    ///     Refresh the author/stats line (the author name may arrive later) and the state badge.
    /// </summary>
    public void RefreshMeta(ModBrowserStatus pStatus)
    {
        Init();
        if (Item == null) return;
        string version = pStatus?.InstalledMod?.Version;
        meta.text = ModBrowserText.RowMeta(Item, version);
        if (pStatus == null) return;

        state.text = ModBrowserText.StatusBadge(pStatus);
        bool show_progress = pStatus.Kind is ModBrowserStatusKind.Downloading or ModBrowserStatusKind.Installing;
        progress_fill.gameObject.SetActive(show_progress);
        if (show_progress)
        {
            progress_fill.sizeDelta = new Vector2((Width - 46) * Mathf.Clamp01(pStatus.Progress), 2);
        }
    }

    protected override void Init()
    {
        if (Initialized) return;
        base.Init();
    }

    private static void _init()
    {
        var obj = new GameObject(nameof(ModBrowserListItem), typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(WorldBoxMod.InactiveTransform, false);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(Width, Height);
        var bg = obj.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        bg.type = Image.Type.Sliced;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(obj.transform, false);
        icon.transform.localPosition = new Vector3(-104, 0);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
        var icon_image = icon.GetComponent<Image>();
        icon_image.preserveAspect = true;
        icon_image.raycastTarget = false;

        var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(icon.transform, false);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
        var frame_image = frame.GetComponent<Image>();
        frame_image.sprite = InternalResourcesGetter.GetIconFrame();
        frame_image.type = Image.Type.Sliced;
        frame_image.raycastTarget = false;

        Text title = ModBrowserText.CreateText(obj.transform, "Title", new Vector2(148, 12), new Vector2(-10, 14), 8,
            TextAnchor.UpperLeft);
        Text state = ModBrowserText.CreateText(obj.transform, "State", new Vector2(55, 12), new Vector2(93.5f, 14),
            6, TextAnchor.UpperRight, true);
        Text meta = ModBrowserText.CreateText(obj.transform, "Meta", new Vector2(150, 10), new Vector2(-9, 3), 6,
            TextAnchor.UpperLeft);
        meta.color = new Color(0.85f, 0.85f, 0.85f);
        Text unverified = ModBrowserText.CreateText(obj.transform, "Unverified", new Vector2(53, 10),
            new Vector2(94.5f, 3), 6, TextAnchor.UpperRight);
        unverified.text = LM.Get("mod_browser_unverified");
        unverified.color = ModBrowserText.WarningColor;
        Text description = ModBrowserText.CreateText(obj.transform, "Description", new Vector2(205, 18),
            new Vector2(18.5f, -11), 6, TextAnchor.UpperLeft);
        description.color = new Color(0.75f, 0.75f, 0.75f);

        var progress = new GameObject("Progress", typeof(RectTransform), typeof(Image));
        progress.transform.SetParent(obj.transform, false);
        var progress_rect = progress.GetComponent<RectTransform>();
        progress_rect.anchorMin = new Vector2(0, 0);
        progress_rect.anchorMax = new Vector2(0, 0);
        progress_rect.pivot = new Vector2(0, 0);
        progress_rect.anchoredPosition = new Vector2(40, 1);
        progress_rect.sizeDelta = new Vector2(0, 2);
        var progress_image = progress.GetComponent<Image>();
        progress_image.color = ModBrowserText.ProgressColor;
        progress_image.raycastTarget = false;
        progress.SetActive(false);

        Prefab = obj.AddComponent<ModBrowserListItem>();
        Prefab.background = bg;
        Prefab.icon = icon_image;
        Prefab.title = title;
        Prefab.state = state;
        Prefab.meta = meta;
        Prefab.unverified = unverified;
        Prefab.description = description;
        Prefab.progress_fill = progress_rect;
        Prefab.button = obj.GetComponent<Button>();
    }
}
