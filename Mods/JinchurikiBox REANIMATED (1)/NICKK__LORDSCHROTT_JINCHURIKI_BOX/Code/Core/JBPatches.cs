using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Linq;
using HarmonyLib;
using JinchurikiBox.Actions;
using JinchurikiBox.Core;
using JinchurikiBox.Content;

using NCMS.Utils;
using ai;
using UnityEngine.UI;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Concurrent;


namespace JinchurikiBox.Patches;
public static class JBPatches
{
    private static ConcurrentDictionary<string, string> _stringData = new ConcurrentDictionary<string, string>();
    private static HashSet<string> _assignedReincarnates = new HashSet<string>();
    private static FieldInfo _remainingField;
    private static bool _chakraTooltipRegistered = false;
    private static readonly object _statusLock = new();
    static JBPatches()
    {
    }

    private static void ShowChakraTooltip(Tooltip tooltip, string type, TooltipData data)
    {
        Actor a = data.actor;
        if (a?.stats == null || a.data == null) return;

        float max = a.stats["chakra"];
        if (max <= 0f) return;
        a.data.get("chakra", out float cur);

        tooltip.addLineText("Chakra", cur.ToText() + "/" + max.ToText(), pLocalize: false);

        float tbMax = a.stats["tailed_beast_chakra"];
        if (tbMax > 0f)
        {
            a.data.get("tailed_beast_chakra", out float tbCur);
            tooltip.addLineText("TB Chakra", tbCur.ToText() + "/" + tbMax.ToText(), pLocalize: false);
        }

        float sgMax = a.stats["sage_chakra"];
        if (sgMax > 0f)
        {
            a.data.get("sage_chakra", out float sgCur);
            tooltip.addLineText("Nature Energy", sgCur.ToText() + "/" + sgMax.ToText(), pLocalize: false);
        }
    }
    private static HashSet<string> _permanentStatuses = new HashSet<string>
    {
        "StolenByakup",
        "StolenRinnegan",
        "3 Tomoe-S eye", "2 Tomoe-S eye", "1 Tomoe-S eye",
        "Uchiha's MS eye", "Madara's eye", "Indra's eye", "Itachi's eye"
    };
    public static string GetStringData(long actorId, string key) {
        _stringData.TryGetValue($"{actorId}_{key}", out string val);
        return val;
    }
    public static void SetStringData(long actorId, string key, string val) {
        _stringData[$"{actorId}_{key}"] = val;
    }

    private static readonly ConditionalWeakTable<SelectedUnitTab, StatBar> _selectedUnitTabBars = new();

    private static void ApplyChakraColor(StatBar statBar)
    {
        if (statBar?.bar == null) return;
        Image fillImage = statBar.bar.GetComponent<Image>();
        if (fillImage != null)
            fillImage.color = new Color(0f, 0.40f, 1.00f);
    }

    private static void ApplySageChakraColor(StatBar statBar)
    {
        if (statBar?.bar == null) return;
        Image fillImage = statBar.bar.GetComponent<Image>();
        if (fillImage != null)
            fillImage.color = new Color(0.1f, 0.45f, 0.05f);
    }

    private static (float current, float max) GetChakraValues(Actor actor)
    {
        if (actor?.data == null || actor.stats == null)
            return (0f, 0f);
        actor.data.get("chakra", out float currentChakra);
        return (currentChakra, actor.stats["chakra"]);
    }

    private static (float current, float max) GetSageChakraValues(Actor actor)
    {
        if (actor?.data == null || actor.stats == null)
            return (0f, 0f);
        actor.data.get("sage_chakra", out float current);
        return (current, actor.stats["sage_chakra"]);
    }

    [HarmonyPatch(typeof(SelectedUnitTab), "showStatBars")]
    [HarmonyPostfix]
    public static void ShowStatBars_Postfix(SelectedUnitTab __instance, Actor pActor)
    {
        if (__instance == null || pActor?.data == null || pActor.stats == null) return;

        StatBar chakraBar = _selectedUnitTabBars.GetValue(__instance, tab =>
        {
            if (tab._bar_health == null) return null;
            Transform parent = ((UnityEngine.Component)tab._bar_health).transform.parent;
            if (parent == null) return null;

            Transform existing = parent.Find("ChakraBar");
            if (existing != null) GameObject.Destroy(existing.gameObject);

            GameObject clone = GameObject.Instantiate(tab._bar_health.gameObject, parent);
            if (clone == null) return null;

            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;
            clone.name = "ChakraBar";

            StatBar newBar = clone.GetComponent<StatBar>();
            if (newBar != null) ApplyChakraColor(newBar);
            foreach (Text txt in clone.GetComponentsInChildren<Text>(true))
                if (txt != newBar?.textField)
                    txt.text = "Chakra";
            return newBar;
        });
        if (chakraBar == null) return;

        (float current, float max) = GetChakraValues(pActor);
        bool show = max > 0;
        chakraBar.gameObject.SetActive(show);
        if (show)
            chakraBar.setBar(current, max, "/" + max.ToText(), pReset: false, pFloat: false, pUpdateText: true, pSpeed: 0.25f);

        Transform iconTf = chakraBar.transform.Find("Icon");
        if (iconTf != null)
        {
            Image iconImg = iconTf.GetComponent<Image>();
            if (iconImg != null)
                iconImg.sprite = SpriteTextureLoader.getSprite("ui/icons/ChakraNum");
        }
    }

    private static readonly ConditionalWeakTable<UnitHealthBarElement, StatBar> _healthBarElementBars = new();

