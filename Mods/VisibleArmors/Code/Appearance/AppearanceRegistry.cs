using System;
using System.Collections.Generic;
using UnityEngine;
using VisibleArmors.Core;

namespace VisibleArmors.Appearance
{
    /// <summary>A resolved look for one slot: which template or style, in which colours.</summary>
    internal sealed class ArmorLook
    {
        public int Id;
        public ArmorSlot Slot;
        public string Style;
        public MaterialPalette Palette;
        /// <summary>Helmet (5x5) or torso (5x4) template; null for boots and amulets.</summary>
        public PieceTemplate Template;
        public BootsStyle Boots;
        public CapeMode Cape;
    }

    /// <summary>
    /// Maps equipment assets to <see cref="ArmorLook"/> ids (1..65535, 0 = nothing drawn). Identical looks share an
    /// id, so composite sprites are shared too. Resolution order for an item:
    /// API registration, the "items" table, keyword rules, the material table, then fallbacks
    /// (palette from the item icon, then a hue derived from the material name). Main thread only.
    /// </summary>
    internal static class AppearanceRegistry
    {
        private static readonly List<ArmorLook> _looks = new List<ArmorLook> { null };
        private static readonly Dictionary<string, int> _intern = new Dictionary<string, int>();
        private static readonly Dictionary<EquipmentAsset, int> _byAsset = new Dictionary<EquipmentAsset, int>();
        private static readonly Dictionary<string, ItemRule> _apiItems = new Dictionary<string, ItemRule>(StringComparer.OrdinalIgnoreCase);

        public static int LookCount => _looks.Count - 1;

        public static ArmorLook Get(int pId)
        {
            return pId > 0 && pId < _looks.Count ? _looks[pId] : null;
        }

        public static bool TrySlotOf(EquipmentType pType, out ArmorSlot pSlot)
        {
            switch (pType)
            {
                case EquipmentType.Helmet: pSlot = ArmorSlot.Helmet; return true;
                case EquipmentType.Armor: pSlot = ArmorSlot.Armor; return true;
                case EquipmentType.Boots: pSlot = ArmorSlot.Boots; return true;
                case EquipmentType.Amulet: pSlot = ArmorSlot.Amulet; return true;
                default: pSlot = ArmorSlot.Helmet; return false;
            }
        }

        /// <summary>Look id for an equipment asset (cached). Returns 0 when the item has no visible look.</summary>
        public static int IdFor(EquipmentAsset pAsset)
        {
            if (pAsset == null) return 0;
            if (_byAsset.TryGetValue(pAsset, out int id)) return id;
            try
            {
                id = Resolve(pAsset);
            }
            catch (Exception e)
            {
                VALog.Warn("Could not resolve a look for item " + pAsset.id + ": " + e.Message);
                id = 0;
            }
            _byAsset[pAsset] = id;
            return id;
        }

        /// <summary>API: per-item override. Clears the cache so the next frame picks it up.</summary>
        public static void SetItemRule(string pItemId, string pStyle, string pPalette)
        {
            _apiItems[pItemId] = new ItemRule { Style = pStyle, Palette = pPalette };
            ClearCache();
        }

        public static void ClearCache()
        {
            _byAsset.Clear();
        }

        private static int Resolve(EquipmentAsset pAsset)
        {
            if (!TrySlotOf(pAsset.equipment_type, out ArmorSlot slot)) return 0;
            string id = pAsset.id ?? string.Empty;
            string lowerId = id.ToLowerInvariant();
            string material = string.IsNullOrEmpty(pAsset.material) ? "basic" : pAsset.material;

            string style = null;
            string paletteId = null;
            if (_apiItems.TryGetValue(id, out ItemRule api))
            {
                style = api.Style;
                paletteId = api.Palette;
            }
            if (AppearanceRules.Items.TryGetValue(id, out ItemRule rule))
            {
                style = style ?? rule.Style;
                paletteId = paletteId ?? rule.Palette;
            }
            foreach (KeywordRule keyword in AppearanceRules.Keywords)
            {
                if (!lowerId.Contains(keyword.Match)) continue;
                if (style == null && keyword.Style != null && (!keyword.Slot.HasValue || keyword.Slot.Value == slot)) style = keyword.Style;
                if (paletteId == null && keyword.Palette != null) paletteId = keyword.Palette;
            }
            if (style == null) style = StyleFromTable(material, slot);
            if (style == null) style = StyleFromTable("default", slot);

            MaterialPalette palette = ResolvePalette(pAsset, paletteId, material);
            ArmorLook look = Build(slot, style, palette);
            if (look == null)
            {
                string fallback = StyleFromTable("default", slot);
                if (fallback != style) look = Build(slot, fallback, palette);
            }
            if (look == null)
            {
                VALog.Warn("No art for item " + id + " (style '" + style + "'), it stays invisible on the body");
                return 0;
            }
            VALog.Debug("Item " + id + " (" + slot + ", material " + material + ") -> style " + look.Style + ", palette " + look.Palette.Id + ", look #" + look.Id);
            return look.Id;
        }

