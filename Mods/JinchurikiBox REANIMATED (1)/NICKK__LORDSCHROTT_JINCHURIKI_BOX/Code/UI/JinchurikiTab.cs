// using System;
// using NeoModLoader.General;
// using NeoModLoader.General.UI.Tab;
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

// namespace JinchurikiBox.UI {
//     internal static class JinchurikiTab {
//         public static GameObject JBTabObject;
//         public static PowersTab JBTab;
//         public static int buttonNum;
//         public static void init()
//         {
//             JBTab = TabManager.CreateTab("jb_tab", "jinchurikiTab", "jinchurikiTab_description", SpriteTextureLoader.getSprite("ui/icons/Kurama!"));

//             JBTab.SetLayout(new List<string>() {
//                     "info",
//                     "display",
//                     "creature"
//                 });
            
//             JBTab.UpdateLayout();

//             // JBTab.AddPowerButton("info",
//             // PowerButtonCreator.CreateGodPowerButton("spawnNaruto",
//             //     SpriteTextureLoader.getSprite("ui/icons/iconAttack")));
            
//             CreatePowerButton("spawnNaruto", SpriteTextureLoader.getSprite("ui/icons/iconNaruto"), "info");
//             CreatePowerButton("chakraRain", SpriteTextureLoader.getSprite("ui/icons/Chakra2"), "info");
//         }

//         public static GameObject CreatePowerButton(string power, Sprite sprite, string layout) {
//             PowerButton button = PowerButtonCreator.CreateGodPowerButton(power, sprite, JBTab.gameObject.transform);
//             setButtonPosition(button);
//             // JBTab.AddPowerButton(layout, button);
//             return button.gameObject;
//         }

//         public static void setButtonPosition(PowerButton button) {
//             /*float x = 99 + (buttonNum != 0 ? (18 * (buttonNum % 2 == 0 ? buttonNum : buttonNum - 1)) : 0);
//             float y = (buttonNum % 2 == 0 ? 18 : -18);
//             button.transform.localPosition = new Vector3(x, y);
//             buttonNum++;*/
//             float x = 0;
//             float y = 0;
//             if (buttonNum % 2 == 0) {
//                 x = 108f + (buttonNum * 18f);
//             } else {
//                 x = 108f + ((buttonNum - 1) * 18f);
//             }

//             if (buttonNum % 2 == 0) {
//                 y = 18f;
//             } else {
//                 y = -18f;
//             }
//             button.transform.localPosition = new Vector3(x, y, 0);
//             buttonNum++;
//         }
//     }
// }