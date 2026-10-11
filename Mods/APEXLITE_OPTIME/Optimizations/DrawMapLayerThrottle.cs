using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class DrawMapLayerThrottle : Feature<DrawMapLayerThrottle> {
        public new static void SetLoaded(bool value) {
            Feature<DrawMapLayerThrottle>.SetLoaded(value);
        }

        [HarmonyPatch(typeof(MapBox), nameof(MapBox.updateMapLayers))]
        [HarmonyPrefix]
        public static bool MapBox_updateSystems_Prefix(ref MapBox __instance, float pElapsed) {
            Bench.bench("heat", "game_total");
            __instance.heat.update(pElapsed);
            Bench.benchEnd("heat", "game_total");
            Bench.bench("map_chunk_manager", "game_total");
            __instance.map_chunk_manager.update(pElapsed);
            Bench.benchEnd("map_chunk_manager", "game_total");
            Bench.bench("map_layers", "game_total");

            foreach (MapLayer mapLayer in __instance._map_layers) {
                mapLayer.update(pElapsed);
            }

            Bench.benchEnd("map_layers", "game_total");
            Bench.bench("map_layers_draw", "game_total");

            if ((Time.frameCount + 1) % 3 == 0) {
                foreach (MapLayer mapLayer in __instance._map_layers) {
                    mapLayer.draw(pElapsed);
                }
            }

            Bench.benchEnd("map_layers_draw", "game_total");
            Bench.bench("map_modules", "game_total");

            foreach (BaseModule mapLayer in __instance._map_modules) {
                mapLayer.update(pElapsed);
            }

            Bench.benchEnd("map_modules", "game_total");

            return false;
        }
    }
}