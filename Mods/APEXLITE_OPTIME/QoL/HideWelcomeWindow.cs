namespace Optime.QoL {
    internal sealed class HideWelcomeWindow : Feature<HideWelcomeWindow> {
        public new static void SetLoaded(bool value) {
            Feature<HideWelcomeWindow>.SetLoaded(value);
        }

        protected override void OnLoad() {
            Config.disable_startup_window = true;
        }

        protected override void OnUnload() {
            Config.disable_startup_window = false;
        }
    }
}