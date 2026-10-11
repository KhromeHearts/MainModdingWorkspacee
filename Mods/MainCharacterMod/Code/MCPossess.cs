using System;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.api;
using HarmonyLib;

namespace MC
{
    public class MCPossess : MonoBehaviour
    {
        public static MCPossess Instance;

        private PossessionUnitInfo possessionInfo;
        private Transform healthBar;
        private Transform barBackground;
        private CanvasGroup[] healthBarParentGroups;
        private CanvasRenderer[] healthBarChildRenderers;
        private bool healthBarConfigured;
        private void Awake()
        {
            Instance = this;

            if (MCUIManager.Instance == null)
            {
                gameObject.AddComponent<MCUIManager>();
            }

            if (MCInventory.Instance == null)
            {
                gameObject.AddComponent<MCInventory>();
            }
        }

        private void LateUpdate()
        {
            if (ControllableUnit.getControllableUnit() == null) return;

            if (possessionInfo == null || !possessionInfo.isActiveAndEnabled)
            {
                possessionInfo = UnityEngine.Object.FindObjectOfType<PossessionUnitInfo>();
            }

            if (possessionInfo != null && !healthBarConfigured)
            {
                ConfigureHealthBar(possessionInfo);
            }
        }

        public void ConfigureHealthBar(PossessionUnitInfo info)
        {
            if (info == null) return;

            Canvas canvas = CanvasMain.instance == null ? null : CanvasMain.instance.canvas_ui;
            StatBar statBar = info.GetComponentInChildren<StatBar>(true);

            if (statBar == null)
            {
                return;
            }

            healthBar = statBar.transform;
            barBackground = healthBar.Find("Background");
            RectTransform healthBarRect = healthBar.GetComponent<RectTransform>();
            if (canvas != null && healthBarRect != null)
            {
                healthBarRect.SetParent(canvas.transform, false);
                healthBarRect.anchorMin = new Vector2(1f, 1f);
                healthBarRect.anchorMax = new Vector2(1f, 1f);
                healthBarRect.pivot = new Vector2(1f, 1f);
                healthBarRect.anchoredPosition = new Vector2(-18f, -18f);
                healthBarRect.localScale = Vector3.one;
                healthBarRect.sizeDelta = new Vector2(180f, 26f);
            }

            Image healthBarBackground = healthBar.GetComponent<Image>();
            if (healthBarBackground != null)
            {
                healthBarBackground.sprite = Resources.Load<Sprite>("ui/icons/GoldHealthBar");
                healthBarBackground.color = Color.white;
                healthBarBackground.material = null;
                healthBarBackground.type = Image.Type.Sliced;
            }

            RectTransform backgroundRect = null;
            if (barBackground != null)
            {
                backgroundRect = barBackground.GetComponent<RectTransform>();
                if (backgroundRect != null)
                {
                    backgroundRect.anchorMin = Vector2.zero;
                    backgroundRect.anchorMax = Vector2.one;
                    backgroundRect.offsetMin = Vector2.zero;
                    backgroundRect.offsetMax = Vector2.zero;
                    backgroundRect.localScale = new Vector3(0.75f, 0.65f, 1f);
                }
            }



            RectTransform maskRect = statBar.mask;
            RectTransform barRect = statBar.bar;
            RectTransform textRect = statBar.textField == null
                ? null
                : statBar.textField.GetComponent<RectTransform>();
            if (maskRect == null || barRect == null) return;

            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = new Vector2(6f, 5f);
            maskRect.offsetMax = new Vector2(-6f, -5f);
            maskRect.localScale = new Vector3(0.82f, 1.0f, 1.0f);
            maskRect.localPosition = new Vector3(-89.8704f, -12.7408f, 0f);

            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.anchoredPosition = Vector2.zero;
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;

            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                statBar.textField.alignment = TextAnchor.MiddleCenter;
                statBar.textField.transform.localScale = Vector3.one;
            }

            healthBarParentGroups = healthBar.GetComponentsInParent<CanvasGroup>(true);
            foreach (CanvasGroup parentGroup in healthBarParentGroups)
            {
                parentGroup.alpha = 1f;
            }

            healthBarChildRenderers = healthBar.GetComponentsInChildren<CanvasRenderer>(true);
            foreach (CanvasRenderer childRenderer in healthBarChildRenderers)
            {
                childRenderer.SetAlpha(1f);
            }

            healthBarConfigured = true;
        }
    }


    // Zoom camera & enable custom UI when unit info panel turns on
    [HarmonyPatch(typeof(PossessionUnitInfo), "OnEnable")]
    public static class PossesionOnEnable
    {
        public static void Postfix(PossessionUnitInfo __instance)
        {
            if (MoveCamera.instance.main_camera.orthographicSize > 11f)
            {
                MoveCamera.instance._target_zoom = 11f;
                MoveCamera.instance.main_camera.orthographicSize = 11f;
            }

            if (MCUIManager.Instance != null)
            {
                MCUIManager.Instance.SetUIVisible(true);
            }

            if (MCPossess.Instance != null)
            {
                MCPossess.Instance.ConfigureHealthBar(__instance);
            }
        }
    }


    // Sync custom UI state with PossessionUI state transitions
    [HarmonyPatch(typeof(PossessionUI), "toggleInstance")]
    public static class PossessionUITogglePatch
    {
        public static void Postfix(bool pState)
        {
            Transform inner = PossessionUI.instance.transform.Find("Inner");
            Transform possesionText = PossessionUI.instance.transform.FindRecursive("Right");

            // Sync custom UI visibility with native possession state
            if (MCUIManager.Instance != null)
            {
                MCUIManager.Instance.SetUIVisible(pState);
            }

            // Hide native possession HUD elements so only custom UI is visible
            if (PossessionUI.instance != null && PossessionUI.instance.transform.Find("Inner") != null)
            {
                PossessionUI.instance.transform.Find("Inner").gameObject.SetActive(true);
                possesionText.gameObject.SetActive(false);
                //SetPossessionChildActive("Age", false);

                //healthBarRect.anchoredPosition = new Vector2(1700f, 1000f);

                //healthBarRect.localScale = new Vector3(1.5f, 1.5f, 1);

            }


        }

        private static void SetPossessionChildActive(string childName, bool active)
        {
            if (PossessionUI.instance == null)
            {
                return;
            }

            Transform child =
                PossessionUI.instance.transform.FindRecursive(childName);

            if (child != null)
            {
                child.gameObject.SetActive(active);
            }
        }
    }

    [HarmonyPatch(typeof(ControllableUnit), "setControllableCreature")]
    public static class PossessionMovementHotkeyPatch
    {
        public static void Postfix()
        {
            HotkeyLibrary.up.ignore_mod_keys = true;
            HotkeyLibrary.down.ignore_mod_keys = true;
            HotkeyLibrary.left.ignore_mod_keys = true;
            HotkeyLibrary.right.ignore_mod_keys = true;
        }
    }

    [HarmonyPatch(typeof(Actor), "pickupResourcesFromKill")]
    public static class PossessedKillResourcesPatch
    {
        public static void Prefix(Actor __instance, Actor pAttacker)
        {
            if (__instance == null || pAttacker == null || pAttacker.hasCity()) return;
            if (!ControllableUnit.isControllingUnit(pAttacker)) return;
            if (__instance.asset.id == pAttacker.asset.id && !pAttacker.hasTrait("savage")) return;

            foreach (ResourceContainer resource in __instance.getResourcesFromActor())
            {
                pAttacker.addToInventory(resource);
            }
        }
    }

}