using System;
using HarmonyLib;
using UnityEngine;
using VisibleArmors.Core;
using VisibleArmors.Render;

namespace VisibleArmors.Patches
{
    /// <summary>
    /// Why: the unit window and other UI portraits build their sprites from a snapshot (ActorAvatarData), not from the
    /// in-world sprite, so they need their own hook to show the same armor. The zoom limit does not apply here.
    /// </summary>
    [HarmonyPatch(typeof(ActorAvatarData), nameof(ActorAvatarData.getColoredSprite))]
    internal static class AvatarPatch
    {
        private static void Postfix(ActorAvatarData __instance, Sprite pSprite, AnimationContainerUnit pContainer, ref Sprite __result)
        {
            if (!VAConfig.Enabled || !VAConfig.UiAvatars || ReferenceEquals(__result, null) || pSprite == null || __instance == null) return;
            try
            {
                if (World.world == null || World.world.units == null || __instance.asset == null) return;
                if (__instance.asset.texture_atlas != UnitTextureAtlasID.Units) return;
                Actor actor = World.world.units.get(__instance.actor_id);
                if (actor == null) return;
                long signature = Signature.Compute(actor, false);
                if (signature == 0) return;
                AnimationFrameData frame = null;
                pContainer?.dict_frame_data.TryGetValue(pSprite.name, out frame);
                Sprite head = DynamicActorSpriteCreatorUI.getSpriteHeadForUI(__instance.asset, __instance.sex, pContainer, __instance.actor_id, __instance.head_id,
                    __instance.is_adult, __instance.is_egg, __instance.is_king, __instance.is_warrior, __instance.is_wise, false);
                Sprite armored = Compositor.GetOrCreate(__result, pSprite, head, frame, __instance.kingdom_color, __instance.asset,
                    pContainer != null ? pContainer.id : null, signature);
                if (armored != null) __result = armored;
            }
            catch (Exception e)
            {
                VALog.GuardedException("Avatar postfix", e);
            }
        }
    }
}
