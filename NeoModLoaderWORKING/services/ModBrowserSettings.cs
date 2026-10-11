using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoModLoader.services;

/// <summary>
///     Mod Browser preferences, stored in NML/mod_browser_settings.json.
/// </summary>
/// <remarks>
///     The search text is not stored: it lasts for the current game session only, so an old search does not
///     silently narrow the list after a restart. Files written by earlier builds may still contain
///     "search_text"; it is ignored.
///     Kept out of mod_compile_records.json on purpose: that file is a strict UID-to-record dictionary, older NML
///     versions read it as such, and it is truncated when a compile fails. Enabled state still lives there.
/// </remarks>
internal sealed class ModBrowserSettings
{
    [JsonProperty("scope")]
    [JsonConverter(typeof(StringEnumConverter))]
    public WorkshopBrowseScope Scope { get; set; } = WorkshopBrowseScope.All;

    [JsonProperty("sort")]
    [JsonConverter(typeof(StringEnumConverter))]
    public WorkshopBrowseSort Sort { get; set; } = WorkshopBrowseSort.MostSubscribed;

    [JsonProperty("include_verified")] public bool IncludeVerified { get; set; } = true;
    [JsonProperty("include_unverified")] public bool IncludeUnverified { get; set; } = true;

    /// <summary>
    ///     The one-time warning about running Workshop code was accepted.
    /// </summary>
    [JsonProperty("warning_accepted")]
    public bool WarningAccepted { get; set; }

    public WorkshopBrowseQuery ToQuery()
    {
        return new WorkshopBrowseQuery
        {
            Scope = Scope,
            // Relevance needs search text, which is never restored.
            Sort = Sort == WorkshopBrowseSort.Relevance ? WorkshopBrowseSort.MostSubscribed : Sort,
            SearchText = "",
            IncludeVerified = IncludeVerified,
            IncludeUnverified = IncludeUnverified
        };
    }

    public void SetQuery(WorkshopBrowseQuery pQuery)
    {
        Scope = pQuery.Scope;
        Sort = pQuery.Sort;
        IncludeVerified = pQuery.IncludeVerified;
        IncludeUnverified = pQuery.IncludeUnverified;
    }

    /// <summary>
    ///     Read settings, falling back to defaults (with a warning) when the file is missing or invalid.
    /// </summary>
    public static ModBrowserSettings Load(string pPath)
    {
        if (!File.Exists(pPath)) return new ModBrowserSettings();
        try
        {
            ModBrowserSettings settings = JsonConvert.DeserializeObject<ModBrowserSettings>(File.ReadAllText(pPath));
            if (settings == null) throw new JsonException("empty settings file");
            if (!settings.IncludeVerified && !settings.IncludeUnverified)
            {
                settings.IncludeVerified = true;
                settings.IncludeUnverified = true;
            }

            return settings;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Ignoring invalid Mod Browser settings at {pPath}: {e.Message}");
            return new ModBrowserSettings();
        }
    }

    public void Save(string pPath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pPath)!);
            File.WriteAllText(pPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        catch (Exception e)
        {
            LogService.LogError($"Failed to save Mod Browser settings to {pPath}: {e.Message}");
        }
    }
}
