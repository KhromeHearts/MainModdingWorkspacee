using HarmonyLib;

namespace Optime.Developer {
    internal sealed class DisableMapStats : Feature<DisableMapStats> {
        private static bool _neverEnabled = true;

        [HarmonyPatch(typeof(MapStats), nameof(MapStats.recalcCounters))]
        [HarmonyPrefix]
        public static bool recalcCounters(ref bool __runOriginal) {
            return __runOriginal = false;
        }

        public new static void SetLoaded(bool value) {
            if (_neverEnabled && value) {
                _neverEnabled = false;

                return;
            }

            if (value && !Main.ModConfig["Developer"]["Developer Mode"].BoolVal) {
                Main.ModConfig["Developer"]["Disable Map Stats"].SetValue(false, true);

                return;
            }

            Feature<DisableMapStats>.SetLoaded(value);
        }
    }
}