        private static string StyleFromTable(string pMaterial, ArmorSlot pSlot)
        {
            return AppearanceRules.MaterialStyles.TryGetValue(pMaterial, out string[] styles) ? styles[(int)pSlot] : null;
        }

        private static MaterialPalette ResolvePalette(EquipmentAsset pAsset, string pPaletteId, string pMaterial)
        {
            MaterialPalette palette = ArtLibrary.GetPalette(pPaletteId);
            if (palette != null) return palette;
            if (AppearanceRules.PaletteAliases.TryGetValue(pMaterial, out string alias)) palette = ArtLibrary.GetPalette(alias);
            if (palette != null) return palette;
            palette = ArtLibrary.GetPalette(pMaterial);
            if (palette != null) return palette;

            // Unknown material (usually from another mod): take the colours from the item icon.
            palette = PaletteFromIcon(pAsset);
            if (palette != null)
            {
                ArtLibrary.AddPalette(palette);
                return palette;
            }
            if (pMaterial == "basic")
            {
                palette = ArtLibrary.GetPalette(AppearanceRules.FallbackPalette);
                if (palette != null) return palette;
            }
            // Last resort: a stable hue derived from the material name, so different unknown materials still differ.
            string key = pMaterial == "basic" ? pAsset.id : pMaterial;
            float hue = (StableHash(key) & 0xFFFF) / 65535f;
            palette = MaterialPalette.FromHue("hue:" + key, hue, 0.35f);
            ArtLibrary.AddPalette(palette);
            return palette;
        }

        private static MaterialPalette PaletteFromIcon(EquipmentAsset pAsset)
        {
            try
            {
                Sprite icon = pAsset.getSprite();
                if (icon == null || icon.texture == null || !icon.texture.isReadable) return null;
                Rect r = icon.textureRect;
                Color[] px = icon.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
                var px32 = new Color32[px.Length];
                for (int i = 0; i < px.Length; i++) px32[i] = px[i];
                return MaterialPalette.FromPixels("icon:" + pAsset.id, px32);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static ArmorLook Build(ArmorSlot pSlot, string pStyle, MaterialPalette pPalette)
        {
            if (string.IsNullOrEmpty(pStyle) || pPalette == null) return null;
            var look = new ArmorLook { Slot = pSlot, Style = pStyle, Palette = pPalette };
            switch (pSlot)
            {
                case ArmorSlot.Helmet:
                    look.Template = ArtLibrary.GetHelmet(pStyle);
                    if (look.Template == null) return null;
                    break;
                case ArmorSlot.Armor:
                    look.Template = ArtLibrary.GetTorso(pStyle);
                    if (look.Template == null) return null;
                    AppearanceRules.Capes.TryGetValue(pStyle, out look.Cape);
                    break;
                case ArmorSlot.Boots:
                    if (!AppearanceRules.Boots.TryGetValue(pStyle, out look.Boots)) return null;
                    break;
            }
            string key = (int)pSlot + "|" + pStyle + "|" + pPalette.Id;
            if (_intern.TryGetValue(key, out int existing)) return _looks[existing];
            if (_looks.Count >= 0xFFFF) return null;
            look.Id = _looks.Count;
            _looks.Add(look);
            _intern[key] = look.Id;
            return look;
        }

        /// <summary>String hash that is the same in every session (string.GetHashCode is not guaranteed to be).</summary>
        private static int StableHash(string pText)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in pText ?? string.Empty) hash = hash * 31 + c;
                return hash;
            }
        }
    }
}
