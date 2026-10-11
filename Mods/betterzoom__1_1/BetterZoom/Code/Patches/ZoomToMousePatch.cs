using HarmonyLib;
using UnityEngine;

namespace BetterZoom.Code.Patches
{
    /// <summary>
    /// Makes scroll-wheel zoom anchor to the mouse cursor rather than the screen centre.
    ///
    /// Patched on zoomToBounds (the per-frame interpolation method) so the position
    /// correction is applied in lock-step with each incremental orthographicSize change.
    /// Both transform.position and _target_position (if present) are updated so that
    /// MoveCamera's own position lerp tracks the corrected target rather than fighting it.
    /// </summary>
    [HarmonyPatch(typeof(MoveCamera), "zoomToBounds")]
    internal static class ZoomToMousePatch
    {
        private static Vector3 _mouseWorldBefore;
        private static float   _sizeBefore;

        // Timestamp of the last raw scroll input, in unscaled real time.
        private static float _lastScrollTime = float.MinValue;

        // How long to keep applying the mouse-anchor correction after scroll input stops.
        // Covers the camera's own inertial/smoothing phase without being so long that a
        // world-log click immediately after scrolling gets pulled toward the cursor.
        private const float ScrollLinger = 0.4f;

        [HarmonyPrefix]
        private static void Prefix()
        {
            // Record the moment raw scroll input arrives so the Postfix linger window
            // starts from the last actual wheel event, not from the animated zoom end.
            if (!Mathf.Approximately(Input.GetAxis("Mouse ScrollWheel"), 0f))
                _lastScrollTime = Time.unscaledTime;

            Camera cam = Camera.main;
            if (cam == null) return;

            _sizeBefore         = cam.orthographicSize;
            _mouseWorldBefore   = cam.ScreenToWorldPoint(Input.mousePosition);
            _mouseWorldBefore.z = 0f;
        }

        [HarmonyPostfix]
        private static void Postfix(MoveCamera __instance)
        {
            // Apply correction only while the zoom is user-driven (scroll wheel) or within
            // the linger window that covers the camera's inertial smoothing phase.
            // Programmatic zoom calls (world log click-to-zoom) arrive outside this window.
            if (Time.unscaledTime - _lastScrollTime > ScrollLinger) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            // No size change this frame — nothing to correct
            if (Mathf.Approximately(cam.orthographicSize, _sizeBefore)) return;

            Vector3 mouseWorldAfter = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldAfter.z = 0f;

            Vector3 delta = _mouseWorldBefore - mouseWorldAfter;
            delta.z = 0f;
            if (delta.sqrMagnitude < 1e-8f) return;

            // Move the transform
            Transform t = ((Component)__instance).transform;
            t.position += delta;

            // Also shift _target_position if MoveCamera lerps position toward a target,
            // so its own smoothing doesn't fight our correction next frame.
            Traverse trav = Traverse.Create(__instance).Field("_target_position");
            if (trav.FieldExists())
            {
                Vector3 tp = trav.GetValue<Vector3>();
                tp += delta;
                trav.SetValue(tp);
            }
        }
    }
}
