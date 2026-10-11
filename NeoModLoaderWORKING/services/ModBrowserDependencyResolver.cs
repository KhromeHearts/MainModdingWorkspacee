using NeoModLoader.utils;

namespace NeoModLoader.services;

/// <summary>
///     Finds Workshop items that may provide a missing dependency.
/// </summary>
/// <remarks>
///     Workshop items carry no NML metadata, so a dependency UID ("AUTHOR_MOD_NAME", built from mod.json's author
///     and name) can only be matched by title: NML uploads use the mod name as the Workshop title. A match is a
///     candidate only; the installed mod.json must confirm the UID (see
///     <see cref="WorkshopQueueEntry.ExpectedUid" />).
/// </remarks>
internal static class ModBrowserDependencyResolver
{
    private const int max_search_terms = 3;

    /// <summary>
    ///     Search texts for a UID, most specific first, e.g. "AUTHOR_COOL_MOD" gives "AUTHOR COOL MOD",
    ///     "COOL MOD", "MOD". Steam search ignores case.
    /// </summary>
    public static List<string> BuildSearchTerms(string pUid)
    {
        string[] words = (pUid ?? "").Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        var terms = new List<string>();
        for (int i = 0; i < words.Length && terms.Count < max_search_terms; i++)
        {
            string term = string.Join(" ", words.Skip(i));
            if (term.Length >= 3) terms.Add(term);
        }

        return terms;
    }

    /// <summary>
    ///     Whether a title, turned into a UID the way NML does, equals <paramref name="pUid" /> or is its name part.
    /// </summary>
    public static bool TitleMatchesUid(string pTitle, string pUid)
    {
        string title = Normalize(ModDependencyUtils.ParseDepenNameToPreprocessSymbol((pTitle ?? "").Trim()));
        string uid = Normalize(pUid);
        if (title.Length == 0 || uid.Length == 0) return false;
        return uid == title || uid.EndsWith("_" + title, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Best candidate among query results: a title match, preferring the most specific (longest) match, then
    ///     verified items, then subscribers. A short title like "Mod" matches the end of many UIDs.
    /// </summary>
    public static WorkshopModInfo PickCandidate(string pUid, IEnumerable<WorkshopModInfo> pItems)
    {
        return pItems.Where(item => !item.IsModLoader && TitleMatchesUid(item.Title, pUid))
            .OrderByDescending(item => Normalize(ModDependencyUtils.ParseDepenNameToPreprocessSymbol(item.Title.Trim()))
                .Length)
            .ThenByDescending(item => item.IsVerified)
            .ThenByDescending(item => item.Subscriptions)
            .FirstOrDefault();
    }

    /// <summary>
    ///     Upper-case, single underscores, none at the ends.
    /// </summary>
    private static string Normalize(string pSymbol)
    {
        string[] parts = (pSymbol ?? "").ToUpperInvariant()
            .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("_", parts);
    }
}
