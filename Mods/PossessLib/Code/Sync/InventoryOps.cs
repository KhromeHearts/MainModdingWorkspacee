using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using UnityEngine;

namespace PossessLib.Sync
{
    /// <summary>
    /// Operations that change an inventory and the game state together, so the per-frame reconcile
    /// never has anything to fix after PossessLib's own actions. UI, gameplay systems and the public
    /// API all go through here.
    /// </summary>
    internal static class InventoryOps
    {
        // ------------------------------------------------------------------ adding

        /// <summary>Adds resources to the bag and lays them out in slots. Returns the amount that got a slot.</summary>
        public static int AddResource(Actor pActor, Inventory pInv, string pResourceId, int pAmount)
        {
            if (pActor == null || pInv == null || pAmount <= 0 || AssetManager.resources.get(pResourceId) == null) return 0;
            pActor.addToInventory(pResourceId, pAmount);
            int leftover = pInv.Insert(ItemStack.FromResource(pResourceId, pAmount));
            if (leftover > 0)
            {
                pInv.ResourceOverflow += leftover;
                pInv.Bump();
            }
            return pAmount - leftover;
        }

        /// <summary>Removes resources from bag and slots. Fails (and changes nothing) if the unit has too few.</summary>
        public static bool RemoveResource(Actor pActor, Inventory pInv, string pResourceId, int pAmount)
        {
            if (pActor == null || pInv == null || pAmount <= 0) return false;
            if (pActor.data.inventory.getResource(pResourceId) < pAmount) return false;
            pActor.takeFromInventory(pResourceId, pAmount);
            ActorSync.RemoveFromLayout(pInv, pResourceId, pAmount);
            return true;
        }

        /// <summary>Adds a game item. Returns false (and changes nothing) when there is no free slot.</summary>
        public static bool AddItem(Actor pActor, Inventory pInv, Item pItem)
        {
            if (pActor == null || pInv == null || pItem == null || !pItem.isAlive()) return false;
            if (pInv.IndexOf(pItem) >= 0) return true;
            if (pInv.FirstEmpty(true) < 0) return false;
            if (pItem.hasCity()) pItem.clearCity();
            pInv.Insert(ItemStack.FromItem(pItem));
            pItem.setUnitHasIt(pActor);
            return true;
        }

        /// <summary>Adds custom items. Returns the amount that did not fit.</summary>
        public static int AddCustom(Inventory pInv, string pCustomId, int pAmount, System.Collections.Generic.Dictionary<string, string> pData)
        {
            if (pInv == null || pAmount <= 0 || !CustomItemRegistry.Has(pCustomId)) return pAmount;
            return pInv.Insert(ItemStack.FromCustom(pCustomId, pAmount, pData));
        }

        /// <summary>
        /// Gives a stack to a unit. Resources also enter its bag; items become owned by it. What does
        /// not fit is returned (null when everything fit).
        /// </summary>
        public static ItemStack GiveStack(Actor pActor, Inventory pInv, ItemStack pStack)
        {
            if (pStack == null) return null;
            switch (pStack.Kind)
            {
                case StackKind.Resource:
                {
                    int space = pInv.SpaceFor(pStack);
                    int amount = Mathf.Min(space, pStack.Count);
                    if (amount > 0) AddResource(pActor, pInv, pStack.Id, amount);
                    pStack.Count -= amount;
                    return pStack.Count > 0 ? pStack : null;
                }
                case StackKind.Item:
                    return AddItem(pActor, pInv, pStack.Item) ? null : pStack;
                default:
                {
                    int left = pInv.Insert(pStack);
                    if (left <= 0) return null;
                    pStack.Count = left;
                    return pStack;
                }
            }
        }

        // ------------------------------------------------------------------ moving between units

        /// <summary>Moves ownership of a stack that is in transit (on the cursor) from one unit to another.</summary>
        public static void Transfer(ItemStack pStack, Inventory pFrom, Inventory pTo)
        {
            if (pStack == null || ReferenceEquals(pFrom, pTo) || pTo == null) return;
            Actor from = pFrom != null ? pFrom.Owner : null;
            Actor to = pTo.Owner;
            switch (pStack.Kind)
            {
                case StackKind.Resource:
                    if (from != null) from.takeFromInventory(pStack.Id, pStack.Count);
                    if (to != null) to.addToInventory(pStack.Id, pStack.Count);
                    break;
                case StackKind.Item:
                    if (to != null) pStack.Item.setUnitHasIt(to);
                    break;
            }
        }

