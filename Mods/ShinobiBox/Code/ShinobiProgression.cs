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
        public static int AkatsukiMembers = 0;
        private const int MaxAkatsukiMembers = 10;

        private static readonly string[] JinchurikiFormStatuses =
        {
            "status_jinchuriki_initial_release",
            "status_jinchuriki_v1_cloak",
            "status_jinchuriki_v2_cloak",
            "status_jinchuriki_incomplete_beast",
            "status_jinchuriki_kcm1",
            "status_jinchuriki_kcm2",
            "status_jinchuriki_baryon_mode"
        };


        #region CheckProgression
        public static void CheckProgression(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;

            int level = actor.data.level;
            int kills = actor.data.kills;
            int age = actor.data.getAge();
            BirthTraits.AssignOnce(actor);
            ApplyNames(actor);
            ApplyAutoFavorite(actor);
            ApplyPassives(actor);

            RankProgression.CheckRankProgression(actor);

            RemoveFormsIfNoTrait(actor);
            ShinobiActors.CharacterBehaviour(actor);

            // Rinnegan Awakening
            bool hasIndra = actor.hasTrait("07_indra_chakra");
            bool hasAsura = actor.hasTrait("08_asura_chakra");
            bool hasHagoromo = actor.hasTrait("05_hagoromo_chakra");
            bool hasMangekyo = actor.hasTrait("mangekyo_sharingan") || actor.hasTrait("madara_eternal_mangekyo");
            bool hasHashirama = actor.hasTrait("hashi_cells");
            bool hasAnySenjutsu = SageProgression.IsSage(actor);
            string currentName = actor.data.name ?? string.Empty;
            bool hasKageInName = currentName.IndexOf("kage", StringComparison.OrdinalIgnoreCase) >= 0;

            bool canAwakenRinnegan =
                (hasHagoromo && age >= 40) ||
                (hasIndra && hasHashirama) ||
                (hasAsura && hasMangekyo) ||
                (hasIndra && hasAsura);

            if (canAwakenRinnegan && GiveOnce(actor, "rinnegan"))
            {
                actor.removeTrait("trait_blind");
                actor.restoreHealth(actor.getMaxHealth());
                ShinobiWorldLogs.AddWorldLog("log_awaken_rinnegan", "worldlog_awaken_rinnegan", "ui/icons/rinnegan", actor);
            }

            if (actor.hasTrait("06rank_kage") || hasKageInName)
            {
                actor.data.set("was_kage", 1);
            }

            if (level >= 10 && hasAnySenjutsu && ShinobiActors.IsShinobi(actor) && GiveOnce(actor, "07rank_sannin"))
            {
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_became_sannin", "worldlog_became_sannin", "ui/icons/rank_sannin", actor);
                }
            }

            // Ghost Of Uchiha Giver thingy thangy
            bool hasEternalEyes = actor.hasTrait("madara_eternal_mangekyo") || actor.hasTrait("eternal_mangekyo");
            if (level >= 7 && actor.hasTrait("uchiha_clan") && hasEternalEyes && GiveOnce(actor, "08rank_ghost_of_uchiha"))
            {
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_ghost_of_uchiha", "worldlog_ghost_of_uchiha", "ui/icons/ghostofuchiha", actor);
                }
            }


            // god Of Shinobi Giver thingy thangy
            bool hasAllFiveNatures = actor.hasTrait("fire_release") && actor.hasTrait("water_release") && actor.hasTrait("earth_release") && actor.hasTrait("lightning_release") && actor.hasTrait("wind_release");
            int wasKage = 0;
            actor.data.get("was_kage", out wasKage);
            bool qualifiesPastKage = actor.hasTrait("06rank_kage") || hasKageInName || wasKage > 0;
            if (qualifiesPastKage && level >= 5 && hasAllFiveNatures && GiveOnce(actor, "09rank_god_of_shinobi"))
            {
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_god_of_shinobi", "worldlog_god_of_shinobi", "ui/icons/godofshinobi", actor);
                }
            }

            if (actor.hasTrait("05_hagoromo_chakra") && !actor.hasStatus("status_six_paths_senjutsu"))
            {
                actor.addStatusEffect("status_six_paths_senjutsu", 30f);
            }

            if (actor.hasTrait("ten_tails_jinchuriki") && actor.hasTrait("rinnegan") && GiveOnce(actor, "rinnesharingan"))
            {
                if (!actor.hasStatus("status_six_paths_senjutsu"))
                {
                    actor.addStatusEffect("status_six_paths_senjutsu", -1f);
                }
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_awaken_rinnesharingan", "worldlog_awaken_rinnesharingan", "ui/icons/rinne_sharingan", actor);
                }
            }

            // If unit is an anbu or jonin, they are automatically in the military
            if (actor.hasTrait("05rank_anbu") || actor.hasTrait("04rank_jonin"))
            {
                actor.setProfession(UnitProfession.Warrior);
            }

            if (actor.isClone() && actor.isFavorite())
            {
                actor.data.favorite = false;
            }

            if (actor.isClone() && !actor.hasStatus("shadow_clone"))
            {
                actor.addStatusEffect("shadow_clone", 5f);
            }

            SageProgression.HandleSageProgression(actor, level);

            // Sharingan Progression
            if (actor.hasTrait("uchiha_clan"))
            {
                bool hasThreeTomoe = actor.hasStatus("status_sharingan_3t");

                // Mangekyo Awakening
                if (!actor.hasTrait("mangekyo_sharingan") && !actor.hasTrait("trait_blind") && !hasEternalEyes && hasThreeTomoe)
                {
                    if (HasTrauma(actor) && GiveOnce(actor, "mangekyo_sharingan"))
                    {
                        JutsuLibrary.ClearSharinganTomoeStatuses(actor);

                        actor.addStatusEffect("power_up");
                        ShinobiWorldLogs.AddWorldLog("log_mangekyo_awakened", "worldlog_mangekyo_awakened", "ui/icons/mangekyo", actor);
                    }
                }

                if (!actor.hasTrait("sharingan") && !HasBetterEye(actor, 1))
                {
                    if (kills >= 1 || (age >= 12 && UnityEngine.Random.value < 0.001f)) GiveOnce(actor, "sharingan");
                }

            }

            if (age >= 28 && level >= 5 && actor.isSapient() && actor.hasTrait("evil") &&
                AkatsukiMembers < MaxAkatsukiMembers && UnityEngine.Random.value < 0.0015f &&
                GiveOnce(actor, "trait_akatsuki"))
            {
                AkatsukiMembers++;
                if (ShinobiConfig.EnableWorldTips)
                {
                    ShinobiWorldLogs.AddWorldLog("log_joined_akatsuki", "worldlog_joined_akatsuki", "ui/icons/akatsuki", actor);
                }
            }

            #region Ten Tails Progression Stuff
            
            bool hasTen = actor.hasTrait("ten_tails_jinchuriki");
            bool hasSPS = actor.hasStatus("status_six_paths_senjutsu");

            if (hasSPS && !hasTen && !actor.hasTrait("05_hagoromo_chakra"))
            {
                actor.finishStatusEffect("status_six_paths_senjutsu");
            }
            
            #endregion
            
        }
        #endregion

        #region GiveOnce
        public static bool GiveOnce(Actor actor, string traitId)
        {
            string key = "given_" + traitId;
            actor.data.get(key, out bool alreadyGiven, false);
            if (alreadyGiven) return false;

            actor.data.set(key, true);
            if (actor.hasTrait(traitId)) return false;

            actor.addTrait(traitId);
            return actor.hasTrait(traitId);
        }
        #endregion

        #region Naming, Auto Favorite, and Hashirama Cells Helpers
        private static void ApplyNames(Actor actor)
        {
            if (!ShinobiConfig.EnableClanNames) return;

            if (actor.hasTrait("uchiha_clan")) ApplyClanName(actor, "Uchiha");
            else if (actor.hasTrait("hyuga_clan")) ApplyClanName(actor, "Hyuga");
            else if (actor.hasTrait("uzumaki_clan")) ApplyClanName(actor, "Uzumaki");
            else if (actor.hasTrait("senju_clan")) ApplyClanName(actor, "Senju");
            else if (actor.hasTrait("akimichi_clan")) ApplyClanName
            (actor, "Akimichi");
            else if (actor.hasTrait("lee_clan")) ApplyClanName(actor, "Lee");
            else if (actor.hasTrait("kaguya_clan")) ApplyClanName(actor, "Kaguya");
            else if (actor.hasTrait("otsutsuki_clan")) ApplyClanName(actor, "Otsutsuki");
            else if (actor.hasTrait("chinoike_clan")) ApplyClanName(actor, "Chinoike");
            else if (actor.hasTrait("aburame_clan")) ApplyClanName(actor, "Aburame");

            if (actor.hasTrait("06rank_kage"))
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

            bool hasAnySenjutsu = SageProgression.IsSage(actor);

            if (actor.hasTrait("07_indra_chakra") || actor.hasTrait("08_asura_chakra") ||
                actor.hasTrait("mangekyo_sharingan") || actor.hasTrait("madara_eternal_mangekyo") || actor.hasTrait("eternal_mangekyo") ||
                actor.hasTrait("rinnegan") || actor.hasTrait("nine_tails_jinchuriki") || actor.hasTrait("rinnesharingan") ||
                actor.hasTrait("05_hagoromo_chakra") || actor.hasTrait("ten_tails_jinchuriki") || hasAnySenjutsu)
            {
                actor.data.favorite = true;
                actor.data.set("auto_favorited_once", 1);
            }
        }

        private static void ApplyPassives(Actor actor)
        {
            if (actor.hasTrait("hashi_cells"))
            {
                if (actor.hasTrait("crippled")) actor.removeTrait("crippled");
                if (actor.hasTrait("eyepatch")) actor.removeTrait("eyepatch");
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
            if (currentTier < 2 && (a.hasStatus("status_sharingan_2t") || a.hasStatus("status_sharingan_3t") || a.hasTrait("mangekyo_sharingan") || a.hasTrait("trait_blind"))) return true;
            if (currentTier < 3 && (a.hasStatus("status_sharingan_3t") || a.hasTrait("mangekyo_sharingan") || a.hasTrait("trait_blind"))) return true;
            if (currentTier < 4 && (a.hasTrait("mangekyo_sharingan") || a.hasTrait("trait_blind"))) return true;
            return false;
        }
        #endregion

        #region removeWeapon
        public static void removeWeapon(Actor actor)
        {
            if (actor.hasTrait("taijutsu_master") && actor.hasWeapon())
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
            if (a != null && !a.hasTrait("nine_tails_jinchuriki")) RemoveLowerForms(a);
        }
        #endregion
    
    }
}