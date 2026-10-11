using System;
using UnityEngine;
using VisibleArmors.Appearance;
using VisibleArmors.Core;
using VisibleArmors.Render;

namespace VisibleArmors.Api
{
    /// <summary>
    /// Public entry point for other mods (PossessLib, SurvivalBox, item packs). Everything here is optional:
    /// items from other mods already get a look automatically (from their material, or from their icon colours).
    /// Use this to give an item a specific style, add palettes and templates from code, or tune species.
    /// Call it from the main thread, for example in your mod's OnModLoad. Reference it as an optional
    /// dependency (GUID "RAYTRACERX_VISIBLEARMORS") and guard calls with #if RAYTRACERX_VISIBLEARMORS.
    /// </summary>
    public static class VisibleArmorsApi
    {
        /// <summary>Mod version string.</summary>
        public const string Version = "0.1.0";

        /// <summary>API level. Increases when methods are added; existing methods keep working.</summary>
        public const int ApiVersion = 1;

        /// <summary>
        /// Adds or replaces a material palette. pColors needs 8 entries: outline, shadow, dark, mid, light,
        /// highlight, accent, dark accent (the same layout as the materials/*.png files).
        /// </summary>
        /// <returns>False if the arguments are invalid.</returns>
        public static bool RegisterPalette(string pPaletteId, Color32[] pColors)
        {
            if (string.IsNullOrEmpty(pPaletteId) || pColors == null || pColors.Length < MaterialPalette.Size) return false;
            ArtLibrary.AddPalette(new MaterialPalette(pPaletteId, pColors));
            RenderSync.RefreshAll();
            return true;
        }

        /// <summary>
        /// Adds or replaces a helmet template (5x5) or torso template (5x4). Pixels are in Unity order
        /// (bottom row first, as Texture2D.GetPixels32 returns them) and use the colour roles described in the README.
        /// Boots and amulets have no templates; use <see cref="RegisterBootsStyle"/> for boots.
        /// </summary>
        /// <returns>False if the slot has no templates or the size is wrong.</returns>
        public static bool RegisterTemplate(ArmorSlot pSlot, string pStyleId, Color32[] pPixels, int pWidth, int pHeight)
        {
            if (string.IsNullOrEmpty(pStyleId)) return false;
            bool ok = ArtLibrary.AddTemplate(pSlot, pStyleId, pPixels, pWidth, pHeight);
            if (ok) RenderSync.RefreshAll();
            return ok;
        }

        /// <summary>Adds or replaces a boots style (boots are drawn procedurally on the feet).</summary>
        /// <param name="pStyleId">Style id, used by <see cref="SetItemLook"/> and appearances.json.</param>
        /// <param name="pHeight">Boot height in pixels (1 to 4).</param>
        /// <param name="pShade">Palette shade of the boot (0 outline to 5 highlight).</param>
        /// <param name="pTopShade">Palette shade of the top row (cuff).</param>
        /// <param name="pAccentCuff">Use the palette accent for the cuff instead.</param>
        public static void RegisterBootsStyle(string pStyleId, int pHeight, int pShade, int pTopShade, bool pAccentCuff)
        {
            if (string.IsNullOrEmpty(pStyleId)) return;
            AppearanceRules.Boots[pStyleId] = new BootsStyle
            {
                Id = pStyleId,
                Height = Mathf.Clamp(pHeight, 1, 4),
                Shade = Mathf.Clamp(pShade, 0, 5),
                TopShade = Mathf.Clamp(pTopShade, 0, 5),
                AccentCuff = pAccentCuff
            };
            RenderSync.RefreshAll();
        }

        /// <summary>
        /// Chooses how one item looks on the body. pStyle is a template or boots style id ("helm", "plate", "greaves"...),
        /// pPalette a palette id ("iron", or one from <see cref="RegisterPalette"/>). Pass null to keep the automatic choice
        /// for that part. Takes priority over appearances.json.
        /// </summary>
        public static void SetItemLook(string pItemId, string pStyle, string pPalette)
        {
            if (string.IsNullOrEmpty(pItemId)) return;
            AppearanceRegistry.SetItemRule(pItemId, pStyle, pPalette);
            RenderSync.RefreshAll();
        }

        /// <summary>
        /// Sets which pieces a species shows: "humanoid" (all), "head_only" (helmet only), "auto" (decided from the
        /// sprite) or "none". Overrides appearances.json.
        /// </summary>
        public static void SetSpeciesProfile(string pActorAssetId, string pProfile)
        {
            if (string.IsNullOrEmpty(pActorAssetId)) return;
            SpeciesProfiles.SetApi(pActorAssetId, AppearanceRules.ParseProfile(pProfile));
            RenderSync.RefreshAll();
        }

        /// <summary>
        /// Makes a unit rebuild its sprite on the next frame. Not needed after normal equipment changes (those are
        /// picked up automatically every frame); useful if your mod changes something the look depends on.
        /// </summary>
        public static void Refresh(Actor pActor)
        {
            RenderSync.Invalidate(pActor);
        }

        /// <summary>Rebuilds every armored sprite on the next frame.</summary>
        public static void RefreshAll()
        {
            RenderSync.RefreshAll();
        }

        /// <summary>True when the unit is currently drawn with gear from this mod (in the world, not the unit window).</summary>
        public static bool IsArmorDrawn(Actor pActor)
        {
            if (pActor == null) return false;
            try
            {
                return Compositor.SignatureOf(pActor.getLastColoredSprite()) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
