using HarmonyLib;
using PossessLib.Gameplay;

namespace PossessLib.Patches
{
    /// <summary>
    /// Patch 1 (postfix): the possessed unit holds its selected hotbar item.
    /// Runs inside the game's Parallel.For render loop, so it only compares and reads two volatile
    /// references prepared on the main thread by <see cref="HandItem.Refresh"/>. It never returns null
    /// where the game expected a renderer, which keeps the renderer from crashing.
    /// </summary>
    [HarmonyPatch(typeof(Actor), nameof(Actor.getHandRendererAsset))]
    internal static class HandRendererPatch
    {
        private static void Postfix(Actor __instance, ref IHandRenderer __result)
        {
            IHandRenderer renderer = HandItem.OverrideRenderer;
            if (renderer == null || !ReferenceEquals(__instance, HandItem.OverrideActor)) return;
            __result = renderer;
            __instance._has_animated_item = renderer.is_animated;
        }
    }
}
