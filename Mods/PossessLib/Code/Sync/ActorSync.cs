using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;

namespace PossessLib.Sync
{
    /// <summary>
    /// Reconciles an <see cref="Inventory"/> with the game's own data (see DESIGN.md section 4).
    /// <list type="bullet">
    /// <item>Resources: the ActorBag holds the totals, the inventory only holds the layout.</item>
    /// <item>Items: dead, city-stored or stolen items are removed; carried ones keep the carrier as owner.</item>
    /// <item>Weapon (possessed unit only): equipment.weapon follows the selected hotbar slot.</item>
    /// </list>
    /// </summary>
    internal static class ActorSync
    {
        /// <summary>Makes the resource totals in the slots (plus the cursor) match the unit's bag.</summary>
        public static void ReconcileResources(Inventory pInv, Actor pActor)
        {
            if (pInv == null || pActor == null || pActor.data == null) return;
            Dictionary<string, ResourceContainer> bag = pActor.data.inventory != null ? pActor.data.inventory.getResources() : null;
            int overflow = 0;

            if (bag != null)
            {
                foreach (KeyValuePair<string, ResourceContainer> pair in bag)
                {
                    string id = pair.Key;
                    if (AssetManager.resources.get(id) == null) continue;
                    int want = pair.Value.amount < 0 ? 0 : pair.Value.amount;
                    int have = pInv.CountResource(id) + CursorState.CountFor(pInv, StackKind.Resource, id);
                    if (want > have)
                    {
                        overflow += pInv.Insert(ItemStack.FromResource(id, want - have));
                    }
                    else if (want < have)
                    {
                        RemoveFromLayout(pInv, id, have - want);
                    }
                }
            }

            // Resource stacks the bag no longer has at all.
            for (int i = pInv.Capacity - 1; i >= 0; i--)
            {
                ItemStack s = pInv.Get(i);
                if (s == null || s.Kind != StackKind.Resource) continue;
                if (bag == null || !bag.ContainsKey(s.Id)) pInv.SetSlot(i, null);
            }
            if (ReferenceEquals(CursorState.Origin, pInv) && CursorState.Stack != null && CursorState.Stack.IsResource
                && (bag == null || !bag.ContainsKey(CursorState.Stack.Id)))
            {
                CursorState.Clear();
            }

            if (pInv.ResourceOverflow != overflow)
            {
                pInv.ResourceOverflow = overflow;
                pInv.Bump();
            }
        }

        /// <summary>Removes a resource amount from the slots (last slots first), then from the cursor.</summary>
        public static void RemoveFromLayout(Inventory pInv, string pResourceId, int pAmount)
        {
            int removed = pInv.RemoveAmount(StackKind.Resource, pResourceId, pAmount);
            int rest = pAmount - removed;
            if (rest > 0 && ReferenceEquals(CursorState.Origin, pInv) && CursorState.Stack != null
                && CursorState.Stack.IsResource && CursorState.Stack.Id == pResourceId)
            {
                CursorState.Stack.Count -= rest;
                CursorState.Touch();
            }
        }

        /// <summary>Drops references to items that are gone or now belong to someone else; re-claims the rest.</summary>
        public static void ValidateItems(Inventory pInv, Actor pActor)
        {
            if (pInv == null || pActor == null) return;
            for (int i = 0; i < pInv.Capacity; i++)
            {
                ItemStack s = pInv.Get(i);
                if (s == null || s.Kind != StackKind.Item) continue;
                Item item = s.Item;
                if (item == null || !item.isAlive() || item.getAsset() == null || item.hasCity())
                {
                    pInv.SetSlot(i, null);
                    continue;
                }
                Actor holder = item.getActor();
                if (holder != null && !ReferenceEquals(holder, pActor) && holder.isAlive() && ItemOwnership.IsEquippedBy(holder, item))
                {
                    PLLog.Debug(item.getName(true) + " now belongs to " + holder.getName() + ", removing it from " + pActor.getName());
                    pInv.SetSlot(i, null);
                    continue;
                }
                if (!ReferenceEquals(holder, pActor)) item.setUnitHasIt(pActor);
            }
            ItemStack trash = pInv.Trash;
            if (trash != null && trash.Kind == StackKind.Item && (trash.Item == null || !trash.Item.isAlive()))
            {
                pInv.SetTrash(null);
            }
        }

