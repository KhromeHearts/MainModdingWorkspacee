using System.Collections.Generic;
using PossessLib.Core;
using PossessLib.Model;

namespace PossessLib.Sync
{
    /// <summary>
    /// All inventories of the current world, keyed by actor id, plus the set of game items PossessLib
    /// keeps alive (carried, trashed or on the ground). Everything is cleared automatically when the
    /// world changes (MapBox.current_world_seed_id increments in every clearWorld()).
    /// </summary>
    internal static class InventoryRegistry
    {
        private static readonly Dictionary<long, Inventory> _byActor = new Dictionary<long, Inventory>();
        private static readonly List<Inventory> _list = new List<Inventory>();
        private static readonly Dictionary<Item, int> _protected = new Dictionary<Item, int>();
        private static int _worldSeed = int.MinValue;

        public static IReadOnlyList<Inventory> All => _list;

        public static int ProtectedCount => _protected.Count;

        /// <summary>Clears everything if a different world is now loaded. Cheap; call freely.</summary>
        public static void EnsureWorld()
        {
            int seed = MapBox.current_world_seed_id;
            if (seed == _worldSeed) return;
            if (_list.Count > 0 || _protected.Count > 0) PLLog.Debug("World changed: dropping " + _list.Count + " inventories");
            _byActor.Clear();
            _list.Clear();
            _protected.Clear();
            _worldSeed = seed;
        }

        public static Inventory Get(Actor pActor)
        {
            if (pActor == null || pActor.data == null) return null;
            EnsureWorld();
            return _byActor.TryGetValue(pActor.data.id, out Inventory inv) ? inv : null;
        }

        public static bool Has(long pActorId) => _byActor.ContainsKey(pActorId);

        /// <summary>Gets the actor's inventory, creating an empty one sized from the config when needed.</summary>
        public static Inventory GetOrCreate(Actor pActor)
        {
            Inventory inv = Get(pActor);
            if (inv != null) return inv;
            if (pActor == null || !pActor.isAlive()) return null;
            inv = new Inventory(pActor, PLConfig.HotbarSlots, PLConfig.MainSlots);
            Register(inv);
            ActorSync.ReconcileResources(inv, pActor);
            return inv;
        }

        public static void Register(Inventory pInventory)
        {
            EnsureWorld();
            if (_byActor.TryGetValue(pInventory.OwnerId, out Inventory existing))
            {
                if (ReferenceEquals(existing, pInventory)) return;
                Unregister(existing);
            }
            _byActor[pInventory.OwnerId] = pInventory;
            _list.Add(pInventory);
        }

        /// <summary>Removes the inventory and stops protecting its items.</summary>
        public static void Unregister(Inventory pInventory)
        {
            if (pInventory == null) return;
            if (_byActor.TryGetValue(pInventory.OwnerId, out Inventory existing) && ReferenceEquals(existing, pInventory))
            {
                _byActor.Remove(pInventory.OwnerId);
            }
            _list.Remove(pInventory);
            pInventory.ReleaseAll();
            pInventory.Disposed = true;
        }

        // ---- item protection (see Patches/ItemProtectionPatch) ----

        public static void Protect(Item pItem)
        {
            if (pItem == null) return;
            _protected.TryGetValue(pItem, out int count);
            _protected[pItem] = count + 1;
        }

        public static void Unprotect(Item pItem)
        {
            if (pItem == null || !_protected.TryGetValue(pItem, out int count)) return;
            if (count <= 1) _protected.Remove(pItem);
            else _protected[pItem] = count - 1;
        }

        public static bool IsProtected(Item pItem) => pItem != null && _protected.ContainsKey(pItem);
    }

    /// <summary>Keeps Item._actor pointing at the carrier without stealing items equipped by someone else.</summary>
    internal static class ItemOwnership
    {
        public static void Claim(Item pItem, Actor pOwner)
        {
            if (pItem == null || pOwner == null) return;
            Actor current = pItem.getActor();
            if (ReferenceEquals(current, pOwner)) return;
            if (current != null && current.isAlive() && IsEquippedBy(current, pItem)) return;
            pItem.setUnitHasIt(pOwner);
        }

        public static bool IsEquippedBy(Actor pActor, Item pItem)
        {
            if (pActor == null || pItem == null || !pActor.hasEquipment()) return false;
            foreach (ActorEquipmentSlot slot in pActor.equipment)
            {
                if (ReferenceEquals(slot.getItem(), pItem)) return true;
            }
            return false;
        }
    }
}
