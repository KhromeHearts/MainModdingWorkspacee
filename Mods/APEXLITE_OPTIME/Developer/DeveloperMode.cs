using Optime.Optimizations;
using Optime.QoL;
using UnityEngine;

namespace Optime.Developer {
    internal sealed class DeveloperMode : Feature<DeveloperMode> {
        private void OnGUI() {
            int startY = 0;

            if (Feature<FPSCounter>.Instance.enabled) {
                startY += 15;
            }

            if (Feature<FastMinimapRenderer>.Instance.enabled) {
                GUI.Label(new Rect(0, startY, 1000, 50),
                    $"Minimap Renderer - Tile Redraws: {FastMinimapRenderer.TileRedraws}");
                startY += 15;
            }
        }

        public new static void SetLoaded(bool value) {
            Feature<DeveloperMode>.SetLoaded(value);
        }

        protected override void OnLoad() {
            Config.isEditor = true;
            DebugConfig.setOption(DebugOption.DebugButton, true);
        }

        protected override void OnUnload() {
            Config.isEditor = false;
            StopAllCoroutines();
        }
    }
}