        // ------------------------------------------------------------------ leaving the unit

        /// <summary>Takes a stack out of the unit for good (trash, drop, use): resources leave the bag.</summary>
        public static void DetachFromOwner(ItemStack pStack, Inventory pOwnerInv)
        {
            if (pStack == null) return;
            Actor owner = pOwnerInv != null ? pOwnerInv.Owner : null;
            if (pStack.Kind == StackKind.Resource && owner != null) owner.takeFromInventory(pStack.Id, pStack.Count);
            if (pStack.Kind == StackKind.Item && pStack.Item != null && ReferenceEquals(pStack.Item.getActor(), owner))
            {
                // never strip an item that is still equipped (the cursed one in hand)
                if (owner == null || !ItemOwnership.IsEquippedBy(owner, pStack.Item)) pStack.Item.clearUnit();
            }
        }

        /// <summary>
        /// A weapon is shared between a hotbar slot and the hand. Before it leaves the unit (trash,
        /// drop) it is taken out of the hand. A cursed weapon cannot be, so the action is refused.
        /// </summary>
        public static bool TryReleaseFromHand(Actor pOwner, ItemStack pStack)
        {
            if (pStack == null || pStack.Kind != StackKind.Item || pStack.Item == null || pOwner == null || !pOwner.hasEquipment()) return true;
            ActorEquipmentSlot slot = pOwner.equipment.weapon;
            if (!ReferenceEquals(slot.getItem(), pStack.Item)) return true;
            if (pStack.Item.isCursed())
            {
                Toasts.Show(Loc.F("possesslib_toast_cursed_weapon", "Cursed! You can't let go of $0.", pStack.Item.getName(true)));
                UISound.Play(UISound.Deny);
                return false;
            }
            EquipmentGuard.Active = true;
            try { slot.takeAwayItem(); }
            finally { EquipmentGuard.Active = false; }
            pStack.Item.setUnitHasIt(pOwner);
            pOwner.setStatsDirty();
            return true;
        }

        /// <summary>Puts a stack in the trash slot. The previous trash content is destroyed.</summary>
        public static bool Trash(Inventory pInv, ItemStack pStack, Inventory pStackOwner)
        {
            if (pInv == null || pStack == null) return false;
            if (!TryReleaseFromHand(pStackOwner != null ? pStackOwner.Owner : null, pStack)) return false;
            DetachFromOwner(pStack, pStackOwner);
            ItemStack old = pInv.Trash;
            pInv.SetTrash(pStack);
            DestroyStack(old);
            UISound.Play(UISound.Trash);
            return true;
        }

        /// <summary>Takes the trash content back (resources re-enter the bag).</summary>
        public static ItemStack TakeFromTrash(Inventory pInv)
        {
            ItemStack stack = pInv != null ? pInv.Trash : null;
            if (stack == null) return null;
            pInv.SetTrash(null);
            Actor owner = pInv.Owner;
            if (owner != null)
            {
                if (stack.Kind == StackKind.Resource) owner.addToInventory(stack.Id, stack.Count);
                if (stack.Kind == StackKind.Item && stack.Item != null) stack.Item.setUnitHasIt(owner);
            }
            return stack;
        }

        /// <summary>
        /// Lets the game forget a stack that PossessLib no longer keeps: the item loses its carrier and
        /// the item manager removes it on its next cleanup. Equipped or still-protected items are left alone.
        /// </summary>
        public static void DestroyStack(ItemStack pStack)
        {
            if (pStack == null || pStack.Kind != StackKind.Item || pStack.Item == null) return;
            Item item = pStack.Item;
            if (InventoryRegistry.IsProtected(item) || !item.isAlive()) return;
            Actor holder = item.getActor();
            if (holder != null && holder.isAlive() && ItemOwnership.IsEquippedBy(holder, item)) return;
            item.clearUnit(); // also marks the item manager dirty
        }

        /// <summary>Drops a stack on the ground at the owner's feet (or destroys it when ground items are off).</summary>
        public static bool Drop(ItemStack pStack, Inventory pOwnerInv, Vector2 pPosition)
        {
            if (pStack == null) return false;
            if (!TryReleaseFromHand(pOwnerInv != null ? pOwnerInv.Owner : null, pStack)) return false;
            DetachFromOwner(pStack, pOwnerInv);
            Actor owner = pOwnerInv != null ? pOwnerInv.Owner : null;
            if (PLConfig.EnableGroundItems)
            {
                GroundItems.Drop(pStack, pPosition, owner);
            }
            else
            {
                DestroyStack(pStack);
            }
            PossessLibEvents.RaiseItemDropped(owner, pStack, pPosition);
            return true;
        }

