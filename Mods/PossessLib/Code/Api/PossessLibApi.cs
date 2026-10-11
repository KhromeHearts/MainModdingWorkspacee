using System.Collections.Generic;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;
using PossessLib.UI;
using UnityEngine;

namespace PossessLib.Api
{
    /// <summary>
    /// Public entry point for mods that depend on PossessLib. Everything here is safe to call from
    /// your mod's OnModLoad (registrations) or at runtime (queries and changes).
    /// Add <c>"Dependencies": ["RAYTRACERX_POSSESSLIB"]</c> to your mod.json to use it.
    /// </summary>
    public static class PossessLibApi
    {
        /// <summary>PossessLib's version string.</summary>
        public const string Version = "0.1.0";

        /// <summary>API level. Raised only when something in this API changes in a breaking way.</summary>
        public const int ApiVersion = 1;

        /// <summary>True when the installed PossessLib offers at least <paramref name="pRequired"/>.</summary>
        public static bool IsApiAtLeast(int pRequired) => ApiVersion >= pRequired;

        // ------------------------------------------------------------------ possession

        /// <summary>The unit the player currently controls (the "main" possessed unit), or null.</summary>
        public static Actor GetPossessedActor() => PossessionController.Current;

        /// <summary>True while a unit is possessed.</summary>
        public static bool IsPossessing => PossessionController.Current != null;

        /// <summary>True when <paramref name="pActor"/> is the possessed unit.</summary>
        public static bool IsPossessed(Actor pActor) => pActor != null && ReferenceEquals(pActor, PossessionController.Current);

        // ------------------------------------------------------------------ inventories

        /// <summary>
        /// The unit's PossessLib inventory. With <paramref name="pCreate"/> (default) one is created on
        /// demand; otherwise null is returned for units that never had one.
        /// </summary>
        public static Inventory GetInventory(Actor pActor, bool pCreate = true)
        {
            if (pActor == null) return null;
            Inventory inv = pCreate ? InventoryRegistry.GetOrCreate(pActor) : InventoryRegistry.Get(pActor);
            if (inv != null) ActorSync.ReconcileResources(inv, pActor);
            return inv;
        }

        /// <summary>Gives a game item (weapon, armor, accessory) to a unit. Returns false if it has no free slot.</summary>
        public static bool AddItem(Actor pActor, Item pItem)
        {
            Inventory inv = GetInventory(pActor);
            return inv != null && InventoryOps.AddItem(pActor, inv, pItem);
        }

        /// <summary>
        /// Adds resources to the unit's bag and lays them out in slots. Returns how many got a slot; the
        /// rest still counts as carried (shown as overflow until there is room).
        /// </summary>
        public static int AddResource(Actor pActor, string pResourceId, int pAmount)
        {
            Inventory inv = GetInventory(pActor);
            return inv != null ? InventoryOps.AddResource(pActor, inv, pResourceId, pAmount) : 0;
        }

        /// <summary>Removes resources from the unit. Fails and changes nothing if it has fewer.</summary>
        public static bool RemoveResource(Actor pActor, string pResourceId, int pAmount)
        {
            Inventory inv = GetInventory(pActor);
            return inv != null && InventoryOps.RemoveResource(pActor, inv, pResourceId, pAmount);
        }

        /// <summary>How many of a resource the unit carries (its bag is the source of truth).</summary>
        public static int CountResource(Actor pActor, string pResourceId)
        {
            return pActor != null && pActor.data != null ? pActor.data.inventory.getResource(pResourceId) : 0;
        }

        /// <summary>Adds custom items. Returns the amount that did not fit.</summary>
        public static int AddCustomItem(Actor pActor, string pCustomId, int pAmount = 1, Dictionary<string, string> pData = null)
        {
            Inventory inv = GetInventory(pActor);
            if (inv == null) return pAmount;
            if (!CustomItemRegistry.Has(pCustomId))
            {
                PLLog.Warn("AddCustomItem: '" + pCustomId + "' is not registered");
                return pAmount;
            }
            return InventoryOps.AddCustom(inv, pCustomId, pAmount, pData);
        }

        /// <summary>Removes custom items. Returns the amount actually removed.</summary>
        public static int RemoveCustomItem(Actor pActor, string pCustomId, int pAmount = 1)
        {
            Inventory inv = GetInventory(pActor, false);
            return inv != null ? inv.RemoveAmount(StackKind.Custom, pCustomId, pAmount) : 0;
        }

