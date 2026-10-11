using UnityEngine;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>
    /// Layout constants in canvas units. The game's main canvas is about 500 units tall (times the
    /// player's UI-size setting), so these sizes stay the same on every resolution.
    /// </summary>
    internal static class UIConst
    {
        public const float Margin = 6f;
        public const float HudSlot = 24f;
        public const float WindowSlot = 20f;
        public const float Gap = 2f;
        public const float NameLabelHeight = 11f;
        public const float StatusIcon = 14f;
        public const float HealthWidth = 132f;   // GoldHealthBar.png is 88x27; drawn at 1.5x
        public const float HealthHeight = 40.5f;
        public const float PanelPadding = 4f;
        public const int FontSmall = 7;
        public const int FontNormal = 8;
        public const int FontLarge = 10;

        public static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.11f, 0.82f);
        public static readonly Color SlotTint = new Color(1f, 1f, 1f, 0.92f);
        public static readonly Color SlotHover = new Color(1.2f, 1.2f, 1.1f, 1f);
        public static readonly Color CursedRed = new Color(1f, 0.25f, 0.25f, 1f);
        public static readonly Color InHandBlue = new Color(0.45f, 0.85f, 1f, 1f);
        public static readonly Color Gold = new Color(1f, 0.85f, 0.3f, 1f);
    }

    /// <summary>Cached sprites: PossessLib's own (GameResources/ui/possesslib) and the game's UI icons.</summary>
    internal static class UISprites
    {
        private static Sprite _white;

        public static Sprite Slot => Load("ui/possesslib/slot");
        public static Sprite SlotSelect => Load("ui/possesslib/slot_select");
        public static Sprite SlotFrame => Load("ui/possesslib/slot_frame");
        public static Sprite Trash => Load("ui/possesslib/trash");
        public static Sprite Sort => Load("ui/possesslib/sort");
        public static Sprite HealthFrame => Load("ui/possesslib/health_frame");
        public static Sprite Panel => Load("ui/special/windowInnerSliced");
        public static Sprite Button => Load("ui/special/special_buttonGray");
        public static Sprite IconArmor => Load("ui/Icons/iconArmor");
        public static Sprite IconLevel => Load("ui/Icons/iconLevels");
        public static Sprite IconMoney => Load("ui/Icons/iconMoney");
        public static Sprite IconCraft => Load("ui/Icons/iconEquipmentEditor");
        public static Sprite IconInventory => Load("ui/Icons/iconCityInventory");

        /// <summary>A plain white sprite for bars and fills.</summary>
        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    Texture2D tex = Texture2D.whiteTexture;
                    _white = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    _white.name = "possesslib_white";
                }
                return _white;
            }
        }

        public static Sprite Load(string pPath)
        {
            Sprite sprite = SpriteTextureLoader.getSprite(pPath);
            if (sprite == null) sprite = Resources.Load<Sprite>(pPath);
            return sprite;
        }
    }

    /// <summary>Small helpers to build uGUI objects in WorldBox's style (pixel font, game panels).</summary>
    internal static class UIFactory
    {
        public static RectTransform Rect(string pName, Transform pParent)
        {
            GameObject go = new GameObject(pName, typeof(RectTransform));
            go.transform.SetParent(pParent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.localScale = Vector3.one;
            return rt;
        }

        /// <summary>Anchors a rect to a point of its parent (pivot = anchor) with a size and offset.</summary>
        public static RectTransform Place(RectTransform pRect, Vector2 pAnchor, Vector2 pPosition, Vector2 pSize)
        {
            pRect.anchorMin = pAnchor;
            pRect.anchorMax = pAnchor;
            pRect.pivot = pAnchor;
            pRect.anchoredPosition = pPosition;
            pRect.sizeDelta = pSize;
            return pRect;
        }

        /// <summary>Stretches a rect over its parent with insets (left, bottom, right, top).</summary>
        public static RectTransform Stretch(RectTransform pRect, float pLeft = 0f, float pBottom = 0f, float pRight = 0f, float pTop = 0f)
        {
            pRect.anchorMin = Vector2.zero;
            pRect.anchorMax = Vector2.one;
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.offsetMin = new Vector2(pLeft, pBottom);
            pRect.offsetMax = new Vector2(-pRight, -pTop);
            return pRect;
        }

        public static Image Image(string pName, Transform pParent, Sprite pSprite, Color pColor, bool pRaycast = false)
        {
            RectTransform rt = Rect(pName, pParent);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = pSprite;
            img.color = pColor;
            img.raycastTarget = pRaycast;
            return img;
        }

        /// <summary>The game's inner window sprite, sliced, tinted dark: the panel look of WorldBox windows.</summary>
        public static Image Panel(string pName, Transform pParent, Color pColor)
        {
            Image img = Image(pName, pParent, UISprites.Panel, pColor, true);
            if (img.sprite != null && img.sprite.border != Vector4.zero) img.type = UnityEngine.UI.Image.Type.Sliced;
            if (img.sprite == null) img.sprite = UISprites.White;
            return img;
        }

        public static Text Text(string pName, Transform pParent, int pSize, TextAnchor pAlign, Color pColor)
        {
            RectTransform rt = Rect(pName, pParent);
            Text text = rt.gameObject.AddComponent<Text>();
            text.font = LocalizedTextManager.current_font != null ? LocalizedTextManager.current_font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = pSize;
            text.alignment = pAlign;
            text.color = pColor;
            text.raycastTarget = false;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            Shadow shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(0.6f, -0.6f);
            return text;
        }

        public static Color ParseHex(string pHex, Color pFallback)
        {
            if (string.IsNullOrEmpty(pHex)) return pFallback;
            string hex = pHex[0] == '#' ? pHex : "#" + pHex;
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : pFallback;
        }
    }
}