    [HarmonyPatch(typeof(UnitHealthBarElement), "showContent")]
    [HarmonyPostfix]
    public static void ShowContent_Postfix(UnitHealthBarElement __instance)
    {
        try
        {
            if (!_chakraTooltipRegistered)
            {
                _chakraTooltipRegistered = true;
                string[] tipIds = { "actor", "actor_king", "actor_leader" };
                foreach (string id in tipIds)
                {
                    TooltipAsset ta = AssetManager.tooltips.get(id);
                    if (ta != null)
                        ta.callback += ShowChakraTooltip;
                }
            }
        Actor pActor = __instance?.actor;
        if (pActor?.data == null || pActor.stats == null) return;

        StatBar chakraBar = _healthBarElementBars.GetValue(__instance, elem =>
        {
            if (elem._health == null) return null;
            Transform fixedParent = ((UnityEngine.Component)elem._health).transform.parent;
            while (fixedParent != null && !fixedParent.TryGetComponent<ScrollRect>(out _))
                fixedParent = fixedParent.parent;
            if (fixedParent != null) fixedParent = fixedParent.parent;
            if (fixedParent == null) fixedParent = ((UnityEngine.Component)elem).transform.root;

            Transform existing = fixedParent.Find("ChakraBar");
            if (existing != null) GameObject.Destroy(existing.gameObject);

            GameObject clone = GameObject.Instantiate(elem._health.gameObject, fixedParent);
            if (clone == null) return null;

            clone.transform.SetAsLastSibling();
            clone.transform.localPosition = new Vector3(200, -5, 0);
            clone.transform.localScale = Vector3.one;
            clone.name = "ChakraBar";

            RectTransform rt = clone.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 18);

            GameObject labelGO = new GameObject("ChakraLabel", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(clone.transform, false);
            RectTransform lrt = labelGO.GetComponent<RectTransform>();
            lrt.localPosition = new Vector3(0, 18, 0);
            lrt.sizeDelta = new Vector2(130, 18);
            Text labelText = labelGO.GetComponent<Text>();
            labelText.text = "Chakra Reserve";
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 9;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.supportRichText = false;

            foreach (MaskableGraphic mg in clone.GetComponentsInChildren<MaskableGraphic>(true))
                mg.maskable = false;

            StatBar newBar = clone.GetComponent<StatBar>();
            if (newBar != null) ApplyChakraColor(newBar);
            foreach (Text txt in clone.GetComponentsInChildren<Text>(true))
                if (txt != newBar?.textField && txt.gameObject.name != "ChakraLabel")
                    txt.text = "Chakra";
            labelText.text = "Chakra Pool";
            return newBar;
        });
        if (chakraBar == null) return;

        (float current, float max) = GetChakraValues(pActor);
        bool show = max > 0;
        chakraBar.gameObject.SetActive(show);
        if (show)
        {
            chakraBar.setBar(current, max, "/" + max.ToText(), pReset: false, pFloat: false, pUpdateText: true, pSpeed: 0.25f);
            if (chakraBar.textField != null)
                chakraBar.textField.text = current.ToText() + "/" + max.ToText();

            foreach (Image img in chakraBar.GetComponentsInChildren<Image>(true))
                img.raycastTarget = true;
            Button btn = chakraBar.GetComponent<Button>() ?? chakraBar.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            TipButton tipButton = chakraBar.GetComponent<TipButton>() ?? chakraBar.gameObject.AddComponent<TipButton>();
            tipButton.textOnClick = "";
            tipButton.textOnClickDescription = "";
            tipButton.type = "tip";
            tipButton.setHoverAction(new TooltipAction(tipButton.showTooltipDefault), true);
        }

        Transform iconTf = chakraBar.transform.Find("Icon");
        if (iconTf != null)
        {
            Image iconImg = iconTf.GetComponent<Image>();
            if (iconImg != null)
                iconImg.sprite = SpriteTextureLoader.getSprite("ui/icons/ChakraNum");
        }

        Transform hoverParent = ((UnityEngine.Component)chakraBar).transform.parent;
        if (hoverParent != null)
        {
            Transform existing = hoverParent.Find("TailedBeastChakraBar");
            StatBar tbBar = existing != null ? existing.GetComponent<StatBar>() : null;
            if (tbBar == null)
            {
                GameObject clone = GameObject.Instantiate(chakraBar.gameObject, hoverParent);
                if (clone != null)
                {
                    clone.transform.SetAsLastSibling();
                    clone.name = "TailedBeastChakraBar";
                    tbBar = clone.GetComponent<StatBar>();
                    if (tbBar != null)
                    {
                        if (tbBar.bar != null)
                        {
                            Image fillImage = tbBar.bar.GetComponent<Image>();
                            if (fillImage != null)
                                fillImage.color = new Color(0.6f, 0f, 0f);
                        }
                        RectTransform chakraRt = chakraBar.GetComponent<RectTransform>();
                        RectTransform tbRt = tbBar.GetComponent<RectTransform>();
                        Vector3 chakraPos = chakraRt.localPosition;
                        tbRt.localPosition = new Vector3(chakraPos.x, chakraPos.y - tbRt.rect.height - 50f, chakraPos.z);
                        tbRt.localScale = Vector3.one;
                        tbRt.sizeDelta = chakraRt.sizeDelta;

                        Transform labelTf = clone.transform.Find("ChakraLabel");
                        Text labelText;
                        if (labelTf != null)
                        {
                            labelText = labelTf.GetComponent<Text>();
                        }
                        else
                        {
                            GameObject labelGO = new GameObject("ChakraLabel", typeof(RectTransform), typeof(Text));
                            labelGO.transform.SetParent(clone.transform, false);
                            RectTransform lrt = labelGO.GetComponent<RectTransform>();
                            lrt.localPosition = new Vector3(0, 18f, 0);
                            lrt.sizeDelta = new Vector2(130f, 18f);
                            labelText = labelGO.GetComponent<Text>();
                            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                            labelText.fontSize = 9;
                            labelText.alignment = TextAnchor.MiddleCenter;
                            labelText.color = Color.white;
                            labelText.supportRichText = false;
                        }
                        if (labelText != null)
                            labelText.text = "Tailed Beasts Chakra";

                        Transform tbIconTf = clone.transform.Find("Icon");
                        if (tbIconTf != null)
                        {
                            Image iconImg = tbIconTf.GetComponent<Image>();
                            if (iconImg != null)
                                iconImg.sprite = SpriteTextureLoader.getSprite("ui/icons/Seal");
                        }
                    }
                }
            }
            if (tbBar != null)
            {
                float tbMax = pActor.stats["tailed_beast_chakra"];
                pActor.data.get("tailed_beast_chakra", out float tbCur);
                bool showTB = tbMax > 0f;
                tbBar.gameObject.SetActive(showTB);
                if (showTB)
                {
                    tbBar.setBar(tbCur, tbMax, "/" + tbMax.ToText(), pReset: false, pFloat: false, pUpdateText: true, pSpeed: 0.25f);
                    if (tbBar.textField != null)
                        tbBar.textField.text = tbCur.ToText() + "/" + tbMax.ToText();

                    Button tbBtn = tbBar.GetComponent<Button>() ?? tbBar.gameObject.AddComponent<Button>();
                    tbBtn.transition = Selectable.Transition.None;
                    TipButton tbTip = tbBar.GetComponent<TipButton>() ?? tbBar.gameObject.AddComponent<TipButton>();
                    tbTip.textOnClick = "";
                    tbTip.textOnClickDescription = "";
                    tbTip.type = "tip";
                    tbTip.setHoverAction(new TooltipAction(tbTip.showTooltipDefault), true);
                    foreach (Image img in tbBar.GetComponentsInChildren<Image>(true))
                        img.raycastTarget = true;
                }
            }

            existing = hoverParent.Find("NatureEnergyBar");
            StatBar sageBar = existing != null ? existing.GetComponent<StatBar>() : null;
            if (sageBar == null)
            {
                GameObject clone = GameObject.Instantiate(chakraBar.gameObject, hoverParent);
                if (clone != null)
                {
                    clone.transform.SetAsLastSibling();
                    clone.name = "NatureEnergyBar";
                    sageBar = clone.GetComponent<StatBar>();
                    if (sageBar != null)
                    {
                        if (sageBar.bar != null)
                        {
                            Image fillImage = sageBar.bar.GetComponent<Image>();
                            if (fillImage != null)
                                fillImage.color = new Color(0.1f, 0.45f, 0.05f);
                        }
                        RectTransform chakraRt = chakraBar.GetComponent<RectTransform>();
                        RectTransform sageRt = sageBar.GetComponent<RectTransform>();
                        Vector3 chakraPos = chakraRt.localPosition;
                        sageRt.localPosition = new Vector3(chakraPos.x, chakraPos.y + sageRt.rect.height + 50f, chakraPos.z);
                        sageRt.localScale = Vector3.one;
                        sageRt.sizeDelta = chakraRt.sizeDelta;

                        Transform labelTf = clone.transform.Find("ChakraLabel");
                        Text labelText;
                        if (labelTf != null)
                        {
                            labelText = labelTf.GetComponent<Text>();
                        }
                        else
                        {
                            GameObject labelGO = new GameObject("ChakraLabel", typeof(RectTransform), typeof(Text));
                            labelGO.transform.SetParent(clone.transform, false);
                            RectTransform lrt = labelGO.GetComponent<RectTransform>();
                            lrt.localPosition = new Vector3(0, 18f, 0);
                            lrt.sizeDelta = new Vector2(130f, 18f);
                            labelText = labelGO.GetComponent<Text>();
                            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                            labelText.fontSize = 9;
                            labelText.alignment = TextAnchor.MiddleCenter;
                            labelText.color = Color.white;
                            labelText.supportRichText = false;
                        }
                        if (labelText != null)
                            labelText.text = "Nature energy";

                        Transform sageIconTf = clone.transform.Find("Icon");
                        if (sageIconTf != null)
                        {
                            Image iconImg = sageIconTf.GetComponent<Image>();
                            if (iconImg != null)
                                iconImg.sprite = SpriteTextureLoader.getSprite("ui/icons/Toad");
                        }
                    }
                }
            }
            if (sageBar != null)
            {
                float sageMax = pActor.stats["sage_chakra"];
                pActor.data.get("sage_chakra", out float sageCur);
                bool sageShow = sageMax > 0f;
                sageBar.gameObject.SetActive(sageShow);
                if (sageShow)
                {
                    sageBar.setBar(sageCur, sageMax, "/" + sageMax.ToText(), pReset: false, pFloat: false, pUpdateText: true, pSpeed: 0.25f);
                    if (sageBar.textField != null)
                        sageBar.textField.text = sageCur.ToText() + "/" + sageMax.ToText();

                    Button sageBtn = sageBar.GetComponent<Button>() ?? sageBar.gameObject.AddComponent<Button>();
                    sageBtn.transition = Selectable.Transition.None;
                    TipButton sageTip = sageBar.GetComponent<TipButton>() ?? sageBar.gameObject.AddComponent<TipButton>();
                    sageTip.textOnClick = "";
                    sageTip.textOnClickDescription = "";
                    sageTip.type = "tip";
                    sageTip.setHoverAction(new TooltipAction(sageTip.showTooltipDefault), true);
                    foreach (Image img in sageBar.GetComponentsInChildren<Image>(true))
                        img.raycastTarget = true;
                }
            }
        }

        float chMax = pActor.stats["chakra"];
        pActor.data.get("chakra", out float chCur);
        float _tbMax = pActor.stats["tailed_beast_chakra"];
        pActor.data.get("tailed_beast_chakra", out float _tbCur);
        float _sgMax = pActor.stats["sage_chakra"];
        pActor.data.get("sage_chakra", out float _sgCur);

        Text[] allTexts = __instance.GetComponentsInChildren<Text>(true);
        Text descText = null;
        foreach (Text t in allTexts)
        {
            string txt = t.text;
            if (txt != null && (txt.Contains("Clan:") || txt.Contains("Kingdom:") || txt.Contains("Attack:")))
            {
                descText = t;
                break;
            }
        }
        if (descText == null)
        {
            foreach (Text t in allTexts)
            {
                string txt = t.text;
                if (txt != null && t.fontSize <= 11 && txt.Length > 20)
                {
                    descText = t;
                    break;
                }
            }
        }
        if (descText != null)
        {
            if (chMax > 0f)
                descText.text += "\nChakra: " + chCur.ToText() + "/" + chMax.ToText();
            if (_tbMax > 0f)
                descText.text += "\nTB Chakra: " + _tbCur.ToText() + "/" + _tbMax.ToText();
            if (_sgMax > 0f)
                descText.text += "\nNature: " + _sgCur.ToText() + "/" + _sgMax.ToText();
        }
        }
        catch (Exception e) { Debug.LogError("Jinchuriki ShowContent error: " + e); }
    }

