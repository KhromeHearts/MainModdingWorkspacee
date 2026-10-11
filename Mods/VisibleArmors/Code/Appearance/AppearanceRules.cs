using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using VisibleArmors.Core;

namespace VisibleArmors.Appearance
{
    /// <summary>How a cape is coloured (armor styles only).</summary>
    internal enum CapeMode : byte
    {
        None,
        Kingdom,
        Material
    }

    /// <summary>Which parts of the armor a species can show.</summary>
    internal enum BodyProfile : byte
    {
        /// <summary>No armor is drawn.</summary>
        None,
        /// <summary>Upright two-legged body: every piece is drawn.</summary>
        Humanoid,
        /// <summary>Only the helmet and the amulet are drawn (for bodies whose torso and feet cannot be found reliably).</summary>
        HeadOnly,
        /// <summary>Decided per sprite sheet: humanoid if the anatomy check passes, otherwise head only.</summary>
        Auto
    }

    /// <summary>Procedural boots settings (boots are anchored to the feet, which move a lot between frames, so they have no template).</summary>
    internal sealed class BootsStyle
    {
        public string Id;
        public int Height = 2;
        public int Shade = 3;
        public int TopShade = 4;
        public bool AccentCuff;
    }

    /// <summary>One keyword rule: when the item id contains <see cref="Match"/>, use this style and/or palette.</summary>
    internal sealed class KeywordRule
    {
        public string Match;
        public ArmorSlot? Slot;
        public string Style;
        public string Palette;
    }

    /// <summary>Per-item override from the "items" table or from the API.</summary>
    internal sealed class ItemRule
    {
        public string Style;
        public string Palette;
    }

    /// <summary>
    /// The rules that map items to looks, read from GameResources/visible_armors/appearances.json.
    /// See README "Adding armor art" for the format.
    /// </summary>
    internal static class AppearanceRules
    {
        public static readonly Dictionary<string, string[]> MaterialStyles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, string> PaletteAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static readonly List<KeywordRule> Keywords = new List<KeywordRule>();
        public static readonly Dictionary<string, ItemRule> Items = new Dictionary<string, ItemRule>(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, BootsStyle> Boots = new Dictionary<string, BootsStyle>(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, CapeMode> Capes = new Dictionary<string, CapeMode>(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, BodyProfile> Species = new Dictionary<string, BodyProfile>(StringComparer.OrdinalIgnoreCase);
        public static BodyProfile DefaultProfile = BodyProfile.Auto;
        public static string FallbackPalette = "steel";
        public static bool KeepKingCrown = true;

        public static void Load(string pArtRoot)
        {
            string path = Path.Combine(pArtRoot, "appearances.json");
            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException(path);
                Parse(JObject.Parse(File.ReadAllText(path)));
                VALog.Info("Appearance rules: " + MaterialStyles.Count + " materials, " + Keywords.Count + " keywords, " + Items.Count + " items, " + Species.Count + " species");
            }
            catch (Exception e)
            {
                VALog.Error("appearances.json could not be read (" + e.Message + "); using built-in defaults");
                MaterialStyles["default"] = new[] { "helm", "breastplate", "greaves", "pendant" };
                Boots["greaves"] = new BootsStyle { Id = "greaves" };
                Species["human"] = Species["elf"] = Species["orc"] = Species["dwarf"] = BodyProfile.Humanoid;
            }
            if (!MaterialStyles.ContainsKey("default")) MaterialStyles["default"] = new[] { "helm", "breastplate", "greaves", "pendant" };
        }

        private static void Parse(JObject pRoot)
        {
            FallbackPalette = (string)pRoot["fallback_palette"] ?? FallbackPalette;
            KeepKingCrown = (bool?)pRoot["keep_king_crown"] ?? true;

            if (pRoot["materials"] is JObject materials)
            {
                foreach (var pair in materials)
                {
                    var styles = new string[4];
                    if (pair.Value is JObject m)
                    {
                        styles[(int)ArmorSlot.Helmet] = (string)m["helmet"];
                        styles[(int)ArmorSlot.Armor] = (string)m["armor"];
                        styles[(int)ArmorSlot.Boots] = (string)m["boots"];
                        styles[(int)ArmorSlot.Amulet] = (string)m["amulet"];
                        string palette = (string)m["palette"];
                        if (!string.IsNullOrEmpty(palette)) PaletteAliases[pair.Key] = palette;
                    }
                    MaterialStyles[pair.Key] = styles;
                }
            }

            if (pRoot["keywords"] is JArray keywords)
            {
                foreach (JToken token in keywords)
                {
                    var rule = new KeywordRule
                    {
                        Match = ((string)token["match"])?.ToLowerInvariant(),
                        Style = (string)token["style"],
                        Palette = (string)token["palette"]
                    };
                    if (TryParseSlot((string)token["slot"], out ArmorSlot slot)) rule.Slot = slot;
                    if (!string.IsNullOrEmpty(rule.Match)) Keywords.Add(rule);
                }
            }

            if (pRoot["items"] is JObject items)
            {
                foreach (var pair in items)
                {
                    Items[pair.Key] = new ItemRule { Style = (string)pair.Value["style"], Palette = (string)pair.Value["palette"] };
                }
            }

            if (pRoot["boots_styles"] is JObject boots)
            {
                foreach (var pair in boots)
                {
                    Boots[pair.Key] = new BootsStyle
                    {
                        Id = pair.Key,
                        Height = Clamp((int?)pair.Value["height"] ?? 2, 1, 4),
                        Shade = Clamp((int?)pair.Value["shade"] ?? 3, 0, 5),
                        TopShade = Clamp((int?)pair.Value["top_shade"] ?? 4, 0, 5),
                        AccentCuff = (bool?)pair.Value["accent_cuff"] ?? false
                    };
                }
            }

            if (pRoot["capes"] is JObject capes)
            {
                foreach (var pair in capes)
                {
                    string mode = (string)pair.Value;
                    Capes[pair.Key] = mode == "kingdom" ? CapeMode.Kingdom : mode == "material" ? CapeMode.Material : CapeMode.None;
                }
            }

            if (pRoot["species"] is JObject species)
            {
                foreach (var pair in species)
                {
                    BodyProfile profile = ParseProfile((string)pair.Value);
                    if (pair.Key == "default") DefaultProfile = profile;
                    else Species[pair.Key] = profile;
                }
            }
        }

        public static BodyProfile ParseProfile(string pValue)
        {
            switch ((pValue ?? string.Empty).ToLowerInvariant())
            {
                case "humanoid": return BodyProfile.Humanoid;
                case "head_only": return BodyProfile.HeadOnly;
                case "auto": return BodyProfile.Auto;
                default: return BodyProfile.None;
            }
        }

        public static bool TryParseSlot(string pValue, out ArmorSlot pSlot)
        {
            switch ((pValue ?? string.Empty).ToLowerInvariant())
            {
                case "helmet": pSlot = ArmorSlot.Helmet; return true;
                case "armor": pSlot = ArmorSlot.Armor; return true;
                case "boots": pSlot = ArmorSlot.Boots; return true;
                case "amulet": pSlot = ArmorSlot.Amulet; return true;
                default: pSlot = ArmorSlot.Helmet; return false;
            }
        }

        private static int Clamp(int pValue, int pMin, int pMax) => pValue < pMin ? pMin : pValue > pMax ? pMax : pValue;
    }
}
