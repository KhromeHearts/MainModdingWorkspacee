using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class QuantumSpriteThrottle : Feature<QuantumSpriteThrottle> {
        public new static void SetLoaded(bool value) {
            Feature<QuantumSpriteThrottle>.SetLoaded(value);
        }

        [HarmonyPatch(typeof(QuantumSpriteManager), nameof(QuantumSpriteManager.updateSystems))]
        [HarmonyPrefix]
        public static bool QuantumSpriteManager_updateSystems_Prefix() {
            return Time.frameCount % 2 == 0;
        }
    }
}