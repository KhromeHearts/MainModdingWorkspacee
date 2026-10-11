using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GOWB
{
    public static class GOWConfig
    {
        private const string GroupIdGowr = "GOWRagnarok";

        private static readonly string[] LockedTraitIds =
        {
            "Odin Phase 2",
            "Bjorn",
            "Jormungandr"
        };

        public static bool EnableNaturalSpawns = false;
        public static bool UnlockLockedTraits = false;
        public static bool EnableTraitOpposites = true;

        public static void SetGroup1NaturalSpawns(bool value)
        {
            SetNaturalSpawns(value);
        }

        public static void SetNaturalSpawns(bool value)
        {
            EnableNaturalSpawns = value;
            ApplyTraitConfigSettings();
            Debug.Log("[GOWB] EnableNaturalSpawns set to: " + value);
        }

        public static void SetUnlockLockedTraits(bool value)
        {
            UnlockLockedTraits = value;
            ApplyTraitConfigSettings();
            Debug.Log("[GOWB] UnlockLockedTraits set to: " + value);
        }

        public static void SetTraitOpposites(bool value)
        {
            EnableTraitOpposites = value;
            ApplyTraitConfigSettings();
            Debug.Log("[GOWB] EnableTraitOpposites set to: " + value);
        }

        public static void ApplyConfigToTrait(ActorTrait trait)
        {
            if (trait == null)
            {
                return;
            }

            if (trait.group_id == GroupIdGowr)
            {
                trait.rate_birth = GOWTraits.ConfiguredBirthRate(EnableNaturalSpawns);
            }

            if (LockedTraitIds.Contains(trait.id))
            {
                trait.can_be_given = UnlockLockedTraits;
            }
        }

        public static void ApplyTraitConfigSettings()
        {
            if (AssetManager.traits?.dict == null || AssetManager.traits.dict.Count == 0)
            {
                return;
            }

            ApplyBirthRates();
            ApplyLockedTraitAvailability();
            ApplyTraitOpposites();
        }

        private static void ApplyTraitOpposites()
        {
            GOWTraits.populateListOppositeTraits(EnableTraitOpposites);
        }

        private static void ApplyBirthRates()
        {
            int targetRate = GOWTraits.ConfiguredBirthRate(EnableNaturalSpawns);
            List<ActorTrait> group1Traits = AssetManager.traits.dict.Values
                .Where(t => t != null && t.group_id == GroupIdGowr)
                .ToList();

            if (group1Traits.Count == 0)
            {
                return;
            }

            foreach (ActorTrait trait in group1Traits)
            {
                trait.rate_birth = targetRate;
            }

            List<ActorTrait> birthPool = AssetManager.traits.pot_traits_birth;
            if (birthPool == null)
            {
                return;
            }

            birthPool.RemoveAll(t => t != null && t.group_id == GroupIdGowr);
            if (targetRate <= 0)
            {
                return;
            }

            foreach (ActorTrait trait in group1Traits)
            {
                for (int i = 0; i < targetRate; i++)
                {
                    birthPool.Add(trait);
                }
            }

            Debug.Log("[GOWB] Applied Group 1 natural spawns=" + EnableNaturalSpawns + ", traits updated=" + group1Traits.Count + ", target rate=" + targetRate);
        }

        private static void ApplyLockedTraitAvailability()
        {
            foreach (string traitId in LockedTraitIds)
            {
                ActorTrait trait = AssetManager.traits.get(traitId);
                if (trait != null)
                {
                    trait.can_be_given = UnlockLockedTraits;
                }
            }
        }
    }
}