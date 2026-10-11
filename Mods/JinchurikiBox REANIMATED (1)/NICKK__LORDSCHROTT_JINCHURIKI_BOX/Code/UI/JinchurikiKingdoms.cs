// using System;
// using NCMS;
// using NCMS.Utils;
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
// internal static class JinchurikiKingdoms {
//     public static void init() {
//         KingdomAsset Konoha = new KingdomAsset();
//         Konoha.id = "konohagakure";
//         Konoha.mobs = true;
//         Konoha.addTag("konoha");
//         Konoha.addFriendlyTag("konoha");
//         Konoha.addFriendlyTag("civ");
//         Konoha.addFriendlyTag("human");
//         AssetManager.kingdoms.add(Konoha);
//         MapBox.instance.kingdoms.CallMethod("newHiddenKingdom", Konoha);

//         KingdomAsset human = AssetManager.kingdoms.get("human");
//         human.addFriendlyTag("konohagakure");
//     }
// }