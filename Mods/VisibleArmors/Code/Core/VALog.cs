using System;
using Time = UnityEngine.Time;

namespace VisibleArmors.Core
{
    /// <summary>Logging through NML (prefixed with the mod name), plus a debug-only channel and a rate-limited error channel.</summary>
    internal static class VALog
    {
        private static float _nextErrorTime;
        private static int _suppressed;

        public static void Info(string pMessage)
        {
            try { VisibleArmorsMod.LogInfo(pMessage); }
            catch { UnityEngine.Debug.Log("[VisibleArmors] " + pMessage); }
        }

        public static void Warn(string pMessage)
        {
            try { VisibleArmorsMod.LogWarning(pMessage); }
            catch { UnityEngine.Debug.LogWarning("[VisibleArmors] " + pMessage); }
        }

        public static void Error(string pMessage)
        {
            try { VisibleArmorsMod.LogError(pMessage); }
            catch { UnityEngine.Debug.LogError("[VisibleArmors] " + pMessage); }
        }

        /// <summary>Only printed when the "va_debug" config switch is on.</summary>
        public static void Debug(string pMessage)
        {
            if (VAConfig.Debug) Info("[debug] " + pMessage);
        }

        /// <summary>
        /// Logs an exception from a render-path guard at most every two seconds, so one broken unit or item
        /// cannot flood the log at 60 frames per second.
        /// </summary>
        public static void GuardedException(string pContext, Exception pException)
        {
            float now = Time.realtimeSinceStartup;
            if (now < _nextErrorTime)
            {
                _suppressed++;
                return;
            }
            _nextErrorTime = now + 2f;
            string suffix = _suppressed > 0 ? " (" + _suppressed + " similar errors suppressed)" : string.Empty;
            _suppressed = 0;
            Error(pContext + ": " + pException + suffix);
        }
    }
}