        /// <summary>How many of a custom item the unit carries.</summary>
        public static int CountCustomItem(Actor pActor, string pCustomId)
        {
            Inventory inv = GetInventory(pActor, false);
            return inv != null ? inv.CountCustom(pCustomId) : 0;
        }

        /// <summary>Opens the possessed unit's inventory panel (does nothing when not possessing).</summary>
        public static void OpenInventory() => PossessionController.SetInventoryOpen(true);

        /// <summary>Closes the inventory panel.</summary>
        public static void CloseInventory() => PossessionController.SetInventoryOpen(false);

        /// <summary>True while the inventory panel is open.</summary>
        public static bool IsInventoryOpen => PossessionController.InventoryOpen;

        /// <summary>Opens the unit inventory window for any unit (no possession needed).</summary>
        public static void OpenUnitInventoryWindow(Actor pActor) => UnitInventoryWindow.Open(pActor);

        // ------------------------------------------------------------------ registration

        /// <summary>Registers a custom item type. Call it in OnModLoad so saved items resolve on load.</summary>
        public static bool RegisterCustomItem(CustomItemDefinition pDefinition) => CustomItemRegistry.Register(pDefinition);

        /// <summary>Registers (or replaces, by id) a crafting recipe.</summary>
        public static void RegisterRecipe(RecipeDefinition pRecipe) => Crafting.Register(pRecipe);

        /// <summary>Registers an extra equipment slot shown in the equipment column.</summary>
        public static bool RegisterEquipmentSlot(EquipmentSlotDefinition pDefinition) => EquipSlotRegistry.Register(pDefinition);

        /// <summary>Registers a HUD element shown while possessing.</summary>
        public static void RegisterHudElement(HudElementDefinition pDefinition)
        {
            if (pDefinition == null || string.IsNullOrEmpty(pDefinition.Id)) return;
            Hud.CustomElements.RemoveAll(d => d.Id == pDefinition.Id);
            Hud.CustomElements.Add(pDefinition);
            Hud.CustomVersion++;
        }

        /// <summary>Registers an extra panel shown under the crafting list while the inventory is open.</summary>
        public static void RegisterInventoryPanel(InventoryPanelDefinition pDefinition)
        {
            if (pDefinition == null || string.IsNullOrEmpty(pDefinition.Id)) return;
            Hud.CustomPanels.RemoveAll(d => d.Id == pDefinition.Id);
            Hud.CustomPanels.Add(pDefinition);
            Hud.CustomVersion++;
        }

        // ------------------------------------------------------------------ world

        /// <summary>
        /// Puts a stack on the ground at a world position (for loot drops, chests...). The stack must
        /// not be in any inventory. Requires ground items to be enabled; otherwise the stack is discarded.
        /// </summary>
        public static void DropOnGround(ItemStack pStack, Vector2 pPosition)
        {
            if (pStack == null) return;
            if (!PLConfig.EnableGroundItems)
            {
                InventoryOps.DestroyStack(pStack);
                return;
            }
            GroundItems.Drop(pStack, pPosition, null);
            PossessLibEvents.RaiseItemDropped(null, pStack, pPosition);
        }

        /// <summary>Shows a short message at the top of the screen while possessing.</summary>
        public static void ShowToast(string pText) => Toasts.Show(pText);

        // ------------------------------------------------------------------ camera

        /// <summary>Camera helpers (thin, clamped wrappers over the game's MoveCamera).</summary>
        public static class Camera
        {
            /// <summary>Zooms the camera. Larger = farther out. Clamped to the game's limits.</summary>
            public static void SetZoom(float pZoom, bool pInstant = false) => CameraZoom.SetZoom(pZoom, pInstant);

            /// <summary>The camera's target zoom.</summary>
            public static float GetZoom() => CameraZoom.GetZoom();

            /// <summary>Back to the zoom from before possession (or the configured possession zoom).</summary>
            public static void ResetZoom()
            {
                float before = PossessionController.ZoomBeforePossession;
                CameraZoom.SetZoom(IsPossessing ? PLConfig.PossessZoom : (before > 0f ? before : CameraZoom.GetZoom()), false);
            }

            /// <summary>The smallest and largest zoom the game allows.</summary>
            public static Vector2 ZoomLimits => new Vector2(CameraZoom.MinZoom, CameraZoom.MaxZoom);
        }
    }
}
