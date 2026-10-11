using HarmonyLib;

namespace Optime.QoL {
    internal sealed class TimeStopProjectiles : Feature<TimeStopProjectiles> {
        public new static void SetLoaded(bool value) {
            Feature<TimeStopProjectiles>.SetLoaded(value);
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.update))]
        [HarmonyPrefix]
        public static void Projectile_update_Prefix(ref bool __runOriginal) {
            __runOriginal = !MapBox.instance.isPaused();
        }
    }
}