    [HarmonyPatch(typeof(UnitElement), "OnDisable")]
    [HarmonyPostfix]
    public static void UnitElement_OnDisable_Postfix(UnitElement __instance)
    {
        if (__instance is UnitHealthBarElement healthElem && _healthBarElementBars.TryGetValue(healthElem, out StatBar bar) && bar != null)
        {
            Transform t = bar.transform;
            if (t != null)
            {
                Transform p = t.parent;
                if (p != null)
                {
                    Transform tbTf = p.Find("TailedBeastChakraBar");
                    if (tbTf != null) GameObject.Destroy(tbTf.gameObject);
                    Transform sageTf = p.Find("NatureEnergyBar");
                    if (sageTf != null) GameObject.Destroy(sageTf.gameObject);
                }
                GameObject.Destroy(t.gameObject);
            }
            _healthBarElementBars.Remove(healthElem);
        }
    }

    [HarmonyPatch(typeof(Actor), "setFlip")]
    [HarmonyPostfix]
    public static void setFlip(bool pFlip, Actor __instance) {
        if (__instance._active_status_dict != null) {
            foreach (Status effect in __instance._active_status_dict.Values)
            {
                if (effect.asset.id == "madarasusanoo1" || effect.asset.id == "Chidori Attack") {
                    effect.flip_x = pFlip;
                }
            }
        }
    }


    [HarmonyPatch(typeof(StatsWindow), "showStatsRows")]
    [HarmonyPostfix]
    public static void OnEnable_Postfix(StatsWindow __instance) {
        if (!(__instance is UnitWindow unitWindow)) return;
        Actor actor = unitWindow.actor;
        if (actor == null) return;

        float chakra = actor.data["chakra"];
        __instance.showStatRow("jb_chakra", chakra + "/" + actor.stats["chakra"], __instance.meta_type, 0, null, null, null);

        int level_amaterasu = (int)actor.data["level_amaterasu"];
        if (level_amaterasu != 0) {
            __instance.showStatRow("level_amaterasu", level_amaterasu, __instance.meta_type, 0, null, null, null);
        }

        int level_shadow_clone = (int)actor.data["level_shadow_clone"];
        if (level_shadow_clone != 0) {
            __instance.showStatRow("level_shadow_clone", level_shadow_clone, __instance.meta_type, 0, null, null, null);
        }

        int level_sharingan_genjutsu = (int)actor.data["level_sharingan_genjutsu"];
        if (level_sharingan_genjutsu != 0) {
            __instance.showStatRow("level_sharingan_Genjutsu", level_sharingan_genjutsu, __instance.meta_type, 0, null, null, null);
        }

        int level_gates = (int)actor.data["level_gates"];
        if (level_gates != 0) {
            __instance.showStatRow("level_gates", level_gates, __instance.meta_type, 0, null, null, null);
        }

        int level_multis = (int)actor.data["level_multis"];
        if (level_multis != 0) {
            __instance.showStatRow("level_multis", level_multis, __instance.meta_type, 0, null, null, null);
        }

        int level_chidori = (int)actor.data["level_chidori"];
        if (level_chidori != 0) {
            __instance.showStatRow("level_chidori", level_chidori, __instance.meta_type, 0, null, null, null);
        }
        int level_susanoorib = (int)actor.data["level_susanoorib"];
        if (level_susanoorib != 0) {
            __instance.showStatRow("level_susanoorib", level_susanoorib, __instance.meta_type, 0, null, null, null);
        }
        int level_rasengan = (int)actor.data["level_rasengan"];
        if (level_rasengan != 0) {
            __instance.showStatRow("level_rasengan", level_rasengan, __instance.meta_type, 0, null, null, null);
        }

    }
    
