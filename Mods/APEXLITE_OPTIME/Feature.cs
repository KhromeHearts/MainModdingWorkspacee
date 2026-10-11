using HarmonyLib;
using UnityEngine;

namespace Optime {
    internal abstract class Feature<T> : MonoBehaviour where T : Feature<T> {
        // ReSharper disable once StaticMemberInGenericType
        private static bool _enableOnAwake;

        public static Feature<T> Instance { get; private set; }

        protected Harmony Harmony { get; private set; }

        private void Awake() {
            if (Instance != null) {
                enabled = false;

                return;
            }

            Harmony = new Harmony(typeof(T).ToString());
            Instance = this;
            Instance.enabled = _enableOnAwake;
        }

        private void OnEnable() {
            if (Instance == this) {
                TriggerLoad();

                return;
            }

            enabled = false;
        }

        private void OnDisable() {
            if (Instance == this) {
                TriggerUnload();
            }
        }

        public static void SetLoaded(bool value) {
            if (Instance == null) {
                _enableOnAwake = value;
            } else {
                Instance.enabled = value;
            }
        }

        private void TriggerLoad() {
            Harmony.PatchAll(GetType());

            OnLoad();
        }

        private void TriggerUnload() {
            Harmony.UnpatchSelf();

            OnUnload();
        }

        protected virtual void OnLoad() { }

        protected virtual void OnUnload() { }
    }
}