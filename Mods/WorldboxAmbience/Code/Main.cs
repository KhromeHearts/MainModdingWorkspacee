using HarmonyLib;
using NeoModLoader.api;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace Ambience;

public class AmbienceMod : BasicMod<AmbienceMod>
{
    protected override void OnModLoad()
    {
        var harmony = new Harmony("com.worldbox.ambience");
        harmony.PatchAll();
        LogInfo("[Ambience] RTX is soooo cooooool~");

        GameObject ambienceObject = new GameObject("WorldboxAmbienceManager");
        ambienceObject.AddComponent<WorldboxAmbience>();
        UnityEngine.Object.DontDestroyOnLoad(ambienceObject);
    }
}
