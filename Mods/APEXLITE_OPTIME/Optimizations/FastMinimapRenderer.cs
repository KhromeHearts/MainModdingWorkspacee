using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class FastMinimapRenderer : Feature<FastMinimapRenderer> {
        private static bool[] _dirtyFlags;
        private static List<WorldTile> _dirtyList;

        public static int TileRedraws { get; private set; }

        public new static void SetLoaded(bool value) {
            Feature<FastMinimapRenderer>.SetLoaded(value);
        }

        [HarmonyPatch(typeof(WorldLayerEdges), nameof(WorldLayerEdges.clear))]
        [HarmonyPostfix]
        public static void WorldLayerEdges_clear_Postfix() {
            _dirtyFlags = new bool[MapBox.width * MapBox.height];
            _dirtyList.Clear();
        }

        [HarmonyPatch(typeof(MapBox), nameof(MapBox.createTiles))]
        [HarmonyPrefix]
        public static bool MapBox_createTiles_Prefix() {
            int worldSize = MapBox.width * MapBox.height;
            _dirtyFlags = worldSize > 0 ? new bool[worldSize] : null;

            return true;
        }

        [HarmonyPatch(typeof(WorldLayerEdges), nameof(WorldLayerEdges.redraw))]
        [HarmonyPrefix]
        public static bool WorldLayerEdges_redraw_Prefix(ref WorldLayerEdges __instance) {
            if (_dirtyFlags == null || _dirtyList == null) {
                return true;
            }

            foreach (WorldTile tile in _dirtyList) {
                WorldTile downTile = tile.tile_down;
                WorldTile upTile = tile.tile_up;

                __instance.pixels[tile.data.tile_id] = ComputeColor(tile.Type, downTile?.Type, upTile?.Type);
                _dirtyFlags[tile.data.tile_id] = false;
            }

            __instance.texture.SetPixels32(__instance.pixels);
            __instance.texture.Apply(false);

            TileRedraws = _dirtyList.Count;

            _dirtyList.Clear();
            __instance._dirty_chunks.Clear();

            return false;
        }

        [HarmonyPatch(typeof(MapBox), nameof(MapBox.setTileDirty))]
        [HarmonyPostfix]
        public static void MapBox_setTileDirty_Postfix(WorldTile pTile) {
            if (_dirtyFlags == null || _dirtyList == null) {
                return;
            }

            int tileId = pTile.data.tile_id;

            if (_dirtyFlags[tileId]) {
                return;
            }

            _dirtyFlags[tileId] = true;
            _dirtyList.Add(pTile);
        }

        protected override void OnLoad() {
            int worldSize = MapBox.width * MapBox.height;

            _dirtyFlags = worldSize > 0 ? new bool[worldSize] : null;
            _dirtyList = new List<WorldTile>();
        }

        protected override void OnUnload() {
            _dirtyList.Clear();
            _dirtyFlags = null;
            _dirtyList = null;
        }

        private static Color32 ComputeColor(TileTypeBase current, TileTypeBase down, TileTypeBase up) {
            bool isOcean = current.ocean;
            int heightMin = current.height_min;

            if (!current.check_edge || current.wall) {
                return Toolbox.clear;
            }

            if (down != null && !down.wall) {
                if (!down.ocean && !isOcean && current != down && down.height_min < heightMin) {
                    return Toolbox.edge_alpha;
                }
            }

            if (up != null) {
                if (isOcean && up.ocean && up.height_min > heightMin) {
                    return Toolbox.edge_alpha;
                }
            }

            if (up == null || !up.ground || up.can_be_filled_with_ocean) {
                return Toolbox.clear;
            }

            if (down != null && !down.wall) {
                if (current.edge_hills && !down.rocks) {
                    return TileTypeBase.edge_color_hills;
                }

                if (current.edge_mountains && down.edge_hills) {
                    return TileTypeBase.edge_color_mountain;
                }

                if (current.rocks && !down.rocks) {
                    return TileTypeBase.edge_color_mountain;
                }
            }

            if (current.layer_type == TileLayerType.Ocean) {
                return TileTypeBase.edge_color_ocean;
            }

            if (current.can_be_filled_with_ocean && !current.explodable) {
                return TileTypeBase.edge_color_no_ocean;
            }

            if (up.wall) {
                return up.edge_color;
            }

            return Toolbox.clear;
        }
    }
}