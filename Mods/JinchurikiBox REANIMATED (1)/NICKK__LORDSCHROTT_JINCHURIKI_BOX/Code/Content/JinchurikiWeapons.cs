// using System;
// using NeoModLoader;
// using NeoModLoader.api.attributes;
// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using System.Reflection;
// using ReflectionUtility;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Text;
// using ai;
// using ai.behaviours;
// using System.IO;
// using System.Collections.Generic;
// using HarmonyLib;
// using Newtonsoft.Json;



// namespace JinchurikiBox;
// internal static class JinchurikiWeapons
// {
//     public static void init()
//     {
//         craftitems();

//         ItemAsset katana = AssetManager.items.clone("katana", "_melee");
//         katana.id = "katana";
//         katana.name_templates = Toolbox.splitStringIntoList(new string[] {
//             "sword_name#30",
//             "sword_name_king#3",
//             "weapon_name_city",
//             "weapon_name_kingdom",
//             "weapon_name_culture",
//             "weapon_name_enemy_king",
//             "weapon_name_enemy_kingdom"
//         });
//         katana.materials = List.Of<string>(new string[] {"wood", "copper", "bronze", "silver", "iron", "steel", "mythril", "adamantine"});
//         katana.base_stats["attack_speed"] = 45;
//         katana.base_stats["speed"] = 0.5f;
//         katana.base_stats["critical_chance"] = 0.15f;
//         katana.base_stats["damage_range"] = 0.7f;
//         katana.base_stats["damage"] = 1.0f;
//         katana.tech_needed = "weapon_sword";
//         katana.action_special_effect = new WorldAction(NoneRegularAction);
//         katana.action_attack_target = new AttackAction(KatanaAttackAction);
//         katana.name_class = "item_class_weapon";
//         katana.equipmentType = EquipmentType.Weapon;
//         katana.path_slash_animation = "effects/slashes/slash_sword";
//         AssetManager.items.list.AddItem(katana);
//         // Localization.addLocalization("item_katana", "katana");
//         addWeaponsSprite(katana.id, katana.materials[0]);
//         addWeaponsSprite(katana.id, katana.materials[1]);
//         addWeaponsSprite(katana.id, katana.materials[2]);
//         addWeaponsSprite(katana.id, katana.materials[3]);
//         addWeaponsSprite(katana.id, katana.materials[4]);
//         addWeaponsSprite(katana.id, katana.materials[5]);
//         addWeaponsSprite(katana.id, katana.materials[6]);
//         addWeaponsSprite(katana.id, katana.materials[7]);

//         // ItemAsset kunai = AssetManager.items.clone("katana", "_melee");
//         // kunai.id = "kunai";
//         // kunai.name_templates = Toolbox.splitStringIntoList(new string[] {
//         //     "sword_name#30",
//         //     "sword_name_king#3",
//         //     "weapon_name_city",
//         //     "weapon_name_kingdom",
//         //     "weapon_name_culture",
//         //     "weapon_name_enemy_king",
//         //     "weapon_name_enemy_kingdom"
//         // });
//         // kunai.materials = List.Of<string>(new string[] {"wood", "copper", "bronze", "silver", "iron", "steel", "mythril", "adamantine"});
//         // kunai.base_stats["attack_speed"] = 60;
//         // kunai.base_stats["speed"] = 0.5f;
//         // kunai.base_stats["critical_chance"] = 0.15f;
//         // kunai.base_stats["damage_range"] = 0.4f;
//         // kunai.tech_needed = "weapon_sword";
//         // kunai.action_special_effect = new WorldAction(NoneRegularAction);
//         // kunai.action_attack_target = new AttackAction(KatanaAttackAction);
//         // kunai.name_class = "item_class_weapon";
//         // kunai.equipmentType = EquipmentType.Weapon;
//         // kunai.path_slash_animation = "effects/slashes/slash_sword";
//         // AssetManager.items.list.AddItem(kunai);
//         // Localization.addLocalization("item_kunai", "kunai");
//     }

//     static void craftitems() {
//         Race human = AssetManager.raceLibrary.get("human");
//         Race orc = AssetManager.raceLibrary.get("orc");
//         Race dwarf = AssetManager.raceLibrary.get("dwarf");
//         Race elf = AssetManager.raceLibrary.get("elf");

//         human.preferred_weapons.Add("katana");
//         elf.preferred_weapons.Add("katana");
//         dwarf.preferred_weapons.Add("katana");
//         orc.preferred_weapons.Add("katana");
//     }

//     public static bool KatanaAttackAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null) {
//         pTarget.a.data.health -= (int)(pSelf.a.stats["chakra"] / 10);
//         if (pTarget.a.data.health <= 0) {
//             pTarget.a.killHimself(pDestroy: false);
//         }
//         return true;
//     }

//     public static bool NoneRegularAction(BaseSimObject pTarget, WorldTile pTile = null) {
//         return false;
//     }

//     static void addWeaponsSprite(string id, string material) {
//         var dictItems = Reflection.GetField(typeof(ActorAnimationLoader), null, "dictItems") as Dictionary<string, Sprite>;
//         var sprite = Resources.Load<Sprite>("weapons/w_" + id + "_" + material);
//         dictItems.Add(sprite.name, sprite);
//     }
// }