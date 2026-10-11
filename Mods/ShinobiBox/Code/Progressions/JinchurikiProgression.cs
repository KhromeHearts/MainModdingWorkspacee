using System;
using System.Net;
using UnityEngine;

namespace ShinobiBox
{
    public static class Jinchuriki
    {
        #region Nine Tails Jinchuriki

        #region Progression

        public static bool NineTailsJinchuriki(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
        {
            if (pSelf?.a == null || !pSelf.a.isAlive()) return false;

            Actor actor = pSelf.a;
            if (!actor.hasTrait(TraitIds.NineTailsJinchuriki)) return false;
            if (actor.hasStatus(StatusIds.NineTailsBaryon)) return false;

            int level = actor.data.level;
            bool hasAttackTarget = actor.has_attack_target;

            if (!hasAttackTarget) return false;

            string bestStatus = "";

            if (level >= 9 && (actor.intelligence >= 20f || actor.diplomacy >= 20f))
                bestStatus = StatusIds.NineTailsKcm2;
            else if (level >= 7 && (actor.intelligence >= 20f || actor.diplomacy >= 20f))
                bestStatus = StatusIds.NineTailsKcm1;
            else if (level >= 5)
                bestStatus = StatusIds.NineTailsIncompleteBeast;
            else if (level >= 4)
                bestStatus = StatusIds.NineTailsV2Cloak;
            else if (level >= 3)
                bestStatus = StatusIds.NineTailsV1Cloak;
            else if (level >= 2)
                bestStatus = StatusIds.NineTailsInitialRelease;

            if (string.IsNullOrEmpty(bestStatus)) return false;
            if (actor.hasStatus(bestStatus)) return false;
            ClearNineTailsStatuses(actor);

            actor.addStatusEffect(bestStatus, 45f);
            HandleJinchurikiWorldLogs(actor, bestStatus);

            return true;
        }

        private static void ClearNineTailsStatuses(Actor actor)
        {
            actor.finishStatusEffect(StatusIds.NineTailsKcm2);
            actor.finishStatusEffect(StatusIds.NineTailsKcm1);
            actor.finishStatusEffect(StatusIds.NineTailsIncompleteBeast);
            actor.finishStatusEffect(StatusIds.NineTailsV2Cloak);
            actor.finishStatusEffect(StatusIds.NineTailsV1Cloak);
            actor.finishStatusEffect(StatusIds.NineTailsInitialRelease);
        }

        private static void HandleJinchurikiWorldLogs(Actor actor, string appliedStatus)
        {
            if (!ShinobiConfig.EnableWorldTips) return;
            if (actor.isClone()) return;

            if (appliedStatus == StatusIds.NineTailsKcm2)
            {
                actor.data.get("kcm2_done", out bool doneKCM2, false);
                if (!doneKCM2)
                {
                    ShinobiWorldLogs.AddWorldLog("log_mastered_kurama", "worldlog_mastered_kurama", "ui/icons/kcm2", actor);
                    actor.data.set("kcm2_done", true);
                }
            }
            else if (appliedStatus == StatusIds.NineTailsKcm1)
            {
                actor.data.get("kcm1_done", out bool doneKCM1, false);
                if (!doneKCM1)
                {
                    ShinobiWorldLogs.AddWorldLog("log_controlled_kurama", "worldlog_controlled_kurama", "ui/icons/kcm1", actor);
                    actor.data.set("kcm1_done", true);
                }
            }
            else if (appliedStatus == StatusIds.NineTailsV2Cloak || appliedStatus == StatusIds.NineTailsIncompleteBeast)
            {
                actor.data.get("beast_awakens_done", out bool doneBeast, false);
                if (!doneBeast)
                {
                    string icon = appliedStatus == StatusIds.NineTailsV2Cloak ? "ui/icons/v2" : "ui/icons/incomplete";
                    ShinobiWorldLogs.AddWorldLog("log_beast_awakens", "worldlog_beast_awakens", icon, actor);
                    actor.data.set("beast_awakens_done", true);
                }
            }
            else if (appliedStatus == StatusIds.NineTailsV1Cloak)
            {
                actor.data.get("v1_done", out bool doneV1, false);
                if (!doneV1)
                {
                    ShinobiWorldLogs.AddWorldLog("log_demon_shroud", "worldlog_demon_shroud", "ui/icons/v1", actor);
                    actor.data.set("v1_done", true);
                }
            }
            else if (appliedStatus == StatusIds.NineTailsInitialRelease)
            {
                actor.data.get("sealbroken_done", out bool doneSealBroken, false);
                if (!doneSealBroken)
                {
                    ShinobiWorldLogs.AddWorldLog("log_seal_weakens", "worldlog_seal_weakens", "ui/icons/initial_release", actor);
                    actor.data.set("sealbroken_done", true);
                }
            }
        }
        #endregion

        #region Baryon Mode
        public static bool BaryonMode(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf?.a == null || !pSelf.a.isAlive()) return false;
            Actor actor = pSelf.a;

            if (actor.asset.id == "juubi" || actor.asset.id == "kurama") return false;
            if (actor.data.level < 7) return false;
            if (actor.hasStatus(StatusIds.NineTailsBaryon)) return false;
            if (UnityEngine.Random.value > 0.45f) return false;
            //Actor baryonUnit = World.world.units.createNewUnit(actor.asset.id, actor.current_tile, true, 1f);

            /*
            ActorTool.copyUnitToOtherUnit(actor, baryonUnit, true);
            baryonUnit.setKingdom(actor.kingdom);

                if (actor.hasStatus("status_jinchuriki_v1_cloak"))
                {
                    return false;
                }
            baryonUnit.joinCity(actor.city);
            baryonUnit.data.name = actor.data.name;
            baryonUnit.data.level = actor.data.level;
            baryonUnit.data.kills = actor.data.kills;
            baryonUnit.data.health = (int)(actor.getMaxHealth());

            baryonUnit.addStatusEffect("status_jinchuriki_baryon_mode", 60f);
            baryonUnit.restoreHealth(actor.getMaxHealth());
            */

            actor.restoreHealth((int)(actor.getMaxHealth() * 0.5f));
            actor.addStatusEffect(StatusIds.NineTailsBaryon, 60f);

            if (ShinobiConfig.EnableWorldTips && !actor.isClone())
            { 
                ShinobiWorldLogs.AddWorldLog("log_baryon_mode", "worldlog_baryon_mode", "ui/icons/Baryon", actor);
            }

            return true;
        }

