using HarmonyLib;
using NeoModLoader.api.features;
using BetterZoom.Code.Patches;

namespace BetterZoom.Code
{
    public class HarmonySetup : ModObjectFeature<HarmonyLib.Harmony>
    {
        public HarmonyLib.Harmony Instance => Object;

        protected override HarmonyLib.Harmony InitObject()
        {
            var harmony = new HarmonyLib.Harmony("worldbox.betterzoom");
            harmony.PatchAll(typeof(ZoomToMousePatch).Assembly);
            return harmony;
        }
    }
}
