using PossessLib.Model;

namespace PossessLib.Sync
{
    /// <summary>
    /// The stack held on the mouse (Terraria's "mouse item"). It still belongs to <see cref="Origin"/>:
    /// its resources stay in that unit's bag and its game item stays owned by that unit, so moving
    /// things around never changes what the game sees until the stack actually leaves the unit.
    /// </summary>
    internal static class CursorState
    {
        private static int _worldSeed = int.MinValue;

        public static ItemStack Stack { get; private set; }
        public static Inventory Origin { get; private set; }
        public static int Version { get; private set; }

        public static bool IsEmpty
        {
            get
            {
                CheckWorld();
                return Stack == null;
            }
        }

        public static void Set(ItemStack pStack, Inventory pOrigin)
        {
            CheckWorld();
            if (Stack != null && Stack.Item != null) InventoryRegistry.Unprotect(Stack.Item);
            Stack = pStack;
            Origin = pStack != null ? pOrigin : null;
            if (Stack != null && Stack.Item != null) InventoryRegistry.Protect(Stack.Item);
            Version++;
        }

        public static void Clear() => Set(null, null);

        /// <summary>Called after the stack's count changed in place.</summary>
        public static void Touch()
        {
            if (Stack != null && Stack.Count <= 0) Clear();
            else Version++;
        }

        /// <summary>Amount of a resource on the cursor that counts toward <paramref name="pInventory"/>'s bag.</summary>
        public static int CountFor(Inventory pInventory, StackKind pKind, string pId)
        {
            if (Stack == null || !ReferenceEquals(Origin, pInventory)) return 0;
            return Stack.Kind == pKind && Stack.Id == pId ? Stack.Count : 0;
        }

        public static bool IsHolding(Item pItem) => pItem != null && Stack != null && ReferenceEquals(Stack.Item, pItem);

        private static void CheckWorld()
        {
            int seed = MapBox.current_world_seed_id;
            if (seed == _worldSeed) return;
            _worldSeed = seed;
            Stack = null;
            Origin = null;
            Version++;
        }
    }

    /// <summary>
    /// Set while PossessLib itself changes equipment, so the ActorEquipmentSlot.setItem prefix does
    /// not treat our own swaps as "the game displaced an item".
    /// </summary>
    internal static class EquipmentGuard
    {
        public static bool Active;
    }
}
