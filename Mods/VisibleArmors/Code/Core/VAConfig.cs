using System;
using NeoModLoader.api;
using UnityEngine;

namespace VisibleArmors.Core
{
    /// <summary>
    /// Runtime copy of the NML config (default_config.json, group "VisibleArmors").
    /// NML calls the Set* callbacks when the config loads and whenever the player edits a value, so the
    /// fields are always current. <see cref="ReadFrom"/> is a fallback in case a callback could not be resolved.
    /// Every change bumps <see cref="Version"/>; the renderer compares it once per frame and refreshes units.
    /// </summary>
    internal static class VAConfig
    {
        public const string Group = "VisibleArmors";

        public static bool Enabled = true;
        public static bool ShowHelmet = true;
        public static bool ShowArmor = true;
        public static bool ShowBoots = true;
        public static bool ShowCape = true;
        public static bool ShowAmulet = true;
        public static bool UiAvatars = true;
        public static bool OtherSpecies = true;
        public static float MaxZoom = 80f;
        public static int CachePages = 8;
        public static bool Debug;

        /// <summary>Incremented on every change. Read on the main thread only.</summary>
        public static int Version = 1;

        // ---- NML callbacks (referenced by name from default_config.json) ----
        public static void SetEnabled(bool pValue) { Enabled = pValue; Bump(); }
        public static void SetShowHelmet(bool pValue) { ShowHelmet = pValue; Bump(); }
        public static void SetShowArmor(bool pValue) { ShowArmor = pValue; Bump(); }
        public static void SetShowBoots(bool pValue) { ShowBoots = pValue; Bump(); }
        public static void SetShowCape(bool pValue) { ShowCape = pValue; Bump(); }
        public static void SetShowAmulet(bool pValue) { ShowAmulet = pValue; Bump(); }
        public static void SetUiAvatars(bool pValue) { UiAvatars = pValue; Bump(); }
        public static void SetOtherSpecies(bool pValue) { OtherSpecies = pValue; Bump(); }
        public static void SetMaxZoom(float pValue) { MaxZoom = Mathf.Clamp(pValue, 10f, 400f); Bump(); }
        public static void SetCachePages(int pValue) { CachePages = Mathf.Clamp(pValue, 2, 64); Bump(); }
        public static void SetDebug(bool pValue) { Debug = pValue; Bump(); }

        private static void Bump()
        {
            unchecked { Version++; }
        }

        /// <summary>Reads every value straight from the NML config object.</summary>
        public static void ReadFrom(ModConfig pConfig)
        {
            if (pConfig == null) return;
            try
            {
                foreach (var pair in pConfig[Group])
                {
                    object value = pair.Value.GetValue();
                    switch (pair.Key)
                    {
                        case "va_enabled": SetEnabled((bool)value); break;
                        case "va_show_helmet": SetShowHelmet((bool)value); break;
                        case "va_show_armor": SetShowArmor((bool)value); break;
                        case "va_show_boots": SetShowBoots((bool)value); break;
                        case "va_show_cape": SetShowCape((bool)value); break;
                        case "va_show_amulet": SetShowAmulet((bool)value); break;
                        case "va_ui_avatars": SetUiAvatars((bool)value); break;
                        case "va_other_species": SetOtherSpecies((bool)value); break;
                        case "va_max_zoom": SetMaxZoom((float)value); break;
                        case "va_cache_pages": SetCachePages((int)value); break;
                        case "va_debug": SetDebug((bool)value); break;
                    }
                }
            }
            catch (Exception e)
            {
                VALog.Warn("Could not read config, using defaults: " + e.Message);
            }
        }
    }
}
