using System;
using HarmonyLib;
using NeoModLoader.api;
using PossessLib.Core;
using PossessLib.DebugTools;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Persistence;
using PossessLib.Sync;
using PossessLib.UI;
using UnityEngine;

namespace PossessLib
{
    /// <summary>NML entry point: loads config and data, applies the Harmony patches and starts the runtime.</summary>
    public class PossessLibMod : BasicMod<PossessLibMod>
    {
        /// <summary>Harmony id (unique; does not collide with MainCharacterMod's).</summary>
        public const string HarmonyId = "com.raytracerx.possesslib";

        internal static Harmony HarmonyInstance;
        internal static string FolderPath { get; private set; }

        /// <inheritdoc />
        protected override void OnModLoad()
        {
            FolderPath = GetDeclaration().FolderPath;
            PLConfig.ReadFrom(GetConfig());
            DataTables.LoadAll(FolderPath);
            ApplyPatches();
            SlotTooltips.Register();
            GroundItems.RegisterRenderer();

            GameObject runtime = new GameObject("PossessLib_Runtime");
            DontDestroyOnLoad(runtime);
            runtime.AddComponent<PossessLibRuntime>();
            LogInfo("PossessLib " + Api.PossessLibApi.Version + " ready (API v" + Api.PossessLibApi.ApiVersion + ")");
        }

        /// <summary>Each patch class is applied on its own, so one broken patch (after a game update) does not disable the rest.</summary>
        private static void ApplyPatches()
        {
            HarmonyInstance = new Harmony(HarmonyId);
            int ok = 0;
            int failed = 0;
            foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(PossessLibMod).Assembly))
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

    /// <summary>Per-frame driver. Everything is polled from here; see DESIGN.md section 1.</summary>
    internal sealed class PossessLibRuntime : MonoBehaviour
    {
        private float _nextErrorLog;
        private bool _configDirty;
        private int _hotbar = PLConfig.HotbarSlots;
        private int _rows = PLConfig.InventoryRows;
        private float _scale = PLConfig.UiScale;

        private void Awake()
        {
            MapBox.on_world_loaded += OnWorldLoaded;
            PLConfig.Changed += OnConfigChanged;
        }

        private void OnDestroy()
        {
            MapBox.on_world_loaded -= OnWorldLoaded;
            PLConfig.Changed -= OnConfigChanged;
        }

        private void OnWorldLoaded()
        {
            try
            {
                InventoryRegistry.EnsureWorld();
                GroundItems.OnWorldLoaded();
                SelfTest.OnWorldLoaded(this);
            }
            catch (Exception e)
            {
                PLLog.Exception("World loaded handler", e);
            }
        }

        private void OnConfigChanged()
        {
            _configDirty = true;
        }

        private void Update()
        {
            if (World.world == null) return;
            try
            {
                if (_configDirty) ApplyConfigChange();
                InventoryRegistry.EnsureWorld();
                PossessionController.Tick();
                PlayerInput.Tick();
                Hud.Tick();
                WorldTicker.Tick();
                UnitInventoryWindow.TickPendingShow();
            }
            catch (Exception e)
            {
                if (Time.unscaledTime >= _nextErrorLog)
                {
                    _nextErrorLog = Time.unscaledTime + 2f;
                    PLLog.Exception("Runtime update", e);
                }
            }
        }

        private void ApplyConfigChange()
        {
            _configDirty = false;
            bool sizeChanged = _hotbar != PLConfig.HotbarSlots || _rows != PLConfig.InventoryRows;
            bool scaleChanged = !Mathf.Approximately(_scale, PLConfig.UiScale);
            _hotbar = PLConfig.HotbarSlots;
            _rows = PLConfig.InventoryRows;
            _scale = PLConfig.UiScale;
            if (sizeChanged)
            {
                foreach (Inventory inv in InventoryRegistry.All)
                {
                    var leftovers = inv.Resize(PLConfig.HotbarSlots, PLConfig.MainSlots);
                    if (leftovers.Count > 0) PendingDrops.Add(inv, leftovers);
                }
            }
            if (sizeChanged || scaleChanged) Hud.Rebuild();
        }
    }
}
