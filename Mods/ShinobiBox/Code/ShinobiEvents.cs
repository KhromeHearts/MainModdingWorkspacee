using System;
using System.Threading;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Linq;
using ai;
using NeoModLoader.services;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using NCMS.Utils;
using UnityEngine.Events;
using HarmonyLib;
using System.Collections.Generic;


namespace ShinobiBox
{

    [HarmonyPatch(typeof(Actor), "getHit")]
    public static class rasenganFinish
    {
        static void Postfix(Actor __instance, BaseSimObject pAttacker)
        {
            if (pAttacker != null && pAttacker.a != null && pAttacker.a.hasStatus(StatusIds.Rasengan))
            {
                pAttacker.a.finishStatusEffect(StatusIds.Rasengan);
            }
        }
    }

    [HarmonyPatch(typeof(BabyMaker), "makeBaby")]
    public static class HagoromoBirthPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Actor __result, Actor pParent1, Actor pParent2)
        {
            if (__result == null || __result.asset == null || __result.asset.id != "human") return;
            if (__result.hasTrait(TraitIds.IndraChakra) || __result.hasTrait(TraitIds.AsuraChakra)) return;

            bool parentHasHagoromo =
                (pParent1 != null && pParent1.hasTrait(TraitIds.HagoromoChakra)) ||
                (pParent2 != null && pParent2.hasTrait(TraitIds.HagoromoChakra));

            if (!parentHasHagoromo) return;

            if (UnityEngine.Random.value < 0.5f)
            {
                __result.addTrait(TraitIds.IndraChakra);
            }
            else
            {
                __result.addTrait(TraitIds.AsuraChakra);
            }
        }
    }

    [HarmonyPatch(typeof(BabyMaker), "makeBaby")]
    public static class GuaranteedInheritancePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Actor __result, Actor pParent1, Actor pParent2)
        {
            if (__result == null) return;

            ApplyGuaranteedTraitsFromParent(__result, pParent1);
            ApplyGuaranteedTraitsFromParent(__result, pParent2);

            if (__result.hasTrait(TraitIds.OtsutsukiClan) && !__result.hasTrait(TraitIds.HamuraChakra) && !__result.hasTrait(TraitIds.HagoromoChakra))
            {
                if (Randy.randomChance(0.01f))
                {
                    __result.addTrait(TraitIds.HagoromoChakra);
                }
                else if (Randy.randomChance(0.01f))
                {
                    __result.addTrait(TraitIds.HamuraChakra);
                }
            }

            if (__result.hasTrait(TraitIds.OtsutsukiClan))
            {
                float roll = UnityEngine.Random.value;

                if (roll < 0.05f)
                {
                    __result.addTrait(TraitIds.Rinnegan);
                }
                else if (roll < 0.20f)
                {
                    __result.addTrait(TraitIds.GoldenByakugan);
                }
                else if (roll < 0.30f && __result.hasTrait(TraitIds.HamuraChakra))
                {
                    __result.addTrait(TraitIds.Tenseigan);
                }
                else
                {
                    __result.addTrait(TraitIds.Byakugan);
                }
            }

            // NOTE: the clan birth rules below used to be nested INSIDE the Otsutsuki check above.
            // Clans are mutually exclusive, so an Otsutsuki baby could never also be Chinoike/Hyuga/Lee/etc.
            // and none of these rules could ever run. They now run for the matching clan.
            //
            // The old Senju block (5% wood_release / 0.1% hashi_cells) was removed from here on purpose:
            // BirthTraits.ApplyTraitsOnBirth already gives Senju babies a 1% chance of each,
            // and moving the old block out would have quietly made Senju stronger.

            if (!__result.hasTrait(TraitIds.Ketsuryugan) && __result.hasTrait(TraitIds.ChinoikeClan))
            {
                if (Randy.randomChance(0.50f))
                {
                    __result.addTrait(TraitIds.Ketsuryugan);
                }
            }

            if (!__result.hasTrait(TraitIds.Byakugan) && __result.hasTrait(TraitIds.HyugaClan))
            {
                __result.addTrait(TraitIds.Byakugan);
            }

            if (!__result.hasTrait(TraitIds.EightInnerGates) && __result.hasTrait(TraitIds.LeeClan))
            {
                if (Randy.randomChance(0.20f))
                {
                    __result.addTrait(TraitIds.EightInnerGates);
                }
            }

            if (!__result.hasTrait(TraitIds.Shikotsumyaku) && __result.hasTrait(TraitIds.KaguyaClan))
            {
                if (Randy.randomChance(0.05f))
                {
                    __result.addTrait(TraitIds.Shikotsumyaku);
                }
            }

            if (!__result.hasTrait(TraitIds.Sharingan) && __result.hasTrait(TraitIds.UchihaClan))
            {
                if (Randy.randomChance(0.01f))
                {
                    __result.addTrait(TraitIds.Sharingan);
                }
            }
        }

        private static void ApplyGuaranteedTraitsFromParent(Actor child, Actor parent)
        {
            if (child == null || parent == null) return;

            foreach (ActorTrait trait in parent.getTraits())
            {
                if (trait == null) continue;
                if (trait.rate_inherit < 100) continue;

                child.addTrait(trait.id);
            }
        }

    }


    #region JinchurikiStressPatch
    [HarmonyPatch(typeof(Actor), "getHit")]
    public static class JinchurikiStressPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Actor __instance, float pDamage)
        {
            if (__instance == null || !__instance.isAlive()) return;

            if (!__instance.hasTrait(TraitIds.NineTailsJinchuriki)) return;

            if ((__instance.hasStatus(StatusIds.NineTailsInitialRelease) || __instance.hasStatus(StatusIds.NineTailsV1Cloak) ||
                __instance.hasStatus(StatusIds.NineTailsV2Cloak) || __instance.hasStatus(StatusIds.NineTailsIncompleteBeast)) &&
                UnityEngine.Random.value < 0.15f && !__instance.hasStatus(StatusIds.Angry))
            {
                __instance.addStatusEffect(StatusIds.Angry, 10f);
            }
        }
    }
    #endregion

    #region Mangekyo To EMS
    // Mangekyo To EMS
    [HarmonyPatch(typeof(Actor), "getHit")]
    public static class EMSKillPatch
    {
        private const float EyeStealChance = 0.009f;

        [HarmonyPrefix]
        public static void Prefix(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker)
        {
            if (__instance == null || !__instance.isAlive() || pAttacker == null || pAttacker.a == null) return;

            if (__instance.data.health - pDamage <= 0)
            {

                if (UnityEngine.Random.value < EyeStealChance)
                {
                    string stolenSharinganTier = GetStealableSharinganTier(__instance);
                    if (!string.IsNullOrEmpty(stolenSharinganTier))
                    {
                        RemoveAllSharinganTiers(pAttacker.a);
                        pAttacker.a.addTrait(stolenSharinganTier);

                        pAttacker.a.removeTrait(TraitIds.Blind);
                        pAttacker.a.data.set("stolen_sharingan_eye", 1);
                        pAttacker.a.addStatusEffect(StatusIds.WeakEye, 450f);
                    }
                }

                if (UnityEngine.Random.value < EyeStealChance && __instance.hasTrait(TraitIds.Byakugan))
                {
                    if (!pAttacker.a.hasTrait(TraitIds.Byakugan))
                    {
                        pAttacker.a.addTrait(TraitIds.Byakugan);
                    }
                    pAttacker.a.data.set("stolen_byakugan_eye", 1);
                    pAttacker.a.addStatusEffect(StatusIds.WeakEye, 450f);
                }

                // EMS Awaken.
                if (__instance.hasTrait(TraitIds.Mangekyo) || __instance.hasTrait(TraitIds.Blind))
                {
                    if (pAttacker.a.hasTrait(TraitIds.Mangekyo) || pAttacker.a.hasTrait(TraitIds.Blind))
                    {
                        pAttacker.a.removeTrait(TraitIds.Mangekyo);
                        pAttacker.a.removeTrait(TraitIds.Blind);

                        pAttacker.a.addTrait(TraitIds.EternalMangekyo);
                        pAttacker.a.restoreHealth(pAttacker.a.getMaxHealth());

                        ShinobiWorldLogs.AddWorldLog("log_awaken_ems", "worldlog_awaken_ems", "ui/icons/ems", pAttacker.a);
                    }

                }
            }
        }

        private static string GetStealableSharinganTier(Actor victim)
        {
            if (victim == null) return null;

            if (victim.hasTrait(TraitIds.MadaraEternalMangekyo)) return TraitIds.MadaraEternalMangekyo;
            if (victim.hasTrait(TraitIds.EternalMangekyo)) return TraitIds.EternalMangekyo;
            if (victim.hasTrait(TraitIds.Mangekyo)) return TraitIds.Mangekyo;
            if (victim.hasStatus(StatusIds.Sharingan3Tomoe) || victim.hasStatus(StatusIds.Sharingan2Tomoe) || victim.hasStatus(StatusIds.Sharingan1Tomoe)) return TraitIds.Sharingan;
            if (victim.hasTrait(TraitIds.Sharingan)) return TraitIds.Sharingan;

            return null;
        }

        private static void RemoveAllSharinganTiers(Actor actor)
        {
            if (actor == null) return;

            actor.finishStatusEffect(StatusIds.Sharingan1Tomoe);
            actor.finishStatusEffect(StatusIds.Sharingan2Tomoe);
            actor.finishStatusEffect(StatusIds.Sharingan3Tomoe);

            actor.removeTrait(TraitIds.Mangekyo);
            actor.removeTrait(TraitIds.MadaraEternalMangekyo);
            actor.removeTrait(TraitIds.EternalMangekyo);
        }

    }
    #endregion

    // creds to clonedschrott for this code
    #region Unit Maker
    [HarmonyPatch(typeof(Actor), "generatePersonality")]
    public static class GeneratePersonalityPatch
    {
        [HarmonyPostfix]
        public static void generatePersonality_Postfix(Actor __instance)
        {
            if (__instance.hasTrait(TraitIds.WindNature) && !__instance.hasTrait(TraitIds.Rasengan) && Randy.randomChance(0.15f))
                __instance.addTrait(TraitIds.Rasengan);

            if (__instance.hasTrait(TraitIds.LightningNature))
            {
                if (!__instance.hasTrait(TraitIds.Chidori) && Randy.randomChance(0.15f))
                {
                    __instance.addTrait(TraitIds.Chidori);
                }
            }

            if (__instance.data["birth_traits_assigned"] < 0.5f)
            {
                BirthTraits.AssignChakraTrait(__instance);
                BirthTraits.ApplyTraitsOnBirth(__instance);

                try { __instance.data.set("birth_traits_assigned", 1); } catch { }
            }
        }
    }
    #endregion

    #region Birth Traits
    public static class BirthTraits
    {
        private static readonly string[] ChakraReserveTraits =
        {
            TraitIds.LowChakraReserve,
            TraitIds.HighChakraReserve,
            TraitIds.GreaterChakraReserve,
            TraitIds.VastChakraReserve
        };

        public static void AssignChakraTrait(Actor actor)
        {
            if (actor == null || !actor.isSapient()) return;

            foreach (string traitId in ChakraReserveTraits)
            {
                if (actor.hasTrait(traitId)) return;
            }

            float roll = UnityEngine.Random.value;
            string selectedTrait = null;
            if (roll < 0.10f) selectedTrait = TraitIds.VastChakraReserve;
            else if (roll < 0.30f) selectedTrait = TraitIds.GreaterChakraReserve;
            else if (roll < 0.65f) selectedTrait = TraitIds.HighChakraReserve;
            else if (roll < 0.90f) selectedTrait = TraitIds.LowChakraReserve;

            if (!string.IsNullOrEmpty(selectedTrait)) actor.addTrait(selectedTrait);
        }

        public static void ApplyTraitsOnBirth(Actor actor)
        {
            if (!actor.isSapient()) return;

            string[] commonElements = { TraitIds.FireNature, TraitIds.EarthNature, TraitIds.WaterNature, TraitIds.WindNature, TraitIds.LightningNature };
            int elementCount = 0;
            foreach (var element in commonElements)
            {
                if (actor.hasTrait(element)) continue;
                float chance = elementCount >= 3 ? 0.05f : 0.30f;
                if (Randy.randomChance(chance))
                {
                    actor.addTrait(element);
                    elementCount++;
                }
            }

            if (!actor.hasTrait(TraitIds.StormRelease))
            {
                float stormChance = elementCount >= 3 ? 0.02f : 0.10f;
                if (Randy.randomChance(stormChance))
                    actor.addTrait(TraitIds.StormRelease);
            }

            if (Randy.randomChance(0.15f))
            {
                actor.addTrait(TraitIds.AcademyStudent);
            }

            if (actor.hasTrait(TraitIds.SenjuClan))
            {
                if (!actor.hasTrait(TraitIds.HashiramaCells) && Randy.randomChance(0.01f))
                {
                    actor.addTrait(TraitIds.HashiramaCells);
                    if (ShinobiConfig.EnableWorldTips)
                    {
                        ShinobiWorldLogs.AddWorldLog("log_hashi_cells", "worldlog_hashi_cells", "ui/icons/hashirama_cells", actor, "spawn");
                    }
                }

                if (!actor.hasTrait(TraitIds.WoodRelease) && Randy.randomChance(0.01f))
                {
                    actor.addTrait(TraitIds.WoodRelease);
                    if (ShinobiConfig.EnableWorldTips)
                    {
                        ShinobiWorldLogs.AddWorldLog("log_wood_release", "worldlog_wood_release", "ui/icons/wood_release", actor, "spawn");
                    }
                }
            }


            if (actor.hasTrait(TraitIds.OtsutsukiClan))
            {
                if (!actor.hasTrait(TraitIds.HagoromoChakra) && !actor.hasTrait(TraitIds.HamuraChakra))
                {
                    actor.addTrait(UnityEngine.Random.value < 0.5f ? TraitIds.HagoromoChakra : TraitIds.HamuraChakra);
                }

                if (!actor.hasTrait(TraitIds.Byakugan) && !actor.hasTrait(TraitIds.Rinnegan) && Randy.randomChance(0.35f))
                {
                    actor.addTrait(actor.hasTrait(TraitIds.HamuraChakra) ? TraitIds.Byakugan : TraitIds.Rinnegan);
                }
            }
        }
    }
    #endregion

}
