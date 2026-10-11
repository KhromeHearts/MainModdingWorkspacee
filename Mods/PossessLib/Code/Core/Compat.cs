using System;
using NeoModLoader.api;

namespace PossessLib.Core
{
    /// <summary>
    /// Detects mods that drive the possessed unit's hand and HUD too. PossessLib and MainCharacterMod
    /// can be installed side by side (no shared IDs, save keys or resource paths), but if both are
    /// ENABLED they fight every frame: MainCharacterMod's own hotbar keeps taking the weapon out of the
    /// hand, and both draw a hotbar and a status row in the same corner.
    /// </summary>
    internal static class Compat
    {
        public const string MainCharacterModUid = "RTX_MCMOD";

        private static bool _checked;
        private static bool _warnedThisSession;

        public static bool MainCharacterModActive { get; private set; }

        public static void Check()
        {
            if (_checked) return;
            _checked = true;
            try
            {
                foreach (IMod mod in NeoModLoader.WorldBoxMod.LoadedMods)
                {
                    ModDeclare decl = mod.GetDeclaration();
                    if (decl != null && decl.UID == MainCharacterModUid) MainCharacterModActive = true;
                }
            }
            catch (Exception e)
            {
                PLLog.Debug("Compat check failed: " + e.Message);
            }
            if (MainCharacterModActive)
            {
                PLLog.Warn("Main Character Mod is enabled too. Both mods control the possessed unit's weapon and HUD, "
                           + "so weapons will not stay in hand and two HUDs will overlap. Disable one of them in the NML mod list.");
            }
        }

        /// <summary>Called when possession starts: one on-screen reminder per session.</summary>
        public static void WarnOnPossession()
        {
            Check();
            if (!MainCharacterModActive || _warnedThisSession) return;
            _warnedThisSession = true;
            Toasts.Show(Loc.T("possesslib_toast_mcmod_conflict", "<color=#FF8080>Main Character Mod is also enabled: disable one of them.</color>"));
        }
    }
}
