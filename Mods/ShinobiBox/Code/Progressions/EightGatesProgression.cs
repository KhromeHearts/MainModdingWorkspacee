using UnityEngine;

namespace ShinobiBox
{
    public static class EightGatesProgression
    {
        public static bool EightInnerGates(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            // Simple Gate Checks
            if (!actor.hasTrait(TraitIds.EightInnerGates)) return false;
            if (actor.hasStatus(StatusIds.AbilityCooldown)) return false;

            // Only activates if health drops below 70%
            float maxHealth = actor.getMaxHealth();
            if (actor.data.health > maxHealth * 0.70f) return false;

            if (UnityEngine.Random.value > 0.20f) return false;

            // Gate 8: Death Gate
            if (!actor.hasStatus(StatusIds.InnerGate8) && actor.data.level >= 8)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate8, 45f);
                return true;
            }
            // Gate 7: Wonder Gate
            else if (!actor.hasStatus(StatusIds.InnerGate7) && actor.data.level >= 7)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate7, 60f);
                return true;
            }
            // Gate 6: View Gate
            else if (!actor.hasStatus(StatusIds.InnerGate6) && actor.data.level >= 6)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate6, 55f);
                return true;
            }
            // Gate 5: Limit Gate
            else if (!actor.hasStatus(StatusIds.InnerGate5) && actor.data.level >= 5)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate5, 50f);
                if (UnityEngine.Random.value < 0.25f && !actor.hasTrait(TraitIds.Crippled))
                {
                    actor.addTrait(TraitIds.Crippled);
                }
                return true;
            }
            // Gate 4: Wound Gate
            else if (!actor.hasStatus(StatusIds.InnerGate4) && actor.data.level >= 4)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate4, 45f);
                return true;
            }
            // Gate 3: Life Gate
            else if (!actor.hasStatus(StatusIds.InnerGate3) && actor.data.level >= 3)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate3, 40f);
                return true;
            }
            // Gate 2: Healing Gate
            else if (!actor.hasStatus(StatusIds.InnerGate2) && actor.data.level >= 2)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate2, 35f);
                actor.restoreHealth((int)(maxHealth * 0.20f));
                return true;
            }
            // Gate 1: Opening Gate
            else if (!actor.hasStatus(StatusIds.InnerGate1) && actor.data.level >= 1)
            {
                removeAllGates(actor);
                actor.addStatusEffect(StatusIds.InnerGate1, 30f);
                return true;
            }

            return false;
        }

        public static bool InnerGatesStrain(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            int damageAmount = 0;

            if (actor.hasStatus(StatusIds.InnerGate8)) damageAmount = 25;
            else if (actor.hasStatus(StatusIds.InnerGate7)) damageAmount = 12;
            else if (actor.hasStatus(StatusIds.InnerGate6)) damageAmount = 8;
            else if (actor.hasStatus(StatusIds.InnerGate5)) damageAmount = 5;
            else if (actor.hasStatus(StatusIds.InnerGate4)) damageAmount = 2;

            if (damageAmount > 0)
            {
                actor.getHit(damageAmount, true, AttackType.Other, null, false, false);

                if (damageAmount >= 5 && actor.current_tile != null && UnityEngine.Random.value < 0.15f)
                {
                    World.world.applyForceOnTile(actor.current_tile, 5, 1.5f, true, 0);
                }
                return true;
            }

            return false;
        }

        public static void removeAllGates(Actor actor)
        {
            actor.finishStatusEffect(StatusIds.InnerGate1);
            actor.finishStatusEffect(StatusIds.InnerGate2);
            actor.finishStatusEffect(StatusIds.InnerGate3);
            actor.finishStatusEffect(StatusIds.InnerGate4);
            actor.finishStatusEffect(StatusIds.InnerGate5);
            actor.finishStatusEffect(StatusIds.InnerGate6);
            actor.finishStatusEffect(StatusIds.InnerGate7);
            actor.finishStatusEffect(StatusIds.InnerGate8);
        }
    }
}