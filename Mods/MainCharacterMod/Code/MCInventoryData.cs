using UnityEngine;

namespace MC
{
    public class MCInventorySlot
    {
        public Item item;
        public string resourceId;
        public int resourceAmount;

        public bool IsEmpty => item == null && string.IsNullOrEmpty(resourceId);
        public bool IsResource => item == null && !string.IsNullOrEmpty(resourceId);
        public bool HasValidItem => item != null && item.getAsset() != null;

        public void Clear()
        {
            item = null;
            resourceId = null;
            resourceAmount = 0;
        }
    }

    public class MCInventoryData
    {
        public const int InventorySlotCount = 24;
        public const int HotbarSlotCount = 6;
        public const int MainInventorySlotCount = InventorySlotCount - HotbarSlotCount;
        public const int ResourceStackLimit = 99;

        public readonly MCInventorySlot[] slots = new MCInventorySlot[InventorySlotCount];
        public readonly int[] hotbarSlotIndices = new int[HotbarSlotCount];
        public int selectedHotbarSlot;

        public MCInventoryData()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new MCInventorySlot();
            }

            for (int i = 0; i < hotbarSlotIndices.Length; i++)
            {
                hotbarSlotIndices[i] = i;
            }
        }

        public MCInventorySlot GetHotbarSlot(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlotIndices.Length) return null;
            return slots[hotbarSlotIndices[hotbarIndex]];
        }

        public MCInventorySlot GetSelectedHotbarSlot() => GetHotbarSlot(selectedHotbarSlot);

        public IHandRenderer GetSelectedHandRenderer()
        {
            MCInventorySlot slot = GetSelectedHotbarSlot();
            if (slot == null || slot.IsEmpty) return null;
            if (slot.IsResource) return AssetManager.resources.get(slot.resourceId);
            return slot.item == null ? null : slot.item.getAsset();
        }

        public bool TryAdd(Item item)
        {
            if (item == null || item.getAsset() == null) return false;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].item = item;
                    return true;
                }
            }

            return false;
        }

        public bool TryAdd(Item item, Actor owner)
        {
            if (item == null || item.getAsset() == null || owner == null)
            {
                return false;
            }

            item.setUnitHasIt(owner);
            return TryAdd(item);
        }

        public int AddResource(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId) || amount <= 0 ||
                AssetManager.resources.get(resourceId) == null)
            {
                return 0;
            }

            int remaining = amount;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsResource && slots[i].resourceId == resourceId &&
                    slots[i].resourceAmount < ResourceStackLimit)
                {
                    int added = Mathf.Min(remaining, ResourceStackLimit - slots[i].resourceAmount);
                    slots[i].resourceAmount += added;
                    remaining -= added;
                }
            }

            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int added = Mathf.Min(remaining, ResourceStackLimit);
                    slots[i].resourceId = resourceId;
                    slots[i].resourceAmount = added;
                    remaining -= added;
                }
            }

            return amount - remaining;
        }

        public bool RemoveResource(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId) || amount <= 0 || CountResource(resourceId) < amount)
                return false;

            int remaining = amount;
            for (int i = slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (!slots[i].IsResource || slots[i].resourceId != resourceId) continue;

                int removed = Mathf.Min(remaining, slots[i].resourceAmount);
                slots[i].resourceAmount -= removed;
                remaining -= removed;
                if (slots[i].resourceAmount <= 0) slots[i].Clear();
            }

            return true;
        }

        public int CountResource(string resourceId)
        {
            int total = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsResource && slots[i].resourceId == resourceId)
                    total += slots[i].resourceAmount;
            }
            return total;
        }

        public bool EquipReplacingFromInventory(int inventoryIndex, Actor actor)
        {
            if (actor == null || inventoryIndex < 0 || inventoryIndex >= slots.Length) return false;

            MCInventorySlot invSlot = slots[inventoryIndex];
            Item itemToEquip = invSlot.item;
            if (itemToEquip == null || itemToEquip.getAsset() == null) return false;

            // Handle Weapons: Swap weapon into active selected hotbar slot
            if (itemToEquip.getAsset().equipment_type == EquipmentType.Weapon)
            {
                int targetHotbarIndex = hotbarSlotIndices[selectedHotbarSlot];
                if (inventoryIndex == targetHotbarIndex) return false;

                MCInventorySlot tempSlot = slots[targetHotbarIndex];
                slots[targetHotbarIndex] = invSlot;
                slots[inventoryIndex] = tempSlot;
                return true;
            }

            // Handle Armor/Accessories: Equip into standard Equipment Panel slot
            ActorEquipmentSlot equipmentSlot = actor.equipment.getSlot(itemToEquip.getAsset().equipment_type);
            if (equipmentSlot == null || !equipmentSlot.canChangeSlot()) return false;

            Item displacedItem = equipmentSlot.getItem();
            bool hasDisplaced = displacedItem != null && displacedItem.getAsset() != null;

            equipmentSlot.setItem(itemToEquip, actor);
            actor.setStatsDirty();
            invSlot.Clear();
            if (hasDisplaced)
            {
                displacedItem.setUnitHasIt(actor);
                invSlot.item = displacedItem;
            }
            return true;
        }

        public void SyncActorBag(Actor actor)
        {
            if (actor == null || actor.data == null || actor.data.inventory == null) return;

            foreach (ResourceAsset resource in AssetManager.resources.list)
            {
                int nativeAmount = actor.data.inventory.getResource(resource.id);
                int currentAmount = CountResource(resource.id);

                if (nativeAmount > currentAmount)
                    AddResource(resource.id, nativeAmount - currentAmount);
                else if (nativeAmount < currentAmount)
                    RemoveResource(resource.id, currentAmount - nativeAmount);
            }
        }
    }
}