using System;
using System.Collections.Generic;

namespace PossessLib.Core
{
    /// <summary>Short on-screen messages. Systems queue them here; the HUD's ToastView displays them.</summary>
    internal static class Toasts
    {
        private static readonly Queue<string> _pending = new Queue<string>();
        private static string _last;
        private static float _lastTime;

        public static void Show(string pText)
        {
            if (string.IsNullOrEmpty(pText)) return;
            float now = UnityEngine.Time.unscaledTime;
            if (pText == _last && now - _lastTime < 0.75f) return; // no spam when a key is held
            _last = pText;
            _lastTime = now;
            if (_pending.Count > 8) _pending.Dequeue();
            _pending.Enqueue(pText);
        }

        public static bool TryDequeue(out string pText)
        {
            if (_pending.Count == 0)
            {
                pText = null;
                return false;
            }
            pText = _pending.Dequeue();
            return true;
        }
    }

    /// <summary>UI sounds, using event paths the game itself plays (MusicBox.playSoundUI).</summary>
    internal static class UISound
    {
        public const string PickUp = "event:/SFX/UI/SmallKnob";
        public const string Place = "event:/SFX/UI/SmallKnob";
        public const string Equip = "event:/SFX/UI/MediumlKnob";
        public const string Open = "event:/SFX/UI/TabKnobOn";
        public const string Close = "event:/SFX/UI/TabKnobOff";
        public const string Select = "event:/SFX/UI/SimpleHighlight";
        public const string Craft = "event:/SFX/UI/YesPoint";
        public const string Deny = "event:/SFX/UI/NoPoint";
        public const string Trash = "event:/SFX/UI/LargeKnob";

        public static void Play(string pEvent)
        {
            try { MusicBox.playSoundUI(pEvent); }
            catch (Exception) { /* sound is cosmetic */ }
        }
    }
}
