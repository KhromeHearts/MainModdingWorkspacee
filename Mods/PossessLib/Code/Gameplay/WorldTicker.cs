using System;
using System.Collections.Generic;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.Sync;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>
    /// Slow tick (every 0.5 s) over the inventories of units that are NOT possessed:
    /// death handling (DESIGN.md 4.5) and AI weapon switching (DESIGN.md 6.4). Only units that have
    /// PossessLib data are visited, so the cost does not grow with the world's population.
    /// </summary>
    internal static class WorldTicker
    {
        private const float Interval = 0.5f;
        private static float _next;
        private static readonly List<Inventory> _dead = new List<Inventory>();

        public static void Tick()
        {
            float now = Time.unscaledTime;
            if (now < _next) return;
            _next = now + Interval;
            InventoryRegistry.EnsureWorld();

            _dead.Clear();
            IReadOnlyList<Inventory> all = InventoryRegistry.All;
            for (int i = 0; i < all.Count; i++)
            {
                Inventory inv = all[i];
                Actor owner = inv.Owner;
                if (owner == null)
                {
                    _dead.Add(inv);
                    continue;
                }
                inv.UpdateLastKnownPosition(owner);
                if (ReferenceEquals(owner, PossessionController.Current)) continue;
                ActorSync.ValidateItems(inv, owner);
                if (PLConfig.AiWeaponSwitch) MultiWeaponAI.Think(inv, owner);
            }
            for (int i = 0; i < _dead.Count; i++) HandleDeath(_dead[i]);
            Persistence.PendingDrops.Flush();
        }

        /// <summary>Unequipped items fall on the ground (or go to the zone's city, like the game does with gear).</summary>
        private static void HandleDeath(Inventory pInv)
        {
            try
            {
                if (ReferenceEquals(CursorState.Origin, pInv)) CursorState.Clear();
                List<ItemStack> keep = new List<ItemStack>();
                for (int i = 0; i < pInv.Capacity; i++)
                {
                    ItemStack s = pInv.Get(i);
                    if (s == null || s.Kind == StackKind.Resource) continue; // the bag is the game's business
                    if (s.Kind == StackKind.Item && (s.Item == null || !s.Item.isAlive() || s.Item.hasCity())) continue;
                    keep.Add(s);
                }
                foreach (KeyValuePair<string, ItemStack> pair in pInv.CustomEquipment) keep.Add(pair.Value);
                ItemStack trash = pInv.Trash;

                InventoryRegistry.Unregister(pInv); // releases protection
                InventoryOps.DestroyStack(trash);

                Vector2 pos = pInv.LastKnownPosition;
                City city = pInv.LastKnownTile != null && pInv.LastKnownTile.zone != null ? pInv.LastKnownTile.zone.city : null;
                foreach (ItemStack s in keep)
                {
                    if (s.Kind == StackKind.Item)
                    {
                        // Equipped gear already followed the game's death rules; skip anything someone owns now.
                        Actor holder = s.Item.getActor();
                        if (holder != null && holder.isAlive() && ItemOwnership.IsEquippedBy(holder, s.Item)) continue;
                        s.Item.clearUnit();
                    }
                    if (PLConfig.DropOnDeath && PLConfig.EnableGroundItems)
                    {
                        GroundItems.Drop(s, pos, null);
                    }
                    else if (s.Kind == StackKind.Item && city != null && city.isAlive())
                    {
                        city.tryToPutItem(s.Item);
                    }
                    // otherwise the item is ownerless and the game's item manager removes it
                }
                PLLog.Debug("Owner of an inventory died: " + keep.Count + " stacks handled");
            }
            catch (Exception e)
            {
                PLLog.Exception("Death handling", e);
            }
        }
    }

    /// <summary>
    /// AI units that carry several weapons pick the right one for the distance to their target:
    /// ranged when the target is out of melee reach, melee when it is close.
    /// </summary>
    internal static class MultiWeaponAI
    {
        public static void Think(Inventory pInv, Actor pActor)
        {
            if (!pActor.hasEquipment() || !pActor.canUseItems() || !pActor.has_attack_target) return;
            BaseSimObject target = pActor.attack_target;
            if (target == null || !target.isAlive()) return;
            ActorEquipmentSlot slot = pActor.equipment.weapon;
            if (!slot.canChangeSlot()) return;

            Item current = slot.getItem();
            bool currentRanged = current != null && current.getAsset() != null && current.getAsset().attack_type == WeaponType.Range;
            float distance = Vector2.Distance(pActor.current_position, target.current_position);
            bool wantRanged = distance > 4f;
            if (current != null && currentRanged == wantRanged) return;

            Item best = null;
            for (int i = 0; i < pInv.Capacity; i++)
            {
                ItemStack s = pInv.Get(i);
                if (s == null || !s.IsWeapon || !s.IsValid || ReferenceEquals(s.Item, current)) continue;
                bool ranged = s.EquipmentAsset.attack_type == WeaponType.Range;
                if (ranged != wantRanged) continue;
                if (best == null || s.Item.getValue() > best.getValue()) best = s.Item;
            }
            if (best == null) return;

            // keep the current weapon: it goes (back) into the inventory first
            if (current != null && pInv.IndexOf(current) < 0)
            {
                if (pInv.FirstEmpty(false) < 0) return;
                pInv.Insert(ItemStack.FromItem(current), false);
            }
            EquipmentGuard.Active = true;
            try
            {
                if (current != null) slot.takeAwayItem();
                slot.setItem(best, pActor);
            }
            finally
            {
                EquipmentGuard.Active = false;
            }
            if (current != null) ItemOwnership.Claim(current, pActor);
            pActor.setStatsDirty();
        }
    }
}
