using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NeoModLoader.api;

/*-------------------
This file will only implement its changes if config_rebalanceVanillaWeapons is enabled, seperated from items.cs for easier adjustments and to keep code clean
Key: [weapon characteristic focus], (base + change%), 🠝🠝🠝/🠟🠟🠟 Extreme, 🠝🠝/🠟🠟 Intense, 🠝/🠟 Normal
DPTr: An attack speed considerate damage range [low/high]. Formulas: DPTrHigh(d [damage],a [attack speed%]) = d(1+a), DPTrLow(r [damage range]) = DPTrHigh * r
Damage Range stat: Ex. "damage_range" = 0.8f, this means that the lowest damage a unit can deal on a non-crit attack is 80% of their damage stat
Material Constants: This are stats that stay the always *added* across item types for materials, currently this includes mana, stamina, speed, and attack speed% (see these at the bottom of this file)
Notice: Some stats are listed to change here but may be the same as their base game values. If you want a list of the normal stats of items see this wiki page: https://the-official-worldbox-wiki.fandom.com/wiki/Equipment
-------------------*/

namespace avbsexpandedarmory{
    class ItemRebalance : MonoBehaviour{
        public static void init(){
			//Swords [base]---------------------------------------------------------------------------------------------------------
			//notes: damage range of 80%, acts as the base for all basic melee types
			//changes: every tier above silver gets +1 damage (to better incorporate the mod's gold tier)
			EquipmentAsset woodsword = AssetManager.items.get("sword_wood");
			woodsword.base_stats["damage"] = 1; //DPTr = 0.8/1
			EquipmentAsset stonesword = AssetManager.items.get("sword_stone");
			stonesword.base_stats["damage"] = 3; //DPTr = 2.4/3
			EquipmentAsset coppersword = AssetManager.items.get("sword_copper");
			coppersword.base_stats["damage"] = 3; //DPTr = 2.4/3
			EquipmentAsset bronzesword = AssetManager.items.get("sword_bronze");
			bronzesword.base_stats["damage"] = 4; //DPTr = 3.2/4
			EquipmentAsset silversword = AssetManager.items.get("sword_silver");
			silversword.base_stats["damage"] = 5; //DPTr = 4/5
			EquipmentAsset goldsword = AssetManager.items.get("gold_sword");
			goldsword.base_stats["damage"] = 6; //DPTr = 4.8/6
			EquipmentAsset ironsword = AssetManager.items.get("sword_iron");
			ironsword.base_stats["damage"] = 7f; //DPTr = 5.6/7
			EquipmentAsset steelsword = AssetManager.items.get("sword_steel");
			steelsword.base_stats["damage"] = 8f; //DPTr = 6.4/8
			EquipmentAsset mythrilsword = AssetManager.items.get("sword_mythril");
			mythrilsword.base_stats["damage"] = 9f; //DPTr = 7.2/9
			EquipmentAsset adamantinesword = AssetManager.items.get("sword_adamantine");
			adamantinesword.base_stats["damage"] = 11f; //DPTr = 8.8/11

			//Axes [🠝🠝damage, 🠝crit, 🠟🠟attack speed, 🠟damage range]
			//notes: Wihtout rebalance axes have the same stats as swords, except for having a damage range of 60% (makes them objectively worse)
			//changes: damage buff, critial chance buff, attack speed% nerf
			EquipmentAsset woodaxe = AssetManager.items.get("axe_wood");
			woodaxe.base_stats["damage"] = 2f; //1 +100%
			woodaxe.base_stats["multiplier_attack_speed"] = -0.45f; //DPTr = 0.66/1.1
			woodaxe.base_stats["critical_chance"] = 0.2f;
			EquipmentAsset stoneaxe = AssetManager.items.get("axe_stone");
			stoneaxe.base_stats["damage"] = 4f; //3 +33%
			stoneaxe.base_stats["multiplier_attack_speed"] = -0.45f; //DPTr = 1.32/2.2
			stoneaxe.base_stats["critical_chance"] = 0.2f;
			EquipmentAsset copperaxe = AssetManager.items.get("axe_copper");
			copperaxe.base_stats["damage"] = 4f; //3 +33%
			copperaxe.base_stats["multiplier_attack_speed"] = -0.4f; //DPTr = 1.44/2.4
			copperaxe.base_stats["critical_chance"] = 0.2f;
			EquipmentAsset bronzeaxe = AssetManager.items.get("axe_bronze");
			bronzeaxe.base_stats["damage"] = 6f; //4 +50%
			bronzeaxe.base_stats["multiplier_attack_speed"] = -0.4f; //DPTr = 2.16/3.6
			bronzeaxe.base_stats["critical_chance"] = 0.25f;
			EquipmentAsset silveraxe = AssetManager.items.get("axe_silver");
			silveraxe.base_stats["damage"] = 7f; //5 +40%
			silveraxe.base_stats["critical_chance"] = 0.25f;
			silveraxe.base_stats["multiplier_attack_speed"] = -0.4f; //DPTr = 2.52/4.2
			EquipmentAsset goldaxe = AssetManager.items.get("gold_axe");
			goldaxe.base_stats["damage"] = 9f; //6 +50%
			goldaxe.base_stats["multiplier_attack_speed"] = -0.4f; //DPTr = 3.24/5.4
			goldaxe.base_stats["critical_chance"] = 0.25f;
			EquipmentAsset ironaxe = AssetManager.items.get("axe_iron");
			ironaxe.base_stats["damage"] = 10f; //7 +43%
			ironaxe.base_stats["multiplier_attack_speed"] = -0.35f; //DPTr = 3.9/6.5
			ironaxe.base_stats["critical_chance"] = 0.3f;
			EquipmentAsset steelaxe = AssetManager.items.get("axe_steel");
			steelaxe.base_stats["damage"] = 11f; //8 +37.5%
			steelaxe.base_stats["multiplier_attack_speed"] = -0.3f; //DPTr = 4.62/7.7
			steelaxe.base_stats["critical_chance"] = 0.3f;
			EquipmentAsset mythrilaxe = AssetManager.items.get("axe_mythril");
			mythrilaxe.base_stats["damage"] = 13f; //9 +44%
			mythrilaxe.base_stats["multiplier_attack_speed"] = -0.3f; //DPTr = 5.46/9.1
			mythrilaxe.base_stats["critical_chance"] = 0.3f;
			EquipmentAsset adamantineaxe = AssetManager.items.get("axe_adamantine");
			adamantineaxe.base_stats["damage"] = 15f; //11 +36%
			adamantineaxe.base_stats["multiplier_attack_speed"] = -0.25f; //DPTr = 6.75/11.25
			adamantineaxe.base_stats["critical_chance"] = 0.35f;

			//Hammers [🠝damage, 🠝target, +stun, 🠟attack speed, 🠟🠟🠟damage range]
			//notes: hammers have a damage range of 10% but get +2 targets and have the stun modifier
			//changes: damage buff, attack speed% nerf
			EquipmentAsset woodhammer = AssetManager.items.get("hammer_wood");
			woodhammer.base_stats["damage"] = 1f; //1 +0%
			woodhammer.base_stats["multiplier_attack_speed"] = -0.1f; //DPTr = 0.09/0.9
			EquipmentAsset stonehammer = AssetManager.items.get("hammer_stone");
			stonehammer.base_stats["damage"] = 4f; //3 +33%
			stonehammer.base_stats["multiplier_attack_speed"] = -0.2f; //DPTr = 0.32/3.2
			EquipmentAsset copperhammer = AssetManager.items.get("hammer_copper");
			copperhammer.base_stats["damage"] = 4f; //3 +33%
			copperhammer.base_stats["multiplier_attack_speed"] = -0.15f; //DPTr = 0.34/3.4
			EquipmentAsset bronzehammer = AssetManager.items.get("hammer_bronze");
			bronzehammer.base_stats["damage"] = 5f; //4 +25%
			bronzehammer.base_stats["multiplier_attack_speed"] = -0.15f; //DPTr = 0.425/4.25
			EquipmentAsset silverhammer = AssetManager.items.get("hammer_silver");
			silverhammer.base_stats["damage"] = 6f; //5 +20%
			silverhammer.base_stats["multiplier_attack_speed"] = -0.15f; //DPTr = 0.51/5.1
			EquipmentAsset goldhammer = AssetManager.items.get("gold_hammer");
			goldhammer.base_stats["damage"] = 8f; //6 +25%
			goldhammer.base_stats["multiplier_attack_speed"] = -0.15f; //DPTr = 0.68/6.8
			EquipmentAsset ironhammer = AssetManager.items.get("hammer_iron");
			ironhammer.base_stats["damage"] = 9f; //7 +29%
			ironhammer.base_stats["multiplier_attack_speed"] = -0.1f; //DPTr = 0.81/8.1
			EquipmentAsset steelhammer = AssetManager.items.get("hammer_steel");
			steelhammer.base_stats["damage"] = 10f; //8 +25%
			steelhammer.base_stats["multiplier_attack_speed"] = -0.1f; //DPTr = 0.9/9
			EquipmentAsset mythrilhammer = AssetManager.items.get("hammer_mythril");
			mythrilhammer.base_stats["damage"] = 12f; //9 +33%
			mythrilhammer.base_stats["multiplier_attack_speed"] = -0.1f; //DPTr = 1.08/10.8
			EquipmentAsset adamantinehammer = AssetManager.items.get("hammer_adamantine");
			adamantinehammer.base_stats["damage"] = 14f; //11 +27%
			adamantinehammer.base_stats["multiplier_attack_speed"] = -0.05f; //DPTr = 1.33/13.3

			//Spears [/range, 🠟damage, 🠟crit, 🠟damage range]
			//notes: spears have a damage range of 70% and normally all give +1 range
			//changes: range gradualized, damage nerf, critical chance nerf
			EquipmentAsset woodspear = AssetManager.items.get("spear_wood");
			woodspear.base_stats["damage"] = 1f; //1 +0% //DPTr = 0.7/1
			woodspear.base_stats["critical_chance"] = -0.2f;
			woodspear.base_stats["range"] = 0.75f;
			EquipmentAsset stonespear = AssetManager.items.get("spear_stone");
			stonespear.base_stats["damage"] = 2f; //3 -33% //DPTr = 1.4/2
			stonespear.base_stats["critical_chance"] = -0.15f;
			stonespear.base_stats["range"] = 0.8f;
			EquipmentAsset copperspear = AssetManager.items.get("spear_copper");
			copperspear.base_stats["damage"] = 2f; //3 -33% //DPTr = 1.4/2
			copperspear.base_stats["critical_chance"] = -0.1f;
			copperspear.base_stats["range"] = 0.85f;
			EquipmentAsset bronzespear = AssetManager.items.get("spear_bronze");
			bronzespear.base_stats["damage"] = 3f; //4 -25% //DPTr = 2.1/3
			bronzespear.base_stats["critical_chance"] = -0.1f;
			bronzespear.base_stats["range"] = 0.9f;
			EquipmentAsset silverspear = AssetManager.items.get("spear_silver");
			silverspear.base_stats["damage"] = 4f; //5 -20% //DPTr = 2.8/4
			silverspear.base_stats["critical_chance"] = -0.1f;
			silverspear.base_stats["range"] = 0.95f;
			EquipmentAsset goldspear = AssetManager.items.get("gold_spear");
			goldspear.base_stats["damage"] = 5f; //6 -17% //DPTr = 3.5/5
			goldspear.base_stats["critical_chance"] = -0.1f;
			goldspear.base_stats["range"] = 1f;
			EquipmentAsset ironspear = AssetManager.items.get("spear_iron");
			ironspear.base_stats["damage"] = 5f; //7 -29% //DPTr = 3.5/5
			ironspear.base_stats["critical_chance"] = -0.1f;
			ironspear.base_stats["range"] = 1.1f;
			EquipmentAsset steelspear = AssetManager.items.get("spear_steel");
			steelspear.base_stats["damage"] = 6f; //8 -25% //DPTr = 4.2/6
			steelspear.base_stats["critical_chance"] = -0.1f;
			steelspear.base_stats["range"] = 1.15f;
			EquipmentAsset mythrilspear = AssetManager.items.get("spear_mythril");
			mythrilspear.base_stats["damage"] = 7f; //9 -22% //DPTr = 4.9/7
			mythrilspear.base_stats["critical_chance"] = -0.05f;
			mythrilspear.base_stats["range"] = 1.2f;
			EquipmentAsset adamantinespear = AssetManager.items.get("spear_adamantine");
			adamantinespear.base_stats["damage"] = 9f; //11 +18% //DPTr = 6.3/9
			adamantinespear.base_stats["critical_chance"] = -0.05f;
			adamantinespear.base_stats["range"] = 1.25f;

			//Bludgeons [🠝🠝🠝damage, +stun, 🠟🠟attack speed, 🠟🠟critical chance, 🠟🠟damage range]
			//notes: bludgeons have a damage range of 5%
			//changes: every tier above silver gets +3 damage (to better incorporate the mod's gold tier), gold attack speed% nerf
			EquipmentAsset woodbludgeon = AssetManager.items.get("wood_bludgeon");
			woodbludgeon.base_stats["damage"] = 3f; //1 +300% //DPTr = 0.7/1
			woodbludgeon.base_stats["multiplier_attack_speed"] = -0.4f;
			EquipmentAsset stonebludgeon = AssetManager.items.get("stone_bludgeon");
			stonebludgeon.base_stats["damage"] = 9f; //3 +300% //DPTr = 1.4/2
			stonebludgeon.base_stats["multiplier_attack_speed"] = -0.4f;
			EquipmentAsset copperbludgeon = AssetManager.items.get("copper_bludgeon");
			copperbludgeon.base_stats["damage"] = 9f; //3 +300% //DPTr = 1.4/2
			copperbludgeon.base_stats["multiplier_attack_speed"] = -0.35f;
			EquipmentAsset bronzebludgeon = AssetManager.items.get("bronze_bludgeon");
			bronzebludgeon.base_stats["damage"] = 12f; //4 +300% //DPTr = 2.1/3
			bronzebludgeon.base_stats["multiplier_attack_speed"] = -0.35f;
			EquipmentAsset silverbludgeon = AssetManager.items.get("silver_bludgeon");
			silverbludgeon.base_stats["damage"] = 15f; //5 +300% //DPTr = 2.8/4
			silverbludgeon.base_stats["multiplier_attack_speed"] = -0.35f;
			EquipmentAsset goldbludgeon = AssetManager.items.get("gold_bludgeon");
			goldbludgeon.base_stats["damage"] = 18f; //6 +300% //DPTr = 3.5/5
			goldbludgeon.base_stats["multiplier_attack_speed"] = -0.35f; //changed from -0.30f
			EquipmentAsset ironbludgeon = AssetManager.items.get("iron_bludgeon");
			ironbludgeon.base_stats["damage"] = 21f; //7 +300% //DPTr = 3.5/5
			ironbludgeon.base_stats["multiplier_attack_speed"] = -0.3f;
			EquipmentAsset steelbludgeon = AssetManager.items.get("steel_bludgeon");
			steelbludgeon.base_stats["damage"] = 24f; //8 +300% //DPTr = 4.2/6
			steelbludgeon.base_stats["multiplier_attack_speed"] = -0.3f;
			EquipmentAsset mythrilbludgeon = AssetManager.items.get("mythril_bludgeon");
			mythrilbludgeon.base_stats["damage"] = 27f; //9 +300% //DPTr = 4.9/7
			mythrilbludgeon.base_stats["multiplier_attack_speed"] = -0.25f;
			EquipmentAsset adamantinebludgeon = AssetManager.items.get("adamantine_bludgeon");
			adamantinebludgeon.base_stats["damage"] = 33f; //11 +300% //DPTr = 6.3/9
			adamantinebludgeon.base_stats["multiplier_attack_speed"] = -0.2f;

			//Halberds [🠝🠝damage, 🠝🠝critical chance, 🠟🠟attack speed, 🠟🠟damage range]
			//notes: halberds have a damage range of 55%
			//changes: range gradualized
			EquipmentAsset woodhalberd = AssetManager.items.get("wood_halberd");
			woodhalberd.base_stats["damage"] = 2f; //1 +100% //DPTr = 0.715/1.3
            woodhalberd.base_stats["multiplier_attack_speed"] = -0.35f;
			woodhalberd.base_stats["range"] = 0.75f;
			EquipmentAsset stonehalberd = AssetManager.items.get("stone_halberd");
			stonehalberd.base_stats["damage"] = 3f; //3 //DPTr = 1.07/1.95
            stonehalberd.base_stats["multiplier_attack_speed"] = -0.35f;
			stonehalberd.base_stats["range"] = 0.8f;
			EquipmentAsset copperhalberd = AssetManager.items.get("copper_halberd");
			copperhalberd.base_stats["damage"] = 3f; //3 //DPTr = 1.16/2.1
            copperhalberd.base_stats["multiplier_attack_speed"] = -0.3f;
			copperhalberd.base_stats["range"] = 0.85f;
			EquipmentAsset bronzehalberd = AssetManager.items.get("bronze_halberd");
			bronzehalberd.base_stats["damage"] = 4f; //4 //DPTr = 1.54/2.8
            bronzehalberd.base_stats["multiplier_attack_speed"] = -0.3f;
			bronzehalberd.base_stats["range"] = 0.9f;
			EquipmentAsset silverhalberd = AssetManager.items.get("silver_halberd");
			silverhalberd.base_stats["damage"] = 5f; //5 //DPTr = 2.06/3.75
            silverhalberd.base_stats["multiplier_attack_speed"] = -0.25f;
			silverhalberd.base_stats["range"] = 0.95f;
			EquipmentAsset goldhalberd = AssetManager.items.get("gold_halberd");
			goldhalberd.base_stats["damage"] = 6f; //6 //DPTr = 2.64/4.8
            goldhalberd.base_stats["multiplier_attack_speed"] = -0.2f;
			goldhalberd.base_stats["range"] = 1f;
			EquipmentAsset ironhalberd = AssetManager.items.get("iron_halberd");
			ironhalberd.base_stats["damage"] = 7f; //7 //DPTr = 3.08/5.6
            ironhalberd.base_stats["multiplier_attack_speed"] = -0.2f;
			ironhalberd.base_stats["range"] = 1.05f;
			EquipmentAsset steelhalberd = AssetManager.items.get("steel_halberd");
			steelhalberd.base_stats["damage"] = 8f; //8 //DPTr = 3.52/6.4
            steelhalberd.base_stats["multiplier_attack_speed"] = -0.2f;
			steelhalberd.base_stats["range"] = 1.1f;
			EquipmentAsset mythrilhalberd = AssetManager.items.get("mythril_halberd");
			mythrilhalberd.base_stats["damage"] = 9f; //9 //DPTr = 4.21/7.65
            mythrilhalberd.base_stats["multiplier_attack_speed"] = -0.15f;
			mythrilhalberd.base_stats["range"] = 1.15f;
			EquipmentAsset adamantinehalberd = AssetManager.items.get("adamantine_halberd");
			adamantinehalberd.base_stats["damage"] = 11f; //11 //DPTr = 5.14/9.35
            adamantinehalberd.base_stats["multiplier_attack_speed"] = -0.15f;
			adamantinehalberd.base_stats["range"] = 1.2f;

			//Daggers [🠝🠝critical chance, 🠝🠝attack speed, 🠝damage range, 🠟🠟damage, 🠟🠟range]
			//notes: daggers have a damage range of 85%
			//changes: none
			EquipmentAsset wooddagger = AssetManager.items.get("wood_halberd");
			wooddagger.base_stats["damage"] = 1f;
            wooddagger.base_stats["multiplier_attack_speed"] = 0.1f;
			wooddagger.base_stats["range"] = -0.4f;
			EquipmentAsset stonedagger = AssetManager.items.get("stone_halberd");
			stonedagger.base_stats["damage"] = 1f;
            stonedagger.base_stats["multiplier_attack_speed"] = 0.15f;
			stonedagger.base_stats["range"] = -0.35f;
			EquipmentAsset copperdagger = AssetManager.items.get("copper_halberd");
			copperdagger.base_stats["damage"] = 2f;
            copperdagger.base_stats["multiplier_attack_speed"] = 0.15f;
			copperdagger.base_stats["range"] = -0.35f;
			EquipmentAsset bronzedagger = AssetManager.items.get("bronze_halberd");
			bronzedagger.base_stats["damage"] = 2f;
            bronzedagger.base_stats["multiplier_attack_speed"] = 0.2f;
			bronzedagger.base_stats["range"] = -0.3f;
			EquipmentAsset silverdagger = AssetManager.items.get("silver_halberd");
			silverdagger.base_stats["damage"] = 3f;
            silverdagger.base_stats["multiplier_attack_speed"] = 0.2f;
			silverdagger.base_stats["range"] = -0.3f;
			EquipmentAsset golddagger = AssetManager.items.get("gold_halberd");
			golddagger.base_stats["damage"] = 3f;
            golddagger.base_stats["multiplier_attack_speed"] = 0.25f;
			golddagger.base_stats["range"] = -0.25f;
			EquipmentAsset irondagger = AssetManager.items.get("iron_halberd");
			irondagger.base_stats["damage"] = 4f;
            irondagger.base_stats["multiplier_attack_speed"] = 0.25f;
			irondagger.base_stats["range"] = -0.25f;
			EquipmentAsset steeldagger = AssetManager.items.get("steel_halberd");
			steeldagger.base_stats["damage"] = 4f;
            steeldagger.base_stats["multiplier_attack_speed"] = 0.3f;
			steeldagger.base_stats["range"] = -0.2f;
			EquipmentAsset mythrildagger = AssetManager.items.get("mythril_halberd");
			mythrildagger.base_stats["damage"] = 5f; 
            mythrildagger.base_stats["multiplier_attack_speed"] = 0.3f;
			mythrildagger.base_stats["range"] = -0.2f;
			EquipmentAsset adamantinedagger = AssetManager.items.get("adamantine_halberd");
			adamantinedagger.base_stats["damage"] = 5f;
            adamantinedagger.base_stats["multiplier_attack_speed"] = 0.35f;
			adamantinedagger.base_stats["range"] = -0.15f;

			//Bows [standard]---------------------------------------------------------------------------------------------------------
			//notes: bows have a damage range of 60%, range hasn't been changed but was included because it is significant. All bows also get have +10% crit and +50% crit damage
			//summary: Made bows slightly weaker than swords, farther nerfs are not needed as the low accurarcy stat makes up for the bow's advantages (though base game accuracy has been slightly increased, normally = 1)
			//changes: accuracy gradualized, damage nerf
			EquipmentAsset woodbow = AssetManager.items.get("bow_wood");
			woodbow.base_stats["damage"] = 1f; //DPTr = 0.6/1
			woodbow.base_stats["accuracy"] = 0f;
			woodbow.base_stats["range"] = 6f;
			EquipmentAsset copperbow = AssetManager.items.get("bow_copper");
			copperbow.base_stats["damage"] = 2f; //DPTr = 1.2/2
			copperbow.base_stats["accuracy"] = 0.05f;
			copperbow.base_stats["range"] = 6f;
			EquipmentAsset bronzebow = AssetManager.items.get("bow_bronze");
			bronzebow.base_stats["damage"] = 3f; //DPTr = 1.8/3
			bronzebow.base_stats["accuracy"] = 0.1f;
			bronzebow.base_stats["range"] = 7f;
			EquipmentAsset silverbow = AssetManager.items.get("bow_silver");
			silverbow.base_stats["damage"] = 4f; //DPTr = 2.4/4
			silverbow.base_stats["accuracy"] = 0.1f;
			silverbow.base_stats["range"] = 8f;
			EquipmentAsset goldbow = AssetManager.items.get("gold_bow");
			goldbow.base_stats["damage"] = 5f; //DPTr = 3/5
			goldbow.base_stats["accuracy"] = 0.15f;
			goldbow.base_stats["range"] = 8f;
			EquipmentAsset ironbow = AssetManager.items.get("bow_iron");
			ironbow.base_stats["damage"] = 6f; //DPTr = 3.6/6
			ironbow.base_stats["accuracy"] = 0.2f;
			ironbow.base_stats["range"] = 9f;
			EquipmentAsset steelbow = AssetManager.items.get("bow_steel");
			steelbow.base_stats["damage"] = 7f; //DPTr = 4.2/7
			steelbow.base_stats["accuracy"] = 0.2f;
			steelbow.base_stats["range"] = 10f;
			EquipmentAsset mythrilbow = AssetManager.items.get("bow_mythril");
			mythrilbow.base_stats["damage"] = 8f; //DPTr = 4.8/8
			mythrilbow.base_stats["accuracy"] = 0.2f;
			mythrilbow.base_stats["range"] = 11f;
			EquipmentAsset adamantinebow = AssetManager.items.get("bow_adamantine");
			adamantinebow.base_stats["damage"] = 10f; //DPTr = 6/10
			adamantinebow.base_stats["accuracy"] = 0.25f;
			adamantinebow.base_stats["range"] = 12f;

			//Crossbows [🠝🠝damage, 🠝accuracy, 🠟🠟range, 🠟🠟attack speed]
			//notes: crossbows also have a damage range of 60% and recieve +10% crit and +50% crit damage like bows
			//changes: damage nerf to early mid tiers
			EquipmentAsset woodcrossbow = AssetManager.items.get("wood_crossbow");
			woodcrossbow.base_stats["damage"] = 2f; //1 +100%
			woodcrossbow.base_stats["multiplier_attack_speed"] = -0.6f; //DPTr = 0.48/0.8
			woodcrossbow.base_stats["accuracy"] = 0.1f;
			woodcrossbow.base_stats["range"] = 4f;
			EquipmentAsset coppercrossbow = AssetManager.items.get("copper_crossbow");
			coppercrossbow.base_stats["damage"] = 4f; //2 +100%
			coppercrossbow.base_stats["multiplier_attack_speed"] = -0.55f; //DPTr = 1.08/1.8
			coppercrossbow.base_stats["accuracy"] = 0.2f;
			coppercrossbow.base_stats["range"] = 4f;
			EquipmentAsset bronzecrossbow = AssetManager.items.get("bronze_crossbow");
			bronzecrossbow.base_stats["damage"] = 6f; //3 +100%
			bronzecrossbow.base_stats["multiplier_attack_speed"] = -0.5f; //DPTr = 1.8/3
			bronzecrossbow.base_stats["accuracy"] = 0.2f;
			bronzecrossbow.base_stats["range"] = 4.5f;
			EquipmentAsset silvercrossbow = AssetManager.items.get("silver_crossbow");
			silvercrossbow.base_stats["damage"] = 8f; //4 +100%
			silvercrossbow.base_stats["multiplier_attack_speed"] = -0.5f; //DPTr = 2.4/4
			silvercrossbow.base_stats["accuracy"] = 0.25f;
			silvercrossbow.base_stats["range"] = 5f;
			EquipmentAsset goldcrossbow = AssetManager.items.get("gold_crossbow");
			goldcrossbow.base_stats["damage"] = 10f; //5 +100%
			goldcrossbow.base_stats["multiplier_attack_speed"] = -0.5f; //DPTr = 3/5
			goldcrossbow.base_stats["accuracy"] = 0.3f;
			goldcrossbow.base_stats["range"] = 5.5f;
			EquipmentAsset ironcrossbow = AssetManager.items.get("iron_crossbow");
			ironcrossbow.base_stats["damage"] = 12f; //6 +100%
			ironcrossbow.base_stats["multiplier_attack_speed"] = -0.45f; //DPTr = 3.96/6.6
			ironcrossbow.base_stats["accuracy"] = 0.3f;
			ironcrossbow.base_stats["range"] = 6f;
			EquipmentAsset steelcrossbow = AssetManager.items.get("steel_crossbow");
			steelcrossbow.base_stats["damage"] = 14f; //7 +100%
			steelcrossbow.base_stats["multiplier_attack_speed"] = -0.45f; //DPTr = 4.62/7.7
			steelcrossbow.base_stats["accuracy"] = 0.35f;
			steelcrossbow.base_stats["range"] = 6f;
			EquipmentAsset mythrilcrossbow = AssetManager.items.get("mythril_crossbow");
			mythrilcrossbow.base_stats["damage"] = 16f; //8 +100%
			mythrilcrossbow.base_stats["multiplier_attack_speed"] = -0.45f; //DPTr = 5.28/8.8
			mythrilcrossbow.base_stats["accuracy"] = 0.4f;
			mythrilcrossbow.base_stats["range"] = 6.5f;
			EquipmentAsset adamantinecrossbow = AssetManager.items.get("adamantine_crossbow");
			adamantinecrossbow.base_stats["damage"] = 20f; //10 +100%
			adamantinecrossbow.base_stats["multiplier_attack_speed"] = -0.4f; //DPTr = 7.2/12
			adamantinecrossbow.base_stats["accuracy"] = 0.5f;
			adamantinecrossbow.base_stats["range"] = 7f;

			//Slingshots [🠝attack speed, 🠝projectile, 🠟🠟damage, 🠟🠟🠟range]
			//notes: slingshots have a damage range of 50%, and recieve +10% crit and +50% crit damage like bows
			//changes: none
			EquipmentAsset woodslingshot = AssetManager.items.get("wood_slingshot");
			woodslingshot.base_stats["damage"] = 1f; //1
			woodslingshot.base_stats["multiplier_attack_speed"] = 0.1f;
			woodslingshot.base_stats["range"] = 1f;
			woodslingshot.base_stats["projectiles"] = 1f; //DPTr*Projectiles = 0.44/1.1
			EquipmentAsset copperslingshot = AssetManager.items.get("copper_slingshot");
			copperslingshot.base_stats["damage"] = 1f; //2
			copperslingshot.base_stats["multiplier_attack_speed"] = 0.1f;
			copperslingshot.base_stats["range"] = 1f;
			copperslingshot.base_stats["projectiles"] = 2f; //DPTr*Projectiles = 0.88/2.2
			EquipmentAsset bronzeslingshot = AssetManager.items.get("bronze_slingshot");
			bronzeslingshot.base_stats["damage"] = 2f; //3
			bronzeslingshot.base_stats["multiplier_attack_speed"] = 0.15f;
			bronzeslingshot.base_stats["range"] = 2f;
			bronzeslingshot.base_stats["projectiles"] = 2f; //DPTr*Projectiles = 1.76/4.4
			EquipmentAsset silverslingshot = AssetManager.items.get("silver_slingshot");
			silverslingshot.base_stats["damage"] = 3f; //4
			silverslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
			silverslingshot.base_stats["range"] = 2f;
			silverslingshot.base_stats["projectiles"] = 2f; //DPTr*Projectiles = 2.88/7.2
			EquipmentAsset goldslingshot = AssetManager.items.get("gold_slingshot");
			goldslingshot.base_stats["damage"] = 3f; //5
			goldslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
			goldslingshot.base_stats["range"] = 3f;
			goldslingshot.base_stats["projectiles"] = 3f; //DPTr*Projectiles = 4.32/10.8
			EquipmentAsset ironslingshot = AssetManager.items.get("iron_slingshot");
			ironslingshot.base_stats["damage"] = 4f; //6
			ironslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
			ironslingshot.base_stats["range"] = 3f;
			ironslingshot.base_stats["projectiles"] = 3f; //DPTr*Projectiles = 5.76/14.4
			EquipmentAsset steelslingshot = AssetManager.items.get("steel_slingshot");
			steelslingshot.base_stats["damage"] = 4f; //7
			steelslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
			steelslingshot.base_stats["range"] = 4f;
			steelslingshot.base_stats["projectiles"] = 3f; //DPTr*Projectiles = 5.76/14.4
			EquipmentAsset mythrilslingshot = AssetManager.items.get("mythril_slingshot");
			mythrilslingshot.base_stats["damage"] = 4f; //8
			mythrilslingshot.base_stats["multiplier_attack_speed"] = 0.25f;
			mythrilslingshot.base_stats["range"] = 4f;
			mythrilslingshot.base_stats["projectiles"] = 3f; //DPTr*Projectiles = 6/15
			EquipmentAsset adamantineslingshot = AssetManager.items.get("adamantine_slingshot");
			adamantineslingshot.base_stats["damage"] = 4f; //10
			adamantineslingshot.base_stats["multiplier_attack_speed"] = 0.25f;
			adamantineslingshot.base_stats["range"] = 5f;
			adamantineslingshot.base_stats["projectiles"] = 4f; //DPTr*Projectiles = 8/20

			//Special---------------------------------------------------------------------------------------------------------
			//flame vs ice: 🠝damage, 0attack speed%, 0critical chance, 🠝critical damage%, 0recoil, 🠟damage range, 🠝targets, 🠝mana, 🠝stamina
			//ice vs flame: 🠟damage, 🠝attack speed%, 🠝critical chance, 0critical damage%, 🠟recoil, 🠝damage range, 0targets, 🠟mana, 🠟stamina

			//flame: changed mana and stamina constants from 15 to 35, damage range is 93.75% of normally
			EquipmentAsset flamesword = AssetManager.items.get("flame_sword"); //DPTr = 22.5/30
			flamesword.base_stats["damage"] = 30f; //changed from 33
			flamesword.base_stats["damage_range"] = 0.75f; //changed from 0.5 (swords are normally 0.8)
			flamesword.base_stats["mana"] = 35f;
			flamesword.base_stats["stamina"] = 35f;
			flamesword.setCost(10, "dragon_scales", 2, "none", 0); //added 10 gold cost (not sure why it doesn't it have this in the base game, probably an overlook)
			EquipmentAsset flameaxe = AssetManager.items.get("flame_axe"); //DPTr = 18/32
			flameaxe.base_stats["damage"] = 40f; //changed from 35
			flameaxe.base_stats["multiplier_attack_speed"] = -0.2f; //changed from 0
			flameaxe.base_stats["critical_chance"] = 0.35f; //changed from 0.25
			flameaxe.base_stats["damage_range"] = 0.5625f; //changed from 0.6
			flameaxe.base_stats["mana"] = 35f;
			flameaxe.base_stats["stamina"] = 35f;
			EquipmentAsset flamehammer = AssetManager.items.get("flame_hammer"); //DPTr = 3.117/33.25
			flamehammer.base_stats["damage"] = 35f; //changed from 20
			flamehammer.base_stats["multiplier_attack_speed"] = -0.05f; //changed from 0
			flamehammer.base_stats["critical_damage_multiplier"] = 0.1f; //changed from 0
			flamehammer.base_stats["damage_range"] = 0.09375f; //changed from 0.5
			flamehammer.base_stats["mana"] = 35f;
			flamehammer.base_stats["stamina"] = 35f;
			flamehammer.base_stats.addTag("building_immunity_fire"); //for some reason this isn't added to the orignal item
			EquipmentAsset flamespear = AssetManager.items.get("flame_spear"); //DPTr = 16.41/25
			flamespear.base_stats["critical_chance"] = -0.03f; //changed from 0
			flamespear.base_stats["range"] = 1.35f; //changed from 1.1
			flamespear.base_stats["damage_range"] = 0.6563f; //changed from 0.7
			flamespear.base_stats["mana"] = 35f;
			flamespear.base_stats["stamina"] = 35f;
			EquipmentAsset flamebow = AssetManager.items.get("flame_bow"); //DPTr = 12.38/22
			flamebow.base_stats["damage_range"] = 0.5625f; //changed from 0.6
			flamebow.base_stats["mana"] = 35f;
			flamebow.base_stats["stamina"] = 35f;
			EquipmentAsset flamebludgeon = AssetManager.items.get("flame_bludgeon"); //DPTr = 3.1892/68
			flamebludgeon.base_stats["damage"] = 80f; //changed from 50
			flamebludgeon.base_stats["damage_range"] = 0.0469f; //changed from 0.05
            flamebludgeon.base_stats["mana"] = 35f;
            flamebludgeon.base_stats["stamina"] = 35f;
			EquipmentAsset flamehalberd = AssetManager.items.get("flame_halberd"); //DPTr = 12.53/24.3
			flamehalberd.base_stats["damage"] = 27f; //not changed
			flamehalberd.base_stats["damage_range"] = 0.5156f; //changed from 0.55
            flamehalberd.base_stats["mana"] = 35f;
            flamehalberd.base_stats["stamina"] = 35f;
			EquipmentAsset flamedagger = AssetManager.items.get("ice_dagger"); //DPTr = 17.85/22.4
			flamedagger.base_stats["damage"] = 16f; //not changed
			flamedagger.base_stats["multiplier_attack_speed"] = 0.4f; //not changed
			flamedagger.base_stats["damage_range"] = 0.7969f; //changed from 0.85
            flamedagger.base_stats["mana"] = 35f;
            flamedagger.base_stats["stamina"] = 35f;
			EquipmentAsset flamecrossbow = AssetManager.items.get("flame_crossbow"); //DPTr = 14.04/23.4
			flamecrossbow.base_stats["damage_range"] = 0.5625f; //changed from 0.6
			flamecrossbow.base_stats["mana"] = 35f;
			flamecrossbow.base_stats["stamina"] = 35f;
			EquipmentAsset flameslingshot = AssetManager.items.get("flame_slingshot"); //DPTr*Projectiles = 9.375/25
            flameslingshot.base_stats["damage_range"] = 0.375f; //changed from 0.4
			flameslingshot.base_stats["mana"] = 35f;
			flameslingshot.base_stats["stamina"] = 35f;

			//ice: normal constants are +15 mana, + 15 stamina, +1 speed, this changes to +30 mana, + 30 stamina, +3 speed, damage range is 106.25% of normally
			EquipmentAsset icesword = AssetManager.items.get("ice_sword"); //DPTr = 23.38/27.5
			icesword.base_stats["damage"] = 25f; //not changed
			icesword.base_stats["damage_range"] = 0.85f; //changed from 0.8
			icesword.base_stats["mana"] = 30f;
			icesword.base_stats["stamina"] = 30f;
			EquipmentAsset iceaxe = AssetManager.items.get("ice_axe"); //DPTr = 17.88/28.05
			iceaxe.base_stats["damage"] = 33f; //changed from 28
			iceaxe.base_stats["multiplier_attack_speed"] = -0.15f; //changed from 0
			iceaxe.base_stats["critical_chance"] = 0.4f; //changed from 0.25
            iceaxe.base_stats["damage_range"] = 0.6375f; //changed from 0.6
            iceaxe.base_stats["mana"] = 30f;
            iceaxe.base_stats["stamina"] = 30f;
			EquipmentAsset icehammer = AssetManager.items.get("ice_hammer"); //DPTr = 4.85/30.45
			icehammer.base_stats["damage"] = 29f; //changed from 20
			icehammer.base_stats["multiplier_attack_speed"] = -0.05f; //changed from 0
			icehammer.base_stats["damage_range"] = 0.1594f; //changed from 0.5
			icehammer.base_stats["mana"] = 30f;
			icehammer.base_stats["stamina"] = 30f;
			EquipmentAsset icespear = AssetManager.items.get("ice_spear"); //DPTr = 14.87/20
			icespear.base_stats["critical_chance"] = -0.03f; //changed from 0
			icespear.base_stats["range"] = 1.35f; //changed from 1.1
            icespear.base_stats["damage_range"] = 0.7438f; //changed from 0.7
            icespear.base_stats["mana"] = 30f;
            icespear.base_stats["stamina"] = 30f;
			EquipmentAsset icebow = AssetManager.items.get("ice_bow"); //DPTr = 11.48/18
			icebow.base_stats["damage_range"] = 0.6375f; //changed from 0.6
			icebow.base_stats["mana"] = 30f;
			icebow.base_stats["stamina"] = 30f;
			EquipmentAsset icebludgeon = AssetManager.items.get("ice_bludgeon"); //DPTr = 2.93/55.25
			icebludgeon.base_stats["damage"] = 65f; //chagned from 40
			icebludgeon.base_stats["damage_range"] = 0.0531f; //changed from 0.05
            icebludgeon.base_stats["mana"] = 30f;
            icebludgeon.base_stats["stamina"] = 30f;
			EquipmentAsset icehalberd = AssetManager.items.get("ice_halberd"); //DPTr = 12.21/20.9
			icehalberd.base_stats["damage"] = 22f; //not changed
			icehalberd.base_stats["damage_range"] = 0.5844f; //changed from 0.55
            icehalberd.base_stats["mana"] = 30f;
            icehalberd.base_stats["stamina"] = 30f;
			EquipmentAsset icedagger = AssetManager.items.get("ice_dagger"); //DPTr = 15.71/17.4
			icedagger.base_stats["damage"] = 12f; //not changed
			icedagger.base_stats["multiplier_attack_speed"] = 0.45f; //not changed
			icedagger.base_stats["damage_range"] = 0.9031f; //changed from 0.85
            icedagger.base_stats["mana"] = 30f;
            icedagger.base_stats["stamina"] = 30f;
			EquipmentAsset icecrossbow = AssetManager.items.get("ice_crossbow"); //DPTr = 12.43/19.5
			icecrossbow.base_stats["damage_range"] = 0.6375f; //changed from 0.6
			icecrossbow.base_stats["mana"] = 30f;
			icecrossbow.base_stats["stamina"] = 30f;
			EquipmentAsset iceslingshot = AssetManager.items.get("ice_slingshot"); //DPTr*Projectiles = 9.18/21.6
            iceslingshot.base_stats["damage_range"] = 0.425f; //changed from 0.4
			iceslingshot.base_stats["mana"] = 30f;
			iceslingshot.base_stats["stamina"] = 30f;

			//---------------------------------------------------------------------------------------------------------
			//Material Constants
			//Wood: +15 Stamina
			//Stone: -2 Speed
			//Copper: +10 Stamina
			//Bronze: +5 Stamina
			//Silver: +5 Stamina, +15 Mana
			//Gold: +10 Stamina, +10 Mana, +10% Attack Speed
			//Iron: +5 Stamina
			//Steel: +5 Stamina, +1 Speed
			//Mythril: +10 Stamina, +25 Mana
			//Adamantine: +10 Stamina, +35 Mana, +1 Speed
        }
		public static bool addStunnedEffectOnTarget20Edit(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
		{
			if (!pTarget.isRekt() && !pTarget.isBuilding() && Randy.randomChance(0.2f)){
				pTarget.addStatusEffect("stunned", 3f, true);
				return true;
			}
			return false;
		}
		public static bool addBurningEffectOnTargetEdit(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
		{
			if (!pTarget.isAlive())
			{
				return false;
			}
			if (pTarget.isBuilding() && pTarget.b.isBurnable() && Randy.randomChance(0.33f))
			{
				pTarget.addStatusEffect("burning", 10f, true);
				return true;
			}
			if (pTarget.isActor() && Randy.randomChance(0.33f))
			{
				pTarget.addStatusEffect("burning", 10f, true);
				return true;
			}
			return false;
		}
		public static bool addFrozenEffectOnTarget20Edit(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
		{
			if(!pTarget.isRekt() && !pTarget.isBuilding() && Randy.randomChance(0.2f)){
				if (!pTarget.current_tile.Type.lava && !pTarget.current_tile.isOnFire()){
					pTarget.addStatusEffect("frozen", 3f, true);
					return true;
				}
			}
			return false;
		}
    }
}
