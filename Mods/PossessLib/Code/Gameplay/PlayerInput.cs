using System;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.UI;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>Keyboard and wheel input while possessing. Mouse clicks on slots are handled by SlotView.</summary>
    internal static class PlayerInput
    {
        public static void Tick()
        {
            Actor actor = PossessionController.Current;
            Inventory inv = PossessionController.CurrentInventory;
            if (actor == null || inv == null) return;
            if (IsTyping()) return;

            if (Input.GetKeyDown(PLConfig.KeyInventory)) PossessionController.ToggleInventory();

            int numberKeys = Mathf.Min(inv.HotbarSize, 10);
            for (int i = 0; i < numberKeys; i++)
            {
                KeyCode key = i == 9 ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha1 + i);
                if (Input.GetKeyDown(key)) InventoryActions.SelectHotbar(inv, i);
            }

            float wheel = Input.mouseScrollDelta.y;
            if (wheel != 0f && PLConfig.ScrollSelectsHotbar && World.world != null && !World.world.isOverUI())
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (wheel > 0f) MoveCamera.zoomInWheel(null);
                    else MoveCamera.zoomOutWheel(null);
                }
                else
                {
                    // Terraria: wheel down = next slot.
                    PLLog.Debug("Mouse wheel " + wheel);
                    InventoryActions.SelectHotbar(inv, inv.Selected + (wheel > 0f ? -1 : 1));
                }
            }

            if (Input.GetKeyDown(PLConfig.KeyDrop)) InventoryActions.DropKey(actor, inv);
        }

        /// <summary>True while a text field has focus (renaming a unit, console...).</summary>
        public static bool IsTyping()
        {
            try
            {
                return AssetManager.hotkey_library != null && AssetManager.hotkey_library.isInputActive();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
