using PossessLib.Api;
using PossessLib.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PossessLib.UI
{
    internal enum SlotKind
    {
        Inventory,
        Equipment,
        CustomEquipment,
        Trash
    }

    /// <summary>What a slot widget shows and acts on.</summary>
    internal sealed class SlotAddress
    {
        public SlotKind Kind;
        public Inventory Inventory;
        public int Index;
        public EquipmentType EquipType;
        public EquipmentSlotDefinition Custom;
        /// <summary>HUD hotbar while the inventory is closed: a click selects instead of picking up.</summary>
        public bool HudHotbar;
    }

    /// <summary>Tracks whether the mouse is over PossessLib UI (used to stop attacks and dashes from slot clicks).</summary>
    internal static class UIState
    {
        public static int HoverCount;
        public static SlotView HoveredSlot;
        public static bool PointerOverPossessLib => HoverCount > 0;
        public static bool Dragging;
    }

    /// <summary>
    /// One inventory slot: background, rarity frame, icon, count, hotbar number, durability bar and
    /// selection / cursed / in-hand markers. All mouse input is forwarded to <see cref="InventoryActions"/>.
    /// </summary>
    internal sealed class SlotView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public SlotAddress Address = new SlotAddress();

        private Image _bg;
        private Image _rarity;
        private Image _placeholder;
        private Image _icon;
        private Image _selected;
        private Image _marker;
        private Image _durabilityBg;
        private Image _durability;
        private Text _count;
        private Text _key;
        private Text _missing;
        private bool _hover;

        /// <summary>The stack shown right now (for tooltips and actions).</summary>
        public ItemStack Shown { get; private set; }

        /// <summary>True when the icon image is enabled and has a sprite (used by the self-test).</summary>
        internal bool IconVisible => _icon != null && _icon.enabled && _icon.sprite != null && _icon.gameObject.activeInHierarchy;

        private Item _equipWrapperItem;
        private ItemStack _equipWrapper;

        public static SlotView Create(Transform pParent, string pName, float pSize)
        {
            RectTransform root = UIFactory.Rect(pName, pParent);
            root.sizeDelta = new Vector2(pSize, pSize);
            SlotView view = root.gameObject.AddComponent<SlotView>();
            view.Build(pSize);
            return view;
        }

        private void Build(float pSize)
        {
            _bg = gameObject.AddComponent<Image>();
            _bg.sprite = UISprites.Slot;
            _bg.color = UIConst.SlotTint;
            _bg.raycastTarget = true;

            float inset = Mathf.Round(pSize * 0.12f);
            _rarity = UIFactory.Image("Rarity", transform, UISprites.SlotFrame, Color.white);
            UIFactory.Stretch(_rarity.rectTransform);
            _rarity.enabled = false;

            _placeholder = UIFactory.Image("Placeholder", transform, null, new Color(1f, 1f, 1f, 0.22f));
            UIFactory.Stretch(_placeholder.rectTransform, inset + 1, inset + 1, inset + 1, inset + 1);
            _placeholder.preserveAspect = true;
            _placeholder.enabled = false;

            _icon = UIFactory.Image("Icon", transform, null, Color.white);
            UIFactory.Stretch(_icon.rectTransform, inset, inset, inset, inset);
            _icon.preserveAspect = true;
            _icon.enabled = false;

            _missing = UIFactory.Text("Missing", transform, UIConst.FontLarge, TextAnchor.MiddleCenter, new Color(1f, 0.6f, 0.6f));
            UIFactory.Stretch((RectTransform)_missing.transform);
            _missing.text = "?";
            _missing.enabled = false;

            _durabilityBg = UIFactory.Image("DurabilityBg", transform, UISprites.White, new Color(0f, 0f, 0f, 0.7f));
            RectTransform dbg = _durabilityBg.rectTransform;
            dbg.anchorMin = new Vector2(0f, 0f);
            dbg.anchorMax = new Vector2(1f, 0f);
            dbg.pivot = new Vector2(0.5f, 0f);
            dbg.offsetMin = new Vector2(inset, inset * 0.5f);
            dbg.offsetMax = new Vector2(-inset, inset * 0.5f + 1.5f);
            _durability = UIFactory.Image("Durability", _durabilityBg.transform, UISprites.White, Color.green);
            _durability.type = Image.Type.Filled;
            _durability.fillMethod = Image.FillMethod.Horizontal;
            UIFactory.Stretch(_durability.rectTransform);
            _durabilityBg.gameObject.SetActive(false);

            _selected = UIFactory.Image("Selected", transform, UISprites.SlotSelect, Color.white);
            UIFactory.Stretch(_selected.rectTransform, -1, -1, -1, -1);
            _selected.enabled = false;

            _marker = UIFactory.Image("Marker", transform, UISprites.SlotFrame, UIConst.CursedRed);
            UIFactory.Stretch(_marker.rectTransform, 1, 1, 1, 1);
            _marker.enabled = false;

            _count = UIFactory.Text("Count", transform, UIConst.FontNormal, TextAnchor.LowerRight, Color.white);
            RectTransform crt = (RectTransform)_count.transform;
            UIFactory.Stretch(crt, 1f, 1f, 2f, 1f);

            _key = UIFactory.Text("Key", transform, UIConst.FontSmall, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.75f));
            UIFactory.Stretch((RectTransform)_key.transform, 2f, 1f, 1f, 1f);
            _key.text = string.Empty;
        }

        public void SetKeyLabel(string pText)
        {
            _key.text = pText ?? string.Empty;
        }

        public void SetPlaceholder(Sprite pSprite)
        {
            _placeholder.sprite = pSprite;
        }

        /// <summary>Re-reads the slot content from the game/inventory and redraws.</summary>
        public void Refresh()
        {
            ItemStack stack = ResolveStack();
            Shown = stack;

            bool has = stack != null;
            Sprite icon = has ? stack.GetIcon() : null;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _missing.enabled = has && icon == null;
            if (_missing.enabled) _missing.text = stack.Kind == StackKind.Unresolved ? "?" : FirstLetter(stack.GetDisplayName());
            _placeholder.enabled = !has && _placeholder.sprite != null;

            _count.text = has && stack.Count > 1 ? stack.Count.ToString() : string.Empty;

            string rarityHex = has ? stack.GetRarityColorHex() : null;
            _rarity.enabled = rarityHex != null;
            if (rarityHex != null) _rarity.color = UIFactory.ParseHex(rarityHex, Color.white);

            // durability
            bool showDurability = false;
            if (has && stack.Kind == StackKind.Item && stack.Item != null && stack.Item.getAsset() != null)
            {
                int max = stack.Item.getDurabilityMax();
                int cur = stack.Item.getDurabilityCurrent();
                if (max > 0 && cur < max)
                {
                    showDurability = true;
                    float pct = Mathf.Clamp01((float)cur / max);
                    _durability.fillAmount = pct;
                    _durability.color = Color.Lerp(new Color(0.9f, 0.2f, 0.15f), new Color(0.3f, 0.95f, 0.3f), pct);
                }
            }
            if (_durabilityBg.gameObject.activeSelf != showDurability) _durabilityBg.gameObject.SetActive(showDurability);

            // markers
            bool selected = false;
            bool cursedInHand = false;
            bool inHand = false;
            Inventory inv = Address.Inventory;
            if (Address.Kind == SlotKind.Inventory && inv != null)
            {
                selected = IsHud && inv.IsHotbar(Address.Index) && inv.Selected == Address.Index;
                if (has && stack.Item != null)
                {
                    cursedInHand = ReferenceEquals(inv.PinnedCursedWeapon, stack.Item);
                    Actor owner = inv.Owner;
                    inHand = !cursedInHand && owner != null && owner.hasEquipment() && ReferenceEquals(owner.equipment.weapon.getItem(), stack.Item);
                }
            }
            else if (Address.Kind == SlotKind.Equipment && has && stack.IsCursed)
            {
                cursedInHand = true;
            }
            _selected.enabled = selected;
            float scale = selected ? 1.1f : 1f;
            if (!Mathf.Approximately(transform.localScale.x, scale)) transform.localScale = new Vector3(scale, scale, 1f);
            _marker.enabled = cursedInHand || (inHand && !IsHud);
            _marker.color = cursedInHand ? UIConst.CursedRed : UIConst.InHandBlue;
            _bg.color = _hover ? UIConst.SlotHover : UIConst.SlotTint;
        }

        /// <summary>True for slots of the possessed unit's HUD (selection is shown there).</summary>
        public bool IsHud;

        private ItemStack ResolveStack()
        {
            Inventory inv = Address.Inventory;
            if (inv == null) return null;
            switch (Address.Kind)
            {
                case SlotKind.Inventory:
                    return inv.Get(Address.Index);
                case SlotKind.Trash:
                    return inv.Trash;
                case SlotKind.CustomEquipment:
                    return Address.Custom != null ? inv.GetCustomEquipment(Address.Custom.Id) : null;
                case SlotKind.Equipment:
                {
                    Actor owner = inv.Owner;
                    Item item = owner != null && owner.hasEquipment() ? owner.equipment.getSlot(Address.EquipType).getItem() : null;
                    if (item == null) return null;
                    if (!ReferenceEquals(item, _equipWrapperItem))
                    {
                        _equipWrapperItem = item;
                        _equipWrapper = ItemStack.FromItem(item);
                    }
                    return _equipWrapper;
                }
            }
            return null;
        }

        private static string FirstLetter(string pName)
        {
            return string.IsNullOrEmpty(pName) ? "?" : pName.Substring(0, 1).ToUpperInvariant();
        }

        // ------------------------------------------------------------------ input

        public void OnPointerDown(PointerEventData pEventData)
        {
            InventoryActions.OnSlotPointerDown(this, pEventData.button);
        }

        public void OnPointerEnter(PointerEventData pEventData)
        {
            if (!_hover)
            {
                _hover = true;
                UIState.HoverCount++;
            }
            UIState.HoveredSlot = this;
            _bg.color = UIConst.SlotHover;
            SlotTooltips.Show(this);
        }

        public void OnPointerExit(PointerEventData pEventData)
        {
            ClearHover();
            SlotTooltips.Hide();
        }

        private void ClearHover()
        {
            if (_hover)
            {
                _hover = false;
                UIState.HoverCount = Mathf.Max(0, UIState.HoverCount - 1);
            }
            if (ReferenceEquals(UIState.HoveredSlot, this)) UIState.HoveredSlot = null;
            if (_bg != null) _bg.color = UIConst.SlotTint;
        }

        private void OnDisable()
        {
            ClearHover();
        }

        public void OnBeginDrag(PointerEventData pEventData)
        {
            UIState.Dragging = pEventData.button == PointerEventData.InputButton.Left;
        }

        public void OnDrag(PointerEventData pEventData)
        {
            // required for OnBeginDrag/OnEndDrag to fire; the cursor view follows the mouse itself
        }

        public void OnEndDrag(PointerEventData pEventData)
        {
            bool wasDragging = UIState.Dragging;
            UIState.Dragging = false;
            if (wasDragging) InventoryActions.OnDragReleased(this, pEventData);
        }

        public void OnDrop(PointerEventData pEventData)
        {
            if (UIState.Dragging) InventoryActions.OnDroppedOnSlot(this);
        }
    }
}
