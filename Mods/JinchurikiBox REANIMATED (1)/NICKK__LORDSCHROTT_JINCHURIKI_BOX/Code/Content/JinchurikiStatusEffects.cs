using System;
using NeoModLoader;
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
using NeoModLoader;
using NeoModLoader.General;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using JinchurikiBox.Core;
using JinchurikiBox.Actions;




namespace JinchurikiBox.Content;
internal static class JinchurikiStatusAssets
{
    public static void init()
    {
        Material material = LibraryMaterials.instance.dict["mat_world_object_lit"];

        if (AssetManager.projectiles.get("arrow") == null)
        {
            ProjectileAsset defArrow = new ProjectileAsset();
            defArrow.id = "arrow";
            defArrow.speed = 20f;
            defArrow.texture = "arrow";
            defArrow.look_at_target = true;
            defArrow.scale_start = 0.1f;
            defArrow.scale_target = 0.1f;
            defArrow.can_be_blocked = true;
            defArrow.can_be_collided = true;
            AssetManager.projectiles.add(defArrow);
        }

        StatusAsset Pain1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Pain1",
            locale_id = "status_Pain1",
            locale_description = "status_Pain1_locale_description",
            duration = 14f
        };
        Pain1.base_stats["range"] = -2f; 
        Pain1.base_stats["critical_chance"] = -10f;
        Pain1.base_stats["accuracy"] = -10f;
        Pain1.base_stats["armor"] = -3;
        Pain1.base_stats["damage"] = -25;
        Pain1.base_stats["attack_speed"] = -100;
        Pain1.base_stats["speed"] = -25;
        Pain1.path_icon = "ui/icons/Palms1";
        AssetManager.status.add(Pain1);

