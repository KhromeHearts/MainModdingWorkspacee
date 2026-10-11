using HarmonyLib;
using UnityEngine;

namespace Optime.Optimizations {
    internal sealed class PowerButtonSelectorFix : Feature<PowerButtonSelectorFix> {
        public new static void SetLoaded(bool value) {
            Feature<PowerButtonSelectorFix>.SetLoaded(value);
        }

        [HarmonyPatch(typeof(PowerButtonSelector), nameof(PowerButtonSelector.updateTopButtons))]
        [HarmonyPrefix]
        public static bool PowerButtonSelector_updateTopButtons_Prefix(ref PowerButtonSelector __instance) {
            if (__instance.bottomElementsMover.visible) {
                __instance.cancelButton.goDown = false;
                __instance.cancelButton.goUp = false;
                __instance.cancelButton.gameObject.SetActive(true);
                bool enableDebugButton = DebugConfig.isOn(DebugOption.DebugButton);
                DebugConfig.instance.debugButton.SetActive(enableDebugButton);
                __instance.clockButton.GetComponent<CancelButton>().goDown = false;
                __instance.joy_control_cancel_button.SetActive(false);

                return false;
            }

            if (ControllableUnit.isControllingUnit()) {
                __instance.cancelButton.gameObject.SetActive(false);
                PremiumElementsChecker.toggleActive(false);
                DebugConfig.instance.debugButton.SetActive(false);
                __instance.joy_control_cancel_button.SetActive(true);
                Sprite sprite = ControllableUnit.getControllableUnit().getActorAsset().getSpriteIcon();
                __instance._joy_control_cancel_button_icon.sprite = sprite;
            }

            bool inSpectatorMode = MoveCamera.inSpectatorMode();
            __instance.cancelButton.goDown =
                inSpectatorMode || ControllableUnit.isControllingUnit() && !Config.joyControls;
            __instance.clockButton.GetComponent<CancelButton>().goDown = inSpectatorMode;

            return false;
        }
    }
}