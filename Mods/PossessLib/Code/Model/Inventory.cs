using System;
using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Sync;
using UnityEngine;

namespace PossessLib.Model
{
    /// <summary>
    /// A unit's PossessLib inventory: hotbar + main grid, trash slot and extra equipment slots.
    /// This class only holds data. Changes that also touch the game (the resource bag, equipment) go
    /// through <see cref="InventoryOps"/> or <see cref="PossessLibApi"/>.
    /// Slots 0..HotbarSize-1 are the hotbar, the rest is the main grid. Empty slots are null.
    /// </summary>
    public sealed class Inventory
    {
        private ItemStack[] _slots;
        private readonly Dictionary<string, ItemStack> _customEquipment = new Dictionary<string, ItemStack>();
        private Actor _owner;

        internal Inventory(Actor pOwner, int pHotbar, int pMain)
        {
            _owner = pOwner;
            OwnerId = pOwner.data.id;
            HotbarSize = Mathf.Max(1, pHotbar);
            MainSize = Mathf.Max(0, pMain);
            _slots = new ItemStack[HotbarSize + MainSize];
            if (pOwner.current_tile != null) LastKnownTile = pOwner.current_tile;
            LastKnownPosition = pOwner.current_position;
        }

        /// <summary>Id of the unit that owns this inventory (ActorData.id).</summary>
        public long OwnerId { get; private set; }

        /// <summary>The owning unit, or null when it is dead or gone.</summary>
        public Actor Owner
        {
            get
            {
                if (_owner != null && _owner.isAlive() && _owner.data != null && _owner.data.id == OwnerId) return _owner;
                return null;
            }
        }

        /// <summary>Number of hotbar slots.</summary>
        public int HotbarSize { get; private set; }

        /// <summary>Number of main grid slots.</summary>
        public int MainSize { get; private set; }

        /// <summary>Total number of slots.</summary>
        public int Capacity => _slots.Length;

        /// <summary>Selected hotbar index.</summary>
        public int Selected { get; private set; }

        /// <summary>Incremented on every change. Compare it to detect changes cheaply.</summary>
        public int Version { get; private set; }

        /// <summary>Content of the trash slot (recoverable until replaced).</summary>
        public ItemStack Trash { get; private set; }

        /// <summary>Resources the unit carries in its bag that do not fit in any slot right now.</summary>
        public int ResourceOverflow { get; internal set; }

        internal Vector2 LastKnownPosition;
        internal WorldTile LastKnownTile;
        internal Item PinnedCursedWeapon;
        internal bool Disposed;

        /// <summary>Stack in a slot, or null.</summary>
        public ItemStack Get(int pIndex)
        {
            return (uint)pIndex < (uint)_slots.Length ? _slots[pIndex] : null;
        }

        /// <summary>Stack in the selected hotbar slot, or null.</summary>
        public ItemStack SelectedStack => Get(Selected);

        /// <summary>True when the index belongs to the hotbar.</summary>
        public bool IsHotbar(int pIndex) => pIndex >= 0 && pIndex < HotbarSize;

        /// <summary>Content of an extra equipment slot registered by another mod.</summary>
        public ItemStack GetCustomEquipment(string pSlotId)
        {
            return pSlotId != null && _customEquipment.TryGetValue(pSlotId, out ItemStack stack) ? stack : null;
        }

        /// <summary>All filled extra equipment slots.</summary>
        public IEnumerable<KeyValuePair<string, ItemStack>> CustomEquipment => _customEquipment;

        internal int CustomEquipmentCount => _customEquipment.Count;

        /// <summary>Total amount of a resource in the slots.</summary>
        public int CountResource(string pResourceId) => Count(StackKind.Resource, pResourceId);

        /// <summary>Total amount of a custom item in the slots.</summary>
        public int CountCustom(string pCustomId) => Count(StackKind.Custom, pCustomId);

        internal int Count(StackKind pKind, string pId)
        {
            int total = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                ItemStack s = _slots[i];
                if (s != null && s.Kind == pKind && s.Id == pId) total += s.Count;
            }
            return total;
        }

        /// <summary>Slot index holding a game item, or -1.</summary>
        public int IndexOf(Item pItem)
        {
            if (pItem == null) return -1;
            for (int i = 0; i < _slots.Length; i++)
            {
                ItemStack s = _slots[i];
                if (s != null && ReferenceEquals(s.Item, pItem)) return i;
            }
            return -1;
        }

        /// <summary>First empty slot (hotbar first when <paramref name="pPreferHotbar"/>), or -1.</summary>
        public int FirstEmpty(bool pPreferHotbar = true)
        {
            if (pPreferHotbar)
            {
                for (int i = 0; i < _slots.Length; i++) if (_slots[i] == null) return i;
                return -1;
            }
            for (int i = HotbarSize; i < _slots.Length; i++) if (_slots[i] == null) return i;
            for (int i = 0; i < HotbarSize; i++) if (_slots[i] == null) return i;
            return -1;
        }

