using System;
using NeoModLoader;
using NeoModLoader.api.attributes;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using ReflectionUtility;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using ai;
using ai.behaviours;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using Newtonsoft.Json;
using JinchurikiBox.Core;
using JinchurikiBox.Actions;


namespace JinchurikiBox.Content;

internal static class JinchurikiTraits
{
    [Hotfixable]
    public static void init()
    {

        ActorTrait sealedfate1 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "sealedfate",
            path_icon = "ui/icons/SealedFate",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = false,
            group_id = JinchurikiTraitGroup.ClanAddonTrait,
            needs_to_be_explored = false
        };
        sealedfate1.base_stats["multiplier_lifespan"] = -0.1f;
        sealedfate1.base_stats["intelligence"] = -10;
        AssetManager.traits.add(sealedfate1);

        ActorTrait reincarnate3 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "itachiReincarnate",
            path_icon = "ui/icons/ItachiWheel",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.SoulStuff,
            needs_to_be_explored = false
        };
        reincarnate3.base_stats["chakra"] = 100f;
        reincarnate3.base_stats["damage"] = 50f;
        AssetManager.traits.add(reincarnate3);
        //         PlayerConfig.unlockTrait("itachiReincarnate");

        ActorTrait reincarnate2 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "indrasReincarnate",
            path_icon = "ui/icons/Indrawheel",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.SoulStuff,
            needs_to_be_explored = false
        };
        reincarnate2.base_stats["chakra"] = 200f;
        reincarnate2.base_stats["damage"] = 75f;
        AssetManager.traits.add(reincarnate2);
        //         PlayerConfig.unlockTrait("indrasReincarnate");

        ActorTrait reincarnate1 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "madarasReincarnate",
            path_icon = "ui/icons/Madarawheel",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.SoulStuff,
            needs_to_be_explored = false
        };
        reincarnate1.base_stats["chakra"] = 100f;
        reincarnate1.base_stats["damage"] = 50f;
        AssetManager.traits.add(reincarnate1);
        //         PlayerConfig.unlockTrait("madarasReincarnate");

        ActorTrait HashiramaC = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "Hashc",
            path_icon = "ui/icons/HashiramaC",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.jinchurikiLockedTrait,
            action_special_effect = JinchurikiActions.HashCells,
            needs_to_be_explored = false
        };
        HashiramaC.base_stats["multiplier_health"] = +1.3f;
        HashiramaC.opposite_list = new List<string>() { "sharinganBlindness" };
        AssetManager.traits.add(HashiramaC);
        //         PlayerConfig.unlockTrait("Hashc");

        ActorTrait EightGH = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "EGH",
            path_icon = "ui/icons/EightGH",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = false,
            group_id = JinchurikiTraitGroup.jinchurikiLockedTrait,
            needs_to_be_explored = false
        };
        EightGH.base_stats["range"] = -80f;
        EightGH.base_stats["critical_chance"] = -75f;
        EightGH.base_stats["accuracy"] = -40f;
        EightGH.base_stats["speed"] = -35f;
        AssetManager.traits.add(EightGH);
        //         PlayerConfig.unlockTrait("EGH");

        ActorTrait Ccont = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "caloriecontrol",
            path_icon = "ui/icons/Calorie",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.ClanAddonTrait,
            needs_to_be_explored = false,
            action_special_effect = (BaseSimObject pTarget, WorldTile pTile = null) =>
                {
                    JutsuLibrary.Multisize.Cast(pTarget, pTarget.a.attack_target);
                    return true;
                }
        };
        Ccont.base_stats["chakra"] = 30f;
        AssetManager.traits.add(Ccont);
        //         PlayerConfig.unlockTrait("caloriecontrol");


        ActorTrait Blind = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "sharinganBlindness",
            path_icon = "ui/icons/Blind",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = false,
            group_id = JinchurikiTraitGroup.ClanAddonTrait,
            needs_to_be_explored = false
        };
        Blind.base_stats["range"] = -100f;
        Blind.base_stats["critical_chance"] = -1000f;
        Blind.base_stats["accuracy"] = -125f;
        AssetManager.traits.add(Blind);
        //         PlayerConfig.unlockTrait("sharinganBlindness");
        //         // PlayerConfig.unlockTrait("Sharingan-Blindness");
        // addTraitToLocalizedLibrary(Blind.id, "They Abused their power, and now are handi-capped.");

        ActorTrait FKage = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "formerKage",
            path_icon = "ui/icons/FormerK",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.RankTrait,
            action_special_effect = JinchurikiActions.ShinobiRankControl,
            needs_to_be_explored = false
        };
        FKage.base_stats["attack_speed"] = 16f;
        FKage.base_stats["critical_chance"] = 0.11f;
        FKage.base_stats["accuracy"] = 0.25f;
        FKage.base_stats["health"] = 384f;
        FKage.base_stats["speed"] = 45f;
        FKage.base_stats["damage"] = 128f;
        FKage.base_stats["chakra"] = 256f;
        FKage.base_stats["armor"] = 32;
        FKage.base_stats["chakra_regen"] = 15f;
        FKage.base_stats["diplomacy"] = 40;
        FKage.base_stats["loyalty_traits"] = 56f;
        FKage.base_stats["opinion"] = 79f;
        FKage.base_stats["intelligence"] = 28;
        AssetManager.traits.add(FKage);
        //         PlayerConfig.unlockTrait("formerKage");
        // addTraitToLocalizedLibrary(FKage.id, "They were the leader of a society, now retired.");

        ActorTrait Legend = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "SLegend",
            path_icon = "ui/icons/legend",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.RankTrait,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        Legend.base_stats["chakra"] = 200f;
        Legend.base_stats["health"] = 500f;
        Legend.base_stats["armor"] = 15;
        Legend.base_stats["speed"] = 20f;
        Legend.base_stats["critical_chance"] = 0.20f;
        Legend.base_stats["chakra_regen"] = 9f;
        AssetManager.traits.add(Legend);

        ActorTrait Prod = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "prodigy",
            path_icon = "ui/icons/Prodigy",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.RankTrait,
            needs_to_be_explored = false
        };
        Prod.base_stats["chakra"] = 50f;
        Prod.base_stats["damage"] = 20f;
        Prod.base_stats["attack_speed"] = 5f;
        Prod.base_stats["intelligence"] = 50;
        Prod.base_stats["warfare"] = 100f;
        Prod.base_stats["chakra_regen"] = 10f;
        Prod.base_stats["skill_combat"] = 0.60f;
        AssetManager.traits.add(Prod);

        ActorTrait Kage = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "kage",
            path_icon = "ui/icons/Kage",
            rate_inherit = 0,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.RankTrait,
            action_special_effect = JinchurikiActions.ShinobiRankControl,
            needs_to_be_explored = false
        };
        Kage.base_stats["attack_speed"] = 20f;
        Kage.base_stats["critical_chance"] = 0.11f;
        Kage.base_stats["accuracy"] = 0.25f;
        Kage.base_stats["health"] = 480f;
        Kage.base_stats["speed"] = 56f;
        Kage.base_stats["damage"] = 160f;
        Kage.base_stats["chakra"] = 320f;
        Kage.base_stats["armor"] = 40;
        Kage.base_stats["chakra_regen"] = 15f;
        Kage.base_stats["diplomacy"] = 50;
        Kage.base_stats["loyalty_traits"] = 70f;
        Kage.base_stats["opinion"] = 99f;
        Kage.base_stats["intelligence"] = 35;
        AssetManager.traits.add(Kage);
        //         PlayerConfig.unlockTrait("kage");
        // addTraitToLocalizedLibrary(Kage.id, "They are the leader of a society.");


        ActorTrait Gyate1 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "eightGatesTechnique",
            path_icon = "ui/icons/brain",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Gyate1.base_stats["damage"] = 30f;
        Gyate1.base_stats["armor"] = 2;
        Gyate1.base_stats["attack_speed"] = 3f;
        Gyate1.base_stats["health"] = 30;
        Gyate1.base_stats["speed"] = 10f;
        Gyate1.can_be_given = true;
        Gyate1.group_id = JinchurikiTraitGroup.TransformationTrait;
        Gyate1.action_get_hit = JinchurikiActions.Eight;
        AssetManager.traits.add(Gyate1);
        //         PlayerConfig.unlockTrait("eightGatesTechnique");
        // addTraitToLocalizedLibrary(Gyate1.id, "A techniques that lets a user trick their brain into using 100% of their chakra, instead of the normal 20%.");

        ActorTrait Jonin = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "jonin",
            path_icon = "ui/icons/Jonin",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Jonin.base_stats["attack_speed"] = 5f;
        Jonin.base_stats["critical_chance"] = 0.06f;
        Jonin.base_stats["accuracy"] = 0.20f;
        Jonin.base_stats["health"] = 120f;
        Jonin.base_stats["speed"] = 14f;
        Jonin.base_stats["damage"] = 40f;
        Jonin.base_stats["chakra"] = 80f;
        Jonin.base_stats["armor"] = 10;
        Jonin.base_stats["chakra_regen"] = 7f;
        Jonin.can_be_given = true;
        Jonin.group_id = JinchurikiTraitGroup.RankTrait;
        Jonin.action_special_effect = JinchurikiActions.ShinobiRankControl;
        AssetManager.traits.add(Jonin);

        ActorTrait Chunin = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chunin",
            path_icon = "ui/icons/Chunin",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Chunin.base_stats["damage"] = 20f;
        Chunin.base_stats["chakra"] = 40f;
        Chunin.base_stats["chakra_regen"] = 5f;
        Chunin.base_stats["speed"] = 10f;
        Chunin.base_stats["critical_chance"] = 0.05f;
        Chunin.base_stats["accuracy"] = 0.10f;
        Chunin.base_stats["attack_speed"] = 1f;
        Chunin.base_stats["intelligence"] = 5;
        Chunin.base_stats["armor"] = 5;
        Chunin.base_stats["health"] = 60f;
        Chunin.can_be_given = true;
        Chunin.group_id = JinchurikiTraitGroup.RankTrait;
        Chunin.action_special_effect = JinchurikiActions.ShinobiRankControl;
        AssetManager.traits.add(Chunin);

        ActorTrait Genin = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "genin",
            path_icon = "ui/icons/Genin",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Genin.base_stats["chakra"] = 25f;
        Genin.base_stats["damage"] = 10f;
        Genin.base_stats["health"] = 20f;
        Genin.base_stats["accuracy"] = 0.05f;
        Genin.base_stats["critical_chance"] = 0.02f;
        Genin.base_stats["chakra_regen"] = 3f;
        Genin.can_be_given = true;
        Genin.group_id = JinchurikiTraitGroup.RankTrait;
        Genin.action_special_effect = JinchurikiActions.ShinobiRankControl;
        AssetManager.traits.add(Genin);

        ActorTrait AcademyStudent = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "academyStudent",
            path_icon = "ui/icons/student",
            rate_inherit = 0,
            rate_birth = 50,
            needs_to_be_explored = false
        };
        AcademyStudent.base_stats["chakra"] = 20f;
        AcademyStudent.base_stats["damage"] = 5f;
        AcademyStudent.base_stats["chakra_regen"] = 2f;
        AcademyStudent.can_be_given = true;
        AcademyStudent.group_id = JinchurikiTraitGroup.RankTrait;
        AcademyStudent.action_special_effect = JinchurikiActions.ShinobiRankControl;
        AssetManager.traits.add(AcademyStudent);

        ActorTrait Rinnegan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "rinnegan",
            path_icon = "ui/icons/Rinnegan",
            rate_inherit = 20,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        Rinnegan.base_stats["chakra"] = 300f;
        Rinnegan.base_stats["chakra_regen"] = 8f;
        Rinnegan.can_be_given = true;
        Rinnegan.group_id = JinchurikiTraitGroup.TransformationTrait;
        Rinnegan.action_get_hit = JinchurikiActions.RinneganActivation;
        AssetManager.traits.add(Rinnegan);
        //         PlayerConfig.unlockTrait("rinnegan");
        // addTraitToLocalizedLibrary(Rinnegan.id, "They have access to the strongest Dojutsu.");

        ActorTrait LargeBeast = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "largeBeast",
            path_icon = "ui/icons/LargeBeast",
            rate_birth = 0,
            needs_to_be_explored = false
        };
        LargeBeast.base_stats["scale"] = 0.5f;
        LargeBeast.base_stats["health"] = 1000f;
        LargeBeast.base_stats["damage"] = 45f;
        LargeBeast.can_be_given = true;
        LargeBeast.group_id = JinchurikiTraitGroup.TransformationTrait;
        AssetManager.traits.add(LargeBeast);
        //         PlayerConfig.unlockTrait("largeBeast");
        // addTraitToLocalizedLibrary(LargeBeast.id, "This... POWER!");

        ActorTrait KsealW = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "incompletedKarmaSeal",
            path_icon = "ui/icons/KarmaW",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        KsealW.base_stats["chakra"] = 150f;
        KsealW.base_stats["chakra_regen"] = 4f;
        KsealW.can_be_given = true;
        KsealW.group_id = JinchurikiTraitGroup.TransformationTrait;
        KsealW.action_get_hit = JinchurikiActions.sealK2;
        AssetManager.traits.add(KsealW);
        //         PlayerConfig.unlockTrait("incompletedKarmaSeal");
        // addTraitToLocalizedLibrary(KsealW.id, "The transfering progress of the karma seal has failed, the spirit of the otsutsuki couldn't implant, thus they have a weak karma seal.");

        ActorTrait Kseal = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "completedKarmaSeal",
            path_icon = "ui/icons/KarmaB",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Kseal.base_stats["chakra"] = 150f;
        Kseal.base_stats["chakra_regen"] = 6f;
        Kseal.can_be_given = true;
        Kseal.group_id = JinchurikiTraitGroup.TransformationTrait;
        Kseal.action_get_hit = JinchurikiActions.sealK;
        AssetManager.traits.add(Kseal);
        //         PlayerConfig.unlockTrait("completedKarmaSeal");
        // addTraitToLocalizedLibrary(Kseal.id, "The transfering progress of the karma seal has been completed, they will slowly turn into their Otsutsuki..");

        ActorTrait Aclan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "akimichiClan",
            path_icon = "ui/icons/AkiClan",
            rate_inherit = 95,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Aclan.base_stats["damage"] = 5f;
        Aclan.base_stats["speed"] = -4f;
        Aclan.base_stats["scale"] = 0.015f;
        Aclan.base_stats["chakra"] = 15f;
        Aclan.base_stats["opinion"] = -2f;
        Aclan.can_be_given = true;
        Aclan.group_id = JinchurikiTraitGroup.ClanTrait;
        AssetManager.traits.add(Aclan);

        ActorTrait HClan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "hyugaClan",
            path_icon = "ui/icons/hyu",
            rate_inherit = 95,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        HClan.base_stats["intelligence"] = 15;
        HClan.base_stats["chakra"] = 15f;
        HClan.base_stats["diplomacy"] = 2f;
        HClan.base_stats["loyalty_traits"] = 10f;
        HClan.can_be_given = true;
        HClan.group_id = JinchurikiTraitGroup.ClanTrait;
        HClan.action_special_effect = JinchurikiActions.Checker2;
        HClan.action_special_effect = JinchurikiActions.ByakuganMethod;
        HClan.action_death = JinchurikiActions.Transfer;
        AssetManager.traits.add(HClan);

        /* namikazeClan removed per balancing
        ActorTrait NClan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "namikazeClan",
            path_icon = "ui/icons/Nclan",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        NClan.can_be_given = false;
        NClan.group_id = JinchurikiTraitGroup.ClanTrait;
        AssetManager.traits.add(NClan);
        */

        ActorTrait UZClan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "uzumakiClan",
            path_icon = "ui/icons/UZclan",
            rate_inherit = 95,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R2_Epic
        };
        UZClan.base_stats["armor"] = 5;
        UZClan.base_stats["intelligence"] = -4;
        UZClan.base_stats["chakra"] = 80f;
        UZClan.can_be_given = true;
        UZClan.group_id = JinchurikiTraitGroup.ClanTrait;
        AssetManager.traits.add(UZClan);

        ActorTrait UClan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "uchihaClan",
            path_icon = "ui/icons/uch",
            rate_inherit = 95,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        UClan.base_stats["intelligence"] = 10;
        UClan.base_stats["diplomacy"] = -10f;
        UClan.base_stats["loyalty_traits"] = -35f;
        UClan.base_stats["opinion"] = -20f;
        UClan.base_stats["chakra"] = 10f;
        UClan.base_stats["accuracy"] = 0.10f;
        UClan.base_stats["critical_chance"] = 0.02f;
        UClan.base_stats["skill_combat"] = 0.15f;
        UClan.can_be_given = true;
        UClan.group_id = JinchurikiTraitGroup.ClanTrait;
        UClan.action_special_effect = JinchurikiActions.SharinganMethod;
        UClan.action_death = JinchurikiActions.Transfer;
        AssetManager.traits.add(UClan);

        ActorTrait CIMPLANT = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "cursedSealOfHeaven",
            path_icon = "ui/icons/Snake",
            rate_inherit = 20,
            rate_birth = 0,
            can_be_given = true,
            group_id = JinchurikiTraitGroup.TransformationTrait,
            action_get_hit = JinchurikiActions.CurseMark,
            needs_to_be_explored = false
        };
        CIMPLANT.base_stats["loyalty_traits"] = -50f;
        CIMPLANT.base_stats["damage"] = 10f;
        CIMPLANT.base_stats["chakra"] = 10f;
        AssetManager.traits.add(CIMPLANT);
        //         PlayerConfig.unlockTrait("cursedSealOfHeaven");
        // addTraitToLocalizedLibrary(CIMPLANT.id, " Something bit them, they couldnt make out what it was, seemed to be a weird looking snake.");

        ActorTrait Partner9T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "nineTailedPartner",
            path_icon = "ui/icons/Partner",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Partner9T.base_stats["chakra"] = 40;
        Partner9T.can_be_given = true;
        Partner9T.action_get_hit += new GetHitAction(PartnerCTB);
        Partner9T.action_death += new WorldAction(BaryonActivation);
        Partner9T.group_id = JinchurikiTraitGroup.jinchurikiLockedTrait;
        AssetManager.traits.add(Partner9T);
        //         PlayerConfig.unlockTrait("nineTailedPartner");
        // addTraitToLocalizedLibrary(Partner9T.id, "After a long time, kurama has found his match with his human. The gates have opened and the power is flowing.");

        ActorTrait IndraChakra = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "indraChakra",
            path_icon = "ui/icons/moon",
            rate_inherit = 55,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        IndraChakra.base_stats["damage"] = 15;
        IndraChakra.base_stats["armor"] = 5;
        IndraChakra.base_stats["chakra"] = 250;
        IndraChakra.can_be_given = true;
        IndraChakra.group_id = JinchurikiTraitGroup.ChakraTrait;
        AssetManager.traits.add(IndraChakra);

        ActorTrait SenjuS = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "senjuSage",
            path_icon = "ui/icons/SenjuSageScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        SenjuS.base_stats["damage"] = 25;
        SenjuS.base_stats["armor"] = 0;
        SenjuS.base_stats["attack_speed"] = 0;
        SenjuS.base_stats["health"] = 30;
        SenjuS.base_stats["intelligence"] = 20;
        SenjuS.base_stats["speed"] = 0;
        SenjuS.base_stats["chakra"] = 75;
        SenjuS.base_stats["sage_chakra"] = 115;
        SenjuS.base_stats["sage_chakra_regen"] = 6f;
        SenjuS.base_stats["multiplier_lifespan"] = 0.2f;
        SenjuS.can_be_given = true;
        SenjuS.action_get_hit = JinchurikiActions.SenjuActivation;
        SenjuS.group_id = JinchurikiTraitGroup.JutsuTraits2;
        AssetManager.traits.add(SenjuS);
        //         PlayerConfig.unlockTrait("senjuSage");
        // addTraitToLocalizedLibrary(SenjuS.id, "The Senju Sage Jutsu, this legendary technique being the strongest version of the sage series.");   

        ActorTrait SenjuC = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "senjuClan",
            path_icon = "ui/icons/SenjuClan",
            rate_inherit = 85,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        SenjuC.base_stats["health"] = 50;
        SenjuC.base_stats["chakra"] = 25;
        SenjuC.base_stats["skill_combat"] = 0.10f;
        SenjuC.can_be_given = true;
        SenjuC.group_id = JinchurikiTraitGroup.ClanTrait;
        AssetManager.traits.add(SenjuC);

        ActorTrait Otsuk = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "otsutsukiClan",
            path_icon = "ui/icons/OClan",
            rate_inherit = 10,
            rate_birth = 0,
            rarity = Rarity.R3_Legendary,
            needs_to_be_explored = false
        };
        Otsuk.base_stats["multiplier_lifespan"] = 0.2f;
        Otsuk.base_stats["health"] = 100;
        Otsuk.base_stats["chakra"] = 150;
        Otsuk.base_stats["armor"] = 10;
        Otsuk.base_stats["intelligence"] = 30;
        Otsuk.base_stats["speed"] = 2;
        Otsuk.base_stats["opinion"] = 40f;
        Otsuk.base_stats["loyalty_traits"] = -30f;
        Otsuk.base_stats["critical_chance"] = 0.10f;
        Otsuk.base_stats["skill_combat"] = 0.12f;
        Otsuk.can_be_given = true;
        Otsuk.group_id = JinchurikiTraitGroup.ClanTrait;
        Otsuk.action_death = JinchurikiActions.Transfer;
        AssetManager.traits.add(Otsuk);

        ActorTrait OtsutsukiH4 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "otsutsuki80",
            path_icon = "ui/icons/bigDN",
            rate_inherit = 80,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        OtsutsukiH4.base_stats["damage"] = 20;
        OtsutsukiH4.base_stats["armor"] = 15;
        OtsutsukiH4.base_stats["attack_speed"] = 30;
        OtsutsukiH4.base_stats["health"] = 80;
        OtsutsukiH4.base_stats["intelligence"] = 10;
        OtsutsukiH4.base_stats["speed"] = 10;
        OtsutsukiH4.base_stats["chakra"] = 50;
        OtsutsukiH4.can_be_given = true;
        OtsutsukiH4.group_id = JinchurikiTraitGroup.ClanAddonTrait;
        OtsutsukiH4.opposite_list = new List<string>() { "Ōtsutsuki Clan" };
        AssetManager.traits.add(OtsutsukiH4);
        //         PlayerConfig.unlockTrait("otsutsuki80");
        // addTraitToLocalizedLibrary(OtsutsukiH4.id, "Their Otsutsuki heritage is strong, but they are more than likely a hybrid.");

        ActorTrait OtsutsukiH3 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "otsutsuki60",
            path_icon = "ui/icons/bigDN",
            rate_inherit = 60,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        OtsutsukiH3.base_stats["damage"] = 15;
        OtsutsukiH3.base_stats["armor"] = 10;
        OtsutsukiH3.base_stats["attack_speed"] = 20;
        OtsutsukiH3.base_stats["health"] = 60;
        OtsutsukiH3.base_stats["intelligence"] = 5;
        OtsutsukiH3.base_stats["speed"] = 10;
        OtsutsukiH3.base_stats["chakra"] = 40;
        OtsutsukiH3.can_be_given = true;
        OtsutsukiH3.group_id = JinchurikiTraitGroup.ClanAddonTrait;
        OtsutsukiH3.opposite_list = new List<string>() { "otsutsuki80" };
        AssetManager.traits.add(OtsutsukiH3);
        //         PlayerConfig.unlockTrait("otsutsuki60");
        // addTraitToLocalizedLibrary(OtsutsukiH3.id, "Their Otsutsuki heritage is strong, but they are more than likely a hybrid.");

        ActorTrait OtsutsukiH2 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "otsutsuki40",
            path_icon = "ui/icons/smallDN",
            rate_inherit = 40,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        OtsutsukiH2.base_stats["damage"] = 10;
        OtsutsukiH2.base_stats["armor"] = 5;
        OtsutsukiH2.base_stats["attack_speed"] = 10;
        OtsutsukiH2.base_stats["health"] = 40;
        OtsutsukiH2.base_stats["intelligence"] = 0;
        OtsutsukiH2.base_stats["speed"] = 5;
        OtsutsukiH2.base_stats["chakra"] = 30;
        OtsutsukiH2.can_be_given = true;
        OtsutsukiH2.group_id = JinchurikiTraitGroup.ClanAddonTrait;
        OtsutsukiH2.opposite_list = new List<string>() { "otsutsuki80", "otsutsuki60" };
        AssetManager.traits.add(OtsutsukiH2);
        //         PlayerConfig.unlockTrait("otsutsuki40");
        // addTraitToLocalizedLibrary(OtsutsukiH2.id, "They have an Otsutsuki heritage but its weak and faint, they are more than likely a hybrid.");

        ActorTrait OtsutsukiH = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "otsutsuki20",
            path_icon = "ui/icons/smallDN",
            rate_inherit = 20,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        OtsutsukiH.base_stats["damage"] = 5;
        OtsutsukiH.base_stats["armor"] = 1;
        OtsutsukiH.base_stats["attack_speed"] = 0;
        OtsutsukiH.base_stats["health"] = 20;
        OtsutsukiH.base_stats["intelligence"] = 0;
        OtsutsukiH.base_stats["chakra"] = 20;
        OtsutsukiH.can_be_given = true;
        OtsutsukiH.group_id = JinchurikiTraitGroup.ClanAddonTrait;
        OtsutsukiH.opposite_list = new List<string>() { "otsutsuki80", "otsutsuki60", "otsutsuki40" };
        AssetManager.traits.add(OtsutsukiH);
        //         PlayerConfig.unlockTrait("otsutsuki20");
        // addTraitToLocalizedLibrary(OtsutsukiH.id, "They have an Otsutsuki heritage but its weak and faint, they are more than likely a hybrid..");


        ActorTrait AsuraChakra = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "asuraChakra",
            path_icon = "ui/icons/sun",
            rate_inherit = 55,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        AsuraChakra.base_stats["damage"] = 15;
        AsuraChakra.base_stats["armor"] = 5;
        AsuraChakra.base_stats["chakra"] = 250;
        AsuraChakra.can_be_given = true;
        AsuraChakra.group_id = JinchurikiTraitGroup.ChakraTrait;
        AssetManager.traits.add(AsuraChakra);
        //         PlayerConfig.unlockTrait("asuraChakra");

        ActorTrait HagoromoChakra = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "hagoromoChakra",
            path_icon = "ui/icons/HagoromoChakra",
            rate_inherit = 10,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        HagoromoChakra.base_stats["chakra"] = 1000;
        HagoromoChakra.can_be_given = true;
        HagoromoChakra.group_id = JinchurikiTraitGroup.ChakraTrait;
        AssetManager.traits.add(HagoromoChakra);

        ActorTrait HamuraChakra = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "hamuraChakra",
            path_icon = "ui/icons/HamuraChakra",
            rate_inherit = 20,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        HamuraChakra.base_stats["chakra"] = 850;
        HamuraChakra.can_be_given = true;
        HamuraChakra.group_id = JinchurikiTraitGroup.ChakraTrait;
        HamuraChakra.action_get_hit = JinchurikiActions.jogan;
        AssetManager.traits.add(HamuraChakra);

        ActorTrait SNSage = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "snakeSage",
            path_icon = "ui/icons/SnakeSageScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        SNSage.base_stats["multiplier_lifespan"] = 0.5f;
        SNSage.base_stats["damage"] = 15;
        SNSage.base_stats["armor"] = 0;
        SNSage.base_stats["attack_speed"] = 0;
        SNSage.base_stats["speed"] = 5;
        SNSage.base_stats["health"] = 35;
        SNSage.base_stats["chakra"] = 35;
        SNSage.base_stats["sage_chakra"] = 45;
        SNSage.base_stats["sage_chakra_regen"] = 6f;
        SNSage.can_be_given = true;
        SNSage.group_id = JinchurikiTraitGroup.JutsuTraits2;
        SNSage.action_get_hit = JinchurikiActions.SnakeActivation;
        AssetManager.traits.add(SNSage);
        //         PlayerConfig.unlockTrait("snakeSage");
        // addTraitToLocalizedLibrary(SNSage.id, "This scrolls expands their knowledge on the Snake Sage Technique. Letting them use it.");

        ActorTrait SSage = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "100heals",
            path_icon = "ui/icons/SlugSageScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        SSage.base_stats["multiplier_lifespan"] = -0.3f;
        SSage.base_stats["damage"] = 0;
        SSage.base_stats["armor"] = 0;
        SSage.base_stats["attack_speed"] = 0;
        SSage.base_stats["speed"] = 0;
        SSage.base_stats["health"] = 0;
        SSage.base_stats["chakra"] = 25;
        SSage.can_be_given = true;
        SSage.group_id = JinchurikiTraitGroup.TransformationTrait;
        SSage.action_get_hit = JinchurikiActions.SlugActivation;
        AssetManager.traits.add(SSage);
        //         PlayerConfig.unlockTrait("slugSage");
        // addTraitToLocalizedLibrary(SSage.id, "They are a Slug Sage.");

        ActorTrait TSage = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "toadSage",
            path_icon = "ui/icons/FrogSageScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        TSage.base_stats["damage"] = 10;
        TSage.base_stats["armor"] = 0;
        TSage.base_stats["attack_speed"] = 0;
        TSage.base_stats["speed"] = 0;
        TSage.base_stats["health"] = 20;
        TSage.base_stats["chakra"] = 50;
        TSage.base_stats["sage_chakra"] = 65;
        TSage.base_stats["sage_chakra_regen"] = 6f;
        TSage.can_be_given = true;
        TSage.group_id = JinchurikiTraitGroup.JutsuTraits2;
        TSage.action_special_effect = JinchurikiActions.ToadSageMode;
        AssetManager.traits.add(TSage);
        //         PlayerConfig.unlockTrait("toadSage");
        // addTraitToLocalizedLibrary(TSage.id, "They are a Toad Sage.");

        ActorTrait immeasurableReserves = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "immeasurableReserves",
            path_icon = "ui/icons/ChakraNum",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        immeasurableReserves.base_stats["multiplier_lifespan"] = 0.25f;
        immeasurableReserves.base_stats["chakra"] = 675;
        immeasurableReserves.base_stats["damage"] = 35;
        immeasurableReserves.base_stats["armor"] = 2;
        immeasurableReserves.base_stats["health"] = 20;
        immeasurableReserves.base_stats["chakra_regen"] = 10f;
        immeasurableReserves.can_be_given = true;
        immeasurableReserves.group_id = JinchurikiTraitGroup.ChakraTrait2;
        immeasurableReserves.action_special_effect = JinchurikiActions.ChakraRegeneration;
        immeasurableReserves.action_attack_target = JinchurikiActions.ImmeasurableAction;
        immeasurableReserves.opposite_list = new List<string>() { "chakraC5", "chakraC4", "chakraC3", "chakraC2", "chakraC1", "chakraC" };
        AssetManager.traits.add(immeasurableReserves);

        ActorTrait ChakraC5 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chakraC5",
            path_icon = "ui/icons/Chakra3",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        ChakraC5.base_stats["multiplier_lifespan"] = 0.15f;
        ChakraC5.base_stats["chakra"] = 350;
        ChakraC5.base_stats["damage"] = 20;
        ChakraC5.base_stats["armor"] = 1;
        ChakraC5.base_stats["health"] = 10;
        ChakraC5.base_stats["chakra_regen"] = 9f;
        ChakraC5.can_be_given = true;
        ChakraC5.group_id = JinchurikiTraitGroup.ChakraTrait2;
        ChakraC5.action_special_effect = JinchurikiActions.ChakraRegeneration;
        ChakraC5.opposite_list = new List<string>() { "immeasurableReserves", "chakraC4", "chakraC3", "chakraC2", "chakraC1", "chakraC" };
        AssetManager.traits.add(ChakraC5);

        ActorTrait ChakraC4 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chakraC4",
            path_icon = "ui/icons/Chakra2",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ChakraC4.base_stats["multiplier_lifespan"] = 0.10f;
        ChakraC4.base_stats["chakra"] = 200;
        ChakraC4.base_stats["chakra_regen"] = 7f;
        ChakraC4.can_be_given = true;
        ChakraC4.group_id = JinchurikiTraitGroup.ChakraTrait2;
        ChakraC4.action_special_effect = JinchurikiActions.ChakraRegeneration;
        ChakraC4.opposite_list = new List<string>() { "immeasurableReserves", "chakraC5", "chakraC3", "chakraC2", "chakraC1", "chakraC" };
        AssetManager.traits.add(ChakraC4);

        ActorTrait ChakraC3 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chakraC3",
            path_icon = "ui/icons/Chakra2",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ChakraC3.base_stats["chakra"] = 150;
        ChakraC3.base_stats["chakra_regen"] = 5f;
        ChakraC3.can_be_given = true;
        ChakraC3.action_special_effect = JinchurikiActions.ChakraRegeneration;
        ChakraC3.group_id = JinchurikiTraitGroup.ChakraTrait2;
        ChakraC3.opposite_list = new List<string>() { "immeasurableReserves", "chakraC5", "chakraC4", "chakraC2", "chakraC1", "chakraC" };
        AssetManager.traits.add(ChakraC3);

        ActorTrait ChakraC2 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chakraC2",
            path_icon = "ui/icons/Chakra",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ChakraC2.base_stats["chakra"] = 120;
        ChakraC2.base_stats["chakra_regen"] = 3f;
        ChakraC2.can_be_given = true;
        ChakraC2.action_special_effect = JinchurikiActions.ChakraRegeneration;
        ChakraC2.group_id = JinchurikiTraitGroup.ChakraTrait2;
        ChakraC2.opposite_list = new List<string>() { "immeasurableReserves", "chakraC5", "chakraC4", "chakraC3", "chakraC1", "chakraC" };
        AssetManager.traits.add(ChakraC2);

        ActorTrait ChakraC1 = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chakraC1",
            path_icon = "ui/icons/Chakra",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ChakraC1.base_stats["chakra"] = 100;
        ChakraC1.base_stats["chakra_regen"] = 2f;
        ChakraC1.can_be_given = true;
        ChakraC1.action_special_effect = JinchurikiActions.ChakraRegeneration;
        ChakraC1.group_id = JinchurikiTraitGroup.ChakraTrait2;
        ChakraC1.opposite_list = new List<string>() { "immeasurableReserves", "chakraC5", "chakraC4", "chakraC3", "chakraC2", "chakraC" };
        AssetManager.traits.add(ChakraC1);

        ActorTrait ChakraC = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chakraC",
            path_icon = "ui/icons/Chakra",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ChakraC.base_stats["chakra"] = 10;
        ChakraC.base_stats["chakra_regen"] = 1f;
        ChakraC.can_be_given = true;
        ChakraC.action_special_effect = JinchurikiActions.ChakraRegeneration;
        ChakraC.group_id = JinchurikiTraitGroup.ChakraTrait2;
        ChakraC.opposite_list = new List<string>() { "immeasurableReserves", "chakraC5", "chakraC4", "chakraC3", "chakraC2", "chakraC1" };
        AssetManager.traits.add(ChakraC);

        // ActorTrait Seal3 = new ActorTrait();
        // Seal3.id = "Controlled Jinchuriki";
        // Seal3.path_icon = "ui/icons/Seal2";
        // Seal3.rate_inherit = 0;
        // Seal3.rate_birth = 0;
        // Seal3.base_stats["attack_speed"] = 20f;
        // Seal3.base_stats["health"] = 100;
        // Seal3.base_stats["intelligence"] = 9;
        // Seal3.base_stats["speed"] = 10f;
        // Seal3.opposite_list = new List<string>() {"Rampaged Jinchuriki"};         
        // Seal3.can_be_given = true;
        // Seal3.action_special_effect = (WorldAction)Delegate.Combine(Seal3.action_special_effect, new WorldAction(NineTailedRegeneration));
        // Seal3.action_get_hit += JinchurikiActions.NineTailedBeast;
        // Seal3.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        // AssetManager.traits.add(Seal3);
        //         // PlayerConfig.unlockTrait("Controlled Jinchuriki");
        // // addTraitToLocalizedLibrary(Seal3.id, "The seal, has stayed in matter of control, meaning you've became friends with your monster.");


        // ActorTrait Seal2 = new ActorTrait();
        // Seal2.id = "Rampaged Jinchuriki";
        // Seal2.path_icon = "ui/icons/Seal3";
        // Seal2.rate_inherit = 0;
        // Seal2.rate_birth = 0;
        // Seal2.base_stats["attack_speed"] = 20f;
        // Seal2.base_stats["health"] = 100;
        // Seal2.base_stats["intelligence"] = 5;
        // Seal2.base_stats["speed"] = 10f;
        // Seal2.opposite_list = new List<string>() {"Controlled Jinchuriki"};          
        // Seal2.can_be_given = true;
        // Seal2.action_special_effect += new WorldAction(NineTailedRegeneration);
        // Seal2.action_get_hit = JinchurikiActions.RTB;
        // Seal2.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        // AssetManager.traits.add(Seal2);
        //         // PlayerConfig.unlockTrait("Rampaged Jinchuriki");
        // // addTraitToLocalizedLibrary(Seal2.id, "The seal, has broken.");

        ActorTrait Seal1T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "oneTailedJinchuriki",
            path_icon = "ui/icons/1J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal1T.base_stats["tailed_beast_chakra"] = 1000f;
        // Seal1Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal1T.can_be_given = true;
        Seal1T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal1T.action_special_effect += JinchurikiActions.OneTailedBeast;
        Seal1T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal1T);
        //         PlayerConfig.unlockTrait("oneTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal1Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal2T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "twoTailedJinchuriki",
            path_icon = "ui/icons/2J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal2T.base_stats["tailed_beast_chakra"] = 1250f;
        // Seal2Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal2T.can_be_given = true;
        Seal2T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal2T.action_special_effect += JinchurikiActions.TwoTailedBeast;
        Seal2T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal2T);
        //         PlayerConfig.unlockTrait("twoTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal9Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal3T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "threeTailedJinchuriki",
            path_icon = "ui/icons/Seal",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal3T.base_stats["tailed_beast_chakra"] = 1500f;
        // Sea.l3Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal3T.can_be_given = true;
        Seal3T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal3T.action_special_effect += JinchurikiActions.ThreeTailedBeast;
        Seal3T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal3T);
        //         PlayerConfig.unlockTrait("threeTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal3Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal4T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "fourTailedJinchuriki",
            path_icon = "ui/icons/4J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal4T.base_stats["tailed_beast_chakra"] = 1750f;
        // Seal4Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal4T.can_be_given = true;
        Seal4T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal4T.action_special_effect += JinchurikiActions.FourTailedBeast;
        Seal4T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal4T);
        //         PlayerConfig.unlockTrait("fourTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal4Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal5T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "fiveTailedJinchuriki",
            path_icon = "ui/icons/Seal",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal5T.base_stats["tailed_beast_chakra"] = 2000f;
        // Seal5Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal5T.can_be_given = true;
        Seal5T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal5T.action_special_effect += JinchurikiActions.FiveTailedBeast;
        Seal5T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal5T);
        //         PlayerConfig.unlockTrait("fiveTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal5Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal6T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "sixTailedJinchuriki",
            path_icon = "ui/icons/6J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal6T.base_stats["tailed_beast_chakra"] = 2250f;
        // Seal6Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal6T.can_be_given = true;
        Seal6T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal6T.action_special_effect += JinchurikiActions.SixTailedBeast;
        Seal6T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal6T);
        //         PlayerConfig.unlockTrait("sixTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal6Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal7T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "sevenTailedJinchuriki",
            path_icon = "ui/icons/7J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal7T.base_stats["tailed_beast_chakra"] = 2500f;
        // Seal7Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal7T.can_be_given = true;
        Seal7T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal7T.action_special_effect += JinchurikiActions.SevenTailedBeast;
        Seal7T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal7T);
        //         PlayerConfig.unlockTrait("sevenTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal7Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal8T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "eightTailedJinchuriki",
            path_icon = "ui/icons/8J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal8T.base_stats["tailed_beast_chakra"] = 2750f;
        // Seal8Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal8T.can_be_given = true;
        Seal8T.action_special_effect += JinchurikiActions.EightTailedBeast;
        Seal8T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal8T);
        //         PlayerConfig.unlockTrait("eightTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal8Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait Seal9T = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "nineTailedJinchuriki",
            path_icon = "ui/icons/9J",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Seal9T.base_stats["tailed_beast_chakra"] = 3000f;
        // Seal9Topposite_list = new List<string>() { "Controlled Jinchuriki", "Rampaged Jinchuriki" };   
        Seal9T.can_be_given = true;
        Seal9T.action_special_effect += new WorldAction(NineTailedRegeneration);
        Seal9T.action_special_effect += JinchurikiActions.NineTailedBeast;
        Seal9T.group_id = JinchurikiTraitGroup.jinchurikiTrait;
        AssetManager.traits.add(Seal9T);
        //         PlayerConfig.unlockTrait("nineTailedJinchuriki");
        // addTraitToLocalizedLibrary(Seal9Tid, "A monster, sealed inside of you, dont let it rampage.");

        ActorTrait WoodR = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "woodRelease",
            path_icon = "ui/icons/WoodstyleR",
            rate_inherit = 25,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        WoodR.base_stats["damage"] = 3f;
        WoodR.base_stats["attack_speed"] = 1f;
        WoodR.base_stats["intelligence"] = 5;
        WoodR.base_stats["chakra"] = 50f;
        WoodR.can_be_given = true;
        WoodR.action_attack_target = JinchurikiActions.WoodStyle;
        WoodR.group_id = JinchurikiTraitGroup.ChakraNTrait2;
        AssetManager.traits.add(WoodR);
        //         PlayerConfig.unlockTrait("woodRelease");
        // addTraitToLocalizedLibrary(WoodR.id, "WIP skill (made by Nickk)");

        ActorTrait LightningCloakJ = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "LC",
            path_icon = "ui/icons/LightningScroll",
            rate_inherit = 5,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        LightningCloakJ.base_stats["chakra"] = 10f;
        LightningCloakJ.can_be_given = true;
        LightningCloakJ.action_get_hit = JinchurikiActions.LightningActiv;
        LightningCloakJ.group_id = JinchurikiTraitGroup.JutsuTrait;
        AssetManager.traits.add(LightningCloakJ);
        //         PlayerConfig.unlockTrait("LC");

        ActorTrait LightningE = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "raiton",
            path_icon = "ui/icons/Lightning",
            rate_inherit = 5,
            rate_birth = 12,
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary
        };
        LightningE.base_stats["chakra"] = 10f;
        LightningE.can_be_given = true;
        LightningE.action_special_effect = JinchurikiActions.ApplyLightningJutsus;
        LightningE.group_id = JinchurikiTraitGroup.ChakraNTrait;
        AssetManager.traits.add(LightningE);
        //         PlayerConfig.unlockTrait("raiton");
        // addTraitToLocalizedLibrary(LightningE.id, "The unit can use lightning based Jutsus!");

        ActorTrait KirinJ = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "kirin",
            path_icon = "ui/icons/LightningScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        KirinJ.base_stats["chakra"] = 5f;
        KirinJ.can_be_given = true;
        KirinJ.action_special_effect = JinchurikiActions.Kirin;
        KirinJ.group_id = JinchurikiTraitGroup.JutsuTrait;
        AssetManager.traits.add(KirinJ);
        //         PlayerConfig.unlockTrait("kirin");
        // addTraitToLocalizedLibrary(KirinJ.id, "The unit can use the Kirin!");

        ActorTrait Rasengan = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "rasengan",
            path_icon = "ui/icons/HLWindScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        Rasengan.base_stats["chakra"] = 10f;
        Rasengan.can_be_given = true;
        Rasengan.group_id = JinchurikiTraitGroup.JutsuTrait;
        Rasengan.action_special_effect = JinchurikiActions.Rasengan;
        AssetManager.traits.add(Rasengan);
        //         PlayerConfig.unlockTrait("rasengan");
        // // addTraitToLocalizedLibrary(Rasengan.id, "The Jutsu of the fourth Hokage.");

        ActorTrait ChidoriJ = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "chidori",
            path_icon = "ui/icons/LightningScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ChidoriJ.base_stats["chakra"] = 5f;
        ChidoriJ.can_be_given = true;
        ChidoriJ.action_special_effect = JinchurikiActions.Chidori;
        ChidoriJ.group_id = JinchurikiTraitGroup.JutsuTrait;
        AssetManager.traits.add(ChidoriJ);
        //         PlayerConfig.unlockTrait("chidori");
        // addTraitToLocalizedLibrary(ChidoriJ.id, "The unit can use the Chidori!");

        ActorTrait FireE = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "fireRelease",
            path_icon = "ui/icons/Fire",
            rate_inherit = 15,
            rate_birth = 5,
            needs_to_be_explored = false
        };
        FireE.base_stats["chakra"] = 10f;
        FireE.can_be_given = true;
        // FireE.action_attack_target = new AttackAction(ActionLibrary.addBurningEffectOnTarget);
        FireE.group_id = JinchurikiTraitGroup.ChakraNTrait;
        AssetManager.traits.add(FireE);
        //         PlayerConfig.unlockTrait("fireRelease");
        // addTraitToLocalizedLibrary(FireE.id, "WIP skill (made by LordSchrott)");

        ActorTrait WindE = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "windRelease",
            path_icon = "ui/icons/Wind",
            rate_inherit = 15,
            rate_birth = 3,
            needs_to_be_explored = false
        };
        WindE.base_stats["chakra"] = 10f;
        WindE.can_be_given = true;
        WindE.group_id = JinchurikiTraitGroup.ChakraNTrait;
        AssetManager.traits.add(WindE);
        //         PlayerConfig.unlockTrait("windRelease");
        // addTraitToLocalizedLibrary(WindE.id, "WIP skill (made by LordSchrott)");

        ActorTrait WaterE = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "waterRelease",
            path_icon = "ui/icons/Water",
            rate_inherit = 14,
            rate_birth = 40,
            needs_to_be_explored = false
        };
        WaterE.base_stats["chakra"] = 10f;
        WaterE.can_be_given = true;
        WaterE.group_id = JinchurikiTraitGroup.ChakraNTrait;
        AssetManager.traits.add(WaterE);
        //         PlayerConfig.unlockTrait("waterRelease");
        // addTraitToLocalizedLibrary(WaterE.id, "WIP skill (made by LordSchrott)");

        ActorTrait EarthE = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "earthRelease",
            path_icon = "ui/icons/Earth",
            rate_inherit = 14,
            rate_birth = 45,
            needs_to_be_explored = false
        };
        EarthE.base_stats["chakra"] = 10f;
        EarthE.can_be_given = true;
        EarthE.group_id = JinchurikiTraitGroup.ChakraNTrait;
        AssetManager.traits.add(EarthE);
        //         PlayerConfig.unlockTrait("earthRelease");
        // addTraitToLocalizedLibrary(EarthE.id, "WIP skill (made by LordSchrott)");

        ActorTrait YinR = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "yinRelease",
            path_icon = "ui/icons/YinR",
            rate_inherit = 50,
            rate_birth = 1,
            needs_to_be_explored = false
        };
        YinR.base_stats["damage"] = 5f;
        YinR.base_stats["attack_speed"] = 21f;
        YinR.base_stats["intelligence"] = 20;
        YinR.base_stats["chakra"] = 50f;
        YinR.action_special_effect = (WorldAction)Delegate.Combine(YinR.action_special_effect, new WorldAction(YinYangFuse));
        YinR.can_be_given = true;
        YinR.group_id = JinchurikiTraitGroup.ChakraNTrait2;
        AssetManager.traits.add(YinR);
        //         PlayerConfig.unlockTrait("yinRelease");
        // addTraitToLocalizedLibrary(YinR.id, "WIP skill (made by LordSchrott)");

        ActorTrait YangR = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "yangRelease",
            path_icon = "ui/icons/YangR",
            rate_inherit = 50,
            rate_birth = 1,
            needs_to_be_explored = false
        };
        YangR.base_stats["damage"] = 5f;
        YangR.base_stats["attack_speed"] = 21f;
        YangR.base_stats["intelligence"] = 20;
        YangR.base_stats["chakra"] = 50f;
        YangR.can_be_given = true;
        YangR.group_id = JinchurikiTraitGroup.ChakraNTrait2;
        AssetManager.traits.add(YangR);
        //         PlayerConfig.unlockTrait("yangRelease");
        // addTraitToLocalizedLibrary(YangR.id, "WIP skill (made by LordSchrott)");

        ActorTrait YinYangR = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "yinYangRelease",
            path_icon = "ui/icons/YinYang",
            rate_inherit = 65,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        YinYangR.base_stats["damage"] = 10f;
        YinYangR.base_stats["attack_speed"] = 42f;
        YinYangR.base_stats["intelligence"] = 40;
        YinYangR.base_stats["chakra"] = 100f;
        YinYangR.can_be_given = true;
        YinYangR.group_id = JinchurikiTraitGroup.ChakraNTrait2;
        AssetManager.traits.add(YinYangR);
        //         PlayerConfig.unlockTrait("yinYangRelease");
        // addTraitToLocalizedLibrary(YinYangR.id, "WIP skill (made by LordSchrott)");

        ActorTrait adamiteJ = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "adamantineSealingChains",
            path_icon = "ui/icons/YangScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        adamiteJ.base_stats["chakra"] = 5f;
        adamiteJ.can_be_given = true;
        adamiteJ.action_get_hit = JinchurikiActions.ChainsUzumaki;
        adamiteJ.group_id = JinchurikiTraitGroup.JutsuTrait;
        AssetManager.traits.add(adamiteJ);
        //         PlayerConfig.unlockTrait("adamantineSealingChains");
        // addTraitToLocalizedLibrary(adamiteJ.id, "An A-Rank Ninjutsu Yang scroll, it contains an Hidden Clan Technique. Tho it requires Yang release and Uzumaki.");

        ActorTrait ShadowCloneJ = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "shadowCloneJutsu",
            path_icon = "ui/icons/SpecialScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        ShadowCloneJ.base_stats["chakra"] = 3f;
        ShadowCloneJ.can_be_given = true;
        ShadowCloneJ.group_id = JinchurikiTraitGroup.JutsuTrait;
        AssetManager.traits.add(ShadowCloneJ);
        ShadowCloneJ.action_special_effect += JinchurikiActions.ShadowCloneJutsu;
        //         PlayerConfig.unlockTrait("shadowCloneJutsu");

        ActorTrait MultiSizeJ = new ActorTrait
        {
            base_stats = new BaseStats(),
            id = "multisizej",
            path_icon = "ui/icons/YangScroll",
            rate_inherit = 0,
            rate_birth = 0,
            needs_to_be_explored = false
        };
        MultiSizeJ.base_stats["chakra"] = 7f;
        MultiSizeJ.can_be_given = true;
        MultiSizeJ.group_id = JinchurikiTraitGroup.JutsuTrait;
        MultiSizeJ.action_get_hit = JinchurikiActions.Bigguy;
        AssetManager.traits.add(MultiSizeJ);
        //         PlayerConfig.unlockTrait("multisizej");


        // Example.base_stats["fertility"] = 0.0f;
        //Example.base_stats["max_children"] = 0f;
        // Example.base_stats["multiplier_lifespan"] = 0f;
        // Example.base_stats["attack_speed"] = 0;
        // Example.base_stats["damage"] = 0;
        // Example.base_stats["speed"] = 0f;
        // Example.base_stats["health"] = 0;
        // Example.base_stats["accuracy"] = 0f;
        // Example.base_stats["range"] = 0;
        // Example.base_stats["armor"] = 0;
        // Example.base_stats["scale"] = 0.0f;
        // Example.base_stats["dodge"] = 0f;
        //  Example.base_stats["targets"] = 0f;
        //  Example.base_stats["critical_chance"] = 0.0f;
        //  Example.base_stats["knockback"] = 0f;
        // Example.base_stats["knockback_reduction"] = 0f;
        // Example.base_stats["intelligence"] = 0;
        // Example.base_stats["warfare"] = 0;
        //  Example.base_stats["diplomacy"] = 0;
        // Example.base_stats["stewardship"] = 0;
        //  Example.base_stats["opinion"] = 0f;
        //  Example.base_stats["loyalty_traits"] = 0f;
        //  Example.base_stats["cities"] = 0;
        //  Example.base_stats["zone_range"] = 0;   

    }


    public static bool NineTailedRegeneration(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f))
        {
            a.restoreHealth(4 + a.data.level);
            if (Randy.randomChance(0.1f))
            {
                a.removeTrait("crippled");
                a.removeTrait("eyepatch");
                a.removeTrait("skin_burns");
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool YinYangFuse(BaseSimObject pTarget, WorldTile pTile = null)
    {
        if (pTarget.a.hasTrait("yangRelease"))
        {
            pTarget.a.removeTrait("yinRelease");
            pTarget.a.removeTrait("yangRelease");
            pTarget.a.addTrait("yinYangRelease");
            return true;
        }
        else
        {
            return false;
        }
    }

    // Seal evolution for Jinchuriki
    public static bool Sealevo(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;
        AttackType pType = AttackType.Tumor;

        //evolutionary tailed beast code
        if (a.data.getAge() >= 12)
        {
            if (Randy.randomChance(0.05f))
            {
                // a.addTrait("Tailed-Beast Chakra");
                a.addTrait("Rampaged Jinchuriki");
                a.addTrait("madness");
                a.removeTrait("Jinchuriki");
                a.restoreHealth(100);
            }

            else if (Randy.randomChance(0.05f))
            {
                // a.addTrait("Tailed-Beast Chakra");
                a.addTrait("Controlled Jinchuriki");
                a.removeTrait("Jinchuriki");
                a.restoreHealth(100);

            }
        }

        if (a.data.health < a.getMaxHealth() / 3)
        {
            if (Randy.randomChance(0.01f))
            {
                a.addTrait("Tailed-Beast Chakra");
                a.addTrait("Rampaged Jinchuriki");
                a.addTrait("madness");
                a.removeTrait("Jinchuriki");
                a.restoreHealth(100);
            }

            else if (Randy.randomChance(0.01f))
            {
                a.addTrait("Tailed-Beast Chakra");
                a.addTrait("Controlled Jinchuriki");
                a.removeTrait("Jinchuriki");
                a.restoreHealth(100);
            }
        }
        return true;
    }

    public static bool PartnerCTB(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
        Actor a = pSelf.a;

        if (!JinchurikiActions.hasChakraPercent(a, 0.6f)) return false;

        if (Randy.randomChance(0.6f))
        {
            if (a.hasTrait("yinYangRelease") && a.data.level >= 19)
            {
                JinchurikiActions.addStatus(a, "Six Path Sage", 200);
                return true;
            }
            else if (a.data.level >= 15 && !a.hasStatus("Six Path Sage"))
            {
                JinchurikiActions.addStatus(a, "Kyuubi Sage", 140);
                return true;
            }
            else if (a.data.level >= 11 && !a.hasStatus("Six Path Sage") && !a.hasStatus("Kyuubi Sage"))
            {
                JinchurikiActions.addStatus(a, "Link Mode", 100);
                return true;
            }
        }

        return false;
    }

    /// Tornado punch
    public static bool BeastSpeed(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
        if (pTarget.a != null)
        {
            Actor a = pSelf.a;
            if (Randy.randomChance(0.05f))
            {
                ActionLibrary.castTornado(null, pTarget, pTile);
            }

        }
        return true;
    }

    /// FURY OF THE BEAST!
    public static bool BeastPunch(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {

        Actor a = pTarget.a;
        if (Randy.randomChance(0.005f))
        {
            ActionLibrary.startNuke(pTarget, pTile);
        }

        return true;
    }

    /// Punch
    public static bool BaryonRage(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {

        Actor a = pTarget.a;
        if (Randy.randomChance(100.0f))
        {
            ActionLibrary.startNuke(pTarget, pTile);
        }

        return true;
    }

    public static bool UnrealRegeneration(BaseSimObject pTarget, WorldTile pTile = null)
    {
        if (Randy.randomChance(100.0f))
        {
            pTarget.a.restoreHealth(130);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// The flash punches, for baryon
    public static bool StrengthDisplay(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
        if (pTarget.a != null)
        {
            Actor a = pSelf.a;
            if (Randy.randomChance(1.09f))
            {
                // World.world.getObjectsInChunks(pTile, 5, MapObjectType.Actor);
                // for (int i = 0; i < World.world.temp_map_objects.Count; i++)
                // {                            
                //     Actor actor = (Actor)World.world.temp_map_objects[i];
                //     // MapBox.instance.spawnFlash(pTile, 1);
                //     EffectsLibrary.spawn("fx_nuke_flash", pSelf.a.current_tile, null, null, 0f, -1f, -1f);
                // }
            }
        }

        return true;
    }

    /// Death of the sage, and jinchuriki's last stance
    public static bool BaryonActivation(BaseSimObject pTarget, WorldTile pTile = null)
    {
        if (pTarget.hasStatus("Shadow Clone"))
        {
            return false;
        }
        if (pTarget != null)
        {
            if (Randy.randomChance(0.25f))
            {
                Actor a = pTarget.a;
                a.removeTrait("nineTailedPartner");
                a.removeTrait("jinchuriki");
                var act = World.world.units.createNewUnit(a.asset.id, pTile);

                ActionLibrary.castLightning(null, pTarget, pTile);

                ActorTool.copyUnitToOtherUnit(a, act);
                act.kingdom = pTarget.kingdom;
                // act.data.skin_id = a.data.skin_id;
                act.data.head = a.data.head;
                act.addStatusEffect("Kuramas Death");
                act.data.health = a.getMaxHealth();
            }
        }

        return true;
    }

    public static void addTraitToLocalizedLibrary(string id, string description)
    {
        if (!LocalizedTextManager.instance.contains("trait_" + id))
        {
            LocalizedTextManager.add("trait_" + id, id);
            LocalizedTextManager.add("trait_" + id + "_info", description);
        }
    }

}

/// Nickk and LordSchrott were here 😈 

