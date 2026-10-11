using System;
using System.Threading;
using HarmonyLib;
using UnityEngine;
using VisibleArmors.Core;
using VisibleArmors.Render;

namespace VisibleArmors.Patches
{
    /// <summary>
    /// Why: this is where the game hands out the coloured sprite of a unit for its current animation frame
    /// (it is called whenever the frame, kingdom colour or head changes). The postfix swaps the result for the
    /// armored composite. The game's own sprite and cache are left untouched, so other mods that read or patch
    /// this method still see vanilla behaviour, and disabling the mod restores vanilla sprites at once.
    /// </summary>
    [HarmonyPatch(typeof(DynamicSpriteCreator), nameof(DynamicSpriteCreator.getSpriteUnit))]
    internal static class SpriteUnitPatch
    {
        internal static int MainThreadId = -1;

        /// <summary>Total time spent in the postfix, in Stopwatch ticks (debug statistics).</summary>
        internal static long Ticks;
        internal static long Calls;

        private static void Postfix(AnimationFrameData pFrameData, Sprite pMainSprite, Actor pActor, ColorAsset pKingdomColor, UnitTextureAtlasID pTextureAtlasID, ref Sprite __result)
        {
            if (!VAConfig.Enabled || ReferenceEquals(__result, null) || pActor == null) return;
            if (pTextureAtlasID != UnitTextureAtlasID.Units) return;
            if (Thread.CurrentThread.ManagedThreadId != MainThreadId) return;
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            Calls++;
            try
            {
                long signature = Signature.Compute(pActor, true);
                if (signature == 0) return;
                AnimationContainerUnit container = pActor.animation_container;
                Sprite armored = Compositor.GetOrCreate(__result, pMainSprite, pActor.cached_sprite_head, pFrameData, pKingdomColor, pActor.asset,
                    container != null ? container.id : null, signature);
                if (armored != null) __result = armored;
            }
            catch (Exception e)
            {
                VALog.GuardedException("getSpriteUnit postfix", e);
            }
            finally
            {
                Ticks += System.Diagnostics.Stopwatch.GetTimestamp() - start;
            }
        }
    }
}
