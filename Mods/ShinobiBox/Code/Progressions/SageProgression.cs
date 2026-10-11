using UnityEngine;

namespace ShinobiBox
{
    public static class SageProgression
    {
        private const int MeditationsToBecomeSage = 5;
        private const float PerfectSageChance = 0.35f;
        private const float ToadFailureChance = 0.015f;
        private const float SignatureSageChakraCost = 15f;

        private static readonly string[] SageTraits = { "frog_sage_mode", "slug_sage_mode", "snake_sage_mode", "wood_sage_mode" };
        private static readonly string[] SageStatuses = { "status_frog_sage", "status_slug_sage", "status_snake_sage", "status_wood_sage" };

        #region Progression
        public static void HandleSageProgression(Actor actor, int level)
        {
            if (actor == null || !actor.isAlive() || !ShinobiActors.IsShinobi(actor)) return;

            if (IsSage(actor))
            {
                if (ChakraSystem.GetSageChakra(actor) < ChakraSystem.GetMaxSageChakra(actor) * 0.6f)
                {
                    TryMeditate(actor);
                }
                return;
            }

            if (actor.hasTrait("sage_apprentice"))
            {
                TryMeditate(actor);
                return;
            }

            TryStartTraining(actor, level);
        }

        private static void TryStartTraining(Actor actor, int level)
        {
            if (level < 6 || actor.data.getAge() < 16) return;

            actor.data.get("sage_training_animal", out string trainedBefore);
            if (!string.IsNullOrEmpty(trainedBefore)) return;

            bool woodUser = actor.hasTrait("senju_clan") && actor.hasTrait("wood_release");
            bool hugeChakra = actor.hasTrait("03_greater_chakra_reserve") || actor.hasTrait("04_vast_chakra_reserve");
            if (!woodUser && !hugeChakra) return;

            int year = Date.getCurrentYear();
            actor.data.get("sage_roll_year", out int lastRollYear, -1);
            if (lastRollYear == year) return;
            actor.data.set("sage_roll_year", year);

            if (!Randy.randomChance(0.03f)) return;

            actor.data.set("sage_training_animal", woodUser ? "wood" : RandomAnimal());
            actor.addTrait("sage_apprentice");

            if (ShinobiConfig.EnableWorldTips)
            {
                ShinobiWorldLogs.AddWorldLog("log_sage_apprentice", "worldlog_sage_apprentice", "ui/icons/senjutsu", actor);
            }
        }

