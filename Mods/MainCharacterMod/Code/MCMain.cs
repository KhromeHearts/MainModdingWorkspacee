using System;
using UnityEngine;
using NeoModLoader.api;
using HarmonyLib;

namespace MC
{
    public class MCMod : BasicMod<MCMod>
    {
        protected override void OnModLoad()
        {
            var harmony = new Harmony("com.worldbox.maincharacter");
            harmony.PatchAll();
            LogInfo("[Main Character Mod] RTX is soooo cooooool~");

            GameObject mcModObject = new GameObject("MainCharacterModManager");
            mcModObject.AddComponent<MCPossess>();
            mcModObject.AddComponent<ResourceMining>();
            mcModObject.AddComponent<MCCityStorage>();
            UnityEngine.Object.DontDestroyOnLoad(mcModObject);
        }
    }

}