    [HarmonyPatch(typeof(BabyHelper), "traitsInherit")]
    [HarmonyPostfix]
    public static void traitsInherit_Postfix(Actor pActorTarget, Actor pParent1, Actor pParent2) {
        if (pActorTarget.isSapient())
        {
            string[] clanTraitIds = { "akimichiClan", "hyugaClan", "uzumakiClan", "uchihaClan", "senjuClan" };
            foreach (var clanId in clanTraitIds)
            {
                if (!pActorTarget.hasTrait(clanId))
                {
                    bool parentHasIt = (pParent1 != null && pParent1.hasTrait(clanId)) || (pParent2 != null && pParent2.hasTrait(clanId));
                    if (parentHasIt)
                    {
                        ActorTrait trait = AssetManager.traits.get(clanId);
                        if (trait != null && Randy.randomChance((float)trait.rate_inherit / 100f))
                            pActorTarget.addTrait(clanId);
                    }
                }
            }
            bool p1Otsu = pParent1 != null && pParent1.hasTrait("otsutsukiClan");
            bool p2Otsu = pParent2 != null && pParent2.hasTrait("otsutsukiClan");
            if (p1Otsu && p2Otsu)
            {
                if (!pActorTarget.hasTrait("otsutsukiClan"))
                    pActorTarget.addTrait("otsutsukiClan");
            }
            else if (p1Otsu || p2Otsu)
            {
                if (pActorTarget.hasTrait("otsutsukiClan"))
                    pActorTarget.removeTrait("otsutsukiClan");
                if (Randy.randomChance(0.90f))
                {
                    string[] dnaTraits = { "otsutsuki20", "otsutsuki40", "otsutsuki60", "otsutsuki80" };
                    pActorTarget.addTrait(GetRandomElement(dnaTraits));
                }
                else
                {
                    pActorTarget.addTrait("otsutsukiClan");
                }
            }
        }

        if (Randy.randomChance(0.1f) && pActorTarget.isSapient()) {
            if (pActorTarget.hasTrait("senjuClan")) {
                pActorTarget.addTrait("woodRelease");
            }
        }

        if (Randy.randomChance(0.05f)) {
            if (pActorTarget.hasTrait("uchihaClan") && pActorTarget.isSapient()) {
                pActorTarget.addTrait("madaraSpirit");
            }
        }

        if (Randy.randomChance(0.0f) && pActorTarget.isSapient()) {
            if (pActorTarget.hasTrait("uzumakiClan")) {
                pActorTarget.addTrait("adamantineSealingChains");
                pActorTarget.addTrait("yangRelease");
            }
        }
    
        if (pActorTarget.hasTrait("hyugaClan") && pActorTarget.isSapient()) {
            if (!pActorTarget.hasTrait("indraChakra") && !pActorTarget.hasTrait("hamuraChakra") && !pActorTarget.hasTrait("asuraChakra") && !pActorTarget.hasTrait("hagoromoChakra"))
                pActorTarget.addTrait("sealedfate");
        }

        if (pActorTarget.isSapient())
        {
            pActorTarget.AssignBirthTraits();
            pActorTarget.AssignChakraTrait();

            if (pActorTarget.hasTrait("prodigy"))
                SendWorldLog(pActorTarget, "jb_log_prodigy_born");

            if (pActorTarget.hasTrait("SLegend"))
            {
                SendWorldLog(pActorTarget, "jb_log_legend_born");
                if (WorldTip.instance != null)
                    WorldTip.instance.show("<color=orange>Legendary Shinobi spawn!</color>", false, "top", 3f);
                try { pActorTarget.data.favorite = true; } catch { }
            }

            if (pActorTarget.hasTrait("immeasurableReserves"))
            {
                SendWorldLog(pActorTarget, "jb_log_immeasurable_born");
                if (WorldTip.instance != null)
                    WorldTip.instance.show("<color=#00BFFF>A vast Chakra presence has been Detected!</color>", false, "top", 3f);
                try { pActorTarget.data.favorite = true; } catch { }
            }

            if (pActorTarget.hasTrait("chakraC5"))
                SendWorldLog(pActorTarget, "jb_log_chakraC5_born");

            if (pActorTarget.hasTrait("rinnegan"))
                SendWorldLog(pActorTarget, "jb_log_rinnegan_inherited");

            if (pActorTarget.hasTrait("uchihaClan"))
            {
                if (!pActorTarget.hasTrait("itachiReincarnate") && !ReincarnateExists("itachiReincarnate") && Randy.randomChance(0.01f))
                {
                    pActorTarget.addTrait("itachiReincarnate");
                    _assignedReincarnates.Add("itachiReincarnate");
                    SendWorldLog(pActorTarget, "jb_log_itachi_reincarnated");
                    if (WorldTip.instance != null)
                        WorldTip.instance.show("<color=#FF8C00>" + pActorTarget.getName() + " Has been reincarnated!</color>", false, "top", 3f);
                    try { pActorTarget.data.favorite = true; } catch { }
                    if (!pActorTarget.hasTrait("prodigy")) pActorTarget.addTrait("prodigy");
                    pActorTarget.data.name = "Itachi Uchiha";
                }
                if (!pActorTarget.hasTrait("indrasReincarnate") && !ReincarnateExists("indrasReincarnate") && Randy.randomChance(0.01f))
                {
                    pActorTarget.addTrait("indrasReincarnate");
                    _assignedReincarnates.Add("indrasReincarnate");
                    SendWorldLog(pActorTarget, "jb_log_indra_reincarnated");
                    if (WorldTip.instance != null)
                        WorldTip.instance.show("<color=purple>" + pActorTarget.getName() + " Has been reincarnated!</color>", false, "top", 3f);
                    try { pActorTarget.data.favorite = true; } catch { }
                    if (Randy.randomChance(0.20f) && !pActorTarget.hasTrait("otsutsuki80")) pActorTarget.addTrait("otsutsuki80");
                    if (!pActorTarget.hasTrait("indraChakra")) pActorTarget.addTrait("indraChakra");
                    pActorTarget.data.name = "Indra Otsutsuki";
                }
                if (!pActorTarget.hasTrait("madarasReincarnate") && !ReincarnateExists("madarasReincarnate") && Randy.randomChance(0.01f))
                {
                    pActorTarget.addTrait("madarasReincarnate");
                    _assignedReincarnates.Add("madarasReincarnate");
                    SendWorldLog(pActorTarget, "jb_log_madara_reincarnated");
                    if (WorldTip.instance != null)
                        WorldTip.instance.show("<color=red>" + pActorTarget.getName() + " Has been reincarnated!</color>", false, "top", 3f);
                    try { pActorTarget.data.favorite = true; } catch { }
                    if (!pActorTarget.hasTrait("SLegend") && Randy.randomChance(0.30f)) pActorTarget.addTrait("SLegend");
                    pActorTarget.data.name = "Madara Uchiha";
                }
            }

            try { pActorTarget.data.set("birth_traits_assigned", 1); } catch { }
        }
    }

        public static T GetRandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("Array is null or empty");
            }

            int randomIndex = UnityEngine.Random.Range(0, array.Length);
            return array[randomIndex];
        }

    [HarmonyPatch(typeof(Actor), "addForce")]
    [HarmonyPrefix]
    private static bool addForce_Prefix(Actor __instance)
    {
        if (__instance != null && __instance.hasStatus("Palmtech2"))
            return false;
        return true;
    }

