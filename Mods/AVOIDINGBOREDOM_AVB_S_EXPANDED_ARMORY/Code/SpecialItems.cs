using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NeoModLoader.api;

namespace avbsexpandedarmory{
    class SpecialItems : MonoBehaviour{
        public static void init(){
            int targetIndex = 0; //used for category/item positioning
    //Modifiers Added---------------------------------------------------------------------------------------------------------
            ItemModAsset measurement = AssetManager.items_modifiers.clone("measurement", "ice");
            measurement.mod_type = "measurement";
            measurement.translation_key = "mod_measurement";
            measurement.pool = "weapon";
            measurement.rarity = 0;
            measurement.mod_rank = 1;
            measurement.action_attack_target = new AttackAction(SpecialItems.addMeasurementEffect);

            ItemModAsset reaperscurse = new ItemModAsset();
            reaperscurse.id = "reapers_curse";
            reaperscurse.mod_type = "reapers_curse";
            reaperscurse.translation_key = "mod_reapers_curse";
            reaperscurse.pool = "weapon";
            reaperscurse.rarity = 0;
            reaperscurse.mod_rank = 0;
            reaperscurse.action_attack_target = new AttackAction(SpecialItems.addReapersEffect);
            AssetManager.items_modifiers.add(reaperscurse);
            
            ItemModAsset silence = new ItemModAsset();
            silence.id = "silence";
            silence.mod_type = "silence";
            silence.translation_key = "mod_silence";
            silence.pool = "weapon";
            silence.rarity = 0;
            silence.mod_rank = 1;
            silence.action_attack_target = new AttackAction(SpecialItems.addSilenceEffect);
            AssetManager.items_modifiers.add(silence);
	//Items Added---------------------------------------------------------------------------------------------------------
        //Angle Ruler
            EquipmentAsset angleruler = AssetManager.items.clone("angle_ruler", "$melee");
            angleruler.needs_to_be_explored = false;
            angleruler.is_pool_weapon = true;
            angleruler.metallic = true;
            angleruler.group_id = "staff";
            angleruler.translation_key = "angle_ruler_locale";
            angleruler.path_icon = "ui/icons/items/icon_angle_ruler";
            angleruler.path_gameplay_sprite = "weapons/angle_ruler";
            angleruler.path_slash_animation = "effects/slashes/slash_sword";
            angleruler.setCost(50, "common_metals", 10, "gems", 5);
            angleruler.name_templates = AssetLibrary<ItemAsset>.l<string>("white_staff_name");
            angleruler.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"measurement"});
            angleruler.base_stats["damage"] = 12f;
            angleruler.base_stats["multiplier_attack_speed"] = 0.5f;
			angleruler.base_stats["critical_chance"] = 0.1f;
			angleruler.base_stats["damage_range"] = 1f;
            angleruler.base_stats["mana"] = 30f;
            angleruler.base_stats["stamina"] = 30f;
            angleruler.rigidity_rating = 8;
            angleruler.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(angleruler);
            AssetManager.items.pot_weapon_assets_unlocked.Add(angleruler);
        //Scythe
            EquipmentAsset scythe = AssetManager.items.clone("scythe", "$melee");
            scythe.needs_to_be_explored = false;
            scythe.is_pool_weapon = true;
            scythe.metallic = true;
            scythe.group_id = "staff";
            scythe.translation_key = "scythe_locale";
            scythe.path_icon = "ui/icons/items/icon_scythe";
            scythe.path_gameplay_sprite = "weapons/scythe";
            scythe.path_slash_animation = "effects/slashes/slash_sword";
            scythe.setCost(500, "mythril", 25, "bones", 25);
            scythe.name_templates = AssetLibrary<ItemAsset>.l<string>("necromancer_staff_name");
            scythe.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>(new string[]{"reapers_curse"});
            scythe.base_stats["damage"] = 140f;
            scythe.base_stats["multiplier_attack_speed"] = 0.75f;
			scythe.base_stats["damage_range"] = 0.85f;
            scythe.base_stats["range"] = 2f;
            scythe.base_stats["targets"] = 3f;
            scythe.base_stats["mana"] = 50f;
            scythe.base_stats["stamina"] = 50f;
            scythe.rigidity_rating = 10;
            scythe.equipment_value = 500;
            AssetManager.items.pot_weapon_assets_all.Add(scythe);
            AssetManager.items.pot_weapon_assets_unlocked.Add(scythe);
        //Trispike
            ProjectileAsset spike = AssetManager.projectiles.clone("spike", "arrow");
            spike.texture = "spike";
            spike.texture_shadow = "shadows/projectiles/spike_shadow";
            EquipmentAsset trispike = AssetManager.items.clone("trispike", "$weapon");
            trispike.needs_to_be_explored = false;
            trispike.is_pool_weapon = true;
            trispike.metallic = true;
            trispike.attack_type = WeaponType.Range; //actually hybrid, see patch for tryToAttack
            trispike.group_id = "staff";
            trispike.projectile = "spike";
            trispike.translation_key = "trispike_locale";
            trispike.path_icon = "ui/icons/items/icon_trispike";
            trispike.path_gameplay_sprite = "weapons/trispike";
            trispike.path_slash_animation = "effects/slashes/slash_spear";
            trispike.setCost(30, "common_metals", 9, "stone", 3);
            trispike.name_templates = new List<string>();
            trispike.name_templates.AddTimes(30, "spear_name");
            trispike.name_templates.Add("weapon_name_city");
		    trispike.name_templates.Add("weapon_name_kingdom");
		    trispike.name_templates.Add("weapon_name_culture");
		    trispike.name_templates.Add("weapon_name_enemy_king");
		    trispike.name_templates.Add("weapon_name_enemy_kingdom");
            trispike.base_stats["damage"] = 18f;
            trispike.base_stats["multiplier_attack_speed"] = -0.3f;
            trispike.base_stats["accuracy"] = -0.1f;
            trispike.base_stats["range"] = 9f;
            trispike.base_stats["projectiles"] = 3f;
            trispike.base_stats["damage_range"] = 0.65f;
            trispike.base_stats["stamina"] = 15f;
            trispike.rigidity_rating = 8;
            trispike.equipment_value = 400;
            AssetManager.items.pot_weapon_assets_all.Add(trispike);
            AssetManager.items.pot_weapon_assets_unlocked.Add(trispike);
	//Culture Trait Implements---------------------------------------------------------------------------------------------------------
            ActorAsset angle = AssetManager.actor_library.get("angle");
            CultureTrait craftangle = AssetManager.culture_traits.clone("Angle Items", "craft_ice_weapon");
            craftangle.needs_to_be_explored = false;
            craftangle.related_weapons_ids.Clear();
            craftangle.related_weapons_ids.Add("angle_ruler");
            craftangle.default_for_actor_assets.Clear();
            craftangle.default_for_actor_assets.Add(angle);
            craftangle.path_icon = "ui/icons/culture_trait_craft_angle_ruler";
            //craftangle.priority = 100;

