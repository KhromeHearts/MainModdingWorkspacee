using System;
using HarmonyLib;
using NeoModLoader.api;
using UnityEngine;
using VisibleArmors.Appearance;
using VisibleArmors.Core;
using VisibleArmors.DebugTools;
using VisibleArmors.Patches;
using VisibleArmors.Render;

namespace VisibleArmors
{
    /// <summary>NML entry point: reads the config, loads the armor art, applies the Harmony patches and starts the runtime.</summary>
    public class VisibleArmorsMod : BasicMod<VisibleArmorsMod>
    {
        /// <summary>Harmony id (unique; no other mod uses it).</summary>
        public const string HarmonyId = "com.raytracerx.visiblearmors";

        internal static Harmony HarmonyInstance;
        internal static string FolderPath { get; private set; }

        /// <inheritdoc />
        protected override void OnModLoad()
        {
            FolderPath = GetDeclaration().FolderPath;
            VAConfig.ReadFrom(GetConfig());
            SpriteUnitPatch.MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            ArtLibrary.Load(FolderPath);
            AppearanceRules.Load(ArtLibrary.RootPath);
            ApplyPatches();

            GameObject runtime = new GameObject("VisibleArmors_Runtime");
            DontDestroyOnLoad(runtime);
            runtime.AddComponent<VisibleArmorsRuntime>();
            LogInfo("Visible Armors " + Api.VisibleArmorsApi.Version + " ready");
        }

        /// <summary>Each patch class is applied on its own, so one broken patch (after a game update) does not disable the rest.</summary>
        private static void ApplyPatches()
        {
            HarmonyInstance = new Harmony(HarmonyId);
            int ok = 0;
            int failed = 0;
            foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(VisibleArmorsMod).Assembly))
            {
                if (type.GetCustomAttributes(typeof(HarmonyPatch), true).Length == 0) continue;
                try
                {
                    HarmonyInstance.CreateClassProcessor(type).Patch();
                    ok++;
                }
                catch (Exception e)
                {
                    failed++;
                    LogError("Patch " + type.Name + " failed: " + e.Message);
                }
            }
            LogInfo("Harmony: " + ok + " patch classes applied, " + failed + " failed");
        }
    }

    /// <summary>Per-frame driver: uploads new composite sprites, reacts to config changes and runs the debug tools.</summary>
    internal sealed class VisibleArmorsRuntime : MonoBehaviour
    {
        private void Awake()
        {
            MapBox.on_world_loaded += OnWorldLoaded;
        }

        private void OnDestroy()
        {
            MapBox.on_world_loaded -= OnWorldLoaded;
        }

        private void OnWorldLoaded()
        {
            try
            {
                SpriteDump.OnWorldLoaded(this);
                SelfTest.OnWorldLoaded(this);
            }
            catch (Exception e)
            {
                VALog.Error("World loaded handler: " + e);
            }
        }

        private float _nextStats;

        private void Update()
        {
            if (!VAConfig.Debug || Time.realtimeSinceStartup < _nextStats) return;
            _nextStats = Time.realtimeSinceStartup + 10f;
            try
            {
                VALog.Info("[stats] visible=" + RenderSync.LastVisible + " armored=" + RenderSync.LastArmored
                           + " composites=" + Compositor.CachedCount + " pages=" + Compositor.Atlas.PageCount + "/" + VAConfig.CachePages
                           + " (~" + (Compositor.Atlas.ApproxBytes / 1024) + " KB) created=" + Compositor.Created + " evicted=" + Compositor.Evicted
                           + " scan avg=" + (RenderSync.ScanFrames > 0 ? (RenderSync.ScanMsTotal / RenderSync.ScanFrames).ToString("F3") : "-") + " ms"
                           + " compose avg=" + (Compositor.Created > 0 ? (Compositor.ComposeMsTotal / Compositor.Created).ToString("F3") : "-") + " ms"
                           + " looks=" + AppearanceRegistry.LookCount);
            }
            catch (Exception e)
            {
                VALog.GuardedException("Stats", e);
            }
        }

        private void LateUpdate()
        {
            try
            {
                // Composites built outside the unit render pass (unit window, statuses) are uploaded here.
                Compositor.Upload();
            }
            catch (Exception e)
            {
                VALog.GuardedException("Atlas upload", e);
            }
        }
    }
}
