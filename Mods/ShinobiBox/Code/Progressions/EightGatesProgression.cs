using UnityEngine;

namespace ShinobiBox
{
    public static class EightGatesProgression
    {
        private static readonly string[] Gates =
        {
            StatusIds.InnerGate1, StatusIds.InnerGate2, StatusIds.InnerGate3, StatusIds.InnerGate4,
            StatusIds.InnerGate5, StatusIds.InnerGate6, StatusIds.InnerGate7, StatusIds.InnerGate8
        };

        // Hooked on the Eight Inner Gates trait (action_get_hit).
        // Below 70% health, each hit has a 20% chance to open the NEXT gate. A unit can open one gate per level (max 8).
        public static bool EightInnerGates(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            if (!actor.hasTrait(TraitIds.EightInnerGates)) return false;
            // The body has to recover after a high gate closes before it can open them again.
            if (actor.hasStatus(StatusIds.EightGateStrain)) return false;

            float maxHealth = actor.getMaxHealth();
            if (actor.data.health > maxHealth * 0.70f) return false;

            if (UnityEngine.Random.value > 0.20f) return false;

            int openGate = GetOpenGate(actor);
            int highestGate = Mathf.Min(actor.data.level, Gates.Length);
            if (openGate >= highestGate) return false;

            int nextGate = openGate + 1;
            removeAllGates(actor);
            actor.addStatusEffect(Gates[nextGate - 1]);

            // Gate 2: Gate of Healing
            if (nextGate == 2)
            {
                actor.restoreHealth((int)(maxHealth * 0.20f));
            }

            if (nextGate == 8 && ShinobiConfig.EnableWorldTips)
            {
                ShinobiWorldLogs.AddWorldLog("log_death_gate", "worldlog_death_gate", "ui/icons/eight_gates", actor);
            }

            return true;
        }

        // 0 = no gate open, otherwise 1-8.
        public static int GetOpenGate(Actor actor)
        {
            for (int i = Gates.Length; i >= 1; i--)
            {
                if (actor.hasStatus(Gates[i - 1])) return i;
            }
            return 0;
        }

        // Status action on gates 4-8, once per second: the body tears itself apart while the gate is open.
        public static bool InnerGatesStrain(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            float percent;
            switch (GetOpenGate(actor))
            {
                case 4: percent = 0.005f; break;
                case 5: percent = 0.010f; break;
                case 6: percent = 0.015f; break;
                case 7: percent = 0.020f; break;
                case 8: percent = 0.025f; break;
                default: return false;
            }

            float damage = Mathf.Max(1f, actor.getMaxHealth() * percent);
            actor.getHit(damage, true, AttackType.Other, null, false, false);
            return true;
        }

        // Status action_finish on gates 5-7: when the gate closes on its own, the body pays for it.
        public static bool HighGateClosed(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;

            pSelf.a.addStatusEffect(StatusIds.EightGateStrain, 30f);
            return true;
        }

        // Status action_finish on gate 8: the Gate of Death leaves the user barely alive.
        public static bool DeathGateClosed(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            int barelyAlive = Mathf.Max(1, (int)(actor.getMaxHealth() * 0.10f));
            if (actor.data.health > barelyAlive) actor.data.health = barelyAlive;

            actor.addStatusEffect(StatusIds.EightGateStrain, 90f);
            return true;
        }

        public static void removeAllGates(Actor actor)
        {
            foreach (string gate in Gates)
            {
                actor.finishStatusEffect(gate);
            }
        }
    }
}
