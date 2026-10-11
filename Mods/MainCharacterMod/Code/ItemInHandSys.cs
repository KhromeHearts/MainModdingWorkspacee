using HarmonyLib;
using UnityEngine;

namespace MC
{
    public static class ItemInHandSys
    {
        public static bool CanRender(IHandRenderer renderer)
        {
            if (renderer == null) return false;

            try
            {
                // Verify that the asset can actually generate a valid hand sprite frame
                Sprite sprite = ItemRendering.getItemMainSpriteFrame(renderer);
                return sprite != null;
            }
            catch
            {
                return false;
            }
        }

        public static IHandRenderer GetSelectedRenderer(Actor actor)
        {
            if (actor == null ||
                actor != ControllableUnit.getControllableUnit() ||
                MCInventory.Instance == null)
            {
                return null;
            }

            IHandRenderer renderer = MCInventory.Instance.data.GetSelectedHandRenderer();
            return CanRender(renderer) ? renderer : null;
        }

        public static bool ControlsActor(Actor actor)
        {
            return actor != null &&
                actor == ControllableUnit.getControllableUnit() &&
                MCInventory.Instance != null;
        }
    }

    [HarmonyPatch(typeof(Actor), "getHandRendererAsset")]
    public static class SelectedHotbarHandRendererPatch
    {
        public static void Postfix(Actor __instance, ref IHandRenderer __result)
        {
            IHandRenderer selectedRenderer =
                ItemInHandSys.GetSelectedRenderer(__instance);

            if (selectedRenderer != null)
            {
                __result = selectedRenderer;
            }
        }
    }

    [HarmonyPatch(typeof(Actor), "getRenderedItemSprite")]
    public static class SelectedHotbarItemSpritePatch
    {
        public static void Postfix(Actor __instance, ref Sprite __result)
        {
            IHandRenderer selectedRenderer =
                ItemInHandSys.GetSelectedRenderer(__instance);

            if (selectedRenderer != null)
            {
                Sprite selectedSprite =
                    ItemRendering.getItemMainSpriteFrame(selectedRenderer);

                if (selectedSprite != null)
                {
                    __result = selectedSprite;
                }
            }
        }
    }
}