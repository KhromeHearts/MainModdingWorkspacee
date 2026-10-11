using System.Collections.Generic;
using DG.Tweening;
using PossessLib.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>Shows a "tip" tooltip on hover (for HUD icons that have no slot).</summary>
    internal sealed class HoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Title;
        public string Description;
        public System.Func<string> DynamicDescription;

        public void OnPointerEnter(PointerEventData pEventData)
        {
            UIState.HoverCount++;
            string desc = DynamicDescription != null ? DynamicDescription() : Description;
            Tooltip.show(this, "tip", new TooltipData { tip_name = Title, tip_description = desc });
        }

        public void OnPointerExit(PointerEventData pEventData)
        {
            UIState.HoverCount = Mathf.Max(0, UIState.HoverCount - 1);
            Tooltip.hideTooltip();
        }

        private void OnDisable()
        {
            // OnPointerExit is not sent when the object is hidden under the pointer
            UIState.HoverCount = 0;
        }
    }

    /// <summary>Gold health bar (MainCharacterMod's frame) with level, XP bar and armor (defense) readout.</summary>
    internal sealed class HealthPanel
    {
        public RectTransform Root;
        private Image _fill;
        private Image _xpFill;
        private Text _hpText;
        private Text _levelText;
        private Text _armorText;
        private float _shown = 1f;
        private int _lastHp = -1, _lastMax = -1, _lastLevel = -1, _lastArmor = int.MinValue;

        public const float Height = UIConst.HealthHeight + 15f;

        public static HealthPanel Create(Transform pParent)
        {
            HealthPanel p = new HealthPanel();
            p.Root = UIFactory.Rect("HealthPanel", pParent);
            UIFactory.Place(p.Root, new Vector2(1f, 1f), Vector2.zero, new Vector2(UIConst.HealthWidth, Height));

            RectTransform bar = UIFactory.Rect("Bar", p.Root);
            UIFactory.Place(bar, new Vector2(1f, 1f), Vector2.zero, new Vector2(UIConst.HealthWidth, UIConst.HealthHeight));
            // inner opening of GoldHealthBar.png: x 11..76 of 88, y 5..19 of 27 (from the top)
            Vector2 innerMin = new Vector2(11f / 88f, 7f / 27f);
            Vector2 innerMax = new Vector2(77f / 88f, 22f / 27f);
            Image back = UIFactory.Image("Back", bar, UISprites.White, new Color(0.18f, 0.05f, 0.05f, 0.95f));
            back.rectTransform.anchorMin = innerMin;
            back.rectTransform.anchorMax = innerMax;
            back.rectTransform.offsetMin = back.rectTransform.offsetMax = Vector2.zero;
            p._fill = UIFactory.Image("Fill", bar, UISprites.White, new Color(0.86f, 0.16f, 0.16f, 1f));
            p._fill.type = Image.Type.Filled;
            p._fill.fillMethod = Image.FillMethod.Horizontal;
            p._fill.rectTransform.anchorMin = innerMin;
            p._fill.rectTransform.anchorMax = innerMax;
            p._fill.rectTransform.offsetMin = p._fill.rectTransform.offsetMax = Vector2.zero;
            Image shine = UIFactory.Image("Shine", p._fill.transform, UISprites.White, new Color(1f, 1f, 1f, 0.18f));
            shine.rectTransform.anchorMin = new Vector2(0f, 0.6f);
            shine.rectTransform.anchorMax = new Vector2(1f, 0.85f);
            shine.rectTransform.offsetMin = shine.rectTransform.offsetMax = Vector2.zero;

            Image frame = UIFactory.Image("Frame", bar, UISprites.HealthFrame, Color.white, true);
            UIFactory.Stretch(frame.rectTransform);
            frame.gameObject.AddComponent<HoverTip>().Title = Loc.T("health", "Health");

            p._hpText = UIFactory.Text("Value", bar, UIConst.FontLarge, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch((RectTransform)p._hpText.transform, 0f, 1f, 0f, 0f);

            // stats row: [lvl icon][lvl] [xp bar] [armor icon][armor]
            RectTransform row = UIFactory.Rect("StatsRow", p.Root);
            UIFactory.Place(row, new Vector2(1f, 1f), new Vector2(-4f, -UIConst.HealthHeight - 1f), new Vector2(UIConst.HealthWidth - 8f, 12f));

            Image levelIcon = UIFactory.Image("LevelIcon", row, UISprites.IconLevel, Color.white, true);
            UIFactory.Place(levelIcon.rectTransform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(10f, 10f));
            levelIcon.preserveAspect = true;
            HoverTip levelTip = levelIcon.gameObject.AddComponent<HoverTip>();
            levelTip.Title = Loc.T("possesslib_level", "Level");
            p._levelText = UIFactory.Text("Level", row, UIConst.FontNormal, TextAnchor.MiddleLeft, UIConst.Gold);
            UIFactory.Place((RectTransform)p._levelText.transform, new Vector2(0f, 0.5f), new Vector2(11f, 0f), new Vector2(16f, 10f));

            Image xpBack = UIFactory.Image("XpBack", row, UISprites.White, new Color(0f, 0f, 0f, 0.6f), true);
            UIFactory.Place(xpBack.rectTransform, new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(row.sizeDelta.x - 28f - 30f, 4f));
            p._xpFill = UIFactory.Image("XpFill", xpBack.transform, UISprites.White, new Color(0.35f, 0.85f, 1f, 1f));
            p._xpFill.type = Image.Type.Filled;
            p._xpFill.fillMethod = Image.FillMethod.Horizontal;
            UIFactory.Stretch(p._xpFill.rectTransform);
            HoverTip xpTip = xpBack.gameObject.AddComponent<HoverTip>();
            xpTip.Title = Loc.T("possesslib_experience", "Experience");

            Image armorIcon = UIFactory.Image("ArmorIcon", row, UISprites.IconArmor, Color.white, true);
            UIFactory.Place(armorIcon.rectTransform, new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(10f, 10f));
            armorIcon.preserveAspect = true;
            HoverTip armorTip = armorIcon.gameObject.AddComponent<HoverTip>();
            armorTip.Title = Loc.T("armor", "Armor");
            armorTip.Description = Loc.T("possesslib_armor_tip", "Defense from your gear and traits (the unit's armor stat).");
            p._armorText = UIFactory.Text("Armor", row, UIConst.FontNormal, TextAnchor.MiddleRight, Color.white);
            UIFactory.Place((RectTransform)p._armorText.transform, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(17f, 10f));

            xpTip.DynamicDescription = () => p._xpDescription;
            levelTip.DynamicDescription = () => p._xpDescription;
            return p;
        }

        private string _xpDescription = string.Empty;

        public void Tick(Actor pActor)
        {
            if (pActor == null) return;
            int hp = pActor.getHealth();
            int max = Mathf.Max(1, pActor.getMaxHealth());
            float target = Mathf.Clamp01((float)hp / max);
            _shown = Mathf.MoveTowards(_shown, target, Time.unscaledDeltaTime * 1.5f);
            _fill.fillAmount = _shown;
            if (hp != _lastHp || max != _lastMax)
            {
                _lastHp = hp;
                _lastMax = max;
                _hpText.text = hp + "/" + max;
            }

            int level = pActor.level;
            int xpNeed = Mathf.Max(1, pActor.getExpToLevelup());
            int xp = pActor.data.experience;
            _xpFill.fillAmount = Mathf.Clamp01((float)xp / xpNeed);
            if (level != _lastLevel)
            {
                _lastLevel = level;
                _levelText.text = level.ToString();
            }
            int armor = Mathf.RoundToInt(pActor.stats["armor"]);
            if (armor != _lastArmor)
            {
                _lastArmor = armor;
                _armorText.text = armor.ToString();
            }
            if (xp != _lastXp || xpNeed != _lastXpNeed)
            {
                _lastXp = xp;
                _lastXpNeed = xpNeed;
                _xpDescription = xp + " / " + xpNeed;
            }
        }

        private int _lastXp = -1;
        private int _lastXpNeed = -1;

        public void ResetSmoothing()
        {
            _shown = 1f;
            _lastHp = _lastMax = _lastLevel = -1;
            _lastArmor = int.MinValue;
        }
    }

    /// <summary>The possessed unit's status effects (Terraria's buff row), with the game's status tooltips.</summary>
    internal sealed class StatusRow
    {
        private const int Max = 12;
        public RectTransform Root;
        private readonly Image[] _icons = new Image[Max];
        private readonly StatusHover[] _hovers = new StatusHover[Max];
        private float _next;

        public static StatusRow Create(Transform pParent)
        {
            StatusRow row = new StatusRow();
            row.Root = UIFactory.Rect("StatusRow", pParent);
            UIFactory.Place(row.Root, new Vector2(0f, 1f), Vector2.zero, new Vector2(Max * (UIConst.StatusIcon + 2f), UIConst.StatusIcon));
            for (int i = 0; i < Max; i++)
            {
                Image img = UIFactory.Image("Status_" + i, row.Root, null, Color.white, true);
                UIFactory.Place(img.rectTransform, new Vector2(0f, 1f), new Vector2(i * (UIConst.StatusIcon + 2f), 0f),
                    new Vector2(UIConst.StatusIcon, UIConst.StatusIcon));
                img.preserveAspect = true;
                img.enabled = false;
                row._icons[i] = img;
                row._hovers[i] = img.gameObject.AddComponent<StatusHover>();
            }
            return row;
        }

        public void Tick(Actor pActor, bool pForce)
        {
            if (!pForce && Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 0.25f;
            int i = 0;
            if (pActor != null && pActor.isAlive())
            {
                foreach (Status status in pActor.getStatuses())
                {
                    if (i >= Max) break;
                    if (status == null || status.is_finished || status.asset == null) continue;
                    Sprite sprite = status.asset.getSprite();
                    if (sprite == null) continue;
                    _icons[i].sprite = sprite;
                    _icons[i].enabled = true;
                    _hovers[i].Status = status;
                    i++;
                }
            }
            for (; i < Max; i++)
            {
                _icons[i].enabled = false;
                _hovers[i].Status = null;
            }
        }
    }

    internal sealed class StatusHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Status Status;

        public void OnPointerEnter(PointerEventData pEventData)
        {
            UIState.HoverCount++;
            if (Status == null || Status.asset == null) return;
            Tooltip.show(this, "status_updatable", new TooltipData
            {
                tip_name = Status.asset.getLocaleID(),
                tip_description = Status.asset.getDescriptionID(),
                status = Status
            });
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

    /// <summary>Short messages at the top of the screen, fed by <see cref="Toasts"/>.</summary>
    internal sealed class ToastView
    {
        private const int Lines = 4;
        private const float LifeTime = 2.6f;
        public RectTransform Root;
        private readonly Text[] _texts = new Text[Lines];
        private readonly CanvasGroup[] _groups = new CanvasGroup[Lines];
        private readonly float[] _until = new float[Lines];

        public static ToastView Create(Transform pParent)
        {
            ToastView view = new ToastView();
            view.Root = UIFactory.Rect("Toasts", pParent);
            UIFactory.Place(view.Root, new Vector2(0.5f, 1f), new Vector2(0f, -UIConst.Margin - 40f), new Vector2(240f, Lines * 11f));
            for (int i = 0; i < Lines; i++)
            {
                Text t = UIFactory.Text("Toast_" + i, view.Root, UIConst.FontLarge, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Place((RectTransform)t.transform, new Vector2(0.5f, 1f), new Vector2(0f, -i * 11f), new Vector2(240f, 11f));
                t.text = string.Empty;
                view._texts[i] = t;
                // fade with a CanvasGroup: Text.color alpha does not apply to rich-text <color> segments
                CanvasGroup group = t.gameObject.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false;
                group.interactable = false;
                group.alpha = 0f;
                view._groups[i] = group;
            }
            return view;
        }

        public void Tick()
        {
            float now = Time.unscaledTime;
            while (Toasts.TryDequeue(out string message))
            {
                // shift lines down, newest on top
                for (int i = Lines - 1; i > 0; i--)
                {
                    _texts[i].text = _texts[i - 1].text;
                    _until[i] = _until[i - 1];
                }
                _texts[0].text = message;
                _until[0] = now + LifeTime;
            }
            for (int i = 0; i < Lines; i++)
            {
                float left = _until[i] - now;
                float a = left <= 0f ? 0f : Mathf.Clamp01(left / 0.6f);
                if (!Mathf.Approximately(_groups[i].alpha, a)) _groups[i].alpha = a;
            }
        }
    }

    /// <summary>"LEVEL UP!" popup with a punch animation (DOTween, which the game ships).</summary>
    internal sealed class LevelUpPopup
    {
        public RectTransform Root;
        private Text _text;
        private CanvasGroup _group;

        public static LevelUpPopup Create(Transform pParent)
        {
            LevelUpPopup p = new LevelUpPopup();
            p.Root = UIFactory.Rect("LevelUp", pParent);
            UIFactory.Place(p.Root, new Vector2(0.5f, 1f), new Vector2(0f, -UIConst.Margin - 90f), new Vector2(200f, 24f));
            p._group = p.Root.gameObject.AddComponent<CanvasGroup>();
            p._group.alpha = 0f;
            p._group.blocksRaycasts = false;
            p._group.interactable = false;
            p._text = UIFactory.Text("Text", p.Root, 16, TextAnchor.MiddleCenter, UIConst.Gold);
            UIFactory.Stretch((RectTransform)p._text.transform);
            Outline outline = p._text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.18f, 0f, 1f);
            outline.effectDistance = new Vector2(1f, -1f);
            return p;
        }

        public void Show(int pLevel)
        {
            _text.text = Loc.T("possesslib_level_up", "LEVEL UP!") + "  " + pLevel;
            Root.DOKill();
            _group.DOKill();
            Root.localScale = Vector3.one * 0.6f;
            _group.alpha = 1f;
            Root.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
            _group.DOFade(0f, 0.8f).SetDelay(2.2f).SetUpdate(true);
        }
    }
}
