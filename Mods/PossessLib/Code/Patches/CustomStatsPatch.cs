using System.Collections.Generic;
using HarmonyLib;
using PossessLib.Api;
using PossessLib.Model;
using PossessLib.Sync;

namespace PossessLib.Patches
{
    /// <summary>
    /// Patch 7 (prefix + postfix): adds the EquippedStats of custom items worn in extra equipment
    /// slots (registered by other mods) after the game recomputes a unit's stats. Fast path: does
    /// nothing until some mod registers an extra slot, and only when the stats were actually rebuilt.
    /// </summary>
    [HarmonyPatch(typeof(Actor), nameof(Actor.updateStats))]
    internal static class CustomStatsPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(Actor __instance, out bool __state)
        {
            __state = EquipSlotRegistry.AnyStatSlots && __instance.isStatsDirty();
        }

        private static void Postfix(Actor __instance, bool __state)
        {
            if (!__state) return;
            Inventory inv = InventoryRegistry.Get(__instance);
            if (inv == null || inv.CustomEquipmentCount == 0) return;
            bool any = false;
            foreach (KeyValuePair<string, ItemStack> pair in inv.CustomEquipment)
            {
                CustomItemDefinition def = pair.Value.CustomDefinition;
                if (def == null || def.EquippedStats == null) continue;
                __instance.stats.mergeStats(def.EquippedStats, 1f);
                any = true;
            }
            if (any) __instance.stats.normalize();
        }
    }
}