        // ------------------------------------------------------------------ equipment

        /// <summary>Equips a wearable from a slot; the displaced item takes its place. Respects curses.</summary>
        public static bool EquipFromSlot(Inventory pInv, int pSlot)
        {
            Actor actor = pInv != null ? pInv.Owner : null;
            ItemStack stack = pInv != null ? pInv.Get(pSlot) : null;
            if (actor == null || stack == null) return false;
            if (stack.Kind == StackKind.Custom) return EquipCustomFromSlot(pInv, pSlot, actor, stack);
            if (!stack.IsWearable) return false;
            if (!CanUseEquipment(actor)) return false;

            ActorEquipmentSlot target = actor.equipment.getSlot(stack.EquipmentAsset.equipment_type);
            if (!target.canChangeSlot())
            {
                Toasts.Show(Loc.F("possesslib_toast_cursed_slot", "Cursed! $0 can't be taken off.", target.getItem().getName(true)));
                UISound.Play(UISound.Deny);
                return false;
            }
            Item old = target.getItem();
            pInv.SetSlot(pSlot, null);
            SetEquipment(target, stack.Item, actor);
            if (old != null)
            {
                pInv.SetSlot(pSlot, ItemStack.FromItem(old));
            }
            PossessLibEvents.RaiseEquipmentChanged(actor, EquipSlotRegistry.VanillaId(target.type),
                old != null ? ItemStack.FromItem(old) : null, stack);
            UISound.Play(UISound.Equip);
            return true;
        }

        /// <summary>Equips the stack on the cursor into a vanilla slot. The displaced item goes to the inventory (or the cursor).</summary>
        public static bool EquipFromCursor(Inventory pInv, EquipmentType pType)
        {
            Actor actor = pInv != null ? pInv.Owner : null;
            ItemStack stack = CursorState.Stack;
            if (actor == null || stack == null || !stack.IsWearable || stack.EquipmentAsset.equipment_type != pType) return false;
            if (!CanUseEquipment(actor)) return false;
            ActorEquipmentSlot target = actor.equipment.getSlot(pType);
            if (!target.canChangeSlot())
            {
                Toasts.Show(Loc.F("possesslib_toast_cursed_slot", "Cursed! $0 can't be taken off.", target.getItem().getName(true)));
                UISound.Play(UISound.Deny);
                return false;
            }
            Transfer(stack, CursorState.Origin, pInv);
            CursorState.Clear();
            Item old = target.getItem();
            SetEquipment(target, stack.Item, actor);
            if (old != null)
            {
                ItemStack oldStack = ItemStack.FromItem(old);
                if (pInv.Insert(oldStack) > 0) CursorState.Set(oldStack, pInv);
            }
            PossessLibEvents.RaiseEquipmentChanged(actor, EquipSlotRegistry.VanillaId(pType), old != null ? ItemStack.FromItem(old) : null, stack);
            UISound.Play(UISound.Equip);
            return true;
        }

        /// <summary>Takes a vanilla equipment piece off into the inventory (or onto the cursor).</summary>
        public static bool Unequip(Inventory pInv, EquipmentType pType, bool pToCursor)
        {
            Actor actor = pInv != null ? pInv.Owner : null;
            if (actor == null || !actor.hasEquipment()) return false;
            ActorEquipmentSlot slot = actor.equipment.getSlot(pType);
            Item item = slot.getItem();
            if (item == null) return false;
            if (!slot.canChangeSlot())
            {
                Toasts.Show(Loc.F("possesslib_toast_cursed_slot", "Cursed! $0 can't be taken off.", item.getName(true)));
                UISound.Play(UISound.Deny);
                return false;
            }
            ItemStack stack = ItemStack.FromItem(item);
            if (!pToCursor && pInv.FirstEmpty(false) < 0)
            {
                Toasts.Show(Loc.T("possesslib_toast_inventory_full", "Inventory full"));
                return false;
            }
            EquipmentGuard.Active = true;
            try { slot.takeAwayItem(); }
            finally { EquipmentGuard.Active = false; }
            item.setUnitHasIt(actor);
            actor.setStatsDirty();
            if (pToCursor) CursorState.Set(stack, pInv);
            else pInv.Insert(stack, false);
            PossessLibEvents.RaiseEquipmentChanged(actor, EquipSlotRegistry.VanillaId(pType), stack, null);
            UISound.Play(UISound.PickUp);
            return true;
        }

