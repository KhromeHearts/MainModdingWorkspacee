//---Credit for code help: This page uses minorly modified [//Item Lists] structure from the Modernbox mod
//---This mod received no direct assistance from contributors of the Modernbox mod and was made by referencing its code
//FillInItems: This file adds custom flame, ice, and gold items for the game's base item types (does not include custom item type versions, like crossbows, see BasicItems.cs for those)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NeoModLoader.api;

namespace avbsexpandedarmory{
    class FillInItemsAndExtraChanges : MonoBehaviour{
        public static void init(){
            int targetIndex = 0; //used for category/item positioning
    //Category Colors
            if (ConfigsAEA.categoryColor == true){
                ItemGroupAsset helmet = AssetManager.item_groups.get("helmet");
                helmet.color = "#bad0ff";
                ItemGroupAsset armor = AssetManager.item_groups.get("armor");
                armor.color = "#bad0ff";
                ItemGroupAsset boots = AssetManager.item_groups.get("boots");
                boots.color = "#bad0ff";
                ItemGroupAsset amulet = AssetManager.item_groups.get("amulet");
                amulet.color = "#baffdf";
                ItemGroupAsset ring = AssetManager.item_groups.get("ring");
                ring.color = "#baffdf";
                ItemGroupAsset sword = AssetManager.item_groups.get("sword");
                sword.color = "#ffbaba";
                ItemGroupAsset axe = AssetManager.item_groups.get("axe");
                axe.color = "#ffbaba";
                ItemGroupAsset hammer = AssetManager.item_groups.get("hammer");
                hammer.color = "#ffbaba";
                ItemGroupAsset spear = AssetManager.item_groups.get("spear");
                spear.color = "#ffbaba";
                ItemGroupAsset bow = AssetManager.item_groups.get("bow");
                bow.color = "#e6baff";
                ItemGroupAsset staff = AssetManager.item_groups.get("staff");
                staff.color = "#fffeba";
                ItemGroupAsset firearm = AssetManager.item_groups.get("firearm");
                firearm.color = "#fffeba";
            }
    //Stat Changes
			BaseStatAsset damage = AssetManager.base_stats_library.get("damage");
            damage.sort_rank = 1001;
			BaseStatAsset statarmor = AssetManager.base_stats_library.get("armor");
            statarmor.sort_rank = 1000;
			BaseStatAsset attackspeed = AssetManager.base_stats_library.get("attack_speed");
			attackspeed.sort_rank = 999;
            attackspeed.normalize_min = 0.25f; //changed from 0.5
			BaseStatAsset mattackspeed = AssetManager.base_stats_library.get("multiplier_attack_speed");
            mattackspeed.sort_rank = 998;
			BaseStatAsset criticalchance = AssetManager.base_stats_library.get("critical_chance");
            criticalchance.sort_rank = 997;
			BaseStatAsset mcriticaldamage = AssetManager.base_stats_library.get("critical_damage_multiplier");
            mcriticaldamage.sort_rank = 996;
            BaseStatAsset skillspell = AssetManager.base_stats_library.get("skill_spell");
            skillspell.sort_rank = 995;
            BaseStatAsset skillcombat = AssetManager.base_stats_library.get("skill_combat");
            skillcombat.sort_rank = 994;
			BaseStatAsset accuracy = AssetManager.base_stats_library.get("accuracy");
            accuracy.sort_rank = 993;
			BaseStatAsset range = AssetManager.base_stats_library.get("range");
            range.sort_rank = 992;
			BaseStatAsset recoil = AssetManager.base_stats_library.get("recoil");
            recoil.sort_rank = 991;
			BaseStatAsset speed = AssetManager.base_stats_library.get("speed");
            speed.sort_rank = 990;
			BaseStatAsset mana = AssetManager.base_stats_library.get("mana");
            mana.sort_rank = 989;
			BaseStatAsset stamina = AssetManager.base_stats_library.get("stamina");
            stamina.sort_rank = 988;
			BaseStatAsset damagerange = AssetManager.base_stats_library.get("damage_range");
            damagerange.sort_rank = 987;
			if (ConfigsAEA.showSomeHiddenStats){damagerange.hidden = false;}
            damagerange.show_as_percents = true; //added
            damagerange.tooltip_multiply_for_visual_number = 100f; //added
            damagerange.normalize_min = 0.05f; //changed from 0.1
			BaseStatAsset targets = AssetManager.base_stats_library.get("targets");
            targets.sort_rank = 986;
			if (ConfigsAEA.showSomeHiddenStats){targets.hidden = false;}
			BaseStatAsset projectiles = AssetManager.base_stats_library.get("projectiles");
            projectiles.sort_rank = 985;
			if (ConfigsAEA.showSomeHiddenStats){projectiles.hidden = false;}
    //Modifier Changes
            ItemModAsset divinerune = AssetManager.items_modifiers.get("divine_rune");
			divinerune.mod_rank = 0;
            ItemModAsset stun = AssetManager.items_modifiers.get("stun");
			if (ConfigsAEA.rebalanceSpecialModifiers == true){stun.action_attack_target = new AttackAction(ItemRebalance.addStunnedEffectOnTarget20Edit);} //changes from 4 to 3 seconds
                EquipmentAsset woodhammer = AssetManager.items.get("hammer_wood"); //Decreasing base value of hammers by 2 so stun doesn't inflate value
                woodhammer.equipment_value = -1;
                EquipmentAsset stonehammer = AssetManager.items.get("hammer_stone");
                stonehammer.equipment_value = 8;
                EquipmentAsset copperhammer = AssetManager.items.get("hammer_copper");
                copperhammer.equipment_value = 8;
                EquipmentAsset bronzehammer = AssetManager.items.get("hammer_bronze");
                bronzehammer.equipment_value = 13;
                EquipmentAsset silverhammer = AssetManager.items.get("hammer_silver");
                silverhammer.equipment_value = 18;
                EquipmentAsset ironhammer = AssetManager.items.get("hammer_iron");
                ironhammer.equipment_value = 28;
                EquipmentAsset steelhammer = AssetManager.items.get("hammer_steel");
                steelhammer.equipment_value = 38;
                EquipmentAsset mythrilhammer = AssetManager.items.get("hammer_mythril");
                mythrilhammer.equipment_value = 48;
                EquipmentAsset adamantinehammer = AssetManager.items.get("hammer_adamantine");
                adamantinehammer.equipment_value = 68;
                EquipmentAsset flamehammer = AssetManager.items.get("flame_hammer");
                flamehammer.equipment_value = 398;
                EquipmentAsset icehammer = AssetManager.items.get("ice_hammer");
                icehammer.equipment_value = 398;
            ItemModAsset flame = AssetManager.items_modifiers.get("flame");
            if (ConfigsAEA.rebalanceSpecialModifiers == true){flame.action_attack_target = new AttackAction(ItemRebalance.addBurningEffectOnTargetEdit);} //adds a 33% chacne and sets to 10 seconds
            ItemModAsset ice = AssetManager.items_modifiers.get("ice");
            if (ConfigsAEA.rebalanceSpecialModifiers == true){ice.action_attack_target = new AttackAction(ItemRebalance.addFrozenEffectOnTarget20Edit);} //adds a 20% chacne and sets to 3 seconds
    //Gear Transmutation Groups
            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("helmet")){AssetManager.items.pot_equipment_by_groups_all.Add("helmet", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("armor")){AssetManager.items.pot_equipment_by_groups_all.Add("armor", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("boots")){AssetManager.items.pot_equipment_by_groups_all.Add("boots", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("amulet")){AssetManager.items.pot_equipment_by_groups_all.Add("amulet", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("ring")){AssetManager.items.pot_equipment_by_groups_all.Add("ring", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("helmet")){AssetManager.items.pot_equipment_by_groups_unlocked.Add("helmet", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("armor")){AssetManager.items.pot_equipment_by_groups_unlocked.Add("armor", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("boots")){AssetManager.items.pot_equipment_by_groups_unlocked.Add("boots", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("amulet")){AssetManager.items.pot_equipment_by_groups_unlocked.Add("amulet", new List<EquipmentAsset>());}
            if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("ring")){AssetManager.items.pot_equipment_by_groups_unlocked.Add("ring", new List<EquipmentAsset>());}
	//Items Added
    	//Flame Items---------------------------------------------------------------------------------------------------------
        //Flame Sword Value Fix
            EquipmentAsset flamesword = AssetManager.items.get("flame_sword");
            flamesword.equipment_value = 400;
    	//Scale Helmet 
            EquipmentAsset scalehelmet = AssetManager.items.clone("scale_helmet", "$helmet");
            scalehelmet.needs_to_be_explored = false;
            scalehelmet.is_pool_weapon = false;
            scalehelmet.material = "basic";
            scalehelmet.group_id = "helmet";
            scalehelmet.translation_key = "scale_helmet_locale";
            scalehelmet.path_icon = "ui/icons/items/icon_scale_helmet";
            scalehelmet.setCost(10, "dragon_scales", 2, "none", 0);
            scalehelmet.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scalehelmet.base_stats.addTag("building_immunity_fire");
            scalehelmet.base_stats.addTag("immunity_fire");
            scalehelmet.base_stats["armor"] = 15f;
            scalehelmet.base_stats["mana"] = 40f;
            scalehelmet.base_stats["stamina"] = 20f;
            scalehelmet.rigidity_rating = 8;
            scalehelmet.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["helmet"].Add(scalehelmet);
            AssetManager.items.pot_equipment_by_groups_unlocked["helmet"].Add(scalehelmet);
    	//Scale Armor 
            EquipmentAsset scalearmor = AssetManager.items.clone("scale_armor", "$armor");
            scalearmor.needs_to_be_explored = false;
            scalearmor.is_pool_weapon = false;
            scalearmor.material = "basic";
            scalearmor.group_id = "armor";
            scalearmor.translation_key = "scale_armor_locale";
            scalearmor.path_icon = "ui/icons/items/icon_scale_armor";
            scalearmor.setCost(10, "dragon_scales", 2, "none", 0);
            scalearmor.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scalearmor.base_stats.addTag("building_immunity_fire");
            scalearmor.base_stats.addTag("immunity_fire");
            scalearmor.base_stats["armor"] = 15f;
            scalearmor.base_stats["mana"] = 45f;
            scalearmor.base_stats["stamina"] = 20f;
            scalearmor.rigidity_rating = 8;
            scalearmor.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["armor"].Add(scalearmor);
            AssetManager.items.pot_equipment_by_groups_unlocked["armor"].Add(scalearmor);
    	//Scale Boots 
            EquipmentAsset scaleboots = AssetManager.items.clone("scale_boots", "$boots");
            scaleboots.needs_to_be_explored = false;
            scaleboots.is_pool_weapon = false;
            scaleboots.material = "basic";
            scaleboots.group_id = "boots";
            scaleboots.translation_key = "scale_boots_locale";
            scaleboots.path_icon = "ui/icons/items/icon_scale_boots";
            scaleboots.setCost(10, "dragon_scales", 2, "none", 0);
            scaleboots.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scaleboots.base_stats.addTag("building_immunity_fire");
            scaleboots.base_stats.addTag("immunity_fire");
            scaleboots.base_stats["armor"] = 15f;
            scaleboots.base_stats["mana"] = 40f;
            scaleboots.base_stats["stamina"] = 20f;
            scaleboots.rigidity_rating = 8;
            scaleboots.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["boots"].Add(scaleboots);
            AssetManager.items.pot_equipment_by_groups_unlocked["boots"].Add(scaleboots);
    	//Scale Amulet 
            EquipmentAsset scaleamulet = AssetManager.items.clone("scale_amulet", "$amulet");
            scaleamulet.needs_to_be_explored = false;
            scaleamulet.is_pool_weapon = false;
            scaleamulet.material = "basic";
            scaleamulet.group_id = "amulet";
            scaleamulet.translation_key = "scale_amulet_locale";
            scaleamulet.path_icon = "ui/icons/items/icon_scale_amulet";
            scaleamulet.setCost(10, "dragon_scales", 2, "gems", 1);
            scaleamulet.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scaleamulet.base_stats.addTag("building_immunity_fire");
            scaleamulet.base_stats.addTag("immunity_fire");
            scaleamulet.base_stats["critical_chance"] = 0.15f;
            scaleamulet.base_stats["mana"] = 25f;
            scaleamulet.base_stats["stamina"] = 10f;
            scaleamulet.rigidity_rating = 8;
            scaleamulet.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["amulet"].Add(scaleamulet);
            AssetManager.items.pot_equipment_by_groups_unlocked["amulet"].Add(scaleamulet);
    	//Scale Ring 
            EquipmentAsset scalering = AssetManager.items.clone("scale_ring", "$ring");
            scalering.needs_to_be_explored = false;
            scalering.is_pool_weapon = false;
            scalering.material = "basic";
            scalering.group_id = "ring";
            scalering.translation_key = "scale_ring_locale";
            scalering.path_icon = "ui/icons/items/icon_scale_ring";
            scalering.setCost(10, "dragon_scales", 2, "gems", 1);
            scalering.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            scalering.base_stats.addTag("building_immunity_fire");
            scalering.base_stats.addTag("immunity_fire");
            scalering.base_stats["critical_chance"] = 0.15f;
            scalering.base_stats["mana"] = 25f;
            scalering.base_stats["stamina"] = 10f;
            scalering.rigidity_rating = 8;
            scalering.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["ring"].Add(scalering);
            AssetManager.items.pot_equipment_by_groups_unlocked["ring"].Add(scalering);
    	//Flame Axe
            EquipmentAsset flameaxe = AssetManager.items.clone("flame_axe", "$melee");
            flameaxe.needs_to_be_explored = false;
            flameaxe.animated = true;
            flameaxe.is_pool_weapon = true;
            flameaxe.metallic = true;
            flameaxe.group_id = "axe";
            flameaxe.translation_key = "flame_axe_locale";
            flameaxe.path_icon = "ui/icons/items/icon_flame_axe";
            flameaxe.path_gameplay_sprite = "weapons/flame_axe";
            flameaxe.path_slash_animation = "effects/slashes/slash_axe";
            flameaxe.setCost(15, "dragon_scales", 3, "none", 0);
            flameaxe.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_hammer_name");
            flameaxe.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flameaxe.base_stats.addTag("building_immunity_fire");
            flameaxe.base_stats["damage"] = 35f;
			flameaxe.base_stats["critical_chance"] = 0.25f;
            flameaxe.base_stats["critical_damage_multiplier"] = 0.1f;
            flameaxe.base_stats["damage_range"] = 0.6f;
            flameaxe.base_stats["targets"] = 1f;
            flameaxe.base_stats["mana"] = 15f;
            flameaxe.base_stats["stamina"] = 15f;
            flameaxe.pool_rate = 1;
            flameaxe.rigidity_rating = 8;
            flameaxe.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flameaxe);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flameaxe);
    	//Flame Spear
            EquipmentAsset flamespear = AssetManager.items.clone("flame_spear", "$melee");
            flamespear.needs_to_be_explored = false;
            flamespear.animated = true;
            flamespear.is_pool_weapon = true;
            flamespear.metallic = true;
            flamespear.group_id = "spear";
            flamespear.translation_key = "flame_spear_locale";
            flamespear.path_icon = "ui/icons/items/icon_flame_spear";
            flamespear.path_gameplay_sprite = "weapons/flame_spear";
            flamespear.path_slash_animation = "effects/slashes/slash_spear";
            flamespear.setCost(15, "dragon_scales", 3, "none", 0);
            flamespear.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flamespear.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flamespear.base_stats.addTag("building_immunity_fire");
            flamespear.base_stats["damage"] = 25f;
            flamespear.base_stats["critical_damage_multiplier"] = 0.1f;
            flamespear.base_stats["range"] = 1.1f;
			flamespear.base_stats["damage_range"] = 0.7f;
            flamespear.base_stats["targets"] = 1f;
            flamespear.base_stats["mana"] = 15f;
            flamespear.base_stats["stamina"] = 15f;
            flamespear.rigidity_rating = 8;
            flamespear.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flamespear);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flamespear);
    	//Flame Bow
            EquipmentAsset flamebow = AssetManager.items.clone("flame_bow", "$range");
            flamebow.needs_to_be_explored = false;
            flamebow.animated = true;
            flamebow.is_pool_weapon = true;
            flamebow.metallic = true;
            flamebow.group_id = "bow";
            flamebow.projectile = "arrow";
            flamebow.translation_key = "flame_bow_locale";
            flamebow.path_icon = "ui/icons/items/icon_flame_bow";
            flamebow.path_gameplay_sprite = "weapons/flame_bow";
            flamebow.path_slash_animation = "effects/slashes/slash_bow";
            flamebow.setCost(15, "dragon_scales", 3, "none", 0);
            flamebow.name_templates = AssetLibrary<ItemAsset>.l<string>("flame_sword_name");
            flamebow.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"flame"});
            flamebow.base_stats.addTag("building_immunity_fire");
            flamebow.base_stats["damage"] = 22f;
            flamebow.base_stats["critical_chance"] = 0.25f;
            flamebow.base_stats["critical_damage_multiplier"] = 0.6f;
            flamebow.base_stats["accuracy"] = 0.3f;
            flamebow.base_stats["range"] = 13f;
			flamebow.base_stats["targets"] = 1f;
            flamebow.base_stats["mana"] = 15f;
            flamebow.base_stats["stamina"] = 15f;
            flamebow.pool_rate = 1;
            flamebow.rigidity_rating = 8;
            flamebow.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(flamebow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(flamebow);
    	//Ice Items---------------------------------------------------------------------------------------------------------
    	//Ice Helmet
            EquipmentAsset icehelmet = AssetManager.items.clone("ice_helmet", "$helmet");
            icehelmet.needs_to_be_explored = false;
            icehelmet.is_pool_weapon = false;
            icehelmet.material = "basic";
            icehelmet.group_id = "helmet";
            icehelmet.translation_key = "ice_helmet_locale";
            icehelmet.path_icon = "ui/icons/items/icon_ice_helmet";
            icehelmet.setCost(10, "mythril", 10, "gems", 2);
            icehelmet.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icehelmet.base_stats.addTag("immunity_cold");
            icehelmet.base_stats["armor"] = 12f;
            icehelmet.base_stats["speed"] = 3f;
            icehelmet.base_stats["mana"] = 35f;
            icehelmet.base_stats["stamina"] = 15f;
            icehelmet.rigidity_rating = 8;
            icehelmet.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["helmet"].Add(icehelmet);
            AssetManager.items.pot_equipment_by_groups_unlocked["helmet"].Add(icehelmet);
    	//Ice Armor
            EquipmentAsset icearmor = AssetManager.items.clone("ice_armor", "$armor");
            icearmor.needs_to_be_explored = false;
            icearmor.is_pool_weapon = false;
            icearmor.material = "basic";
            icearmor.group_id = "armor";
            icearmor.translation_key = "ice_armor_locale";
            icearmor.path_icon = "ui/icons/items/icon_ice_armor";
            icearmor.setCost(10, "mythril", 10, "gems", 2);
            icearmor.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icearmor.base_stats.addTag("immunity_cold");
            icearmor.base_stats["armor"] = 12f;
            icearmor.base_stats["speed"] = 3f;
            icearmor.base_stats["mana"] = 40f;
            icearmor.base_stats["stamina"] = 15f;
            icearmor.rigidity_rating = 8;
            icearmor.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["armor"].Add(icearmor);
            AssetManager.items.pot_equipment_by_groups_unlocked["armor"].Add(icearmor);
    	//Ice Boots
            EquipmentAsset iceboots = AssetManager.items.clone("ice_boots", "$boots");
            iceboots.needs_to_be_explored = false;
            iceboots.is_pool_weapon = false;
            iceboots.material = "basic";
            iceboots.group_id = "boots";
            iceboots.translation_key = "ice_boots_locale";
            iceboots.path_icon = "ui/icons/items/icon_ice_boots";
            iceboots.setCost(10, "mythril", 10, "gems", 2);
            iceboots.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            iceboots.base_stats.addTag("immunity_cold");
            iceboots.base_stats["armor"] = 12f;
            iceboots.base_stats["speed"] = 3f;
            iceboots.base_stats["mana"] = 35f;
            iceboots.base_stats["stamina"] = 15f;
            iceboots.rigidity_rating = 8;
            iceboots.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["boots"].Add(iceboots);
            AssetManager.items.pot_equipment_by_groups_unlocked["boots"].Add(iceboots);
    	//Ice Amulet 
            EquipmentAsset iceamulet = AssetManager.items.clone("ice_amulet", "$amulet");
            iceamulet.needs_to_be_explored = false;
            iceamulet.is_pool_weapon = false;
            iceamulet.material = "basic";
            iceamulet.group_id = "amulet";
            iceamulet.translation_key = "ice_amulet_locale";
            iceamulet.path_icon = "ui/icons/items/icon_ice_amulet";
            iceamulet.setCost(10, "mythril", 8, "gems", 1);
            iceamulet.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            iceamulet.base_stats.addTag("immunity_cold");
            iceamulet.base_stats["critical_chance"] = 0.12f;
            iceamulet.base_stats["speed"] = 3f;
            iceamulet.base_stats["mana"] = 20f;
            iceamulet.base_stats["stamina"] = 10f;
            iceamulet.rigidity_rating = 8;
            iceamulet.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["amulet"].Add(iceamulet);
            AssetManager.items.pot_equipment_by_groups_unlocked["amulet"].Add(iceamulet);
    	//Ice Ring
            EquipmentAsset icering = AssetManager.items.clone("ice_ring", "$ring");
            icering.needs_to_be_explored = false;
            icering.is_pool_weapon = false;
            icering.material = "basic";
            icering.group_id = "ring";
            icering.translation_key = "ice_ring_locale";
            icering.path_icon = "ui/icons/items/icon_ice_ring";
            icering.setCost(10, "mythril", 8, "gems", 1);
            icering.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icering.base_stats.addTag("immunity_cold");
            icering.base_stats["critical_chance"] = 0.12f;
            icering.base_stats["speed"] = 3f;
            icering.base_stats["mana"] = 20f;
            icering.base_stats["stamina"] = 10f;
            icering.rigidity_rating = 8;
            icering.equipment_value = 400;
			AssetManager.items.pot_equipment_by_groups_all["ring"].Add(icering);
            AssetManager.items.pot_equipment_by_groups_unlocked["ring"].Add(icering);
    	//Ice Sword
            EquipmentAsset icesword = AssetManager.items.clone("ice_sword", "$melee");
            icesword.needs_to_be_explored = false;
            icesword.is_pool_weapon = true;
            icesword.metallic = true;
            icesword.group_id = "sword";
            icesword.translation_key = "ice_sword_locale";
            icesword.path_icon = "ui/icons/items/icon_ice_sword";
            icesword.path_gameplay_sprite = "weapons/ice_sword";
            icesword.path_slash_animation = "effects/slashes/slash_sword";
            icesword.setCost(10, "mythril", 10, "gems", 2);
            icesword.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icesword.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icesword.base_stats["damage"] = 25f;
            icesword.base_stats["multiplier_attack_speed"] = 0.05f;
			icesword.base_stats["critical_chance"] = 0.15f;
			icesword.base_stats["damage_range"] = 0.8f;
            icesword.base_stats["speed"] = 3f;
            icesword.base_stats["mana"] = 15f;
            icesword.base_stats["stamina"] = 15f;
            icesword.rigidity_rating = 8;
            icesword.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(icesword);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icesword);
    	//Ice Axe
            EquipmentAsset iceaxe = AssetManager.items.clone("ice_axe", "$melee");
            iceaxe.needs_to_be_explored = false;
            iceaxe.is_pool_weapon = true;
            iceaxe.metallic = true;
            iceaxe.group_id = "axe";
            iceaxe.translation_key = "ice_axe_locale";
            iceaxe.path_icon = "ui/icons/items/icon_ice_axe";
            iceaxe.path_gameplay_sprite = "weapons/ice_axe";
            iceaxe.path_slash_animation = "effects/slashes/slash_axe";
            iceaxe.setCost(10, "mythril", 10, "gems", 2);
            iceaxe.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            iceaxe.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            iceaxe.base_stats["damage"] = 28f;
            iceaxe.base_stats["multiplier_attack_speed"] = 0.05f;
			iceaxe.base_stats["critical_chance"] = 0.25f;
            iceaxe.base_stats["damage_range"] = 0.6f;
            iceaxe.base_stats["speed"] = 3f;
            iceaxe.base_stats["mana"] = 15f;
            iceaxe.base_stats["stamina"] = 15f;
            iceaxe.rigidity_rating = 8;
            iceaxe.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(iceaxe);
            AssetManager.items.pot_weapon_assets_unlocked.Add(iceaxe);
    	//Ice Spear
            EquipmentAsset icespear = AssetManager.items.clone("ice_spear", "$melee");
            icespear.needs_to_be_explored = false;
            icespear.is_pool_weapon = true;
            icespear.metallic = true;
            icespear.group_id = "spear";
            icespear.translation_key = "ice_spear_locale";
            icespear.path_icon = "ui/icons/items/icon_ice_spear";
            icespear.path_gameplay_sprite = "weapons/ice_spear";
            icespear.path_slash_animation = "effects/slashes/slash_spear";
            icespear.setCost(10, "mythril", 8, "gems", 2);
            icespear.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icespear.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icespear.base_stats["damage"] = 20f;
            icespear.base_stats["multiplier_attack_speed"] = 0.05f;
            icespear.base_stats["critical_chance"] = 0.15f;
			icespear.base_stats["range"] = 1.1f;
            icespear.base_stats["damage_range"] = 0.7f;
            icespear.base_stats["speed"] = 3f;
            icespear.base_stats["mana"] = 15f;
            icespear.base_stats["stamina"] = 15f;
            icespear.rigidity_rating = 8;
            icespear.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(icespear);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icespear);
    	//Ice Bow
            EquipmentAsset icebow = AssetManager.items.clone("ice_bow", "$range");
            icebow.needs_to_be_explored = false;
            icebow.is_pool_weapon = true;
            icebow.metallic = true;
            icebow.group_id = "bow";
            icebow.projectile = "arrow";
            icebow.translation_key = "ice_bow_locale";
            icebow.path_icon = "ui/icons/items/icon_ice_bow";
            icebow.path_gameplay_sprite = "weapons/ice_bow";
            icebow.path_slash_animation = "effects/slashes/slash_bow";
            icebow.setCost(15, "mythril", 10, "gems", 2);
            icebow.name_templates = AssetLibrary<ItemAsset>.l<string>("ice_hammer_name");
            icebow.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"ice"});
            icebow.base_stats["damage"] = 18f;
            icebow.base_stats["multiplier_attack_speed"] = 0.05f;
            icebow.base_stats["critical_chance"] = 0.2f;
            icebow.base_stats["critical_damage_multiplier"] = 0.5f;
            icebow.base_stats["accuracy"] = 0.3f;
            icebow.base_stats["range"] = 13f;
            icebow.base_stats["recoil"] = 0.8f;
            icebow.base_stats["damage_range"] = 0.65f;
            icebow.base_stats["speed"] = 3f;
            icebow.base_stats["mana"] = 15f;
            icebow.base_stats["stamina"] = 15f;
            icebow.rigidity_rating = 8;
            icebow.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(icebow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(icebow);
    	//Gold Items---------------------------------------------------------------------------------------------------------
    	//Gold Helmet 
            EquipmentAsset goldhelmet = AssetManager.items.clone("gold_helmet", "$helmet");
            goldhelmet.needs_to_be_explored = false;
            goldhelmet.equipment_subtype = null;
            goldhelmet.material = "gold";
            goldhelmet.group_id = "helmet";
            goldhelmet.translation_key = "gold_helmet_locale";
            goldhelmet.path_icon = "ui/icons/items/icon_gold_helmet";
            goldhelmet.path_gameplay_sprite = "weapons/gold_helmet";
            goldhelmet.setCost(0, "gold", 2, "common_metals", 2);
            goldhelmet.base_stats["armor"] = 5.5f;
            goldhelmet.base_stats["mana"] = 10f;
            goldhelmet.base_stats["stamina"] = 10f;
            goldhelmet.rigidity_rating = 2;
            goldhelmet.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "helmet_silver");
            AssetManager.items.list.Remove(goldhelmet);
            AssetManager.items.list.Insert(targetIndex + 1, goldhelmet);
			AssetManager.items.pot_equipment_by_groups_all["helmet"].Add(goldhelmet);
            AssetManager.items.pot_equipment_by_groups_unlocked["helmet"].Add(goldhelmet);
    	//Gold Armor
            EquipmentAsset goldarmor = AssetManager.items.clone("gold_armor", "$armor");
            goldarmor.needs_to_be_explored = false;
            goldarmor.equipment_subtype = null;
            goldarmor.material = "gold";
            goldarmor.group_id = "armor";
            goldarmor.translation_key = "gold_armor_locale";
            goldarmor.path_icon = "ui/icons/items/icon_gold_armor";
            goldarmor.path_gameplay_sprite = "weapons/gold_armor";
            goldarmor.setCost(0, "gold", 2, "common_metals", 2);
            goldarmor.base_stats["armor"] = 5.5f;
            goldarmor.base_stats["mana"] = 10f;
            goldarmor.base_stats["stamina"] = 10f;
            goldarmor.rigidity_rating = 2;
            goldarmor.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "armor_silver");
            AssetManager.items.list.Remove(goldarmor);
            AssetManager.items.list.Insert(targetIndex + 1, goldarmor);
			AssetManager.items.pot_equipment_by_groups_all["armor"].Add(goldarmor);
            AssetManager.items.pot_equipment_by_groups_unlocked["armor"].Add(goldarmor);
    	//Gold Boots
            EquipmentAsset goldboots = AssetManager.items.clone("gold_boots", "$boots");
            goldboots.needs_to_be_explored = false;
            goldboots.equipment_subtype = null;
            goldboots.material = "gold";
            goldboots.group_id = "boots";
            goldboots.translation_key = "gold_boots_locale";
            goldboots.path_icon = "ui/icons/items/icon_gold_boots";
            goldboots.path_gameplay_sprite = "weapons/gold_boots";
            goldboots.setCost(0, "gold", 2, "common_metals", 2);
            goldboots.base_stats["armor"] = 5.5f;
            goldboots.base_stats["mana"] = 10f;
            goldboots.base_stats["stamina"] = 10f;
            goldboots.rigidity_rating = 2;
            goldboots.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "boots_silver");
            AssetManager.items.list.Remove(goldboots);
            AssetManager.items.list.Insert(targetIndex + 1, goldboots);
			AssetManager.items.pot_equipment_by_groups_all["boots"].Add(goldboots);
            AssetManager.items.pot_equipment_by_groups_unlocked["boots"].Add(goldboots);
    	//Gold Ring
            EquipmentAsset goldring = AssetManager.items.clone("gold_ring", "$ring");
            goldring.needs_to_be_explored = false;
            goldring.equipment_subtype = null;
            goldring.material = "gold";
            goldring.group_id = "ring";
            goldring.translation_key = "gold_ring_locale";
            goldring.path_icon = "ui/icons/items/icon_gold_ring";
            goldring.path_gameplay_sprite = "weapons/gold_ring";
            goldring.setCost(0, "gold", 3, "gems", 2);
            goldring.base_stats["critical_chance"] = 0.055f;
            goldring.base_stats["mana"] = 5f;
            goldring.base_stats["stamina"] = 5f;
            goldring.rigidity_rating = 2;
            goldring.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "ring_silver");
            AssetManager.items.list.Remove(goldring);
            AssetManager.items.list.Insert(targetIndex + 1, goldring);
			AssetManager.items.pot_equipment_by_groups_all["ring"].Add(goldring);
            AssetManager.items.pot_equipment_by_groups_unlocked["ring"].Add(goldring);
    	//Gold Amulet
            EquipmentAsset goldamulet = AssetManager.items.clone("gold_amulet", "$amulet");
            goldamulet.needs_to_be_explored = false;
            goldamulet.equipment_subtype = null;
            goldamulet.material = "gold";
            goldamulet.group_id = "amulet";
            goldamulet.translation_key = "gold_amulet_locale";
            goldamulet.path_icon = "ui/icons/items/icon_gold_amulet";
            goldamulet.path_gameplay_sprite = "weapons/gold_amulet";
            goldamulet.setCost(0, "gold", 3, "gems", 2);
            goldamulet.base_stats["critical_chance"] = 0.055f;
            goldamulet.base_stats["mana"] = 5f;
            goldamulet.base_stats["stamina"] = 5f;
            goldamulet.rigidity_rating = 2;
            goldamulet.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "amulet_silver");
            AssetManager.items.list.Remove(goldamulet);
            AssetManager.items.list.Insert(targetIndex + 1, goldamulet);
			AssetManager.items.pot_equipment_by_groups_all["amulet"].Add(goldamulet);
            AssetManager.items.pot_equipment_by_groups_unlocked["amulet"].Add(goldamulet);
    	//Gold Sword
            EquipmentAsset goldsword = AssetManager.items.clone("gold_sword", "$sword");
            goldsword.needs_to_be_explored = false;
            goldsword.is_pool_weapon = true;
            goldsword.metallic = true;
            goldsword.equipment_subtype = null;
            goldsword.material = "gold";
            goldsword.translation_key = "gold_sword_locale";
            goldsword.path_icon = "ui/icons/items/icon_gold_sword";
            goldsword.path_gameplay_sprite = "weapons/gold_sword";
            goldsword.setCost(0, "gold", 2, "common_metals", 2);
            goldsword.base_stats["damage"] = 5f;
            goldsword.base_stats["multiplier_attack_speed"] = 0.05f;
            goldsword.base_stats["mana"] = 10f;
            goldsword.base_stats["stamina"] = 10f;
            goldsword.rigidity_rating = 2;
            goldsword.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "sword_silver");
            AssetManager.items.list.Remove(goldsword);
            AssetManager.items.list.Insert(targetIndex + 1, goldsword);
            AssetManager.items.pot_weapon_assets_all.Add(goldsword);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldsword);
    	//Gold Axe
            EquipmentAsset goldaxe = AssetManager.items.clone("gold_axe", "$axe");
            goldaxe.needs_to_be_explored = false;
            goldaxe.is_pool_weapon = true;
            goldaxe.metallic = true;
            goldaxe.equipment_subtype = null;
            goldaxe.material = "gold";
            goldaxe.translation_key = "gold_axe_locale";
            goldaxe.path_icon = "ui/icons/items/icon_gold_axe";
            goldaxe.path_gameplay_sprite = "weapons/gold_axe";
            goldaxe.setCost(0, "gold", 2, "common_metals", 2);
            goldaxe.base_stats["damage"] = 5f;
            goldaxe.base_stats["multiplier_attack_speed"] = 0.05f;
            goldaxe.base_stats["mana"] = 10f;
            goldaxe.base_stats["stamina"] = 10f;
            goldaxe.rigidity_rating = 2;
            goldaxe.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "axe_silver");
            AssetManager.items.list.Remove(goldaxe);
            AssetManager.items.list.Insert(targetIndex + 1, goldaxe);
            AssetManager.items.pot_weapon_assets_all.Add(goldaxe);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldaxe);
    	//Gold Hammer
            EquipmentAsset goldhammer = AssetManager.items.clone("gold_hammer", "$hammer");
            goldhammer.needs_to_be_explored = false;
            goldhammer.is_pool_weapon = true;
            goldhammer.metallic = true;
            goldhammer.equipment_subtype = null;
            goldhammer.material = "gold";
            goldhammer.translation_key = "gold_hammer_locale";
            goldhammer.path_icon = "ui/icons/items/icon_gold_hammer";
            goldhammer.path_gameplay_sprite = "weapons/gold_hammer";
            goldhammer.setCost(0, "gold", 2, "common_metals", 2);
            goldhammer.base_stats["damage"] = 5f;
            goldhammer.base_stats["multiplier_attack_speed"] = 0.05f;
            goldhammer.base_stats["mana"] = 10f;
            goldhammer.base_stats["stamina"] = 10f;
            goldhammer.rigidity_rating = 2;
            goldhammer.equipment_value = 23; //to accommodate for stun modifier value
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "hammer_silver");
            AssetManager.items.list.Remove(goldhammer);
            AssetManager.items.list.Insert(targetIndex + 1, goldhammer);
            AssetManager.items.pot_weapon_assets_all.Add(goldhammer);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldhammer);
    	//Gold Spear
            EquipmentAsset goldspear = AssetManager.items.clone("gold_spear", "$spear");
            goldspear.needs_to_be_explored = false;
            goldspear.is_pool_weapon = true;
            goldspear.metallic = true;
            goldspear.equipment_subtype = null;
            goldspear.material = "gold";
            goldspear.translation_key = "gold_spear_locale";
            goldspear.path_icon = "ui/icons/items/icon_gold_spear";
            goldspear.path_gameplay_sprite = "weapons/gold_spear";
            goldspear.setCost(0, "gold", 2, "common_metals", 2);
            goldspear.base_stats["damage"] = 5f;
            goldspear.base_stats["multiplier_attack_speed"] = 0.05f;
            goldspear.base_stats["mana"] = 10f;
            goldspear.base_stats["stamina"] = 10f;
            goldspear.rigidity_rating = 2;
            goldspear.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "spear_silver");
            AssetManager.items.list.Remove(goldspear);
            AssetManager.items.list.Insert(targetIndex + 1, goldspear);
            AssetManager.items.pot_weapon_assets_all.Add(goldspear);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldspear);
    	//Gold Bow
            EquipmentAsset goldbow = AssetManager.items.clone("gold_bow", "$bow");
            goldbow.needs_to_be_explored = false;
            goldbow.is_pool_weapon = true;
            goldbow.metallic = true;
            goldbow.equipment_subtype = null;
            goldbow.material = "gold";
            goldbow.translation_key = "gold_bow_locale";
            goldbow.path_icon = "ui/icons/items/icon_gold_bow";
            goldbow.path_gameplay_sprite = "weapons/gold_bow";
            goldbow.setCost(0, "gold", 2, "common_metals", 2);
            goldbow.base_stats["damage"] = 5f;
            goldbow.base_stats["multiplier_attack_speed"] = 0.05f;
            goldbow.base_stats["range"] = 8f;
            goldbow.base_stats["mana"] = 10f;
            goldbow.base_stats["stamina"] = 10f;
            goldbow.rigidity_rating = 2;
            goldbow.equipment_value = 25;
            targetIndex = AssetManager.items.list.FindIndex(a => a.id == "bow_silver");
            AssetManager.items.list.Remove(goldbow);
            AssetManager.items.list.Insert(targetIndex + 1, goldbow);
            AssetManager.items.pot_weapon_assets_all.Add(goldbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(goldbow);
	//Item Config Adjustments
            if(ConfigsAEA.scciVal == 1){ //light
                icearmor.path_icon = "ui/icons/items/icon_ice_armor_lightC";
                iceboots.path_icon = "ui/icons/items/icon_ice_boots_lightC";
                iceamulet.path_icon = "ui/icons/items/icon_ice_amulet_lightC";
                icesword.path_icon = "ui/icons/items/icon_ice_sword_lightC";
                icespear.path_icon = "ui/icons/items/icon_ice_spear_lightC";
                iceaxe.path_icon = "ui/icons/items/icon_ice_axe_lightC";
                icebow.path_icon = "ui/icons/items/icon_ice_bow_lightC";
                //EquipmentAsset icehammer = AssetManager.items.get("ice_hammer"); //already called above
                icehammer.path_icon = "ui/icons/items/icon_ice_hammer_lightC";
            }
            if(ConfigsAEA.changeItemSprites){
                EquipmentAsset copperspear = AssetManager.items.get("spear_copper");
                copperspear.path_icon = "ui/icons/items/avbchanges/icon_copper_spear";
                EquipmentAsset copperarmor = AssetManager.items.get("armor_copper");
                copperarmor.path_icon = "ui/icons/items/avbchanges/icon_copper_armor";
                EquipmentAsset ironamulet = AssetManager.items.get("amulet_iron");
                ironamulet.path_icon = "ui/icons/items/avbchanges/icon_iron_amulet";
                EquipmentAsset mythrilamulet = AssetManager.items.get("amulet_mythril");
                mythrilamulet.path_icon = "ui/icons/items/avbchanges/icon_mythril_amulet";
                EquipmentAsset stoneaxe = AssetManager.items.get("axe_stone");
                stoneaxe.path_icon = "ui/icons/items/avbchanges/icon_stone_axe";
				EquipmentAsset woodsword = AssetManager.items.get("sword_wood");
                woodsword.path_icon = "ui/icons/items/avbchanges/icon_wood_sword";
                //EquipmentAsset flamesword = AssetManager.items.get("flame_sword"); //already called above
                flamesword.path_icon = "ui/icons/items/avbchanges/icon_flame_sword";
                EquipmentAsset plaguedoctorstaff = AssetManager.items.get("plague_doctor_staff");
                plaguedoctorstaff.path_icon = "ui/icons/items/avbchanges/icon_plague_doctor_staff";
                CultureTrait plaguedoctorstafftrait = AssetManager.culture_traits.get("craft_doctor_staff");
                plaguedoctorstafftrait.path_icon = "ui/icons/culture_trait_craft_doctor_staff";
                EquipmentAsset alienblaster = AssetManager.items.get("alien_blaster");
                alienblaster.path_icon = "ui/icons/items/avbchanges/icon_alien_blaster";

                EquipmentAsset bronzesword = AssetManager.items.get("sword_bronze");
                bronzesword.path_icon = "ui/icons/items/avbchanges/icon_bronze_sword";
                EquipmentAsset bronzeaxe = AssetManager.items.get("axe_bronze");
                bronzeaxe.path_icon = "ui/icons/items/avbchanges/icon_bronze_axe";
                //EquipmentAsset bronzehammer = AssetManager.items.get("hammer_bronze"); //already called above
                bronzehammer.path_icon = "ui/icons/items/avbchanges/icon_bronze_hammer";
                EquipmentAsset bronzespear = AssetManager.items.get("spear_bronze");
                bronzespear.path_icon = "ui/icons/items/avbchanges/icon_bronze_spear";
                EquipmentAsset bronzebow = AssetManager.items.get("bow_bronze");
                bronzebow.path_icon = "ui/icons/items/avbchanges/icon_bronze_bow";
                EquipmentAsset bronzehelmet = AssetManager.items.get("helmet_bronze");
                bronzehelmet.path_icon = "ui/icons/items/avbchanges/icon_bronze_helmet";
                EquipmentAsset bronzearmor = AssetManager.items.get("armor_bronze");
                bronzearmor.path_icon = "ui/icons/items/avbchanges/icon_bronze_armor";
                EquipmentAsset bronzeboots = AssetManager.items.get("boots_bronze");
                bronzeboots.path_icon = "ui/icons/items/avbchanges/icon_bronze_boots";
                EquipmentAsset bronzeamulet = AssetManager.items.get("amulet_bronze");
                bronzeamulet.path_icon = "ui/icons/items/avbchanges/icon_bronze_amulet";
                EquipmentAsset bronzering = AssetManager.items.get("ring_bronze");
                bronzering.path_icon = "ui/icons/items/avbchanges/icon_bronze_ring";
                
                EquipmentAsset silversword = AssetManager.items.get("sword_silver");
                silversword.path_icon = "ui/icons/items/avbchanges/icon_silver_sword";
                EquipmentAsset silveraxe = AssetManager.items.get("axe_silver");
                silveraxe.path_icon = "ui/icons/items/avbchanges/icon_silver_axe";
                //EquipmentAsset silverhammer = AssetManager.items.get("hammer_silver"); //already called above
                silverhammer.path_icon = "ui/icons/items/avbchanges/icon_silver_hammer";
                EquipmentAsset silverspear = AssetManager.items.get("spear_silver");
                silverspear.path_icon = "ui/icons/items/avbchanges/icon_silver_spear";
                EquipmentAsset silverbow = AssetManager.items.get("bow_silver");
                silverbow.path_icon = "ui/icons/items/avbchanges/icon_silver_bow";
                EquipmentAsset silverhelmet = AssetManager.items.get("helmet_silver");
                silverhelmet.path_icon = "ui/icons/items/avbchanges/icon_silver_helmet";
                EquipmentAsset silverarmor = AssetManager.items.get("armor_silver");
                silverarmor.path_icon = "ui/icons/items/avbchanges/icon_silver_armor";
                EquipmentAsset silverboots = AssetManager.items.get("boots_silver");
                silverboots.path_icon = "ui/icons/items/avbchanges/icon_silver_boots";
                EquipmentAsset silveramulet = AssetManager.items.get("amulet_silver");
                silveramulet.path_icon = "ui/icons/items/avbchanges/icon_silver_amulet";
                EquipmentAsset silverring = AssetManager.items.get("ring_silver");
                silverring.path_icon = "ui/icons/items/avbchanges/icon_silver_ring";

                EquipmentAsset adamantinesword = AssetManager.items.get("sword_adamantine");
                adamantinesword.path_icon = "ui/icons/items/avbchanges/icon_adamantine_sword";
                EquipmentAsset adamantineaxe = AssetManager.items.get("axe_adamantine");
                adamantineaxe.path_icon = "ui/icons/items/avbchanges/icon_adamantine_axe";
                // EquipmentAsset adamantinehammer = AssetManager.items.get("hammer_adamantine"); //already called above
                adamantinehammer.path_icon = "ui/icons/items/avbchanges/icon_adamantine_hammer";
                EquipmentAsset adamantinespear = AssetManager.items.get("spear_adamantine");
                adamantinespear.path_icon = "ui/icons/items/avbchanges/icon_adamantine_spear";
                EquipmentAsset adamantinebow = AssetManager.items.get("bow_adamantine");
                adamantinebow.path_icon = "ui/icons/items/avbchanges/icon_adamantine_bow";
                EquipmentAsset adamantinehelmet = AssetManager.items.get("helmet_adamantine");
                adamantinehelmet.path_icon = "ui/icons/items/avbchanges/icon_adamantine_helmet";
                EquipmentAsset adamantinearmor = AssetManager.items.get("armor_adamantine");
                adamantinearmor.path_icon = "ui/icons/items/avbchanges/icon_adamantine_armor";
                EquipmentAsset adamantineboots = AssetManager.items.get("boots_adamantine");
                adamantineboots.path_icon = "ui/icons/items/avbchanges/icon_adamantine_boots";
                EquipmentAsset adamantineamulet = AssetManager.items.get("amulet_adamantine");
                adamantineamulet.path_icon = "ui/icons/items/avbchanges/icon_adamantine_amulet";
                EquipmentAsset adamantinering = AssetManager.items.get("ring_adamantine");
                adamantinering.path_icon = "ui/icons/items/avbchanges/icon_adamantine_ring";
            }
            if(ConfigsAEA.bronzeIronSteelRecipeFix){
                EquipmentAsset bronzeamulet = AssetManager.items.get("amulet_bronze");
                bronzeamulet.setCost(0, "common_metals", 2, "gems", 1);
                EquipmentAsset bronzering = AssetManager.items.get("ring_bronze");
                bronzering.setCost(0, "common_metals", 2, "gems", 1);

                EquipmentAsset ironamulet = AssetManager.items.get("amulet_iron");
                ironamulet.setCost(0, "common_metals", 3, "gems", 1);
                EquipmentAsset ironring = AssetManager.items.get("ring_iron");
                ironring.setCost(0, "common_metals", 3, "gems", 1);

                EquipmentAsset steelamulet = AssetManager.items.get("amulet_steel");
                steelamulet.setCost(0, "common_metals", 4, "gems", 1);
                EquipmentAsset steelring = AssetManager.items.get("ring_steel");
                steelring.setCost(0, "common_metals", 4, "gems", 1);
            }
	//Culture Trait Implements
			CultureTrait craftflame = AssetManager.culture_traits.get("craft_flame_weapon");
			if (ConfigsAEA.flameTierCraft == true){
                if(ConfigsAEA.helmetCraft == true){craftflame.related_weapons_ids.Add("scale_helmet");}
                craftflame.related_weapons_ids.Add("scale_armor");
                craftflame.related_weapons_ids.Add("scale_boots");
                craftflame.related_weapons_ids.Add("scale_amulet");
                craftflame.related_weapons_ids.Add("scale_ring");
				if(ConfigsAEA.swordCraft == false){craftflame.related_weapons_ids.Remove("flame_sword");}
				if(ConfigsAEA.axeCraft == true){craftflame.related_weapons_ids.Add("flame_axe");}
				if(ConfigsAEA.hammerCraft == false){craftflame.related_weapons_ids.Remove("flame_hammer");}
				if(ConfigsAEA.spearCraft == true){craftflame.related_weapons_ids.Add("flame_spear");}
				if(ConfigsAEA.bowCraft == true){craftflame.related_weapons_ids.Add("flame_bow");}
            }
			else {
				if(ConfigsAEA.swordCraft == false){craftflame.related_weapons_ids.Remove("flame_sword");}
				if(ConfigsAEA.hammerCraft == false){craftflame.related_weapons_ids.Remove("flame_hammer");}
			}
			CultureTrait craftice = AssetManager.culture_traits.get("craft_ice_weapon");
            if (ConfigsAEA.iceTierCraft == true){
                craftice.related_weapons_ids.Add("ice_helmet");
                craftice.related_weapons_ids.Add("ice_armor");
                craftice.related_weapons_ids.Add("ice_boots");
                craftice.related_weapons_ids.Add("ice_amulet");
                craftice.related_weapons_ids.Add("ice_ring");
				if(ConfigsAEA.swordCraft == true){craftice.related_weapons_ids.Add("ice_sword");}
				if(ConfigsAEA.axeCraft == true){craftice.related_weapons_ids.Add("ice_axe");}
				if(ConfigsAEA.hammerCraft == false){craftice.related_weapons_ids.Remove("ice_hammer");}
				if(ConfigsAEA.spearCraft == true){craftice.related_weapons_ids.Add("ice_spear");}
				if(ConfigsAEA.bowCraft == true){craftice.related_weapons_ids.Add("ice_bow");}
			}
			else {
				if(ConfigsAEA.hammerCraft == false){craftice.related_weapons_ids.Remove("ice_hammer");}
			}
	//Items Lists
        //Weapons-------------------------------------------------------------------------------
        //Subtype, used for deciding how to craft items (craftItem)
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("customItem")){AssetManager.items.equipment_by_subtypes.Add("customItem", new List<EquipmentAsset>());} //used for special items
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("sword")){AssetManager.items.equipment_by_subtypes.Add("sword", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("axe")){AssetManager.items.equipment_by_subtypes.Add("axe", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("hammer")){AssetManager.items.equipment_by_subtypes.Add("hammer", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("spear")){AssetManager.items.equipment_by_subtypes.Add("spear", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("bow")){AssetManager.items.equipment_by_subtypes.Add("bow", new List<EquipmentAsset>());}
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flameaxe);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flamespear);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(flamebow);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icesword);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(iceaxe);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icespear);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icebow);
            if(ConfigsAEA.goldTierCraft == true){
                AssetManager.items.equipment_by_subtypes["sword"].Insert(5, goldsword);
                AssetManager.items.equipment_by_subtypes["axe"].Insert(5, goldaxe);
                AssetManager.items.equipment_by_subtypes["hammer"].Insert(5, goldhammer);
                AssetManager.items.equipment_by_subtypes["spear"].Insert(5, goldspear);
                AssetManager.items.equipment_by_subtypes["bow"].Insert(5, goldbow);
            }

        //Armor-------------------------------------------------------------------------------
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("customItem")){AssetManager.items.equipment_by_subtypes.Add("customItem", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("helmet")){AssetManager.items.equipment_by_subtypes.Add("helmet", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("armor")){AssetManager.items.equipment_by_subtypes.Add("armor", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("boots")){AssetManager.items.equipment_by_subtypes.Add("boots", new List<EquipmentAsset>());}
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scalehelmet);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scalearmor);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scaleboots);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icehelmet);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icearmor);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(iceboots);
            if(ConfigsAEA.goldTierCraft == true){
                AssetManager.items.equipment_by_subtypes["helmet"].Insert(4, goldhelmet);
                AssetManager.items.equipment_by_subtypes["armor"].Insert(4, goldarmor);
                AssetManager.items.equipment_by_subtypes["boots"].Insert(4, goldboots);
            }

        //Accessories-------------------------------------------------------------------------------
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("customItem")){AssetManager.items.equipment_by_subtypes.Add("customItem", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("amulet")){AssetManager.items.equipment_by_subtypes.Add("amulet", new List<EquipmentAsset>());}
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("ring")){AssetManager.items.equipment_by_subtypes.Add("ring", new List<EquipmentAsset>());}
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scaleamulet);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(iceamulet);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scalering);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(icering);
            if(ConfigsAEA.goldTierCraft == true){
                AssetManager.items.equipment_by_subtypes["amulet"].Insert(4, goldamulet);
                AssetManager.items.equipment_by_subtypes["ring"].Insert(4, goldring);
            }
        }
    }
}
