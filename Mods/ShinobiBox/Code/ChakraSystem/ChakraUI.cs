using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;

namespace ShinobiBox
{
    [HarmonyPatch(typeof(StatsIconContainer), "Awake")]
    public static class StatsIconPatches
    {
        [HarmonyPostfix]
        static void Postfix(StatsIconContainer __instance)
        {
            if (__instance == null || __instance._stats_icons == null || __instance._stats_icons.Count == 0) return;

            CreateIcon(__instance, "i_Chakra", "ui/icons/staticon", "Chakra", "Energy used for Shinobi techniques.");
            CreateIcon(__instance, "i_tailed_beast_chakra", "ui/icons/9Tails", "Tailed-Beast Chakra", "Fuels jinchuriki forms and Tailed Beast Bombs. Refills while not transformed.");
            CreateIcon(__instance, "i_sage_chakra", "ui/icons/staticon", "Sage Chakra", "Natural energy that fuels Sage Mode. Refills slowly, and quickly while meditating.");
            CreateIcon(__instance, "i_blindness", "ui/icons/Blind", "Blindness", "Mangekyo Sharingan usage level. At 25, permanent blindness occurs.");
        }

        public static void CreateIcon(StatsIconContainer container, string id, string spritePath, string title, string desc)
        {
            if (container._stats_icons.ContainsKey(id)) return;

            var template = container._stats_icons.Values.First();
            GameObject cloneGO = UnityEngine.Object.Instantiate(template.gameObject, template.transform.parent);
            cloneGO.name = id;

            StatsIcon icon = cloneGO.GetComponent<StatsIcon>();
            Sprite sprite = SpriteTextureLoader.getSprite(spritePath);
            if (sprite != null) icon.getIcon().sprite = sprite;

            TipButton tip = cloneGO.GetComponent<TipButton>();
            tip.textOnClick = title;
            tip.textOnClickDescription = desc;

            container._stats_icons.Add(id, icon);
        }
    }

    [HarmonyPatch(typeof(UnitStatsElement), "showContent")]
    public static class ChakraValuePatch
    {
        [HarmonyPostfix]
        static void Postfix(UnitStatsElement __instance)
        {
            if (__instance == null || __instance.actor == null || !__instance.actor.isAlive()) return;

            UpdateChakraIcons(__instance, __instance.actor);
        }

        public static void UpdateChakraIcons(UnitStatsElement stats, Actor actor)
        {
            if (!ChakraSystem.CanUseChakra(actor))
            {
                HideIcon(stats, "i_Chakra");
                HideIcon(stats, "i_tailed_beast_chakra");
                HideIcon(stats, "i_sage_chakra");
                HideIcon(stats, "i_blindness");
                return;
            }

            ShowResource(stats, "i_Chakra", ChakraSystem.GetCurrent(actor), ChakraSystem.GetMax(actor));
            ShowResource(stats, "i_tailed_beast_chakra", ChakraSystem.GetTailedBeastChakra(actor), ChakraSystem.GetMaxTailedBeastChakra(actor));
            ShowResource(stats, "i_sage_chakra", ChakraSystem.GetSageChakra(actor), ChakraSystem.GetMaxSageChakra(actor));

            int blindness = 0;
            actor.data.get("ms_usage", out blindness);
            if (blindness > 0 || actor.hasTrait("mangekyo_sharingan"))
            {
                stats.setIconValue("i_blindness", Mathf.Min(blindness, 25), 25, "", false, "", '/');
            }
            else
            {
                HideIcon(stats, "i_blindness");
            }
        }

        private static void ShowResource(UnitStatsElement stats, string iconId, float current, float max)
        {
            if (max <= 0f)
            {
                HideIcon(stats, iconId);
                return;
            }

            stats.setIconValue(iconId, (int)current, (int)max, "", false, "", '/');
        }

        private static void HideIcon(UnitStatsElement stats, string iconId)
        {
            if (stats._stats_icons != null && stats._stats_icons.TryGetValue(iconId, out StatsIcon icon))
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(UnitWindow), "Update")]
    public static class RealTimeChakraUpdate
    {
        private static UnitWindow cachedWindow;
        private static UnitStatsElement cachedStats;
        private static float nextUpdateTime;

        [HarmonyPostfix]
        public static void Postfix(UnitWindow __instance)
        {
            if (!__instance.gameObject.activeSelf || __instance.actor == null || !__instance.actor.isAlive()) return;
            if (Time.unscaledTime < nextUpdateTime) return;
            nextUpdateTime = Time.unscaledTime + 0.25f;

            if (cachedWindow != __instance || cachedStats == null)
            {
                cachedWindow = __instance;
                cachedStats = __instance.GetComponentInChildren<UnitStatsElement>();
            }

            if (cachedStats != null)
            {
                ChakraValuePatch.UpdateChakraIcons(cachedStats, __instance.actor);
            }
        }
    }
}