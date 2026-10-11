using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class UiWorldAgeFix : Feature<UiWorldAgeFix> {
        private static float _lastMonthTime = -1f;
        private static float _lastAgeProgress = -1f;
        private static bool _lastPaused;
        private static WorldAgeAsset _lastNextEra;
        private static WorldAgeAsset _lastCurrentEra;

        public new static void SetLoaded(bool value) {
            Feature<UiWorldAgeFix>.SetLoaded(value);
        }

        protected override void OnUnload() {
            _lastMonthTime = -1f;
            _lastAgeProgress = -1f;
            _lastPaused = false;
            _lastNextEra = null;
            _lastCurrentEra = null;
        }

        [HarmonyPatch(typeof(UiWorldAgeInfo), nameof(UiWorldAgeInfo.Update))]
        [HarmonyPrefix]
        public static bool UiWorldAgeInfo_Update_Prefix(ref UiWorldAgeInfo __instance) {
            if (!Config.game_loaded || World.world == null || World.world_era == null) {
                return true;
            }

            WorldAgeManager eraManager = World.world.era_manager;
            bool paused = eraManager.isPaused();

            if (paused != _lastPaused) {
                Sprite clockSprite = paused ? __instance._sprite_pause : __instance._sprite_play;
                __instance._icon_clock_left.sprite = clockSprite;
                __instance._icon_clock_right.sprite = clockSprite;
                _lastPaused = paused;
            }

            WorldAgeAsset currentEra = World.world_era;

            if (currentEra != _lastCurrentEra) {
                Sprite currentAgeSprite = currentEra.getSprite();
                __instance._icon_age_left.sprite = currentAgeSprite;
                __instance._icon_age_right.sprite = currentAgeSprite;
                __instance._text_age_title.color = currentEra.title_color;
                __instance._text_age_title_localized.setKeyAndUpdate(currentEra.getLocaleID());
                _lastCurrentEra = currentEra;
            }

            float monthTime = Date.getCurrentMonth() + Date.getMonthTime() / 5f - 1f;

            if (Mathf.Abs(monthTime - _lastMonthTime) > 0.001f) {
                __instance._bar_year.bar.sizeDelta = new Vector2(
                    Mathf.Floor(monthTime / 12f * __instance._bar_year.mask.rect.width),
                    __instance._bar_year.bar.sizeDelta.y);
                __instance._text_year_localized.setKeyAndUpdate("year_era");
                __instance._text_month_localized.setKeyAndUpdate(AssetManager.months.getMonth(Date.getCurrentMonth())
                    .getLocaleID());
                _lastMonthTime = monthTime;
            }

            float ageProgress = World.world.map_stats.current_age_progress;

            if (Mathf.Abs(ageProgress - _lastAgeProgress) > 0.001f) {
                __instance._bar_age.bar.sizeDelta = new Vector2(
                    Mathf.Floor(ageProgress * __instance._bar_age.mask.rect.width),
                    __instance._bar_age.bar.sizeDelta.y);
                __instance._bar_age.gameObject.SetActive(true);

                _lastAgeProgress = ageProgress;
            }

            __instance._bar_age_localized.setKeyAndUpdate(paused ? "ages_paused" : "next_age_in_moons");

            WorldAgeAsset nextEra = eraManager.getNextAge();

            if (nextEra != _lastNextEra) {
                Sprite nextSprite = nextEra != null ? nextEra.getSprite() : __instance._random_age_sprite;
                __instance._icon_age_next_left.sprite = nextSprite;
                __instance._icon_age_next_right.sprite = nextSprite;
                _lastNextEra = nextEra;
            }

            return false;
        }
    }
}