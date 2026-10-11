// snippet_status_effect.cs — Apply a status effect (internal method access)
// Source pattern: XMM MagicPatches, game API

// IMPORTANT: addStatusEffect is technically `internal` but IS callable from
// NML mods because NML compiles source against Assembly-CSharp.dll directly.
// If compilation fails, use the reflection fallback below.

public static void ApplyStatusDirectly(Actor actor)
{
    // Direct call — works in most NML mod setups
    actor.addStatusEffect("burning", 5f, true);
    // Args: statusID, timer override (0 = use default), show color flash

    // Check if actor has a status
    if (actor.hasStatus("burning"))
    {
        // Remove it
        actor.finishStatusEffect("burning");
    }
}

// ---- Reflection fallback (if direct call won't compile) ----
using System.Reflection;
using HarmonyLib;

public static void ApplyStatusViaReflection(Actor actor)
{
    MethodInfo addStatus = AccessTools.Method(
        typeof(BaseSimObject),
        "addStatusEffect",
        new[] { typeof(string), typeof(float), typeof(bool) }
    );
    addStatus.Invoke(actor, new object[] { "burning", 5f, true });
}

// ---- Harmony Postfix to hook when ANY status is applied ----
[HarmonyPatch(typeof(BaseSimObject), "addStatusEffect")]
[HarmonyPatch(new[] { typeof(string), typeof(float), typeof(bool) })]
public static class Patch_OnStatusApplied
{
    static void Postfix(BaseSimObject __instance, string pID, bool __result)
    {
        if (__result && __instance.isActor())
        {
            Debug.Log($"Status '{pID}' applied to {((Actor)__instance).getName()}");
        }
    }
}

// ---- Using ActionLibrary helpers (no Harmony needed) ----
public static void ApplyStatusViaActionLibrary(Actor actor, WorldTile tile)
{
    // These are public static methods that internally call addStatusEffect
    ActionLibrary.giveCursed(tile, actor);
    ActionLibrary.giveEnchanted(tile, actor);
}
