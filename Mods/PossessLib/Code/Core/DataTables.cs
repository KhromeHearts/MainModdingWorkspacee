using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PossessLib.Core
{
    // ---------------------------------------------------------------- JSON shapes (Data/*.json)
    // Fields are filled by Newtonsoft through reflection.
#pragma warning disable CS0649

    internal sealed class FoodEntry
    {
        [JsonProperty("cooldown")] public float? Cooldown;
        [JsonProperty("bonus_heal_flat")] public int? BonusHealFlat;
        [JsonProperty("bonus_heal_percent")] public float? BonusHealPercent;
    }

    internal sealed class FoodFile
    {
        [JsonProperty("version")] public int Version = 1;
        [JsonProperty("allow_any_game_food")] public bool AllowAnyGameFood = true;
        [JsonProperty("default")] public FoodEntry Default = new FoodEntry();
        [JsonProperty("foods")] public Dictionary<string, FoodEntry> Foods = new Dictionary<string, FoodEntry>();
        [JsonProperty("blocked")] public List<string> Blocked = new List<string>();
    }

    internal sealed class GatherRule
    {
        [JsonProperty("hits")] public int Hits = 1;
        [JsonProperty("hits_with_tool")] public int HitsWithTool = 1;
        [JsonProperty("tool_groups")] public List<string> ToolGroups = new List<string>();
        [JsonProperty("experience")] public int Experience;
        [JsonProperty("hit_sound")] public string HitSound;
        [JsonProperty("finish_sound")] public string FinishSound;
        [JsonProperty("requires_sapient")] public bool RequiresSapient;
    }

    internal sealed class GatheringFile
    {
        [JsonProperty("version")] public int Version = 1;
        [JsonProperty("reach_bonus")] public float ReachBonus = 2f;
        [JsonProperty("progress_timeout")] public float ProgressTimeout = 8f;
        [JsonProperty("rules")] public Dictionary<string, GatherRule> Rules = new Dictionary<string, GatherRule>();
        [JsonProperty("building_overrides")] public Dictionary<string, GatherRule> BuildingOverrides = new Dictionary<string, GatherRule>();
    }

    internal sealed class RecipeEntry
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("result_item")] public string ResultItem;
        [JsonProperty("result_resource")] public string ResultResource;
        [JsonProperty("result_custom")] public string ResultCustom;
        [JsonProperty("count")] public int Count = 1;
        [JsonProperty("resources")] public Dictionary<string, int> Resources = new Dictionary<string, int>();
        [JsonProperty("custom_items")] public Dictionary<string, int> CustomItems = new Dictionary<string, int>();
        [JsonProperty("money")] public int Money;
        [JsonProperty("near_buildings")] public List<string> NearBuildings;
        [JsonProperty("station_radius")] public float StationRadius = 6f;
        [JsonProperty("category")] public string Category = "misc";
    }

    internal sealed class RecipeFile
    {
        [JsonProperty("version")] public int Version = 1;
        [JsonProperty("auto_from_game_items")] public bool AutoFromGameItems = true;
        [JsonProperty("auto_groups")] public List<string> AutoGroups = new List<string>();
        [JsonProperty("auto_require_unlocked")] public bool AutoRequireUnlocked = true;
        [JsonProperty("auto_cost_multiplier")] public float AutoCostMultiplier = 1f;
        [JsonProperty("auto_charge_money")] public bool AutoChargeMoney = true;
        [JsonProperty("auto_near_buildings")] public List<string> AutoNearBuildings;
        [JsonProperty("disabled")] public List<string> Disabled = new List<string>();
        [JsonProperty("recipes")] public List<RecipeEntry> Recipes = new List<RecipeEntry>();
    }
#pragma warning restore CS0649

    /// <summary>Loads the data-driven tables from the mod's Data folder. Missing or broken files fall back to defaults.</summary>
    internal static class DataTables
    {
        public static FoodFile Food = new FoodFile();
        public static GatheringFile Gathering = new GatheringFile();
        public static RecipeFile Recipes = new RecipeFile();

        public static void LoadAll(string pModFolder)
        {
            string dir = Path.Combine(pModFolder, "Data");
            Food = Load(Path.Combine(dir, "food.json"), Food);
            Gathering = Load(Path.Combine(dir, "gathering.json"), Gathering);
            Recipes = Load(Path.Combine(dir, "recipes.json"), Recipes);
            PLLog.Info("Data loaded: " + Food.Foods.Count + " food entries, " + Gathering.Rules.Count + " gathering rules, "
                       + Recipes.Recipes.Count + " hand-written recipes");
        }

        private static T Load<T>(string pPath, T pFallback) where T : class
        {
            try
            {
                if (!File.Exists(pPath))
                {
                    PLLog.Warn("Missing data file " + pPath + ", using defaults");
                    return pFallback;
                }
                T result = JsonConvert.DeserializeObject<T>(File.ReadAllText(pPath));
                return result ?? pFallback;
            }
            catch (Exception e)
            {
                PLLog.Error("Could not read " + pPath + ": " + e.Message + " (using defaults)");
                return pFallback;
            }
        }

        // ---------------------------------------------------------------- lookups

        public static bool IsEdible(ResourceAsset pAsset)
        {
            if (pAsset == null) return false;
            if (Food.Blocked != null && Food.Blocked.Contains(pAsset.id)) return false;
            if (Food.Foods != null && Food.Foods.ContainsKey(pAsset.id)) return true;
            return Food.AllowAnyGameFood && pAsset.food;
        }

        public static void GetFood(string pId, out float pCooldown, out int pFlat, out float pPercent)
        {
            FoodEntry def = Food.Default ?? new FoodEntry();
            FoodEntry entry = null;
            if (Food.Foods != null) Food.Foods.TryGetValue(pId, out entry);
            pCooldown = entry?.Cooldown ?? def.Cooldown ?? 0.8f;
            pFlat = entry?.BonusHealFlat ?? def.BonusHealFlat ?? 0;
            pPercent = entry?.BonusHealPercent ?? def.BonusHealPercent ?? 0f;
        }

        public static GatherRule GetGatherRule(BuildingAsset pAsset)
        {
            if (pAsset == null) return null;
            if (Gathering.BuildingOverrides != null && Gathering.BuildingOverrides.TryGetValue(pAsset.id, out GatherRule over)) return over;
            if (Gathering.Rules != null && Gathering.Rules.TryGetValue(pAsset.building_type.ToString(), out GatherRule rule)) return rule;
            return null;
        }
    }
}