        /// <summary>
        /// Possessed unit only: equipment.weapon = selected hotbar weapon, or empty. A cursed weapon
        /// stays in the hand (the game's rule) and is reported through Inventory.PinnedCursedWeapon so
        /// the HUD can show it. Weapons the game hands to the unit are imported into the hotbar.
        /// </summary>
        public static void SyncPossessedWeapon(Inventory pInv, Actor pActor)
        {
            if (pInv == null || pActor == null) return;
            if (!pActor.hasEquipment() || !pActor.canUseItems())
            {
                pInv.PinnedCursedWeapon = null;
                return;
            }

            ActorEquipmentSlot slot = pActor.equipment.weapon;
            Item current = slot.getItem();
            ItemStack selected = pInv.SelectedStack;
            Item expected = selected != null && selected.IsWeapon && selected.IsValid ? selected.Item : null;

            if (ReferenceEquals(current, expected))
            {
                if (pInv.PinnedCursedWeapon != null)
                {
                    pInv.PinnedCursedWeapon = null;
                    pInv.Bump();
                }
                return;
            }
            PLLog.Debug("Weapon sync: hand=" + (current != null ? current.getName(true) : "empty") + " selected=" + (expected != null ? expected.getName(true) : "none"));

            if (current != null)
            {
                if (pInv.IndexOf(current) < 0 && !CursorState.IsHolding(current))
                {
                    // A weapon given by the game (loot, steal, god power, another mod): import it.
                    int target = pInv.SelectedStack == null ? pInv.Selected : pInv.FirstEmpty(true);
                    if (target >= 0)
                    {
                        pInv.SetSlot(target, ItemStack.FromItem(current));
                        if (target == pInv.Selected) return;
                    }
                    else if (!current.isCursed() && PLConfig.EnableGroundItems)
                    {
                        Unequip(slot, current, pActor);
                        GroundItems.Drop(ItemStack.FromItem(current), pActor.current_position, pActor);
                        Toasts.Show(Loc.F("possesslib_toast_full_dropped", "Inventory full: dropped $0", current.getName(true)));
                        current = null;
                    }
                    else
                    {
                        SetPinned(pInv, current);
                        return;
                    }
                }

                if (current != null)
                {
                    if (current.isCursed())
                    {
                        SetPinned(pInv, current);
                        return;
                    }
                    Unequip(slot, current, pActor);
                    // still carried in the hotbar: keep the carrier as owner
                    ItemOwnership.Claim(current, pActor);
                }
            }

            if (expected != null)
            {
                EquipmentGuard.Active = true;
                try { slot.setItem(expected, pActor); }
                finally { EquipmentGuard.Active = false; }
            }
            SetPinned(pInv, null);
            pActor.setStatsDirty();
            HandItem.Refresh();
            PossessLibEvents.RaiseEquipmentChanged(pActor, "weapon",
                current != null ? ItemStack.FromItem(current) : null, selected != null && selected.IsWeapon ? selected : null);
        }

        private static void SetPinned(Inventory pInv, Item pItem)
        {
            if (ReferenceEquals(pInv.PinnedCursedWeapon, pItem)) return;
            pInv.PinnedCursedWeapon = pItem;
            pInv.Bump();
            HandItem.Refresh();
        }

        private static void Unequip(ActorEquipmentSlot pSlot, Item pItem, Actor pActor)
        {
            EquipmentGuard.Active = true;
            try { pSlot.takeAwayItem(); }
            finally { EquipmentGuard.Active = false; }
            pActor.setStatsDirty();
        }

        /// <summary>On possession start: make sure the weapon in hand is in the hotbar and selected.</summary>
        public static void ImportEquippedWeapon(Inventory pInv, Actor pActor)
        {
            if (pInv == null || pActor == null || !pActor.hasEquipment()) return;
            Item current = pActor.equipment.weapon.getItem();
            if (current == null) return;
            int index = pInv.IndexOf(current);
            if (index < 0)
            {
                int target = pInv.SelectedStack == null ? pInv.Selected : FirstEmptyHotbar(pInv);
                if (target < 0) target = pInv.FirstEmpty(false);
                if (target < 0) return;
                pInv.SetSlot(target, ItemStack.FromItem(current));
                index = target;
            }
            if (pInv.IsHotbar(index)) pInv.Select(index);
            else pInv.Swap(index, pInv.Selected);
        }

        public static int FirstEmptyHotbar(Inventory pInv)
        {
            for (int i = 0; i < pInv.HotbarSize; i++) if (pInv.Get(i) == null) return i;
            return -1;
        }

        /// <summary>For AI units: equip the most valuable carried weapon when the hand is empty.</summary>
        public static void EquipBestWeaponIfEmpty(Inventory pInv, Actor pActor)
        {
            if (pInv == null || pActor == null || !pActor.hasEquipment() || !pActor.canUseItems()) return;
            if (!pActor.equipment.weapon.isEmpty()) return;
            Item best = null;
            for (int i = 0; i < pInv.Capacity; i++)
            {
                ItemStack s = pInv.Get(i);
                if (s == null || !s.IsWeapon || !s.IsValid) continue;
                if (best == null || s.Item.getValue() > best.getValue()) best = s.Item;
            }
            if (best == null) return;
            EquipmentGuard.Active = true;
            try { pActor.equipment.weapon.setItem(best, pActor); }
            finally { EquipmentGuard.Active = false; }
            pActor.setStatsDirty();
        }
    }
}
