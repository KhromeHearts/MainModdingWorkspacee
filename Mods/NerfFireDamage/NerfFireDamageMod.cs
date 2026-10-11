using HarmonyLib;
using NeoModLoader.api;

namespace NerfFireDamage;

public class NerfFireDamageMod : BasicMod<NerfFireDamageMod>
{
    protected override void OnModLoad()
    {
        var harmony = new Harmony("raytracerx.nerf_fire_damage");
        harmony.PatchAll();
        LogInfo("RTX is soooo cooooool~");
    }
}
