using System;
using System.Collections.Generic;
using HarmonyLib;
using NeoModLoader.api;
using UnityEngine;

namespace RenderFix {
    public sealed class Main : BasicMod<Main> {
        internal const string HarmonyId = "jbrow.worldbox.renderfix";

        protected override void OnModLoad() {
            Harmony harmony = new Harmony(HarmonyId);
            harmony.PatchAll();
            Debug.Log("[Render fix] Loaded stability patch for minimap, wild kingdom building null-guards, and sprite refresh recovery.");
        }
    }

    internal static class MinimapGuard {
        private static readonly HashSet<string> LoggedIssues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static Color32 Fallback(Building building, WorldTile tile, string reason, Exception exception = null) {
            string assetId = building?.asset?.id;
            if (string.IsNullOrWhiteSpace(assetId)) {
                assetId = "<unknown-building>";
            }

            string tileInfo = tile == null ? "<no tile>" : $"{tile.x},{tile.y}";
            string issueKey = assetId + "|" + reason;
            if (LoggedIssues.Add(issueKey)) {
                string exceptionDetails = exception == null ? string.Empty : $" | {exception.GetType().Name}: {exception.Message}";
                Debug.LogWarning($"[Render fix] Suppressed broken minimap icon for '{assetId}' at {tileInfo}: {reason}{exceptionDetails}");
            }

            return Toolbox.clear;
        }

        internal static void LogWildBuildingIssue(Building building, string reason, Exception exception = null) {
            string assetId = building?.asset?.id;
            if (string.IsNullOrWhiteSpace(assetId)) {
                assetId = "<unknown-building>";
            }

            WorldTile tile = building?.current_tile;
            string tileInfo = tile == null ? "<no tile>" : $"{tile.x},{tile.y}";
            string issueKey = "wild|" + assetId + "|" + reason;
            if (LoggedIssues.Add(issueKey)) {
                string exceptionDetails = exception == null ? string.Empty : $" | {exception.GetType().Name}: {exception.Message}";
                Debug.LogWarning($"[Render fix] Suppressed wild kingdom building crash for '{assetId}' at {tileInfo}: {reason}{exceptionDetails}");
            }
        }

        internal static void LogOnce(string issueKey, string message) {
            if (LoggedIssues.Add(issueKey)) {
                Debug.LogWarning(message);
            }
        }