        public static void PostBaryonExhaustion(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;

            if (actor.hasTrait(TraitIds.NineTailsJinchuriki))
            {
                actor.removeTrait(TraitIds.NineTailsJinchuriki);
                actor.addStatusEffect(StatusIds.ChakraExhaustion, 120f);
                actor.addStatusEffect(StatusIds.AbilityCooldown, 300f);
            }
        }
        #endregion

        #endregion

        #region Ten Tails Jinchuriki
        public static bool TenTailsJinchuriki(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            Actor actor = pSelf.a;

            float maxHealth = actor.getMaxHealth();

            float healthPct = actor.data.health / maxHealth;
            int level = actor.data.level;

            bool hasJuubi1 = actor.hasStatus(StatusIds.Juubi1);
            bool hasJuubi2 = actor.hasStatus(StatusIds.Juubi2);
            bool hasSps = actor.hasStatus(StatusIds.SixPathsSenjutsu);

            if (!hasJuubi2 && level >= 5 && hasJuubi1)
            {
                actor.finishStatusEffect(StatusIds.Juubi1);
                actor.addStatusEffect(StatusIds.Juubi2, 60f);
                actor.restoreHealth(actor.getMaxHealth());
                hasJuubi2 = true;
            }
            else if (!hasJuubi1 && !hasJuubi2)
            {
                actor.addStatusEffect(StatusIds.Juubi1, 60f);
                hasJuubi1 = true;
            }

            if ((hasJuubi1 || hasJuubi2) && !hasSps)
            {
                actor.addStatusEffect(StatusIds.SixPathsSenjutsu, 60f);
                actor.setFlying(true);
            }

            if (!actor.hasTrait(TraitIds.Regeneration) && (hasJuubi1 || hasJuubi2))
            {
                actor.addTrait(TraitIds.Regeneration);
            }

            return hasJuubi1 || hasJuubi2;
        }
        #endregion
    }
}