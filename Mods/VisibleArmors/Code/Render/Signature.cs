using UnityEngine;
using VisibleArmors.Appearance;
using VisibleArmors.Core;

namespace VisibleArmors.Render
{
    /// <summary>
    /// The "equipment signature" of a unit: the look ids of its visible slots packed into one long
    /// (16 bits per slot, helmet in the lowest bits). 0 means "draw the vanilla sprite". It is derived
    /// from the live equipment every time, so nothing is stored per unit and nothing goes into saves.
    /// </summary>
    internal static class Signature
    {
        /// <summary>True while the camera is zoomed out past the configured limit (updated once per frame).</summary>
        public static bool TooFarAway;

        public static void UpdateLod()
        {
            Camera cam = Camera.main;
            TooFarAway = cam != null && cam.orthographicSize > VAConfig.MaxZoom;
        }

        public static int Part(long pSignature, ArmorSlot pSlot)
        {
            return (int)((pSignature >> (16 * (int)pSlot)) & 0xFFFF);
        }

        /// <summary>Signature for the unit as it should look now. pInWorld applies the zoom limit (the unit window ignores it).</summary>
        public static long Compute(Actor pActor, bool pInWorld)
        {
            if (!VAConfig.Enabled || pActor == null) return 0;
            if (pInWorld && TooFarAway) return 0;
            ActorEquipment equipment = pActor.equipment;
            if (equipment == null || !pActor.isAlive()) return 0;
            if (pActor.isEgg() || pActor.isBaby()) return 0;
            if (SpeciesProfiles.For(pActor.asset) == BodyProfile.None) return 0;

            long signature = 0;
            if (VAConfig.ShowHelmet && !(AppearanceRules.KeepKingCrown && pActor.is_profession_king))
                signature |= SlotLook(equipment.helmet);
            if (VAConfig.ShowArmor) signature |= SlotLook(equipment.armor) << 16;
            if (VAConfig.ShowBoots) signature |= SlotLook(equipment.boots) << 32;
            if (VAConfig.ShowAmulet) signature |= SlotLook(equipment.amulet) << 48;
            return signature;
        }

        private static long SlotLook(ActorEquipmentSlot pSlot)
        {
            Item item = pSlot.getItem();
            if (item == null || item.shouldbe_removed) return 0;
            return AppearanceRegistry.IdFor(item.getAsset());
        }
    }
}
