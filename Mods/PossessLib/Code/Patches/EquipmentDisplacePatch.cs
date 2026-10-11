using HarmonyLib;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;

namespace PossessLib.Patches
{
    /// <summary>
    /// Patch 6 (prefix + postfix, never skips): when the game replaces a piece of equipment (looting,
    /// stealing, crafting, god powers, other mods), the old item normally becomes ownerless and is
    /// deleted. For units with PossessLib data, the old item is put in their inventory instead, which is
    /// what lets units keep and carry several weapons. Does nothing for units without PossessLib data,
    /// or while PossessLib itself is swapping equipment (EquipmentGuard).
    /// Runs first: some mods (e.g. Avb's Expanded Armory) replace setItem with a prefix that returns
    /// false, and Harmony skips any prefix that would run after that.
    /// </summary>
    [HarmonyPatch(typeof(ActorEquipmentSlot), nameof(ActorEquipmentSlot.setItem))]
    internal static class EquipmentDisplacePatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ActorEquipmentSlot __instance, Item pItem, Actor pActor, out Item __state)
        {
            __state = null;
            if (EquipmentGuard.Active || pActor == null) return;
            Item old = __instance.getItem();
            if (old == null || ReferenceEquals(old, pItem) || old.isCursed()) return;
            Inventory inv = InventoryRegistry.Get(pActor);
            if (inv == null) return;
            if (inv.IndexOf(old) >= 0 || CursorState.IsHolding(old))
            {
                __state = old; // already carried (the weapon shared with a hotbar slot): just keep the owner
                return;
            }
            if (inv.FirstEmpty(false) >= 0)
            {
                inv.Insert(ItemStack.FromItem(old), false);
                __state = old;
                PLLog.Debug(pActor.getName() + " kept displaced " + old.getName(true));
            }
            else if (PLConfig.EnableGroundItems)
            {
                GroundItems.Drop(ItemStack.FromItem(old), pActor.current_position, pActor);
            }
        }

        private static void Postfix(Actor pActor, Item __state)
        {
            // setItem -> takeAwayItem cleared the old item's owner; the carrier still has it
            if (__state != null && pActor != null && pActor.isAlive()) __state.setUnitHasIt(pActor);
        }
    }
}
