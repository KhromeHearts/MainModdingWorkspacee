//---Credit for code help: This page uses minorly modified [//Item Lists] structure from the Modernbox mod
//---This mod received no direct assistance from contributors of the Modernbox mod and was made by referencing its code
//BasicItems: This file adds custom basic item types, these should fit in with items like swords, bows, etc
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NeoModLoader.api;

namespace avbsexpandedarmory{
    class BasicWeaponItems : MonoBehaviour{
        public static void init(){
            int targetIndex = 0; //used for category/item positioning
    //Groups Addded
            ItemGroupAsset bludgeons = new ItemGroupAsset(){
               id = "Bludgeons",
               name = "equipment_group_bludgeon",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){bludgeons.color = "#ffbaba";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "spear");
            AssetManager.item_groups.list.Remove(bludgeons);
            AssetManager.item_groups.list.Insert(targetIndex + 1, bludgeons);
            ItemGroupAsset halberds = new ItemGroupAsset(){
               id = "Halberds",
               name = "equipment_group_halberd",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){halberds.color = "#ffbaba";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "Bludgeons");
            AssetManager.item_groups.list.Remove(halberds);
            AssetManager.item_groups.list.Insert(targetIndex + 1, halberds);
            ItemGroupAsset daggers = new ItemGroupAsset(){
               id = "Daggers",
               name = "equipment_group_dagger",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){daggers.color = "#ffbaba";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "Halberds");
            AssetManager.item_groups.list.Remove(daggers);
            AssetManager.item_groups.list.Insert(targetIndex + 1, daggers);
            ItemGroupAsset crossbows = new ItemGroupAsset(){
               id = "Crossbows",
               name = "equipment_group_crossbow",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){crossbows.color = "#e6baff";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "bow");
            AssetManager.item_groups.list.Remove(crossbows);
            AssetManager.item_groups.list.Insert(targetIndex + 1, crossbows);
            ItemGroupAsset slingshots = new ItemGroupAsset(){
               id = "Slingshots",
               name = "equipment_group_slingshot",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){slingshots.color = "#e6baff";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "Crossbows");
            AssetManager.item_groups.list.Remove(slingshots);
            AssetManager.item_groups.list.Insert(targetIndex + 1, slingshots);
    //Items Added
	    //Bludgeons---------------------------------------------------------------------------------------------------------
        //Bludgeon Lovers culture trait
            CultureTrait bludgeonlovers = AssetManager.culture_traits.clone("Bludgeon Lovers", "sword_lovers");
            bludgeonlovers.needs_to_be_explored = false;
            bludgeonlovers.path_icon = "ui/icons/iconBludgeonLovers";
            bludgeonlovers.related_weapon_subtype_ids.Add("bludgeon");
            bludgeonlovers.related_weapon_subtype_ids.Remove("sword");
            bludgeonlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){
                bludgeonlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_dog"));
                bludgeonlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_buffalo"));
            }
	    //Wood Bludgeon
            EquipmentAsset woodbludgeon = AssetManager.items.clone("wood_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            woodbludgeon.needs_to_be_explored = false;
            woodbludgeon.metallic = false;
            woodbludgeon.equipment_subtype = null;
            woodbludgeon.material = "wood";
            woodbludgeon.group_id = "Bludgeons";
            woodbludgeon.translation_key = "wood_bludgeon_locale";
            woodbludgeon.path_icon = "ui/icons/items/icon_wood_bludgeon";
            woodbludgeon.path_gameplay_sprite = "weapons/wood_bludgeon";
            woodbludgeon.setCost(0, "wood", 1, "none", 0);
            woodbludgeon.base_stats["damage"] = 3f;
            woodbludgeon.base_stats["multiplier_attack_speed"] = -0.4f;
			woodbludgeon.base_stats["critical_chance"] = -0.55f;
			woodbludgeon.base_stats["damage_range"] = 0.05f;
            woodbludgeon.base_stats["targets"] = 1f;
            woodbludgeon.base_stats["stamina"] = 15f;
            woodbludgeon.rigidity_rating = 1;
            woodbludgeon.equipment_value = 1;
            AssetManager.items.pot_weapon_assets_all.Add(woodbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(woodbludgeon);
	    //Stone Bludgeon
            EquipmentAsset stonebludgeon = AssetManager.items.clone("stone_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            stonebludgeon.needs_to_be_explored = false;
            stonebludgeon.metallic = false;
            stonebludgeon.equipment_subtype = null;
            stonebludgeon.material = "stone";
            stonebludgeon.group_id = "Bludgeons";
            stonebludgeon.translation_key = "stone_bludgeon_locale";
            stonebludgeon.path_icon = "ui/icons/items/icon_stone_bludgeon";
            stonebludgeon.path_gameplay_sprite = "weapons/stone_bludgeon";
            stonebludgeon.setCost(0, "stone", 1, "none", 0);
            stonebludgeon.base_stats["damage"] = 9f;
            stonebludgeon.base_stats["multiplier_attack_speed"] = -0.4f;
			stonebludgeon.base_stats["critical_chance"] = -0.5f;
			stonebludgeon.base_stats["damage_range"] = 0.05f;
            stonebludgeon.base_stats["targets"] = 1f;
            stonebludgeon.base_stats["speed"] = -2f;
            stonebludgeon.rigidity_rating = 2;
            stonebludgeon.equipment_value = 10;
            AssetManager.items.pot_weapon_assets_all.Add(stonebludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(stonebludgeon);
	    //Copper Bludgeon
            EquipmentAsset copperbludgeon = AssetManager.items.clone("copper_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            copperbludgeon.needs_to_be_explored = false;
            copperbludgeon.metallic = true;
            copperbludgeon.equipment_subtype = null;
            copperbludgeon.material = "copper";
            copperbludgeon.group_id = "Bludgeons";
            copperbludgeon.translation_key = "copper_bludgeon_locale";
            copperbludgeon.path_icon = "ui/icons/items/icon_copper_bludgeon";
            copperbludgeon.path_gameplay_sprite = "weapons/copper_bludgeon";
            copperbludgeon.setCost(0, "common_metals", 1, "none", 0);
            copperbludgeon.base_stats["damage"] = 9f;
            copperbludgeon.base_stats["multiplier_attack_speed"] = -0.35f;
			copperbludgeon.base_stats["critical_chance"] = -0.5f;
            copperbludgeon.base_stats["damage_range"] = 0.05f;
            copperbludgeon.base_stats["targets"] = 1f;
            copperbludgeon.base_stats["stamina"] = 10f;
            copperbludgeon.rigidity_rating = 2;
            copperbludgeon.equipment_value = 10;
            AssetManager.items.pot_weapon_assets_all.Add(copperbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(copperbludgeon);
	    //Bronze Bludgeon
            EquipmentAsset bronzebludgeon = AssetManager.items.clone("bronze_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            bronzebludgeon.needs_to_be_explored = false;
            bronzebludgeon.metallic = true;
            bronzebludgeon.equipment_subtype = null;
            bronzebludgeon.material = "bronze";
            bronzebludgeon.group_id = "Bludgeons";
            bronzebludgeon.translation_key = "bronze_bludgeon_locale";
            bronzebludgeon.path_icon = "ui/icons/items/icon_bronze_bludgeon";
            bronzebludgeon.path_gameplay_sprite = "weapons/bronze_bludgeon";
            bronzebludgeon.setCost(0, "common_metals", 2, "none", 0);
            bronzebludgeon.base_stats["damage"] = 12f;
            bronzebludgeon.base_stats["multiplier_attack_speed"] = -0.35f;
			bronzebludgeon.base_stats["critical_chance"] = -0.45f;
            bronzebludgeon.base_stats["damage_range"] = 0.05f;
            bronzebludgeon.base_stats["targets"] = 1f;
            bronzebludgeon.base_stats["stamina"] = 5f;
            bronzebludgeon.rigidity_rating = 3;
            bronzebludgeon.equipment_value = 15;
            AssetManager.items.pot_weapon_assets_all.Add(bronzebludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(bronzebludgeon);
	    //Silver Bludgeon
            EquipmentAsset silverbludgeon = AssetManager.items.clone("silver_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            silverbludgeon.needs_to_be_explored = false;
            silverbludgeon.metallic = true;
            silverbludgeon.equipment_subtype = null;
            silverbludgeon.material = "silver";
            silverbludgeon.group_id = "Bludgeons";
            silverbludgeon.translation_key = "silver_bludgeon_locale";
            silverbludgeon.path_icon = "ui/icons/items/icon_silver_bludgeon";
            silverbludgeon.path_gameplay_sprite = "weapons/silver_bludgeon";
            silverbludgeon.setCost(0, "silver", 1, "none", 0);
            silverbludgeon.base_stats["damage"] = 15f;
            silverbludgeon.base_stats["multiplier_attack_speed"] = -0.35f;
			silverbludgeon.base_stats["critical_chance"] = -0.45f;
            silverbludgeon.base_stats["damage_range"] = 0.05f;
            silverbludgeon.base_stats["targets"] = 1f;
            silverbludgeon.base_stats["mana"] = 15f;
            silverbludgeon.base_stats["stamina"] = 5f;
            silverbludgeon.rigidity_rating = 2;
            silverbludgeon.equipment_value = 20;
            AssetManager.items.pot_weapon_assets_all.Add(silverbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(silverbludgeon);
	    //Gold Bludgeon
			EquipmentAsset goldbludgeon = AssetManager.items.clone("gold_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            goldbludgeon.needs_to_be_explored = false;
            goldbludgeon.metallic = true;
            goldbludgeon.equipment_subtype = null;
            goldbludgeon.material = "gold";
            goldbludgeon.group_id = "Bludgeons";
            goldbludgeon.translation_key = "gold_bludgeon_locale";
            goldbludgeon.path_icon = "ui/icons/items/icon_gold_bludgeon";
            goldbludgeon.path_gameplay_sprite = "weapons/gold_bludgeon";
            goldbludgeon.setCost(0, "gold", 2, "common_metals", 2);
            goldbludgeon.base_stats["damage"] = 15f;
            goldbludgeon.base_stats["multiplier_attack_speed"] = -0.3f;
			goldbludgeon.base_stats["critical_chance"] = -0.45f;
            goldbludgeon.base_stats["damage_range"] = 0.05f;
            goldbludgeon.base_stats["targets"] = 1f;
            goldbludgeon.base_stats["mana"] = 10f;
            goldbludgeon.base_stats["stamina"] = 10f;
            goldbludgeon.rigidity_rating = 2;
            goldbludgeon.equipment_value = 25;
            AssetManager.items.pot_weapon_assets_all.Add(goldbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldbludgeon);
	    //Iron Bludgeon
            EquipmentAsset ironbludgeon = AssetManager.items.clone("iron_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            ironbludgeon.needs_to_be_explored = false;
            ironbludgeon.metallic = true;
            ironbludgeon.equipment_subtype = null;
            ironbludgeon.material = "iron";
            ironbludgeon.group_id = "Bludgeons";
            ironbludgeon.translation_key = "iron_bludgeon_locale";
            ironbludgeon.path_icon = "ui/icons/items/icon_iron_bludgeon";
            ironbludgeon.path_gameplay_sprite = "weapons/iron_bludgeon";
            ironbludgeon.setCost(0, "common_metals", 3, "none", 0);
            ironbludgeon.base_stats["damage"] = 18f;
            ironbludgeon.base_stats["multiplier_attack_speed"] = -0.3f;
			ironbludgeon.base_stats["critical_chance"] = -0.4f;
            ironbludgeon.base_stats["damage_range"] = 0.05f;
            ironbludgeon.base_stats["targets"] = 1f;
            ironbludgeon.base_stats["stamina"] = 5f;
            ironbludgeon.rigidity_rating = 4;
            ironbludgeon.equipment_value = 30;
            AssetManager.items.pot_weapon_assets_all.Add(ironbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(ironbludgeon);
	    //Steel Bludgeon
            EquipmentAsset steelbludgeon = AssetManager.items.clone("steel_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            steelbludgeon.needs_to_be_explored = false;
            steelbludgeon.metallic = true;
            steelbludgeon.equipment_subtype = null;
            steelbludgeon.material = "steel";
            steelbludgeon.group_id = "Bludgeons";
            steelbludgeon.translation_key = "steel_bludgeon_locale";
            steelbludgeon.path_icon = "ui/icons/items/icon_steel_bludgeon";
            steelbludgeon.path_gameplay_sprite = "weapons/steel_bludgeon";
            steelbludgeon.setCost(0, "common_metals", 4, "none", 0);
            steelbludgeon.base_stats["damage"] = 21f;
            steelbludgeon.base_stats["multiplier_attack_speed"] = -0.3f;
			steelbludgeon.base_stats["critical_chance"] = -0.4f;
            steelbludgeon.base_stats["damage_range"] = 0.05f;
            steelbludgeon.base_stats["targets"] = 1f;
            steelbludgeon.base_stats["speed"] = 1f;
            steelbludgeon.base_stats["stamina"] = 5f;
            steelbludgeon.rigidity_rating = 5;
            steelbludgeon.equipment_value = 40;
            AssetManager.items.pot_weapon_assets_all.Add(steelbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(steelbludgeon);
	    //Mythril Bludgeon
            EquipmentAsset mythrilbludgeon = AssetManager.items.clone("mythril_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            mythrilbludgeon.needs_to_be_explored = false;
            mythrilbludgeon.metallic = true;
            mythrilbludgeon.equipment_subtype = null;
            mythrilbludgeon.material = "mythril";
            mythrilbludgeon.group_id = "Bludgeons";
            mythrilbludgeon.translation_key = "mythril_bludgeon_locale";
            mythrilbludgeon.path_icon = "ui/icons/items/icon_mythril_bludgeon";
            mythrilbludgeon.path_gameplay_sprite = "weapons/mythril_bludgeon";
            mythrilbludgeon.setCost(0, "mythril", 1, "none", 0);
            mythrilbludgeon.base_stats["damage"] = 24f; 
            mythrilbludgeon.base_stats["multiplier_attack_speed"] = -0.25f;
			mythrilbludgeon.base_stats["critical_chance"] = -0.35f;
            mythrilbludgeon.base_stats["damage_range"] = 0.05f;
            mythrilbludgeon.base_stats["targets"] = 1f;
            mythrilbludgeon.base_stats["mana"] = 25f;
            mythrilbludgeon.base_stats["stamina"] = 10f;
            mythrilbludgeon.rigidity_rating = 6;
            mythrilbludgeon.equipment_value = 50;
            AssetManager.items.pot_weapon_assets_all.Add(mythrilbludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(mythrilbludgeon);
	    //Adamantine Bludgeon
            EquipmentAsset adamantinebludgeon = AssetManager.items.clone("adamantine_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            adamantinebludgeon.needs_to_be_explored = false;
            adamantinebludgeon.metallic = true;
            adamantinebludgeon.equipment_subtype = null;
            adamantinebludgeon.material = "adamantine";
            adamantinebludgeon.group_id = "Bludgeons";
            adamantinebludgeon.translation_key = "adamantine_bludgeon_locale";
            adamantinebludgeon.path_icon = "ui/icons/items/icon_adamantine_bludgeon";
            adamantinebludgeon.path_gameplay_sprite = "weapons/adamantine_bludgeon";
            adamantinebludgeon.setCost(0, "adamantine", 1, "none", 0);
            adamantinebludgeon.base_stats["damage"] = 30f;
            adamantinebludgeon.base_stats["multiplier_attack_speed"] = -0.2f;
			adamantinebludgeon.base_stats["critical_chance"] = -0.3f;
            adamantinebludgeon.base_stats["damage_range"] = 0.05f;
            adamantinebludgeon.base_stats["targets"] = 1f;
            adamantinebludgeon.base_stats["speed"] = 1f;
            adamantinebludgeon.base_stats["mana"] = 35f;
            adamantinebludgeon.base_stats["stamina"] = 10f;
            adamantinebludgeon.rigidity_rating = 7;
            adamantinebludgeon.equipment_value = 70;
            AssetManager.items.pot_weapon_assets_all.Add(adamantinebludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(adamantinebludgeon);
	    //Flame Bludgeon
            EquipmentAsset flamebludgeon = AssetManager.items.clone("flame_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            flamebludgeon.needs_to_be_explored = false;
            flamebludgeon.animated = true;
            flamebludgeon.metallic = true;
            flamebludgeon.equipment_subtype = null;
            flamebludgeon.material = "basic";
            flamebludgeon.group_id = "Bludgeons";
            flamebludgeon.translation_key = "flame_bludgeon_locale";
            flamebludgeon.path_icon = "ui/icons/items/icon_flame_bludgeon";
            flamebludgeon.path_gameplay_sprite = "weapons/flame_bludgeon";
            flamebludgeon.setCost(15, "dragon_scales", 3, "none", 0);
            flamebludgeon.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flamebludgeon.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flamebludgeon.base_stats.addTag("building_immunity_fire");
            flamebludgeon.base_stats["damage"] = 50f;
            flamebludgeon.base_stats["multiplier_attack_speed"] = -0.15f;
			flamebludgeon.base_stats["critical_chance"] = -0.25f;
            flamebludgeon.base_stats["damage_range"] = 0.05f;
            flamebludgeon.base_stats["targets"] = 2f;
            flamebludgeon.base_stats["mana"] = 15f;
            flamebludgeon.base_stats["stamina"] = 15f;
            flamebludgeon.rigidity_rating = 8;
            flamebludgeon.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flamebludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flamebludgeon);
	    //Ice Bludgeon
            EquipmentAsset icebludgeon = AssetManager.items.clone("ice_bludgeon", "$hammer"); //adds stun modifer and hammer sounds
            icebludgeon.needs_to_be_explored = false;
            icebludgeon.metallic = true;
            icebludgeon.equipment_subtype = null;
            icebludgeon.material = "basic";
            icebludgeon.group_id = "Bludgeons";
            icebludgeon.translation_key = "ice_bludgeon_locale";
            icebludgeon.path_icon = "ui/icons/items/icon_ice_bludgeon";
            icebludgeon.path_gameplay_sprite = "weapons/ice_bludgeon";
            icebludgeon.setCost(10, "mythril", 10, "gems", 2);
            icebludgeon.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icebludgeon.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icebludgeon.base_stats["damage"] = 40f;
            icebludgeon.base_stats["multiplier_attack_speed"] = -0.1f;
			icebludgeon.base_stats["critical_chance"] = -0.2f;
            icebludgeon.base_stats["damage_range"] = 0.05f;
            icebludgeon.base_stats["targets"] = 1f;
            icebludgeon.base_stats["mana"] = 15f;
            icebludgeon.base_stats["stamina"] = 15f;
            icebludgeon.rigidity_rating = 8;
            icebludgeon.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(icebludgeon);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icebludgeon);
        //Halberds---------------------------------------------------------------------------------------------------------
        //Halberd Lovers culture trait
            CultureTrait halberdlovers = AssetManager.culture_traits.clone("Halberd Lovers", "sword_lovers");
            halberdlovers.needs_to_be_explored = false;
            halberdlovers.path_icon = "ui/icons/iconHalberdLovers";
            halberdlovers.related_weapon_subtype_ids.Add("halberd");
            halberdlovers.related_weapon_subtype_ids.Remove("sword");
            halberdlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){halberdlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_rabbit"));}
	    //Wood Halberd
            EquipmentAsset woodhalberd = AssetManager.items.clone("wood_halberd", "$spear"); //adds stun modifer and hammer sounds
            woodhalberd.needs_to_be_explored = false;
            woodhalberd.metallic = false;
            woodhalberd.equipment_subtype = null;
            woodhalberd.material = "wood";
            woodhalberd.group_id = "Halberds";
            woodhalberd.translation_key = "wood_halberd_locale";
            woodhalberd.path_icon = "ui/icons/items/icon_wood_halberd";
            woodhalberd.path_gameplay_sprite = "weapons/wood_halberd";
            woodhalberd.path_slash_animation = "effects/slashes/slash_axe";
            woodhalberd.setCost(0, "wood", 1, "none", 0);
            woodhalberd.base_stats["damage"] = 2f;
            woodhalberd.base_stats["multiplier_attack_speed"] = -0.35f;
            woodhalberd.base_stats["critical_chance"] = 0.01f;
            woodhalberd.base_stats["range"] = 1f;
            woodhalberd.base_stats["damage_range"] = 0.55f;
            woodhalberd.base_stats["stamina"] = 15f;
            woodhalberd.rigidity_rating = 1;
            woodhalberd.equipment_value = 1;
            AssetManager.items.pot_weapon_assets_all.Add(woodhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(woodhalberd);
        //Stone Halberd
            EquipmentAsset stonehalberd = AssetManager.items.clone("stone_halberd", "$spear"); //adds stun modifer and hammer sounds
            stonehalberd.needs_to_be_explored = false;
            stonehalberd.metallic = false;
            stonehalberd.equipment_subtype = null;
            stonehalberd.material = "stone";
            stonehalberd.group_id = "Halberds";
            stonehalberd.translation_key = "stone_halberd_locale";
            stonehalberd.path_icon = "ui/icons/items/icon_stone_halberd";
            stonehalberd.path_gameplay_sprite = "weapons/stone_halberd";
            stonehalberd.path_slash_animation = "effects/slashes/slash_axe";
            stonehalberd.setCost(0, "stone", 1, "none", 0);
            stonehalberd.base_stats["damage"] = 3f;
            stonehalberd.base_stats["multiplier_attack_speed"] = -0.35f;
            stonehalberd.base_stats["critical_chance"] = 0.03f;
            stonehalberd.base_stats["range"] = 1f;
            stonehalberd.base_stats["damage_range"] = 0.55f;
            stonehalberd.base_stats["speed"] = -2f;
            stonehalberd.rigidity_rating = 2;
            stonehalberd.equipment_value = 10;
            AssetManager.items.pot_weapon_assets_all.Add(stonehalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(stonehalberd);
	    //Copper Halberd
            EquipmentAsset copperhalberd = AssetManager.items.clone("copper_halberd", "$spear"); //adds stun modifer and hammer sounds
            copperhalberd.needs_to_be_explored = false;
            copperhalberd.metallic = true;
            copperhalberd.equipment_subtype = null;
            copperhalberd.material = "copper";
            copperhalberd.group_id = "Halberds";
            copperhalberd.translation_key = "copper_halberd_locale";
            copperhalberd.path_icon = "ui/icons/items/icon_copper_halberd";
            copperhalberd.path_gameplay_sprite = "weapons/copper_halberd";
            copperhalberd.path_slash_animation = "effects/slashes/slash_axe";
            copperhalberd.setCost(0, "common_metals", 1, "none", 0);
            copperhalberd.base_stats["damage"] = 3f;
            copperhalberd.base_stats["multiplier_attack_speed"] = -0.3f;
            copperhalberd.base_stats["critical_chance"] = 0.05f;
            copperhalberd.base_stats["range"] = 1f;
            copperhalberd.base_stats["damage_range"] = 0.55f;
            copperhalberd.base_stats["stamina"] = 10f;
            copperhalberd.rigidity_rating = 2;
            copperhalberd.equipment_value = 10;
            AssetManager.items.pot_weapon_assets_all.Add(copperhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(copperhalberd);
	    //Bronze Halberd
            EquipmentAsset bronzehalberd = AssetManager.items.clone("bronze_halberd", "$spear"); //adds stun modifer and hammer sounds
            bronzehalberd.needs_to_be_explored = false;
            bronzehalberd.metallic = true;
            bronzehalberd.equipment_subtype = null;
            bronzehalberd.material = "bronze";
            bronzehalberd.group_id = "Halberds";
            bronzehalberd.translation_key = "bronze_halberd_locale";
            bronzehalberd.path_icon = "ui/icons/items/icon_bronze_halberd";
            bronzehalberd.path_gameplay_sprite = "weapons/bronze_halberd";
            bronzehalberd.path_slash_animation = "effects/slashes/slash_axe";
            bronzehalberd.setCost(0, "common_metals", 2, "none", 0);
            bronzehalberd.base_stats["damage"] = 4f;
            bronzehalberd.base_stats["multiplier_attack_speed"] = -0.3f;
            bronzehalberd.base_stats["critical_chance"] = 0.05f;
            bronzehalberd.base_stats["range"] = 1f;
            bronzehalberd.base_stats["damage_range"] = 0.55f;
            bronzehalberd.base_stats["stamina"] = 5f;
            bronzehalberd.rigidity_rating = 3;
            bronzehalberd.equipment_value = 15;
            AssetManager.items.pot_weapon_assets_all.Add(bronzehalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(bronzehalberd);
	    //Silver Halberd
            EquipmentAsset silverhalberd = AssetManager.items.clone("silver_halberd", "$spear"); //adds stun modifer and hammer sounds
            silverhalberd.needs_to_be_explored = false;
            silverhalberd.metallic = true;
            silverhalberd.equipment_subtype = null;
            silverhalberd.material = "silver";
            silverhalberd.group_id = "Halberds";
            silverhalberd.translation_key = "silver_halberd_locale";
            silverhalberd.path_icon = "ui/icons/items/icon_silver_halberd";
            silverhalberd.path_gameplay_sprite = "weapons/silver_halberd";
            silverhalberd.path_slash_animation = "effects/slashes/slash_axe";
            silverhalberd.setCost(0, "silver", 1, "none", 0);
            silverhalberd.base_stats["damage"] = 5f;
            silverhalberd.base_stats["multiplier_attack_speed"] = -0.25f;
            silverhalberd.base_stats["critical_chance"] = 0.08f;
            silverhalberd.base_stats["range"] = 1f;
            silverhalberd.base_stats["damage_range"] = 0.55f;
            silverhalberd.base_stats["mana"] = 15f;
            silverhalberd.base_stats["stamina"] = 5f;
            silverhalberd.rigidity_rating = 2;
            silverhalberd.equipment_value = 20;
            AssetManager.items.pot_weapon_assets_all.Add(silverhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(silverhalberd);
	    //Gold Halberd
			EquipmentAsset goldhalberd = AssetManager.items.clone("gold_halberd", "$spear"); //adds stun modifer and hammer sounds
            goldhalberd.needs_to_be_explored = false;
            goldhalberd.metallic = true;
            goldhalberd.equipment_subtype = null;
            goldhalberd.material = "gold";
            goldhalberd.group_id = "Halberds";
            goldhalberd.translation_key = "gold_halberd_locale";
            goldhalberd.path_icon = "ui/icons/items/icon_gold_halberd";
            goldhalberd.path_gameplay_sprite = "weapons/gold_halberd";
            goldhalberd.path_slash_animation = "effects/slashes/slash_axe";
            goldhalberd.setCost(0, "gold", 2, "common_metals", 2);
            goldhalberd.base_stats["damage"] = 6f;
            goldhalberd.base_stats["multiplier_attack_speed"] = -0.2f;
            goldhalberd.base_stats["critical_chance"] = 0.1f;
            goldhalberd.base_stats["range"] = 1f;
            goldhalberd.base_stats["damage_range"] = 0.55f;
            goldhalberd.base_stats["mana"] = 10f;
            goldhalberd.base_stats["stamina"] = 10f;
            goldhalberd.rigidity_rating = 2;
            goldhalberd.equipment_value = 25;
            AssetManager.items.pot_weapon_assets_all.Add(goldhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldhalberd);
	    //Iron Halberd
            EquipmentAsset ironhalberd = AssetManager.items.clone("iron_halberd", "$spear"); //adds stun modifer and hammer sounds
            ironhalberd.needs_to_be_explored = false;
            ironhalberd.metallic = true;
            ironhalberd.equipment_subtype = null;
            ironhalberd.material = "iron";
            ironhalberd.group_id = "Halberds";
            ironhalberd.translation_key = "iron_halberd_locale";
            ironhalberd.path_icon = "ui/icons/items/icon_iron_halberd";
            ironhalberd.path_gameplay_sprite = "weapons/iron_halberd";
            ironhalberd.path_slash_animation = "effects/slashes/slash_axe";
            ironhalberd.setCost(0, "common_metals", 3, "none", 0);
            ironhalberd.base_stats["damage"] = 7f;
            ironhalberd.base_stats["multiplier_attack_speed"] = -0.2f;
            ironhalberd.base_stats["critical_chance"] = 0.15f;
            ironhalberd.base_stats["range"] = 1f;
            ironhalberd.base_stats["damage_range"] = 0.55f;
            ironhalberd.base_stats["stamina"] = 5f;
            ironhalberd.rigidity_rating = 4;
            ironhalberd.equipment_value = 30;
            AssetManager.items.pot_weapon_assets_all.Add(ironhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(ironhalberd);
	    //Steel Halberd
            EquipmentAsset steelhalberd = AssetManager.items.clone("steel_halberd", "$spear"); //adds stun modifer and hammer sounds
            steelhalberd.needs_to_be_explored = false;
            steelhalberd.metallic = true;
            steelhalberd.equipment_subtype = null;
            steelhalberd.material = "steel";
            steelhalberd.group_id = "Halberds";
            steelhalberd.translation_key = "steel_halberd_locale";
            steelhalberd.path_icon = "ui/icons/items/icon_steel_halberd";
            steelhalberd.path_gameplay_sprite = "weapons/steel_halberd";
            steelhalberd.path_slash_animation = "effects/slashes/slash_axe";
            steelhalberd.setCost(0, "common_metals", 4, "none", 0);
            steelhalberd.base_stats["damage"] = 8f;
            steelhalberd.base_stats["multiplier_attack_speed"] = -0.2f;
            steelhalberd.base_stats["critical_chance"] = 0.2f;
            steelhalberd.base_stats["range"] = 1f;
            steelhalberd.base_stats["damage_range"] = 0.55f;
            steelhalberd.base_stats["speed"] = 1f;
            steelhalberd.base_stats["stamina"] = 5f;
            steelhalberd.rigidity_rating = 5;
            steelhalberd.equipment_value = 40;
            AssetManager.items.pot_weapon_assets_all.Add(steelhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(steelhalberd);
	    //Mythril Halberd
            EquipmentAsset mythrilhalberd = AssetManager.items.clone("mythril_halberd", "$spear"); //adds stun modifer and hammer sounds
            mythrilhalberd.needs_to_be_explored = false;
            mythrilhalberd.metallic = true;
            mythrilhalberd.equipment_subtype = null;
            mythrilhalberd.material = "mythril";
            mythrilhalberd.group_id = "Halberds";
            mythrilhalberd.translation_key = "mythril_halberd_locale";
            mythrilhalberd.path_icon = "ui/icons/items/icon_mythril_halberd";
            mythrilhalberd.path_gameplay_sprite = "weapons/mythril_halberd";
            mythrilhalberd.path_slash_animation = "effects/slashes/slash_axe";
            mythrilhalberd.setCost(0, "mythril", 1, "none", 0);
            mythrilhalberd.base_stats["damage"] = 9f;
            mythrilhalberd.base_stats["multiplier_attack_speed"] = -0.15f;
            mythrilhalberd.base_stats["critical_chance"] = 0.2f;
            mythrilhalberd.base_stats["range"] = 1f;
            mythrilhalberd.base_stats["damage_range"] = 0.55f;
            mythrilhalberd.base_stats["mana"] = 25f;
            mythrilhalberd.base_stats["stamina"] = 10f;
            mythrilhalberd.rigidity_rating = 6;
            mythrilhalberd.equipment_value = 50;
            AssetManager.items.pot_weapon_assets_all.Add(mythrilhalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(mythrilhalberd);
	    //Adamantine Halberd
            EquipmentAsset adamantinehalberd = AssetManager.items.clone("adamantine_halberd", "$spear"); //adds stun modifer and hammer sounds
            adamantinehalberd.needs_to_be_explored = false;
            adamantinehalberd.metallic = true;
            adamantinehalberd.equipment_subtype = null;
            adamantinehalberd.material = "adamantine";
            adamantinehalberd.group_id = "Halberds";
            adamantinehalberd.translation_key = "adamantine_halberd_locale";
            adamantinehalberd.path_icon = "ui/icons/items/icon_adamantine_halberd";
            adamantinehalberd.path_gameplay_sprite = "weapons/adamantine_halberd";
            adamantinehalberd.path_slash_animation = "effects/slashes/slash_axe";
            adamantinehalberd.setCost(0, "adamantine", 1, "none", 0);
            adamantinehalberd.base_stats["damage"] = 11f;
            adamantinehalberd.base_stats["multiplier_attack_speed"] = -0.15f;
            adamantinehalberd.base_stats["critical_chance"] = 0.25f;
            adamantinehalberd.base_stats["range"] = 1f;
            adamantinehalberd.base_stats["damage_range"] = 0.55f;
            adamantinehalberd.base_stats["speed"] = 1f;
            adamantinehalberd.base_stats["mana"] = 35f;
            adamantinehalberd.base_stats["stamina"] = 10f;
            adamantinehalberd.rigidity_rating = 7;
            adamantinehalberd.equipment_value = 70;
            AssetManager.items.pot_weapon_assets_all.Add(adamantinehalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(adamantinehalberd);
	    //Flame Halberd
            EquipmentAsset flamehalberd = AssetManager.items.clone("flame_halberd", "$spear"); //adds stun modifer and hammer sounds
            flamehalberd.needs_to_be_explored = false;
            flamehalberd.animated = true;
            flamehalberd.metallic = true;
            flamehalberd.equipment_subtype = null;
            flamehalberd.material = "basic";
            flamehalberd.group_id = "Halberds";
            flamehalberd.translation_key = "flame_halberd_locale";
            flamehalberd.path_icon = "ui/icons/items/icon_flame_halberd";
            flamehalberd.path_gameplay_sprite = "weapons/flame_halberd";
            flamehalberd.path_slash_animation = "effects/slashes/slash_axe";
            flamehalberd.setCost(10, "dragon_scales", 2, "none", 0);
            flamehalberd.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flamehalberd.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flamehalberd.base_stats.addTag("building_immunity_fire");
            flamehalberd.base_stats["damage"] = 27f;
            flamehalberd.base_stats["multiplier_attack_speed"] = -0.1f;
            flamehalberd.base_stats["critical_chance"] = 0.25f;
            flamehalberd.base_stats["critical_damage_multiplier"] = 0.1f;
            flamehalberd.base_stats["range"] = 1.1f;
            flamehalberd.base_stats["damage_range"] = 0.55f;
            flamehalberd.base_stats["mana"] = 15f;
            flamehalberd.base_stats["stamina"] = 15f;
            flamehalberd.rigidity_rating = 8;
            flamehalberd.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flamehalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flamehalberd);
	    //Ice Halberd
            EquipmentAsset icehalberd = AssetManager.items.clone("ice_halberd", "$spear"); //adds stun modifer and hammer sounds
            icehalberd.needs_to_be_explored = false;
            icehalberd.metallic = true;
            icehalberd.equipment_subtype = null;
            icehalberd.material = "basic";
            icehalberd.group_id = "Halberds";
            icehalberd.translation_key = "ice_halberd_locale";
            icehalberd.path_icon = "ui/icons/items/icon_ice_halberd";
            icehalberd.path_gameplay_sprite = "weapons/ice_halberd";
            icehalberd.path_slash_animation = "effects/slashes/slash_axe";
            icehalberd.setCost(10, "mythril", 10, "gems", 2);
            icehalberd.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icehalberd.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icehalberd.base_stats["damage"] = 23f;
            icehalberd.base_stats["multiplier_attack_speed"] = -0.05f;
            icehalberd.base_stats["critical_chance"] = 0.35f;
            icehalberd.base_stats["range"] = 1.1f;
            icehalberd.base_stats["damage_range"] = 0.55f;
            icehalberd.base_stats["speed"] = 3f;
            icehalberd.base_stats["mana"] = 15f;
            icehalberd.base_stats["stamina"] = 15f;
            icehalberd.rigidity_rating = 8;
            icehalberd.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(icehalberd);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icehalberd);
        //Daggers---------------------------------------------------------------------------------------------------------
        //Dagger Lovers culture trait
            CultureTrait daggerlovers = AssetManager.culture_traits.clone("Dagger Lovers", "sword_lovers");
            daggerlovers.needs_to_be_explored = false;
            daggerlovers.path_icon = "ui/icons/iconDaggerLovers";
            daggerlovers.related_weapon_subtype_ids.Add("dagger");
            daggerlovers.related_weapon_subtype_ids.Remove("sword");
            daggerlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){daggerlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("bee"));}
        //Initiative modifier
            ItemModAsset initiative = new ItemModAsset();
            initiative.id = "initiative";
            initiative.mod_type = "initiative";
            initiative.translation_key = "mod_initiative";
            initiative.pool = "weapon";
            initiative.rarity = 0;
            initiative.mod_rank = 1;
            initiative.action_attack_target = new AttackAction(BasicWeaponItems.addInitiativeEffect);
            AssetManager.items_modifiers.add(initiative);
	    //Wood Dagger
            EquipmentAsset wooddagger = AssetManager.items.clone("wood_dagger", "$sword");
            wooddagger.needs_to_be_explored = false;
            wooddagger.metallic = false;
            wooddagger.equipment_subtype = null;
            wooddagger.material = "wood";
            wooddagger.group_id = "Daggers";
            wooddagger.translation_key = "wood_dagger_locale";
            wooddagger.path_icon = "ui/icons/items/icon_wood_dagger";
            wooddagger.path_gameplay_sprite = "weapons/wood_dagger";
            wooddagger.setCost(0, "wood", 1, "none", 0);
            wooddagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            wooddagger.base_stats["damage"] = 1f;
            wooddagger.base_stats["multiplier_attack_speed"] = 0.1f;
			wooddagger.base_stats["critical_chance"] = 0.1f;
			wooddagger.base_stats["damage_range"] = 0.9f;
            wooddagger.base_stats["range"] = -0.4f;
            wooddagger.base_stats["stamina"] = 15f;
            wooddagger.rigidity_rating = 1;
            wooddagger.equipment_value = -1;
            AssetManager.items.pot_weapon_assets_all.Add(wooddagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(wooddagger);
	    //Stone Dagger
            EquipmentAsset stonedagger = AssetManager.items.clone("stone_dagger", "$sword");
            stonedagger.needs_to_be_explored = false;
            stonedagger.metallic = false;
            stonedagger.equipment_subtype = null;
            stonedagger.material = "stone";
            stonedagger.group_id = "Daggers";
            stonedagger.translation_key = "stone_dagger_locale";
            stonedagger.path_icon = "ui/icons/items/icon_stone_dagger";
            stonedagger.path_gameplay_sprite = "weapons/stone_dagger";
            stonedagger.setCost(0, "stone", 1, "none", 0);
            stonedagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            stonedagger.base_stats["damage"] = 1f;
            stonedagger.base_stats["multiplier_attack_speed"] = 0.15f;
			stonedagger.base_stats["critical_chance"] = 0.15f;
			stonedagger.base_stats["damage_range"] = 0.9f;
            stonedagger.base_stats["range"] = -0.35f;
            stonedagger.base_stats["speed"] = -2f;
            stonedagger.rigidity_rating = 2;
            stonedagger.equipment_value = 8;
            AssetManager.items.pot_weapon_assets_all.Add(stonedagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(stonedagger);
	    //Copper Dagger
            EquipmentAsset copperdagger = AssetManager.items.clone("copper_dagger", "$sword");
            copperdagger.needs_to_be_explored = false;
            copperdagger.metallic = true;
            copperdagger.equipment_subtype = null;
            copperdagger.material = "copper";
            copperdagger.group_id = "Daggers";
            copperdagger.translation_key = "copper_dagger_locale";
            copperdagger.path_icon = "ui/icons/items/icon_copper_dagger";
            copperdagger.path_gameplay_sprite = "weapons/copper_dagger";
            copperdagger.setCost(0, "common_metals", 1, "none", 0);
            copperdagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            copperdagger.base_stats["damage"] = 2f;
            copperdagger.base_stats["multiplier_attack_speed"] = 0.15f;
			copperdagger.base_stats["critical_chance"] = 0.15f;
            copperdagger.base_stats["damage_range"] = 0.9f;
            copperdagger.base_stats["range"] = -0.35f;
            copperdagger.base_stats["stamina"] = 10f;
            copperdagger.rigidity_rating = 2;
            copperdagger.equipment_value = 8;
            AssetManager.items.pot_weapon_assets_all.Add(copperdagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(copperdagger);
	    //Bronze Dagger
            EquipmentAsset bronzedagger = AssetManager.items.clone("bronze_dagger", "$sword");
            bronzedagger.needs_to_be_explored = false;
            bronzedagger.metallic = true;
            bronzedagger.equipment_subtype = null;
            bronzedagger.material = "bronze";
            bronzedagger.group_id = "Daggers";
            bronzedagger.translation_key = "bronze_dagger_locale";
            bronzedagger.path_icon = "ui/icons/items/icon_bronze_dagger";
            bronzedagger.path_gameplay_sprite = "weapons/bronze_dagger";
            bronzedagger.setCost(0, "common_metals", 2, "none", 0);
            bronzedagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            bronzedagger.base_stats["damage"] = 2f;
            bronzedagger.base_stats["multiplier_attack_speed"] = 0.2f;
			bronzedagger.base_stats["critical_chance"] = 0.2f;
            bronzedagger.base_stats["damage_range"] = 0.9f;
            bronzedagger.base_stats["range"] = -0.3f;
            bronzedagger.base_stats["stamina"] = 5f;
            bronzedagger.rigidity_rating = 3;
            bronzedagger.equipment_value = 13;
            AssetManager.items.pot_weapon_assets_all.Add(bronzedagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(bronzedagger);
	    //Silver Dagger
            EquipmentAsset silverdagger = AssetManager.items.clone("silver_dagger", "$sword");
            silverdagger.needs_to_be_explored = false;
            silverdagger.metallic = true;
            silverdagger.equipment_subtype = null;
            silverdagger.material = "silver";
            silverdagger.group_id = "Daggers";
            silverdagger.translation_key = "silver_dagger_locale";
            silverdagger.path_icon = "ui/icons/items/icon_silver_dagger";
            silverdagger.path_gameplay_sprite = "weapons/silver_dagger";
            silverdagger.setCost(0, "silver", 1, "none", 0);
            silverdagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            silverdagger.base_stats["damage"] = 3f;
            silverdagger.base_stats["multiplier_attack_speed"] = 0.2f;
			silverdagger.base_stats["critical_chance"] = 0.2f;
            silverdagger.base_stats["damage_range"] = 0.9f;
            silverdagger.base_stats["range"] = -0.3f;
            silverdagger.base_stats["mana"] = 15f;
            silverdagger.base_stats["stamina"] = 5f;
            silverdagger.rigidity_rating = 2;
            silverdagger.equipment_value = 18;
            AssetManager.items.pot_weapon_assets_all.Add(silverdagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(silverdagger);
	    //Gold Dagger
			EquipmentAsset golddagger = AssetManager.items.clone("gold_dagger", "$sword");
            golddagger.needs_to_be_explored = false;
            golddagger.metallic = true;
            golddagger.equipment_subtype = null;
            golddagger.material = "gold";
            golddagger.group_id = "Daggers";
            golddagger.translation_key = "gold_dagger_locale";
            golddagger.path_icon = "ui/icons/items/icon_gold_dagger";
            golddagger.path_gameplay_sprite = "weapons/gold_dagger";
            golddagger.setCost(0, "gold", 2, "common_metals", 2);
            golddagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            golddagger.base_stats["damage"] = 3f;
            golddagger.base_stats["multiplier_attack_speed"] = 0.25f;
			golddagger.base_stats["critical_chance"] = 0.25f;
            golddagger.base_stats["damage_range"] = 0.9f;
            golddagger.base_stats["range"] = -0.25f;
            golddagger.base_stats["mana"] = 10f;
            golddagger.base_stats["stamina"] = 10f;
            golddagger.rigidity_rating = 2;
            golddagger.equipment_value = 23;
            AssetManager.items.pot_weapon_assets_all.Add(golddagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(golddagger);
	    //Iron Dagger
            EquipmentAsset irondagger = AssetManager.items.clone("iron_dagger", "$sword");
            irondagger.needs_to_be_explored = false;
            irondagger.metallic = true;
            irondagger.equipment_subtype = null;
            irondagger.material = "iron";
            irondagger.group_id = "Daggers";
            irondagger.translation_key = "iron_dagger_locale";
            irondagger.path_icon = "ui/icons/items/icon_iron_dagger";
            irondagger.path_gameplay_sprite = "weapons/iron_dagger";
            irondagger.setCost(0, "common_metals", 3, "none", 0);
            irondagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            irondagger.base_stats["damage"] = 4f;
            irondagger.base_stats["multiplier_attack_speed"] = 0.25f;
			irondagger.base_stats["critical_chance"] = 0.25f;
            irondagger.base_stats["damage_range"] = 0.9f;
            irondagger.base_stats["range"] = -0.25f;
            irondagger.base_stats["stamina"] = 5f;
            irondagger.rigidity_rating = 4;
            irondagger.equipment_value = 28;
            AssetManager.items.pot_weapon_assets_all.Add(irondagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(irondagger);
	    //Steel Dagger
            EquipmentAsset steeldagger = AssetManager.items.clone("steel_dagger", "$sword");
            steeldagger.needs_to_be_explored = false;
            steeldagger.metallic = true;
            steeldagger.equipment_subtype = null;
            steeldagger.material = "steel";
            steeldagger.group_id = "Daggers";
            steeldagger.translation_key = "steel_dagger_locale";
            steeldagger.path_icon = "ui/icons/items/icon_steel_dagger";
            steeldagger.path_gameplay_sprite = "weapons/steel_dagger";
            steeldagger.setCost(0, "common_metals", 4, "none", 0);
            steeldagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            steeldagger.base_stats["damage"] = 4f;
            steeldagger.base_stats["multiplier_attack_speed"] = 0.3f;
			steeldagger.base_stats["critical_chance"] = 0.3f;
            steeldagger.base_stats["damage_range"] = 0.9f;
            steeldagger.base_stats["range"] = -0.2f;
            steeldagger.base_stats["speed"] = 1f;
            steeldagger.base_stats["stamina"] = 5f;
            steeldagger.rigidity_rating = 5;
            steeldagger.equipment_value = 38;
            AssetManager.items.pot_weapon_assets_all.Add(steeldagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(steeldagger);
	    //Mythril Dagger
            EquipmentAsset mythrildagger = AssetManager.items.clone("mythril_dagger", "$sword");
            mythrildagger.needs_to_be_explored = false;
            mythrildagger.metallic = true;
            mythrildagger.equipment_subtype = null;
            mythrildagger.material = "mythril";
            mythrildagger.group_id = "Daggers";
            mythrildagger.translation_key = "mythril_dagger_locale";
            mythrildagger.path_icon = "ui/icons/items/icon_mythril_dagger";
            mythrildagger.path_gameplay_sprite = "weapons/mythril_dagger";
            mythrildagger.setCost(0, "mythril", 1, "none", 0);
            mythrildagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            mythrildagger.base_stats["damage"] = 5f; 
            mythrildagger.base_stats["multiplier_attack_speed"] = 0.3f;
			mythrildagger.base_stats["critical_chance"] = 0.3f;
            mythrildagger.base_stats["damage_range"] = 0.9f;
            mythrildagger.base_stats["range"] = -0.2f;
            mythrildagger.base_stats["mana"] = 25f;
            mythrildagger.base_stats["stamina"] = 10f;
            mythrildagger.rigidity_rating = 6;
            mythrildagger.equipment_value = 48;
            AssetManager.items.pot_weapon_assets_all.Add(mythrildagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(mythrildagger);
	    //Adamantine Dagger
            EquipmentAsset adamantinedagger = AssetManager.items.clone("adamantine_dagger", "$sword");
            adamantinedagger.needs_to_be_explored = false;
            adamantinedagger.metallic = true;
            adamantinedagger.equipment_subtype = null;
            adamantinedagger.material = "adamantine";
            adamantinedagger.group_id = "Daggers";
            adamantinedagger.translation_key = "adamantine_dagger_locale";
            adamantinedagger.path_icon = "ui/icons/items/icon_adamantine_dagger";
            adamantinedagger.path_gameplay_sprite = "weapons/adamantine_dagger";
            adamantinedagger.setCost(0, "adamantine", 1, "none", 0);
            adamantinedagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"initiative"});
            adamantinedagger.base_stats["damage"] = 5f;
            adamantinedagger.base_stats["multiplier_attack_speed"] = 0.35f;
			adamantinedagger.base_stats["critical_chance"] = 0.35f;
            adamantinedagger.base_stats["damage_range"] = 0.9f;
            adamantinedagger.base_stats["range"] = -0.15f;
            adamantinedagger.base_stats["speed"] = 1f;
            adamantinedagger.base_stats["mana"] = 35f;
            adamantinedagger.base_stats["stamina"] = 10f;
            adamantinedagger.rigidity_rating = 7;
            adamantinedagger.equipment_value = 68;
            AssetManager.items.pot_weapon_assets_all.Add(adamantinedagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(adamantinedagger);
	    //Flame Dagger
            EquipmentAsset flamedagger = AssetManager.items.clone("flame_dagger", "$sword");
            flamedagger.needs_to_be_explored = false;
            flamedagger.animated = true;
            flamedagger.metallic = true;
            flamedagger.equipment_subtype = null;
            flamedagger.material = "basic";
            flamedagger.group_id = "Daggers";
            flamedagger.translation_key = "flame_dagger_locale";
            flamedagger.path_icon = "ui/icons/items/icon_flame_dagger";
            flamedagger.path_gameplay_sprite = "weapons/flame_dagger";
            flamedagger.setCost(15, "dragon_scales", 3, "none", 0);
            flamedagger.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flamedagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flamedagger.base_stats.addTag("building_immunity_fire");
            flamedagger.base_stats["damage"] = 16f;
            flamedagger.base_stats["multiplier_attack_speed"] = 0.4f;
			flamedagger.base_stats["critical_chance"] = 0.35f;
            flamedagger.base_stats["damage_range"] = 0.9f;
            flamedagger.base_stats["range"] = -0.1f;
            flamedagger.base_stats["mana"] = 15f;
            flamedagger.base_stats["stamina"] = 15f;
            flamedagger.rigidity_rating = 8;
            flamedagger.equipment_value = 398;
            AssetManager.items.pot_weapon_assets_all.Add(flamedagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flamedagger);
	    //Ice Dagger
            EquipmentAsset icedagger = AssetManager.items.clone("ice_dagger", "$sword");
            icedagger.needs_to_be_explored = false;
            icedagger.metallic = true;
            icedagger.equipment_subtype = null;
            icedagger.material = "basic";
            icedagger.group_id = "Daggers";
            icedagger.translation_key = "ice_dagger_locale";
            icedagger.path_icon = "ui/icons/items/icon_ice_dagger";
            icedagger.path_gameplay_sprite = "weapons/ice_dagger";
            icedagger.setCost(10, "mythril", 10, "gems", 2);
            icedagger.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icedagger.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icedagger.base_stats["damage"] = 12f;
            icedagger.base_stats["multiplier_attack_speed"] = 0.45f;
			icedagger.base_stats["critical_chance"] = 0.35f;
            icedagger.base_stats["damage_range"] = 0.9f;
            icedagger.base_stats["range"] = -0.1f;
            icedagger.base_stats["mana"] = 15f;
            icedagger.base_stats["stamina"] = 15f;
            icedagger.rigidity_rating = 8;
            icedagger.equipment_value = 398;
            AssetManager.items.pot_weapon_assets_all.Add(icedagger);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icedagger);
        //Crossbows---------------------------------------------------------------------------------------------------------
        //Crossbow Lovers culture trait
            CultureTrait crossbowlovers = AssetManager.culture_traits.clone("Crossbow Lovers", "bow_lovers");
            crossbowlovers.needs_to_be_explored = false;
            crossbowlovers.path_icon = "ui/icons/iconCrossbowLovers";
            crossbowlovers.related_weapon_subtype_ids.Add("crossbow");
            crossbowlovers.related_weapon_subtype_ids.Remove("bow");
            crossbowlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){crossbowlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_fox"));}
        //Crossbow Bolt projectile
            ProjectileAsset bolt = AssetManager.projectiles.clone("bolt", "arrow");
            bolt.texture = "bolt";
            bolt.texture_shadow = "shadows/projectiles/bolt_shadow";
        //Wood Crossbow
            EquipmentAsset woodcrossbow = AssetManager.items.clone("wood_crossbow", "$bow");
            woodcrossbow.needs_to_be_explored = false;
            woodcrossbow.metallic = false;
            woodcrossbow.equipment_subtype = null;
            woodcrossbow.material = "wood";
            woodcrossbow.group_id = "Crossbows";
            woodcrossbow.projectile = "bolt";
            woodcrossbow.translation_key = "wood_crossbow_locale";
            woodcrossbow.path_icon = "ui/icons/items/icon_wood_crossbow";
            woodcrossbow.path_gameplay_sprite = "weapons/wood_crossbow";
            woodcrossbow.setCost(0, "wood", 1, "none", 0);
            woodcrossbow.base_stats["damage"] = 2f;
            woodcrossbow.base_stats["multiplier_attack_speed"] = -0.6f;
            woodcrossbow.base_stats["accuracy"] = 0.1f;
            woodcrossbow.base_stats["range"] = 4f;
			woodcrossbow.base_stats["stamina"] = 15f;
            woodcrossbow.rigidity_rating = 1;
            woodcrossbow.equipment_value = 1;
            AssetManager.items.pot_weapon_assets_all.Add(woodcrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(woodcrossbow);
        //Copper Crossbow
            EquipmentAsset coppercrossbow = AssetManager.items.clone("copper_crossbow", "$bow");
            coppercrossbow.needs_to_be_explored = false;
            coppercrossbow.metallic = true;
            coppercrossbow.equipment_subtype = null;
            coppercrossbow.material = "copper";
            coppercrossbow.group_id = "Crossbows";
            coppercrossbow.projectile = "bolt";
            coppercrossbow.translation_key = "copper_crossbow_locale";
            coppercrossbow.path_icon = "ui/icons/items/icon_copper_crossbow";
            coppercrossbow.path_gameplay_sprite = "weapons/copper_crossbow";
            coppercrossbow.setCost(0, "common_metals", 1, "none", 0);
            coppercrossbow.base_stats["damage"] = 6f;
            coppercrossbow.base_stats["multiplier_attack_speed"] = -0.55f;
            coppercrossbow.base_stats["accuracy"] = 0.2f;
            coppercrossbow.base_stats["range"] = 4f;
			coppercrossbow.base_stats["stamina"] = 10f;
            coppercrossbow.rigidity_rating = 2;
            coppercrossbow.equipment_value = 10;
            AssetManager.items.pot_weapon_assets_all.Add(coppercrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(coppercrossbow);
        //Bronze Crossbow
            EquipmentAsset bronzecrossbow = AssetManager.items.clone("bronze_crossbow", "$bow");
            bronzecrossbow.needs_to_be_explored = false;
            bronzecrossbow.metallic = true;
            bronzecrossbow.equipment_subtype = null;
            bronzecrossbow.material = "bronze";
            bronzecrossbow.group_id = "Crossbows";
            bronzecrossbow.projectile = "bolt";
            bronzecrossbow.translation_key = "bronze_crossbow_locale";
            bronzecrossbow.path_icon = "ui/icons/items/icon_bronze_crossbow";
            bronzecrossbow.path_gameplay_sprite = "weapons/bronze_crossbow";
            bronzecrossbow.setCost(0, "common_metals", 2, "none", 0);
            bronzecrossbow.base_stats["damage"] = 8f;
            bronzecrossbow.base_stats["multiplier_attack_speed"] = -0.5f;
            bronzecrossbow.base_stats["accuracy"] = 0.2f;
            bronzecrossbow.base_stats["range"] = 4.5f;
			bronzecrossbow.base_stats["stamina"] = 5f;
            bronzecrossbow.rigidity_rating = 3;
            bronzecrossbow.equipment_value = 15;
            AssetManager.items.pot_weapon_assets_all.Add(bronzecrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(bronzecrossbow);
        //Silver Crossbow
            EquipmentAsset silvercrossbow = AssetManager.items.clone("silver_crossbow", "$bow");
            silvercrossbow.needs_to_be_explored = false;
            silvercrossbow.metallic = true;
            silvercrossbow.equipment_subtype = null;
            silvercrossbow.material = "silver";
            silvercrossbow.group_id = "Crossbows";
            silvercrossbow.projectile = "bolt";
            silvercrossbow.translation_key = "silver_crossbow_locale";
            silvercrossbow.path_icon = "ui/icons/items/icon_silver_crossbow";
            silvercrossbow.path_gameplay_sprite = "weapons/silver_crossbow";
            silvercrossbow.setCost(0, "silver", 1, "none", 0);
            silvercrossbow.base_stats["damage"] = 10f;
            silvercrossbow.base_stats["multiplier_attack_speed"] = -0.5f;
            silvercrossbow.base_stats["accuracy"] = 0.25f;
            silvercrossbow.base_stats["range"] = 5f;
			silvercrossbow.base_stats["mana"] = 15f;
			silvercrossbow.base_stats["stamina"] = 5f;
            silvercrossbow.rigidity_rating = 2;
            silvercrossbow.equipment_value = 20;
            AssetManager.items.pot_weapon_assets_all.Add(silvercrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(silvercrossbow);
        //Gold Crossbow
            EquipmentAsset goldcrossbow = AssetManager.items.clone("gold_crossbow", "$bow");
            goldcrossbow.needs_to_be_explored = false;
            goldcrossbow.metallic = true;
            goldcrossbow.equipment_subtype = null;
            goldcrossbow.material = "gold";
            goldcrossbow.group_id = "Crossbows";
            goldcrossbow.projectile = "bolt";
            goldcrossbow.translation_key = "gold_crossbow_locale";
            goldcrossbow.path_icon = "ui/icons/items/icon_gold_crossbow";
            goldcrossbow.path_gameplay_sprite = "weapons/gold_crossbow";
            goldcrossbow.setCost(0, "gold", 2, "common_metals", 2);
            goldcrossbow.base_stats["damage"] = 11f;
            goldcrossbow.base_stats["multiplier_attack_speed"] = -0.5f;
            goldcrossbow.base_stats["accuracy"] = 0.3f;
            goldcrossbow.base_stats["range"] = 5.5f;
			goldcrossbow.base_stats["mana"] = 10f;
            goldcrossbow.base_stats["stamina"] = 10f;
            goldcrossbow.rigidity_rating = 2;
            goldcrossbow.equipment_value = 25;
            AssetManager.items.pot_weapon_assets_all.Add(goldcrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldcrossbow);
        //Iron Crossbow
            EquipmentAsset ironcrossbow = AssetManager.items.clone("iron_crossbow", "$bow");
            ironcrossbow.needs_to_be_explored = false;
            ironcrossbow.metallic = true;
            ironcrossbow.equipment_subtype = null;
            ironcrossbow.material = "iron";
            ironcrossbow.group_id = "Crossbows";
            ironcrossbow.projectile = "bolt";
            ironcrossbow.translation_key = "iron_crossbow_locale";
            ironcrossbow.path_icon = "ui/icons/items/icon_iron_crossbow";
            ironcrossbow.path_gameplay_sprite = "weapons/iron_crossbow";
            ironcrossbow.setCost(0, "common_metals", 3, "none", 0);
            ironcrossbow.base_stats["damage"] = 12f;
            ironcrossbow.base_stats["multiplier_attack_speed"] = -0.45f;
            ironcrossbow.base_stats["critical_chance"] = 0.05f;
            ironcrossbow.base_stats["accuracy"] = 0.3f;
            ironcrossbow.base_stats["range"] = 6f;
			ironcrossbow.base_stats["stamina"] = 5f;
            ironcrossbow.rigidity_rating = 4;
            ironcrossbow.equipment_value = 30;
            AssetManager.items.pot_weapon_assets_all.Add(ironcrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(ironcrossbow);
        //Steel Crossbow
            EquipmentAsset steelcrossbow = AssetManager.items.clone("steel_crossbow", "$bow");
            steelcrossbow.needs_to_be_explored = false;
            steelcrossbow.metallic = true;
            steelcrossbow.equipment_subtype = null;
            steelcrossbow.material = "steel";
            steelcrossbow.group_id = "Crossbows";
            steelcrossbow.projectile = "bolt";
            steelcrossbow.translation_key = "steel_crossbow_locale";
            steelcrossbow.path_icon = "ui/icons/items/icon_steel_crossbow";
            steelcrossbow.path_gameplay_sprite = "weapons/steel_crossbow";
            steelcrossbow.setCost(0, "common_metals", 4, "none", 0);
            steelcrossbow.base_stats["damage"] = 14f;
            steelcrossbow.base_stats["multiplier_attack_speed"] = -0.45f;
            steelcrossbow.base_stats["critical_chance"] = 0.05f;
            steelcrossbow.base_stats["accuracy"] = 0.35f;
            steelcrossbow.base_stats["range"] = 6f;
            steelcrossbow.base_stats["speed"] = 1f;
			steelcrossbow.base_stats["stamina"] = 5f;
            steelcrossbow.rigidity_rating = 5;
            steelcrossbow.equipment_value = 40;
            AssetManager.items.pot_weapon_assets_all.Add(steelcrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(steelcrossbow);
        //Mythril Crossbow
            EquipmentAsset mythrilcrossbow = AssetManager.items.clone("mythril_crossbow", "$bow");
            mythrilcrossbow.needs_to_be_explored = false;
            mythrilcrossbow.metallic = true;
            mythrilcrossbow.equipment_subtype = null;
            mythrilcrossbow.material = "mythril";
            mythrilcrossbow.group_id = "Crossbows";
            mythrilcrossbow.projectile = "bolt";
            mythrilcrossbow.translation_key = "mythril_crossbow_locale";
            mythrilcrossbow.path_icon = "ui/icons/items/icon_mythril_crossbow";
            mythrilcrossbow.path_gameplay_sprite = "weapons/mythril_crossbow";
            mythrilcrossbow.setCost(0, "mythril", 1, "none", 0);
            mythrilcrossbow.base_stats["damage"] = 16f;
            mythrilcrossbow.base_stats["multiplier_attack_speed"] = -0.45f;
            mythrilcrossbow.base_stats["critical_chance"] = 0.05f;
            mythrilcrossbow.base_stats["accuracy"] = 0.4f;
            mythrilcrossbow.base_stats["range"] = 6.5f;
			mythrilcrossbow.base_stats["mana"] = 25f;
			mythrilcrossbow.base_stats["stamina"] = 10f;
            mythrilcrossbow.rigidity_rating = 6;
            mythrilcrossbow.equipment_value = 50;
            AssetManager.items.pot_weapon_assets_all.Add(mythrilcrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(mythrilcrossbow);
        //Adamantine Crossbow
            EquipmentAsset adamantinecrossbow = AssetManager.items.clone("adamantine_crossbow", "$bow");
            adamantinecrossbow.needs_to_be_explored = false;
            adamantinecrossbow.metallic = true;
            adamantinecrossbow.equipment_subtype = null;
            adamantinecrossbow.material = "adamantine";
            adamantinecrossbow.group_id = "Crossbows";
            adamantinecrossbow.projectile = "bolt";
            adamantinecrossbow.translation_key = "adamantine_crossbow_locale";
            adamantinecrossbow.path_icon = "ui/icons/items/icon_adamantine_crossbow";
            adamantinecrossbow.path_gameplay_sprite = "weapons/adamantine_crossbow";
            adamantinecrossbow.setCost(0, "adamantine", 1, "none", 0);
            adamantinecrossbow.base_stats["damage"] = 20f;
            adamantinecrossbow.base_stats["multiplier_attack_speed"] = -0.4f;
            adamantinecrossbow.base_stats["critical_chance"] = 0.15f;
            adamantinecrossbow.base_stats["accuracy"] = 0.5f;
            adamantinecrossbow.base_stats["range"] = 7f;
            adamantinecrossbow.base_stats["speed"] = 1f;
			adamantinecrossbow.base_stats["mana"] = 35f;
			adamantinecrossbow.base_stats["stamina"] = 10f;
            adamantinecrossbow.rigidity_rating = 7;
            adamantinecrossbow.equipment_value = 70;
            AssetManager.items.pot_weapon_assets_all.Add(adamantinecrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(adamantinecrossbow);
        //Flame Crossbow
            EquipmentAsset flamecrossbow = AssetManager.items.clone("flame_crossbow", "$range");
            flamecrossbow.needs_to_be_explored = false;
            flamecrossbow.animated = true;
            flamecrossbow.is_pool_weapon = true;
            flamecrossbow.metallic = true;
            flamecrossbow.group_id = "Crossbows";
            flamecrossbow.projectile = "bolt";
            flamecrossbow.translation_key = "flame_crossbow_locale";
            flamecrossbow.path_icon = "ui/icons/items/icon_flame_crossbow";
            flamecrossbow.path_gameplay_sprite = "weapons/flame_crosssbow";
            flamecrossbow.path_slash_animation = "effects/slashes/slash_bow";
            flamecrossbow.setCost(15, "dragon_scales", 3, "none", 0);
            flamecrossbow.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flamecrossbow.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flamecrossbow.base_stats.addTag("building_immunity_fire");
            flamecrossbow.base_stats["damage"] = 36f;
            flamecrossbow.base_stats["multiplier_attack_speed"] = -0.35f;
            flamecrossbow.base_stats["critical_chance"] = 0.25f;
            flamecrossbow.base_stats["critical_damage_multiplier"] = 0.6f;
            flamecrossbow.base_stats["accuracy"] = 0.6f;
            flamecrossbow.base_stats["range"] = 8f;
            flamecrossbow.base_stats["recoil"] = 1f;
            flamecrossbow.base_stats["targets"] = 1f;
            flamecrossbow.rigidity_rating = 8;
            flamecrossbow.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flamecrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flamecrossbow);
        //Ice Crossbow
            EquipmentAsset icecrossbow = AssetManager.items.clone("ice_crossbow", "$range");
            icecrossbow.needs_to_be_explored = false;
            icecrossbow.is_pool_weapon = true;
            icecrossbow.metallic = true;
            icecrossbow.group_id = "Crossbows";
            icecrossbow.projectile = "bolt";
            icecrossbow.translation_key = "ice_crossbow_locale";
            icecrossbow.path_icon = "ui/icons/items/icon_ice_crossbow";
            icecrossbow.path_gameplay_sprite = "weapons/ice_crossbow";
            icecrossbow.path_slash_animation = "effects/slashes/slash_bow";
            icecrossbow.setCost(15, "mythril", 10, "gems", 2);            
            icecrossbow.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icecrossbow.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icecrossbow.base_stats["damage"] = 30f;
            icecrossbow.base_stats["multiplier_attack_speed"] = -0.3f;
            icecrossbow.base_stats["critical_chance"] = 0.25f;
            icecrossbow.base_stats["critical_damage_multiplier"] = 0.5f;
            icecrossbow.base_stats["accuracy"] = 0.6f;
			icecrossbow.base_stats["range"] = 8f;
            icecrossbow.base_stats["recoil"] = 0.8f;
            icecrossbow.base_stats["damage_range"] = 0.6f;
            icecrossbow.rigidity_rating = 8;
            icecrossbow.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(icecrossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icecrossbow);
        //Slingshots---------------------------------------------------------------------------------------------------------
        //Slingshot Lovers culture trait
            CultureTrait slingshotlovers = AssetManager.culture_traits.clone("Slingshot Lovers", "bow_lovers");
            slingshotlovers.needs_to_be_explored = false;
            slingshotlovers.path_icon = "ui/icons/iconSlingshotLovers";
            slingshotlovers.related_weapon_subtype_ids.Add("slingshot");
            slingshotlovers.related_weapon_subtype_ids.Remove("bow");
            slingshotlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){slingshotlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_scorpion"));}
        //Slingshot Stone projectile
            ProjectileAsset pellet = AssetManager.projectiles.clone("pellet", "rock");
            pellet.texture = "pellet";
            pellet.texture_shadow = "shadows/projectiles/pellet_shadow";
        //Wood Slingshot
            EquipmentAsset woodslingshot = AssetManager.items.clone("wood_slingshot", "$bow");
            woodslingshot.needs_to_be_explored = false;
            woodslingshot.metallic = false;
            woodslingshot.equipment_subtype = null;
            woodslingshot.material = "wood";
            woodslingshot.group_id = "Slingshots";
            woodslingshot.projectile = "pellet";
            woodslingshot.translation_key = "wood_slingshot_locale";
            woodslingshot.path_icon = "ui/icons/items/icon_wood_slingshot";
            woodslingshot.path_gameplay_sprite = "weapons/wood_slingshot";
            woodslingshot.path_slash_animation = "effects/slashes/slash_talk";
            woodslingshot.setCost(0, "wood", 1, "none", 0);
            woodslingshot.base_stats["damage"] = 1f;
            woodslingshot.base_stats["multiplier_attack_speed"] = 0.1f;
            woodslingshot.base_stats["critical_damage_multiplier"] = 0f;
            woodslingshot.base_stats["accuracy"] = -0.15f;
            woodslingshot.base_stats["range"] = 1f;
            woodslingshot.base_stats["recoil"] = 0.5f;
            woodslingshot.base_stats["stamina"] = 15f;
            woodslingshot.base_stats["damage_range"] = 0.4f;
            woodslingshot.base_stats["projectiles"] = 1f;
            woodslingshot.rigidity_rating = 1;
            woodslingshot.equipment_value = 1;
            AssetManager.items.pot_weapon_assets_all.Add(woodslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(woodslingshot);
        //Copper Slingshot
            EquipmentAsset copperslingshot = AssetManager.items.clone("copper_slingshot", "$bow");
            copperslingshot.needs_to_be_explored = false;
            copperslingshot.metallic = true;
            copperslingshot.equipment_subtype = null;
            copperslingshot.material = "copper";
            copperslingshot.group_id = "Slingshots";
            copperslingshot.projectile = "pellet";
            copperslingshot.translation_key = "copper_slingshot_locale";
            copperslingshot.path_icon = "ui/icons/items/icon_copper_slingshot";
            copperslingshot.path_gameplay_sprite = "weapons/copper_slingshot";
            copperslingshot.path_slash_animation = "effects/slashes/slash_talk";
            copperslingshot.setCost(0, "common_metals", 1, "none", 0);
            copperslingshot.base_stats["damage"] = 1f;
            copperslingshot.base_stats["multiplier_attack_speed"] = 0.1f;
            copperslingshot.base_stats["critical_damage_multiplier"] = 0f;
            copperslingshot.base_stats["accuracy"] = -0.15f;
            copperslingshot.base_stats["range"] = 1f;
            copperslingshot.base_stats["recoil"] = 0.5f;
            copperslingshot.base_stats["damage_range"] = 0.4f;
            copperslingshot.base_stats["projectiles"] = 2f;
            copperslingshot.base_stats["stamina"] = 10f;
            copperslingshot.rigidity_rating = 2;
            copperslingshot.equipment_value = 10;
            AssetManager.items.pot_weapon_assets_all.Add(copperslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(copperslingshot);
        //Bronze Slingshot
            EquipmentAsset bronzeslingshot = AssetManager.items.clone("bronze_slingshot", "$bow");
            bronzeslingshot.equipment_subtype = null;
            bronzeslingshot.metallic = true;
            bronzeslingshot.needs_to_be_explored = false;
            bronzeslingshot.material = "bronze";
            bronzeslingshot.group_id = "Slingshots";
            bronzeslingshot.projectile = "pellet";
            bronzeslingshot.translation_key = "bronze_slingshot_locale";
            bronzeslingshot.path_icon = "ui/icons/items/icon_bronze_slingshot";
            bronzeslingshot.path_gameplay_sprite = "weapons/bronze_slingshot";
            bronzeslingshot.path_slash_animation = "effects/slashes/slash_talk";
            bronzeslingshot.setCost(0, "common_metals", 2, "none", 0);
            bronzeslingshot.base_stats["damage"] = 2f;
            bronzeslingshot.base_stats["multiplier_attack_speed"] = 0.15f;
            bronzeslingshot.base_stats["critical_damage_multiplier"] = 0f;
            bronzeslingshot.base_stats["accuracy"] = -0.15f;
            bronzeslingshot.base_stats["range"] = 2f;
            bronzeslingshot.base_stats["recoil"] = 0.5f;
            bronzeslingshot.base_stats["damage_range"] = 0.4f;
            bronzeslingshot.base_stats["projectiles"] = 2f;
			bronzeslingshot.base_stats["stamina"] = 5f;
            bronzeslingshot.rigidity_rating = 3;
            bronzeslingshot.equipment_value = 15;
            AssetManager.items.pot_weapon_assets_all.Add(bronzeslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(bronzeslingshot);
        //Silver Slingshot
            EquipmentAsset silverslingshot = AssetManager.items.clone("silver_slingshot", "$bow");
            silverslingshot.needs_to_be_explored = false;
            silverslingshot.metallic = true;
            silverslingshot.equipment_subtype = null;
            silverslingshot.material = "silver";
            silverslingshot.group_id = "Slingshots";
            silverslingshot.projectile = "pellet";
            silverslingshot.translation_key = "silver_slingshot_locale";
            silverslingshot.path_icon = "ui/icons/items/icon_silver_slingshot";
            silverslingshot.path_gameplay_sprite = "weapons/silver_slingshot";
            silverslingshot.path_slash_animation = "effects/slashes/slash_talk";
            silverslingshot.setCost(0, "silver", 1, "none", 0);
            silverslingshot.base_stats["damage"] = 3f;
            silverslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
            silverslingshot.base_stats["critical_damage_multiplier"] = 0f;
            silverslingshot.base_stats["accuracy"] = -0.15f;
            silverslingshot.base_stats["range"] = 2f;
            silverslingshot.base_stats["recoil"] = 0.5f;
            silverslingshot.base_stats["damage_range"] = 0.4f;
            silverslingshot.base_stats["projectiles"] = 2f;
			silverslingshot.base_stats["mana"] = 15f;
			silverslingshot.base_stats["stamina"] = 5f;
            silverslingshot.rigidity_rating = 2;
            silverslingshot.equipment_value = 20;
            AssetManager.items.pot_weapon_assets_all.Add(silverslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(silverslingshot);
        //Gold Slingshot
            EquipmentAsset goldslingshot = AssetManager.items.clone("gold_slingshot", "$bow");
            goldslingshot.needs_to_be_explored = false;
            goldslingshot.metallic = true;
            goldslingshot.equipment_subtype = null;
            goldslingshot.material = "gold";
            goldslingshot.group_id = "Slingshots";
            goldslingshot.projectile = "pellet";
            goldslingshot.translation_key = "gold_slingshot_locale";
            goldslingshot.path_icon = "ui/icons/items/icon_gold_slingshot";
            goldslingshot.path_gameplay_sprite = "weapons/gold_slingshot";
            goldslingshot.path_slash_animation = "effects/slashes/slash_talk";
            goldslingshot.setCost(0, "gold", 2, "common_metals", 2);
            goldslingshot.base_stats["damage"] = 3f;
            goldslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
            goldslingshot.base_stats["critical_damage_multiplier"] = 0f;
            goldslingshot.base_stats["accuracy"] = -0.15f;
            goldslingshot.base_stats["range"] = 3f;
            goldslingshot.base_stats["recoil"] = 0.5f;
            goldslingshot.base_stats["damage_range"] = 0.4f;
            goldslingshot.base_stats["projectiles"] = 3f;
			goldslingshot.base_stats["mana"] = 10f;
            goldslingshot.base_stats["stamina"] = 10f;
            goldslingshot.rigidity_rating = 2;
            goldslingshot.equipment_value = 25;
            AssetManager.items.pot_weapon_assets_all.Add(goldslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldslingshot);
        //Iron Slingshot
            EquipmentAsset ironslingshot = AssetManager.items.clone("iron_slingshot", "$bow");
            ironslingshot.needs_to_be_explored = false;
            ironslingshot.metallic = true;
            ironslingshot.equipment_subtype = null;
            ironslingshot.material = "iron";
            ironslingshot.group_id = "Slingshots";
            ironslingshot.projectile = "pellet";
            ironslingshot.translation_key = "iron_slingshot_locale";
            ironslingshot.path_icon = "ui/icons/items/icon_iron_slingshot";
            ironslingshot.path_gameplay_sprite = "weapons/iron_slingshot";
            ironslingshot.path_slash_animation = "effects/slashes/slash_talk";
            ironslingshot.setCost(0, "common_metals", 3, "none", 0);
            ironslingshot.base_stats["damage"] = 4f;
            ironslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
            ironslingshot.base_stats["critical_damage_multiplier"] = 0f;
            ironslingshot.base_stats["accuracy"] = -0.15f;
            ironslingshot.base_stats["range"] = 3f;
            ironslingshot.base_stats["recoil"] = 0.5f;
            ironslingshot.base_stats["damage_range"] = 0.4f;
            ironslingshot.base_stats["projectiles"] = 3f;
			ironslingshot.base_stats["stamina"] = 5f;
            ironslingshot.rigidity_rating = 4;
            ironslingshot.equipment_value = 30;
            AssetManager.items.pot_weapon_assets_all.Add(ironslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(ironslingshot);
        //Steel Slingshot
            EquipmentAsset steelslingshot = AssetManager.items.clone("steel_slingshot", "$bow");
            steelslingshot.needs_to_be_explored = false;
            steelslingshot.metallic = true;
            steelslingshot.equipment_subtype = null;
            steelslingshot.material = "steel";
            steelslingshot.group_id = "Slingshots";
            steelslingshot.projectile = "pellet";
            steelslingshot.translation_key = "steel_slingshot_locale";
            steelslingshot.path_icon = "ui/icons/items/icon_steel_slingshot";
            steelslingshot.path_gameplay_sprite = "weapons/steel_slingshot";
            steelslingshot.path_slash_animation = "effects/slashes/slash_talk";
            steelslingshot.setCost(0, "common_metals", 4, "none", 0);
            steelslingshot.base_stats["damage"] = 4f;
            steelslingshot.base_stats["multiplier_attack_speed"] = 0.2f;
            steelslingshot.base_stats["critical_damage_multiplier"] = 0f;
            steelslingshot.base_stats["accuracy"] = -0.1f;
            steelslingshot.base_stats["range"] = 4f;
            steelslingshot.base_stats["recoil"] = 0.5f;
            steelslingshot.base_stats["damage_range"] = 0.4f;
            steelslingshot.base_stats["projectiles"] = 3f;
            steelslingshot.base_stats["speed"] = 1f;
			steelslingshot.base_stats["stamina"] = 5f;
            steelslingshot.rigidity_rating = 5;
            steelslingshot.equipment_value = 40;
            AssetManager.items.pot_weapon_assets_all.Add(steelslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(steelslingshot);
        //Mythril Slingshot
            EquipmentAsset mythrilslingshot = AssetManager.items.clone("mythril_slingshot", "$bow");
            mythrilslingshot.needs_to_be_explored = false;
            mythrilslingshot.metallic = true;
            mythrilslingshot.equipment_subtype = null;
            mythrilslingshot.material = "mythril";
            mythrilslingshot.group_id = "Slingshots";
            mythrilslingshot.projectile = "pellet";
            mythrilslingshot.translation_key = "mythril_slingshot_locale";
            mythrilslingshot.path_icon = "ui/icons/items/icon_mythril_slingshot";
            mythrilslingshot.path_gameplay_sprite = "weapons/mythril_slingshot";
            mythrilslingshot.path_slash_animation = "effects/slashes/slash_talk";
            mythrilslingshot.setCost(0, "mythril", 1, "none", 0);
            mythrilslingshot.base_stats["damage"] = 4f;
            mythrilslingshot.base_stats["multiplier_attack_speed"] = 0.25f;
            mythrilslingshot.base_stats["critical_damage_multiplier"] = 0f;
            mythrilslingshot.base_stats["accuracy"] = -0.1f;
            mythrilslingshot.base_stats["range"] = 4f;
            mythrilslingshot.base_stats["recoil"] = 0.5f;
            mythrilslingshot.base_stats["damage_range"] = 0.4f;
            mythrilslingshot.base_stats["projectiles"] = 3f;
			mythrilslingshot.base_stats["mana"] = 25f;
			mythrilslingshot.base_stats["stamina"] = 10f;
            mythrilslingshot.rigidity_rating = 6;
            mythrilslingshot.equipment_value = 50;
            AssetManager.items.pot_weapon_assets_all.Add(mythrilslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(mythrilslingshot);
        //Adamantine Slingshot
            EquipmentAsset adamantineslingshot = AssetManager.items.clone("adamantine_slingshot", "$bow");
            adamantineslingshot.needs_to_be_explored = false;
            adamantineslingshot.metallic = true;
            adamantineslingshot.equipment_subtype = null;
            adamantineslingshot.material = "adamantine";
            adamantineslingshot.group_id = "Slingshots";
            adamantineslingshot.projectile = "pellet";
            adamantineslingshot.translation_key = "adamantine_slingshot_locale";
            adamantineslingshot.path_icon = "ui/icons/items/icon_adamantine_slingshot";
            adamantineslingshot.path_gameplay_sprite = "weapons/adamantine_slingshot";
            adamantineslingshot.path_slash_animation = "effects/slashes/slash_talk";
            adamantineslingshot.setCost(0, "adamantine", 1, "none", 0);
            adamantineslingshot.base_stats["damage"] = 4f;
            adamantineslingshot.base_stats["multiplier_attack_speed"] = 0.25f;
            adamantineslingshot.base_stats["critical_damage_multiplier"] = 0f;
            adamantineslingshot.base_stats["accuracy"] = -0.1f;
            adamantineslingshot.base_stats["range"] = 5f;
            adamantineslingshot.base_stats["recoil"] = 0.5f;
            adamantineslingshot.base_stats["damage_range"] = 0.4f;
            adamantineslingshot.base_stats["projectiles"] = 4f;
            adamantineslingshot.base_stats["speed"] = 1f;
			adamantineslingshot.base_stats["mana"] = 35f;
			adamantineslingshot.base_stats["stamina"] = 10f;
            adamantineslingshot.rigidity_rating = 7;
            adamantineslingshot.equipment_value = 70;
            AssetManager.items.pot_weapon_assets_all.Add(adamantineslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(adamantineslingshot);
        //Flame Slingshot
            EquipmentAsset flameslingshot = AssetManager.items.clone("flame_slingshot", "$range");
            flameslingshot.needs_to_be_explored = false;
            flameslingshot.animated = true;
            flameslingshot.is_pool_weapon = true;
            flameslingshot.metallic = true;
            flameslingshot.group_id = "Slingshots";
            flameslingshot.projectile = "pellet";
            flameslingshot.translation_key = "flame_slingshot_locale";
            flameslingshot.path_icon = "ui/icons/items/icon_flame_slingshot";
            flameslingshot.path_gameplay_sprite = "weapons/flame_slingshot";
            flameslingshot.path_slash_animation = "effects/slashes/slash_talk";
            flameslingshot.setCost(15, "dragon_scales", 3, "none", 0);
            flameslingshot.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flameslingshot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flameslingshot.base_stats.addTag("building_immunity_fire");
            flameslingshot.base_stats["damage"] = 5f;
            flameslingshot.base_stats["multiplier_attack_speed"] = 0.25f;
            flameslingshot.base_stats["critical_damage_multiplier"] = 0.1f;
            flameslingshot.base_stats["accuracy"] = -0.1f;
            flameslingshot.base_stats["range"] = 6f;
            flameslingshot.base_stats["recoil"] = 0.5f;
            flameslingshot.base_stats["damage_range"] = 0.4f;
            flameslingshot.base_stats["projectiles"] = 4f;
            flameslingshot.rigidity_rating = 8;
            flameslingshot.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flameslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flameslingshot);
        //Ice Slingshot
            EquipmentAsset iceslingshot = AssetManager.items.clone("ice_slingshot", "$range");
            iceslingshot.needs_to_be_explored = false;
            iceslingshot.is_pool_weapon = true;
            iceslingshot.metallic = true;
            iceslingshot.group_id = "Slingshots";
            iceslingshot.projectile = "pellet";
            iceslingshot.translation_key = "ice_slingshot_locale";
            iceslingshot.path_icon = "ui/icons/items/icon_ice_slingshot";
            iceslingshot.path_gameplay_sprite = "weapons/ice_slingshot";
            iceslingshot.path_slash_animation = "effects/slashes/slash_talk";
            iceslingshot.setCost(15, "mythril", 10, "gems", 2);            
            iceslingshot.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            iceslingshot.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            iceslingshot.base_stats["damage"] = 4f;
            iceslingshot.base_stats["multiplier_attack_speed"] = 0.3f;
            iceslingshot.base_stats["critical_damage_multiplier"] = 0f;
            iceslingshot.base_stats["accuracy"] = -0.1f;
            iceslingshot.base_stats["range"] = 6f;
            iceslingshot.base_stats["recoil"] = 0.4f;
            iceslingshot.base_stats["damage_range"] = 0.4f;
            iceslingshot.base_stats["projectiles"] = 4f;
            iceslingshot.rigidity_rating = 8;
            iceslingshot.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(iceslingshot);
            AssetManager.items.pot_weapon_assets_unlocked.Add(iceslingshot);
    //Item Config Adjustments
            if(ConfigsAEA.scciVal == 1){ //light
                icebludgeon.path_icon = "ui/icons/items/icon_ice_bludgeon_lightC";
                icehalberd.path_icon = "ui/icons/items/icon_ice_halberd_lightC";
                icecrossbow.path_icon = "ui/icons/items/icon_ice_crossbow_lightC";
                iceslingshot.path_icon = "ui/icons/items/icon_ice_slingshot_lightC";
            }
            if(ConfigsAEA.changeItemSprites){
                adamantinebludgeon.path_icon = "ui/icons/items/avbchanges/icon_adamantine_bludgeon";
                adamantinehalberd.path_icon = "ui/icons/items/avbchanges/icon_adamantine_halberd";
                adamantinecrossbow.path_icon = "ui/icons/items/avbchanges/icon_adamantine_crossbow";
                adamantineslingshot.path_icon = "ui/icons/items/avbchanges/icon_adamantine_slingshot";

                silverbludgeon.path_icon = "ui/icons/items/avbchanges/icon_silver_bludgeon";
                silverhalberd.path_icon = "ui/icons/items/avbchanges/icon_silver_halberd";
                silvercrossbow.path_icon = "ui/icons/items/avbchanges/icon_silver_crossbow";
                silverslingshot.path_icon = "ui/icons/items/avbchanges/icon_silver_slingshot";
            }
    //Culture Trait Implements
            if (ConfigsAEA.flameTierCraft == true){
                CultureTrait craftflame = AssetManager.culture_traits.get("craft_flame_weapon");
                if(ConfigsAEA.bludgeonCraft == true){craftflame.related_weapons_ids.Add("flame_bludgeon");}
                if(ConfigsAEA.halberdCraft == true){craftflame.related_weapons_ids.Add("flame_halberd");}
                if(ConfigsAEA.crossbowCraft == true){craftflame.related_weapons_ids.Add("flame_crossbow");}
                if(ConfigsAEA.slingshotCraft == true){craftflame.related_weapons_ids.Add("flame_slingshot");}
            }
            if (ConfigsAEA.iceTierCraft == true){
                CultureTrait craftice = AssetManager.culture_traits.get("craft_ice_weapon");
                if(ConfigsAEA.bludgeonCraft == true){craftice.related_weapons_ids.Add("ice_bludgeon");}
                if(ConfigsAEA.halberdCraft == true){craftice.related_weapons_ids.Add("ice_halberd");}
                if(ConfigsAEA.crossbowCraft == true){craftice.related_weapons_ids.Add("ice_crossbow");}
                if(ConfigsAEA.slingshotCraft == true){craftice.related_weapons_ids.Add("ice_slingshot");}
            }
    //Items Lists
        //Weapons-------------------------------------------------------------------------------
        //Subtype, used for deciding how to craft items (craftItem)
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("customItem")){AssetManager.items.equipment_by_subtypes.Add("customItem", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("sword")){AssetManager.items.equipment_by_subtypes.Add("sword", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("axe")){AssetManager.items.equipment_by_subtypes.Add("axe", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("hammer")){AssetManager.items.equipment_by_subtypes.Add("hammer", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("spear")){AssetManager.items.equipment_by_subtypes.Add("spear", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("bow")){AssetManager.items.equipment_by_subtypes.Add("bow", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("bludgeon")){AssetManager.items.equipment_by_subtypes.Add("bludgeon", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("halberd")){AssetManager.items.equipment_by_subtypes.Add("halberd", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("dagger")){AssetManager.items.equipment_by_subtypes.Add("dagger", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("crossbow")){AssetManager.items.equipment_by_subtypes.Add("crossbow", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("slingshot")){AssetManager.items.equipment_by_subtypes.Add("slingshot", new List<EquipmentAsset>());}
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flamebludgeon);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icebludgeon);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flamehalberd);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icehalberd);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flamecrossbow);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icecrossbow);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flameslingshot);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(iceslingshot);
            FieldInfo field = typeof(ItemLibrary).GetField("default_weapon_pool", BindingFlags.Static | BindingFlags.Public);
            List<string> weaponPool = ((string[])field.GetValue(null)).ToList();
            if (ConfigsAEA.bludgeonCraft == true){
                weaponPool.Add("bludgeon");
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(woodbludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(stonebludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(copperbludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(bronzebludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(silverbludgeon);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["bludgeon"].Add(goldbludgeon);}
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(ironbludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(steelbludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(mythrilbludgeon);
                AssetManager.items.equipment_by_subtypes["bludgeon"].Add(adamantinebludgeon);
            }
            else {
                bludgeonlovers.related_weapon_subtype_ids.Remove("bludgeon");
            }
            if (ConfigsAEA.halberdCraft == true){
                weaponPool.Add("halberd");
                AssetManager.items.equipment_by_subtypes["halberd"].Add(woodhalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(stonehalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(copperhalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(bronzehalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(silverhalberd);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["halberd"].Add(goldhalberd);}
                AssetManager.items.equipment_by_subtypes["halberd"].Add(ironhalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(steelhalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(mythrilhalberd);
                AssetManager.items.equipment_by_subtypes["halberd"].Add(adamantinehalberd);
            }
            else {
                halberdlovers.related_weapon_subtype_ids.Remove("halberd");
            }
            if (ConfigsAEA.daggerCraft == true){
                weaponPool.Add("dagger");
                AssetManager.items.equipment_by_subtypes["dagger"].Add(wooddagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(stonedagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(copperdagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(bronzedagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(silverdagger);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["dagger"].Add(golddagger);}
                AssetManager.items.equipment_by_subtypes["dagger"].Add(irondagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(steeldagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(mythrildagger);
                AssetManager.items.equipment_by_subtypes["dagger"].Add(adamantinedagger);
            }
            else {
                daggerlovers.related_weapon_subtype_ids.Remove("dagger");
            }
            if (ConfigsAEA.crossbowCraft == true){
                weaponPool.Add("crossbow");
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(woodcrossbow);
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(coppercrossbow);
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(bronzecrossbow);
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(silvercrossbow);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["crossbow"].Add(goldcrossbow);}
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(ironcrossbow);
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(steelcrossbow);
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(mythrilcrossbow);
                AssetManager.items.equipment_by_subtypes["crossbow"].Add(adamantinecrossbow);
            }
            else {
                crossbowlovers.related_weapon_subtype_ids.Remove("crossbow");
            }
            if (ConfigsAEA.slingshotCraft == true){
                weaponPool.Add("slingshot");
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(woodslingshot);
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(copperslingshot);
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(bronzeslingshot);
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(silverslingshot);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["slingshot"].Add(goldslingshot);}
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(ironslingshot);
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(steelslingshot);
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(mythrilslingshot);
                AssetManager.items.equipment_by_subtypes["slingshot"].Add(adamantineslingshot);
            }
            else {
                slingshotlovers.related_weapon_subtype_ids.Remove("slingshot");
            }
            if (ConfigsAEA.swordCraft == false){
                CultureTrait swordlovers = AssetManager.culture_traits.get("sword_lovers");
                swordlovers.related_weapon_subtype_ids.Remove("sword");
                weaponPool.Remove("sword");
            }
            if (ConfigsAEA.axeCraft == false){
                CultureTrait axelovers = AssetManager.culture_traits.get("axe_lovers");
                axelovers.related_weapon_subtype_ids.Remove("axe");
                weaponPool.Remove("axe");
            }
            if (ConfigsAEA.hammerCraft == false){
                CultureTrait hammerlovers = AssetManager.culture_traits.get("hammer_lovers");
                hammerlovers.related_weapon_subtype_ids.Remove("hammer");
                weaponPool.Remove("hammer");
            }
            if (ConfigsAEA.spearCraft == false){
                CultureTrait spearlovers = AssetManager.culture_traits.get("spear_lovers");
                spearlovers.related_weapon_subtype_ids.Remove("spear");
                weaponPool.Remove("spear");
            }
            if (ConfigsAEA.bowCraft == false){
                CultureTrait bowlovers = AssetManager.culture_traits.get("bow_lovers");
                bowlovers.related_weapon_subtype_ids.Remove("bow");
                weaponPool.Remove("bow");
            }
            if (ConfigsAEA.stickCraft == false){weaponPool.Remove("stick");}
            field.SetValue(null, weaponPool.ToArray());
        }

        public static bool addInitiativeEffect(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null){
            if (pTarget.isAlive() && !pTarget.isBuilding() && pTarget.a.getHealthRatio() > 0.85f){
                float weaponDamage = pSelf.a.equipment.weapon.getItem().getAsset().base_stats["damage"];
                pTarget.a.getHit(weaponDamage * 2, false, AttackType.Other, null, false, true, true);
                return true;
            }
            return false;
        }
    }
}
