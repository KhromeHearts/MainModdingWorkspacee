using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class PowersTabUpdateThrottle : Feature<PowersTabUpdateThrottle> {
        private const int TabUpdateInterval = 15;
        private static int _lastTabUpdateFrame = -1;
        private static int _lastChildren = -1;

        public new static void SetLoaded(bool value) {
            Feature<PowersTabUpdateThrottle>.SetLoaded(value);
        }

        protected override void OnUnload() {
            _lastTabUpdateFrame = -1;
            _lastChildren = -1;
        }

        [HarmonyPatch(typeof(PowersTab), nameof(PowersTab.update))]
        [HarmonyPrefix]
        public static bool PowersTab_update_Prefix(PowersTab __instance) {
            if (__instance._selected_nano != null
                && !ScrollWindow.isWindowActive()
                && !ScrollWindow.isAnimationActive()) {
                __instance._selected_nano.update();
            }

            int frame = Time.frameCount;

            if (frame - _lastTabUpdateFrame < TabUpdateInterval) {
                return false;
            }

            _lastTabUpdateFrame = frame;

            int activeChildren = __instance.transform.CountChildren(pChild =>
                pChild.gameObject.activeSelf && !pChild.name.StartsWith("ButtonSelection"));

            if (activeChildren != _lastChildren) {
                _lastChildren = activeChildren;
                __instance.sortButtons();
                __instance.setNewWidth();

                if (__instance._asset != null) {
                    __instance._asset.last_scroll_position = 0f;
                }
            }

            return false;
        }
    }
}