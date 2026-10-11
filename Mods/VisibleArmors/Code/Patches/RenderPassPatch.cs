using System;
using HarmonyLib;
using VisibleArmors.Core;
using VisibleArmors.Render;

namespace VisibleArmors.Patches
{
    /// <summary>
    /// Why: precalculateRenderDataNormal is the game's main-thread step that fills in coloured sprites the parallel
    /// step could not. Running our check right before it lets an equipment change (or a zoom-limit crossing) take
    /// effect in the same frame, on the main thread, without touching the parallel code. The postfix uploads any
    /// new composite pixels before the frame is drawn. Neither patch skips the original.
    /// </summary>
    [HarmonyPatch(typeof(ActorManager), "precalculateRenderDataNormal")]
    internal static class RenderPassPatch
    {
        private static void Prefix(ActorManager __instance)
        {
            try
            {
                RenderSync.BeforeNormalPass(__instance);
            }
            catch (Exception e)
            {
                VALog.GuardedException("Render sync", e);
            }
        }

        private static void Postfix()
        {
            try
            {
                Compositor.Upload();
            }
            catch (Exception e)
            {
                VALog.GuardedException("Atlas upload", e);
            }
        }
    }
}
