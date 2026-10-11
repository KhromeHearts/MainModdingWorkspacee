using System;

namespace PossessLib.Core
{
    /// <summary>Logging that goes through NML (prefixed with the mod name) with a debug-only channel.</summary>
    internal static class PLLog
    {
        public static void Info(string pMessage)
        {
            try { PossessLibMod.LogInfo(pMessage); }
            catch { UnityEngine.Debug.Log("[PossessLib] " + pMessage); }
        }

        public static void Warn(string pMessage)
        {
            try { PossessLibMod.LogWarning(pMessage); }
            catch { UnityEngine.Debug.LogWarning("[PossessLib] " + pMessage); }
        }

        public static void Error(string pMessage)
        {
            try { PossessLibMod.LogError(pMessage); }
            catch { UnityEngine.Debug.LogError("[PossessLib] " + pMessage); }
        }

        public static void Exception(string pContext, Exception pException)
        {
            Error(pContext + ": " + pException);
        }

        /// <summary>Only printed when the "pl_debug" config switch is on.</summary>
        public static void Debug(string pMessage)
        {
            if (PLConfig.Debug) Info("[debug] " + pMessage);
        }
    }
}
