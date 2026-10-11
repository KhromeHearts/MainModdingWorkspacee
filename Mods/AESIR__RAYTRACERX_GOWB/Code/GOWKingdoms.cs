using System;
using System.Linq;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NeoModLoader.services;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using HarmonyLib;

namespace GOWB
{
    public static class GOWKingdoms
    {
        public static void Init()
        {
            bool createdAesir = false;
            bool createdVanir = false;
            bool createdJotnar = false;

            KingdomAsset AesirKingdom = AssetManager.kingdoms.get("Aesir");
            if (AesirKingdom == null)
            {
                AesirKingdom = new KingdomAsset();
                AesirKingdom.id = "Aesir";
                AesirKingdom.mobs = true;
                AesirKingdom.addTag("Aesir");
                AesirKingdom.addFriendlyTag("Aesir");
                AesirKingdom.addFriendlyTag("dragons");
                AesirKingdom.addFriendlyTag("dwarf");
                AesirKingdom.addFriendlyTag("undead");
                AesirKingdom.addEnemyTag("demons");
                AesirKingdom.default_kingdom_color = new ColorAsset("rgb(236, 0, 236)");
                AssetManager.kingdoms.add(AesirKingdom);
                createdAesir = true;
            }

            KingdomAsset VanirKingdom = AssetManager.kingdoms.get("Vanir");
            if (VanirKingdom == null)
            {
                VanirKingdom = new KingdomAsset();
                VanirKingdom.id = "Vanir";
                VanirKingdom.mobs = true;
                VanirKingdom.addTag("Vanir");
                VanirKingdom.addTag("good");
                VanirKingdom.addFriendlyTag("Vanir");
                VanirKingdom.addFriendlyTag("druid");
                VanirKingdom.addFriendlyTag("animal");
                VanirKingdom.addFriendlyTag("good");
                VanirKingdom.addFriendlyTag("nature_creature");
                VanirKingdom.addFriendlyTag("human");
                VanirKingdom.addEnemyTag("Aesir");
                VanirKingdom.default_kingdom_color = new ColorAsset("rgb(135, 0, 189)");
                AssetManager.kingdoms.add(VanirKingdom);
                createdVanir = true;
            }

            KingdomAsset JotnarKingdom = AssetManager.kingdoms.get("Jotnar");
            if (JotnarKingdom == null)
            {
                JotnarKingdom = new KingdomAsset();
                JotnarKingdom.id = "Jotnar";
                JotnarKingdom.mobs = true;
                JotnarKingdom.addTag("Jotnar");
                JotnarKingdom.addTag("good");
                JotnarKingdom.addFriendlyTag("Jotnar");
                JotnarKingdom.addFriendlyTag("Vanir");
                JotnarKingdom.addFriendlyTag("druid");
                JotnarKingdom.addFriendlyTag("animal");
                JotnarKingdom.addFriendlyTag("good");
                JotnarKingdom.addFriendlyTag("nature_creature");
                JotnarKingdom.addFriendlyTag("human");
                JotnarKingdom.addEnemyTag("Aesir");
                JotnarKingdom.default_kingdom_color = new ColorAsset("rgb(0, 82, 189)");
                AssetManager.kingdoms.add(JotnarKingdom);
                createdJotnar = true;
            }

            if (createdAesir && createdJotnar && createdVanir != null && World.world.kingdoms_wild != null)
            {
                World.world.kingdoms_wild.newWildKingdom(AesirKingdom);
                World.world.kingdoms_wild.newWildKingdom(VanirKingdom);
                World.world.kingdoms_wild.newWildKingdom(JotnarKingdom);
            }

            //kingdom relationship adjustment
            AesirKingdom.addEnemyTag("Vanir");
            AesirKingdom.addEnemyTag("Jotnar");
            VanirKingdom.addFriendlyTag("Jotnar");

            //Vanilla kingdom relationship adjustment
            KingdomAsset SKdwarf = AssetManager.kingdoms.get("dwarf");
            if (SKdwarf != null)
            {
                SKdwarf.addFriendlyTag("Aesir");
            }

            KingdomAsset SKundead = AssetManager.kingdoms.get("undead");
            if (SKundead != null)
            {
                SKundead.addFriendlyTag("Aesir");
            }
        }

    }
}

