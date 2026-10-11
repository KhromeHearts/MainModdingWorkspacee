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
    public class ShinobiProgression
    {
        private static readonly string[] JinchurikiFormStatuses =
        {
            StatusIds.NineTailsInitialRelease,
            StatusIds.NineTailsV1Cloak,
            StatusIds.NineTailsV2Cloak,
            StatusIds.NineTailsIncompleteBeast,
            StatusIds.NineTailsKcm1,
            StatusIds.NineTailsKcm2,
            StatusIds.NineTailsBaryon
        };

        #region IsModeActive
        public static bool IsModeActive(Actor a)
        {
            if (a == null) return false;

            foreach (string statusId in JinchurikiFormStatuses)
            {
                if (a.hasStatus(statusId)) return true;
            }

            return false;
        }
        #endregion

        #region CheckProgression
        public static void CheckProgression(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;

            int level = actor.data.level;
            int kills = actor.data.kills;
            int age = actor.data.getAge();
            ApplyNames(actor);
            ApplyAutoFavorite(actor);
            ApplyPassives(actor);

            RankProgression.CheckRankProgression(actor);

            RemoveFormsIfNoTrait(actor);

            // Rinnegan Awakening
            bool hasIndra = actor.hasTrait(TraitIds.IndraChakra);
            bool hasAsura = actor.hasTrait(TraitIds.AsuraChakra);
            bool hasHagoromo = actor.hasTrait(TraitIds.HagoromoChakra);
            bool hasMangekyo = actor.hasTrait(TraitIds.Mangekyo) || actor.hasTrait(TraitIds.MadaraEternalMangekyo);
            bool hasHashirama = actor.hasTrait(TraitIds.HashiramaCells);
            bool hasAnySenjutsu = actor.hasTrait(TraitIds.FrogSage) || actor.hasTrait(TraitIds.SlugSage) || actor.hasTrait(TraitIds.SnakeSage) || actor.hasTrait(TraitIds.WoodSage);
            string currentName = actor.data.name ?? string.Empty;
            bool hasKageInName = currentName.IndexOf("kage", StringComparison.OrdinalIgnoreCase) >= 0;

            bool canAwakenRinnegan =
                (hasHagoromo && age >= 40) ||
                (hasIndra && hasHashirama) ||
                (hasAsura && hasMangekyo) ||
                (hasIndra && hasAsura);

            if (canAwakenRinnegan)
            {
                if (!actor.hasTrait(TraitIds.Rinnegan))
                {
                    actor.removeTrait(TraitIds.Blind);
                    actor.addTrait(TraitIds.Rinnegan);
                    actor.restoreHealth(actor.getMaxHealth());
                    ShinobiWorldLogs.AddWorldLog("log_awaken_rinnegan", "worldlog_awaken_rinnegan", "ui/icons/rinnegan", actor);
                }
            }

            if (actor.hasTrait(TraitIds.Kage) || hasKageInName)
            {
                actor.data.set("was_kage", 1);
            }

            if (level >= 10 && hasAnySenjutsu && !actor.hasTrait(TraitIds.Sannin) && actor.isSapient())
            {
                actor.addTrait(TraitIds.Sannin);

                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_became_sannin", "worldlog_became_sannin", "ui/icons/rank_sannin", actor);
                }
            }

            // Ghost Of Uchiha Giver thingy thangy
            bool hasEternalEyes = actor.hasTrait(TraitIds.MadaraEternalMangekyo) || actor.hasTrait(TraitIds.EternalMangekyo);
            if (level >= 7 && actor.hasTrait(TraitIds.UchihaClan) && hasEternalEyes && !actor.hasTrait(TraitIds.GhostOfUchiha))
            {
                actor.addTrait(TraitIds.GhostOfUchiha);
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_ghost_of_uchiha", "worldlog_ghost_of_uchiha", "ui/icons/ghostofuchiha", actor);
                }
            }


            // god Of Shinobi Giver thingy thangy
            bool hasAllFiveNatures = actor.hasTrait(TraitIds.FireNature) && actor.hasTrait(TraitIds.WaterNature) && actor.hasTrait(TraitIds.EarthNature) && actor.hasTrait(TraitIds.LightningNature) && actor.hasTrait(TraitIds.WindNature);
            int wasKage = 0;
            actor.data.get("was_kage", out wasKage);
            bool qualifiesPastKage = actor.hasTrait(TraitIds.Kage) || hasKageInName || wasKage > 0;
            if (qualifiesPastKage && level >= 5 && hasAllFiveNatures && !actor.hasTrait(TraitIds.GodOfShinobi))
            {
                actor.addTrait(TraitIds.GodOfShinobi);
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_god_of_shinobi", "worldlog_god_of_shinobi", "ui/icons/godofshinobi", actor);
                }
            }

            if (actor.hasTrait(TraitIds.HagoromoChakra) && !actor.hasStatus(StatusIds.SixPathsSenjutsu))
            {
                actor.addStatusEffect(StatusIds.SixPathsSenjutsu, 30f);
            }

            if (actor.hasTrait(TraitIds.TenTailsJinchuriki) && actor.hasTrait(TraitIds.Rinnegan) && !actor.hasTrait(TraitIds.RinneSharingan))
            {
                actor.addTrait(TraitIds.RinneSharingan);
                if (!actor.hasStatus(StatusIds.SixPathsSenjutsu))
                {
                    actor.addStatusEffect(StatusIds.SixPathsSenjutsu, -1f);
                }
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_awaken_rinnesharingan", "worldlog_awaken_rinnesharingan", "ui/icons/rinne_sharingan", actor);
                }
            }

            // If unit is an anbu or jonin, they are automatically in the military
            if (actor.hasTrait(TraitIds.Anbu) || actor.hasTrait(TraitIds.Jonin))
            {
                actor.setProfession(UnitProfession.Warrior);
            }

            if (actor.isClone() && actor.isFavorite())
            {
                actor.data.favorite = false;
            }

            if (actor.isClone() && !actor.hasStatus(StatusIds.ShadowClone))
            {
                actor.addStatusEffect(StatusIds.ShadowClone, 5f);
            }

            //SageProgression.HandleSageProgression(actor, level);

            // Sharingan Progression
            if (actor.hasTrait(TraitIds.UchihaClan))
            {
                bool hasThreeTomoe = actor.hasStatus(StatusIds.Sharingan3Tomoe);

                // Mangekyo Awakening
                if (!actor.hasTrait(TraitIds.Mangekyo) && !actor.hasTrait(TraitIds.Blind) && !hasEternalEyes && hasThreeTomoe)
                {
                    if (HasTrauma(actor))
                    {
                        JutsuLibrary.ClearSharinganTomoeStatuses(actor);

                        actor.addTrait(TraitIds.Mangekyo);
                        actor.addStatusEffect(StatusIds.WillOfFireBuff);
                        ShinobiWorldLogs.AddWorldLog("log_mangekyo_awakened", "worldlog_mangekyo_awakened", "ui/icons/mangekyo", actor);
                    }
                }

                if (!actor.hasTrait(TraitIds.Sharingan) && !HasBetterEye(actor, 1))
                {
                    if (kills >= 1 || (age >= 12 && UnityEngine.Random.value < 0.001f)) actor.addTrait(TraitIds.Sharingan);
                }

            }

            #region Ten Tails Progression Stuff
            
            bool hasTen = actor.hasTrait(TraitIds.TenTailsJinchuriki);
            bool hasSPS = actor.hasStatus(StatusIds.SixPathsSenjutsu);

            if (hasSPS && !hasTen && !actor.hasTrait(TraitIds.HagoromoChakra))
            {
                actor.finishStatusEffect(StatusIds.SixPathsSenjutsu);
            }
            
            #endregion
            
        }
        #endregion

        #region Naming, Auto Favorite, and Hashirama Cells Helpers
        private static void ApplyNames(Actor actor)
        {
            if (!ShinobiConfig.EnableClanNames) return;

            if (actor.hasTrait(TraitIds.UchihaClan)) ApplyClanName(actor, "Uchiha");
            else if (actor.hasTrait(TraitIds.HyugaClan)) ApplyClanName(actor, "Hyuga");
            else if (actor.hasTrait(TraitIds.UzumakiClan)) ApplyClanName(actor, "Uzumaki");
            else if (actor.hasTrait(TraitIds.SenjuClan)) ApplyClanName(actor, "Senju");
            else if (actor.hasTrait(TraitIds.AkimichiClan)) ApplyClanName
            (actor, "Akimichi");
            else if (actor.hasTrait(TraitIds.LeeClan)) ApplyClanName(actor, "Lee");
            else if (actor.hasTrait(TraitIds.KaguyaClan)) ApplyClanName(actor, "Kaguya");
            else if (actor.hasTrait(TraitIds.OtsutsukiClan)) ApplyClanName(actor, "Otsutsuki");
            else if (actor.hasTrait(TraitIds.ChinoikeClan)) ApplyClanName(actor, "Chinoike");
            else if (actor.hasTrait(TraitIds.AburameClan)) ApplyClanName(actor, "Aburame");

            if (actor.hasTrait(TraitIds.Kage))
            {
                ApplyKageName(actor, "Kage");
            }
        }

        private static void ApplyAutoFavorite(Actor actor)
        {
            if (!ShinobiConfig.EnableAutoFavorite) return;

            int favoriteOptOut = 0;
            actor.data.get("auto_favorite_opt_out", out favoriteOptOut);
            if (favoriteOptOut > 0) return;

            int autoFavoritedBefore = 0;
            actor.data.get("auto_favorited_once", out autoFavoritedBefore);
            if (autoFavoritedBefore > 0 && !actor.data.favorite)
            {
                actor.data.set("auto_favorite_opt_out", 1);
                return;
            }

            if (actor.data.favorite) return;

            bool hasAnySenjutsu = actor.hasTrait(TraitIds.FrogSage) || actor.hasTrait(TraitIds.SlugSage) || actor.hasTrait(TraitIds.SnakeSage) || actor.hasTrait(TraitIds.WoodSage);

            if (actor.hasTrait(TraitIds.IndraChakra) || actor.hasTrait(TraitIds.AsuraChakra) ||
                actor.hasTrait(TraitIds.Mangekyo) || actor.hasTrait(TraitIds.MadaraEternalMangekyo) || actor.hasTrait(TraitIds.EternalMangekyo) ||
                actor.hasTrait(TraitIds.Rinnegan) || actor.hasTrait(TraitIds.NineTailsJinchuriki) || actor.hasTrait(TraitIds.RinneSharingan) ||
                actor.hasTrait(TraitIds.HagoromoChakra) || actor.hasTrait(TraitIds.TenTailsJinchuriki) || hasAnySenjutsu)
            {
                actor.data.favorite = true;
                actor.data.set("auto_favorited_once", 1);
            }
        }

        private static void ApplyPassives(Actor actor)
        {
            if (actor.hasTrait(TraitIds.HashiramaCells))
            {
                if (actor.hasTrait(TraitIds.Crippled)) actor.removeTrait(TraitIds.Crippled);
                if (actor.hasTrait(TraitIds.Eyepatch)) actor.removeTrait(TraitIds.Eyepatch);
            }
        }
        #endregion

        #region HasTrauma (such a dumb name right?)
        private static bool HasTrauma(Actor a)
        {
            foreach (var historyItem in a.happiness_change_history)
            {
                if (historyItem.asset != null)
                {
                    string id = historyItem.asset.id;
                    if (id == "death_family_member" || id == "death_best_friend" || id == "death_child" || id == "death_lover")
                    {
                        if (UnityEngine.Random.value < 0.80f)
                        {
                            return true;
                        }
                    }
                }
            }

            if (a.data.kills >= 25)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region ApplyClanName
        private static void ApplyClanName(Actor actor, string clanName)
        {
            string currentName = actor.data.name;
            if (string.IsNullOrEmpty(currentName)) return;

            if (currentName.IndexOf(clanName, StringComparison.OrdinalIgnoreCase) < 0)
            {
                actor.data.name = $"{currentName} {clanName}";
            }
        }
        #endregion

        #region ApplyKageName
        private static void ApplyKageName(Actor actor, string kageTitle)
        {
            string currentName = actor.data.name;
            if (string.IsNullOrEmpty(currentName)) return;

            if (currentName.IndexOf(kageTitle, StringComparison.OrdinalIgnoreCase) < 0)
            {
                actor.data.name = $"{kageTitle} {currentName}";
            }
        }
        #endregion

        #region HasBetterEye
        private static bool HasBetterEye(Actor a, int currentTier)
        {
            if (currentTier < 2 && (a.hasStatus(StatusIds.Sharingan2Tomoe) || a.hasStatus(StatusIds.Sharingan3Tomoe) || a.hasTrait(TraitIds.Mangekyo) || a.hasTrait(TraitIds.Blind))) return true;
            if (currentTier < 3 && (a.hasStatus(StatusIds.Sharingan3Tomoe) || a.hasTrait(TraitIds.Mangekyo) || a.hasTrait(TraitIds.Blind))) return true;
            if (currentTier < 4 && (a.hasTrait(TraitIds.Mangekyo) || a.hasTrait(TraitIds.Blind))) return true;
            return false;
        }
        #endregion

        #region removeWeapon
        public static void removeWeapon(Actor actor)
        {
            if (actor.hasTrait(TraitIds.TaijutsuMaster) && actor.hasWeapon())
            {
                actor.takeAwayItems();
            }
        }
        #endregion

        #region RemoveLowerForms
        public static void RemoveLowerForms(Actor a)
        {
            if (a == null) return;

            foreach (string statusId in JinchurikiFormStatuses)
            {
                a.finishStatusEffect(statusId);
            }
        }

        public static void RemoveFormsIfNoTrait(Actor a)
        {
            if (a != null && !a.hasTrait(TraitIds.NineTailsJinchuriki)) RemoveLowerForms(a);
        }
        #endregion
    
    }
}