// using System;
// using NCMS;
// using NeoModLoader.General;
// using JinchurikiBox.Core;
// using JinchurikiBox.UI;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using System.Reflection;
// using System.Threading;
// using System.Threading.Tasks;
// using System.IO;
// using System.Collections.Generic;
// using System.Text;
// using ai;
// using ai.behaviours;
// using HarmonyLib;
// using Newtonsoft.Json;
// using ReflectionUtility;
// using static Config;

// namespace JinchurikiBox.Content;
// internal static class JinchurikiGodPowers {
//     public static void init()
//     {
//         GodPower Naruto = new GodPower();
//         Naruto.id = "spawnNaruto";
//         Naruto.show_spawn_effect = true;
//         Naruto.multiple_spawn_tip = true;
//         Naruto.actor_spawn_height = 3f;
//         Naruto.name = "god_power_naruto";
//         Naruto.sound_drawing = "spawnelf";
//         Naruto.actor_asset_id = "naruto";
//         Naruto.click_action = new PowerActionWithID(AssetManager.powers.spawnUnit);
//         AssetManager.powers.add(Naruto);

//         GodPower chakraRain = new GodPower {
//             id = "chakraRain",
//             name = "god_power_chakraRain",
//             show_tool_sizes = true,
//             falling_chance = 0.03f,
//             hold_action = true,
//             unselect_when_window = true,
//             drop_id = "chakraRainDrop",
//             click_power_action = (pTile, pPower) => AssetManager.powers.spawnDrops(pTile, pPower),
//         };
//         AssetManager.powers.add(chakraRain);

//         DropAsset chakraRainDrop = new DropAsset() {
//             id = "chakraRainDrop",
//             path_texture = "drops/drop_rain",
//             animated = true,
//             animation_speed = 0.03f,
//             default_scale = 0.1f,
//             action_landed = actionChakraRain
//         };
//         AssetManager.drops.add(chakraRainDrop);
//     }

//     public static void actionChakraRain(WorldTile pTile = null, string pdrop_id = null) {
//         World.world.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
// 		for (int i = 0; i < World.world.temp_map_objects.Count; i++)
// 		{
// 			Actor actor = (Actor)World.world.temp_map_objects[i];
// 			if (actor.isAlive())
// 			{
// 				actor.finishStatusEffect("burning");
// 				ChakraSystem.addChakra(actor, actor.data.set("charka", actor.stats["chakra"] * 0.3f));
// 				actor.startShake();
// 				actor.startColorEffect();
// 			}
// 		}
//     }

//     public static bool callSpawnUnit(WorldTile pTile, string pPowerID)
//     {
//         AssetManager.powers.CallMethod("spawnUnit", pTile, pPowerID);
//         return true;
//     }

//     public static bool callSpawnDrops(WorldTile pTile, string pPowerID) {
//         AssetManager.powers.CallMethod("spawnDrops", pTile, pPowerID);
//         return true;
//     }

//     // private static PowersTab getPowersTab(string id)
//     // {
//     //     GameObject gameObject = GameObjects.FindEvenInactive("Tab_" + id);
//     //     return gameObject.GetComponent<PowersTab>();
//     // }
// }