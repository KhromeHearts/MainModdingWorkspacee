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
    class BasicGearItems : MonoBehaviour{
        private static List<CultureTrait> aeaCultureTraitsList = new();

        public static void init(){
            loadCustomGroupsAndTraits();
            populateListOppositeCultureTraits();
        }

        public static void loadCustomGroupsAndTraits(){
            int targetIndex = 0; //used for category/item positioning
    //Groups Addded
            ItemGroupAsset headbands = new ItemGroupAsset(){
               id = "headband", //needs to match subtype for transmutation
               name = "equipment_group_headband",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){headbands.color = "#bad0ff";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "helmet");
            AssetManager.item_groups.list.Remove(headbands);
            AssetManager.item_groups.list.Insert(targetIndex + 1, headbands);
            ItemGroupAsset cloaks = new ItemGroupAsset(){
               id = "cloak",
               name = "equipment_group_cloak",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){cloaks.color = "#bad0ff";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "armor");
            AssetManager.item_groups.list.Remove(cloaks);
            AssetManager.item_groups.list.Insert(targetIndex + 1, cloaks);
            ItemGroupAsset highcleats = new ItemGroupAsset(){
               id = "highcleats",
               name = "equipment_group_highcleats",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){highcleats.color = "#bad0ff";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "boots");
            AssetManager.item_groups.list.Remove(highcleats);
            AssetManager.item_groups.list.Insert(targetIndex + 1, highcleats);
            ItemGroupAsset earrings = new ItemGroupAsset(){
               id = "earrings",
               name = "equipment_group_earrings",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){earrings.color = "#baffdf";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "ring");
            AssetManager.item_groups.list.Remove(earrings);
            AssetManager.item_groups.list.Insert(targetIndex + 1, earrings);
            ItemGroupAsset bracelet = new ItemGroupAsset(){
               id = "bracelet",
               name = "equipment_group_bracelet",
               color = "#BAFFC2"
            };
            if (ConfigsAEA.categoryColor == true){bracelet.color = "#baffdf";}
            targetIndex = AssetManager.item_groups.list.FindIndex(a => a.id == "amulet");
            AssetManager.item_groups.list.Remove(bracelet);
            AssetManager.item_groups.list.Insert(targetIndex + 1, bracelet);
            CultureTraitGroupAsset gear = new CultureTraitGroupAsset(){
               id = "Gear",
               name = "trait_group_gear",
               color = "#FF6B86"
            };
            targetIndex = AssetManager.culture_trait_groups.list.FindIndex(a => a.id == "weapons");
            AssetManager.culture_trait_groups.list.Remove(gear);
            AssetManager.culture_trait_groups.list.Insert(targetIndex, gear);
    //Transmutation Groups
            List<string> newGear = new List<string> { "headband", "cloak", "highcleats", "earrings", "bracelet" };
            foreach (string gearType in newGear){
                if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey(gearType)){AssetManager.items.pot_equipment_by_groups_all.Add(gearType, new List<EquipmentAsset>());}
                if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey(gearType)){AssetManager.items.pot_equipment_by_groups_unlocked.Add(gearType, new List<EquipmentAsset>());}
            }
    //Items Added
	    //Headbands---------------------------------------------------------------------------------------------------------
        //Headband & Helmet Lovers culture trait
            CultureTrait headbandlovers = AssetManager.culture_traits.clone("Headband Lovers", "sword_lovers");
            headbandlovers.needs_to_be_explored = false;
            headbandlovers.group_id = "Gear";
            headbandlovers.path_icon = "ui/icons/iconHeadbandLovers";
            headbandlovers.priority = 99;
            headbandlovers.related_weapon_subtype_ids.Remove("sword");
            headbandlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){headbandlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("bandit"));}
            headbandlovers.addOpposite("Helmet Lovers");
            reAddToCultureTraitLists(headbandlovers);
            CultureTrait helmetlovers = AssetManager.culture_traits.clone("Helmet Lovers", "sword_lovers");
            helmetlovers.needs_to_be_explored = false;
            helmetlovers.group_id = "Gear";
            helmetlovers.path_icon = "ui/icons/iconHelmetLovers";
            helmetlovers.priority = 100;
            helmetlovers.related_weapon_subtype_ids.Remove("sword");
            helmetlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){helmetlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_lemon_man"));}
            helmetlovers.addOpposite("Headband Lovers");
            reAddToCultureTraitLists(helmetlovers);
	    //Leather Headband
            EquipmentAsset leatherheadband = AssetManager.items.clone("leather_headband", "$helmet");
            leatherheadband.needs_to_be_explored = false;
            leatherheadband.metallic = false;
            leatherheadband.equipment_subtype = null;
            leatherheadband.material = "leather";
            leatherheadband.group_id = "headband";
            leatherheadband.translation_key = "leather_headband_locale";
            leatherheadband.path_icon = "ui/icons/items/icon_leather_headband";
            leatherheadband.setCost(0, "leather", 1, "none", 0);
            leatherheadband.base_stats["armor"] = 1f;
            leatherheadband.base_stats["multiplier_attack_speed"] = 0.01f;
            leatherheadband.base_stats["speed"] = 5f;
            leatherheadband.base_stats["stamina"] = 10f;
            leatherheadband.rigidity_rating = 1;
            leatherheadband.equipment_value = 1;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(leatherheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(leatherheadband);
	    //Copper Headband
            EquipmentAsset copperheadband = AssetManager.items.clone("copper_headband", "$helmet");
            copperheadband.needs_to_be_explored = false;
            copperheadband.metallic = true;
            copperheadband.equipment_subtype = null;
            copperheadband.material = "copper";
            copperheadband.group_id = "headband";
            copperheadband.translation_key = "copper_headband_locale";
            copperheadband.path_icon = "ui/icons/items/icon_copper_headband";
            copperheadband.setCost(0, "common_metals", 1, "none", 0);
            copperheadband.base_stats["armor"] = 1f; 
            copperheadband.base_stats["multiplier_attack_speed"] = 0.01f;
            copperheadband.base_stats["speed"] = 5f;
            copperheadband.base_stats["stamina"] = 5f;
            copperheadband.rigidity_rating = 2;
            copperheadband.equipment_value = 10;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(copperheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(copperheadband);
	    //Bronze Headband
            EquipmentAsset bronzeheadband = AssetManager.items.clone("bronze_headband", "$helmet");
            bronzeheadband.needs_to_be_explored = false;
            bronzeheadband.metallic = true;
            bronzeheadband.equipment_subtype = null;
            bronzeheadband.material = "bronze";
            bronzeheadband.group_id = "headband";
            bronzeheadband.translation_key = "bronze_headband_locale";
            bronzeheadband.path_icon = "ui/icons/items/icon_bronze_headband";
            bronzeheadband.setCost(0, "common_metals", 2, "none", 0);
            bronzeheadband.base_stats["armor"] = 2f;
            bronzeheadband.base_stats["multiplier_attack_speed"] = 0.02f;
            bronzeheadband.base_stats["speed"] = 8f; 
            bronzeheadband.base_stats["stamina"] = 5f;
            bronzeheadband.rigidity_rating = 3;
            bronzeheadband.equipment_value = 15;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(bronzeheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(bronzeheadband);
	    //Silver Headband
            EquipmentAsset silverheadband = AssetManager.items.clone("silver_headband", "$helmet");
            silverheadband.needs_to_be_explored = false;
            silverheadband.metallic = true;
            silverheadband.equipment_subtype = null;
            silverheadband.material = "silver";
            silverheadband.group_id = "headband";
            silverheadband.translation_key = "silver_headband_locale";
            silverheadband.path_icon = "ui/icons/items/icon_silver_headband";
            silverheadband.setCost(0, "silver", 1, "none", 0);
            silverheadband.base_stats["armor"] = 2f;
            silverheadband.base_stats["multiplier_attack_speed"] = 0.02f;
            silverheadband.base_stats["speed"] = 10f;
            silverheadband.base_stats["mana"] = 15f;
            silverheadband.base_stats["stamina"] = 5f;
            silverheadband.rigidity_rating = 2;
            silverheadband.equipment_value = 20;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(silverheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(silverheadband);
	    //Gold Headband
			EquipmentAsset goldheadband = AssetManager.items.clone("gold_headband", "$helmet");
            goldheadband.needs_to_be_explored = false;
            goldheadband.metallic = true;
            goldheadband.equipment_subtype = null;
            goldheadband.material = "gold";
            goldheadband.group_id = "headband";
            goldheadband.translation_key = "gold_headband_locale";
            goldheadband.path_icon = "ui/icons/items/icon_gold_headband";
            goldheadband.setCost(0, "gold", 2, "common_metals", 2);
            goldheadband.base_stats["armor"] = 2.5f;
            goldheadband.base_stats["multiplier_attack_speed"] = 0.02f;
            goldheadband.base_stats["speed"] = 12f;
            goldheadband.base_stats["mana"] = 10f;
            goldheadband.base_stats["stamina"] = 10f;
            goldheadband.rigidity_rating = 2;
            goldheadband.equipment_value = 25;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(goldheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(goldheadband);
	    //Iron Headband
            EquipmentAsset ironheadband = AssetManager.items.clone("iron_headband", "$helmet");
            ironheadband.needs_to_be_explored = false;
            ironheadband.metallic = true;
            ironheadband.equipment_subtype = null;
            ironheadband.material = "iron";
            ironheadband.group_id = "headband";
            ironheadband.translation_key = "iron_headband_locale";
            ironheadband.path_icon = "ui/icons/items/icon_iron_headband";
            ironheadband.setCost(0, "common_metals", 3, "none", 0);
            ironheadband.base_stats["armor"] = 3f;
            ironheadband.base_stats["multiplier_attack_speed"] = 0.03f;
            ironheadband.base_stats["speed"] = 12f;
            ironheadband.base_stats["stamina"] = 5f;
            ironheadband.rigidity_rating = 4;
            ironheadband.equipment_value = 30;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(ironheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(ironheadband);
	    //Steel Headband
            EquipmentAsset steelheadband = AssetManager.items.clone("steel_headband", "$helmet");
            steelheadband.needs_to_be_explored = false;
            steelheadband.metallic = true;
            steelheadband.equipment_subtype = null;
            steelheadband.material = "steel";
            steelheadband.group_id = "headband";
            steelheadband.translation_key = "steel_headband_locale";
            steelheadband.path_icon = "ui/icons/items/icon_steel_headband";
            steelheadband.setCost(0, "common_metals", 4, "none", 0);
            steelheadband.base_stats["armor"] = 4f;
            steelheadband.base_stats["multiplier_attack_speed"] = 0.03f;
            steelheadband.base_stats["speed"] = 15f;
            steelheadband.base_stats["stamina"] = 5f;
            steelheadband.rigidity_rating = 5;
            steelheadband.equipment_value = 40;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(steelheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(steelheadband);
	    //Mythril Headband
            EquipmentAsset mythrilheadband = AssetManager.items.clone("mythril_headband", "$helmet");
            mythrilheadband.needs_to_be_explored = false;
            mythrilheadband.metallic = true;
            mythrilheadband.equipment_subtype = null;
            mythrilheadband.material = "mythril";
            mythrilheadband.group_id = "headband";
            mythrilheadband.translation_key = "mythril_headband_locale";
            mythrilheadband.path_icon = "ui/icons/items/icon_mythril_headband"; //_OLD
            mythrilheadband.setCost(0, "mythril", 1, "none", 0);
            mythrilheadband.base_stats["armor"] = 4f;
            mythrilheadband.base_stats["multiplier_attack_speed"] = 0.03f;
            mythrilheadband.base_stats["speed"] = 18f;
            mythrilheadband.base_stats["mana"] = 25f;
            mythrilheadband.base_stats["stamina"] = 10f;
            mythrilheadband.rigidity_rating = 6;
            mythrilheadband.equipment_value = 50;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(mythrilheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(mythrilheadband);
            EquipmentAsset mythrilhelmet = AssetManager.items.get("helmet_mythril");
            mythrilhelmet.path_icon = "ui/icons/items/icon_mythril_helmet_NEW";
	    //Adamantine Headband
            EquipmentAsset adamantineheadband = AssetManager.items.clone("adamantine_headband", "$helmet");
            adamantineheadband.needs_to_be_explored = false;
            adamantineheadband.metallic = true;
            adamantineheadband.equipment_subtype = null;
            adamantineheadband.material = "adamantine";
            adamantineheadband.group_id = "headband";
            adamantineheadband.translation_key = "adamantine_headband_locale";
            adamantineheadband.path_icon = "ui/icons/items/icon_adamantine_headband";
            adamantineheadband.setCost(0, "adamantine", 1, "none", 0);
            adamantineheadband.base_stats["armor"] = 5f;
            adamantineheadband.base_stats["multiplier_attack_speed"] = 0.04f;
            adamantineheadband.base_stats["speed"] = 20f;
            adamantineheadband.base_stats["mana"] = 35f;
            adamantineheadband.base_stats["stamina"] = 10f;
            adamantineheadband.rigidity_rating = 7;
            adamantineheadband.equipment_value = 70;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(adamantineheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(adamantineheadband);
	    //Scale Headband
            EquipmentAsset scaleheadband = AssetManager.items.clone("scale_headband", "$helmet");
            scaleheadband.needs_to_be_explored = false;
            scaleheadband.animated = true;
            scaleheadband.metallic = true;
            scaleheadband.equipment_subtype = null;
            scaleheadband.material = "basic";
            scaleheadband.group_id = "headband";
            scaleheadband.translation_key = "scale_headband_locale";
            scaleheadband.path_icon = "ui/icons/items/icon_scale_headband";
            scaleheadband.setCost(10, "dragon_scales", 2, "none", 0);
            scaleheadband.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scaleheadband.base_stats.addTag("building_immunity_fire");
            scaleheadband.base_stats.addTag("immunity_fire");
            scaleheadband.base_stats["armor"] = 8f;
            scaleheadband.base_stats["multiplier_attack_speed"] = 0.05f;
            scaleheadband.base_stats["speed"] = 22f;
            scaleheadband.base_stats["mana"] = 40f;
            scaleheadband.base_stats["stamina"] = 20f;
            scaleheadband.rigidity_rating = 8;
            scaleheadband.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(scaleheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(scaleheadband);
	    //Ice Headband
            EquipmentAsset iceheadband = AssetManager.items.clone("ice_headband", "$helmet");
            iceheadband.needs_to_be_explored = false;
            iceheadband.metallic = true;
            iceheadband.equipment_subtype = null;
            iceheadband.material = "basic";
            iceheadband.group_id = "headband";
            iceheadband.translation_key = "ice_headband_locale";
            iceheadband.path_icon = "ui/icons/items/icon_ice_headband";
            iceheadband.setCost(10, "mythril", 10, "gems", 2);
            iceheadband.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            iceheadband.base_stats.addTag("immunity_cold");
            iceheadband.base_stats["armor"] = 7f;
            iceheadband.base_stats["multiplier_attack_speed"] = 0.05f;
            iceheadband.base_stats["speed"] = 25f;
            iceheadband.base_stats["mana"] = 35f;
            iceheadband.base_stats["stamina"] = 15f;
            iceheadband.rigidity_rating = 8;
            iceheadband.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["headband"].Add(iceheadband);
            AssetManager.items.pot_equipment_by_groups_unlocked["headband"].Add(iceheadband);
        //Cloaks---------------------------------------------------------------------------------------------------------
        //Cloak & Armor Lovers culture trait
            CultureTrait cloaklovers = AssetManager.culture_traits.clone("Cloak Lovers", "sword_lovers");
            cloaklovers.needs_to_be_explored = false;
            cloaklovers.group_id = "Gear";
            cloaklovers.path_icon = "ui/icons/iconCloakLovers";
            cloaklovers.priority = 97;
            cloaklovers.related_weapon_subtype_ids.Remove("sword");
            cloaklovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){
                cloaklovers.default_for_actor_assets.Add(AssetManager.actor_library.get("evil_mage"));
                cloaklovers.default_for_actor_assets.Add(AssetManager.actor_library.get("white_mage"));
                cloaklovers.default_for_actor_assets.Add(AssetManager.actor_library.get("necromancer"));
                cloaklovers.default_for_actor_assets.Add(AssetManager.actor_library.get("druid"));
                cloaklovers.default_for_actor_assets.Add(AssetManager.actor_library.get("plague_doctor"));
            }
            cloaklovers.addOpposite("Armor Lovers");
            reAddToCultureTraitLists(cloaklovers);
            CultureTrait armorlovers = AssetManager.culture_traits.clone("Armor Lovers", "sword_lovers");
            armorlovers.needs_to_be_explored = false;
            armorlovers.group_id = "Gear";
            armorlovers.path_icon = "ui/icons/iconArmorLovers";
            armorlovers.priority = 98;
            armorlovers.related_weapon_subtype_ids.Remove("sword");
            armorlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){armorlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_rhino"));}
            armorlovers.addOpposite("Cloak Lovers");
            reAddToCultureTraitLists(armorlovers);
	    //Leather Cloak
            EquipmentAsset leathercloak = AssetManager.items.clone("leather_cloak", "$armor");
            leathercloak.needs_to_be_explored = false;
            leathercloak.metallic = false;
            leathercloak.equipment_subtype = null;
            leathercloak.material = "leather";
            leathercloak.group_id = "cloak";
            leathercloak.translation_key = "leather_cloak_locale";
            leathercloak.path_icon = "ui/icons/items/icon_leather_cloak";
            leathercloak.setCost(0, "leather", 1, "none", 0);
            leathercloak.base_stats["armor"] = 1f;
            leathercloak.base_stats["multiplier_attack_speed"] = 0.01f;
            leathercloak.base_stats["speed"] = 5f;
            leathercloak.base_stats["stamina"] = 20f;
            leathercloak.rigidity_rating = 1;
            leathercloak.equipment_value = 1;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(leathercloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(leathercloak);
	    //Copper Cloak
            EquipmentAsset coppercloak = AssetManager.items.clone("copper_cloak", "$armor");
            coppercloak.needs_to_be_explored = false;
            coppercloak.metallic = true;
            coppercloak.equipment_subtype = null;
            coppercloak.material = "copper";
            coppercloak.group_id = "cloak";
            coppercloak.translation_key = "copper_cloak_locale";
            coppercloak.path_icon = "ui/icons/items/icon_copper_cloak";
            coppercloak.setCost(0, "common_metals", 1, "none", 0);
            coppercloak.base_stats["armor"] = 1f; 
            coppercloak.base_stats["multiplier_attack_speed"] = 0.01f;
            coppercloak.base_stats["speed"] = 5f;
            coppercloak.base_stats["stamina"] = 15f;
            coppercloak.rigidity_rating = 2;
            coppercloak.equipment_value = 10;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(coppercloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(coppercloak);
	    //Bronze Cloak
            EquipmentAsset bronzecloak = AssetManager.items.clone("bronze_cloak", "$armor");
            bronzecloak.needs_to_be_explored = false;
            bronzecloak.metallic = true;
            bronzecloak.equipment_subtype = null;
            bronzecloak.material = "bronze";
            bronzecloak.group_id = "cloak";
            bronzecloak.translation_key = "bronze_cloak_locale";
            bronzecloak.path_icon = "ui/icons/items/icon_bronze_cloak";
            bronzecloak.setCost(0, "common_metals", 2, "none", 0);
            bronzecloak.base_stats["armor"] = 2f;
            bronzecloak.base_stats["multiplier_attack_speed"] = 0.02f;
            bronzecloak.base_stats["speed"] = 8f; 
            bronzecloak.base_stats["stamina"] = 10f;
            bronzecloak.rigidity_rating = 3;
            bronzecloak.equipment_value = 15;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(bronzecloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(bronzecloak);
	    //Silver Cloak
            EquipmentAsset silvercloak = AssetManager.items.clone("silver_cloak", "$armor");
            silvercloak.needs_to_be_explored = false;
            silvercloak.metallic = true;
            silvercloak.equipment_subtype = null;
            silvercloak.material = "silver";
            silvercloak.group_id = "cloak";
            silvercloak.translation_key = "silver_cloak_locale";
            silvercloak.path_icon = "ui/icons/items/icon_silver_cloak";
            silvercloak.setCost(0, "silver", 1, "none", 0);
            silvercloak.base_stats["armor"] = 2f;
            silvercloak.base_stats["multiplier_attack_speed"] = 0.02f;
            silvercloak.base_stats["speed"] = 10f;
            silvercloak.base_stats["mana"] = 15f;
            silvercloak.base_stats["stamina"] = 5f;
            silvercloak.rigidity_rating = 2;
            silvercloak.equipment_value = 20;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(silvercloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(silvercloak);
	    //Gold Cloak
			EquipmentAsset goldcloak = AssetManager.items.clone("gold_cloak", "$armor");
            goldcloak.needs_to_be_explored = false;
            goldcloak.metallic = true;
            goldcloak.equipment_subtype = null;
            goldcloak.material = "gold";
            goldcloak.group_id = "cloak";
            goldcloak.translation_key = "gold_cloak_locale";
            goldcloak.path_icon = "ui/icons/items/icon_gold_cloak";
            goldcloak.setCost(0, "gold", 2, "common_metals", 2);
            goldcloak.base_stats["armor"] = 2.5f;
            goldcloak.base_stats["multiplier_attack_speed"] = 0.02f;
            goldcloak.base_stats["speed"] = 12f;
            goldcloak.base_stats["mana"] = 10f;
            goldcloak.base_stats["stamina"] = 10f;
            goldcloak.rigidity_rating = 2;
            goldcloak.equipment_value = 25;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(goldcloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(goldcloak);
	    //Iron Cloak
            EquipmentAsset ironcloak = AssetManager.items.clone("iron_cloak", "$armor");
            ironcloak.needs_to_be_explored = false;
            ironcloak.metallic = true;
            ironcloak.equipment_subtype = null;
            ironcloak.material = "iron";
            ironcloak.group_id = "cloak";
            ironcloak.translation_key = "iron_cloak_locale";
            ironcloak.path_icon = "ui/icons/items/icon_iron_cloak";
            ironcloak.setCost(0, "common_metals", 3, "none", 0);
            ironcloak.base_stats["armor"] = 3f;
            ironcloak.base_stats["multiplier_attack_speed"] = 0.03f;
            ironcloak.base_stats["speed"] = 12f;
            ironcloak.base_stats["stamina"] = 5f;
            ironcloak.rigidity_rating = 4;
            ironcloak.equipment_value = 30;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(ironcloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(ironcloak);
	    //Steel Cloak
            EquipmentAsset steelcloak = AssetManager.items.clone("steel_cloak", "$armor");
            steelcloak.needs_to_be_explored = false;
            steelcloak.metallic = true;
            steelcloak.equipment_subtype = null;
            steelcloak.material = "steel";
            steelcloak.group_id = "cloak";
            steelcloak.translation_key = "steel_cloak_locale";
            steelcloak.path_icon = "ui/icons/items/icon_steel_cloak";
            steelcloak.setCost(0, "common_metals", 4, "none", 0);
            steelcloak.base_stats["armor"] = 4f;
            steelcloak.base_stats["multiplier_attack_speed"] = 0.03f;
            steelcloak.base_stats["speed"] = 15f;
            steelcloak.base_stats["stamina"] = 5f;
            steelcloak.rigidity_rating = 5;
            steelcloak.equipment_value = 40;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(steelcloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(steelcloak);
	    //Mythril Cloak
            EquipmentAsset mythrilcloak = AssetManager.items.clone("mythril_cloak", "$armor");
            mythrilcloak.needs_to_be_explored = false;
            mythrilcloak.metallic = true;
            mythrilcloak.equipment_subtype = null;
            mythrilcloak.material = "mythril";
            mythrilcloak.group_id = "cloak";
            mythrilcloak.translation_key = "mythril_cloak_locale";
            mythrilcloak.path_icon = "ui/icons/items/icon_mythril_cloak";
            mythrilcloak.setCost(0, "mythril", 1, "none", 0);
            mythrilcloak.base_stats["armor"] = 4f;
            mythrilcloak.base_stats["multiplier_attack_speed"] = 0.03f;
            mythrilcloak.base_stats["speed"] = 18f;
            mythrilcloak.base_stats["mana"] = 25f;
            mythrilcloak.base_stats["stamina"] = 5f;
            mythrilcloak.rigidity_rating = 6;
            mythrilcloak.equipment_value = 50;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(mythrilcloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(mythrilcloak);
	    //Adamantine Cloak
            EquipmentAsset adamantinecloak = AssetManager.items.clone("adamantine_cloak", "$armor");
            adamantinecloak.needs_to_be_explored = false;
            adamantinecloak.metallic = true;
            adamantinecloak.equipment_subtype = null;
            adamantinecloak.material = "adamantine";
            adamantinecloak.group_id = "cloak";
            adamantinecloak.translation_key = "adamantine_cloak_locale";
            adamantinecloak.path_icon = "ui/icons/items/icon_adamantine_cloak";
            adamantinecloak.setCost(0, "adamantine", 1, "none", 0);
            adamantinecloak.base_stats["armor"] = 5f;
            adamantinecloak.base_stats["multiplier_attack_speed"] = 0.04f;
            adamantinecloak.base_stats["speed"] = 20f;
            adamantinecloak.base_stats["mana"] = 35f;
            adamantinecloak.base_stats["stamina"] = 5f;
            adamantinecloak.rigidity_rating = 7;
            adamantinecloak.equipment_value = 70;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(adamantinecloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(adamantinecloak);
	    //Scale Cloak
            EquipmentAsset scalecloak = AssetManager.items.clone("scale_cloak", "$armor");
            scalecloak.needs_to_be_explored = false;
            scalecloak.animated = true;
            scalecloak.metallic = true;
            scalecloak.equipment_subtype = null;
            scalecloak.material = "basic";
            scalecloak.group_id = "cloak";
            scalecloak.translation_key = "scale_cloak_locale";
            scalecloak.path_icon = "ui/icons/items/icon_scale_cloak";
            scalecloak.setCost(10, "dragon_scales", 2, "none", 0);
            scalecloak.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scalecloak.base_stats.addTag("building_immunity_fire");
            scalecloak.base_stats.addTag("immunity_fire");
            scalecloak.base_stats["armor"] = 8f;
            scalecloak.base_stats["multiplier_attack_speed"] = 0.05f;
            scalecloak.base_stats["speed"] = 22f;
            scalecloak.base_stats["mana"] = 40f;
            scalecloak.base_stats["stamina"] = 20f;
            scalecloak.rigidity_rating = 8;
            scalecloak.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(scalecloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(scalecloak);
	    //Ice Cloak
            EquipmentAsset icecloak = AssetManager.items.clone("ice_cloak", "$armor");
            icecloak.needs_to_be_explored = false;
            icecloak.metallic = true;
            icecloak.equipment_subtype = null;
            icecloak.material = "basic";
            icecloak.group_id = "cloak";
            icecloak.translation_key = "ice_cloak_locale";
            icecloak.path_icon = "ui/icons/items/icon_ice_cloak";
            icecloak.setCost(10, "mythril", 10, "gems", 2);
            icecloak.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icecloak.base_stats.addTag("immunity_cold");
            icecloak.base_stats["armor"] = 7f;
            icecloak.base_stats["multiplier_attack_speed"] = 0.05f;
            icecloak.base_stats["speed"] = 25f;
            icecloak.base_stats["mana"] = 35f;
            icecloak.base_stats["stamina"] = 15f;
            icecloak.rigidity_rating = 8;
            icecloak.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["cloak"].Add(icecloak);
            AssetManager.items.pot_equipment_by_groups_unlocked["cloak"].Add(icecloak);
        //Highcleats---------------------------------------------------------------------------------------------------------
        //Highcleat & Boot Lovers culture trait
            CultureTrait highcleatlovers = AssetManager.culture_traits.clone("Highcleat Lovers", "sword_lovers");
            highcleatlovers.needs_to_be_explored = false;
            highcleatlovers.group_id = "Gear";
            highcleatlovers.path_icon = "ui/icons/iconHighcleatLovers";
            highcleatlovers.priority = 95;
            highcleatlovers.related_weapon_subtype_ids.Remove("sword");
            highcleatlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){highcleatlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_goat"));}
            highcleatlovers.addOpposite("Boot Lovers");
            reAddToCultureTraitLists(highcleatlovers);
            CultureTrait bootlovers = AssetManager.culture_traits.clone("Boot Lovers", "sword_lovers");
            bootlovers.needs_to_be_explored = false;
            bootlovers.group_id = "Gear";
            bootlovers.path_icon = "ui/icons/iconBootLovers";
            bootlovers.priority = 96;
            bootlovers.related_weapon_subtype_ids.Remove("sword");
            bootlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){bootlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_seal"));}
            bootlovers.addOpposite("Highcleat Lovers");
            reAddToCultureTraitLists(bootlovers);
	    //Leather Highcleat
            EquipmentAsset leatherhighcleat = AssetManager.items.clone("leather_highcleats", "$boots");
            leatherhighcleat.needs_to_be_explored = false;
            leatherhighcleat.metallic = false;
            leatherhighcleat.equipment_subtype = null;
            leatherhighcleat.material = "leather";
            leatherhighcleat.group_id = "highcleats";
            leatherhighcleat.translation_key = "leather_highcleats_locale";
            leatherhighcleat.path_icon = "ui/icons/items/icon_leather_highcleats";
            leatherhighcleat.setCost(0, "leather", 1, "none", 0);
            leatherhighcleat.base_stats["armor"] = 1f;
            leatherhighcleat.base_stats["multiplier_attack_speed"] = 0.01f;
            leatherhighcleat.base_stats["speed"] = 5f;
            leatherhighcleat.base_stats["stamina"] = 10f;
            leatherhighcleat.rigidity_rating = 1;
            leatherhighcleat.equipment_value = 1;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(leatherhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(leatherhighcleat);
	    //Copper Highcleat
            EquipmentAsset copperhighcleat = AssetManager.items.clone("copper_highcleats", "$boots");
            copperhighcleat.needs_to_be_explored = false;
            copperhighcleat.metallic = true;
            copperhighcleat.equipment_subtype = null;
            copperhighcleat.material = "copper";
            copperhighcleat.group_id = "highcleats";
            copperhighcleat.translation_key = "copper_highcleats_locale";
            copperhighcleat.path_icon = "ui/icons/items/icon_copper_highcleats";
            copperhighcleat.setCost(0, "common_metals", 1, "none", 0);
            copperhighcleat.base_stats["armor"] = 1f; 
            copperhighcleat.base_stats["multiplier_attack_speed"] = 0.01f;
            copperhighcleat.base_stats["speed"] = 5f;
            copperhighcleat.base_stats["stamina"] = 5f;
            copperhighcleat.rigidity_rating = 2;
            copperhighcleat.equipment_value = 10;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(copperhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(copperhighcleat);
	    //Bronze Highcleat
            EquipmentAsset bronzehighcleat = AssetManager.items.clone("bronze_highcleats", "$boots");
            bronzehighcleat.needs_to_be_explored = false;
            bronzehighcleat.metallic = true;
            bronzehighcleat.equipment_subtype = null;
            bronzehighcleat.material = "bronze";
            bronzehighcleat.group_id = "highcleats";
            bronzehighcleat.translation_key = "bronze_highcleats_locale";
            bronzehighcleat.path_icon = "ui/icons/items/icon_bronze_highcleats";
            bronzehighcleat.setCost(0, "common_metals", 2, "none", 0);
            bronzehighcleat.base_stats["armor"] = 2f;
            bronzehighcleat.base_stats["multiplier_attack_speed"] = 0.02f;
            bronzehighcleat.base_stats["speed"] = 8f; 
            bronzehighcleat.base_stats["stamina"] = 5f;
            bronzehighcleat.rigidity_rating = 3;
            bronzehighcleat.equipment_value = 15;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(bronzehighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(bronzehighcleat);
	    //Silver Highcleat
            EquipmentAsset silverhighcleat = AssetManager.items.clone("silver_highcleats", "$boots");
            silverhighcleat.needs_to_be_explored = false;
            silverhighcleat.metallic = true;
            silverhighcleat.equipment_subtype = null;
            silverhighcleat.material = "silver";
            silverhighcleat.group_id = "highcleats";
            silverhighcleat.translation_key = "silver_highcleats_locale";
            silverhighcleat.path_icon = "ui/icons/items/icon_silver_highcleats";
            silverhighcleat.setCost(0, "silver", 1, "none", 0);
            silverhighcleat.base_stats["armor"] = 2f;
            silverhighcleat.base_stats["multiplier_attack_speed"] = 0.02f;
            silverhighcleat.base_stats["speed"] = 10f;
            silverhighcleat.base_stats["mana"] = 15f;
            silverhighcleat.base_stats["stamina"] = 5f;
            silverhighcleat.rigidity_rating = 2;
            silverhighcleat.equipment_value = 20;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(silverhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(silverhighcleat);
	    //Gold Highcleat
			EquipmentAsset goldhighcleat = AssetManager.items.clone("gold_highcleats", "$boots");
            goldhighcleat.needs_to_be_explored = false;
            goldhighcleat.metallic = true;
            goldhighcleat.equipment_subtype = null;
            goldhighcleat.material = "gold";
            goldhighcleat.group_id = "highcleats";
            goldhighcleat.translation_key = "gold_highcleats_locale";
            goldhighcleat.path_icon = "ui/icons/items/icon_gold_highcleats";
            goldhighcleat.setCost(0, "gold", 2, "common_metals", 2);
            goldhighcleat.base_stats["armor"] = 2.5f;
            goldhighcleat.base_stats["multiplier_attack_speed"] = 0.02f;
            goldhighcleat.base_stats["speed"] = 12f;
            goldhighcleat.base_stats["mana"] = 10f;
            goldhighcleat.base_stats["stamina"] = 10f;
            goldhighcleat.rigidity_rating = 2;
            goldhighcleat.equipment_value = 25;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(goldhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(goldhighcleat);
	    //Iron Highcleat
            EquipmentAsset ironhighcleat = AssetManager.items.clone("iron_highcleats", "$boots");
            ironhighcleat.needs_to_be_explored = false;
            ironhighcleat.metallic = true;
            ironhighcleat.equipment_subtype = null;
            ironhighcleat.material = "iron";
            ironhighcleat.group_id = "highcleats";
            ironhighcleat.translation_key = "iron_highcleats_locale";
            ironhighcleat.path_icon = "ui/icons/items/icon_iron_highcleats";
            ironhighcleat.setCost(0, "common_metals", 3, "none", 0);
            ironhighcleat.base_stats["armor"] = 3f;
            ironhighcleat.base_stats["multiplier_attack_speed"] = 0.03f;
            ironhighcleat.base_stats["speed"] = 12f;
            ironhighcleat.base_stats["stamina"] = 5f;
            ironhighcleat.rigidity_rating = 4;
            ironhighcleat.equipment_value = 30;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(ironhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(ironhighcleat);
	    //Steel Highcleat
            EquipmentAsset steelhighcleat = AssetManager.items.clone("steel_highcleats", "$boots");
            steelhighcleat.needs_to_be_explored = false;
            steelhighcleat.metallic = true;
            steelhighcleat.equipment_subtype = null;
            steelhighcleat.material = "steel";
            steelhighcleat.group_id = "highcleats";
            steelhighcleat.translation_key = "steel_highcleats_locale";
            steelhighcleat.path_icon = "ui/icons/items/icon_steel_highcleats";
            steelhighcleat.setCost(0, "common_metals", 4, "none", 0);
            steelhighcleat.base_stats["armor"] = 4f;
            steelhighcleat.base_stats["multiplier_attack_speed"] = 0.03f;
            steelhighcleat.base_stats["speed"] = 15f;
            steelhighcleat.base_stats["stamina"] = 5f;
            steelhighcleat.rigidity_rating = 5;
            steelhighcleat.equipment_value = 40;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(steelhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(steelhighcleat);
	    //Mythril Highcleat
            EquipmentAsset mythrilhighcleat = AssetManager.items.clone("mythril_highcleats", "$boots");
            mythrilhighcleat.needs_to_be_explored = false;
            mythrilhighcleat.metallic = true;
            mythrilhighcleat.equipment_subtype = null;
            mythrilhighcleat.material = "mythril";
            mythrilhighcleat.group_id = "highcleats";
            mythrilhighcleat.translation_key = "mythril_highcleats_locale";
            mythrilhighcleat.path_icon = "ui/icons/items/icon_mythril_highcleats";
            mythrilhighcleat.setCost(0, "mythril", 1, "none", 0);
            mythrilhighcleat.base_stats["armor"] = 4f;
            mythrilhighcleat.base_stats["multiplier_attack_speed"] = 0.03f;
            mythrilhighcleat.base_stats["speed"] = 18f;
            mythrilhighcleat.base_stats["mana"] = 25f;
            mythrilhighcleat.base_stats["stamina"] = 10f;
            mythrilhighcleat.rigidity_rating = 6;
            mythrilhighcleat.equipment_value = 50;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(mythrilhighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(mythrilhighcleat);
	    //Adamantine Highcleat
            EquipmentAsset adamantinehighcleat = AssetManager.items.clone("adamantine_highcleats", "$boots");
            adamantinehighcleat.needs_to_be_explored = false;
            adamantinehighcleat.metallic = true;
            adamantinehighcleat.equipment_subtype = null;
            adamantinehighcleat.material = "adamantine";
            adamantinehighcleat.group_id = "highcleats";
            adamantinehighcleat.translation_key = "adamantine_highcleats_locale";
            adamantinehighcleat.path_icon = "ui/icons/items/icon_adamantine_highcleats";
            adamantinehighcleat.setCost(0, "adamantine", 1, "none", 0);
            adamantinehighcleat.base_stats["armor"] = 5f;
            adamantinehighcleat.base_stats["multiplier_attack_speed"] = 0.04f;
            adamantinehighcleat.base_stats["speed"] = 20f;
            adamantinehighcleat.base_stats["mana"] = 35f;
            adamantinehighcleat.base_stats["stamina"] = 10f;
            adamantinehighcleat.rigidity_rating = 7;
            adamantinehighcleat.equipment_value = 70;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(adamantinehighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(adamantinehighcleat);
	    //Scale Highcleat
            EquipmentAsset scalehighcleat = AssetManager.items.clone("scale_highcleats", "$boots");
            scalehighcleat.needs_to_be_explored = false;
            scalehighcleat.animated = true;
            scalehighcleat.metallic = true;
            scalehighcleat.equipment_subtype = null;
            scalehighcleat.material = "basic";
            scalehighcleat.group_id = "highcleats";
            scalehighcleat.translation_key = "scale_highcleats_locale";
            scalehighcleat.path_icon = "ui/icons/items/icon_scale_highcleats";
            scalehighcleat.setCost(10, "dragon_scales", 2, "none", 0);
            scalehighcleat.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scalehighcleat.base_stats.addTag("building_immunity_fire");
            scalehighcleat.base_stats.addTag("immunity_fire");
            scalehighcleat.base_stats["armor"] = 8f;
            scalehighcleat.base_stats["multiplier_attack_speed"] = 0.05f;
            scalehighcleat.base_stats["speed"] = 22f;
            scalehighcleat.base_stats["mana"] = 40f;
            scalehighcleat.base_stats["stamina"] = 20f;
            scalehighcleat.rigidity_rating = 8;
            scalehighcleat.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(scalehighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(scalehighcleat);
	    //Ice Highcleat
            EquipmentAsset icehighcleat = AssetManager.items.clone("ice_highcleats", "$boots");
            icehighcleat.needs_to_be_explored = false;
            icehighcleat.metallic = true;
            icehighcleat.equipment_subtype = null;
            icehighcleat.material = "basic";
            icehighcleat.group_id = "highcleats";
            icehighcleat.translation_key = "ice_highcleats_locale";
            icehighcleat.path_icon = "ui/icons/items/icon_ice_highcleats";
            icehighcleat.setCost(10, "mythril", 10, "gems", 2);
            icehighcleat.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icehighcleat.base_stats.addTag("immunity_cold");
            icehighcleat.base_stats["armor"] = 7f;
            icehighcleat.base_stats["multiplier_attack_speed"] = 0.05f;
            icehighcleat.base_stats["speed"] = 25f;
            icehighcleat.base_stats["mana"] = 35f;
            icehighcleat.base_stats["stamina"] = 15f;
            icehighcleat.rigidity_rating = 8;
            icehighcleat.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(icehighcleat);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(icehighcleat);
        //Earrings---------------------------------------------------------------------------------------------------------
        //Earring & Ring Lovers culture trait
            CultureTrait earringlovers = AssetManager.culture_traits.clone("Earring Lovers", "sword_lovers");
            earringlovers.needs_to_be_explored = false;
            earringlovers.group_id = "Gear";
            earringlovers.path_icon = "ui/icons/iconEarringLovers";
            earringlovers.priority = 93;
            earringlovers.related_weapon_subtype_ids.Remove("sword");
            earringlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){earringlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_liliar"));}
            earringlovers.addOpposite("Ring Lovers");
            reAddToCultureTraitLists(earringlovers);
            CultureTrait ringlovers = AssetManager.culture_traits.clone("Ring Lovers", "sword_lovers");
            ringlovers.needs_to_be_explored = false;
            ringlovers.group_id = "Gear";
            ringlovers.path_icon = "ui/icons/iconRingLovers";
            ringlovers.priority = 94;
            ringlovers.related_weapon_subtype_ids.Remove("sword");
            ringlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){ringlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_sheep"));}
            ringlovers.addOpposite("Earring Lovers");
            reAddToCultureTraitLists(ringlovers);
	    //Bone Earrings
            EquipmentAsset boneearrings = AssetManager.items.clone("bone_earrings", "$ring");
            boneearrings.needs_to_be_explored = false;
            boneearrings.metallic = false;
            boneearrings.equipment_subtype = null;
            boneearrings.material = "bone";
            boneearrings.group_id = "earrings";
            boneearrings.translation_key = "bone_earrings_locale";
            boneearrings.path_icon = "ui/icons/items/icon_bone_earrings";
            boneearrings.setCost(0, "bones", 1, "gems", 1);
            boneearrings.base_stats["skill_spell"] = 0.01f;
            boneearrings.base_stats["skill_combat"] = 0.01f;
            boneearrings.base_stats["stamina"] = 5f;
            boneearrings.rigidity_rating = 1;
            boneearrings.equipment_value = 1;
            AssetManager.items.pot_equipment_by_groups_all["highcleats"].Add(boneearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["highcleats"].Add(boneearrings);
	    //Copper Earrings
            EquipmentAsset copperearrings = AssetManager.items.clone("copper_earrings", "$ring");
            copperearrings.needs_to_be_explored = false;
            copperearrings.metallic = true;
            copperearrings.equipment_subtype = null;
            copperearrings.material = "copper";
            copperearrings.group_id = "earrings";
            copperearrings.translation_key = "copper_earrings_locale";
            copperearrings.path_icon = "ui/icons/items/icon_copper_earrings";
            copperearrings.setCost(0, "common_metals", 1, "gems", 1);
            copperearrings.base_stats["skill_spell"] = 0.015f;
            copperearrings.base_stats["skill_combat"] = 0.015f;
            copperearrings.base_stats["stamina"] = 5f;
            copperearrings.rigidity_rating = 2;
            copperearrings.equipment_value = 10;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(copperearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(copperearrings);
	    //Bronze Earrings
            EquipmentAsset bronzeearrings = AssetManager.items.clone("bronze_earrings", "$ring");
            bronzeearrings.needs_to_be_explored = false;
            bronzeearrings.metallic = true;
            bronzeearrings.equipment_subtype = null;
            bronzeearrings.material = "bronze";
            bronzeearrings.group_id = "earrings";
            bronzeearrings.translation_key = "bronze_earrings_locale";
            bronzeearrings.path_icon = "ui/icons/items/icon_bronze_earrings";
            bronzeearrings.setCost(0, "common_metals", 2, "gems", 1);
            bronzeearrings.base_stats["skill_spell"] = 0.02f;
            bronzeearrings.base_stats["skill_combat"] = 0.02f;
            bronzeearrings.base_stats["stamina"] = 5f;
            bronzeearrings.rigidity_rating = 3;
            bronzeearrings.equipment_value = 15;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(bronzeearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(bronzeearrings);
	    //Silver Earrings
            EquipmentAsset silverearrings = AssetManager.items.clone("silver_earrings", "$ring");
            silverearrings.needs_to_be_explored = false;
            silverearrings.metallic = true;
            silverearrings.equipment_subtype = null;
            silverearrings.material = "silver";
            silverearrings.group_id = "earrings";
            silverearrings.translation_key = "silver_earrings_locale";
            silverearrings.path_icon = "ui/icons/items/icon_silver_earrings";
            silverearrings.setCost(0, "silver", 1, "gems", 1);
            silverearrings.base_stats["skill_spell"] = 0.025f;
            silverearrings.base_stats["skill_combat"] = 0.025f;
            silverearrings.base_stats["mana"] = 10f;
            silverearrings.base_stats["stamina"] = 5f;
            silverearrings.rigidity_rating = 2;
            silverearrings.equipment_value = 20;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(silverearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(silverearrings);
	    //Gold Earrings
			EquipmentAsset goldearrings = AssetManager.items.clone("gold_earrings", "$ring");
            goldearrings.needs_to_be_explored = false;
            goldearrings.metallic = true;
            goldearrings.equipment_subtype = null;
            goldearrings.material = "gold";
            goldearrings.group_id = "earrings";
            goldearrings.translation_key = "gold_earrings_locale";
            goldearrings.path_icon = "ui/icons/items/icon_gold_earrings";
            goldearrings.setCost(0, "gold", 3, "gems", 2);
            goldearrings.base_stats["skill_spell"] = 0.0275f;
            goldearrings.base_stats["skill_combat"] = 0.0275f;
            goldearrings.base_stats["mana"] = 5f;
            goldearrings.base_stats["stamina"] = 5f;
            goldearrings.rigidity_rating = 2;
            goldearrings.equipment_value = 25;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(goldearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(goldearrings);
	    //Iron Earrings
            EquipmentAsset ironearrings = AssetManager.items.clone("iron_earrings", "$ring");
            ironearrings.needs_to_be_explored = false;
            ironearrings.metallic = true;
            ironearrings.equipment_subtype = null;
            ironearrings.material = "iron";
            ironearrings.group_id = "earrings";
            ironearrings.translation_key = "iron_earrings_locale";
            ironearrings.path_icon = "ui/icons/items/icon_iron_earrings";
            ironearrings.setCost(0, "common_metals", 3, "gems", 1);
            ironearrings.base_stats["skill_spell"] = 0.03f;
            ironearrings.base_stats["skill_combat"] = 0.03f;
            ironearrings.base_stats["stamina"] = 5f;
            ironearrings.rigidity_rating = 4;
            ironearrings.equipment_value = 30;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(ironearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(ironearrings);
	    //Steel Earrings
            EquipmentAsset steelearrings = AssetManager.items.clone("steel_earrings", "$ring");
            steelearrings.needs_to_be_explored = false;
            steelearrings.metallic = true;
            steelearrings.equipment_subtype = null;
            steelearrings.material = "steel";
            steelearrings.group_id = "earrings";
            steelearrings.translation_key = "steel_earrings_locale";
            steelearrings.path_icon = "ui/icons/items/icon_steel_earrings";
            steelearrings.setCost(0, "common_metals", 4, "gems", 1);
            steelearrings.base_stats["skill_spell"] = 0.035f;
            steelearrings.base_stats["skill_combat"] = 0.035f;
            steelearrings.base_stats["stamina"] = 5f;
            steelearrings.rigidity_rating = 5;
            steelearrings.equipment_value = 40;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(steelearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(steelearrings);
	    //Mythril Earrings
            EquipmentAsset mythrilearrings = AssetManager.items.clone("mythril_earrings", "$ring");
            mythrilearrings.needs_to_be_explored = false;
            mythrilearrings.metallic = true;
            mythrilearrings.equipment_subtype = null;
            mythrilearrings.material = "mythril";
            mythrilearrings.group_id = "earrings";
            mythrilearrings.translation_key = "mythril_earrings_locale";
            mythrilearrings.path_icon = "ui/icons/items/icon_mythril_earrings";
            mythrilearrings.setCost(0, "mythril", 1, "gems", 1);
            mythrilearrings.base_stats["skill_spell"] = 0.04f;
            mythrilearrings.base_stats["skill_combat"] = 0.04f;
            mythrilearrings.base_stats["mana"] = 15f;
            mythrilearrings.base_stats["stamina"] = 5f;
            mythrilearrings.rigidity_rating = 6;
            mythrilearrings.equipment_value = 50;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(mythrilearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(mythrilearrings);
	    //Adamantine Earrings
            EquipmentAsset adamantineearrings = AssetManager.items.clone("adamantine_earrings", "$ring");
            adamantineearrings.needs_to_be_explored = false;
            adamantineearrings.metallic = true;
            adamantineearrings.equipment_subtype = null;
            adamantineearrings.material = "adamantine";
            adamantineearrings.group_id = "earrings";
            adamantineearrings.translation_key = "adamantine_earrings_locale";
            adamantineearrings.path_icon = "ui/icons/items/icon_adamantine_earrings";
            adamantineearrings.setCost(0, "adamantine", 1, "gems", 1);
            adamantineearrings.base_stats["skill_spell"] = 0.05f;
            adamantineearrings.base_stats["skill_combat"] = 0.05f;
            adamantineearrings.base_stats["mana"] = 20f;
            adamantineearrings.base_stats["stamina"] = 5f;
            adamantineearrings.rigidity_rating = 7;
            adamantineearrings.equipment_value = 70;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(adamantineearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(adamantineearrings);
	    //Scale Earrings
            EquipmentAsset scaleearrings = AssetManager.items.clone("scale_earrings", "$ring");
            scaleearrings.needs_to_be_explored = false;
            scaleearrings.animated = true;
            scaleearrings.metallic = true;
            scaleearrings.equipment_subtype = null;
            scaleearrings.material = "basic";
            scaleearrings.group_id = "earrings";
            scaleearrings.translation_key = "scale_earrings_locale";
            scaleearrings.path_icon = "ui/icons/items/icon_scale_earrings";
            scaleearrings.setCost(10, "dragon_scales", 2, "gems", 1);
            scaleearrings.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scaleearrings.base_stats.addTag("building_immunity_fire");
            scaleearrings.base_stats.addTag("immunity_fire");
            scaleearrings.base_stats["skill_spell"] = 0.075f;
            scaleearrings.base_stats["skill_combat"] = 0.075f;
            scaleearrings.base_stats["mana"] = 25f;
            scaleearrings.base_stats["stamina"] = 10f;
            scaleearrings.rigidity_rating = 8;
            scaleearrings.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(scaleearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(scaleearrings);
	    //Ice Earrings
            EquipmentAsset iceearrings = AssetManager.items.clone("ice_earrings", "$ring");
            iceearrings.needs_to_be_explored = false;
            iceearrings.metallic = true;
            iceearrings.equipment_subtype = null;
            iceearrings.material = "basic";
            iceearrings.group_id = "earrings";
            iceearrings.translation_key = "ice_earrings_locale";
            iceearrings.path_icon = "ui/icons/items/icon_ice_earrings";
            iceearrings.setCost(10, "mythril", 8, "gems", 1);
            iceearrings.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            iceearrings.base_stats.addTag("immunity_cold");
            iceearrings.base_stats["skill_spell"] = 0.06f;
            iceearrings.base_stats["skill_combat"] = 0.06f;
            iceearrings.base_stats["speed"] = 3f;
            iceearrings.base_stats["mana"] = 20f;
            iceearrings.base_stats["stamina"] = 10f;
            iceearrings.rigidity_rating = 8;
            iceearrings.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["earrings"].Add(iceearrings);
            AssetManager.items.pot_equipment_by_groups_unlocked["earrings"].Add(iceearrings);
    //Bracelets---------------------------------------------------------------------------------------------------------
        //Bracelet & Amulet Lovers culture trait
            CultureTrait braceletlovers = AssetManager.culture_traits.clone("Bracelet Lovers", "sword_lovers");
            braceletlovers.needs_to_be_explored = false;
            braceletlovers.group_id = "Gear";
            braceletlovers.path_icon = "ui/icons/iconBraceletLovers";
            braceletlovers.priority = 91;
            braceletlovers.related_weapon_subtype_ids.Remove("sword");
            braceletlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){braceletlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_unicorn"));}
            braceletlovers.addOpposite("Amulet Lovers");
            reAddToCultureTraitLists(braceletlovers);
            CultureTrait amuletlovers = AssetManager.culture_traits.clone("Amulet Lovers", "sword_lovers");
            amuletlovers.needs_to_be_explored = false;
            amuletlovers.group_id = "Gear";
            amuletlovers.path_icon = "ui/icons/iconAmuletLovers";
            amuletlovers.priority = 92;
            amuletlovers.related_weapon_subtype_ids.Remove("sword");
            amuletlovers.default_for_actor_assets.Clear();
            if (ConfigsAEA.loverDefaults){amuletlovers.default_for_actor_assets.Add(AssetManager.actor_library.get("civ_monkey"));}
            amuletlovers.addOpposite("Bracelet Lovers");
            reAddToCultureTraitLists(amuletlovers);
	    //Bone Bracelet
            EquipmentAsset bonebracelet = AssetManager.items.clone("bone_bracelet", "$amulet");
            bonebracelet.needs_to_be_explored = false;
            bonebracelet.metallic = false;
            bonebracelet.equipment_subtype = null;
            bonebracelet.material = "bone";
            bonebracelet.group_id = "bracelet";
            bonebracelet.translation_key = "bone_bracelet_locale";
            bonebracelet.path_icon = "ui/icons/items/icon_bone_bracelet";
            bonebracelet.setCost(0, "bones", 1, "gems", 1);
            bonebracelet.base_stats["skill_spell"] = 0.01f;
            bonebracelet.base_stats["skill_combat"] = 0.01f;
            bonebracelet.base_stats["stamina"] = 5f;
            bonebracelet.rigidity_rating = 1;
            bonebracelet.equipment_value = 1;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(bonebracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(bonebracelet);
	    //Copper Bracelet
            EquipmentAsset copperbracelet = AssetManager.items.clone("copper_bracelet", "$amulet");
            copperbracelet.needs_to_be_explored = false;
            copperbracelet.metallic = true;
            copperbracelet.equipment_subtype = null;
            copperbracelet.material = "copper";
            copperbracelet.group_id = "bracelet";
            copperbracelet.translation_key = "copper_bracelet_locale";
            copperbracelet.path_icon = "ui/icons/items/icon_copper_bracelet";
            copperbracelet.setCost(0, "common_metals", 1, "gems", 1);
            copperbracelet.base_stats["skill_spell"] = 0.015f;
            copperbracelet.base_stats["skill_combat"] = 0.015f;
            copperbracelet.base_stats["stamina"] = 5f;
            copperbracelet.rigidity_rating = 2;
            copperbracelet.equipment_value = 10;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(copperbracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(copperbracelet);
	    //Bronze Bracelet
            EquipmentAsset bronzebracelet = AssetManager.items.clone("bronze_bracelet", "$amulet");
            bronzebracelet.needs_to_be_explored = false;
            bronzebracelet.metallic = true;
            bronzebracelet.equipment_subtype = null;
            bronzebracelet.material = "bronze";
            bronzebracelet.group_id = "bracelet";
            bronzebracelet.translation_key = "bronze_bracelet_locale";
            bronzebracelet.path_icon = "ui/icons/items/icon_bronze_bracelet";
            bronzebracelet.setCost(0, "common_metals", 2, "gems", 1);
            bronzebracelet.base_stats["skill_spell"] = 0.02f;
            bronzebracelet.base_stats["skill_combat"] = 0.02f;
            bronzebracelet.base_stats["stamina"] = 5f;
            bronzebracelet.rigidity_rating = 3;
            bronzebracelet.equipment_value = 15;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(bronzebracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(bronzebracelet);
	    //Silver Bracelet
            EquipmentAsset silverbracelet = AssetManager.items.clone("silver_bracelet", "$amulet");
            silverbracelet.needs_to_be_explored = false;
            silverbracelet.metallic = true;
            silverbracelet.equipment_subtype = null;
            silverbracelet.material = "silver";
            silverbracelet.group_id = "bracelet";
            silverbracelet.translation_key = "silver_bracelet_locale";
            silverbracelet.path_icon = "ui/icons/items/icon_silver_bracelet";
            silverbracelet.setCost(0, "silver", 1, "gems", 1);
            silverbracelet.base_stats["skill_spell"] = 0.025f;
            silverbracelet.base_stats["skill_combat"] = 0.025f;
            silverbracelet.base_stats["mana"] = 10f;
            silverbracelet.base_stats["stamina"] = 5f;
            silverbracelet.rigidity_rating = 2;
            silverbracelet.equipment_value = 20;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(silverbracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(silverbracelet);
	    //Gold Bracelet
			EquipmentAsset goldbracelet = AssetManager.items.clone("gold_bracelet", "$amulet");
            goldbracelet.needs_to_be_explored = false;
            goldbracelet.metallic = true;
            goldbracelet.equipment_subtype = null;
            goldbracelet.material = "gold";
            goldbracelet.group_id = "bracelet";
            goldbracelet.translation_key = "gold_bracelet_locale";
            goldbracelet.path_icon = "ui/icons/items/icon_gold_bracelet";
            goldbracelet.setCost(0, "gold", 3, "gems", 2);
            goldbracelet.base_stats["skill_spell"] = 0.0275f;
            goldbracelet.base_stats["skill_combat"] = 0.0275f;
            goldbracelet.base_stats["mana"] = 5f;
            goldbracelet.base_stats["stamina"] = 5f;
            goldbracelet.rigidity_rating = 2;
            goldbracelet.equipment_value = 25;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(goldbracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(goldbracelet);
	    //Iron Bracelet
            EquipmentAsset ironbracelet = AssetManager.items.clone("iron_bracelet", "$amulet");
            ironbracelet.needs_to_be_explored = false;
            ironbracelet.metallic = true;
            ironbracelet.equipment_subtype = null;
            ironbracelet.material = "iron";
            ironbracelet.group_id = "bracelet";
            ironbracelet.translation_key = "iron_bracelet_locale";
            ironbracelet.path_icon = "ui/icons/items/icon_iron_bracelet";
            ironbracelet.setCost(0, "common_metals", 3, "gems", 1);
            ironbracelet.base_stats["skill_spell"] = 0.03f;
            ironbracelet.base_stats["skill_combat"] = 0.03f;
            ironbracelet.base_stats["stamina"] = 5f;
            ironbracelet.rigidity_rating = 4;
            ironbracelet.equipment_value = 30;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(ironbracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(ironbracelet);
	    //Steel Bracelet
            EquipmentAsset steelbracelet = AssetManager.items.clone("steel_bracelet", "$amulet");
            steelbracelet.needs_to_be_explored = false;
            steelbracelet.metallic = true;
            steelbracelet.equipment_subtype = null;
            steelbracelet.material = "steel";
            steelbracelet.group_id = "bracelet";
            steelbracelet.translation_key = "steel_bracelet_locale";
            steelbracelet.path_icon = "ui/icons/items/icon_steel_bracelet";
            steelbracelet.setCost(0, "common_metals", 4, "gems", 1);
            steelbracelet.base_stats["skill_spell"] = 0.035f;
            steelbracelet.base_stats["skill_combat"] = 0.035f;
            steelbracelet.base_stats["stamina"] = 5f;
            steelbracelet.rigidity_rating = 5;
            steelbracelet.equipment_value = 40;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(steelbracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(steelbracelet);
	    //Mythril Bracelet
            EquipmentAsset mythrilbracelet = AssetManager.items.clone("mythril_bracelet", "$amulet");
            mythrilbracelet.needs_to_be_explored = false;
            mythrilbracelet.metallic = true;
            mythrilbracelet.equipment_subtype = null;
            mythrilbracelet.material = "mythril";
            mythrilbracelet.group_id = "bracelet";
            mythrilbracelet.translation_key = "mythril_bracelet_locale";
            mythrilbracelet.path_icon = "ui/icons/items/icon_mythril_bracelet";
            mythrilbracelet.setCost(0, "mythril", 1, "gems", 1);
            mythrilbracelet.base_stats["skill_spell"] = 0.04f;
            mythrilbracelet.base_stats["skill_combat"] = 0.04f;
            mythrilbracelet.base_stats["mana"] = 15f;
            mythrilbracelet.base_stats["stamina"] = 5f;
            mythrilbracelet.rigidity_rating = 6;
            mythrilbracelet.equipment_value = 50;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(mythrilbracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(mythrilbracelet);
	    //Adamantine Bracelet
            EquipmentAsset adamantinebracelet = AssetManager.items.clone("adamantine_bracelet", "$amulet");
            adamantinebracelet.needs_to_be_explored = false;
            adamantinebracelet.metallic = true;
            adamantinebracelet.equipment_subtype = null;
            adamantinebracelet.material = "adamantine";
            adamantinebracelet.group_id = "bracelet";
            adamantinebracelet.translation_key = "adamantine_bracelet_locale";
            adamantinebracelet.path_icon = "ui/icons/items/icon_adamantine_bracelet";
            adamantinebracelet.setCost(0, "adamantine", 1, "gems", 1);
            adamantinebracelet.base_stats["skill_spell"] = 0.05f;
            adamantinebracelet.base_stats["skill_combat"] = 0.05f;
            adamantinebracelet.base_stats["mana"] = 20f;
            adamantinebracelet.base_stats["stamina"] = 5f;
            adamantinebracelet.rigidity_rating = 7;
            adamantinebracelet.equipment_value = 70;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(adamantinebracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(adamantinebracelet);
	    //Scale Bracelet
            EquipmentAsset scalebraclet = AssetManager.items.clone("scale_bracelet", "$amulet");
            scalebraclet.needs_to_be_explored = false;
            scalebraclet.animated = true;
            scalebraclet.metallic = true;
            scalebraclet.equipment_subtype = null;
            scalebraclet.material = "basic";
            scalebraclet.group_id = "bracelet";
            scalebraclet.translation_key = "scale_bracelet_locale";
            scalebraclet.path_icon = "ui/icons/items/icon_scale_bracelet";
            scalebraclet.setCost(10, "dragon_scales", 2, "gems", 1);
            scalebraclet.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scalebraclet.base_stats.addTag("building_immunity_fire");
            scalebraclet.base_stats.addTag("immunity_fire");
            scalebraclet.base_stats["skill_spell"] = 0.075f;
            scalebraclet.base_stats["skill_combat"] = 0.075f;
            scalebraclet.base_stats["mana"] = 25f;
            scalebraclet.base_stats["stamina"] = 10f;
            scalebraclet.rigidity_rating = 8;
            scalebraclet.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(scalebraclet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(scalebraclet);
	    //Ice Bracelet
            EquipmentAsset icebracelet = AssetManager.items.clone("ice_bracelet", "$amulet");
            icebracelet.needs_to_be_explored = false;
            icebracelet.metallic = true;
            icebracelet.equipment_subtype = null;
            icebracelet.material = "basic";
            icebracelet.group_id = "bracelet";
            icebracelet.translation_key = "ice_bracelet_locale";
            icebracelet.path_icon = "ui/icons/items/icon_ice_bracelet";
            icebracelet.setCost(10, "mythril", 8, "gems", 1);
            icebracelet.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icebracelet.base_stats.addTag("immunity_cold");
            icebracelet.base_stats["skill_spell"] = 0.06f;
            icebracelet.base_stats["skill_combat"] = 0.06f;
            icebracelet.base_stats["speed"] = 3f;
            icebracelet.base_stats["mana"] = 20f;
            icebracelet.base_stats["stamina"] = 10f;
            icebracelet.rigidity_rating = 8;
            icebracelet.equipment_value = 400;
            AssetManager.items.pot_equipment_by_groups_all["bracelet"].Add(icebracelet);
            AssetManager.items.pot_equipment_by_groups_unlocked["bracelet"].Add(icebracelet);
    //Item Config Adjustments
            if(ConfigsAEA.scciVal == 1){ //light
                icehighcleat.path_icon = "ui/icons/items/icon_ice_highcleats_lightC";
                icebracelet.path_icon = "ui/icons/items/icon_ice_bracelet_lightC";
                iceearrings.path_icon = "ui/icons/items/icon_ice_earrings_lightC";
            }
            if(ConfigsAEA.changeItemSprites){
                adamantineheadband.path_icon = "ui/icons/items/avbchanges/icon_adamantine_headband";
                adamantinecloak.path_icon = "ui/icons/items/avbchanges/icon_adamantine_cloak";
                adamantinehighcleat.path_icon = "ui/icons/items/avbchanges/icon_adamantine_highcleats";
                adamantinebracelet.path_icon = "ui/icons/items/avbchanges/icon_adamantine_bracelet";
                adamantineearrings.path_icon = "ui/icons/items/avbchanges/icon_adamantine_earrings";
            }
    //Culture Trait Implements
            if (ConfigsAEA.flameTierCraft == true){
                CultureTrait craftflame = AssetManager.culture_traits.get("craft_flame_weapon");
                if(ConfigsAEA.headbandCraft == true){craftflame.related_weapons_ids.Add("scale_headband");}
                if(ConfigsAEA.cloakCraft == true){craftflame.related_weapons_ids.Add("scale_cloak");}
                if(ConfigsAEA.highcleatCraft == true){craftflame.related_weapons_ids.Add("scale_highcleats");}
                if(ConfigsAEA.braceletCraft == true){craftflame.related_weapons_ids.Add("scale_bracelet");}
                if(ConfigsAEA.earringCraft == true){craftflame.related_weapons_ids.Add("scale_earrings");}
            }
            if (ConfigsAEA.iceTierCraft == true){
                CultureTrait craftice = AssetManager.culture_traits.get("craft_ice_weapon");
                if(ConfigsAEA.headbandCraft == true){craftice.related_weapons_ids.Add("ice_headband");}
                if(ConfigsAEA.cloakCraft == true){craftice.related_weapons_ids.Add("ice_cloak");}
                if(ConfigsAEA.highcleatCraft == true){craftice.related_weapons_ids.Add("ice_highcleats");}
                if(ConfigsAEA.braceletCraft == true){craftice.related_weapons_ids.Add("ice_bracelet");}
                if(ConfigsAEA.earringCraft == true){craftice.related_weapons_ids.Add("ice_earrings");}
            }
    //Items Lists
        //Subtype, used for deciding how to craft items (craftItem)
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("customItem")){AssetManager.items.equipment_by_subtypes.Add("customItem", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("headband")){AssetManager.items.equipment_by_subtypes.Add("headband", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("cloak")){AssetManager.items.equipment_by_subtypes.Add("cloak", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("highcleats")){AssetManager.items.equipment_by_subtypes.Add("highcleats", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("earrings")){AssetManager.items.equipment_by_subtypes.Add("earrings", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("bracelet")){AssetManager.items.equipment_by_subtypes.Add("bracelet", new List<EquipmentAsset>());}
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scaleheadband);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(iceheadband);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scalecloak);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icecloak);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scalehighcleat);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icehighcleat);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scaleearrings);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(iceearrings);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scalebraclet);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icebracelet);
            if (ConfigsAEA.headbandCraft == true){
                AssetManager.items.equipment_by_subtypes["headband"].Add(leatherheadband);
                AssetManager.items.equipment_by_subtypes["headband"].Add(copperheadband);
                AssetManager.items.equipment_by_subtypes["headband"].Add(bronzeheadband);
                AssetManager.items.equipment_by_subtypes["headband"].Add(silverheadband);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["headband"].Add(goldheadband);}
                AssetManager.items.equipment_by_subtypes["headband"].Add(ironheadband);
                AssetManager.items.equipment_by_subtypes["headband"].Add(steelheadband);
                AssetManager.items.equipment_by_subtypes["headband"].Add(mythrilheadband);
                AssetManager.items.equipment_by_subtypes["headband"].Add(adamantineheadband);
            }
            if (ConfigsAEA.cloakCraft == true){
                AssetManager.items.equipment_by_subtypes["cloak"].Add(leathercloak);
                AssetManager.items.equipment_by_subtypes["cloak"].Add(coppercloak);
                AssetManager.items.equipment_by_subtypes["cloak"].Add(bronzecloak);
                AssetManager.items.equipment_by_subtypes["cloak"].Add(silvercloak);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["cloak"].Add(goldcloak);}
                AssetManager.items.equipment_by_subtypes["cloak"].Add(ironcloak);
                AssetManager.items.equipment_by_subtypes["cloak"].Add(steelcloak);
                AssetManager.items.equipment_by_subtypes["cloak"].Add(mythrilcloak);
                AssetManager.items.equipment_by_subtypes["cloak"].Add(adamantinecloak);
            }
            if (ConfigsAEA.highcleatCraft == true){
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(leatherhighcleat);
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(copperhighcleat);
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(bronzehighcleat);
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(silverhighcleat);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["highcleats"].Add(goldhighcleat);}
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(ironhighcleat);
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(steelhighcleat);
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(mythrilhighcleat);
                AssetManager.items.equipment_by_subtypes["highcleats"].Add(adamantinehighcleat);
            }
            if (ConfigsAEA.earringCraft == true){
                AssetManager.items.equipment_by_subtypes["earrings"].Add(boneearrings);
                AssetManager.items.equipment_by_subtypes["earrings"].Add(copperearrings);
                AssetManager.items.equipment_by_subtypes["earrings"].Add(bronzeearrings);
                AssetManager.items.equipment_by_subtypes["earrings"].Add(silverearrings);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["earrings"].Add(goldearrings);}
                AssetManager.items.equipment_by_subtypes["earrings"].Add(ironearrings);
                AssetManager.items.equipment_by_subtypes["earrings"].Add(steelearrings);
                AssetManager.items.equipment_by_subtypes["earrings"].Add(mythrilearrings);
                AssetManager.items.equipment_by_subtypes["earrings"].Add(adamantineearrings);
            }
            if (ConfigsAEA.braceletCraft == true){
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(bonebracelet);
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(copperbracelet);
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(bronzebracelet);
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(silverbracelet);
                if(ConfigsAEA.goldTierCraft == true || ConfigsAEA.royalGold == true){AssetManager.items.equipment_by_subtypes["bracelet"].Add(goldbracelet);}
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(ironbracelet);
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(steelbracelet);
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(mythrilbracelet);
                AssetManager.items.equipment_by_subtypes["bracelet"].Add(adamantinebracelet);
            }
        }
        private static void reAddToCultureTraitLists(CultureTrait trait)
        {
            if (!aeaCultureTraitsList.Contains(trait)){
                aeaCultureTraitsList.Add(trait);
            }
        }
        private static void populateListOppositeCultureTraits(){
            if (aeaCultureTraitsList.Any()){
                foreach(var trait in aeaCultureTraitsList){
                    List<string>? curentTraitOppositeList = trait.opposite_list;
                    if (curentTraitOppositeList.Any()){
                        if (trait.opposite_traits == null){
                            trait.opposite_traits = new();
                        }
                        foreach (var opposite in trait.opposite_list){
                            var matchedTrait = aeaCultureTraitsList.FirstOrDefault(t => t.id == opposite);
                            if (matchedTrait != null && !trait.opposite_traits.Contains(matchedTrait)){
                                trait.opposite_traits.Add(matchedTrait);
                            }
                        }
                    }
                }
            }
        }
    }
}
