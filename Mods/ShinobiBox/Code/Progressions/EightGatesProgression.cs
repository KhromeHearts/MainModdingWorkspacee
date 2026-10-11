using UnityEngine;

namespace ShinobiBox
{
    public static class EightGatesProgression
    {
        private static readonly string[] Gates =
        {
            "inner_gate_1", "inner_gate_2", "inner_gate_3", "inner_gate_4",
            "inner_gate_5", "inner_gate_6", "inner_gate_7", "inner_gate_8"
        };

        public static bool EightInnerGates(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            if (!actor.hasTrait("eight_inner_gates")) return false;
            if (actor.hasStatus("eight_gate_strain")) return false;

            float maxHealth = actor.getMaxHealth();
            if (actor.data.health > maxHealth * 0.70f) return false;

            if (UnityEngine.Random.value > 0.20f) return false;

            int openGate = GetOpenGate(actor);
            int highestGate = Mathf.Min(actor.data.level, Gates.Length);
            if (openGate >= highestGate) return false;

            int nextGate = openGate + 1;
            removeAllGates(actor);
            actor.addStatusEffect(Gates[nextGate - 1]);

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

        public static int GetOpenGate(Actor actor)
        {
            for (int i = Gates.Length; i >= 1; i--)
            {
                if (actor.hasStatus(Gates[i - 1])) return i;
            }
            return 0;
        }

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

        public static bool HighGateClosed(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;

            pSelf.a.addStatusEffect("eight_gate_strain", 30f);
            return true;
        }

        public static bool DeathGateClosed(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.isAlive()) return false;
            Actor actor = pSelf.a;

            int barelyAlive = Mathf.Max(1, (int)(actor.getMaxHealth() * 0.10f));
            if (actor.data.health > barelyAlive) actor.data.health = barelyAlive;

            actor.addStatusEffect("eight_gate_strain", 90f);
            actor.addTrait("crippled");
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
