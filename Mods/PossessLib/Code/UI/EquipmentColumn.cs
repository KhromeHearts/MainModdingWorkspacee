using System.Collections.Generic;
using PossessLib.Core;
using PossessLib.Model;
using UnityEngine;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>
    /// Terraria-style equipment column (or row in the unit window): helmet, armor, boots, ring, amulet
    /// and any slots registered by other mods, plus a defense (armor stat) readout.
    /// </summary>
    internal sealed class EquipmentColumn
    {
        public RectTransform Root;
        private readonly List<SlotView> _views = new List<SlotView>();
        private Text _defense;
        private Inventory _inventory;
        private int _registryVersion = -1;
        private bool _vertical;
        private float _size;
        private int _lastArmor = int.MinValue;
        private readonly List<Item> _lastItems = new List<Item>();

        public Vector2 Size { get; private set; }

        public static EquipmentColumn Create(Transform pParent, bool pVertical, float pSlotSize)
        {
            EquipmentColumn col = new EquipmentColumn { _vertical = pVertical, _size = pSlotSize };
            col.Root = UIFactory.Rect("Equipment", pParent);
            col.Rebuild();
            return col;
        }

        private void Rebuild()
        {
            for (int i = Root.childCount - 1; i >= 0; i--) Object.Destroy(Root.GetChild(i).gameObject);
            _views.Clear();
            _lastItems.Clear();
            _registryVersion = EquipSlotRegistry.Version;

            IReadOnlyList<EquipSlotInfo> slots = EquipSlotRegistry.Slots;
            float step = _size + UIConst.Gap;
            float header = 13f;

            // defense readout (Terraria's shield icon)
            Image shield = UIFactory.Image("DefenseIcon", Root, UISprites.IconArmor, Color.white, true);
            shield.preserveAspect = true;
            HoverTip tip = shield.gameObject.AddComponent<HoverTip>();
            tip.Title = Loc.T("armor", "Armor");
            tip.Description = Loc.T("possesslib_armor_tip", "Defense from your gear and traits (the unit's armor stat).");
            _defense = UIFactory.Text("Defense", Root, UIConst.FontNormal, TextAnchor.MiddleLeft, Color.white);

            if (_vertical)
            {
                Size = new Vector2(_size, header + slots.Count * step - UIConst.Gap);
                UIFactory.Place(shield.rectTransform, new Vector2(1f, 1f), new Vector2(-_size + 10f, 0f), new Vector2(10f, 10f));
                UIFactory.Place((RectTransform)_defense.transform, new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(_size - 11f, 10f));
            }
            else
            {
                Size = new Vector2(slots.Count * step + 26f, _size);
                UIFactory.Place(shield.rectTransform, new Vector2(0f, 1f), new Vector2(slots.Count * step + 2f, -(_size - 10f) / 2f), new Vector2(10f, 10f));
                UIFactory.Place((RectTransform)_defense.transform, new Vector2(0f, 1f), new Vector2(slots.Count * step + 13f, -(_size - 10f) / 2f), new Vector2(14f, 10f));
            }
            Root.sizeDelta = Size;

            for (int i = 0; i < slots.Count; i++)
            {
                EquipSlotInfo info = slots[i];
                SlotView view = SlotView.Create(Root, "Equip_" + info.Id, _size);
                Vector2 anchor = _vertical ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
                Vector2 pos = _vertical ? new Vector2(0f, -header - i * step) : new Vector2(i * step, 0f);
                UIFactory.Place((RectTransform)view.transform, anchor, pos, new Vector2(_size, _size));
                view.Address.Kind = info.IsVanilla ? SlotKind.Equipment : SlotKind.CustomEquipment;
                view.Address.EquipType = info.Type;
                view.Address.Custom = info.Custom;
                view.Address.Inventory = _inventory;
                view.SetPlaceholder(info.Placeholder);
                _views.Add(view);
                _lastItems.Add(null);
            }
        }

        public void Bind(Inventory pInventory)
        {
            _inventory = pInventory;
            foreach (SlotView view in _views) view.Address.Inventory = pInventory;
            _lastArmor = int.MinValue;
            ForceRefresh();
        }

        public void Tick()
        {
            if (_registryVersion != EquipSlotRegistry.Version)
            {
                Rebuild();
                Bind(_inventory);
                return;
            }
            Actor owner = _inventory != null ? _inventory.Owner : null;
            bool changed = false;
            for (int i = 0; i < _views.Count; i++)
            {
                SlotView v = _views[i];
                Item current = null;
                if (v.Address.Kind == SlotKind.Equipment && owner != null && owner.hasEquipment())
                {
                    current = owner.equipment.getSlot(v.Address.EquipType).getItem();
                }
                if (!ReferenceEquals(current, _lastItems[i]))
                {
                    _lastItems[i] = current;
                    changed = true;
                }
            }
            if (changed || _inventory != null && _inventory.Version != _invVersion || Sync.CursorState.Version != _cursorVersion) ForceRefresh();

            int armor = owner != null ? Mathf.RoundToInt(owner.stats["armor"]) : 0;
            if (armor != _lastArmor)
            {
                _lastArmor = armor;
                _defense.text = armor.ToString();
            }
        }

        private int _invVersion = -1;
        private int _cursorVersion = -1;

        private void ForceRefresh()
        {
            _invVersion = _inventory != null ? _inventory.Version : -1;
            _cursorVersion = Sync.CursorState.Version;
            foreach (SlotView view in _views) view.Refresh();
        }
    }
}
