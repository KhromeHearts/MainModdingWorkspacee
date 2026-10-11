using HarmonyLib;
using PossessLib.Persistence;

namespace PossessLib.Patches
{
    /// <summary>
    /// Patch 4 (prefix, never skips): SavedMap.create gathers everything for a save. PossessLib writes
    /// its inventories and ground items into the game's custom-data containers just before that, so
    /// they are saved inside the world file.
    /// </summary>
    [HarmonyPatch(typeof(SavedMap), nameof(SavedMap.create))]
    internal static class SavePatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix()
        {
            WorldPersistence.BeforeSave();
        }
    }

    /// <summary>
    /// Patch 5 (postfix): ActorManager.loadObject calls Actor.loadFromSave right after it reloads the
    /// unit's equipment. Items are already loaded at that point, so this is the moment to rebuild the
    /// unit's PossessLib inventory and re-protect its items.
    /// </summary>
    [HarmonyPatch(typeof(Actor), nameof(Actor.loadFromSave))]
    internal static class LoadPatch
    {
        private static void Postfix(Actor __instance)
        {
            WorldPersistence.AfterActorLoaded(__instance);
        }
    }
}
