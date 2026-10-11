using System;
using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class LazyZoneCamera : Feature<LazyZoneCamera> {
        private static Vector3 _lastMoveCameraPosition = Vector3.positiveInfinity;
        private static float _lastOrthographicSize = -1f;

        public new static void SetLoaded(bool value) {
            Feature<LazyZoneCamera>.SetLoaded(value);
        }

        protected override void OnUnload() {
            _lastMoveCameraPosition = Vector3.positiveInfinity;
            _lastOrthographicSize = -1f;
        }

        [HarmonyPatch(typeof(ZoneCamera), nameof(ZoneCamera.update))]
        [HarmonyPrefix]
        public static bool ZoneCamera_update_Prefix() {
            if (Math.Abs(_lastOrthographicSize - MapBox.instance.camera.orthographicSize) < 0.01f
                && _lastMoveCameraPosition == MoveCamera.instance.transform.position) {
                return false;
            }

            _lastOrthographicSize = MapBox.instance.camera.orthographicSize;
            _lastMoveCameraPosition = MoveCamera.instance.transform.position;

            return true;
        }
    }
}