        StatusAsset Palmtech2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Palmtech2",
            locale_id = "status_Palmtech2",
            locale_description = "status_Palmtech2_locale_description",
            texture = "rotation",
            animated = true,
            animation_speed = 0.03f,
            duration = 3f,
            action = JinchurikiActions.RotationTechnique,
            action_interval = 0.5f,
            action_get_hit = JinchurikiActions.RotationDeflect
        };
        Palmtech2.opposite_status = new string[] { "stunned" };
        Palmtech2.base_stats["range"] = 3f; 
        Palmtech2.base_stats["armor"] = 2;
        Palmtech2.base_stats["damage"] = 25;
        Palmtech2.base_stats["attack_speed"] = 150;
        Palmtech2.base_stats["mass"] = 999f;

        Palmtech2.path_icon = "ui/icons/Palms1";
        AssetManager.status.add(Palmtech2);

        var stunnedAsset = AssetManager.status.get("stunned");
        if (stunnedAsset != null)
        {
            var current = stunnedAsset.opposite_status;
            var list = current != null ? new List<string>(current) : new List<string>();
            if (!list.Contains("Palmtech2"))
                list.Add("Palmtech2");
            stunnedAsset.opposite_status = list.ToArray();
        }

        StatusAsset Palmtech1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Palmtech1",
            locale_id = "status_Palmtech1",
            locale_description = "status_Palmtech1_locale_description",
            texture = "64Palm",
            animated = true,
            duration = 15f
        };
        Palmtech1.base_stats["damage"] = 15;
        Palmtech1.base_stats["attack_speed"] = 70;
        Palmtech1.base_stats["speed"] = 24;
        Palmtech1.path_icon = "ui/icons/Palms1";
        AssetManager.status.add(Palmtech1);

        StatusAsset MultiSize = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "MultiSize",
            locale_id = "status_MSi",
            locale_description = "status_MSi_locale_description",
            duration = 25f
        };
        MultiSize.base_stats["armor"] = 7;
        MultiSize.base_stats["damage"] = 15;
        MultiSize.base_stats["speed"] = -20;
        MultiSize.path_icon = "ui/icons/Yangscroll";
        AssetManager.status.add(MultiSize);


        StatusAsset ButterflyWings = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "ButterflyMode",
            locale_id = "status_BM",
            locale_description = "status_BM_locale_description",
            texture = "ButterflyMode",
            animated = true,
            animation_speed = 0.1f,
            duration = 20f
        };
        ButterflyWings.base_stats["scale"] = 0.20f;
        ButterflyWings.base_stats["armor"] = 25;
        ButterflyWings.base_stats["damage"] = 34;
        ButterflyWings.base_stats["speed"] = 20;
        ButterflyWings.path_icon = "ui/icons/ButterflyMode";
        AssetManager.status.add(ButterflyWings);

        StatusAsset Lightningcloak = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "LightingCl",
            locale_id = "status_LC",
            locale_description = "status_LC_locale_description",
            texture = "Rebirth",
            animated = true,
            animation_speed = 0.1f,
            duration = 90f
        };
        Lightningcloak.base_stats["armor"] = 10;
        Lightningcloak.base_stats["damage"] = 48;
        Lightningcloak.base_stats["speed"] = 78;
        Lightningcloak.path_icon = "ui/icons/LightningScroll";
        AssetManager.status.add(Lightningcloak);

        StatusAsset Tens = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Tenseigan",
            locale_id = "status_tenseigan",
            locale_description = "status_tenseigan_locale_description",
            texture = "TenseiganChakra",
            animated = true,
            duration = 90f
        };
        Tens.base_stats["health"] = 450;
        Tens.base_stats["damage"] = 155;
        Tens.base_stats["attack_speed"] = 210;
        Tens.base_stats["speed"] = 48;
        Tens.base_stats["armor"] = 60;
        Tens.base_stats["range"] = 0.80f;
        Tens.base_stats["chakra"] = 350;
        Tens.path_icon = "ui/icons/Tenseigan";
        AssetManager.status.add(Tens);

        StatusAsset PulledRinne1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "StolenRinnegan",
            locale_id = "status_stolenrinne",
            locale_description = "status_stolenrinne_locale_description",
            duration = 86400f
        };
        PulledRinne1.base_stats["attack_speed"] = 50;
        PulledRinne1.base_stats["speed"] = 34;
        PulledRinne1.base_stats["range"] = 0.60f;
        PulledRinne1.base_stats["damage"] = 68;
        PulledRinne1.base_stats["armor"] = 12;
        PulledRinne1.path_icon = "ui/icons/PulledRin";
        AssetManager.status.add(PulledRinne1);

        StatusAsset PulledByaku1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "StolenByakup",
            locale_id = "status_stolenByakuganp",
            locale_description = "status_PulledByakuganp_locale_description",
            duration = 86400f
        };
        PulledByaku1.base_stats["speed"] = 16;
        PulledByaku1.base_stats["attack_speed"] = 32;
        PulledByaku1.base_stats["armor"] = 8;
        PulledByaku1.base_stats["damage"] = 32;
        PulledByaku1.path_icon = "ui/icons/PulledByak";
        AssetManager.status.add(PulledByaku1);

        StatusAsset PulledShar7 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "3 Tomoe-S eye",
            locale_id = "status_uchihaeye3",
            locale_description = "status_PulledShar7_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar7.base_stats["speed"] = 22;
        PulledShar7.base_stats["attack_speed"] = 48;
        PulledShar7.base_stats["range"] = 0.25f;
        PulledShar7.base_stats["damage"] = 25;
        PulledShar7.base_stats["armor"] = 2;
        AssetManager.status.add(PulledShar7);

        StatusAsset PulledShar6 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "2 Tomoe-S eye",
            locale_id = "status_uchihaeye2",
            locale_description = "status_PulledShar6_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar6.base_stats["speed"] = 16;
        PulledShar6.base_stats["attack_speed"] = 35;
        PulledShar6.base_stats["range"] = 0.15f;
        PulledShar6.base_stats["damage"] = 18;
        PulledShar6.base_stats["armor"] = 1;
        AssetManager.status.add(PulledShar6);

        StatusAsset PulledShar5 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "1 Tomoe-S eye",
            locale_id = "status_uchihaeye1",
            locale_description = "status_PulledShar1_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar5.base_stats["speed"] = 10;
        PulledShar5.base_stats["attack_speed"] = 22;
        PulledShar5.base_stats["range"] = 0.08f;
        PulledShar5.base_stats["damage"] = 12;
        PulledShar5.base_stats["armor"] = 0;
        AssetManager.status.add(PulledShar5);

        StatusAsset PulledShar4 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Uchiha's MS eye",
            locale_id = "status_uchihaeye",
            locale_description = "status_PulledShar4_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar4.base_stats["speed"] = 28;
        PulledShar4.base_stats["attack_speed"] = 70;
        PulledShar4.base_stats["range"] = 0.40f;
        PulledShar4.base_stats["damage"] = 42;
        PulledShar4.base_stats["armor"] = 5;
        AssetManager.status.add(PulledShar4);

        StatusAsset PulledShar3 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Madara's eye",
            locale_id = "status_madaraeye",
            locale_description = "status_PulledShar3_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar3.base_stats["speed"] = 30;
        PulledShar3.base_stats["attack_speed"] = 78;
        PulledShar3.base_stats["range"] = 0.50f;
        PulledShar3.base_stats["armor"] = 7;
        PulledShar3.base_stats["damage"] = 58;
        AssetManager.status.add(PulledShar3);

        StatusAsset PulledShar2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Indra's eye",
            locale_id = "status_indraeye",
            locale_description = "status_PulledShar2_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar2.base_stats["accuracy"] = 0.50f;
        PulledShar2.base_stats["speed"] = 32;
        PulledShar2.base_stats["attack_speed"] = 82;
        PulledShar2.base_stats["range"] = 0.55f;
        PulledShar2.base_stats["armor"] = 8;
        PulledShar2.base_stats["damage"] = 60;
        AssetManager.status.add(PulledShar2);

        StatusAsset PulledShar1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Itachi's eye",
            locale_id = "status_itachieye",
            locale_description = "status_PulledShar1_locale_description",
            duration = 86400f,
            animated = false,
            path_icon = "ui/icons/PulledShar"
        };
        PulledShar1.base_stats["accuracy"] = 50f;
        PulledShar1.base_stats["critical_chance"] = 0.55f;
        PulledShar1.base_stats["speed"] = 30;
        PulledShar1.base_stats["attack_speed"] = 75;
        PulledShar1.base_stats["range"] = 0.50f;
        PulledShar1.base_stats["armor"] = 7;
        PulledShar1.base_stats["damage"] = 55;
        AssetManager.status.add(PulledShar1);

        StatusAsset ItachiMS = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "ItachiMS",
            locale_id = "status_itachiMangekyo",
            locale_description = "status_ItachiMS_locale_description",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f,
            animated = false,
            path_icon = "ui/icons/ItachiMS"
        };
        ItachiMS.base_stats["accuracy"] = 50f;
        ItachiMS.base_stats["critical_chance"] = 0.55f;
        ItachiMS.base_stats["speed"] = 30;
        ItachiMS.base_stats["attack_speed"] = 75;
        ItachiMS.base_stats["range"] = 0.50f;
        ItachiMS.base_stats["armor"] = 7;
        ItachiMS.base_stats["damage"] = 55;
        ItachiMS.remove_status = new string[] { "MSharingan", "Sharingan3", "Sharingan2", "Sharingan" };
        AssetManager.status.add(ItachiMS);

        StatusAsset IndraMS = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "IndraMS",
            locale_id = "status_indraMangekyo",
            locale_description = "status_IndraMS_locale_description",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f,
            animated = false,
            path_icon = "ui/icons/IndraMS"
        };
        IndraMS.base_stats["accuracy"] = 0.50f;
        IndraMS.base_stats["speed"] = 32;
        IndraMS.base_stats["attack_speed"] = 82;
        IndraMS.base_stats["range"] = 0.55f;
        IndraMS.base_stats["armor"] = 8;
        IndraMS.base_stats["damage"] = 60;
        IndraMS.remove_status = new string[] { "MSharingan", "Sharingan3", "Sharingan2", "Sharingan" };
        AssetManager.status.add(IndraMS);

        // StatusAsset madarasusanoo1 = new StatusAsset() {
        //     id = "madarasusanoo1",
        //     locale_id = "status_madaraRibcage1",
        //     locale_description = "status_madaraRibcage1_locale_description",
        //     duration = 35f,
        //     animated = true,
        //     path_icon = "ui/icons/madaraMS"
        // };
        StatusAsset madarasusanoo2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "madarasusanoo2",
            locale_id = "status_madarasusanoo2",
            locale_description = "status_madarasusanoo2_locale_description",
            duration = 35f,
            animated = true,
            path_icon = "ui/icons/madaraMS",
            texture = "Suusanoo2",
            animation_speed = 0.1f
        };
        madarasusanoo2.base_stats["speed"] = 20;
        madarasusanoo2.base_stats["range"] = 4; 
        madarasusanoo2.base_stats["armor"] = 50;
        madarasusanoo2.base_stats["damage"] = 34;
        madarasusanoo2.remove_status = new string[] { "madarasusanoo1" };
        AssetManager.status.add(madarasusanoo2);

        StatusAsset madarasusanoo1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "madarasusanoo1",
            locale_id = "status_madaraRibcage1",
            locale_description = "status_madaraRibcage1_locale_description",
            duration = 35f,
            animated = true,
            path_icon = "ui/icons/madaraMS",
            texture = "Susanoo1",
            animation_speed = 0.1f
        };
        madarasusanoo1.base_stats["speed"] = 0;
        madarasusanoo1.base_stats["range"] = 0; 
        madarasusanoo1.base_stats["armor"] = 25;
        madarasusanoo1.base_stats["damage"] = 0;
        AssetManager.status.add(madarasusanoo1);

        // StatusAsset madaraMS = new StatusAsset() {
        //     id = "madaraMS",
        //     locale_id = "status_madaraMangekyo",
        //     locale_description = "status_madaraMS_locale_description",
        //     duration = 35f,
        //     path_icon = "ui/icons/madaraMS"
        // };
        StatusAsset madaraMS = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "madaraMS",
            locale_id = "status_madaraMangekyo",
            locale_description = "status_madaraMS_locale_description",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f,
            path_icon = "ui/icons/madaraMS"
        };
        madaraMS.base_stats["speed"] = 30;
        madaraMS.base_stats["attack_speed"] = 78;
        madaraMS.base_stats["range"] = 0.50f;
        madaraMS.base_stats["armor"] = 7;
        madaraMS.base_stats["damage"] = 58;
        madaraMS.remove_status = new string[] { "MSharingan", "Sharingan3", "Sharingan2", "Sharingan" };
        AssetManager.status.add(madaraMS);


        // StatusAsset ChainedGiver = new StatusAsset() {
        //     id = "ChainedAttacker",
        //     locale_id = "status_adamantineChainsAttacker",
        //     animated = true,
        //     duration = 35f,
        //     locale_description = "status_adamantineChainsAttacker_locale_description",
        //     path_icon = "ui/icons/UZclan"
        // };
        StatusAsset ChainedGiver = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "ChainedAttacker",
            locale_id = "status_adamantineChainsAttacker",
            animated = true,
            duration = 35f,
            locale_description = "status_adamantineChainsAttacker_locale_description",
            path_icon = "ui/icons/UZclan",
            texture = "ChainAttack",
            animation_speed = 0.1f
        };
        ChainedGiver.base_stats["speed"] = 35;
        ChainedGiver.base_stats["range"] = 3f; 
        ChainedGiver.base_stats["armor"] = 10;
        ChainedGiver.base_stats["damage"] = 100;
        AssetManager.status.add(ChainedGiver);
        // addTraitToLocalizedLibrary(ChainedGiver.locale_id, ChainedGiver.locale_description, "(A Tier Hidden-Ninjutsu) Adamantine Sealing Chains", "They've entered an form that is kept secret within their clan..");

        // StatusAsset Chained = new StatusAsset() {
        //     id = "ChainedGuy",
        //     locale_id = "status_adamantineChains",
        //     animated = true,
        //     duration = 10f,
        //     path_icon = "ui/icons/UZclan",
        //     locale_description = "status_adamantineChains_locale_description"
        // };
        StatusAsset Chained = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "ChainedGuy",
            locale_id = "status_adamantineChains",
            animated = true,
            duration = 10f,
            path_icon = "ui/icons/UZclan",
            locale_description = "status_adamantineChains_locale_description",
            texture = "Chained",
            animation_speed = 0.1f
        };
        Chained.base_stats["speed"] = -1000;
        Chained.base_stats["range"] = -100f; 
        Chained.base_stats["accuracy"] = -100 ;
        // Chained.action = JinchurikiActions.WoodStyle_Damage;
        AssetManager.status.add(Chained);
        // addTraitToLocalizedLibrary(Chained.locale_id, Chained.locale_description, "Adamantine Chains of Sealing", "They've been caught in this jutsu, what is it?....");

        StatusAsset WoodSheild = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "WoodSheild",
            locale_id = "status_kakuan",
            texture = "bringing",
            animated = true,
            animation_speed = 0.1f,
            duration = 10f
        };
        WoodSheild.base_stats["speed"] = -1000;
        WoodSheild.base_stats["range"] = -100f; 
        WoodSheild.base_stats["accuracy"] = -100 ;
        WoodSheild.action = JinchurikiActions.WoodStyle_Damage;
        WoodSheild.locale_description = "They're being suppressed spiritually as of now.";
        WoodSheild.path_icon = "ui/icons/Woodstyle";
        AssetManager.status.add(WoodSheild);
        // addTraitToLocalizedLibrary(WoodSheild.locale_id, WoodSheild.locale_description, "(C Tier Ninjutsu) Kakuan", "They're being suppressed spiritually as of now.");

        // StatusAsset SharGenjutsu = new StatusAsset() {
        //     id = "sharinganGenjutsu",
        //     locale_id = "status_falseReality",
        //     animated = true,
        //     duration = 5f,
        //     path_icon = "ui/icons/WATCHING",
        //     locale_description = "status_falseReality_locale_description",
        // };
        StatusAsset SharGenjutsu = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "sharinganGenjutsu",
            locale_id = "status_falseReality",
            animated = true,
            duration = 5f,
            path_icon = "ui/icons/WATCHING",
            locale_description = "status_falseReality_locale_description",
            texture = "sgenAura",
            animation_speed = 0.1f
        };
        SharGenjutsu.base_stats["speed"] = -1000;
        SharGenjutsu.base_stats["accuracy"] = -100;
        AssetManager.status.add(SharGenjutsu);
        // addTraitToLocalizedLibrary(SharGenjutsu.locale_id, SharGenjutsu.locale_description, "(E Tier Genjutsu) False Reality", "I cant tell if this is reality..");

        StatusAsset BurningA = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "amaterasu",
            locale_id = "status_amaterasu",
            texture = "blackflamesAura",
            animated = true,
            animation_speed = 0.1f,
            duration = 86400f
        };
        BurningA.base_stats["speed"] = -40;
        BurningA.locale_description = "status_amaterasu_locale_description";
        BurningA.path_icon = "ui/icons/Amate";
        BurningA.action = JinchurikiActions.Amaterasu;
        BurningA.action_interval = 1f;
        AssetManager.status.add(BurningA);
        // addTraitToLocalizedLibrary(BurningA.locale_id, BurningA.locale_description, "Amaterasu", "IT BURNS, THE FLAMES.. THEY WONT GO OUT");

        StatusAsset Gate8 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate8",
            locale_id = "status_gateOfDeath",
            texture = "EG",
            animated = true,
            animation_speed = 0.1f,
            duration = 90f
        };
        Gate8.base_stats["speed"] = 13;
        Gate8.base_stats["attack_speed"] = 75;
        Gate8.base_stats["armor"] = 5;
        Gate8.base_stats["damage"] = 70;
        Gate8.base_stats["range"] = 0.35f;
        Gate8.base_stats["chakra"] = 20f;
        Gate8.locale_description = "status_gateOfDeath_locale_description";
        Gate8.path_icon = "ui/icons/heart";
        Gate8.action = JinchurikiActions.EightGatesDrain;
        Gate8.action_interval = 1f;
        AssetManager.status.add(Gate8);
        // addTraitToLocalizedLibrary(Gate8.locale_id, Gate8.locale_description, "Gate of Death", "After using their technique, they open the last gate, which is located at the heart. A steam of blood is emitted from the user's pores and hair, resembling a flaming red aura.");

        StatusAsset Gate7 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate7",
            locale_id = "status_gateOfWonder",
            duration = 90f
        };
        Gate7.base_stats["speed"] = 8;
        Gate7.base_stats["attack_speed"] = 40;
        Gate7.base_stats["damage"] = 45;
        Gate7.base_stats["armor"] = 7;
        Gate7.base_stats["chakra"] = 20f;
        Gate7.locale_description = "After using their technique, they open the seventh gate, which is located below the stomach. Causes the user's body to secrete a blue sweat.";
        Gate7.path_icon = "ui/icons/stomach";
        Gate7.action = JinchurikiActions.EightGatesDrain;
        Gate7.action_interval = 1f;
        AssetManager.status.add(Gate7);
        // addTraitToLocalizedLibrary(Gate7.locale_id, Gate7.locale_description, "Gate of Wonder", "After using their technique, they open the seventh gate, which is located below the stomach. Causes the user's body to secrete a blue sweat.");

        StatusAsset Gate6 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate6",
            locale_id = "status_gateOfView",
            duration = 90f
        };
        Gate6.base_stats["speed"] = 7;
        Gate6.base_stats["attack_speed"] = 28;
        Gate6.base_stats["armor"] = 6;
        Gate6.base_stats["damage"] = 32;
        Gate6.base_stats["chakra"] = 20f;
        Gate6.locale_description = "After using their technique, they open the sixth gate, which is located in the stomach. Creating an aura around the user.";
        Gate6.path_icon = "ui/icons/stomach";
        Gate6.action = JinchurikiActions.EightGatesDrain;
        Gate6.action_interval = 1f;
        AssetManager.status.add(Gate6);
        // addTraitToLocalizedLibrary(Gate6.locale_id, Gate6.locale_description, "Gate of View", "After using their technique, they open the sixth gate, which is located in the stomach. Creating an aura around the user.");

        StatusAsset Gate5 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate5",
            locale_id = "status_gateOfLimit",
            duration = 90f
        };
        Gate5.base_stats["speed"] = 6;
        Gate5.base_stats["attack_speed"] = 22;
        Gate5.base_stats["damage"] = 24;
        Gate5.base_stats["armor"] = 6;
        Gate5.locale_description = "After using their technique, they open the fifth gate, which is located in the abdomen. Limbs receive compound fractures.";
        Gate5.path_icon = "ui/icons/stomach";
        Gate5.action = JinchurikiActions.EightGatesDrain;
        Gate5.action_interval = 1f;
        AssetManager.status.add(Gate5);
        // addTraitToLocalizedLibrary(Gate5.locale_id, Gate5.locale_description, "Gate of Limit", "After using their technique, they open the fifth gate, which is located in the abdomen. Limbs receive compound fractures.");

        StatusAsset Gate4 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate4",
            locale_id = "status_gateOfPain",
            duration = 90f
        };
        Gate4.base_stats["speed"] = 5;
        Gate4.base_stats["attack_speed"] = 18;
        Gate4.base_stats["damage"] = 18;
        Gate4.base_stats["armor"] = 5;
        Gate4.base_stats["chakra"] = 20f;
        Gate4.locale_description = "After using their technique, they open the fourth gate, which is located in the spinal cord. Muscles start to tear.";
        Gate4.path_icon = "ui/icons/spine";
        Gate4.action = JinchurikiActions.EightGatesDrain;
        Gate4.action_interval = 1f;
        AssetManager.status.add(Gate4);
        // addTraitToLocalizedLibrary(Gate4.locale_id, Gate4.locale_description, "Gate of Pain", "After using their technique, they open the fourth gate, which is located in the spinal cord. Muscles start to tear.");

        StatusAsset Gate3 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate3",
            locale_id = "status_gateOfLife",
            duration = 90f
        };
        Gate3.base_stats["speed"] = 4;
        Gate3.base_stats["attack_speed"] = 12;
        Gate3.base_stats["armor"] = 5;
        Gate3.base_stats["damage"] = 12;
        Gate3.base_stats["chakra"] = 20f;
        Gate3.locale_description = "After using their technique, they open the third gate, which is located in the spinal cord. Turning their skin red.";
        Gate3.path_icon = "ui/icons/spine";
        Gate3.action = JinchurikiActions.EightGatesDrain;
        Gate3.action_interval = 1f;
        AssetManager.status.add(Gate3);
        // addTraitToLocalizedLibrary(Gate3.locale_id, Gate3.locale_description, "Gate of Life", "After using their technique, they open the third gate, which is located in the spinal cord. Turning their skin red.");

        StatusAsset Gate2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate2",
            locale_id = "status_gateOfHealing",
            duration = 90f
        };
        Gate2.base_stats["speed"] = 3;
        Gate2.base_stats["attack_speed"] = 10;
        Gate2.base_stats["damage"] = 10;
        Gate2.base_stats["armor"] = 3;
        Gate2.locale_description = "After using their technique, they tricked their brain into releasing more of a potential of their full chakra, thus opening the second gate.";
        Gate2.path_icon = "ui/icons/Brain";
        Gate2.action = JinchurikiActions.EightGatesDrain;
        Gate2.action_interval = 1f;
        AssetManager.status.add(Gate2);
        // addTraitToLocalizedLibrary(Gate2.locale_id, Gate2.locale_description, "Gate of Healing", "After using their technique, they tricked their brain into releasing more of a potential of their full chakra, thus opening the second gate.");

        StatusAsset Gate1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "gate1",
            locale_id = "status_gateOfOpening",
            duration = 90f
        };
        Gate1.base_stats["speed"] = 4;
        Gate1.base_stats["attack_speed"] = 12;
        Gate1.base_stats["armor"] = 3;
        Gate1.base_stats["damage"] = 8;
        Gate1.locale_description = "After using their technique, they tricked their brain into releasing more of a potential of their full chakra, thus opening the first gate.";
        Gate1.path_icon = "ui/icons/Brain";
        Gate1.action = JinchurikiActions.EightGatesDrain;
        Gate1.action_interval = 1f;
        AssetManager.status.add(Gate1);
        // addTraitToLocalizedLibrary(Gate1.locale_id, Gate1.locale_description, "Gate of Opening", "After using their technique, they tricked their brain into releasing more of a potential of their full chakra, thus opening the first gate.");

        StatusAsset MSharingan = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "MSharingan",
            locale_id = "status_mSharingan",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        MSharingan.base_stats["speed"] = 28;
        MSharingan.base_stats["attack_speed"] = 70;
        MSharingan.base_stats["range"] = 0.40f;
        MSharingan.base_stats["damage"] = 42;
        MSharingan.base_stats["armor"] = 5;
        MSharingan.locale_description = "They are a user of the Mangekyō Sharingan, subjected to nothing but the cycle of hatred.";
        MSharingan.path_icon = "ui/icons/ShinMS";
        MSharingan.remove_status = new string[] { "Sharingan3", "Sharingan2", "Sharingan" };
        AssetManager.status.add(MSharingan);
        // addTraitToLocalizedLibrary(MSharingan.locale_id, MSharingan.locale_description, "Mangekyō Sharingan", "They are a user of the Mangekyō Sharingan, subjected to nothing but the cycle of hatred.");

        StatusAsset Sharingan3 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Sharingan3",
            locale_id = "status_sharingan3",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        Sharingan3.base_stats["speed"] = 22;
        Sharingan3.base_stats["attack_speed"] = 48;
        Sharingan3.base_stats["range"] = 0.25f;
        Sharingan3.base_stats["damage"] = 25;
        Sharingan3.base_stats["armor"] = 2;
        Sharingan3.locale_description = "After countless battles and witnessing so much death, they unlock and fully master their sharingan, or did they?.";
        Sharingan3.path_icon = "ui/icons/3T";
        Sharingan3.remove_status = new string[] { "Sharingan2", "Sharingan" };
        AssetManager.status.add(Sharingan3);
        // addTraitToLocalizedLibrary(Sharingan3.locale_id, Sharingan3.locale_description, "Three Tomoe Sharingan", "After countless battles and witnessing so much death, they unlock and fully master their sharingan, or did they?.");

        StatusAsset Sharingan2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Sharingan2",
            locale_id = "status_sharingan2",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        Sharingan2.base_stats["speed"] = 16;
        Sharingan2.base_stats["attack_speed"] = 35;
        Sharingan2.base_stats["range"] = 0.15f;
        Sharingan2.base_stats["damage"] = 18;
        Sharingan2.base_stats["armor"] = 1;
        Sharingan2.locale_description = "After fighting and training, they upgrade their visual prowess. Unlocking the second Tomoe.";
        Sharingan2.path_icon = "ui/icons/2T";
        Sharingan2.remove_status = new string[] { "Sharingan" };
        AssetManager.status.add(Sharingan2);
        // addTraitToLocalizedLibrary(Sharingan2.locale_id, Sharingan2.locale_description, "Two Tomoe Sharingan", "After fighting and training, they upgrade their visual prowess. Unlocking the second Tomoe.");

        StatusAsset Sharingan = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Sharingan",
            locale_id = "status_sharingan",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        Sharingan.base_stats["speed"] = 10;
        Sharingan.base_stats["attack_speed"] = 22;
        Sharingan.base_stats["range"] = 0.08f;
        Sharingan.base_stats["damage"] = 12;
        Sharingan.base_stats["armor"] = 0;
        Sharingan.locale_description = "After seeing so much cruelity they awaken their clans prized possession, the sharingan.";
        Sharingan.path_icon = "ui/icons/1T";
        AssetManager.status.add(Sharingan);
        // addTraitToLocalizedLibrary(Sharingan.locale_id, Sharingan.locale_description, "One Tomoe Sharingan", "After seeing so much cruelity they awaken their clans prized possession, the sharingan.");

        StatusAsset Jogan = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Jog",
            locale_id = "status_jogan",
            duration = 120f
        };
        Jogan.base_stats["speed"] = 40f;
        Jogan.base_stats["attack_speed"] = 90f;
        Jogan.base_stats["range"] = 0.25f;
        Jogan.base_stats["armor"] = 10;
        Jogan.base_stats["damage"] = 41;
        Jogan.base_stats["health"] = 250;
        Jogan.locale_description = "Their ancestory that dates back to the princess of the Otsutsuki earned them this eye.";
        Jogan.path_icon = "ui/icons/Jog";
        Jogan.action_interval = 1f;
        AssetManager.status.add(Jogan);
        // addTraitToLocalizedLibrary(Jogan.locale_id, Jogan.locale_description, "Pure Eye", "Their ancestory that dates back to the princess of the Otsutsuki earned them this eye.");

        StatusAsset SenjuSageM = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "SenjuS",
            locale_id = "status_senjuSage",
            duration = 60f
        };
        SenjuSageM.base_stats["damage"] = 65;
        SenjuSageM.base_stats["armor"] = 32;
        SenjuSageM.base_stats["attack_speed"] = 70;
        SenjuSageM.base_stats["health"] = 500;
        SenjuSageM.base_stats["speed"] = 24;
        SenjuSageM.base_stats["range"] = 0.25f;
        SenjuSageM.base_stats["chakra"] = 30;
        SenjuSageM.base_stats["sage_chakra"] = 100;
        SenjuSageM.base_stats["sage_chakra_regen"] = 6f;
        SenjuSageM.locale_description = "They are a in the Senju Sage Mode. Their power increases to the highest degree possible, their opponent should fear for their life.";
        SenjuSageM.path_icon = "ui/icons/SenjuS";
        SenjuSageM.action = JinchurikiActions.SageModeDrain;
        SenjuSageM.action_interval = 1f;
        AssetManager.status.add(SenjuSageM);
        // addTraitToLocalizedLibrary(SenjuSageM.locale_id, SenjuSageM.locale_description, "Senju Sage", "They are a in the Senju Sage Mode. Their power increases to the highest degree possible, their opponent should fear for their life.");

        StatusAsset SnakeSageMode = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "SnakeSage",
            locale_id = "status_snakeSage",
            duration = 60f
        };
        SnakeSageMode.base_stats["speed"] = 18;
        SnakeSageMode.base_stats["attack_speed"] = 30;
        SnakeSageMode.base_stats["armor"] = 18;
        SnakeSageMode.base_stats["damage"] = 35;
        SnakeSageMode.base_stats["range"] = 0.10f;
        SnakeSageMode.base_stats["health"] = 200;
        SnakeSageMode.base_stats["sage_chakra"] = 80;
        SnakeSageMode.base_stats["sage_chakra_regen"] = 6f;
        SnakeSageMode.locale_description = "They are a in the Snake Sage Mode. Their power increases insanely.";
        SnakeSageMode.path_icon = "ui/icons/SnakeS";
        SnakeSageMode.action = JinchurikiActions.SageModeDrain;
        SnakeSageMode.action_interval = 1f;
        AssetManager.status.add(SnakeSageMode);
        // addTraitToLocalizedLibrary(SnakeSageMode.locale_id, SnakeSageMode.locale_description, "Snake Sage Technique", "They are a in the Snake Sage Mode. Their power increases insanely.");

        StatusAsset SlugSageMode = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "OneHundredSeal",
            locale_id = "status_oneHundredHeal",
            duration = 60f
        };
        SlugSageMode.base_stats["speed"] = 0;
        SlugSageMode.base_stats["attack_speed"] = 35;
        SlugSageMode.base_stats["range"] = 0.50f;
        SlugSageMode.base_stats["armor"] = 42;
        SlugSageMode.base_stats["damage"] = 75;
        SlugSageMode.base_stats["health"] = 500;
        SlugSageMode.base_stats["sage_chakra"] = 60;
        SlugSageMode.base_stats["sage_chakra_regen"] = 6f;
        SlugSageMode.locale_description = "They are using the 100 Healings technique.";
        SlugSageMode.action = JinchurikiActions.SageModeDrain;
        SlugSageMode.action_interval = 1f;
        SlugSageMode.path_icon = "ui/icons/Slug2";
        AssetManager.status.add(SlugSageMode);
        // addTraitToLocalizedLibrary(SlugSageMode.locale_id, SlugSageMode.locale_description, "One-Hundred Healings Technique", "They are a in the Slug Sage Mode. Their power increases insanely.");

        StatusAsset ToadSageMode = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Toad Sage Mode",
            locale_id = "status_toadSage",
            duration = 60f
        };
        ToadSageMode.base_stats["speed"] = 20;
        ToadSageMode.base_stats["attack_speed"] = 60;
        ToadSageMode.base_stats["range"] = 0.20f;
        ToadSageMode.base_stats["armor"] = 24;
        ToadSageMode.base_stats["damage"] = 55;
        ToadSageMode.base_stats["health"] = 350;
        ToadSageMode.base_stats["sage_chakra"] = 80;
        ToadSageMode.base_stats["sage_chakra_regen"] = 6f;
        ToadSageMode.locale_description = "They are a in the Toad Sage Mode. Their power increases insanely.";
        ToadSageMode.path_icon = "ui/icons/SageEye";
        ToadSageMode.action = JinchurikiActions.SageModeDrain;
        ToadSageMode.action_interval = 1f;
        AssetManager.status.add(ToadSageMode);
        // addTraitToLocalizedLibrary(ToadSageMode.locale_id, ToadSageMode.locale_description, "Toad Sage Mode", "They are a in the Toad Sage Mode. Their power increases insanely." );

        StatusAsset ChidoriAtk = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Chidori Attack",
            locale_id = "status_chidori",
            texture = "chidoriActive",
            animated = true,
            animation_speed = 0.1f,
            duration = 8f
        };
        ChidoriAtk.base_stats["speed"] = 40f;
        ChidoriAtk.path_icon = "ui/icons/LightningScroll";
        ChidoriAtk.action_interval = 0.2f;
        ChidoriAtk.action = JinchurikiActions.ChidoriMovement;
        ChidoriAtk.locale_description = "They are about to use the chidori, a really strong technique!";
        AssetManager.status.add(ChidoriAtk);
        // addTraitToLocalizedLibrary(ChidoriAtk.locale_id, ChidoriAtk.locale_description, "Lightning Release: Chidori", "They are about to use the chidori, a really strong technique!");

        StatusAsset ShadowClone = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Shadow Clone",
            locale_id = "status_shadowClone"
        };
        ShadowClone.base_stats["multiplier_health"] = -0.99f;
        ShadowClone.duration = 46f;
        ShadowClone.locale_description = "They are a shadow clone.";
        ShadowClone.path_icon = "ui/icons/Smoke";
        ShadowClone.action = JinchurikiActions.CloneDestroy;
        ShadowClone.action_interval = 45f;
        AssetManager.status.add(ShadowClone);
        // addTraitToLocalizedLibrary(ShadowClone.locale_id, ShadowClone.locale_description, "Shadow Clone", "They are a shadow clone.");

        StatusAsset Checker = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Checker",
            locale_id = "Checker",
            duration = 3f,
            locale_description = "Removing Zombie trait, for your safety",
            path_icon = "ui/icons/Rinnegan",
            action = JinchurikiActions.Checker1,
            action_interval = 2f
        };
        AssetManager.status.add(Checker);
        // addTraitToLocalizedLibrary(Checker.locale_id, Checker.locale_description, "Checker", "Removing Zombie trait, for your safety");

        StatusAsset RinneganControlled = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Rinnegan Controlled",
            locale_id = "status_rinneganControlled",
            duration = 20f,
            locale_description = "They are controlled by a Rinnegan user.",
            path_icon = "ui/icons/Rinnegan",
            action = JinchurikiActions.setOnlyFightUnit,
            action_interval = 18f
        };
        AssetManager.status.add(RinneganControlled);
        // addTraitToLocalizedLibrary(RinneganControlled.locale_id, RinneganControlled.locale_description, "Under Control", "They are controlled by a Rinnegan user.");

        StatusAsset RinneganMade = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Rinnegan Created",
            locale_id = "status_rinneganCreation",
            duration = 20f,
            locale_description = "They are created by a Rinnegan user.",
            path_icon = "ui/icons/Rinnegan",
            action = JinchurikiActions.CloneDestroy,
            action_interval = 18f
        };
        AssetManager.status.add(RinneganMade);
        // addTraitToLocalizedLibrary(RinneganMade.locale_id, RinneganMade.locale_description, "Rinnegan Created", "They are created by a Rinnegan user.");

        StatusAsset RasenganActive = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Active Rasengan",
            locale_id = "status_rasengan",
            texture = "RAAASSS",
            animated = true,
            duration = 8f,
            locale_description = "status_rasengan_locale_description",
            path_icon = "ui/icons/WindScroll",
            action = JinchurikiActions.RaenganMovement,
            action_interval = 0.2f
        };
        AssetManager.status.add(RasenganActive);
        // addTraitToLocalizedLibrary(RasenganActive.locale_id, RasenganActive.locale_description, "Making Rasengan", "The fourths most famous jutsu!");

        StatusAsset GoldRinneganActive = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Active GoldRinnegan",
            locale_id = "status_goldRinnegan",
            duration = 30f,
            action = JinchurikiActions.RinneganDrain,
            action_interval = 1f
        };
        GoldRinneganActive.base_stats["attack_speed"] = 80;
        GoldRinneganActive.base_stats["speed"] = 42;
        GoldRinneganActive.base_stats["range"] = 0.90f;
        GoldRinneganActive.base_stats["damage"] = 105;
        GoldRinneganActive.base_stats["armor"] = 22;
        GoldRinneganActive.locale_description = "The evolved state of the normal Rinnegan, deprived of the Otsutsuki heritage...";
        GoldRinneganActive.path_icon = "ui/icons/GOLDENRI";
        GoldRinneganActive.opposite_status = new string[] { "Active Rinnegan", "StolenRinnegan" };
        GoldRinneganActive.remove_status = new string[] { "Active Rinnegan" };
        AssetManager.status.add(GoldRinneganActive);
        // addTraitToLocalizedLibrary(GoldRinneganActive.locale_id, GoldRinneganActive.locale_description, "Golden Rinnegan", "The evolved state of the normal Rinnegan, deprived of the Otsutsuki heritage...");

        StatusAsset RinneganActive = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Active Rinnegan",
            locale_id = "status_rinnegan",
            duration = 30f,
            action = JinchurikiActions.RinneganDrain,
            action_interval = 1f
        };
        RinneganActive.base_stats["attack_speed"] = 50;
        RinneganActive.base_stats["speed"] = 34;
        RinneganActive.base_stats["range"] = 0.60f;
        RinneganActive.base_stats["damage"] = 68;
        RinneganActive.base_stats["armor"] = 12;
        RinneganActive.locale_description = "The ultimate Dojutsu.";
        RinneganActive.path_icon = "ui/icons/Rinnegan";
        RinneganActive.opposite_status = new string[] { "StolenRinnegan" };
        AssetManager.status.add(RinneganActive);
        // addTraitToLocalizedLibrary(RinneganActive.locale_id, RinneganActive.locale_description, "Rinnegan", "The ultimate Dojutsu.");

        StatusAsset Byakugan2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "ByakuP",
            locale_id = "status_pureByakugan",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        Byakugan2.base_stats["speed"] = 16;
        Byakugan2.base_stats["attack_speed"] = 32;
        Byakugan2.base_stats["armor"] = 8;
        Byakugan2.base_stats["damage"] = 32;
        Byakugan2.locale_description = "Their Ancestory inheritage earned them this.";
        Byakugan2.path_icon = "ui/icons/Pure";
        Byakugan2.opposite_status = new string[] { "Byaku3" };
        Byakugan2.remove_status = new string[] { "Byaku3" };
        Byakugan2.action_get_hit = JinchurikiActions.BHit;
        AssetManager.status.add(Byakugan2);
        // addTraitToLocalizedLibrary(Byakugan2.locale_id, Byakugan2.locale_description, "Pure Byakugan", "Their Ancestory inheritage earned them this.");

        StatusAsset Byakugan1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "ByakuG",
            locale_id = "status_goldByakugan",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        Byakugan1.base_stats["speed"] = 30;
        Byakugan1.base_stats["attack_speed"] = 70;
        Byakugan1.base_stats["armor"] = 16;
        Byakugan1.base_stats["damage"] = 50;
        Byakugan1.locale_description = "Their Otsutsuki inheritage earned them this.";
        Byakugan1.path_icon = "ui/icons/Golden";
        Byakugan1.opposite_status = new string[] { "Byaku3", "ByakuP" };
        Byakugan1.remove_status = new string[] { "Byaku3", "ByakuP" };
        Byakugan1.action_get_hit = JinchurikiActions.BHit;
        AssetManager.status.add(Byakugan1);
        // addTraitToLocalizedLibrary(Byakugan1.locale_id, Byakugan1.locale_description, "Golden Byakugan", "Their Otsutsuki inheritage earned them this.");

        StatusAsset Byakugan = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Byaku3",
            locale_id = "status_byakugan",
            duration = 30f,
            action = JinchurikiActions.EyeModeDrain,
            action_interval = 1f
        };
        Byakugan.base_stats["speed"] = 10;
        Byakugan.base_stats["attack_speed"] = 20;
        Byakugan.base_stats["armor"] = 5;
        Byakugan.base_stats["damage"] = 22;
        Byakugan.locale_description = "One of the three Dojutsu.";
        Byakugan.path_icon = "ui/icons/Byakugan";
        Byakugan.action_get_hit = JinchurikiActions.BHit;
        AssetManager.status.add(Byakugan);
        // addTraitToLocalizedLibrary(Byakugan.locale_id, Byakugan.locale_description, "Byakugan", "One of the three Dojutsu.");

        StatusAsset KarmMarkW = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Ikarma1",
            locale_id = "status_karmaIncomplete",
            duration = 50f
        };
        KarmMarkW.base_stats["health"] = 425f;
        KarmMarkW.base_stats["scale"] = 0.015f;
        KarmMarkW.base_stats["speed"] = 20;
        KarmMarkW.base_stats["range"] = 0.20f;
        KarmMarkW.base_stats["armor"] = 10;
        KarmMarkW.base_stats["damage"] = 35;
        KarmMarkW.locale_description = "The strange markings on their hand has spreaded throughout their body, feeding them power.";
        KarmMarkW.path_icon = "ui/icons/KarmW1";
        AssetManager.status.add(KarmMarkW);
        // addTraitToLocalizedLibrary(KarmMarkW.locale_id, KarmMarkW.locale_description, "Incomplete Karma Seal", "The strange markings on their hand has spreaded throughout their body, feeding them power.");

        StatusAsset KarmMark2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "karma2",
            locale_id = "status_karmaResurrection",
            duration = 50f
        };
        KarmMark2.base_stats["health"] = 500f;
        KarmMark2.base_stats["scale"] = 0.025f;
        KarmMark2.base_stats["speed"] = 36;
        KarmMark2.base_stats["range"] = 0.55f;
        KarmMark2.base_stats["armor"] = 28;
        KarmMark2.base_stats["damage"] = 75;
        KarmMark2.locale_description = "After overusing their newly found power they've been implanted with too much DNA, thus they've lost control and now have given their body to their Otsutsuki Host for the short time.";
        KarmMark2.path_icon = "ui/icons/KarmH";
        KarmMark2.remove_status = new string[] { "karma1" };
        AssetManager.status.add(KarmMark2);
        // addTraitToLocalizedLibrary(KarmMark2.locale_id, KarmMark2.locale_description, "Karma Seal (Resurrection)", "After overusing their newly found power they've been implanted with too much DNA, thus they've lost control and now have given their body to their Otsutsuki Host for the short time.");

        StatusAsset KarmMark = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "karma1",
            locale_id = "status_karma",
            duration = 50f
        };
        KarmMark.base_stats["health"] = 425f;
        KarmMark.base_stats["scale"] = 0.015f;
        KarmMark.base_stats["speed"] = 28;
        KarmMark.base_stats["range"] = 0.35f;
        KarmMark.base_stats["armor"] = 16;
        KarmMark.base_stats["damage"] = 50;
        KarmMark.locale_description = "The strange markings on their hand has spreaded throughout their body, feeding them power, but at what cost?..";
        KarmMark.path_icon = "ui/icons/Karm";
        AssetManager.status.add(KarmMark);
        // addTraitToLocalizedLibrary(KarmMark.locale_id, KarmMark.locale_description, "Karma Seal", "The strange markings on their hand has spreaded throughout their body, feeding them power, but at what cost?..");

        StatusAsset Curse3 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Curse3",
            locale_id = "status_curseMark3",
            duration = 45f
        };
        Curse3.base_stats["health"] = 425f;
        Curse3.base_stats["scale"] = 0.015f;
        Curse3.base_stats["speed"] = 22;
        Curse3.base_stats["range"] = 0.30f;
        Curse3.base_stats["armor"] = 18;
        Curse3.base_stats["damage"] = 35;
        Curse3.locale_description = "The thrid and final stage, the've now become used to the pain and no longer feel it..";
        Curse3.path_icon = "ui/icons/Curse3";
        Curse3.remove_status = new string[] { "Curse2", "Curse1" };
        AssetManager.status.add(Curse3);
        // addTraitToLocalizedLibrary(Curse3.locale_id, Curse3.locale_description, "Activated Curse Mark III", "The thrid and final stage, the've now become used to the pain and no longer feel it..");

        StatusAsset Curse2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Curse2",
            locale_id = "status_curseMark2",
            duration = 45f
        };
        Curse2.base_stats["health"] = 275f;
        Curse2.base_stats["speed"] = 12;
        Curse2.base_stats["range"] = 0.15f;
        Curse2.base_stats["armor"] = 10;
        Curse2.base_stats["damage"] = 20;
        Curse2.locale_description = "Stage two, more painful but I guess it gives them more power. So its worth it..";
        Curse2.path_icon = "ui/icons/Curse2";
        Curse2.remove_status = new string[] { "Curse1" };
        AssetManager.status.add(Curse2);
        // addTraitToLocalizedLibrary(Curse2.locale_id, Curse2.locale_description, "Activated Curse Mark II", "Stage two, more painful but I guess it gives them more power. So its worth it..");

        StatusAsset Curse1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Curse1",
            locale_id = "status_curseMark1",
            duration = 45f,
            draw_light_area = true,
            draw_light_size = 0.25f
        };
        Curse1.base_stats["health"] = 150f;
        Curse1.base_stats["speed"] = 5;
        Curse1.base_stats["armor"] = 5;
        Curse1.base_stats["damage"] = 10;
        Curse1.base_stats["range"] = 0;
        Curse1.locale_description = "The strange mark on their neck has activated, its painful but feeds them more power while it grows with their anger.";
        Curse1.path_icon = "ui/icons/Curse";
        Curse1.action_interval = 5f;
        AssetManager.status.add(Curse1);
        // addTraitToLocalizedLibrary(Curse1.locale_id, Curse1.locale_description, "Activated Curse Mark I", "The strange mark on their neck has activated, its painful but feeds them more power while it grows with their anger.");

        StatusAsset LastStance = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Kuramas Death",
            locale_id = "status_baryon",
            duration = 90f,
            draw_light_area = true,
            draw_light_size = 0.25f
        };
        LastStance.base_stats["health"] = 2950f;
        LastStance.base_stats["scale"] = 0.01f;
        LastStance.base_stats["attack_speed"] = 225;
        LastStance.base_stats["speed"] = 80;
        LastStance.base_stats["range"] = 0.75f;
        LastStance.base_stats["damage"] = 120;
        LastStance.base_stats["armor"] = 50;
        LastStance.locale_description = "Kurama's last stance. He's trying his best to protect them by offering up his life.";
        LastStance.path_icon = "ui/icons/Baryon";
        LastStance.action = new WorldAction(BeastsDeath);
        LastStance.remove_status = new string[] { "Six Path Sage" };
        LastStance.action_interval = 89.5f;
        AssetManager.status.add(LastStance);
        // addTraitToLocalizedLibrary(LastStance.locale_id, LastStance.locale_description, "Baryon Mode", "Kurama's last stance. He's trying his best to protect them by offering up his life.");

        StatusAsset InnateJinchuriki = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Innate Jinchuriki",
            texture = "baryonAura",
            animated = true,
            animation_speed = 0.1f,
            path_icon = "effects/Control",
            locale_id = "status_innateJinchuriki",
            locale_description = "They have learned to take some chakra of their beast to enhance their strength.",
            duration = 60f
        };
        InnateJinchuriki.base_stats["damage"] = 20f; 
        InnateJinchuriki.base_stats["armor"] = 10; 
        InnateJinchuriki.base_stats["attack_speed"] = 40f;
        InnateJinchuriki.base_stats["health"] = 250;
        InnateJinchuriki.base_stats["speed"] = 10f;
        InnateJinchuriki.base_stats["tailed_beast_chakra"] = 500f;
        AssetManager.status.add(InnateJinchuriki);

        /*StatusAsset ControlS1 = new StatusAsset();
        ControlS1.id = "One Tailed Cloak";
        ControlS1.texture = "tchakraAura";
        ControlS1.animated = true;
        ControlS1.animation_speed = 0.1f;
        ControlS1.path_icon = "ui/icons/Control1";
        ControlS1.locale_id = "status_oneTailed";
        ControlS1.locale_description = "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra.";
        ControlS1.duration = 60f;
        ControlS1.base_stats["damage"] = 28;
        ControlS1.base_stats["armor"] = 30;
        ControlS1.base_stats["attack_speed"] = 50f;
        ControlS1.base_stats["health"] = 350;
        ControlS1.base_stats["speed"] = 15f;
        ControlS1.base_stats["chakra"] = 30f;   
        AssetManager.status.add(ControlS1);
        // addTraitToLocalizedLibrary(ControlS1.locale_id, ControlS1.locale_description, "One Tailed Cloak", "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra.");


        StatusAsset ControlS2 = new StatusAsset();
        ControlS2.id = "Two Tailed Cloak";
        ControlS2.locale_id = "Two Tailed Cloak";
        ControlS2.texture = "tchakraAura";
        ControlS2.animated = true;
        ControlS2.animation_speed = 0.1f;
        ControlS2.locale_description = "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra..";
        ControlS2.path_icon = "ui/icons/Control1";
        ControlS2.duration = 60f;
        ControlS2.base_stats["damage"] = 36;
        ControlS2.base_stats["armor"] = 35;
        ControlS2.base_stats["attack_speed"] = 60f;
        ControlS2.base_stats["health"] = 450;
        ControlS2.base_stats["speed"] = 20f;
        ControlS2.base_stats["chakra"] = 40f;
        ControlS2.remove_status = new string[] { "One Tailed Cloak" };
        AssetManager.status.add(ControlS2);
        // addTraitToLocalizedLibrary(ControlS2.locale_id, ControlS2.locale_description, "Two Tailed Cloak", "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra..");


        StatusAsset ControlS3 = new StatusAsset();
        ControlS3.id = "Three Tailed Cloak";
        ControlS3.locale_id = "Three Tailed Cloak";
        ControlS3.texture = "tchakraAura";
        ControlS3.animated = true;
        ControlS3.animation_speed = 0.1f;
        ControlS3.locale_description = "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra...";
        ControlS3.path_icon = "ui/icons/Control1";
        ControlS3.duration = 60f;
        ControlS3.base_stats["damage"] = 44;
        ControlS3.base_stats["armor"] = 40;
        ControlS3.base_stats["attack_speed"] = 70f;
        ControlS3.base_stats["health"] = 550;
        ControlS3.base_stats["speed"] = 25f;
        ControlS3.base_stats["chakra"] = 50f;
        ControlS3.remove_status = new string[] { "One Tailed Cloak", "Two Tailed Cloak" };
        AssetManager.status.add(ControlS3);
        // addTraitToLocalizedLibrary(ControlS3.locale_id, ControlS3.locale_description, "Three Tailed Cloak", "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra...");


        StatusAsset ControlS4 = new StatusAsset();
        ControlS4.id = "Four Tailed Cloak";
        ControlS4.locale_id = "Four Tailed Cloak";
        ControlS4.texture = "baryonAura";
        ControlS4.animated = true;
        ControlS4.animation_speed = 0.1f;
        ControlS4.locale_description = "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra....";
        ControlS4.path_icon = "ui/icons/Control1";
        ControlS4.duration = 60f;
        ControlS4.base_stats["damage"] = 52;
        ControlS4.base_stats["armor"] = 45;
        ControlS4.base_stats["attack_speed"] = 80f;
        ControlS4.base_stats["health"] = 650;
        ControlS4.base_stats["speed"] = 30f;
        ControlS4.base_stats["chakra"] = 60;
        AssetManager.status.add(ControlS4);
        // addTraitToLocalizedLibrary(ControlS4.locale_id, ControlS4.locale_description, "Four Tailed Cloak", "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra....");


        StatusAsset ControlS5 = new StatusAsset();
        ControlS5.id = "Five Tailed Cloak";
        ControlS5.locale_id = "Five Tailed Cloak";
        ControlS5.texture = "baryonAura";
        ControlS5.animated = true;
        ControlS5.animation_speed = 0.1f;
        ControlS5.locale_description = "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra.....";
        ControlS5.path_icon = "ui/icons/Control1";
        ControlS5.duration = 60f;
        ControlS5.base_stats["damage"] = 60;
        ControlS5.base_stats["armor"] = 50;
        ControlS5.base_stats["attack_speed"] = 90;
        ControlS5.base_stats["health"] = 750;
        ControlS5.base_stats["speed"] = 30f;
        ControlS5.base_stats["chakra"] = 70;
        AssetManager.status.add(ControlS5);
        // addTraitToLocalizedLibrary(ControlS5.locale_id, ControlS5.locale_description, "Five Tailed Cloak", "After getting a deeper bond they were able to get more chakra from their beast, getting cloaked in their chakra.....");*/


        StatusAsset Control9T1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Link Mode",
            locale_id = "status_linkMode",
            texture = "sixPathsAura",
            animated = true,
            animation_speed = 0.1f,
            locale_description = "You finally learned to completely control the Kyuubi. This ability gives you immense power!",
            path_icon = "ui/icons/AngryBiju",
            duration = 60f
        };
        Control9T1.base_stats["damage"] = 85;
        Control9T1.base_stats["armor"] = 30;
        Control9T1.base_stats["range"] = 0.35f;
        Control9T1.base_stats["attack_speed"] = 95;
        Control9T1.base_stats["health"] = 1000;
        Control9T1.base_stats["speed"] = 30;
        Control9T1.base_stats["chakra"] = 100f;
        Control9T1.action = JinchurikiActions.LinkModeChakraDrain;
        Control9T1.action_interval = 1f;
        AssetManager.status.add(Control9T1);
        // addTraitToLocalizedLibrary(Control9T1.locale_id, Control9T1.locale_description, "Link Mode", "You finally learned to completely control the Kyuubi. This ability gives you immense power!");


        StatusAsset Control9T2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Kyuubi Sage",
            locale_id = "status_kyuubiSage",
            texture = "sixPathsAura",
            animated = true,
            animation_speed = 0.1f,
            locale_description = "After finally bonding with your beast you've truely became one, fusing your flows of chakra and giving you immeasurable power.",
            path_icon = "ui/icons/KyubiS",
            duration = 60f
        };
        Control9T2.base_stats["health"] = 1450;
        Control9T2.base_stats["damage"] = 105;
        Control9T2.base_stats["range"] = 0.50f;
        Control9T2.base_stats["armor"] = 40;
        Control9T2.base_stats["speed"] = 36;
        Control9T2.base_stats["attack_speed"] = 130;
        Control9T2.base_stats["chakra"] = 140f;
        Control9T2.base_stats["sage_chakra"] = 200;
        Control9T2.base_stats["sage_chakra_regen"] = 6f;
        Control9T2.action = JinchurikiActions.SageModeDrain;
        Control9T2.action_interval = 1f;
        AssetManager.status.add(Control9T2);
        // addTraitToLocalizedLibrary(Control9T2.locale_id, Control9T2.locale_description, "Kyuubi Sage", "After finally bonding with your beast you've truely became one, fusing your flows of chakra and giving you immeasurable power.");

        StatusAsset Control9T3 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Six Path Sage",
            locale_id = "status_sixPathsSage",
            texture = "Paths",
            animated = true,
            animation_speed = 0.1f,
            locale_description = "The highest form, you've peaked with your friend.",
            path_icon = "ui/icons/Paths",
            duration = 60f
        };
        Control9T3.base_stats["health"] = 2250;
        Control9T3.base_stats["damage"] = 145;
        Control9T3.base_stats["range"] = 0.75f;
        Control9T3.base_stats["armor"] = 55;
        Control9T3.base_stats["attack_speed"] = 190;
        Control9T3.base_stats["speed"] = 42;
        Control9T3.base_stats["chakra"] = 200;
        Control9T3.base_stats["sage_chakra"] = 300;
        Control9T3.base_stats["sage_chakra_regen"] = 6f;
        Control9T3.action = JinchurikiActions.SageModeDrain;
        Control9T3.action_interval = 1f;
        AssetManager.status.add(Control9T3);
        // addTraitToLocalizedLibrary(Control9T3.locale_id, Control9T3.locale_description, "Six Path Sage Mode", "The highest form, you've peaked with your friend.");

        StatusAsset BrokenS = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "One Tailed",
            locale_id = "status_oneTailed",
            texture = "baryonAura",
            locale_description = "locale_description_brokenS",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage"
        };
        BrokenS.base_stats["damage"] = 20;
        BrokenS.base_stats["armor"] = 5;
        BrokenS.base_stats["health"] = 300;
        BrokenS.base_stats["speed"] = 10;
        BrokenS.base_stats["scale"] = 0.005f;
        BrokenS.base_stats["attack_speed"] = 30;
        BrokenS.base_stats["chakra"] = 30f;
        AssetManager.status.add(BrokenS);
        // addTraitToLocalizedLibrary(BrokenS.locale_id, BrokenS.locale_description, "One Tailed", "This guy has a tail and a strange red aura!");

        StatusAsset BrokenS1 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Two Tailed",
            locale_id = "status_twoTailed",
            texture = "baryonAura",
            locale_description = "brokens1_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage2"
        };
        BrokenS1.base_stats["damage"] = 25;
        BrokenS1.base_stats["armor"] = 10;
        BrokenS1.base_stats["scale"] = 0.005f;
        BrokenS1.base_stats["attack_speed"] = 40;
        BrokenS1.base_stats["health"] = 450;
        BrokenS1.base_stats["speed"] = 15;
        BrokenS1.base_stats["chakra"] = 40;
        AssetManager.status.add(BrokenS1);
        // addTraitToLocalizedLibrary(BrokenS1.locale_id, BrokenS1.locale_description, "Two Tailed", "This guy is two tailed!");

        StatusAsset BrokenS2 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Three Tailed",
            locale_id = "status_threeTailed",
            texture = "baryonAura",
            locale_description = "brokens2_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage3"
        };
        BrokenS2.base_stats["damage"] = 30;
        BrokenS2.base_stats["armor"] = 15;
        BrokenS2.base_stats["scale"] = 0.005f;
        BrokenS2.base_stats["attack_speed"] = 50;
        BrokenS2.base_stats["health"] = 600;
        BrokenS2.base_stats["speed"] = 20;
        BrokenS2.base_stats["chakra"] = 50;
        AssetManager.status.add(BrokenS2);
        // addTraitToLocalizedLibrary(BrokenS2.locale_id, BrokenS2.locale_description, "Three Tailed", "This guy is three tailed!");

        StatusAsset BrokenS3 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Four Tailed",
            locale_id = "status_fourTailed",
            texture = "baryonAura",
            locale_description = "brokens3_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage4"
        };
        BrokenS3.base_stats["damage"] = 35;
        BrokenS3.base_stats["armor"] = 20;
        BrokenS3.base_stats["scale"] = 0.01f;
        BrokenS3.base_stats["attack_speed"] = 60;
        BrokenS3.base_stats["health"] = 750;
        BrokenS3.base_stats["speed"] = 25;
        BrokenS3.base_stats["chakra"] = 60;
        AssetManager.status.add(BrokenS3);
        // addTraitToLocalizedLibrary(BrokenS3.locale_id, BrokenS3.locale_description, "Four Tailed", "A four tailed beast!!!");

        StatusAsset BrokenS4 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Five Tailed",
            locale_id = "status_fiveTailed",
            texture = "baryonAura",
            locale_description = "brokens4_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage5"
        };
        BrokenS4.base_stats["damage"] = 50;
        BrokenS4.base_stats["armor"] = 25;
        BrokenS4.base_stats["scale"] = 0.015f;
        BrokenS4.base_stats["attack_speed"] = 70;
        BrokenS4.base_stats["health"] = 900;
        BrokenS4.base_stats["speed"] = 30;
        BrokenS4.base_stats["chakra"] = 70;
        AssetManager.status.add(BrokenS4);
        // addTraitToLocalizedLibrary(BrokenS4.locale_id, BrokenS4.locale_description, "Five Tailed", "A five tailed beast!!!");

        StatusAsset BrokenS5 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Six Tailed",
            locale_id = "status_sixTailed",
            texture = "baryonAura",
            locale_description = "brokens5_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage6"
        };
        BrokenS5.base_stats["damage"] = 65;
        BrokenS5.base_stats["multiplier_damage"] = 0.6f;
        BrokenS5.base_stats["armor"] = 30;
        BrokenS5.base_stats["scale"] = 0.025f;
        BrokenS5.base_stats["multiplier_attack_speed"] = 70;
        BrokenS5.base_stats["health"] = 1050;
        BrokenS5.base_stats["speed"] = 35;
        BrokenS5.base_stats["chakra"] = 80;
        AssetManager.status.add(BrokenS5);
        // addTraitToLocalizedLibrary(BrokenS5.locale_id, BrokenS5.locale_description, "Six Tailed", "A six tailed beast!!!");

        StatusAsset BrokenS6 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Seven Tailed",
            locale_id = "status_sevenTailed",
            texture = "baryonAura",
            locale_description = "brokens6_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage7"
        };
        BrokenS6.base_stats["damage"] = 80;
        BrokenS6.base_stats["multiplier_damage"] = 0.7f;
        BrokenS6.base_stats["armor"] = 30;
        BrokenS6.base_stats["scale"] = 0.035f;
        BrokenS6.base_stats["multiplier_attack_speed"] = 70;
        BrokenS6.base_stats["health"] = 1200;
        BrokenS6.base_stats["speed"] = 40;
        BrokenS6.base_stats["chakra"] = 90;
        AssetManager.status.add(BrokenS6);
        // addTraitToLocalizedLibrary(BrokenS6.locale_id, BrokenS6.locale_description, "Seven Tailed", "A seven tailed beast!!!");

        StatusAsset BrokenS7 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Eight Tailed",
            locale_id = "status_eightTailed",
            texture = "baryonAura",
            locale_description = "brokens7_locale_description",
            animated = true,
            animation_speed = 0.1f,
            duration = 60f,
            path_icon = "ui/icons/Rampage8"
        };
        BrokenS7.base_stats["damage"] = 100;
        BrokenS7.base_stats["armor"] = 30;
        BrokenS7.base_stats["attack_speed"] = 70;
        BrokenS7.base_stats["scale"] = 0.05f;
        BrokenS7.base_stats["health"] = 1350;
        BrokenS7.base_stats["speed"] = 40;
        // BrokenS7.base_stats["fertility"] = -1000f;
        BrokenS7.base_stats["chakra"] = 100;
        AssetManager.status.add(BrokenS7);
        // addTraitToLocalizedLibrary(BrokenS7.locale_id, BrokenS7.locale_description, "Eight Tailed", "A eight tailed beast!!!");

        StatusAsset BrokenS8 = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Nine Tailed",
            locale_id = "status_nineTailed",
            texture = "baryonAura",
            locale_description = "brokens8_locale_description",
            animated = true,
            animation_speed = 0.1f,
            path_icon = "ui/icons/Rampage9",
            duration = 60f
        };
        BrokenS8.base_stats["damage"] = 130;
        BrokenS8.base_stats["armor"] = 35;
        BrokenS8.base_stats["attack_speed"] = 70;
        BrokenS8.base_stats["scale"] = 0.07f;
        BrokenS8.base_stats["health"] = 1500;
        BrokenS8.base_stats["speed"] = 40;
        // BrokenS8.base_stats["fertility"] = -1000f;
        BrokenS8.base_stats["chakra"] = 110;
        AssetManager.status.add(BrokenS8);
        // addTraitToLocalizedLibrary(BrokenS8.locale_id, BrokenS8.locale_description, "Nine Tailed", "A nine tailed beast!!!");

        StatusAsset FocusPoint = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Focus Point",
            locale_id = "status_focus_point",
            locale_description = "status_focus_point_desc",
            duration = 5f,
            animated = true,
            texture = "Focuspoint",
            path_icon = "ui/icons/3T"
        };
        FocusPoint.action_get_hit = JinchurikiActions.FocusPointDeflect;
        AssetManager.status.add(FocusPoint);

        StatusAsset ImmeasurableStatus = new StatusAsset
        {
            base_stats = new BaseStats(),
            id = "Immeasurable",
            locale_id = "status_immeasurable",
            locale_description = "status_immeasurable_desc",
            path_icon = "ui/icons/ChakraNum",
            duration = 1.1f,
            action_interval = 0.1f,
            animated = true,
            texture = "Immeasurable",
            animation_speed = 0.0417f,
            action = JinchurikiActions.ImmeasurableStatusAction
        };
        AssetManager.status.add(ImmeasurableStatus);

        foreach (var s in new StatusAsset[] {
            Palmtech2, Palmtech1, ButterflyWings, Lightningcloak, Tens,
            madarasusanoo2, madarasusanoo1, ChainedGiver, Chained,
            WoodSheild, SharGenjutsu, BurningA, Gate8,
            ChidoriAtk, RasenganActive, InnateJinchuriki,
            Control9T1, Control9T2, Control9T3,
            BrokenS, BrokenS1, BrokenS2, BrokenS3, BrokenS4,
            BrokenS5, BrokenS6, BrokenS7, BrokenS8, FocusPoint,
            ImmeasurableStatus
        })
        {
            if (s.animated && !string.IsNullOrEmpty(s.texture))
            {
                s.need_visual_render = true;
                s.material = material;
                s.sprite_list = SpriteTextureLoader.getSpriteList($"effects/{s.texture}", false) ?? Array.Empty<Sprite>();
            }
        }
    }

    
    public static bool BeastsDeath(BaseSimObject pTarget, WorldTile pTile = null) {
        return true;
    }

    public static bool GateHeal (BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        a.restoreHealth(100);
        return true;
    }


    public static void addTraitToLocalizedLibrary(string locale_id, string desc, string id, string locale_description)
    {
        if (!LocalizedTextManager.instance.contains(locale_id))
        {
            LocalizedTextManager.add(locale_id, id);
            LocalizedTextManager.add(desc, locale_description);
        }
    }
}
