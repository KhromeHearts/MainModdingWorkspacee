using System;
using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;
using UnityEngine;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>Entry points the gameplay layer uses to drive the HUD.</summary>
    internal static class Hud
    {
        private static HudRoot _root;
        internal static readonly List<HudElementDefinition> CustomElements = new List<HudElementDefinition>();
        internal static readonly List<InventoryPanelDefinition> CustomPanels = new List<InventoryPanelDefinition>();
        internal static int CustomVersion;

        public static bool IsBuilt => _root != null;

        public static void OnPossessionStarted(Actor pActor, Inventory pInventory)
        {
            if (!EnsureBuilt()) return;
            _root.Bind(pActor, pInventory);
            _root.gameObject.SetActive(true);
            _root.SetInventoryOpen(false);
        }

        public static void OnPossessionEnded()
        {
            if (_root == null) return;
            _root.SetInventoryOpen(false);
            _root.Bind(null, null);
            _root.gameObject.SetActive(false);
            Tooltip.hideTooltip();
            UIState.HoverCount = 0;
            UIState.HoveredSlot = null;
        }

        public static void SetInventoryOpen(bool pOpen)
        {
            if (_root != null) _root.SetInventoryOpen(pOpen);
        }

        public static void ShowLevelUp(int pLevel)
        {
            if (_root != null) _root.LevelUp.Show(pLevel);
        }

        /// <summary>The HUD slot widget for an inventory index (hotbar or main grid), or null.</summary>
        internal static SlotView GetSlotView(int pIndex) => _root != null ? _root.GetSlotView(pIndex) : null;

        /// <summary>Layout diagnostics: where the HUD groups really are on screen (self-test / bug reports).</summary>
        internal static string DescribeLayout()
        {
            if (_root == null || CanvasMain.instance == null) return "no HUD";
            Canvas canvas = CanvasMain.instance.canvas_ui;
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransform canvasRect = (RectTransform)canvas.transform;
            RectTransform root = (RectTransform)_root.transform;
            return "screen=" + Screen.width + "x" + Screen.height
                   + " canvas mode=" + canvas.renderMode + " scale=" + canvas.scaleFactor + " rect=" + canvasRect.rect.size
                   + " root rect=" + root.rect.size + " rootScale=" + root.localScale + " rootActive=" + root.gameObject.activeInHierarchy
                   + " | " + _root.DescribeGroups(cam);
        }

        public static void Tick()
        {
            if (_root == null) return;
            if (!_root.gameObject.activeSelf) return;
            // The controller can swap the inventory object (e.g. it was reloaded); follow it.
            Inventory current = PossessionController.CurrentInventory;
            if (current != null && !ReferenceEquals(current, _root.BoundInventory))
            {
                _root.Bind(PossessionController.Current, current);
                _root.SetInventoryOpen(PossessionController.InventoryOpen);
            }
            _root.Tick();
        }

        /// <summary>Rebuilds the HUD (slot counts, scale or registered elements changed).</summary>
        public static void Rebuild()
        {
            if (_root == null) return;
            Actor actor = PossessionController.Current;
            Inventory inv = PossessionController.CurrentInventory;
            bool open = PossessionController.InventoryOpen;
            bool active = _root.gameObject.activeSelf;
            UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
            if (!EnsureBuilt()) return;
            _root.Bind(actor, inv);
            _root.gameObject.SetActive(active);
            _root.SetInventoryOpen(open);
        }

        private static bool EnsureBuilt()
        {
            if (_root != null) return true;
            if (CanvasMain.instance == null || CanvasMain.instance.canvas_ui == null) return false;
            try
            {
                CursorView.Ensure();
                RectTransform rt = UIFactory.Rect("PossessLib_HUD", CanvasMain.instance.canvas_ui.transform);
                UIFactory.Stretch(rt);
                _root = rt.gameObject.AddComponent<HudRoot>();
                _root.Build();
                _root.gameObject.SetActive(false);
                return true;
            }
            catch (Exception e)
            {
                PLLog.Exception("Building the HUD", e);
                return false;
            }
        }
    }

    /// <summary>
    /// The possession HUD. Four anchored groups (top-left, top-right, top-centre, bottom corners), each
    /// scaled by the "pl_ui_scale" config. All positions are offsets from those anchors computed from
    /// <see cref="UIConst"/>, so the layout holds at any resolution and UI-size setting.
    /// </summary>
    internal sealed class HudRoot : MonoBehaviour
    {
        private RectTransform _topLeft, _topRight, _topCenter, _bottomLeft, _bottomRight;

        // top-left
        private Text _itemName;
        private SlotGrid _hotbar;
        private SlotGrid _main;
        private Image _inventoryBg;
        private RectTransform _side;
        private Text _coins;
        private SlotView _trash;
        private Text _overflow;
        private StatusRow _status;
        private CraftingPanel _crafting;
        private readonly List<KeyValuePair<InventoryPanelDefinition, RectTransform>> _panels = new List<KeyValuePair<InventoryPanelDefinition, RectTransform>>();
        private readonly List<KeyValuePair<HudElementDefinition, RectTransform>> _elements = new List<KeyValuePair<HudElementDefinition, RectTransform>>();
        private RectTransform _customTopLeft;

        // top-right
        private HealthPanel _health;
        private EquipmentColumn _equipment;
        private RectTransform _customTopRight;

        internal LevelUpPopup LevelUp;
        private ToastView _toasts;

        private Actor _actor;
        private Inventory _inventory;
        private bool _open;
        private int _nameVersion = -1;
        private int _lastMoney = -1;
        private int _lastOverflow = -1;
        private int _customVersion;

        private float _hotbarY;
        private float _mainY;
        private float _gridWidth;
        private float _mainHeight;

        internal void Build()
        {
            float s = PLConfig.UiScale;
            _topLeft = Group("TopLeft", new Vector2(0f, 1f), new Vector2(UIConst.Margin, -UIConst.Margin), s);
            _topRight = Group("TopRight", new Vector2(1f, 1f), new Vector2(-UIConst.Margin, -UIConst.Margin), s);
            _topCenter = Group("TopCenter", new Vector2(0.5f, 1f), Vector2.zero, s);
            _bottomLeft = Group("BottomLeft", new Vector2(0f, 0f), new Vector2(UIConst.Margin, UIConst.Margin), s);
            _bottomRight = Group("BottomRight", new Vector2(1f, 0f), new Vector2(-UIConst.Margin, UIConst.Margin), s);

            int hotbar = Mathf.Clamp(PLConfig.HotbarSlots, 1, 10);
            int main = PLConfig.MainSlots;
            float slot = UIConst.HudSlot;
            float step = slot + UIConst.Gap;
            _gridWidth = hotbar * step - UIConst.Gap;
            _hotbarY = -UIConst.NameLabelHeight - 1f;
            _mainY = _hotbarY - step - 3f;
            int rows = Mathf.CeilToInt(main / (float)hotbar);
            _mainHeight = rows * step - UIConst.Gap;

            // inventory background (behind hotbar + grid + side column), only visible while open
            _inventoryBg = UIFactory.Panel("InventoryBg", _topLeft, UIConst.PanelColor);
            UIFactory.Place(_inventoryBg.rectTransform, new Vector2(0f, 1f), new Vector2(-UIConst.PanelPadding, _hotbarY + UIConst.PanelPadding),
                new Vector2(_gridWidth + 30f + UIConst.PanelPadding * 2f, -(_mainY - _mainHeight) + _hotbarY + UIConst.PanelPadding * 2f));

            _itemName = UIFactory.Text("ItemName", _topLeft, UIConst.FontLarge, TextAnchor.MiddleLeft, Color.white);
            UIFactory.Place((RectTransform)_itemName.transform, new Vector2(0f, 1f), Vector2.zero, new Vector2(_gridWidth, UIConst.NameLabelHeight));

            _hotbar = SlotGrid.Create(_topLeft, "Hotbar", 0, hotbar, hotbar, slot, true, true, true);
            _hotbar.Root.anchoredPosition = new Vector2(0f, _hotbarY);

            _main = SlotGrid.Create(_topLeft, "Main", hotbar, main, hotbar, slot, true, false, false);
            _main.Root.anchoredPosition = new Vector2(0f, _mainY);

            BuildSideColumn(slot);

            _status = StatusRow.Create(_topLeft);
            _crafting = CraftingPanel.Create(_topLeft);
            _customTopLeft = UIFactory.Rect("Custom", _topLeft);
            UIFactory.Place(_customTopLeft, new Vector2(0f, 1f), Vector2.zero, new Vector2(_gridWidth, 0f));

            // top-right: health + stats, custom elements, equipment column
            _health = HealthPanel.Create(_topRight);
            _customTopRight = UIFactory.Rect("Custom", _topRight);
            UIFactory.Place(_customTopRight, new Vector2(1f, 1f), new Vector2(0f, -HealthPanel.Height - 3f), new Vector2(UIConst.HealthWidth, 0f));
            _equipment = EquipmentColumn.Create(_topRight, true, slot);
            UIFactory.Place(_equipment.Root, new Vector2(1f, 1f), new Vector2(0f, -HealthPanel.Height - 6f), _equipment.Size);

            _toasts = ToastView.Create(_topCenter);
            LevelUp = LevelUpPopup.Create(_topCenter);

            BuildCustom();
            Relayout();
        }

        private RectTransform Group(string pName, Vector2 pAnchor, Vector2 pOffset, float pScale)
        {
            RectTransform g = UIFactory.Rect(pName, transform);
            UIFactory.Place(g, pAnchor, pOffset, Vector2.zero);
            g.localScale = new Vector3(pScale, pScale, 1f);
            return g;
        }

        private void BuildSideColumn(float pSlot)
        {
            _side = UIFactory.Rect("Side", _topLeft);
            UIFactory.Place(_side, new Vector2(0f, 1f), new Vector2(_gridWidth + 4f, _mainY), new Vector2(26f, _mainHeight));

            Image coinIcon = UIFactory.Image("CoinIcon", _side, UISprites.IconMoney, Color.white, true);
            coinIcon.preserveAspect = true;
            UIFactory.Place(coinIcon.rectTransform, new Vector2(0f, 1f), Vector2.zero, new Vector2(10f, 10f));
            HoverTip coinTip = coinIcon.gameObject.AddComponent<HoverTip>();
            coinTip.Title = Loc.T("possesslib_coins_title", "Coins");
            coinTip.Description = Loc.T("possesslib_coins_tip", "The unit's own money. Spent on food in cities and on crafting.");
            _coins = UIFactory.Text("Coins", _side, UIConst.FontNormal, TextAnchor.MiddleLeft, UIConst.Gold);
            UIFactory.Place((RectTransform)_coins.transform, new Vector2(0f, 1f), new Vector2(11f, 0f), new Vector2(20f, 10f));

            _trash = SlotView.Create(_side, "Trash", pSlot);
            UIFactory.Place((RectTransform)_trash.transform, new Vector2(0f, 0f), Vector2.zero, new Vector2(pSlot, pSlot));
            _trash.Address.Kind = SlotKind.Trash;
            _trash.SetPlaceholder(UISprites.Trash);

            Image sort = UIFactory.Image("Sort", _side, UISprites.Slot, Color.white, true);
            UIFactory.Place(sort.rectTransform, new Vector2(0f, 0f), new Vector2(0f, pSlot + 3f), new Vector2(pSlot * 0.75f, pSlot * 0.75f));
            Image sortIcon = UIFactory.Image("Icon", sort.transform, UISprites.Sort, Color.white);
            UIFactory.Stretch(sortIcon.rectTransform, 2f, 2f, 2f, 2f);
            sortIcon.preserveAspect = true;
            Button sortButton = sort.gameObject.AddComponent<Button>();
            sortButton.onClick.AddListener(() =>
            {
                if (_inventory == null) return;
                _inventory.SortMain();
                UISound.Play(UISound.Select);
            });
            HoverTip sortTip = sort.gameObject.AddComponent<HoverTip>();
            sortTip.Title = Loc.T("possesslib_sort", "Sort");
            sortTip.Description = Loc.T("possesslib_sort_tip", "Stack and sort the inventory (the hotbar is not touched).");

            _overflow = UIFactory.Text("Overflow", _side, UIConst.FontSmall, TextAnchor.UpperLeft, new Color(1f, 0.6f, 0.4f));
            UIFactory.Place((RectTransform)_overflow.transform, new Vector2(0f, 1f), new Vector2(0f, -12f), new Vector2(30f, 9f));
            _overflow.text = string.Empty;
        }

        private void BuildCustom()
        {
            _customVersion = Hud.CustomVersion;
            foreach (KeyValuePair<HudElementDefinition, RectTransform> pair in _elements) if (pair.Value != null) Destroy(pair.Value.gameObject);
            foreach (KeyValuePair<InventoryPanelDefinition, RectTransform> pair in _panels) if (pair.Value != null) Destroy(pair.Value.gameObject);
            _elements.Clear();
            _panels.Clear();

            List<HudElementDefinition> elements = new List<HudElementDefinition>(Hud.CustomElements);
            elements.Sort((a, b) => a.Order.CompareTo(b.Order));
            float[] offsets = new float[5];
            foreach (HudElementDefinition def in elements)
            {
                RectTransform parent;
                Vector2 anchor;
                int slot = (int)def.Anchor;
                switch (def.Anchor)
                {
                    case HudAnchor.TopRight: parent = _customTopRight; anchor = new Vector2(1f, 1f); break;
                    case HudAnchor.BottomLeft: parent = _bottomLeft; anchor = new Vector2(0f, 0f); break;
                    case HudAnchor.BottomRight: parent = _bottomRight; anchor = new Vector2(1f, 0f); break;
                    case HudAnchor.TopCenter: parent = _topCenter; anchor = new Vector2(0.5f, 1f); break;
                    default: parent = _customTopLeft; anchor = new Vector2(0f, 1f); break;
                }
                RectTransform container = UIFactory.Rect("Hud_" + def.Id, parent);
                float dir = anchor.y > 0.5f ? -1f : 1f;
                float baseOffset = def.Anchor == HudAnchor.TopCenter ? -UIConst.Margin - 120f : 0f;
                UIFactory.Place(container, anchor, new Vector2(0f, baseOffset + dir * offsets[slot]), def.Size);
                offsets[slot] += def.Size.y + 3f;
                try { def.Build?.Invoke(container); }
                catch (Exception e) { PLLog.Exception("HUD element " + def.Id + " Build", e); }
                _elements.Add(new KeyValuePair<HudElementDefinition, RectTransform>(def, container));
            }

            List<InventoryPanelDefinition> panels = new List<InventoryPanelDefinition>(Hud.CustomPanels);
            panels.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (InventoryPanelDefinition def in panels)
            {
                RectTransform panel = UIFactory.Rect("Panel_" + def.Id, _topLeft);
                UIFactory.Place(panel, new Vector2(0f, 1f), Vector2.zero, def.Size + new Vector2(0f, string.IsNullOrEmpty(def.Title) ? 0f : 12f));
                Image bg = UIFactory.Panel("Bg", panel, UIConst.PanelColor);
                UIFactory.Stretch(bg.rectTransform, -3f, -3f, -3f, -3f);
                if (!string.IsNullOrEmpty(def.Title))
                {
                    Text title = UIFactory.Text("Title", panel, UIConst.FontNormal, TextAnchor.UpperLeft, UIConst.Gold);
                    UIFactory.Place((RectTransform)title.transform, new Vector2(0f, 1f), Vector2.zero, new Vector2(def.Size.x, 11f));
                    title.text = Loc.T(def.Title, def.Title);
                }
                RectTransform content = UIFactory.Rect("Content", panel);
                UIFactory.Place(content, new Vector2(0f, 0f), Vector2.zero, def.Size);
                try { def.Build?.Invoke(content); }
                catch (Exception e) { PLLog.Exception("Inventory panel " + def.Id + " Build", e); }
                _panels.Add(new KeyValuePair<InventoryPanelDefinition, RectTransform>(def, panel));
            }
        }

        internal Inventory BoundInventory => _inventory;

        internal string DescribeGroups(Camera pCam)
        {
            return Describe("topLeft", _topLeft, pCam) + " " + Describe("topRight", _topRight, pCam) + " "
                   + Describe("health", _health.Root, pCam) + " " + Describe("equipment", _equipment.Root, pCam);
        }

        private static string Describe(string pName, RectTransform pRect, Camera pCam)
        {
            if (pRect == null) return pName + "=null";
            Vector3[] corners = new Vector3[4];
            pRect.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(pCam, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(pCam, corners[2]);
            return pName + "[active=" + pRect.gameObject.activeInHierarchy + " screen " + min + ".." + max + " scale=" + pRect.lossyScale.x + "]";
        }

        internal SlotView GetSlotView(int pIndex)
        {
            if (pIndex >= 0 && pIndex < _hotbar.Views.Length) return _hotbar.Views[pIndex];
            int main = pIndex - _hotbar.Views.Length;
            return main >= 0 && main < _main.Views.Length ? _main.Views[main] : null;
        }

        internal void Bind(Actor pActor, Inventory pInventory)
        {
            _actor = pActor;
            _inventory = pInventory;
            _hotbar.Bind(pInventory);
            _main.Bind(pInventory);
            _trash.Address.Inventory = pInventory;
            _trash.Refresh();
            _equipment.Bind(pInventory);
            _crafting.Bind(pActor, pInventory);
            _health.ResetSmoothing();
            _status.Tick(pActor, true);
            _nameVersion = -1;
            _lastMoney = -1;
            _lastOverflow = -1;
        }

        internal void SetInventoryOpen(bool pOpen)
        {
            _open = pOpen;
            _inventoryBg.gameObject.SetActive(pOpen);
            _main.Root.gameObject.SetActive(pOpen);
            _side.gameObject.SetActive(pOpen);
            _equipment.Root.gameObject.SetActive(pOpen);
            _crafting.Root.gameObject.SetActive(pOpen && PLConfig.EnableCrafting);
            foreach (KeyValuePair<InventoryPanelDefinition, RectTransform> pair in _panels) pair.Value.gameObject.SetActive(pOpen);
            if (pOpen && _inventory != null)
            {
                _crafting.Bind(_actor, _inventory);
                foreach (KeyValuePair<InventoryPanelDefinition, RectTransform> pair in _panels) RefreshPanel(pair.Key, pair.Value);
            }
            Tooltip.hideTooltip();
            Relayout();
        }

        private void RefreshPanel(InventoryPanelDefinition pDef, RectTransform pPanel)
        {
            if (pDef.OnRefresh == null) return;
            try { pDef.OnRefresh((RectTransform)pPanel.Find("Content"), _actor, _inventory); }
            catch (Exception e) { PLLog.Exception("Inventory panel " + pDef.Id + " OnRefresh", e); }
        }

        /// <summary>Stacks the left column vertically: hotbar, (grid), statuses, (crafting, panels), custom elements.</summary>
        private void Relayout()
        {
            float y = _open ? _mainY - _mainHeight - UIConst.PanelPadding - 5f : _hotbarY - UIConst.HudSlot - 4f;
            _status.Root.anchoredPosition = new Vector2(0f, y);
            y -= UIConst.StatusIcon + 6f;
            if (_open && PLConfig.EnableCrafting)
            {
                _crafting.Root.anchoredPosition = new Vector2(0f, y - 3f);
                y -= _crafting.Height + 9f;
            }
            if (_open)
            {
                foreach (KeyValuePair<InventoryPanelDefinition, RectTransform> pair in _panels)
                {
                    pair.Value.anchoredPosition = new Vector2(0f, y - 3f);
                    y -= pair.Value.sizeDelta.y + 9f;
                }
            }
            _customTopLeft.anchoredPosition = new Vector2(0f, y);
        }

        internal void Tick()
        {
            if (_customVersion != Hud.CustomVersion)
            {
                BuildCustom();
                SetInventoryOpen(_open);
            }
            _hotbar.Tick();
            if (_open)
            {
                _main.Tick();
                _equipment.Tick();
                _crafting.Tick();
                _trash.Refresh();
            }
            _health.Tick(_actor);
            _status.Tick(_actor, false);
            _toasts.Tick();
            UpdateItemName();
            UpdateSide();

            for (int i = 0; i < _elements.Count; i++)
            {
                HudElementDefinition def = _elements[i].Key;
                if (def.OnUpdate == null || _actor == null) continue;
                try { def.OnUpdate(_elements[i].Value, _actor); }
                catch (Exception e)
                {
                    PLLog.Exception("HUD element " + def.Id + " OnUpdate (disabled)", e);
                    def.OnUpdate = null;
                }
            }
        }

        private void UpdateItemName()
        {
            if (_inventory == null) return;
            int version = _inventory.Version;
            if (version == _nameVersion) return;
            _nameVersion = version;
            ItemStack selected = _inventory.SelectedStack;
            if (selected == null)
            {
                _itemName.text = string.Empty;
                return;
            }
            string name = selected.GetDisplayName();
            string hex = selected.GetRarityColorHex();
            if (_inventory.PinnedCursedWeapon != null) name += "  <color=#FF5555>(" + Loc.F("possesslib_cursed_in_hand", "cursed $0 in hand", _inventory.PinnedCursedWeapon.getName(true)) + ")</color>";
            _itemName.text = hex != null ? "<color=" + (hex[0] == '#' ? hex : "#" + hex) + ">" + name + "</color>" : name;
        }

        private void UpdateSide()
        {
            if (!_open || _actor == null || _inventory == null) return;
            int money = _actor.data.money;
            if (money != _lastMoney)
            {
                _lastMoney = money;
                _coins.text = money.ToString();
            }
            int overflow = _inventory.ResourceOverflow;
            if (overflow != _lastOverflow)
            {
                _lastOverflow = overflow;
                _overflow.text = overflow > 0 ? "+" + overflow + " " + Loc.T("possesslib_overflow", "carried") : string.Empty;
            }
        }
    }
}