        private static string RandomAnimal()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.34f) return "frog";
            if (roll < 0.67f) return "slug";
            return "snake";
        }

        private static void BecomeSage(Actor actor, string animal)
        {
            actor.removeTrait("sage_apprentice");
            actor.data.removeInt("sage_meditations");

            if (!Randy.randomChance(PerfectSageChance))
            {
                actor.data.set("sage_imperfect", true);
            }

            actor.addTrait(GetSageTrait(animal));
            ChakraSystem.SetSageChakra(actor, ChakraSystem.GetMaxSageChakra(actor));

            if (actor.current_tile != null)
            {
                EffectsLibrary.spawnExplosionWave(actor.current_tile.posV3, 1f, 1f);
            }

            if (ShinobiConfig.EnableWorldTips)
            {
                ShinobiWorldLogs.AddWorldLog($"log_{animal}_sage_mode", $"worldlog_{animal}_sage_mode", $"ui/icons/{animal}_sage", actor);
            }
        }

        private static void TurnIntoToad(Actor actor)
        {
            WorldTile tile = actor.current_tile;
            if (tile == null) return;

            ShinobiWorldLogs.AddWorldLog("log_turned_into_toad", "worldlog_turned_into_toad", "ui/icons/frog_sage", actor);

            Actor toad = World.world.units.createNewUnit("frog", tile, true, 1f);
            if (toad != null)
            {
                toad.data.name = actor.getName();
                toad.data.custom_name = true;
            }

            actor.die();
        }
        #endregion

        #region Meditation
        private static void TryMeditate(Actor actor)
        {
            if (actor.has_attack_target) return;
            if (actor.hasStatus("status_meditating") || IsInSageMode(actor)) return;
            if (!Randy.randomChance(0.15f)) return;

            actor.addStatusEffect("status_meditating");
        }

        public static bool Meditate(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (!pTarget.isAlive()) return false;
            Actor a = pTarget.a;

            a.stopMovement();
            a.makeWait(1f);
            ChakraSystem.AddSageChakra(a, 15f);

            return true;
        }

        public static bool MeditationFinished(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (!pTarget.isAlive()) return false;
            Actor a = pTarget.a;

            if (!a.hasTrait("sage_apprentice")) return false;

            a.data.get("sage_training_animal", out string animal);
            if (string.IsNullOrEmpty(animal))
            {
                animal = RandomAnimal();
                a.data.set("sage_training_animal", animal);
            }

            if (animal == "frog" && Randy.randomChance(ToadFailureChance))
            {
                TurnIntoToad(a);
                return true;
            }

            a.data.get("sage_meditations", out int meditations);
            meditations++;
            a.data.set("sage_meditations", meditations);

            if (meditations >= MeditationsToBecomeSage)
            {
                BecomeSage(a, animal);
            }

            return true;
        }

        public static bool ClearMeditatingStatus(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (!pSelf.isAlive()) return false;

            pSelf.a.finishStatusEffect("status_meditating");

            return true;
        }
        #endregion

        #region Sage Mode
        public static bool EnterSageMode(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            Actor a = pSelf.a;

            string sageStatus = GetSageStatus(a);
            if (sageStatus == null || IsInSageMode(a)) return false;
            if (ChakraSystem.GetSageChakra(a) < ChakraSystem.GetMaxSageChakra(a) * 0.5f) return false;

            a.finishStatusEffect("status_meditating");
            a.addStatusEffect(sageStatus);
            a.addStatusEffect("jutsuuse", 1f);

            if (a.hasTrait("slug_sage_mode"))
            {
                a.addStatusEffect("status_100_sealings");
            }

            return true;
        }

        public static bool SageAttack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            Actor a = pSelf.a;

            if (!IsInSageMode(a))
            {
                return EnterSageMode(pSelf, pTarget, pTile);
            }

            if (a.hasStatus("status_frog_sage")) return FrogKumite(a, pTarget.a);
            if (a.hasStatus("status_snake_sage")) return SnakeBite(a, pTarget.a);
            if (a.hasStatus("status_wood_sage")) return WoodBind(a, pTarget.a);

            return false;
        }

        private static bool FrogKumite(Actor a, Actor target)
        {
            if (!Randy.randomChance(0.25f)) return false;
            if (!ChakraSystem.TryConsumeSageChakra(a, SignatureSageChakraCost)) return false;

            target.getHit(a.stats["damage"] * 0.5f, true, AttackType.Other, a, true);
            if (target.current_tile != null)
            {
                World.world.applyForceOnTile(target.current_tile, 2, 1.5f, true, 0, null, a);
            }
            return true;
        }

        private static bool SnakeBite(Actor a, Actor target)
        {
            if (!Randy.randomChance(0.20f)) return false;
            if (target.hasStatus("poisoned")) return false;
            if (!ChakraSystem.TryConsumeSageChakra(a, SignatureSageChakraCost)) return false;

            target.addStatusEffect("poisoned", 6f);
            return true;
        }

        private static bool WoodBind(Actor a, Actor target)
        {
            if (!Randy.randomChance(0.15f)) return false;
            if (target.current_tile == null) return false;
            if (!ChakraSystem.TryConsumeSageChakra(a, SignatureSageChakraCost)) return false;

            foreach (Actor unit in Finder.getUnitsFromChunk(target.current_tile, 1, 3f))
            {
                if (unit == a || !unit.isAlive()) continue;
                if (a.kingdom != null && unit.kingdom == a.kingdom) continue;

                unit.addStatusEffect("status_mokuton_tree");
            }
            return true;
        }

        public static bool SnakeRegeneration(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            Actor a = pTarget.a;

            if (a.data.health >= a.getMaxHealth()) return false;

            a.restoreHealth((int)(a.getMaxHealth() * 0.02f) + 1);
            return true;
        }

        public static float GetSageModeDrain(Actor actor)
        {
            actor.data.get("sage_imperfect", out bool imperfect);
            return imperfect ? 20f : 10f;
        }
        #endregion

        #region Helpers
        public static bool IsSage(Actor actor)
        {
            foreach (string trait in SageTraits)
            {
                if (actor.hasTrait(trait)) return true;
            }
            return false;
        }

        public static bool IsInSageMode(Actor actor)
        {
            return GetActiveSageStatus(actor) != null;
        }

        public static string GetActiveSageStatus(Actor actor)
        {
            foreach (string status in SageStatuses)
            {
                if (actor.hasStatus(status)) return status;
            }
            return null;
        }

        private static string GetSageStatus(Actor actor)
        {
            for (int i = 0; i < SageTraits.Length; i++)
            {
                if (actor.hasTrait(SageTraits[i])) return SageStatuses[i];
            }
            return null;
        }

        private static string GetSageTrait(string animal)
        {
            return animal + "_sage_mode";
        }
        #endregion
    }
}
