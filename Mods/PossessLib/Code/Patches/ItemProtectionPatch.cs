using HarmonyLib;
using PossessLib.Sync;

namespace PossessLib.Patches
{
    /// <summary>
    /// Patch 3 (postfix): ItemManager deletes items with no actor and no city. Items PossessLib keeps
    /// (carried but unequipped, in the trash slot, or on the ground) are protected here.
    /// </summary>
    [HarmonyPatch(typeof(Item), nameof(Item.isReadyForRemoval))]
    internal static class ItemProtectionPatch
    {
        private static void Postfix(Item __instance, ref bool __result)
        {
            if (__result && InventoryRegistry.IsProtected(__instance)) __result = false;
        }
    }
}
