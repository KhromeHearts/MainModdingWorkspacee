using System.Collections.Generic;
using System.Text;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>
    /// Crafting list under the inventory (Terraria puts it bottom-left). Craftable recipes first,
    /// the rest dimmed with the reason in the tooltip. Pages instead of a scroll view to stay robust.
    /// </summary>
    internal sealed class CraftingPanel
    {
        private const int RowsPerPage = 6;
        private const float RowHeight = 16f;
        public const float Width = 200f;

        public RectTransform Root;
        private readonly Row[] _rows = new Row[RowsPerPage];
        private Text _title;
        private Text _page;
        private readonly List<RecipeDefinition> _visible = new List<RecipeDefinition>();
        private readonly List<bool> _craftable = new List<bool>();
        private int _pageIndex;
        private float _nextRefresh;
        private int _lastInvVersion = -1;
        private int _lastRecipeVersion = -1;
        private Actor _actor;
        private Inventory _inventory;

        public float Height => 14f + RowsPerPage * (RowHeight + 1f) + 2f;

        private sealed class Row
        {
            public GameObject Go;
            public Image Bg;
            public Image Icon;
            public Text Name;
            public Text Cost;
            public RecipeRowView View;
        }

        public static CraftingPanel Create(Transform pParent)
        {
            CraftingPanel p = new CraftingPanel();
            p.Root = UIFactory.Rect("Crafting", pParent);
            UIFactory.Place(p.Root, new Vector2(0f, 1f), Vector2.zero, new Vector2(Width, p.Height));
            Image bg = UIFactory.Panel("Bg", p.Root, UIConst.PanelColor);
            UIFactory.Stretch(bg.rectTransform, -3f, -3f, -3f, -3f);

            Image icon = UIFactory.Image("TitleIcon", p.Root, UISprites.IconCraft, Color.white);
            icon.preserveAspect = true;
            UIFactory.Place(icon.rectTransform, new Vector2(0f, 1f), Vector2.zero, new Vector2(11f, 11f));
            p._title = UIFactory.Text("Title", p.Root, UIConst.FontNormal, TextAnchor.MiddleLeft, UIConst.Gold);
            UIFactory.Place((RectTransform)p._title.transform, new Vector2(0f, 1f), new Vector2(13f, 0f), new Vector2(100f, 11f));
            p._title.text = Loc.T("possesslib_crafting", "Crafting");

            p._page = UIFactory.Text("Page", p.Root, UIConst.FontSmall, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Place((RectTransform)p._page.transform, new Vector2(1f, 1f), new Vector2(-14f, 0f), new Vector2(28f, 11f));
            MakePageButton(p, "<", new Vector2(-44f, 0f), -1);
            MakePageButton(p, ">", new Vector2(0f, 0f), 1);

            for (int i = 0; i < RowsPerPage; i++)
            {
                Row row = new Row();
                RectTransform rt = UIFactory.Rect("Recipe_" + i, p.Root);
                UIFactory.Place(rt, new Vector2(0f, 1f), new Vector2(0f, -14f - i * (RowHeight + 1f)), new Vector2(Width, RowHeight));
                row.Go = rt.gameObject;
                row.Bg = rt.gameObject.AddComponent<Image>();
                row.Bg.sprite = UISprites.White;
                row.Bg.color = new Color(0f, 0f, 0f, 0.3f);
                row.Icon = UIFactory.Image("Icon", rt, null, Color.white);
                row.Icon.preserveAspect = true;
                UIFactory.Place(row.Icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(RowHeight - 3f, RowHeight - 3f));
                row.Name = UIFactory.Text("Name", rt, UIConst.FontNormal, TextAnchor.UpperLeft, Color.white);
                UIFactory.Place((RectTransform)row.Name.transform, new Vector2(0f, 1f), new Vector2(RowHeight + 1f, -0.5f), new Vector2(Width - RowHeight - 2f, 8f));
                row.Cost = UIFactory.Text("Cost", rt, UIConst.FontSmall, TextAnchor.LowerLeft, new Color(0.85f, 0.85f, 0.85f));
                UIFactory.Place((RectTransform)row.Cost.transform, new Vector2(0f, 0f), new Vector2(RowHeight + 1f, 0.5f), new Vector2(Width - RowHeight - 2f, 7f));
                row.View = rt.gameObject.AddComponent<RecipeRowView>();
                row.View.Panel = p;
                p._rows[i] = row;
            }
            return p;
        }

        private static void MakePageButton(CraftingPanel pPanel, string pLabel, Vector2 pPos, int pDelta)
        {
            Image img = UIFactory.Image("Page" + pLabel, pPanel.Root, UISprites.Slot, Color.white, true);
            UIFactory.Place(img.rectTransform, new Vector2(1f, 1f), pPos, new Vector2(12f, 11f));
            Text t = UIFactory.Text("Label", img.transform, UIConst.FontNormal, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch((RectTransform)t.transform);
            t.text = pLabel;
            Button b = img.gameObject.AddComponent<Button>();
            b.onClick.AddListener(() =>
            {
                pPanel._pageIndex += pDelta;
                pPanel.Refresh(true);
                UISound.Play(UISound.Select);
            });
        }

        public void Bind(Actor pActor, Inventory pInventory)
        {
            _actor = pActor;
            _inventory = pInventory;
            _pageIndex = 0;
            Refresh(true);
        }

        public void Tick()
        {
            if (_inventory == null) return;
            bool changed = _inventory.Version != _lastInvVersion || Crafting.Version != _lastRecipeVersion;
            if (changed || Time.unscaledTime >= _nextRefresh) Refresh(false);
        }

        private void Refresh(bool pForce)
        {
            _nextRefresh = Time.unscaledTime + 0.5f;
            _lastInvVersion = _inventory != null ? _inventory.Version : -1;
            _lastRecipeVersion = Crafting.Version;
            _visible.Clear();
            _craftable.Clear();
            if (_actor == null || _inventory == null || !PLConfig.EnableCrafting)
            {
                for (int i = 0; i < RowsPerPage; i++) _rows[i].Go.SetActive(false);
                _page.text = string.Empty;
                return;
            }

            // craftable first, then the rest; stable within each group
            IReadOnlyList<RecipeDefinition> all = Crafting.All;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    RecipeDefinition r = all[i];
                    if (!Crafting.IsVisible(r)) continue;
                    bool can = Crafting.CanCraft(_actor, _inventory, r, out string _);
                    if (can != (pass == 0)) continue;
                    _visible.Add(r);
                    _craftable.Add(can);
                }
            }

            int pages = Mathf.Max(1, Mathf.CeilToInt(_visible.Count / (float)RowsPerPage));
            _pageIndex = ((_pageIndex % pages) + pages) % pages;
            _page.text = (_pageIndex + 1) + "/" + pages;
            for (int i = 0; i < RowsPerPage; i++)
            {
                int index = _pageIndex * RowsPerPage + i;
                Row row = _rows[i];
                if (index >= _visible.Count)
                {
                    row.Go.SetActive(false);
                    continue;
                }
                RecipeDefinition r = _visible[index];
                bool can = _craftable[index];
                row.Go.SetActive(true);
                row.View.Recipe = r;
                row.Icon.sprite = Crafting.GetResultIcon(r);
                row.Icon.enabled = row.Icon.sprite != null;
                string name = Crafting.GetResultName(r);
                if (r.ResultCount > 1) name += " x" + r.ResultCount;
                row.Name.text = name;
                row.Cost.text = CostText(r);
                Color tint = can ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                row.Icon.color = tint;
                row.Name.color = can ? new Color(0.75f, 1f, 0.6f) : new Color(0.75f, 0.75f, 0.75f, 0.7f);
                row.Bg.color = can ? new Color(0.15f, 0.35f, 0.12f, 0.55f) : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private static readonly StringBuilder _sb = new StringBuilder();

        public static string CostText(RecipeDefinition pRecipe)
        {
            _sb.Length = 0;
            foreach (KeyValuePair<string, int> pair in pRecipe.Resources)
            {
                if (_sb.Length > 0) _sb.Append(", ");
                ResourceAsset res = AssetManager.resources.get(pair.Key);
                _sb.Append(pair.Value).Append(' ').Append(res != null ? res.getTranslatedName() : pair.Key);
            }
            foreach (KeyValuePair<string, int> pair in pRecipe.CustomItems)
            {
                if (_sb.Length > 0) _sb.Append(", ");
                CustomItemDefinition def = CustomItemRegistry.Get(pair.Key);
                _sb.Append(pair.Value).Append(' ').Append(def != null ? Loc.T(def.Name, def.Name) : pair.Key);
            }
            if (pRecipe.Money > 0)
            {
                if (_sb.Length > 0) _sb.Append(", ");
                _sb.Append(pRecipe.Money).Append(' ').Append(Loc.T("possesslib_coins", "coins"));
            }
            return _sb.ToString();
        }

        internal void OnRowClicked(RecipeDefinition pRecipe)
        {
            if (pRecipe == null || _actor == null || _inventory == null) return;
            Crafting.Craft(_actor, _inventory, pRecipe);
            Refresh(true);
        }

        internal Actor Actor => _actor;
        internal Inventory Inventory => _inventory;
    }

    /// <summary>Click and tooltip handling for one recipe row.</summary>
    internal sealed class RecipeRowView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public CraftingPanel Panel;
        public RecipeDefinition Recipe;

        public void OnPointerClick(PointerEventData pEventData)
        {
            if (pEventData.button == PointerEventData.InputButton.Left) Panel.OnRowClicked(Recipe);
        }

        public void OnPointerEnter(PointerEventData pEventData)
        {
            UIState.HoverCount++;
            if (Recipe == null) return;
            string reason = null;
            bool can = Panel.Actor != null && Crafting.CanCraft(Panel.Actor, Panel.Inventory, Recipe, out reason);
            if (Recipe.ResultKind == RecipeResultKind.Item)
            {
                EquipmentAsset asset = AssetManager.items.get(Recipe.ResultId);
                if (asset != null)
                {
                    Tooltip.show(this, "equipment_in_editor", new TooltipData { item_asset = asset });
                    return;
                }
            }
            string desc = Loc.T("possesslib_craft_cost", "Cost") + ": " + CraftingPanel.CostText(Recipe);
            if (!can && reason != null) desc += "\n<color=#FF8080>" + reason + "</color>";
            Tooltip.show(this, "tip", new TooltipData { tip_name = Crafting.GetResultName(Recipe), tip_description = desc });
        }

        public void OnPointerExit(PointerEventData pEventData)
        {
            UIState.HoverCount = Mathf.Max(0, UIState.HoverCount - 1);
            Tooltip.hideTooltip();
        }

        private void OnDisable()
        {
            UIState.HoverCount = 0;
        }
    }
}
