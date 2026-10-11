using UnityEngine;

namespace ShinobiBox
{
    public static class RankProgression
    {
        public static void CheckRankProgression(Actor a)
        {
            if (!ShinobiConfig.EnableAutoRanking) return;
            if (!a.isSapient()) return;

            int level = a.data.level;
            int kills = a.data.kills;
            int age = a.data.getAge();

            if (a.isKing() && !a.hasTrait(TraitIds.Kage))
            {
                RemoveAllRanks(a);
                a.addTrait(TraitIds.Kage);
                a.restoreHealth(a.getMaxHealth());
                return;
            }

            bool anyChakraNature = JutsuLibrary.HasAnyNatureTrait(a);

            bool gifted = a.hasTrait(TraitIds.HighChakraReserve) || a.hasTrait(TraitIds.WillOfFire) || a.hasTrait(TraitIds.CurseOfHatred) ||
            a.hasTrait(TraitIds.GreaterChakraReserve) ||
            a.hasTrait(TraitIds.VastChakraReserve) || a.hasTrait(TraitIds.Genius) || anyChakraNature;

            bool isUnranked = !a.hasTrait(TraitIds.AcademyStudent) &&
            !a.hasTrait(TraitIds.Genin) && !a.hasTrait(TraitIds.Chunin) &&
            !a.hasTrait(TraitIds.Jonin) && !a.hasTrait(TraitIds.Anbu) &&
            !a.hasTrait(TraitIds.Kage) && !a.hasTrait(TraitIds.Sannin) &&
            !a.hasTrait(TraitIds.GhostOfUchiha) &&
            !a.hasTrait(TraitIds.GodOfShinobi);

            if (isUnranked && age >= 12 && age < 40 && gifted)
            {
                float chance = gifted ? 0.04f : 0.0f;
                if (Randy.randomChance(chance))
                {
                    a.addTrait(TraitIds.AcademyStudent);
                }
            }

            if (a.hasTrait(TraitIds.AcademyStudent) && (level >= 3 || (age >= 16 && kills >= 2)))
            {
                a.removeTrait(TraitIds.AcademyStudent);
                a.addTrait(TraitIds.Genin);
                return;
            }

            if (a.hasTrait(TraitIds.Genin) && (level >= 5 || kills >= 8))
            {
                a.removeTrait(TraitIds.Genin);
                a.addTrait(TraitIds.Chunin);
                return;
            }

            if (a.hasTrait(TraitIds.Chunin) && (level >= 7 || kills >= 20))
            {
                a.removeTrait(TraitIds.Chunin);
                a.addTrait(TraitIds.Jonin);
                return;
            }

            if (a.hasTrait(TraitIds.Jonin) && (level >= 9 || kills >= 35))
            {
                a.removeTrait(TraitIds.Jonin);
                a.addTrait(TraitIds.Anbu);
                return;
            }
        }

        private static void RemoveAllRanks(Actor a)
        {
            a.removeTrait(TraitIds.AcademyStudent);
            a.removeTrait(TraitIds.Genin);
            a.removeTrait(TraitIds.Chunin);
            a.removeTrait(TraitIds.Jonin);
            a.removeTrait(TraitIds.Anbu);
            a.removeTrait(TraitIds.Kage);
        }
    }
}