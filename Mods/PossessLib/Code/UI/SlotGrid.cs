using PossessLib.Model;
using UnityEngine;

namespace PossessLib.UI
{
    /// <summary>A block of inventory slots laid out in rows (positions computed from constants, no layout groups).</summary>
    internal sealed class SlotGrid
    {
        public RectTransform Root;
        public SlotView[] Views;
        private int _columns;
        private Inventory _inventory;
        private int _version = -1;
        private int _cursorVersion = -1;

        public Vector2 Size { get; private set; }

        public static SlotGrid Create(Transform pParent, string pName, int pStart, int pCount, int pColumns, float pSize,
            bool pIsHud, bool pHudHotbar, bool pNumberKeys)
        {
            SlotGrid grid = new SlotGrid { _columns = Mathf.Max(1, pColumns) };
            grid.Root = UIFactory.Rect(pName, pParent);
            int rows = Mathf.Max(1, Mathf.CeilToInt(pCount / (float)grid._columns));
            int cols = Mathf.Min(grid._columns, Mathf.Max(1, pCount));
            grid.Size = new Vector2(cols * pSize + (cols - 1) * UIConst.Gap, rows * pSize + (rows - 1) * UIConst.Gap);
            UIFactory.Place(grid.Root, new Vector2(0f, 1f), Vector2.zero, grid.Size);

            grid.Views = new SlotView[pCount];
            for (int i = 0; i < pCount; i++)
            {
                int col = i % grid._columns;
                int row = i / grid._columns;
                SlotView view = SlotView.Create(grid.Root, "Slot_" + (pStart + i), pSize);
                UIFactory.Place((RectTransform)view.transform, new Vector2(0f, 1f),
                    new Vector2(col * (pSize + UIConst.Gap), -row * (pSize + UIConst.Gap)), new Vector2(pSize, pSize));
                view.Address.Kind = SlotKind.Inventory;
                view.Address.Index = pStart + i;
                view.Address.HudHotbar = pHudHotbar;
                view.IsHud = pIsHud;
                if (pNumberKeys) view.SetKeyLabel(i == 9 ? "0" : (i + 1).ToString());
                grid.Views[i] = view;
            }
            return grid;
        }

        public void Bind(Inventory pInventory)
        {
            _inventory = pInventory;
            for (int i = 0; i < Views.Length; i++) Views[i].Address.Inventory = pInventory;
            ForceRefresh();
        }

        /// <summary>Redraws only when the inventory (or the cursor) changed since the last call.</summary>
        public void Tick()
        {
            if (_inventory == null) return;
            if (_inventory.Version == _version && Sync.CursorState.Version == _cursorVersion) return;
            ForceRefresh();
        }

        public void ForceRefresh()
        {
            _version = _inventory != null ? _inventory.Version : -1;
            _cursorVersion = Sync.CursorState.Version;
            for (int i = 0; i < Views.Length; i++) Views[i].Refresh();
        }
    }
}