public static Dictionary<string, AttackAction> attackActions = new Dictionary<string, AttackAction> {

    { "MSharingan", JinchurikiActions.SharinganGen },

    { "Active Rasengan", JinchurikiActions.RasenganAttack },

    { "Chidori Attack", JinchurikiActions.ChidoriAttack },

    { "Sharingan3", JinchurikiActions.SharinganGen },

    { "3 Tomoe-S eye", JinchurikiActions.SharinganGen },

    { "IndraMS", JinchurikiActions.MSHit },

    { "madaraMS", JinchurikiActions.MSHit },

    { "ItachiMS", JinchurikiActions.MSHit },

    { "Itachi's eye", JinchurikiActions.MSHit },

    { "Uchiha's MS eye", JinchurikiActions.SharinganGen },

    { "Madara's eye", JinchurikiActions.MSHit },

    { "Indra's eye", JinchurikiActions.MSHit },

    { "ChainedAttacker", JinchurikiActions.ChainsAttack },

    { "ByakuG", JinchurikiActions.BHit },

    { "StolenByakup", JinchurikiActions.BHit },

    { "ByakuP", JinchurikiActions.BHit },

    { "Palmtech1", JinchurikiActions.Morestuff1 },
};

    public static void AddCMSHitAction(Actor actor)
    {
        if (actor != null && actor.data != null)
        {
            string cmsKey = "CMS_" + actor.data.id;
            attackActions[cmsKey] = (AttackAction)Delegate.Combine((AttackAction)JinchurikiActions.MSHit, (AttackAction)JinchurikiActions.SharinganGen);
        }
    }

    [HarmonyPatch(typeof(Actor), "updateStats")]
    [HarmonyPostfix]
    public static void updateStats(ref Actor __instance) {
        if (__instance.hasStatus("shadowClone")) {
            __instance.stats["health"] = 1;
        }

        if (__instance.hasStatus("Palmtech2")) {
            __instance.data.set("_j_health_snapshot", __instance.getHealth());
            __instance.finishStatusEffect("stunned");
            __instance.stats["mass"] = 99999f;
            __instance.data.get("_j_rt_anchor", out float anchored);
            if (anchored < 0.5f) {
                Vector3 p = __instance.current_tile.posV3;
                __instance.data.set("_j_rt_anchor_x", p.x);
                __instance.data.set("_j_rt_anchor_y", p.y);
                __instance.data.set("_j_rt_anchor", 1f);
            }
            __instance.current_position = new Vector2((float)__instance.data["_j_rt_anchor_x"], (float)__instance.data["_j_rt_anchor_y"]);
        } else {
            __instance.data.set("_j_rt_anchor", 0f);
        }

        if (__instance.hasStatus("MultiSize")) {
            int num = (int)__instance.data["level_multis"];
            if (num > 0) {
                __instance.data.get("_multisize_base_scale", out float baseScale);
                if (baseScale == 0f)
                {
                    baseScale = __instance.stats["scale"];
                    __instance.data.set("_multisize_base_scale", baseScale);
                }
                __instance.stats["scale"] = baseScale * (1.5f + (0.4f * num));
            }
        } else {
            __instance.data.set("_multisize_base_scale", 0);
        }

        if (__instance.isSapient())
        {
            __instance.data.get("has_stolen_3tomoe", out float has3t);
            if (has3t > 0.5f && !__instance.hasStatus("3 Tomoe-S eye"))
                lock (_statusLock) { __instance.addStatusEffect("3 Tomoe-S eye"); }

            __instance.data.get("has_stolen_itachi_eye", out float hasItachi);
            if (hasItachi > 0.5f && !__instance.hasStatus("Itachi's eye"))
                lock (_statusLock) { __instance.addStatusEffect("Itachi's eye"); }

            __instance.data.get("has_stolen_indra_eye", out float hasIndra);
            if (hasIndra > 0.5f && !__instance.hasStatus("Indra's eye"))
                lock (_statusLock) { __instance.addStatusEffect("Indra's eye"); }

            __instance.data.get("has_stolen_madara_eye", out float hasMadara);
            if (hasMadara > 0.5f && !__instance.hasStatus("Madara's eye"))
                lock (_statusLock) { __instance.addStatusEffect("Madara's eye"); }

            __instance.data.get("has_stolen_rinnegan", out float hasRinnegan);
            if (hasRinnegan > 0.5f && !__instance.hasStatus("StolenRinnegan"))
                lock (_statusLock) { __instance.addStatusEffect("StolenRinnegan"); }
            __instance.data.get("hasCMS", out float hasCMS);
            if (hasCMS > 0.5f)
            {
                string cmsId = "CMS_" + __instance.data.id;
                if (AssetManager.status.get(cmsId) == null)
                {
                    string cmsName = __instance.data.name;
                    string[] msIcons = { "ui/icons/SwirlMS", "ui/icons/SwirlMS2", "ui/icons/SecretMS", "ui/icons/TomoeMS", "ui/icons/BulkMS", "ui/icons/BibsMS" };
                    __instance.data.get("CMS_icon_idx", out float iconIdx);
                    string savedIcon = msIcons[Mathf.Clamp((int)iconIdx, 0, msIcons.Length - 1)];
                    StatusAsset cmsAsset = new StatusAsset
                    {
                        base_stats = new BaseStats(),
                        id = cmsId,
                        locale_id = "status_customSharingan_" + __instance.data.id + "_" + cmsName,
                        locale_description = "status_customSharingan_description",
                        duration = 30f,
                        animated = false,
                        path_icon = savedIcon
                    };
                    __instance.data.get("CMS_speed", out float cmsSpeed);
                    __instance.data.get("CMS_attack_speed", out float cmsAttackSpeed);
                    __instance.data.get("CMS_range", out float cmsRange);
                    __instance.data.get("CMS_damage", out float cmsDamage);
                    __instance.data.get("CMS_armor", out float cmsArmor);
                    cmsAsset.base_stats["speed"] = cmsSpeed > 0 ? cmsSpeed : 31;
                    cmsAsset.base_stats["attack_speed"] = cmsAttackSpeed > 0 ? cmsAttackSpeed : 75;
                    cmsAsset.base_stats["range"] = cmsRange > 0 ? cmsRange : 0.45f;
                    cmsAsset.base_stats["damage"] = cmsDamage > 0 ? cmsDamage : 53;
                    cmsAsset.base_stats["armor"] = cmsArmor > 0 ? cmsArmor : 6;
                    AssetManager.status.add(cmsAsset);
                    string cmsNameKey = "status_customSharingan_" + __instance.data.id + "_" + cmsName;
                    string cmsDescKey = "status_customSharingan_description";
                    try { Localization.addLocalization(cmsNameKey, cmsName + "'s Mangekyou Sharingan"); } catch { }
                    try { Localization.addLocalization(cmsDescKey, "A unique Mangekyou Sharingan, unlocked after great loss."); } catch { }
                    if (!__instance.hasStatus(cmsId))
                        lock (_statusLock) { __instance.addStatusEffect(cmsId); }
                }
                JBPatches.AddCMSHitAction(__instance);
            }

            __instance.data.get("has_stolen_byakugan", out float hasByakugan);
            if (hasByakugan > 0.5f)
            {
                bool found = __instance.hasStatus("StolenByakup");
                if (!found && __instance._active_status_dict != null) {
                    foreach (string sid in __instance._active_status_dict.Keys) {
                        if (sid.StartsWith("StolenByakup_")) { found = true; break; }
                    }
                }
                if (!found)
                {
                    int victimId = (int)__instance.data["has_stolen_byakugan"];
                    if (victimId > 0)
                    {
                        string newId = "StolenByakup_" + victimId;
                        if (AssetManager.status.get(newId) == null)
                        {
                            StatusAsset stolen = new StatusAsset
                            {
                                base_stats = new BaseStats(),
                                id = newId,
                                locale_id = "stolenByakugan_" + victimId,
                                locale_description = "stolenByakugan_" + victimId + "_description"
                            };
                            stolen.base_stats["speed"] = 16;
                            stolen.base_stats["attack_speed"] = 32;
                            stolen.base_stats["armor"] = 8;
                            stolen.base_stats["damage"] = 32;
                            stolen.duration = 86400f;
                            stolen.animated = false;
                            stolen.path_icon = "ui/icons/PulledByak";
                            AssetManager.status.add(stolen);
                            attackActions[newId] = JinchurikiActions.BHit;
                        }
                        lock (_statusLock) { __instance.addStatusEffect(newId); }
                    }
                }
            }

            __instance.data.get("hasStolenCMS", out float hasStolenCMS);
            if (hasStolenCMS > 0.5f)
            {
                __instance.data.get("originalSharinganUserID", out float origUserId);
                int victimId = (int)origUserId;
                if (victimId > 0 && !__instance.hasStatus("CMSImplant_" + victimId))
                {
                    string implantId = "CMSImplant_" + victimId;
                    if (AssetManager.status.get(implantId) == null)
                    {
                        StatusAsset src = AssetManager.status.get("CMS_" + victimId);
                        StatusAsset implant = new StatusAsset
                        {
                            base_stats = src?.base_stats ?? new BaseStats(),
                            id = implantId,
                            locale_id = "status_cms_sharingan_implant_" + victimId,
                            locale_description = "status_cms_sharingan_implant_desc",
                            duration = 86400f,
                            animated = false,
                            path_icon = "ui/icons/PulledShar"
                        };
                        AssetManager.status.add(implant);
                        string implantNameKey = "status_cms_sharingan_implant_" + victimId;
                        string implantDescKey = "status_cms_sharingan_implant_desc";
                        try { Localization.addLocalization(implantNameKey, "Stolen Mangekyou Sharingan"); } catch { }
                        try { Localization.addLocalization(implantDescKey, "A Mangekyou Sharingan taken from another Uchiha."); } catch { }
                        attackActions[implantId] = (AttackAction)Delegate.Combine((AttackAction)JinchurikiActions.MSHit, (AttackAction)JinchurikiActions.SharinganGen);
                    }
                    lock (_statusLock) { __instance.addStatusEffect(implantId); }
                }
            }

            bool hasFocusPointEligibleStatus = false;
            string[] focusPointStatuses = { "Sharingan3", "3 Tomoe-S eye", "MSharingan", "ItachiMS", "IndraMS", "MadaraMS", "Uchiha's MS eye", "Itachi's eye", "Indra's eye", "Madara's eye" };
            foreach (string fpStatus in focusPointStatuses) {
                if (__instance.hasStatus(fpStatus)) { hasFocusPointEligibleStatus = true; break; }
            }
            if (!hasFocusPointEligibleStatus && __instance._active_status_dict != null) {
                foreach (string sid in __instance._active_status_dict.Keys) {
                    if (sid.StartsWith("CMS_") || sid.StartsWith("CMSImplant_")) { hasFocusPointEligibleStatus = true; break; }
                }
            }
            if (hasFocusPointEligibleStatus && !__instance.hasStatus("Focus Point") && JinchurikiActions.tryRemoveChakra(__instance, 30))
                lock (_statusLock) { __instance.addStatusEffect("Focus Point"); }
            if (__instance.hasStatus("Focus Point"))
                __instance.data.set("_j_fp_health_snapshot", __instance.getHealth());
        }
            if (__instance._active_status_dict != null) {
            __instance.s_action_attack_target = null;
            if (!__instance.hasStatus("Palmtech2"))
            {
                foreach (string status in __instance._active_status_dict.Keys) {
                    if (attackActions.ContainsKey(status)) {
                        __instance.s_action_attack_target = (AttackAction)Delegate.Combine(__instance.s_action_attack_target, attackActions[status]);
                }
            }
        }

        if (__instance.s_action_attack_target == null)
        {
            ActorTrait immeasurableTrait = AssetManager.traits.get("immeasurableReserves");
            if (immeasurableTrait != null && immeasurableTrait.action_attack_target != null && __instance.hasTrait("immeasurableReserves"))
            {
                __instance.s_action_attack_target = (AttackAction)Delegate.Combine(__instance.s_action_attack_target, immeasurableTrait.action_attack_target);
            }
        }
        }
        float maxCh = __instance.stats["chakra"];
        if (maxCh > 0f)
        {
            __instance.data.get("chakra", out float curCh);
            if (curCh > maxCh)
                __instance.data.set("chakra", maxCh);
        }
        float maxTB = __instance.stats["tailed_beast_chakra"];
        if (maxTB > 0f)
        {
            __instance.data.get("tailed_beast_chakra", out float curTB);
            if (curTB > maxTB)
                __instance.data.set("tailed_beast_chakra", maxTB);
        }
        float maxSage = __instance.stats["sage_chakra"];
        if (maxSage > 0f)
        {
            __instance.data.get("sage_chakra", out float curSage);
            if (curSage > maxSage)
                __instance.data.set("sage_chakra", maxSage);
        }
        /*
        if (__instance._active_status_dict != null && __instance._active_status_dict.Count > 0)
        {
            if (_remainingField == null)
                _remainingField = AccessTools.Field(typeof(Status), "remainingTime");
            if (_remainingField == null) return;
            foreach (var kv in __instance._active_status_dict)
            {
                if (IsPermanentStatus(kv.Key))
                {
                    float remaining = (float)_remainingField.GetValue(kv.Value);
                    if (remaining < 43200f)
                        lock (_statusLock) { __instance.addStatusEffect(kv.Key); }
                }
            }
        }
        */
        __instance.data.get("_j_imeasurable_stunned_until", out float imStunUntil);
        if (imStunUntil > 0f)
        {
            if (imStunUntil > Time.time)
            {
                if (!__instance.hasStatus("stunned"))
                    lock (_statusLock) { __instance.addStatusEffect("stunned"); }
            }
            else
            {
                __instance.data.set("_j_imeasurable_stunned_until", 0f);
            }
        }
    }

    private static bool IsPermanentStatus(string statusId)
    {
        if (_permanentStatuses.Contains(statusId)) return true;
        if (statusId.StartsWith("StolenByakup_")) return true;
        if (statusId.StartsWith("CMSImplant_")) return true;
        return false;
    }

    [HarmonyPatch(typeof(Actor), "checkCallbacksOnDeath")]
    [HarmonyPostfix]
    public static void checkCallbacksOnDeath_Postfix(ref Actor __instance) {
        bool hasCustomSharingan = __instance.data["hasCMS"] >= 0.5f;
        bool hasStolenSharingan = __instance.data["hasStolenCMS"] >= 0.5f;

        if (hasCustomSharingan) {
            CustomSharinganActions.removeCMS("CMS_" + __instance.data.id);
        }

        if (hasStolenSharingan) {
            string id = GetStringData(__instance.data.id, "originalSharinganUserID");
            CustomSharinganActions.removeCMS("CMSImplant_" + id);
        }

        if (__instance.hasTrait("SLegend"))
            SendWorldLog(__instance, "jb_log_legend_died");

        if (__instance.hasTrait("itachiReincarnate") || __instance.hasTrait("indrasReincarnate") || __instance.hasTrait("madarasReincarnate"))
            SendWorldLog(__instance, "jb_log_reincarnated_died");
    }

    [HarmonyPatch(typeof(Actor), "checkCallbacksOnDeath")]
    [HarmonyPrefix]
    public static void checkCallbacksOnDeath_Prefix(ref Actor __instance) {
        JinchurikiActions.Transfer(__instance);
        __instance.data.set("_j_rt_anchor", 0f);
    }

    [HarmonyPatch(typeof(Actor), "generatePersonality")]
    [HarmonyPostfix]
    public static void generatePersonality_Postfix(Actor __instance) {
        if (__instance.hasTrait("windRelease") && !__instance.hasTrait("rasengan") && Randy.randomChance(0.15f))
            __instance.addTrait("rasengan");

        if (__instance.hasTrait("raiton")) {
            if (!__instance.hasTrait("chidori") && Randy.randomChance(0.15f))
                __instance.addTrait("chidori");
            if (!__instance.hasTrait("kirin") && Randy.randomChance(0.08f))
                __instance.addTrait("kirin");
            if (!__instance.hasTrait("LC") && Randy.randomChance(0.01f))
                __instance.addTrait("LC");
        }

        if (__instance.data["birth_traits_assigned"] < 0.5f)
        {
            __instance.AssignChakraTrait();
            __instance.AssignBirthTraits();

            if (__instance.hasTrait("prodigy"))
                SendWorldLog(__instance, "jb_log_prodigy_born");

            if (__instance.hasTrait("SLegend"))
            {
                SendWorldLog(__instance, "jb_log_legend_born");
                if (WorldTip.instance != null)
                    WorldTip.instance.show("<color=orange>Legendary Shinobi spawn!</color>", false, "top", 3f);
                try { __instance.data.favorite = true; } catch { }
            }

            if (__instance.hasTrait("immeasurableReserves"))
            {
                SendWorldLog(__instance, "jb_log_immeasurable_born");
                if (WorldTip.instance != null)
                    WorldTip.instance.show("<color=#00BFFF>A vast Chakra presence has been Detected!</color>", false, "top", 3f);
                try { __instance.data.favorite = true; } catch { }
            }

            if (__instance.hasTrait("chakraC5"))
                SendWorldLog(__instance, "jb_log_chakraC5_born");

            if (__instance.hasTrait("uchihaClan"))
            {
                if (!__instance.hasTrait("itachiReincarnate") && !ReincarnateExists("itachiReincarnate") && Randy.randomChance(0.01f))
                {
                    __instance.addTrait("itachiReincarnate");
                    _assignedReincarnates.Add("itachiReincarnate");
                    SendWorldLog(__instance, "jb_log_itachi_reincarnated");
                    if (WorldTip.instance != null)
                        WorldTip.instance.show("<color=#FF8C00>" + __instance.getName() + " Has been reincarnated!</color>", false, "top", 3f);
                    try { __instance.data.favorite = true; } catch { }
                    if (!__instance.hasTrait("prodigy")) __instance.addTrait("prodigy");
                    __instance.data.name = "Itachi Uchiha";
                }
                if (!__instance.hasTrait("indrasReincarnate") && !ReincarnateExists("indrasReincarnate") && Randy.randomChance(0.01f))
                {
                    __instance.addTrait("indrasReincarnate");
                    _assignedReincarnates.Add("indrasReincarnate");
                    SendWorldLog(__instance, "jb_log_indra_reincarnated");
                    if (WorldTip.instance != null)
                        WorldTip.instance.show("<color=purple>" + __instance.getName() + " Has been reincarnated!</color>", false, "top", 3f);
                    try { __instance.data.favorite = true; } catch { }
                    if (Randy.randomChance(0.20f) && !__instance.hasTrait("otsutsuki80")) __instance.addTrait("otsutsuki80");
                    if (!__instance.hasTrait("indraChakra")) __instance.addTrait("indraChakra");
                    __instance.data.name = "Indra Otsutsuki";
                }
                if (!__instance.hasTrait("madarasReincarnate") && !ReincarnateExists("madarasReincarnate") && Randy.randomChance(0.01f))
                {
                    __instance.addTrait("madarasReincarnate");
                    _assignedReincarnates.Add("madarasReincarnate");
                    SendWorldLog(__instance, "jb_log_madara_reincarnated");
                    if (WorldTip.instance != null)
                        WorldTip.instance.show("<color=red>" + __instance.getName() + " Has been reincarnated!</color>", false, "top", 3f);
                    try { __instance.data.favorite = true; } catch { }
                    if (!__instance.hasTrait("SLegend") && Randy.randomChance(0.30f)) __instance.addTrait("SLegend");
                    __instance.data.name = "Madara Uchiha";
                }
            }

            try { __instance.data.set("birth_traits_assigned", 1); } catch { }
        }

        CleanupSpecialChakra(__instance);

        if (__instance.hasTrait("hyugaClan") && !__instance.hasTrait("indraChakra") && !__instance.hasTrait("hamuraChakra") && !__instance.hasTrait("asuraChakra") && !__instance.hasTrait("hagoromoChakra") && !__instance.hasTrait("sealedfate"))
            __instance.addTrait("sealedfate");
    }

    private static readonly string[] chakraTraitIds = { "immeasurableReserves", "chakraC5", "chakraC4", "chakraC3", "chakraC2", "chakraC1", "chakraC" };
    private static readonly int[] chakraWeights = { 1, 12, 35, 75, 125, 225, 25 };

    private static void CleanupSpecialChakra(Actor actor)
    {
        if ((actor.hasTrait("indraChakra") || actor.hasTrait("hamuraChakra") || actor.hasTrait("asuraChakra") || actor.hasTrait("hagoromoChakra")) && actor.hasTrait("sealedfate"))
            actor.removeTrait("sealedfate");

        if ((actor.hasTrait("indraChakra") || actor.hasTrait("hamuraChakra")) && actor.hasTrait("asuraChakra"))
        {
            actor.addTrait("hagoromoChakra");
            actor.removeTrait("indraChakra");
            actor.removeTrait("hamuraChakra");
            actor.removeTrait("asuraChakra");
        }
    }

    [HarmonyPatch(typeof(Actor), "addTrait", new Type[] { typeof(string), typeof(bool) })]
    [HarmonyPrefix]
    public static bool addTrait_ChakraConflictCheck(Actor __instance, string pTraitID, ref bool __result)
    {
        if (__instance == null || pTraitID == null) return true;

        ActorTrait trait = AssetManager.traits.get(pTraitID);
        if (trait == null) return true;

        if (trait.group_id == JinchurikiTraitGroup.ChakraTrait2)
        {
            foreach (var id in chakraTraitIds)
            {
                if (id != pTraitID && __instance.hasTrait(id))
                {
                    __result = false;
                    return false;
                }
            }
        }

        if (pTraitID == "sealedfate" && (__instance.hasTrait("indraChakra") || __instance.hasTrait("hamuraChakra") || __instance.hasTrait("asuraChakra") || __instance.hasTrait("hagoromoChakra")))
        {
            __result = false;
            return false;
        }

        return true;
    }

    private static void AssignChakraTrait(this Actor actor)
    {
        if (!actor.isSapient()) return;

        foreach (var id in chakraTraitIds)
        {
            if (actor.hasTrait(id))
                actor.removeTrait(id);
        }

        int totalWeight = 0;
        foreach (var w in chakraWeights) totalWeight += w;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        string assignedId = null;
        for (int i = 0; i < chakraTraitIds.Length; i++)
        {
            cumulative += chakraWeights[i];
            if (roll < cumulative)
            {
                assignedId = chakraTraitIds[i];
                break;
            }
        }
        if (assignedId != null)
        {
            actor.addTrait(assignedId);
            if (assignedId == "chakraC" && Randy.randomChance(0.01f))
            {
                actor.addTrait("eightGatesTechnique");
                SendWorldLog(actor, "jb_log_taijutsu_specialist");
            }
        }
        CleanupSpecialChakra(actor);
    }

    private static readonly string[] rankTraitIds = { "academyStudent", "genin", "chunin", "jonin", "kage", "formerKage", "SLegend", "prodigy" };

    private static void AssignBirthTraits(this Actor actor)
    {
        if (!actor.isSapient()) return;

        string[] commonElements = { "fireRelease", "earthRelease", "waterRelease", "windRelease" };
        int elementCount = 0;
        foreach (var id in commonElements) { if (actor.hasTrait(id)) elementCount++; }

        foreach (var element in commonElements)
        {
            if (actor.hasTrait(element)) continue;
            float chance = elementCount >= 3 ? 0.05f : 0.35f;
            if (Randy.randomChance(chance))
            {
                actor.addTrait(element);
                elementCount++;
            }
        }

        if (!actor.hasTrait("raiton"))
        {
            float raitonChance = elementCount >= 3 ? 0.02f : 0.10f;
            if (Randy.randomChance(raitonChance))
                actor.addTrait("raiton");
        }

        bool hasRank = false;
        foreach (var id in rankTraitIds)
        {
            if (actor.hasTrait(id)) { hasRank = true; break; }
        }
        if (!hasRank && Randy.randomChance(0.40f))
            actor.addTrait("academyStudent");

        if (!actor.hasTrait("prodigy") && Randy.randomChance(0.01f))
            actor.addTrait("prodigy");

        if (!actor.hasTrait("SLegend") && Randy.randomChance(0.002f))
            actor.addTrait("SLegend");
    }

    public static void SendWorldLog(Actor actor, string logAssetId, string specialParam = null)
    {
        if (actor == null) return;
        try
        {
            EnsureLogAssets();
            WorldLogLibrary lib = AssetManager.world_log_library;
            WorldLogAsset asset = lib.get(logAssetId);
            if (asset != null)
            {
                var message = new WorldLogMessage(asset, specialParam ?? actor.getName() ?? string.Empty);
                message.unit = actor;
                if (actor.current_tile != null)
                    message.location = actor.current_tile.posV3;
                message.add();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[JBPatches] Error sending world log: " + ex.Message);
        }
    }

    private static bool TraitExistsInWorld(string traitId)
    {
        try
        {
            var allActors = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (var go in allActors)
            {
                if (go != null)
                {
                    var actor = go.GetComponent<Actor>();
                    if (actor != null && actor.data != null && actor.hasTrait(traitId) && actor.isAlive())
                        return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[JBPatches] TraitExistsInWorld error: " + ex.Message);
        }
        return false;
    }

    public static void ClearReincarnates()
    {
        _assignedReincarnates.Clear();
    }

    private static bool ReincarnateExists(string traitId)
    {
        if (_assignedReincarnates.Contains(traitId))
            return true;
        if (TraitExistsInWorld(traitId))
        {
            _assignedReincarnates.Add(traitId);
            return true;
        }
        return false;
    }

    public static void EnsureLogAssets()
    {
        try
        {
            WorldLogLibrary lib = AssetManager.world_log_library;
            if (lib == null) return;

            LogAssetIfMissing(lib, "jb_log_cms_awakened", "ui/icons/uch", Color.red);
            LogAssetIfMissing(lib, "jb_log_prodigy_born", "ui/Icons/Prodigy", Toolbox.color_log_warning);
            LogAssetIfMissing(lib, "jb_log_legend_born", "ui/icons/legend", new Color(1f, 0.5f, 0f));
            LogAssetIfMissing(lib, "jb_log_legend_died", "ui/icons/legend", new Color(1f, 0.5f, 0f));
            LogAssetIfMissing(lib, "jb_log_immeasurable_born", "ui/icons/ChakraNum", new Color(0f, 0.40f, 1.00f));
            LogAssetIfMissing(lib, "jb_log_chakraC5_born", "ui/icons/Chakra3", Toolbox.color_log_warning);
            LogAssetIfMissing(lib, "jb_log_itachi_reincarnated", "ui/icons/WheelofSamsara", new Color(1f, 0.5f, 0f));
            LogAssetIfMissing(lib, "jb_log_indra_reincarnated", "ui/icons/WheelofSamsara", new Color(1f, 0.5f, 0f));
            LogAssetIfMissing(lib, "jb_log_madara_reincarnated", "ui/icons/WheelofSamsara", new Color(1f, 0.5f, 0f));
            LogAssetIfMissing(lib, "jb_log_reincarnated_died", "ui/icons/WheelofSamsara", new Color(1f, 0.5f, 0f));
            LogAssetIfMissing(lib, "jb_log_rinnegan_inherited", "ui/icons/Rinnegan", new Color(0.6f, 0.2f, 0.8f));
            LogAssetIfMissing(lib, "jb_log_taijutsu_specialist", "ui/icons/brain", Toolbox.color_log_warning);
            LogAssetIfMissing(lib, "jb_log_eighth_gate", "ui/icons/EightGH", Color.red);
            LogAssetIfMissing(lib, "jb_log_cms_stolen", "ui/icons/PulledShar", new Color(0.8f, 0.2f, 0.2f));
            LogAssetIfMissing(lib, "jb_log_rinnegan_stolen", "ui/icons/Rinnegan", new Color(0.6f, 0.2f, 0.8f));
        }
        catch (Exception ex)
        {
            Debug.Log("[JBPatches] Error initializing world log assets: " + ex.Message);
        }
    }

    private static void LogAssetIfMissing(WorldLogLibrary lib, string id, string iconPath, Color color)
    {
        if (lib.get(id) != null) return;
        WorldLogAsset asset = new WorldLogAsset
        {
            id = id,
            locale_id = id,
            group = "events",
            path_icon = iconPath,
            color = color,
            text_replacer = (WorldLogMessage msg, ref string text) =>
            {
                text = text.Replace("$name$", msg.getSpecial(1));
            }
        };
        lib.add(asset);
    }

    [HarmonyPatch]
    public static class InfiniteDurationPatch
    {
        private static MethodInfo _target;

        public static bool Prepare()
        {
            try
            {
                var asm = typeof(Actor).Assembly;
                _target = FindDurationFormatMethod(asm);
                if (_target != null)
                {
                    Debug.Log($"[JBDuration] Patching '{_target.DeclaringType.Name}.{_target.Name}' for Infinite display");
                    return true;
                }
                Debug.LogWarning("[JBDuration] No duration format method found. Display will show 'Infinity' instead of 'Infinite'.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JBDuration] Prepare failed: {ex.Message}");
            }
            return false;
        }

        public static MethodBase TargetMethod() => _target;

        [HarmonyPostfix]
        public static void Postfix(ref string __result)
        {
            if (__result != null && __result.Contains("Infinity"))
                __result = __result.Replace("Infinity", "Infinite");
        }

        private static MethodInfo FindDurationFormatMethod(Assembly asm)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            foreach (var type in types)
            {
                if (type == null) continue;
                string tn = type.Name;
                if (tn == "Status" || tn.Contains("Tooltip") || tn.Contains("TipBox"))
                {
                    foreach (var m in type.GetMethods(flags))
                    {
                        if (m.ReturnType != typeof(string)) continue;
                        var par = m.GetParameters();
                        if (par.Length == 1 && par[0].ParameterType == typeof(float))
                            return m;
                    }
                }
            }

            string[] names = { "FormatDuration", "GetDurationText", "GetDurationString", "FormatRemainingTime", "GetTooltip", "GetDescription" };
            foreach (var type in types)
            {
                if (type == null) continue;
                foreach (var name in names)
                {
                    var m = type.GetMethod(name, flags);
                    if (m != null && m.ReturnType == typeof(string))
                        return m;
                }
            }

            foreach (var type in types)
            {
                if (type == null) continue;
                foreach (var m in type.GetMethods(flags))
                {
                    if (m.ReturnType != typeof(string)) continue;
                    var par = m.GetParameters();
                    if (par.Length == 1 && par[0].ParameterType == typeof(float))
                        return m;
                }
            }

            return null;
        }
    }
}
