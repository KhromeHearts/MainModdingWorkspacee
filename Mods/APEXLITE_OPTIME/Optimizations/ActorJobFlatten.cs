using System;
using HarmonyLib;

namespace Optime.Optimizations {
    internal sealed class ActorJobFlatten : Feature<ActorJobFlatten> {
        private static Random _random;

        public new static void SetLoaded(bool value) {
            Feature<ActorJobFlatten>.SetLoaded(value);
        }

        protected override void OnLoad() {
            _random = new Random();
        }

        protected override void OnUnload() {
            _random = null;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.updateNutritionDecay))]
        [HarmonyPrefix]
        public static bool BatchActors_updateNutritionDecay_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.u1_checkInside))]
        [HarmonyPrefix]
        public static bool BatchActors_u1_checkInside_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.u6_checkFrozen))]
        [HarmonyPrefix]
        public static bool BatchActors_u6_checkFrozen_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.u8_checkUpdateTimers))]
        [HarmonyPrefix]
        public static bool BatchActors_u8_checkUpdateTimers_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.b1_checkUnderForce))]
        [HarmonyPrefix]
        public static bool BatchActors_b1_checkUnderForce_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.b3_findEnemyTarget))]
        [HarmonyPrefix]
        public static bool BatchActors_b3_findEnemyTarget_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.b4_checkTaskVerifier))]
        [HarmonyPrefix]
        public static bool BatchActors_b4_checkTaskVerifier_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.b5_checkPathMovement))]
        [HarmonyPrefix]
        public static bool BatchActors_b5_checkPathMovement_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.b55_updateNaturalDeaths))]
        [HarmonyPrefix]
        public static bool BatchActors_b55_updateNaturalDeaths_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.b6_updateAI))]
        [HarmonyPrefix]
        public static bool BatchActors_b6_updateAI_Prefix() {
            return false;
        }

        [HarmonyPatch(typeof(BatchActors), nameof(BatchActors.u4_deadCheck))]
        [HarmonyPrefix]
        public static bool BatchActors_u4_deadCheck_Prefix(ref BatchActors __instance) {
            if (!__instance.check(__instance._cur_container)) {
                return false;
            }

            Actor[] actors = __instance._array;
            int count = __instance._count;
            float elapsed = __instance._elapsed;
            bool isPaused = World.world.isPaused();
            bool starvationDamage = WorldLawLibrary.world_law_hunger.isEnabled();
            bool shouldUpdateHunger = World.world.timer_nutrition_decay <= 0f;
            bool runFindEnemyTarget = _random.NextDouble() < 1.0 / 3.5;
            bool runUpdateNaturalDeaths = _random.NextDouble() < 1.0 / 11.0;

            for (int i = 0; i < count; i++) {
                Actor actor = actors[i];

                actor.u1_checkInside(elapsed);
                actor.u4_deadCheck(elapsed);

                if (isPaused) {
                    continue;
                }

                if (shouldUpdateHunger) {
                    if (actor.needsFood()
                        && !actor.isEgg()
                        && (!actor.hasSubspecies()
                            || !actor.subspecies.has_trait_energy_preserver
                            || !actor.hasStatus("sleeping"))) {
                        actor.updateNutritionDecay(starvationDamage);
                    }
                }

                actor.u6_checkFrozen(elapsed);
                actor.u8_checkUpdateTimers(elapsed);
                actor.b1_checkUnderForce(elapsed);

                if (runFindEnemyTarget) {
                    actor.b3_findEnemyTarget(elapsed);
                }

                actor.b4_checkTaskVerifier(elapsed);
                actor.b5_checkPathMovement(elapsed);

                if (runUpdateNaturalDeaths) {
                    actor.b55_updateNaturalDeaths(elapsed);
                }

                actor.b6_updateAI(elapsed);
            }

            return false;
        }
    }
}