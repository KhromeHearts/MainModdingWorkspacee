using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NeoModLoader.General;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Text building and formatting shared by the Mod Browser UI.
/// </summary>
internal static class ModBrowserText
{
    public static readonly Color WarningColor = new(1f, 0.6f, 0.15f);
    public static readonly Color ProgressColor = new(0.35f, 0.85f, 1f);

    private static readonly Regex bbcode_tag = new(@"\[/?(?:[a-z0-9*]+)(?:=[^\]]*)?\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // "{name} Uploaded by NeoModLoader\n{name} 由NeoModLoader上传\n\n" ... "\n\nModLoader: {url}\n\n模组加载器: {url}"
    private static readonly Regex upload_header =
        new(@"^[^\n]*Uploaded by NeoModLoader\n[^\n]*由NeoModLoader上传\n*", RegexOptions.Compiled);

    private static readonly Regex upload_footer =
        new(@"\n*ModLoader: [^\n]*(?:\n+模组加载器: [^\n]*)?\s*$", RegexOptions.Compiled);

    public static Text CreateText(Transform pParent, string pName, Vector2 pSize, Vector2 pPosition, int pFontSize,
        TextAnchor pAlignment, bool pRichText = false)
    {
        var obj = new GameObject(pName, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(pParent, false);
        obj.transform.localPosition = pPosition;
        obj.GetComponent<RectTransform>().sizeDelta = pSize;
        var text = obj.GetComponent<Text>();
        text.font = LocalizedTextManager.current_font;
        text.fontSize = pFontSize;
        text.alignment = pAlignment;
        text.supportRichText = pRichText;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    public static string Truncate(string pText, int pMaxLength)
    {
        if (string.IsNullOrEmpty(pText)) return "";
        pText = pText.Replace("\r", "").Replace('\n', ' ').Trim();
        return pText.Length <= pMaxLength ? pText : pText.Substring(0, pMaxLength - 3).TrimEnd() + "...";
    }

    /// <summary>
    ///     Workshop descriptions use BBCode; keep the text, turn list bullets and headings into plain lines.
    /// </summary>
    public static string StripBBCode(string pText)
    {
        if (string.IsNullOrEmpty(pText)) return "";
        string text = pText.Replace("\r\n", "\n");
        text = Regex.Replace(text, @"\[\*\]", "\n- ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\[/?h[1-3]\]", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\[img\].*?\[/img\]", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        text = bbcode_tag.Replace(text, "");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return text.Trim();
    }

    /// <summary>
    ///     Description for display: BBCode stripped, and the fixed lines NML's uploader wraps around a mod's
    ///     description (<see cref="ModWorkshopService.UploadMod" />) removed, so rows show the mod's own text.
    /// </summary>
    public static string Description(WorkshopModInfo pItem)
    {
        string text = StripBBCode(pItem.Description);
        text = upload_header.Replace(text, "");
        text = upload_footer.Replace(text, "");
        return text.Trim();
    }

    public static string FormatCount(ulong pValue)
    {
        return pValue.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string FormatSize(long pBytes)
    {
        if (pBytes < 0) return LM.Get("mod_browser_size_unknown");
        if (pBytes < 1024) return $"{pBytes} B";
        if (pBytes < 1024 * 1024) return string.Format(CultureInfo.InvariantCulture, "{0:0.#} KB", pBytes / 1024.0);
        return string.Format(CultureInfo.InvariantCulture, "{0:0.#} MB", pBytes / (1024.0 * 1024.0));
    }

    public static string FormatDate(DateTime pDate)
    {
        return pDate == default ? "-" : pDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string Rating(WorkshopModInfo pItem)
    {
        uint votes = pItem.VotesUp + pItem.VotesDown;
        return votes == 0
            ? LM.Get("mod_browser_no_rating")
            : string.Format(LM.Get("mod_browser_rating"), Mathf.RoundToInt(pItem.Score * 100), votes);
    }

    public static string Author(WorkshopModInfo pItem)
    {
        string name = ModBrowserService.GetOwnerName(pItem);
        if (name == null) return "...";
        return name.Length == 0 ? LM.Get("mod_browser_unknown_author") : name;
    }

    /// <summary>
    ///     "by Author | 1,234 subscribers | 85% | 2026-09-12 | v1.0", plain text.
    /// </summary>
    public static string RowMeta(WorkshopModInfo pItem, string pInstalledVersion)
    {
        var sb = new StringBuilder();
        sb.Append(string.Format(LM.Get("mod_browser_by"), Truncate(Author(pItem), 18)));
        sb.Append(" | ").Append(string.Format(LM.Get("mod_browser_subscribers"), FormatCount(pItem.Subscriptions)));
        uint votes = pItem.VotesUp + pItem.VotesDown;
        if (votes > 0) sb.Append(" | ").Append(Mathf.RoundToInt(pItem.Score * 100)).Append('%');
        sb.Append(" | ").Append(FormatDate(pItem.Updated));
        if (!string.IsNullOrEmpty(pInstalledVersion)) sb.Append(" | v").Append(Truncate(pInstalledVersion, 12));
        return sb.ToString();
    }

    /// <summary>
    ///     Coloured, localized state label (NML's own strings only, safe for rich text).
    /// </summary>
    public static string StatusBadge(ModBrowserStatus pStatus)
    {
        string label = LM.Get(StatusKey(pStatus.Kind));
        if (pStatus.Kind == ModBrowserStatusKind.Downloading)
        {
            label = $"{label} {Mathf.RoundToInt(Mathf.Clamp01(pStatus.Progress) * 100)}%";
        }

        return $"<color=#{ColorUtility.ToHtmlStringRGB(StatusColor(pStatus.Kind))}>{label}</color>";
    }

    public static string StatusKey(ModBrowserStatusKind pKind)
    {
        return pKind switch
        {
            ModBrowserStatusKind.WaitingForSteam => "mod_browser_state_waiting",
            ModBrowserStatusKind.Queued => "mod_browser_state_queued",
            ModBrowserStatusKind.Subscribing => "mod_browser_state_subscribing",
            ModBrowserStatusKind.Downloading => "mod_browser_state_downloading",
            ModBrowserStatusKind.Installing => "mod_browser_state_installing",
            ModBrowserStatusKind.Failed => "mod_browser_state_failed",
            ModBrowserStatusKind.UpdateAvailable => "mod_browser_state_update_available",
            ModBrowserStatusKind.Incompatible => "mod_browser_state_incompatible",
            ModBrowserStatusKind.MissingDependency => "mod_browser_state_missing_dependency",
            ModBrowserStatusKind.Enabled => "mod_browser_state_enabled",
            ModBrowserStatusKind.EnabledNotLoaded => "mod_browser_state_enabled_not_loaded",
            ModBrowserStatusKind.Disabled => "mod_browser_state_disabled",
            ModBrowserStatusKind.LoadFailed => "mod_browser_state_load_failed",
            ModBrowserStatusKind.RestartRequired => "mod_browser_state_restart_required",
            ModBrowserStatusKind.InstalledLocalCopy => "mod_browser_state_local_copy",
            ModBrowserStatusKind.Installed => "mod_browser_state_installed",
            ModBrowserStatusKind.UninstallPending => "mod_browser_state_uninstall_pending",
            _ => "mod_browser_state_not_installed"
        };
    }

    public static Color StatusColor(ModBrowserStatusKind pKind)
    {
        return pKind switch
        {
            ModBrowserStatusKind.Enabled => new Color(0.45f, 0.95f, 0.45f),
            ModBrowserStatusKind.EnabledNotLoaded => new Color(0.75f, 0.95f, 0.45f),
            ModBrowserStatusKind.Installed or ModBrowserStatusKind.Disabled => new Color(0.9f, 0.9f, 0.9f),
            ModBrowserStatusKind.InstalledLocalCopy => new Color(0.55f, 0.8f, 1f),
            ModBrowserStatusKind.Downloading or ModBrowserStatusKind.Installing => ProgressColor,
            ModBrowserStatusKind.Queued or ModBrowserStatusKind.Subscribing or ModBrowserStatusKind.WaitingForSteam
                or ModBrowserStatusKind.RestartRequired => new Color(1f, 0.9f, 0.35f),
            ModBrowserStatusKind.UpdateAvailable or ModBrowserStatusKind.MissingDependency => WarningColor,
            ModBrowserStatusKind.Failed or ModBrowserStatusKind.LoadFailed or ModBrowserStatusKind.Incompatible =>
                new Color(1f, 0.35f, 0.3f),
            _ => new Color(0.65f, 0.65f, 0.65f)
        };
    }
}