            ActorAsset plaguedoctor = AssetManager.actor_library.get("plague_doctor");
            CultureTrait craftscythe = AssetManager.culture_traits.clone("Scythe Weapon", "craft_ice_weapon");
            craftscythe.needs_to_be_explored = false;
            craftscythe.related_weapons_ids.Clear();
            craftscythe.related_weapons_ids.Add("scythe");
            craftscythe.default_for_actor_assets.Clear();
            craftscythe.default_for_actor_assets.Add(plaguedoctor);
            craftscythe.path_icon = "ui/icons/culture_trait_craft_scythe";

            CultureTrait crafttrispike = AssetManager.culture_traits.clone("Trispike Weapon", "craft_ice_weapon");
            crafttrispike.needs_to_be_explored = false;
            crafttrispike.related_weapons_ids.Clear();
            crafttrispike.related_weapons_ids.Add("trispike");
            crafttrispike.default_for_actor_assets.Clear();
            crafttrispike.path_icon = "ui/icons/culture_trait_craft_trispike";
	//Items Lists---------------------------------------------------------------------------------------------------------
            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("customItem")){AssetManager.items.equipment_by_subtypes.Add("customItem", new List<EquipmentAsset>());} //used for special items
            AssetManager.items.equipment_by_subtypes["customItem"].Add(angleruler);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(scythe);
            AssetManager.items.equipment_by_subtypes["customItem"].Add(trispike);
        }

        public static bool addMeasurementEffect(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget.isAlive() && !pTarget.isBuilding() && Randy.randomChance(0.05f))
            {
                pTarget.a.getHit(90, false, AttackType.Weapon, null, false, true, true);
                if (pTarget.a.getHealth() == 0){
                    pSelf.a.increaseKills();
                }
                return true;
            }
            return false;
        }
        public static bool addReapersEffect(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if(pSelf.isAlive() && pSelf.a.getAgeRatio() > 1 && !pSelf.a.hasTrait("immortal") && Randy.randomChance(0.35f)){
                pSelf.a.getHitFullHealth(AttackType.Other);
                return true;
            }
            return false;
        }
        public static bool addSilenceEffect(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if(pTarget.isAlive() && !pTarget.isBuilding() && Randy.randomChance(0.4f)){
                pTarget.addStatusEffect("spell_silence", 15f, true);
                return true;
            }
            return false;
        }
    }
}
