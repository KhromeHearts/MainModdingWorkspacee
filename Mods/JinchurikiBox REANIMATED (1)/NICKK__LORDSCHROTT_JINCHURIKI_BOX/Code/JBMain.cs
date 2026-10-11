using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using NeoModLoader;
using NeoModLoader.General;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using ReflectionUtility;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JinchurikiBox.Content;
using JinchurikiBox.Actions;
using JinchurikiBox.Patches;
using JinchurikiBox.Core;

namespace JinchurikiBox;

public class JBMain : BasicMod<JBMain>, IReloadable, IUnloadable, ILocalizable {
    [Hotfixable]
    public void Reload() {

    }

    public void OnUnload() {
        
    }
    protected override void OnModLoad() {
        try {
            JBPatches.ClearReincarnates();
            var harmony = new Harmony("JinchurikiBox");
            Harmony.CreateAndPatchAll(typeof(JBPatches));
            Debug.Log("Mod made by Nickk and LordSchrott");
        } catch (Exception e) {
            Debug.LogError("[Jinchuriki-Box] Harmony patching failed: " + e.ToString());
        }
        JBPatches.EnsureLogAssets();
        ChakraSystem.init();
        JutsuLibrary.init();
        JinchurikiTraitGroup.init();
        JinchurikiTraits.init();
        JinchurikiStatusAssets.init();
        // JinchurikiKingdoms.init();
        // JinchurikiUnits.init();
        // JinchurikiGodPowers.init();
        // JinchurikiTab.init();
        // JinchurikiButtons.init();
    }
}