        internal static void LogSpriteRecovery(string issueKey, string message, Exception exception = null) {
            if (LoggedIssues.Add(issueKey)) {
                string exceptionDetails = exception == null ? string.Empty : $" | {exception.GetType().Name}: {exception.Message}";
                Debug.LogWarning(message + exceptionDetails);
            }
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.getColorForMinimap))]
    internal static class BuildingGetColorForMinimapPatch {
        private static bool Prefix(Building __instance, WorldTile pTile, ref Color32 __result) {
            if (Config.EVERYTHING_MAGIC_COLOR) {
                __result = Toolbox.EVERYTHING_MAGIC_COLOR32;
                return false;
            }

            if (__instance == null) {
                __result = MinimapGuard.Fallback(null, pTile, "building instance was null");
                return false;
            }

            if (pTile == null) {
                __result = MinimapGuard.Fallback(__instance, null, "minimap tile was null");
                return false;
            }

            BuildingAsset asset = __instance.asset;
            if (asset == null) {
                __result = MinimapGuard.Fallback(__instance, pTile, "building asset was null");
                return false;
            }

            try {
                asset.checkSpritesAreLoaded();
            }
            catch (Exception ex) {
                __result = MinimapGuard.Fallback(__instance, pTile, "building sprites failed to load", ex);
                return false;
            }

            BuildingMapIcon mapIcon = asset.building_sprites?.map_icon;
            if (mapIcon == null) {
                __result = MinimapGuard.Fallback(__instance, pTile, "building map icon was missing");
                return false;
            }

            try {
                __result = mapIcon.getColor(pTile.minimap_building_x, pTile.minimap_building_y, __instance);
                return false;
            }
            catch (Exception ex) {
                __result = MinimapGuard.Fallback(__instance, pTile, "map icon color lookup crashed", ex);
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.checkSpriteToRender))]
    internal static class BuildingCheckSpriteToRenderPatch {
        private static void Postfix(Building __instance, ref Sprite __result) {
            if (__instance == null || __result != null) {
                return;
            }

            string assetId = __instance.asset?.id ?? "<unknown-building>";
            try {
                __instance.asset?.checkSpritesAreLoaded();
                __instance.clearSprites();

                Sprite mainSprite = __instance.calculateMainSprite();
                if (mainSprite != null) {
                    __result = __instance.calculateColoredSprite(mainSprite);
                }

                if (__result == null) {
                    MinimapGuard.LogSpriteRecovery(
                        "sprite|building|" + assetId,
                        $"[Render fix] Building sprite stayed null after refresh for '{assetId}'."
                    );
                }
            }
            catch (Exception ex) {
                MinimapGuard.LogSpriteRecovery(
                    "sprite|building-ex|" + assetId,
                    $"[Render fix] Building sprite refresh crashed for '{assetId}'.",
                    ex
                );
            }
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.getSpriteToRender))]
    internal static class ActorGetSpriteToRenderPatch {
        private static void Postfix(Actor __instance, ref Sprite __result) {
            if (__instance == null || __result != null) {
                return;
            }

            string texturePath = "<unknown-unit-texture>";
            try {
                texturePath = __instance.getUnitTexturePath() ?? texturePath;
                __instance.clearSprites();
                __instance.checkAnimationContainer();

                Sprite mainSprite = __instance.calculateMainSprite();
                if (mainSprite != null) {
                    __result = __instance.asset != null && __instance.asset.need_colored_sprite
                        ? __instance.calculateColoredSprite(mainSprite, true)
                        : mainSprite;
                }

                if (__result == null) {
                    MinimapGuard.LogSpriteRecovery(
                        "sprite|actor|" + texturePath,
                        $"[Render fix] Unit sprite stayed null after refresh for '{texturePath}'."
                    );
                }
            }
            catch (Exception ex) {
                MinimapGuard.LogSpriteRecovery(
                    "sprite|actor-ex|" + texturePath,
                    $"[Render fix] Unit sprite refresh crashed for '{texturePath}'.",
                    ex
                );
            }
        }
    }

    [HarmonyPatch(typeof(WildKingdomsManager), nameof(WildKingdomsManager.updateDirtyBuildings))]
    internal static class WildKingdomsManagerUpdateDirtyBuildingsPatch {
        private static bool Prefix(WildKingdomsManager __instance) {
            try {
                __instance.clearAllBuildingLists();
            }
            catch (Exception ex) {
                MinimapGuard.LogOnce(
                    "wild|clearAllBuildingLists",
                    $"[Render fix] Skipped wild kingdom building rebuild because clearAllBuildingLists crashed | {ex.GetType().Name}: {ex.Message}"
                );
                return false;
            }

            MapBox world = World.world;
            BuildingManager buildingManager = world?.buildings;
            if (buildingManager == null) {
                MinimapGuard.LogOnce(
                    "wild|buildingManager-null",
                    "[Render fix] Skipped wild kingdom building rebuild because World.world or World.world.buildings was null."
                );
                return false;
            }

            foreach (Building building in buildingManager) {
                if (building == null) {
                    MinimapGuard.LogOnce(
                        "wild|null-building-entry",
                        "[Render fix] Skipped a null building entry while rebuilding wild kingdom building lists."
                    );
                    continue;
                }

                bool isAlive;
                try {
                    isAlive = building.isAlive();
                }
                catch (Exception ex) {
                    MinimapGuard.LogWildBuildingIssue(building, "building.isAlive() crashed", ex);
                    continue;
                }

                if (!isAlive) {
                    continue;
                }

                Kingdom kingdom = building.kingdom;
                if (kingdom == null) {
                    MinimapGuard.LogWildBuildingIssue(building, "building kingdom was null");
                    continue;
                }

                try {
                    if (!kingdom.wild) {
                        continue;
                    }

                    kingdom.listBuilding(building);
                }
                catch (Exception ex) {
                    MinimapGuard.LogWildBuildingIssue(building, "wild kingdom building list update crashed", ex);
                }
            }

            return false;
        }
    }
}