        private static void SetEquipment(ActorEquipmentSlot pSlot, Item pItem, Actor pActor)
        {
            EquipmentGuard.Active = true;
            try { pSlot.setItem(pItem, pActor); }
            finally { EquipmentGuard.Active = false; }
            pActor.setStatsDirty();
        }

        public static bool CanUseEquipment(Actor pActor)
        {
            if (pActor != null && pActor.hasEquipment() && pActor.canUseItems()) return true;
            Toasts.Show(Loc.T("possesslib_toast_no_equipment", "This creature can't use equipment."));
            UISound.Play(UISound.Deny);
            return false;
        }

        // ---- custom equipment slots

        private static bool EquipCustomFromSlot(Inventory pInv, int pSlot, Actor pActor, ItemStack pStack)
        {
            CustomItemDefinition def = pStack.CustomDefinition;
            if (def == null || string.IsNullOrEmpty(def.EquipSlotId)) return false;
            EquipmentSlotDefinition slotDef = EquipSlotRegistry.GetCustom(def.EquipSlotId);
            if (slotDef == null || !slotDef.AcceptsStack(pStack)) return false;
            ItemStack single = pStack.Count > 1 ? pStack.CloneWithCount(1) : pStack;
            if (pStack.Count > 1) pInv.ChangeCount(pSlot, -1);
            else pInv.SetSlot(pSlot, null);
            ItemStack old = pInv.GetCustomEquipment(slotDef.Id);
            pInv.SetCustomEquipment(slotDef.Id, single);
            if (old != null)
            {
                SafeCall(slotDef.OnUnequipped, pActor, old);
                if (pInv.Insert(old) > 0) CursorState.Set(old, pInv);
            }
            SafeCall(slotDef.OnEquipped, pActor, single);
            pActor.setStatsDirty();
            UISound.Play(UISound.Equip);
            return true;
        }

        /// <summary>Places the cursor stack in a custom slot (swapping with what was there).</summary>
        public static bool EquipCustomFromCursor(Inventory pInv, EquipmentSlotDefinition pSlotDef)
        {
            Actor actor = pInv != null ? pInv.Owner : null;
            ItemStack stack = CursorState.Stack;
            if (actor == null || pSlotDef == null || stack == null || !pSlotDef.AcceptsStack(stack)) return false;
            Transfer(stack, CursorState.Origin, pInv);
            ItemStack single;
            if (stack.Count > 1)
            {
                single = stack.CloneWithCount(1);
                stack.Count -= 1;
                CursorState.Touch();
            }
            else
            {
                single = stack;
                CursorState.Clear();
            }
            ItemStack old = pInv.GetCustomEquipment(pSlotDef.Id);
            pInv.SetCustomEquipment(pSlotDef.Id, single);
            if (old != null)
            {
                SafeCall(pSlotDef.OnUnequipped, actor, old);
                if (CursorState.IsEmpty) CursorState.Set(old, pInv);
                else if (pInv.Insert(old) > 0) Drop(old, pInv, actor.current_position);
            }
            SafeCall(pSlotDef.OnEquipped, actor, single);
            actor.setStatsDirty();
            UISound.Play(UISound.Equip);
            return true;
        }

        /// <summary>Takes a custom slot's content onto the cursor.</summary>
        public static bool UnequipCustomToCursor(Inventory pInv, EquipmentSlotDefinition pSlotDef)
        {
            Actor actor = pInv != null ? pInv.Owner : null;
            ItemStack old = pInv != null ? pInv.GetCustomEquipment(pSlotDef.Id) : null;
            if (actor == null || old == null || !CursorState.IsEmpty) return false;
            pInv.SetCustomEquipment(pSlotDef.Id, null);
            SafeCall(pSlotDef.OnUnequipped, actor, old);
            CursorState.Set(old, pInv);
            actor.setStatsDirty();
            UISound.Play(UISound.PickUp);
            return true;
        }

        private static void SafeCall(System.Action<Actor, ItemStack> pCallback, Actor pActor, ItemStack pStack)
        {
            if (pCallback == null) return;
            try { pCallback(pActor, pStack); }
            catch (System.Exception e) { PLLog.Exception("Equipment slot callback", e); }
        }
    }
}
