using System;
using NeoModLoader.api;
using UnityEngine;

namespace PossessLib.Core
{
    /// <summary>
    /// Runtime copy of the NML config (default_config.json, group "PossessLib").
    /// NML calls the Set* callbacks when the config is loaded and whenever the player edits a value,
    /// so every field here is always current. <see cref="ReadFrom"/> is a fallback in case a callback
    /// could not be resolved.
    /// </summary>
    internal static class PLConfig
    {
        public const string Group = "PossessLib";

        public static KeyCode KeyInventory = KeyCode.I;
        public static KeyCode KeyDrop = KeyCode.X;
        public static bool ScrollSelectsHotbar = true;
        public static float UiScale = 1f;
        public static int HotbarSlots = 10;
        public static int InventoryRows = 3;
        public static int StackSize = 99;
        public static float PossessZoom = 11f;
        public static bool RestoreZoom = true;
        public static bool HideNativeTips = true;
        public static bool EnableEating = true;
        public static bool EnableGathering = true;
        public static bool EnableCrafting = true;
        public static bool EnableGroundItems = true;
        public static float GroundDespawnMinutes = 10f;
        public static bool DropOnDeath = true;
        public static bool AiWeaponSwitch = true;
        public static bool Debug;

        /// <summary>Raised after any value changes, so the HUD can re-layout.</summary>
        public static event Action Changed;

        public static int MainSlots => Mathf.Clamp(InventoryRows, 1, 6) * Mathf.Clamp(HotbarSlots, 1, 10);

        // ---- NML callbacks (referenced by name from default_config.json) ----
        public static void SetKeyInventory(string pValue) { KeyInventory = ParseKey(pValue, KeyCode.I); Notify(); }
        public static void SetKeyDrop(string pValue) { KeyDrop = ParseKey(pValue, KeyCode.X); Notify(); }
        public static void SetScrollHotbar(bool pValue) { ScrollSelectsHotbar = pValue; Notify(); }
        public static void SetUiScale(float pValue) { UiScale = Mathf.Clamp(pValue, 0.5f, 2f); Notify(); }
        public static void SetHotbarSlots(int pValue) { HotbarSlots = Mathf.Clamp(pValue, 6, 10); Notify(); }
        public static void SetInventoryRows(int pValue) { InventoryRows = Mathf.Clamp(pValue, 1, 5); Notify(); }
        public static void SetStackSize(int pValue) { StackSize = Mathf.Clamp(pValue, 10, 9999); Notify(); }
        public static void SetPossessZoom(float pValue) { PossessZoom = Mathf.Clamp(pValue, 10f, 60f); Notify(); }
        public static void SetRestoreZoom(bool pValue) { RestoreZoom = pValue; Notify(); }
        public static void SetHideNativeTips(bool pValue) { HideNativeTips = pValue; Notify(); }
        public static void SetEnableEating(bool pValue) { EnableEating = pValue; Notify(); }
        public static void SetEnableGathering(bool pValue) { EnableGathering = pValue; Notify(); }
        public static void SetEnableCrafting(bool pValue) { EnableCrafting = pValue; Notify(); }
        public static void SetEnableGroundItems(bool pValue) { EnableGroundItems = pValue; Notify(); }
        public static void SetGroundDespawn(float pValue) { GroundDespawnMinutes = Mathf.Clamp(pValue, 1f, 120f); Notify(); }
        public static void SetDropOnDeath(bool pValue) { DropOnDeath = pValue; Notify(); }
        public static void SetAiWeaponSwitch(bool pValue) { AiWeaponSwitch = pValue; Notify(); }
        public static void SetDebug(bool pValue) { Debug = pValue; Notify(); }

        private static void Notify()
        {
            try { Changed?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogError("[PossessLib] config listener failed: " + e); }
        }

        /// <summary>Reads every value straight from the NML config object.</summary>
        public static void ReadFrom(ModConfig pConfig)
        {
            if (pConfig == null) return;
            try
            {
                var group = pConfig[Group];
                foreach (var pair in group)
                {
                    object value = pair.Value.GetValue();
                    switch (pair.Key)
                    {
                        case "pl_key_inventory": SetKeyInventory(value as string); break;
                        case "pl_key_drop": SetKeyDrop(value as string); break;
                        case "pl_scroll_hotbar": SetScrollHotbar((bool)value); break;
                        case "pl_ui_scale": SetUiScale((float)value); break;
                        case "pl_hotbar_slots": SetHotbarSlots((int)value); break;
                        case "pl_inventory_rows": SetInventoryRows((int)value); break;
                        case "pl_stack_size": SetStackSize((int)value); break;
                        case "pl_possess_zoom": SetPossessZoom((float)value); break;
                        case "pl_restore_zoom": SetRestoreZoom((bool)value); break;
                        case "pl_hide_native_tips": SetHideNativeTips((bool)value); break;
                        case "pl_enable_eating": SetEnableEating((bool)value); break;
                        case "pl_enable_gathering": SetEnableGathering((bool)value); break;
                        case "pl_enable_crafting": SetEnableCrafting((bool)value); break;
                        case "pl_enable_ground_items": SetEnableGroundItems((bool)value); break;
                        case "pl_ground_despawn_minutes": SetGroundDespawn((float)value); break;
                        case "pl_drop_on_death": SetDropOnDeath((bool)value); break;
                        case "pl_ai_weapon_switch": SetAiWeaponSwitch((bool)value); break;
                        case "pl_debug": SetDebug((bool)value); break;
                    }
                }
            }
            catch (Exception e)
            {
                PLLog.Warn("Could not read config, using defaults: " + e.Message);
            }
        }

        /// <summary>Accepts Unity KeyCode names ("I", "Tab", "Mouse3") and plain digits ("1" = Alpha1).</summary>
        public static KeyCode ParseKey(string pText, KeyCode pFallback)
        {
            if (string.IsNullOrWhiteSpace(pText)) return pFallback;
            string text = pText.Trim();
            if (text.Length == 1 && char.IsDigit(text[0])) text = "Alpha" + text;
            return Enum.TryParse(text, true, out KeyCode key) ? key : pFallback;
        }
    }
}
