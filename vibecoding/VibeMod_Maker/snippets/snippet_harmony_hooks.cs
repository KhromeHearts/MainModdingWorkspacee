// snippet_harmony_hooks.cs — Hook into actor behaviour with Harmony
// Source pattern: XMM MagicPatches, RULERBOX Patches

using System;
using HarmonyLib;
using UnityEngine;

// ---- INITIALIZE HARMONY (in your mod's OnLoad/Awake) ----
// var harmony = new Harmony("com.mymod.id");
// harmony.PatchAll();  // auto-discovers all [HarmonyPatch] classes

// ---- Hook: When an actor is spawned ----
[HarmonyPatch(typeof(ActorManager), "finalizeActor")]
[HarmonyPatch(new Type[] { typeof(string), typeof(Actor), typeof(WorldTile), typeof(float) })]
public static class Patch_OnActorSpawned
{
    static void Postfix(Actor pActor)
    {
        // Runs AFTER every actor is finalized (spawned into world)
        if (pActor != null && pActor.asset.id == "human")
        {
            pActor.addTrait("strong");
        }
    }
}

// ---- Hook: When an actor dies ----
[HarmonyPatch(typeof(Actor), "die")]
[HarmonyPatch(new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) })]
public static class Patch_OnActorDeath
{
    static void Postfix(Actor __instance)
    {
        Debug.Log($"{__instance.getName()} has died!");
        // Spawn something at death location, trigger event, etc.
    }
}

// ---- Hook: Modify actor stats dynamically ----
[HarmonyPatch(typeof(Actor), "updateStats")]
public static class Patch_ModifyStats
{
    static void Postfix(Actor __instance)
    {
        if (__instance.hasTrait("berserker"))
        {
            __instance.stats["damage"] *= 2f;   // double damage
            __instance.stats["armor"] -= 5f;     // reduce armor
        }
    }
}

// ---- Hook: When a baby is born ----
[HarmonyPatch(typeof(BabyMaker), "makeBaby")]
public static class Patch_OnBabyBorn
{
    static void Postfix(Actor __result, Actor pParent1, Actor pParent2)
    {
        if (__result == null) return;
        // Inherit custom trait from parents
        if (pParent1?.hasTrait("fire_blood") == true || pParent2?.hasTrait("fire_blood") == true)
        {
            __result.addTrait("fire_blood");
        }
    }
}

// ---- Hook: Intercept war declaration (Prefix can block) ----
[HarmonyPatch(typeof(WarManager), nameof(WarManager.newWar))]
public static class Patch_BlockWar
{
    static bool Prefix(Kingdom pAttacker, Kingdom pDefender, ref War __result)
    {
        // Block wars between kingdoms with same culture
        if (pAttacker.culture == pDefender.culture)
        {
            __result = null;
            return false;  // skip original method — war is blocked
        }
        return true;  // allow original to run
    }
}

// ---- Hook: Save/Load custom data ----
[HarmonyPatch(typeof(MapBox), "saveSave")]
public static class Patch_OnSave
{
    static void Prefix()
    {
        // Sync your custom data to kingdom.data BEFORE save writes to disk
        foreach (var k in World.world.kingdoms.list)
        {
            k.data.set("my_mod_treasury", MyMod.GetTreasury(k));
        }
    }
}

[HarmonyPatch(typeof(MapBox), "loadSave")]
public static class Patch_OnLoad
{
    static void Postfix()
    {
        // Restore custom data AFTER save is loaded
        foreach (var k in World.world.kingdoms.list)
        {
            k.data.get("my_mod_treasury", out int treasury);
            MyMod.SetTreasury(k, treasury);
        }
    }
}