        /// <summary>How many of <paramref name="pStack"/> would fit right now.</summary>
        public int SpaceFor(ItemStack pStack)
        {
            if (pStack == null) return 0;
            int space = 0;
            int max = pStack.MaxStack;
            for (int i = 0; i < _slots.Length; i++)
            {
                ItemStack s = _slots[i];
                if (s == null) space += max;
                else if (s.CanStackWith(pStack)) space += Math.Max(0, max - s.Count);
                if (space >= pStack.Count) return pStack.Count;
            }
            return space;
        }

        /// <summary>True when there is nothing worth saving.</summary>
        public bool IsEmpty
        {
            get
            {
                if (Trash != null || _customEquipment.Count > 0) return false;
                for (int i = 0; i < _slots.Length; i++) if (_slots[i] != null) return false;
                return true;
            }
        }

        // ------------------------------------------------------------------ mutations (data only)

        internal void Bump()
        {
            Version++;
        }

        internal void SetSlot(int pIndex, ItemStack pStack)
        {
            if ((uint)pIndex >= (uint)_slots.Length) return;
            ItemStack old = _slots[pIndex];
            if (ReferenceEquals(old, pStack))
            {
                Bump();
                return;
            }
            _slots[pIndex] = pStack;
            if (old != null)
            {
                OnStackLeft(old);
                PossessLibEvents.RaiseItemRemoved(this, pIndex, old, old.Count);
            }
            if (pStack != null)
            {
                OnStackEntered(pStack);
                PossessLibEvents.RaiseItemAdded(this, pIndex, pStack, pStack.Count);
            }
            Bump();
        }

        /// <summary>Changes the amount of a stack in place (removes it when it reaches 0).</summary>
        internal void ChangeCount(int pIndex, int pDelta)
        {
            ItemStack s = Get(pIndex);
            if (s == null || pDelta == 0) return;
            if (s.Count + pDelta <= 0)
            {
                SetSlot(pIndex, null);
                return;
            }
            s.Count += pDelta;
            if (pDelta > 0) PossessLibEvents.RaiseItemAdded(this, pIndex, s, pDelta);
            else PossessLibEvents.RaiseItemRemoved(this, pIndex, s, -pDelta);
            Bump();
        }

        internal void Swap(int pA, int pB)
        {
            if (pA == pB || (uint)pA >= (uint)_slots.Length || (uint)pB >= (uint)_slots.Length) return;
            ItemStack tmp = _slots[pA];
            _slots[pA] = _slots[pB];
            _slots[pB] = tmp;
            PossessLibEvents.RaiseItemMoved(this, pA, pB);
            Bump();
        }

        internal void Select(int pIndex)
        {
            if (HotbarSize <= 0) return;
            int index = ((pIndex % HotbarSize) + HotbarSize) % HotbarSize;
            if (index == Selected) return;
            int old = Selected;
            Selected = index;
            Bump();
            PossessLibEvents.RaiseHotbarSlotSelected(this, old, index);
        }

        internal void SetTrash(ItemStack pStack)
        {
            ItemStack old = Trash;
            if (ReferenceEquals(old, pStack)) return;
            Trash = pStack;
            if (old != null) OnStackLeft(old);
            if (pStack != null) OnStackEntered(pStack);
            Bump();
        }

        internal void SetCustomEquipment(string pSlotId, ItemStack pStack)
        {
            _customEquipment.TryGetValue(pSlotId, out ItemStack old);
            if (pStack == null) _customEquipment.Remove(pSlotId);
            else _customEquipment[pSlotId] = pStack;
            Bump();
            PossessLibEvents.RaiseEquipmentChanged(Owner, pSlotId, old, pStack);
        }

        /// <summary>
        /// Puts a stack into the inventory: tops up matching stacks first, then fills empty slots.
        /// Returns the amount that did not fit (0 when everything was placed). The passed object may
        /// end up in a slot, so do not reuse it afterwards.
        /// </summary>
        internal int Insert(ItemStack pStack, bool pPreferHotbar = true)
        {
            if (pStack == null || pStack.Count <= 0) return 0;
            if (!pStack.IsStackable)
            {
                int empty = FirstEmpty(pPreferHotbar);
                if (empty < 0) return pStack.Count;
                SetSlot(empty, pStack);
                return 0;
            }

            int max = pStack.MaxStack;
            for (int i = 0; i < _slots.Length && pStack.Count > 0; i++)
            {
                ItemStack target = _slots[i];
                if (target == null || !target.CanStackWith(pStack) || target.Count >= max) continue;
                int add = Math.Min(pStack.Count, max - target.Count);
                pStack.Count -= add;
                ChangeCount(i, add);
            }

            while (pStack.Count > 0)
            {
                int empty = FirstEmpty(pPreferHotbar);
                if (empty < 0) break;
                if (pStack.Count <= max)
                {
                    SetSlot(empty, pStack);
                    return 0;
                }
                ItemStack part = pStack.CloneWithCount(max);
                pStack.Count -= max;
                SetSlot(empty, part);
            }
            return pStack.Count;
        }

