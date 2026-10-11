using System;
using PossessLib.Core;
using PossessLib.Model;
using UnityEngine;

namespace PossessLib.Api
{
    /// <summary>
    /// Events other mods can subscribe to. WorldBox has no hooks for any of these (equipment and
    /// bag changes are plain field writes; possession only toggles a UI), so PossessLib raises them.
    /// Every handler is called inside its own try/catch: a failing handler is logged and skipped.
    /// </summary>
    public static class PossessLibEvents
    {
        /// <summary>A unit became the possessed (main controlled) unit.</summary>
        public static event Action<Actor> UnitPossessed;

        /// <summary>A unit stopped being the possessed unit (released, died, world cleared). The actor may already be dead.</summary>
        public static event Action<Actor> UnitReleased;

        /// <summary>The inventory panel was opened for the possessed unit.</summary>
        public static event Action<Actor, Inventory> InventoryOpened;

        /// <summary>The inventory panel was closed.</summary>
        public static event Action<Actor, Inventory> InventoryClosed;

        /// <summary>A stack was placed in a slot or a stack grew. Args: inventory, slot index, stack, amount added.</summary>
        public static event Action<Inventory, int, ItemStack, int> ItemAdded;

        /// <summary>A stack left a slot or a stack shrank. Args: inventory, slot index, stack, amount removed.</summary>
        public static event Action<Inventory, int, ItemStack, int> ItemRemoved;

        /// <summary>Two slots swapped content. Args: inventory, from slot, to slot.</summary>
        public static event Action<Inventory, int, int> ItemMoved;

        /// <summary>The possessed unit used (ate, drank, activated) an item.</summary>
        public static event Action<Actor, ItemStack> ItemUsed;

        /// <summary>The selected hotbar slot changed. Args: inventory, old index, new index.</summary>
        public static event Action<Inventory, int, int> HotbarSlotSelected;

        /// <summary>
        /// Equipment changed through PossessLib. Args: actor, slot id ("weapon", "helmet", "armor",
        /// "boots", "ring", "amulet" or a custom slot id), old stack (may be null), new stack (may be null).
        /// </summary>
        public static event Action<Actor, string, ItemStack, ItemStack> EquipmentChanged;

        /// <summary>A stack was dropped on the ground. Args: actor (may be null), stack, world position.</summary>
        public static event Action<Actor, ItemStack, Vector2> ItemDropped;

        /// <summary>A unit picked up a ground stack.</summary>
        public static event Action<Actor, ItemStack> ItemPickedUp;

        /// <summary>A recipe was crafted. Args: actor, recipe, produced stack.</summary>
        public static event Action<Actor, RecipeDefinition, ItemStack> ItemCrafted;

        /// <summary>The possessed unit gained a level. Args: actor, new level.</summary>
        public static event Action<Actor, int> PossessedLevelUp;

        /// <summary>The possessed unit finished gathering a building. Args: actor, building, resource id, amount.</summary>
        public static event Action<Actor, Building, string, int> ResourceGathered;

        internal static void RaiseUnitPossessed(Actor a) => Invoke(UnitPossessed, h => ((Action<Actor>)h)(a));
        internal static void RaiseUnitReleased(Actor a) => Invoke(UnitReleased, h => ((Action<Actor>)h)(a));
        internal static void RaiseInventoryOpened(Actor a, Inventory i) => Invoke(InventoryOpened, h => ((Action<Actor, Inventory>)h)(a, i));
        internal static void RaiseInventoryClosed(Actor a, Inventory i) => Invoke(InventoryClosed, h => ((Action<Actor, Inventory>)h)(a, i));
        internal static void RaiseItemUsed(Actor a, ItemStack s) => Invoke(ItemUsed, h => ((Action<Actor, ItemStack>)h)(a, s));
        internal static void RaiseItemDropped(Actor a, ItemStack s, Vector2 p) => Invoke(ItemDropped, h => ((Action<Actor, ItemStack, Vector2>)h)(a, s, p));
        internal static void RaiseItemPickedUp(Actor a, ItemStack s) => Invoke(ItemPickedUp, h => ((Action<Actor, ItemStack>)h)(a, s));
        internal static void RaiseItemCrafted(Actor a, RecipeDefinition r, ItemStack s) => Invoke(ItemCrafted, h => ((Action<Actor, RecipeDefinition, ItemStack>)h)(a, r, s));
        internal static void RaisePossessedLevelUp(Actor a, int l) => Invoke(PossessedLevelUp, h => ((Action<Actor, int>)h)(a, l));
        internal static void RaiseResourceGathered(Actor a, Building b, string id, int n) => Invoke(ResourceGathered, h => ((Action<Actor, Building, string, int>)h)(a, b, id, n));

        internal static void RaiseItemAdded(Inventory i, int slot, ItemStack s, int n)
        {
            if (ItemAdded != null) Invoke(ItemAdded, h => ((Action<Inventory, int, ItemStack, int>)h)(i, slot, s, n));
        }

        internal static void RaiseItemRemoved(Inventory i, int slot, ItemStack s, int n)
        {
            if (ItemRemoved != null) Invoke(ItemRemoved, h => ((Action<Inventory, int, ItemStack, int>)h)(i, slot, s, n));
        }

        internal static void RaiseItemMoved(Inventory i, int from, int to)
        {
            if (ItemMoved != null) Invoke(ItemMoved, h => ((Action<Inventory, int, int>)h)(i, from, to));
        }

        internal static void RaiseHotbarSlotSelected(Inventory i, int oldIndex, int newIndex)
        {
            if (HotbarSlotSelected != null) Invoke(HotbarSlotSelected, h => ((Action<Inventory, int, int>)h)(i, oldIndex, newIndex));
        }

        internal static void RaiseEquipmentChanged(Actor a, string slot, ItemStack oldStack, ItemStack newStack)
        {
            if (EquipmentChanged != null) Invoke(EquipmentChanged, h => ((Action<Actor, string, ItemStack, ItemStack>)h)(a, slot, oldStack, newStack));
        }

        private static void Invoke(Delegate pEvent, Action<Delegate> pCall)
        {
            if (pEvent == null) return;
            foreach (Delegate handler in pEvent.GetInvocationList())
            {
                try
                {
                    pCall(handler);
                }
                catch (Exception e)
                {
                    PLLog.Error("Event handler " + handler.Method.DeclaringType + "." + handler.Method.Name + " failed: " + e);
                }
            }
        }
    }
}
