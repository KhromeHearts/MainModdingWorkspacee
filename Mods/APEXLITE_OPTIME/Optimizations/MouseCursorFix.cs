using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Optime.Optimizations {
    internal sealed class MouseCursorFix : Feature<MouseCursorFix> {
        private static bool _isOverUIButton;

        private static readonly Dictionary<EventTrigger, List<UnityAction<BaseEventData>>> AddedListeners =
            new Dictionary<EventTrigger, List<UnityAction<BaseEventData>>>();

        public new static void SetLoaded(bool value) {
            Feature<MouseCursorFix>.SetLoaded(value);
        }

        protected override void OnLoad() {
            foreach (Button button in FindObjectsOfType<Button>(true)) {
                EventTrigger trigger = button.GetComponent<EventTrigger>();

                if (trigger == null) {
                    continue;
                }

                if (!AddedListeners.ContainsKey(trigger)) {
                    AddedListeners[trigger] = new List<UnityAction<BaseEventData>>();
                }

                foreach (Entry entry in trigger.triggers) {
                    if (entry.eventID == EventTriggerType.PointerEnter) {
                        AddCallback(trigger, entry, e => { _isOverUIButton = true; });
                    } else if (entry.eventID == EventTriggerType.PointerExit) {
                        AddCallback(trigger, entry, e => { _isOverUIButton = false; });
                    }
                }
            }
        }

        protected override void OnUnload() {
            foreach (KeyValuePair<EventTrigger, List<UnityAction<BaseEventData>>> kvp in AddedListeners) {
                EventTrigger trigger = kvp.Key;
                List<UnityAction<BaseEventData>> callbacks = kvp.Value;

                if (trigger == null) {
                    continue;
                }

                foreach (UnityAction<BaseEventData> callback in callbacks) {
                    foreach (Entry entry in trigger.triggers) {
                        entry.callback.RemoveListener(callback);
                    }
                }
            }

            AddedListeners.Clear();
            _isOverUIButton = false;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.isPointerOverUIButton))]
        [HarmonyPrefix]
        public static bool PlayerControl_isPointerOverUIButton_Prefix(ref bool __result) {
            __result = _isOverUIButton;

            return false;
        }

        [HarmonyPatch(typeof(ButtonExtensions), nameof(ButtonExtensions.OnHover), typeof(Button), typeof(UnityAction))]
        [HarmonyPostfix]
        public static void ButtonExtensions_OnHover_Postfix(Button button) {
            EventTrigger trigger = button.GetComponent<EventTrigger>();

            if (trigger == null) {
                return;
            }

            if (!AddedListeners.ContainsKey(trigger)) {
                AddedListeners[trigger] = new List<UnityAction<BaseEventData>>();
            }

            foreach (Entry entry in trigger.triggers.Where(entry => entry.eventID == EventTriggerType.PointerEnter)) {
                AddCallback(trigger, entry, e => { _isOverUIButton = true; });
            }
        }

        [HarmonyPatch(typeof(ButtonExtensions), nameof(ButtonExtensions.OnHoverOut), typeof(Button),
            typeof(UnityAction))]
        [HarmonyPostfix]
        public static void ButtonExtensions_OnHoverOut_Postfix(Button button) {
            EventTrigger trigger = button.GetComponent<EventTrigger>();

            if (trigger == null) {
                return;
            }

            if (!AddedListeners.ContainsKey(trigger)) {
                AddedListeners[trigger] = new List<UnityAction<BaseEventData>>();
            }

            foreach (Entry entry in trigger.triggers.Where(entry => entry.eventID == EventTriggerType.PointerExit)) {
                AddCallback(trigger, entry, e => { _isOverUIButton = false; });
            }
        }

        private static void AddCallback(EventTrigger trigger, Entry entry, UnityAction<BaseEventData> callback) {
            entry.callback.AddListener(callback);
            AddedListeners[trigger].Add(callback);
        }
    }
}