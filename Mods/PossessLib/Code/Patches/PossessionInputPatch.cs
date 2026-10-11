using HarmonyLib;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;
using PossessLib.UI;

namespace PossessLib.Patches
{
    /// <summary>
    /// Patch 2 (postfix): ControllableUnit reads the raw mouse buttons with no UI check, so clicking a
    /// slot would also attack, and Shift-clicking would also dash. This runs right after the game reads
    /// the input and before the possessed status consumes it (same MapBox.update), and:
    /// clears attack and action flags while the pointer is over PossessLib UI; drops the held stack
    /// when the world is clicked; and uses the selected consumable instead of attacking.
    /// </summary>
    [HarmonyPatch(typeof(ControllableUnit), nameof(ControllableUnit.updateControllableUnit))]
    internal static class PossessionInputPatch
    {
        private static void Postfix()
        {
            if (!ControllableUnit.isControllingUnit()) return;
            Actor actor = PossessionController.Current;
            if (actor == null) return;

            bool overUi = UIState.PointerOverPossessLib || (PossessionController.InventoryOpen && World.world.isOverUI());
            if (overUi || UIState.Dragging)
            {
                ClearAttack();
                ControllableUnit._action_pressed_dash = false;
                ControllableUnit._action_pressed_backstep = false;
                return;
            }

            if (!CursorState.IsEmpty)
            {
                if (ControllableUnit._attack_just_pressed_button_left) InventoryActions.DropHeld(actor);
                ClearAttack();
                return;
            }

            Inventory inv = PossessionController.CurrentInventory;
            if (inv == null) return;
            ItemStack selected = inv.SelectedStack;
            if (!Consumables.IsUsable(selected)) return;
            if (ControllableUnit._attack_pressed_button_left && Consumables.TryUse(actor, inv, inv.Selected))
            {
                Core.PLLog.Debug("Used selected item by left click (mouse0 held: " + UnityEngine.Input.GetMouseButton(0) + ")");
            }
            ControllableUnit._attack_pressed_button_left = false;
            ControllableUnit._attack_just_pressed_button_left = false;
        }

        private static void ClearAttack()
        {
            ControllableUnit._attack_pressed_button_left = false;
            ControllableUnit._attack_pressed_button_right = false;
            ControllableUnit._attack_just_pressed_button_left = false;
            ControllableUnit._attack_just_pressed_button_right = false;
        }
    }
}
