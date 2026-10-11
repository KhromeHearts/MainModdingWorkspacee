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
// internal static class JinchurikiUnits {
//     public static void init() {
//         createNaruto();
//         NameGeneratorAsset naruto_uzumaki = new NameGeneratorAsset();
//         naruto_uzumaki.id = "naruto_uzumaki";
//         naruto_uzumaki.use_dictionary = true;
//         naruto_uzumaki.motto_parts.Add("first", "Naruto");
//         naruto_uzumaki.motto_parts.Add("space", " ");
//         naruto_uzumaki.motto_parts.Add("second", "Uzumaki");
//         naruto_uzumaki.templates.Add("first,space,second");
//         AssetManager.name_generator.add(naruto_uzumaki);
//     }

//     public static void createNaruto() {
//         var Naruto = AssetManager.actor_library.clone("naruto", "_mob");
//         Naruto.id = "naruto";
//         Naruto.animal = false;
//         Naruto.race = SK.human;
//         Naruto.kingdom = "konohagakure";
//         Naruto.job = "attacker";
//         Naruto.icon = "iconNaruto";
//         Naruto.base_stats["lifespan"] = 100;
//         Naruto.base_stats["damage"] = 20;
//         Naruto.base_stats["attack_speed"] = 120f;
//         Naruto.base_stats["health"] = 250;
//         Naruto.base_stats["speed"] = 65;
//         Naruto.base_stats["accuracy"] = 99f;
//         Naruto.base_stats["chakra"] = 20;
//         Naruto.nameLocale = "NarutoUzumaki";
//         Naruto.nameTemplate = "naruto_uzumaki";
//         Naruto.body_separate_part_head = false;
//         Naruto.shadow = true;
//         //Naruto.has_skin = false;
//         Naruto.disableJumpAnimation = true;
//         Naruto.canLevelUp = true;
//         Naruto.ignoredByInfinityCoin = true;
//         Naruto.canBeKilledByDivineLight = false;
//         Naruto.canBeKilledByLifeEraser = true;
//         Naruto.canBeMovedByPowers = true;
//         Naruto.canTurnIntoZombie = false;
//         Naruto.canTurnIntoMush = false;
//         Naruto.canTurnIntoTumorMonster = false;
//         Naruto.canAttackBuildings = true;
//         Naruto.hideFavoriteIcon = false;
//         Naruto.can_edit_traits = true;
//         Naruto.action_liquid = new WorldAction(ActionLibrary.swimToIsland);
//         Naruto.landCreature = true;
//         Naruto.take_items = false;
//         Naruto.use_items = true;
//         Naruto.diet_meat = false;
//         Naruto.has_soul = true;
//         Naruto.dieInLava = true;
//         Naruto.needFood = false;
//         Naruto.flying = false;
//         Naruto.action_liquid = new WorldAction(ActionLibrary.swimToIsland);
//         Naruto.animation_walk = "walk_0,walk_1,walk_2,walk_3";
//         Naruto.animation_swim = "walk_0,walk_1,walk_2,walk_3";
//         Naruto.texture_path = "naruto";
//         Naruto.color_hex = "#EE9933";
//         Naruto.fmod_spawn = "event:/SFX/UNITS/Human/HumanSpawn";
//         Naruto.fmod_attack = "event:/SFX/UNITS/Human/HumanAttack";
//         Naruto.fmod_idle = "event:/SFX/UNITS/Human/HumanIdle";
//         Naruto.fmod_death = "event:/SFX/UNITS/Human/HumanDeath";
//         Naruto.traits = new List<string> {
//             "nineTailedJinchuriki",
//             "shadowCloneJutsu",
//             "nineTailedPartner",
//             "chakraC5",
//             "toadSage",
//             "uzumakiClan",
//             "stupid",
//             "strong_minded",
//             "agile",
//             "honest"
//         };
//         AssetManager.actor_library.add(Naruto);
//         AssetManager.actor_library.CallMethod("loadShadow", Naruto);
//         // Localization.addLocalization(Naruto.nameLocale, Naruto.nameLocale);
//     }
// }