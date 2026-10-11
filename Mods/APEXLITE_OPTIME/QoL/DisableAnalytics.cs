namespace Optime.QoL {
    internal sealed class DisableAnalytics : Feature<DisableAnalytics> {
        public new static void SetLoaded(bool value) {
            Feature<DisableAnalytics>.SetLoaded(value);
        }

        protected override void OnLoad() {
            Config.firebaseEnabled = false;
        }

        protected override void OnUnload() {
            Config.firebaseEnabled = true;
        }
    }
}