        /// <summary>Removes up to <paramref name="pAmount"/> of a resource or custom item, last slots first. Returns the amount removed.</summary>
        internal int RemoveAmount(StackKind pKind, string pId, int pAmount)
        {
            int removed = 0;
            for (int i = _slots.Length - 1; i >= 0 && removed < pAmount; i--)
            {
                ItemStack s = _slots[i];
                if (s == null || s.Kind != pKind || s.Id != pId) continue;
                int take = Math.Min(s.Count, pAmount - removed);
                removed += take;
                ChangeCount(i, -take);
            }
            return removed;
        }

        /// <summary>
        /// Changes the slot counts. Stacks keep their index where possible; the rest are moved to free
        /// slots. Returns the stacks that no longer fit (the caller decides where they go).
        /// </summary>
        internal List<ItemStack> Resize(int pHotbar, int pMain)
        {
            pHotbar = Mathf.Max(1, pHotbar);
            pMain = Mathf.Max(0, pMain);
            List<ItemStack> leftovers = new List<ItemStack>();
            if (pHotbar == HotbarSize && pMain == MainSize) return leftovers;

            ItemStack[] old = _slots;
            int oldHotbar = HotbarSize;
            _slots = new ItemStack[pHotbar + pMain];
            HotbarSize = pHotbar;
            MainSize = pMain;
            List<ItemStack> moved = new List<ItemStack>();
            for (int i = 0; i < old.Length; i++)
            {
                if (old[i] == null) continue;
                bool hotbar = i < oldHotbar;
                int localIndex = hotbar ? i : i - oldHotbar;
                int newIndex = hotbar ? (localIndex < pHotbar ? localIndex : -1) : (localIndex < pMain ? pHotbar + localIndex : -1);
                if (newIndex >= 0) _slots[newIndex] = old[i];
                else moved.Add(old[i]);
            }
            foreach (ItemStack stack in moved)
            {
                int empty = FirstEmpty(true);
                if (empty >= 0) _slots[empty] = stack;
                else
                {
                    OnStackLeft(stack);
                    leftovers.Add(stack);
                }
            }
            if (Selected >= HotbarSize) Selected = 0;
            Bump();
            return leftovers;
        }

        /// <summary>Merges and sorts the main grid (the hotbar is left alone).</summary>
        internal void SortMain()
        {
            List<ItemStack> stacks = new List<ItemStack>();
            for (int i = HotbarSize; i < _slots.Length; i++)
            {
                if (_slots[i] != null) stacks.Add(_slots[i]);
                _slots[i] = null;
            }

            // merge stackables
            for (int i = 0; i < stacks.Count; i++)
            {
                ItemStack a = stacks[i];
                if (!a.IsStackable) continue;
                for (int j = i + 1; j < stacks.Count && a.Count < a.MaxStack; j++)
                {
                    ItemStack b = stacks[j];
                    if (!a.CanStackWith(b)) continue;
                    int move = Math.Min(b.Count, a.MaxStack - a.Count);
                    a.Count += move;
                    b.Count -= move;
                }
            }
            stacks.RemoveAll(s => s.Count <= 0);
            stacks.Sort(CompareForSort);
            for (int i = 0; i < stacks.Count && HotbarSize + i < _slots.Length; i++)
            {
                _slots[HotbarSize + i] = stacks[i];
            }
            Bump();
        }

        private static int SortGroup(ItemStack pStack)
        {
            switch (pStack.Kind)
            {
                case StackKind.Item:
                    EquipmentAsset asset = pStack.EquipmentAsset;
                    return asset == null ? 9 : (int)asset.equipment_type;
                case StackKind.Resource:
                    return 20;
                case StackKind.Custom:
                    return 30;
                default:
                    return 40;
            }
        }

        private static int CompareForSort(ItemStack pA, ItemStack pB)
        {
            int group = SortGroup(pA).CompareTo(SortGroup(pB));
            if (group != 0) return group;
            if (pA.Kind == StackKind.Item && pA.Item != null && pB.Item != null)
            {
                int value = pB.Item.getValue().CompareTo(pA.Item.getValue());
                if (value != 0) return value;
            }
            int id = string.CompareOrdinal(pA.Id, pB.Id);
            return id != 0 ? id : pB.Count.CompareTo(pA.Count);
        }

        // ------------------------------------------------------------------ protection / ownership

        private void OnStackEntered(ItemStack pStack)
        {
            if (pStack.Item == null) return;
            InventoryRegistry.Protect(pStack.Item);
            Actor owner = Owner;
            if (owner != null) ItemOwnership.Claim(pStack.Item, owner);
        }

        private void OnStackLeft(ItemStack pStack)
        {
            if (pStack.Item != null) InventoryRegistry.Unprotect(pStack.Item);
        }

        /// <summary>Releases every protected item (called when the inventory is discarded).</summary>
        internal void ReleaseAll()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null) OnStackLeft(_slots[i]);
                _slots[i] = null;
            }
            if (Trash != null) OnStackLeft(Trash);
            Trash = null;
            _customEquipment.Clear();
            Bump();
        }

        internal void UpdateLastKnownPosition(Actor pActor)
        {
            LastKnownPosition = pActor.current_position;
            if (pActor.current_tile != null) LastKnownTile = pActor.current_tile;
        }
    }
}
