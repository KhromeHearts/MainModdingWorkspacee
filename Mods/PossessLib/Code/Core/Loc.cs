namespace PossessLib.Core
{
    /// <summary>Localisation helper: uses Locales/*.json through the game's text manager, with an English fallback.</summary>
    internal static class Loc
    {
        public static string T(string pKey, string pFallback)
        {
            if (!string.IsNullOrEmpty(pKey) && LocalizedTextManager.stringExists(pKey))
            {
                return LocalizedTextManager.getText(pKey, null, false);
            }
            return pFallback;
        }

        public static string F(string pKey, string pFallback, string pArg0)
        {
            return T(pKey, pFallback).Replace("$0", pArg0);
        }
    }
}
