using UnityEngine;

namespace Optime.QoL {
    internal sealed class FPSCounter : Feature<FPSCounter> {
        private void OnGUI() {
            GUI.Label(new Rect(0, 0, 100, 50), $"FPS: {(global::FPS.fps)}");
        }

        public new static void SetLoaded(bool value) {
            Feature<FPSCounter>.SetLoaded(value);
        }
    }
}