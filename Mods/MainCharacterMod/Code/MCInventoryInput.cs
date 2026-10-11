using UnityEngine;
using UnityEngine.EventSystems;

namespace MC
{
    public class MCInventorySlotButton : MonoBehaviour, IPointerClickHandler
    {
        private MCInventory inventory;
        private int slotIndex;
        private bool selectsHotbar;
        private bool equipmentSlot;
        private EquipmentType equipmentType;

        public void ConfigureStorage(MCInventory owner, int index, bool hotbar)
        {
            inventory = owner;
            slotIndex = index;
            selectsHotbar = hotbar;
            equipmentSlot = false;
        }

        public void ConfigureEquipment(MCInventory owner, EquipmentType type)
        {
            inventory = owner;
            equipmentType = type;
            equipmentSlot = true;
            selectsHotbar = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (inventory == null) return;

            // 1. Equipment Panel Clicks
            if (equipmentSlot)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    inventory.HandleEquipmentSlotClick(equipmentType);
                }
                return;
            }

            // 2. Right-Click Auto-Equip (Works for both Hotbar and Main Inventory)
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventory.TryEquipFromSlot(slotIndex);
                return;
            }

            // 3. Left-Click Selection / Pickup / Swap
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (selectsHotbar)
                {
                    inventory.HandleHotbarSlotClick(slotIndex);
                }
                else
                {
                    inventory.HandleStorageSlotClick(slotIndex);
                }
            }
        }
    }
}