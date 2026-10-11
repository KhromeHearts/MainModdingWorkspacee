using System;
using NeoModLoader.api;
using NeoModLoader.General.UI.Prefabs;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;
using UnityEngine;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>
    /// "Inventory" window opened from a unit's window: the same data model, shown and editable without
    /// possessing the unit. A real ScrollWindow (NML AbstractWindow), so it gets the game's frame, title,
    /// close button and back navigation.
    /// </summary>
    internal sealed class UnitInventoryWindow : AbstractWindow<UnitInventoryWindow>
    {
        public const string Id = "possesslib_unit_inventory";
        private const float ContentWidth = 226f;

        private RectTransform _content;
        private SlotGrid _grid;
        private EquipmentColumn _equipment;
        private SlotView _trash;
        private Text _header;
        private Actor _actor;
        private Inventory _inventory;
        private int _gridHotbar;
        private int _gridMain;

        /// <summary>The unit to show next time the window opens.</summary>
        public static Actor PendingActor;

        private static bool _showPending;
        private static float _showDeadline;

        public static void Open(Actor pActor)
        {
            if (pActor == null || !pActor.isAlive()) return;
            try
            {
                if (Instance == null) CreateAndInit(Id);
                PendingActor = pActor;
                // ScrollWindow.clickShow silently refuses while any window tween runs, and creating a
                // window starts one, so the show is retried until the tweens are done.
                _showPending = true;
                _showDeadline = Time.unscaledTime + 3f;
                TickPendingShow();
            }
            catch (Exception e)
            {
                PLLog.Exception("Opening the unit inventory window", e);
            }
        }

        /// <summary>Called every frame by the runtime; performs a queued Open once no window animation is running.</summary>
        internal static void TickPendingShow()
        {
            if (!_showPending) return;
            if (Time.unscaledTime > _showDeadline)
            {
                _showPending = false;
                PLLog.Warn("Could not open the unit inventory window: a window animation did not finish");
                return;
            }
            if (ScrollWindow.isAnimationActive()) return;
            _showPending = false;
            try
            {
                ScrollWindow.showWindow(Id);
            }
            catch (Exception e)
            {
                PLLog.Exception("Showing the unit inventory window", e);
            }
        }

        protected override void Init()
        {
            // Placed directly on the window background (under the title) instead of inside the scroll
            // view: the content fits, and the scroll view's layout pushed it to the bottom of the frame.
            Transform scroll = BackgroundTransform.Find("Scroll View");
            if (scroll != null) scroll.gameObject.SetActive(false);
            _content = UIFactory.Rect("PossessLibContent", BackgroundTransform);
            UIFactory.Place(_content, new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(ContentWidth, 200f));

            _header = UIFactory.Text("Header", _content, UIConst.FontNormal, TextAnchor.UpperLeft, UIConst.Gold);
            UIFactory.Place((RectTransform)_header.transform, new Vector2(0f, 1f), new Vector2(4f, -2f), new Vector2(ContentWidth - 8f, 10f));
        }

        private void BuildGrid(int pHotbar, int pMain)
        {
            if (_grid != null && _gridHotbar == pHotbar && _gridMain == pMain) return;
            if (_grid != null) Destroy(_grid.Root.gameObject);
            _gridHotbar = pHotbar;
            _gridMain = pMain;
            float slot = UIConst.WindowSlot;
            _grid = SlotGrid.Create(_content, "Grid", 0, pHotbar + pMain, pHotbar, slot, false, false, false);
            _grid.Root.anchoredPosition = new Vector2(4f, -14f);

            if (_equipment == null)
            {
                _equipment = EquipmentColumn.Create(_content, false, slot);
            }
            float equipY = -14f - _grid.Size.y - 8f;
            UIFactory.Place(_equipment.Root, new Vector2(0f, 1f), new Vector2(4f, equipY), _equipment.Size);

            if (_trash == null)
            {
                _trash = SlotView.Create(_content, "Trash", slot);
                _trash.Address.Kind = SlotKind.Trash;
                _trash.SetPlaceholder(UISprites.Trash);
            }
            UIFactory.Place((RectTransform)_trash.transform, new Vector2(1f, 1f), new Vector2(-4f, equipY), new Vector2(slot, slot));

            float height = -equipY + slot + 8f;
            _content.sizeDelta = new Vector2(ContentWidth, height);
        }

        public override void OnNormalEnable()
        {
            _actor = PendingActor ?? SelectedUnit.unit;
            PendingActor = null;
            if (_actor == null || !_actor.isAlive())
            {
                _header.text = Loc.T("possesslib_no_unit", "No unit selected");
                return;
            }
            _inventory = InventoryRegistry.GetOrCreate(_actor);
            if (_inventory == null) return;
            ActorSync.ValidateItems(_inventory, _actor);
            ActorSync.ReconcileResources(_inventory, _actor);
            BuildGrid(_inventory.HotbarSize, _inventory.MainSize);
            _grid.Bind(_inventory);
            _equipment.Bind(_inventory);
            _trash.Address.Inventory = _inventory;
            _trash.Refresh();
            // the window title comes from the locale key "possesslib_unit_inventory Title" (set by NML's WindowCreator)
            _header.text = _actor.getName() + (ReferenceEquals(_actor, PossessionController.Current) ? "  " + Loc.T("possesslib_possessed", "(possessed)") : string.Empty);
        }

        public override void OnNormalDisable()
        {
            PossessionController.ReturnCursor(_inventory, _actor);
            Tooltip.hideTooltip();
        }

        private void Update()
        {
            if (_inventory == null || _grid == null) return;
            if (_actor == null || !_actor.isAlive() || _inventory.Disposed)
            {
                _inventory = null;
                _header.text = Loc.T("possesslib_unit_gone", "This unit is gone.");
                return;
            }
            if (Time.frameCount % 15 == 0) ActorSync.ReconcileResources(_inventory, _actor);
            _grid.Tick();
            _equipment.Tick();
            _trash.Refresh();
        }
    }

    /// <summary>Adds the "Inventory" button to the game's unit window (called from the UnitWindow.OnEnable postfix).</summary>
    internal static class UnitWindowButton
    {
        private const string ButtonName = "PossessLib_InventoryButton";

        public static void Ensure(UnitWindow pWindow)
        {
            if (pWindow == null) return;
            try
            {
                Transform background = pWindow.transform.Find("Background") ?? pWindow.transform;
                if (background.Find(ButtonName) != null) return;
                SimpleButton button = SimpleButton.Instantiate(background, false, ButtonName);
                button.Setup(OnClick, UISprites.IconInventory ?? UISprites.IconCraft, null, new Vector2(24f, 24f), "tip",
                    new TooltipData
                    {
                        tip_name = Loc.T("possesslib_inventory", "Inventory"),
                        tip_description = Loc.T("possesslib_inventory_button_tip", "Open this unit's PossessLib inventory (no possession needed).")
                    });
                RectTransform rt = (RectTransform)button.transform;
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(2f, -40f);
                rt.localScale = Vector3.one;
            }
            catch (Exception e)
            {
                PLLog.Exception("Adding the inventory button to the unit window", e);
            }
        }

        private static void OnClick()
        {
            Actor actor = SelectedUnit.unit;
            if (actor == null) return;
            UnitInventoryWindow.Open(actor);
        }
    }
}
