using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace Optime.QoL {
    internal sealed class FPS : Feature<FPS> {
        [HarmonyPatch(typeof(MapBox), nameof(MapBox.calculateCurElapsed))]
        [HarmonyPrefix]
        public static bool MapBox_calculateCurElapsed_Prefix(ref float __result) {
            float fps = 1.0f / Time.unscaledDeltaTime;

            __result = fps > 60f
                ? Time.fixedDeltaTime * Config.time_scale_asset.multiplier * (60f / fps)
                : Time.fixedDeltaTime * Config.time_scale_asset.multiplier;

            return false;
        }

        public static void SetFrameRate(int value) {
            if (PlayerConfig.optionBoolEnabled("vsync")) {
                return;
            }

            if (PlayerConfig.optionBoolEnabled("fps_lock_30")) {
                return;
            }

            if (Application.targetFrameRate != value) {
                Application.targetFrameRate = value < 60 ? 60 : value;
            }
        }

        protected override void OnLoad() {
            StartCoroutine(AutoFrameRate());
            SetFrameRate(Main.ModConfig["qol"]["qol.maxFrameRate"].IntVal);
        }

        protected override void OnUnload() {
            StopAllCoroutines();
            SetFrameRate(PlayerConfig.optionBoolEnabled("fps_lock_30") ? 30 : 60);
        }

        private IEnumerator AutoFrameRate() {
            while (true) {
                SetFrameRate(Main.ModConfig["qol"]["qol.maxFrameRate"].IntVal);

                yield return new WaitForSeconds(1);
            }
        }
    }
}