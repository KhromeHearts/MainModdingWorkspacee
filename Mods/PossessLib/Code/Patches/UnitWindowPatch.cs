using HarmonyLib;
using PossessLib.UI;

namespace PossessLib.Patches
{
    /// <summary>Patch 8 (postfix): adds the "Inventory" button to the game's unit window.</summary>
    [HarmonyPatch(typeof(UnitWindow), "OnEnable")]
    internal static class UnitWindowPatch
    {
        private static void Postfix(UnitWindow __instance)
        {
            UnitWindowButton.Ensure(__instance);
        }
    }
}
