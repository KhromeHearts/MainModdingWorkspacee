using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PossessLib.UI
{
    /// <summary>
    /// Terraria-style slot interaction. Everything goes through one "cursor stack", which gives both
    /// click-to-pick-up/click-to-place and drag-and-drop:
    /// <list type="bullet">
    /// <item>Left click: pick up a stack, or place / merge / swap the held stack.</item>
    /// <item>Drag a stack and release on a slot: the same as clicking that slot. Release outside the UI: drop it on the ground.</item>
    /// <item>Right click: take half a stack; with a matching stack held, place one; on gear, equip it.</item>
    /// <item>Shift + left click: equip gear, otherwise quick-move between hotbar and main grid.</item>
    /// <item>Ctrl + left click: send to the trash slot.</item>
    /// </list>
    /// </summary>
    internal static class InventoryActions
    {
        private static bool Shift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private static bool Ctrl => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        public static void OnSlotPointerDown(SlotView pView, PointerEventData.InputButton pButton)
        {
            SlotAddress a = pView.Address;
            if (a.Inventory == null || a.Inventory.Disposed) return;
            Tooltip.hideTooltip();
            switch (a.Kind)
            {
                case SlotKind.Inventory:
                    if (a.HudHotbar && !PossessionController.InventoryOpen)
                    {
                        if (pButton == PointerEventData.InputButton.Left) SelectHotbar(a.Inventory, a.Index);
                        return;
                    }
                    InventoryClick(a.Inventory, a.Index, pButton);
                    break;
                case SlotKind.Equipment:
                    EquipmentClick(a.Inventory, a.EquipType, pButton);
                    break;
                case SlotKind.CustomEquipment:
                    CustomEquipmentClick(a.Inventory, a.Custom, pButton);
                    break;
                case SlotKind.Trash:
                    TrashClick(a.Inventory);
                    break;
            }
        }

        // ------------------------------------------------------------------ inventory slots

        private static void InventoryClick(Inventory pInv, int pIndex, PointerEventData.InputButton pButton)
        {
            ItemStack slot = pInv.Get(pIndex);
            ItemStack held = CursorState.Stack;

            if (pButton == PointerEventData.InputButton.Left)
            {
                if (held == null && slot != null && Ctrl)
                {
                    TrashFromSlot(pInv, pIndex);
                    return;
                }
                if (held == null && slot != null && Shift)
                {
                    QuickMove(pInv, pIndex);
                    return;
                }
                if (held == null)
                {
                    if (slot != null) PickUp(pInv, pIndex, slot.Count);
                    return;
                }
                PlaceHeld(pInv, pIndex);
                return;
            }

            if (pButton == PointerEventData.InputButton.Right)
            {
                if (held == null)
                {
                    if (slot == null) return;
                    if (slot.IsWearable || IsCustomWearable(slot))
                    {
                        InventoryOps.EquipFromSlot(pInv, pIndex);
                        return;
                    }
                    if (slot.IsStackable && slot.Count > 1)
                    {
                        PickUp(pInv, pIndex, (slot.Count + 1) / 2);
                        return;
                    }
                    PickUp(pInv, pIndex, slot.Count);
                    return;
                }
                PlaceOne(pInv, pIndex);
            }
        }

        private static bool IsCustomWearable(ItemStack pStack)
        {
            CustomItemDefinition def = pStack.CustomDefinition;
            return def != null && !string.IsNullOrEmpty(def.EquipSlotId);
        }

        public static void PickUp(Inventory pInv, int pIndex, int pAmount)
        {
            ItemStack slot = pInv.Get(pIndex);
            if (slot == null || pAmount <= 0) return;
            if (pAmount >= slot.Count)
            {
                pInv.SetSlot(pIndex, null);
                CursorState.Set(slot, pInv);
            }
            else
            {
                ItemStack part = slot.CloneWithCount(pAmount);
                pInv.ChangeCount(pIndex, -pAmount);
                CursorState.Set(part, pInv);
            }
            UISound.Play(UISound.PickUp);
        }

        /// <summary>Places, merges or swaps the held stack with slot <paramref name="pIndex"/>.</summary>
        public static void PlaceHeld(Inventory pInv, int pIndex)
        {
            ItemStack held = CursorState.Stack;
            if (held == null) return;
            ItemStack slot = pInv.Get(pIndex);
            InventoryOps.Transfer(held, CursorState.Origin, pInv);

            if (slot == null)
            {
                pInv.SetSlot(pIndex, held);
                CursorState.Clear();
            }
            else if (slot.CanStackWith(held) && slot.Count < slot.MaxStack)
            {
                int move = Mathf.Min(held.Count, slot.MaxStack - slot.Count);
                pInv.ChangeCount(pIndex, move);
                held.Count -= move;
                if (held.Count <= 0) CursorState.Clear();
                else CursorState.Set(held, pInv);
            }
            else
            {
                pInv.SetSlot(pIndex, held);
                CursorState.Set(slot, pInv);
            }
            UISound.Play(UISound.Place);
        }

        private static void PlaceOne(Inventory pInv, int pIndex)
        {
            ItemStack held = CursorState.Stack;
            ItemStack slot = pInv.Get(pIndex);
            if (held == null) return;
            if (!held.IsStackable || held.Count == 1)
            {
                if (slot == null || slot.CanStackWith(held)) PlaceHeld(pInv, pIndex);
                return;
            }
            if (slot != null && (!slot.CanStackWith(held) || slot.Count >= slot.MaxStack)) return;

            ItemStack one = held.CloneWithCount(1);
            InventoryOps.Transfer(one, CursorState.Origin, pInv);
            if (slot == null) pInv.SetSlot(pIndex, one);
            else pInv.ChangeCount(pIndex, 1);
            held.Count -= 1;
            CursorState.Touch();
            UISound.Play(UISound.Place);
        }

        private static void QuickMove(Inventory pInv, int pIndex)
        {
            ItemStack slot = pInv.Get(pIndex);
            if (slot == null) return;
            if (slot.IsWearable || IsCustomWearable(slot))
            {
                InventoryOps.EquipFromSlot(pInv, pIndex);
                return;
            }
            bool fromHotbar = pInv.IsHotbar(pIndex);
            int start = fromHotbar ? pInv.HotbarSize : 0;
            int end = fromHotbar ? pInv.Capacity : pInv.HotbarSize;

            // top up matching stacks in the other area, then the first empty slot there
            if (slot.IsStackable)
            {
                for (int i = start; i < end && slot.Count > 0; i++)
                {
                    ItemStack t = pInv.Get(i);
                    if (t == null || !t.CanStackWith(slot) || t.Count >= t.MaxStack) continue;
                    int move = Mathf.Min(slot.Count, t.MaxStack - t.Count);
                    pInv.ChangeCount(i, move);
                    pInv.ChangeCount(pIndex, -move);
                    slot = pInv.Get(pIndex);
                    if (slot == null) break;
                }
            }
            if (slot != null)
            {
                for (int i = start; i < end; i++)
                {
                    if (pInv.Get(i) != null) continue;
                    pInv.Swap(pIndex, i);
                    break;
                }
            }
            UISound.Play(UISound.Place);
        }

        private static void TrashFromSlot(Inventory pInv, int pIndex)
        {
            ItemStack slot = pInv.Get(pIndex);
            if (slot == null) return;
            if (!InventoryOps.TryReleaseFromHand(pInv.Owner, slot)) return;
            pInv.SetSlot(pIndex, null);
            InventoryOps.Trash(pInv, slot, pInv);
        }

        // ------------------------------------------------------------------ hotbar selection

        /// <summary>Selects a hotbar slot, refusing to swap away from a cursed weapon to another weapon.</summary>
        public static void SelectHotbar(Inventory pInv, int pIndex)
        {
            if (pInv == null || pInv.HotbarSize <= 0) return;
            int index = ((pIndex % pInv.HotbarSize) + pInv.HotbarSize) % pInv.HotbarSize;
            if (index == pInv.Selected) return;
            ItemStack target = pInv.Get(index);
            // The weapon really in hand decides (it may be selected AND equipped, so nothing is "pinned" yet).
            Actor owner = pInv.Owner;
            Item inHand = owner != null && owner.hasEquipment() ? owner.equipment.weapon.getItem() : null;
            Item cursed = inHand != null && inHand.isCursed() ? inHand : pInv.PinnedCursedWeapon;
            if (cursed != null && target != null && target.IsWeapon && !ReferenceEquals(target.Item, cursed))
            {
                Toasts.Show(Loc.F("possesslib_toast_cursed_weapon", "Cursed! You can't let go of $0.", cursed.getName(true)));
                UISound.Play(UISound.Deny);
                return;
            }
            PLLog.Debug("Hotbar select " + pInv.Selected + " -> " + index);
            pInv.Select(index);
        }

        // ------------------------------------------------------------------ equipment, trash

        private static void EquipmentClick(Inventory pInv, EquipmentType pType, PointerEventData.InputButton pButton)
        {
            ItemStack held = CursorState.Stack;
            if (held != null)
            {
                if (held.IsWearable && held.EquipmentAsset.equipment_type == pType)
                {
                    InventoryOps.EquipFromCursor(pInv, pType);
                }
                else
                {
                    Toasts.Show(Loc.T("possesslib_toast_wrong_slot", "That doesn't go there."));
                    UISound.Play(UISound.Deny);
                }
                return;
            }
            bool toInventory = pButton == PointerEventData.InputButton.Right || Shift;
            InventoryOps.Unequip(pInv, pType, !toInventory);
        }

        private static void CustomEquipmentClick(Inventory pInv, EquipmentSlotDefinition pSlot, PointerEventData.InputButton pButton)
        {
            if (pSlot == null) return;
            if (CursorState.Stack != null)
            {
                if (!InventoryOps.EquipCustomFromCursor(pInv, pSlot))
                {
                    Toasts.Show(Loc.T("possesslib_toast_wrong_slot", "That doesn't go there."));
                    UISound.Play(UISound.Deny);
                }
                return;
            }
            InventoryOps.UnequipCustomToCursor(pInv, pSlot);
        }

        private static void TrashClick(Inventory pInv)
        {
            ItemStack held = CursorState.Stack;
            if (held != null)
            {
                Inventory origin = CursorState.Origin;
                if (!InventoryOps.TryReleaseFromHand(origin != null ? origin.Owner : null, held)) return;
                CursorState.Clear();
                InventoryOps.Trash(pInv, held, origin);
                return;
            }
            ItemStack back = InventoryOps.TakeFromTrash(pInv);
            if (back != null)
            {
                CursorState.Set(back, pInv);
                UISound.Play(UISound.PickUp);
            }
        }

        // ------------------------------------------------------------------ drag and drop

        public static void OnDroppedOnSlot(SlotView pTarget)
        {
            if (CursorState.Stack == null || pTarget == null) return;
            OnSlotPointerDown(pTarget, PointerEventData.InputButton.Left);
        }

        /// <summary>A drag ended. Released over nothing: drop the held stack on the ground.</summary>
        public static void OnDragReleased(SlotView pSource, PointerEventData pEventData)
        {
            if (CursorState.Stack == null) return;
            bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (overUi) return; // released on some other UI: keep holding it
            DropHeld(PossessionController.Current ?? (CursorState.Origin != null ? CursorState.Origin.Owner : null));
        }

        /// <summary>Drops the held stack at a unit's feet (a little toward the mouse).</summary>
        public static void DropHeld(Actor pActor)
        {
            ItemStack held = CursorState.Stack;
            if (held == null) return;
            Inventory origin = CursorState.Origin;
            Actor owner = origin != null ? origin.Owner : null;
            Actor at = pActor ?? owner;
            if (!InventoryOps.TryReleaseFromHand(owner, held)) return;
            Vector2 pos = at != null ? at.current_position : (origin != null ? origin.LastKnownPosition : Vector2.zero);
            if (at != null && World.world != null)
            {
                Vector2 dir = World.world.getMousePos() - pos;
                if (dir.sqrMagnitude > 0.01f) pos += dir.normalized * 1.2f;
            }
            CursorState.Clear();
            InventoryOps.Drop(held, origin, pos);
        }

        /// <summary>The drop key: the held stack, else the hovered slot (inventory open), else the selected hotbar stack.</summary>
        public static void DropKey(Actor pActor, Inventory pInv)
        {
            if (CursorState.Stack != null)
            {
                DropHeld(pActor);
                return;
            }
            int index = pInv.Selected;
            SlotView hovered = UIState.HoveredSlot;
            if (PossessionController.InventoryOpen && hovered != null && hovered.Address.Kind == SlotKind.Inventory
                && ReferenceEquals(hovered.Address.Inventory, pInv))
            {
                index = hovered.Address.Index;
            }
            ItemStack stack = pInv.Get(index);
            if (stack == null) return;
            if (!InventoryOps.TryReleaseFromHand(pActor, stack)) return;
            pInv.SetSlot(index, null);
            Vector2 pos = pActor.current_position;
            InventoryOps.Drop(stack, pInv, pos);
        }
    }
}
