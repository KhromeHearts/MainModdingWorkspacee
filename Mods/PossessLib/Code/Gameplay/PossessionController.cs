using System;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.Persistence;
using PossessLib.Sync;
using PossessLib.UI;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>
    /// Possession lifecycle, detected by polling ControllableUnit once per frame (no patch needed).
    /// Polling catches every way possession starts or ends: the power, the G key, ControllableUnit.remove
    /// when the main unit dies, clear() when the world is cleared, and other mods calling the API.
    /// </summary>
    internal static class PossessionController
    {
        public static Actor Current { get; private set; }
        public static Inventory CurrentInventory { get; private set; }
        public static bool InventoryOpen { get; private set; }

        private static double _lastAttackStamp;
        private static int _lastLevel;
        private static int _lastSelected = -1;
        private static int _lastVersion = -1;
        private static float _nextReconcile;
        private static float _nextValidate;
        private static float _nextPickup;
        private static float _zoomBefore = -1f;
        private static int _zoomWorldSeed;

        public static void Tick()
        {
            Actor now = ControllableUnit.getControllableUnit();
            if (now != null && (!now.isAlive() || now.data == null)) now = null;

            if (!ReferenceEquals(now, Current))
            {
                if (Current != null) End(now != null);
                if (now != null) Begin(now);
            }
            if (Current == null) return;

            Inventory inv = CurrentInventory;
            if (inv == null || inv.Disposed)
            {
                inv = CurrentInventory = InventoryRegistry.GetOrCreate(Current);
                if (inv == null) return;
            }

            Actor actor = Current;
            inv.UpdateLastKnownPosition(actor);
            float time = Time.unscaledTime;

            if (time >= _nextValidate)
            {
                _nextValidate = time + 0.5f;
                ActorSync.ValidateItems(inv, actor);
            }
            if (time >= _nextReconcile)
            {
                _nextReconcile = time + 0.2f;
                ActorSync.ReconcileResources(inv, actor);
            }

            ActorSync.SyncPossessedWeapon(inv, actor);
            if (inv.Selected != _lastSelected || inv.Version != _lastVersion)
            {
                _lastSelected = inv.Selected;
                _lastVersion = inv.Version;
                HandItem.Refresh();
            }

            // A new possession swing (the game stamps it in StatusLibrary.checkPossessedAttackLeft/Right).
            double stamp = actor._possession_attack_happened_frame;
            if (stamp != _lastAttackStamp)
            {
                _lastAttackStamp = stamp;
                if (ControllableUnit.isAttackPressedLeft()) Gathering.OnPossessedSwing(actor, inv);
            }

            int level = actor.level;
            if (level > _lastLevel && _lastLevel > 0)
            {
                Hud.ShowLevelUp(level);
                PossessLibEvents.RaisePossessedLevelUp(actor, level);
            }
            _lastLevel = level;

            if (time >= _nextPickup)
            {
                _nextPickup = time + 0.1f;
                GroundItems.TickPickup(actor, inv);
            }
            PendingDrops.Flush();
        }

        private static void Begin(Actor pActor)
        {
            Current = pActor;
            InventoryRegistry.EnsureWorld();
            CurrentInventory = InventoryRegistry.GetOrCreate(pActor);
            _lastAttackStamp = pActor._possession_attack_happened_frame;
            _lastLevel = pActor.level;
            _lastSelected = -1;
            _lastVersion = -1;
            InventoryOpen = false;

            if (CurrentInventory != null)
            {
                ActorSync.ValidateItems(CurrentInventory, pActor);
                ActorSync.ReconcileResources(CurrentInventory, pActor);
                ActorSync.ImportEquippedWeapon(CurrentInventory, pActor);
            }

            ApplyZoom();
            HotkeyTweaks.Apply();
            NativeHud.Apply();
            Hud.OnPossessionStarted(pActor, CurrentInventory);
            HandItem.Refresh();
            Compat.WarnOnPossession();
            PLLog.Debug("Possessed " + pActor.getName());
            PossessLibEvents.RaiseUnitPossessed(pActor);
        }

        private static void End(bool pSwitchingToAnother)
        {
            Actor old = Current;
            Inventory inv = CurrentInventory;
            if (InventoryOpen) SetInventoryOpen(false);
            ReturnCursor(inv, old);

            Current = null;
            CurrentInventory = null;
            HandItem.Clear();

            if (old != null && old.isAlive() && inv != null && !inv.Disposed)
            {
                ActorSync.EquipBestWeaponIfEmpty(inv, old);
            }

            if (!pSwitchingToAnother)
            {
                HotkeyTweaks.Restore();
                NativeHud.Restore();
                RestoreZoom();
                Hud.OnPossessionEnded();
            }
            PLLog.Debug("Released " + (old != null && old.data != null ? old.getName() : "unit"));
            PossessLibEvents.RaiseUnitReleased(old);
        }

        /// <summary>Puts the cursor stack back into its inventory, or on the ground if it does not fit.</summary>
        public static void ReturnCursor(Inventory pPreferred, Actor pActor)
        {
            ItemStack stack = CursorState.Stack;
            if (stack == null) return;
            Inventory origin = CursorState.Origin ?? pPreferred;
            CursorState.Clear();
            if (origin == null || origin.Disposed) return;
            int left = origin.Insert(stack);
            if (left <= 0) return;
            if (stack.Kind == StackKind.Resource) return; // stays in the bag as overflow
            Actor owner = origin.Owner ?? pActor;
            Vector2 pos = owner != null && owner.isAlive() ? owner.current_position : origin.LastKnownPosition;
            InventoryOps.Drop(stack, origin, pos);
        }

        public static void ToggleInventory() => SetInventoryOpen(!InventoryOpen);

        public static void SetInventoryOpen(bool pOpen)
        {
            if (Current == null) pOpen = false;
            if (InventoryOpen == pOpen) return;
            InventoryOpen = pOpen;
            if (!pOpen) ReturnCursor(CurrentInventory, Current);
            Hud.SetInventoryOpen(pOpen);
            UISound.Play(pOpen ? UISound.Open : UISound.Close);
            if (pOpen) PossessLibEvents.RaiseInventoryOpened(Current, CurrentInventory);
            else PossessLibEvents.RaiseInventoryClosed(Current, CurrentInventory);
        }

        // ------------------------------------------------------------------ camera

        private static void ApplyZoom()
        {
            if (MoveCamera.instance == null) return;
            if (_zoomBefore < 0f)
            {
                _zoomBefore = MoveCamera.instance.getTargetZoom();
                _zoomWorldSeed = MapBox.current_world_seed_id;
            }
            if (MoveCamera.instance.getTargetZoom() > PLConfig.PossessZoom) CameraZoom.SetZoom(PLConfig.PossessZoom, false);
        }

        private static void RestoreZoom()
        {
            float before = _zoomBefore;
            _zoomBefore = -1f;
            if (!PLConfig.RestoreZoom || before <= 0f || MoveCamera.instance == null) return;
            if (_zoomWorldSeed != MapBox.current_world_seed_id) return;
            CameraZoom.SetZoom(before, false);
        }

        /// <summary>The zoom from before possession started (or -1).</summary>
        public static float ZoomBeforePossession => _zoomBefore;
    }

    /// <summary>Camera helpers behind PossessLibApi.Camera. Thin wrappers over MoveCamera, clamped to its limits.</summary>
    internal static class CameraZoom
    {
        public const float MinZoom = 10f; // MoveCamera.ORTHOGRAPHIC_SIZE_MIN

        public static float MaxZoom => MoveCamera.instance != null ? MoveCamera.instance.orthographic_size_max : 200f;

        public static void SetZoom(float pZoom, bool pInstant)
        {
            if (MoveCamera.instance == null) return;
            float zoom = Mathf.Clamp(pZoom, MinZoom, MaxZoom);
            if (pInstant) MoveCamera.instance.forceZoom(zoom);
            else MoveCamera.instance.setTargetZoom(zoom);
        }

        public static float GetZoom() => MoveCamera.instance != null ? MoveCamera.instance.getTargetZoom() : 0f;
    }

    /// <summary>The old mod's hotkey fixes, applied only while possessing and restored afterwards.</summary>
    internal static class HotkeyTweaks
    {
        private static bool _applied;
        private static bool _up, _down, _left, _right, _zoom;

        public static void Apply()
        {
            if (_applied) return;
            try
            {
                _up = HotkeyLibrary.up.ignore_mod_keys;
                _down = HotkeyLibrary.down.ignore_mod_keys;
                _left = HotkeyLibrary.left.ignore_mod_keys;
                _right = HotkeyLibrary.right.ignore_mod_keys;
                // Movement keeps working while Shift (dash) or Ctrl (backstep) is held.
                HotkeyLibrary.up.ignore_mod_keys = true;
                HotkeyLibrary.down.ignore_mod_keys = true;
                HotkeyLibrary.left.ignore_mod_keys = true;
                HotkeyLibrary.right.ignore_mod_keys = true;
                // The wheel selects hotbar slots; PlayerInput zooms on Ctrl+wheel instead.
                _zoom = HotkeyLibrary.zoom.allow_unit_control;
                if (PLConfig.ScrollSelectsHotbar) HotkeyLibrary.zoom.allow_unit_control = false;
                _applied = true;
            }
            catch (Exception e)
            {
                PLLog.Exception("Hotkey tweaks", e);
            }
        }

        public static void Restore()
        {
            if (!_applied) return;
            _applied = false;
            try
            {
                HotkeyLibrary.up.ignore_mod_keys = _up;
                HotkeyLibrary.down.ignore_mod_keys = _down;
                HotkeyLibrary.left.ignore_mod_keys = _left;
                HotkeyLibrary.right.ignore_mod_keys = _right;
                HotkeyLibrary.zoom.allow_unit_control = _zoom;
            }
            catch (Exception e)
            {
                PLLog.Exception("Hotkey restore", e);
            }
        }
    }

    /// <summary>Hides the native key-tips panel and the native health bar while PossessLib's HUD is up.</summary>
    internal static class NativeHud
    {
        private static GameObject _tips;
        private static GameObject _health;
        private static bool _tipsHidden;
        private static bool _healthHidden;

        public static void Apply()
        {
            try
            {
                if (PossessionUI.instance == null) return;
                if (_tips == null)
                {
                    Transform t = PossessionUI.instance.transform.FindRecursive("Right");
                    if (t != null) _tips = t.gameObject;
                }
                if (_health == null)
                {
                    PossessionUnitInfo info = PossessionUI.instance.GetComponentInChildren<PossessionUnitInfo>(true);
                    if (info != null && info._bar_health != null) _health = info._bar_health.gameObject;
                }
                if (PLConfig.HideNativeTips && _tips != null && _tips.activeSelf)
                {
                    _tips.SetActive(false);
                    _tipsHidden = true;
                }
                if (_health != null && _health.activeSelf)
                {
                    _health.SetActive(false);
                    _healthHidden = true;
                }
            }
            catch (Exception e)
            {
                PLLog.Exception("Hiding native possession HUD", e);
            }
        }

        public static void Restore()
        {
            if (_tipsHidden && _tips != null) _tips.SetActive(true);
            if (_healthHidden && _health != null) _health.SetActive(true);
            _tipsHidden = false;
            _healthHidden = false;
        }
    }
}
