using System;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.UI;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>
    /// Using the selected item with left click: eating food (the game's own consumeFoodResource plus
    /// the bonus from Data/food.json) and custom items with an OnUse callback.
    /// </summary>
    internal static class Consumables
    {
        private static float _nextUse;

        /// <summary>True when left click should use this stack instead of attacking.</summary>
        public static bool IsUsable(ItemStack pStack)
        {
            if (pStack == null || !pStack.IsValid) return false;
            if (pStack.IsResource) return PLConfig.EnableEating && DataTables.IsEdible(pStack.ResourceAsset);
            if (pStack.IsCustom)
            {
                CustomItemDefinition def = pStack.CustomDefinition;
                return def != null && def.OnUse != null;
            }
            return false;
        }

        /// <summary>Uses the stack in <paramref name="pSlot"/>. Returns true if something was used.</summary>
        public static bool TryUse(Actor pActor, Inventory pInv, int pSlot)
        {
            if (pActor == null || pInv == null || Time.unscaledTime < _nextUse) return false;
            ItemStack stack = pInv.Get(pSlot);
            if (!IsUsable(stack)) return false;
            try
            {
                return stack.IsResource ? Eat(pActor, pInv, pSlot, stack) : UseCustom(pActor, pInv, pSlot, stack);
            }
            catch (Exception e)
            {
                PLLog.Exception("Using " + stack, e);
                _nextUse = Time.unscaledTime + 1f;
                return false;
            }
        }

        private static bool Eat(Actor pActor, Inventory pInv, int pSlot, ItemStack pStack)
        {
            ResourceAsset asset = pStack.ResourceAsset;
            DataTables.GetFood(asset.id, out float cooldown, out int flat, out float percent);
            int before = pActor.getHealth();

            pActor.consumeFoodResource(asset);
            if (flat > 0) pActor.restoreHealth(flat);
            if (percent > 0f) pActor.restoreHealthPercent(percent);

            // remove exactly one from this slot and from the bag
            pActor.takeFromInventory(asset.id, 1);
            ItemStack eaten = pStack.CloneWithCount(1);
            pInv.ChangeCount(pSlot, -1);

            _nextUse = Time.unscaledTime + Mathf.Max(0.1f, cooldown);
            int healed = pActor.getHealth() - before;
            Toasts.Show(healed > 0
                ? Loc.F("possesslib_toast_ate_heal", "Ate $0", asset.getTranslatedName()) + "  <color=#7CFC00>+" + healed + "</color>"
                : Loc.F("possesslib_toast_ate", "Ate $0", asset.getTranslatedName()));
            UISound.Play(UISound.Craft);
            PossessLibEvents.RaiseItemUsed(pActor, eaten);
            return true;
        }

        private static bool UseCustom(Actor pActor, Inventory pInv, int pSlot, ItemStack pStack)
        {
            CustomItemDefinition def = pStack.CustomDefinition;
            bool used = def.OnUse(pActor, pStack);
            _nextUse = Time.unscaledTime + Mathf.Max(0.05f, def.UseCooldown);
            if (!used) return false;
            ItemStack usedStack = pStack.CloneWithCount(1);
            if (def.ConsumeOnUse) pInv.ChangeCount(pSlot, -1);
            else pInv.Bump();
            PossessLibEvents.RaiseItemUsed(pActor, usedStack);
            return true;
        }
    }
}
