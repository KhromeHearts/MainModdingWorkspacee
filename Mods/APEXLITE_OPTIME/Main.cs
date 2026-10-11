using HarmonyLib;
using NeoModLoader.api;
using Optime.Developer;
using Optime.Optimizations;
using Optime.QoL;

namespace Optime {
    internal class Main : BasicMod<Main> {
        public static ModConfig ModConfig => Instance.GetConfig();

        protected override void OnModLoad() {
            Harmony.CreateAndPatchAll(GetType());

            LoadQoL();
            LoadOptimizations();
            LoadBugFixes();
            LoadDeveloperFeatures();
        }

        private void LoadBugFixes() { }

        private void LoadDeveloperFeatures() {
            gameObject.InitializeFeature<DeveloperMode>(GetConfig()["developer"]["developer.developerMode"].BoolVal);
        }

        private void LoadOptimizations() {
            gameObject.InitializeFeature<ActorJobFlatten>(GetConfig()["optimizations"]["optimizations.actorJobFlatten"]
                .BoolVal);
            gameObject.InitializeFeature<DrawMapLayerThrottle>(
                GetConfig()["optimizations"]["optimizations.drawMapLayerThrottle"].BoolVal);
            gameObject.InitializeFeature<FastMinimapRenderer>(
                GetConfig()["optimizations"]["optimizations.fastMinimapRenderer"].BoolVal);
            gameObject.InitializeFeature<LazyZoneCamera>(GetConfig()["optimizations"]["optimizations.lazyZoneCamera"]
                .BoolVal);
            gameObject.InitializeFeature<MouseCursorFix>(GetConfig()["optimizations"]["optimizations.mouseCursorFix"]
                .BoolVal);
            gameObject.InitializeFeature<PowerButtonSelectorFix>(
                GetConfig()["optimizations"]["optimizations.powerButtonSelectorFix"].BoolVal);
            gameObject.InitializeFeature<PowersTabUpdateThrottle>(
                GetConfig()["optimizations"]["optimizations.powersTabUpdateThrottle"].BoolVal);
            gameObject.InitializeFeature<QuantumSpriteThrottle>(
                GetConfig()["optimizations"]["optimizations.quantumSpriteThrottle"].BoolVal);
            gameObject.InitializeFeature<UiWorldAgeFix>(GetConfig()["optimizations"]["optimizations.uiWorldAgeFix"]
                .BoolVal);
        }

        private void LoadQoL() {
            gameObject.InitializeFeature<DisableAnalytics>(GetConfig()["qol"]["qol.disableAnalytics"].BoolVal);
            gameObject.InitializeFeature<QoL.FPS>();
            gameObject.InitializeFeature<FPSCounter>(GetConfig()["qol"]["qol.displayFPS"].BoolVal);
            gameObject.InitializeFeature<HideWelcomeWindow>(GetConfig()["qol"]["qol.hideWelcomeWindow"].BoolVal);
            gameObject.InitializeFeature<TimeStopProjectiles>(GetConfig()["qol"]["qol.timeStopProjectiles"].BoolVal);
        }
    }
}