using PossessLib.Model;
using PossessLib.Sync;
using UnityEngine;
using UnityEngine.UI;

namespace PossessLib.UI
{
    /// <summary>Draws the held stack under the mouse. Lives on the tooltip canvas so it is above windows too.</summary>
    internal sealed class CursorView : MonoBehaviour
    {
        private const float Size = 18f;
        private static CursorView _instance;
        private RectTransform _rect;
        private RectTransform _canvasRect;
        private Canvas _canvas;
        private Image _icon;
        private Text _count;
        private int _version = -1;

        public static void Ensure()
        {
            if (_instance != null || CanvasMain.instance == null || CanvasMain.instance.canvas_tooltip == null) return;
            Canvas canvas = CanvasMain.instance.canvas_tooltip;
            RectTransform root = UIFactory.Rect("PossessLib_Cursor", canvas.transform);
            UIFactory.Place(root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size, Size));
            _instance = root.gameObject.AddComponent<CursorView>();
            _instance._rect = root;
            _instance._canvas = canvas;
            _instance._canvasRect = (RectTransform)canvas.transform;
            _instance._icon = UIFactory.Image("Icon", root, null, Color.white);
            _instance._icon.preserveAspect = true;
            UIFactory.Stretch(_instance._icon.rectTransform);
            _instance._count = UIFactory.Text("Count", root, UIConst.FontNormal, TextAnchor.LowerRight, Color.white);
            UIFactory.Stretch((RectTransform)_instance._count.transform, 0f, -2f, -3f, 0f);
            _instance._icon.enabled = false;
            _instance._count.text = string.Empty;
        }

        private void LateUpdate()
        {
            ItemStack stack = CursorState.Stack;
            if (CursorState.Version != _version)
            {
                _version = CursorState.Version;
                Sprite icon = stack != null ? stack.GetIcon() : null;
                _icon.sprite = icon;
                _icon.enabled = icon != null;
                _count.text = stack != null && stack.Count > 1 ? stack.Count.ToString() : string.Empty;
            }
            if (stack == null) return;

            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, Input.mousePosition, cam, out Vector2 local))
            {
                _rect.anchoredPosition = local + new Vector2(Size * 0.45f, -Size * 0.45f);
            }
            _rect.SetAsLastSibling();
        }
    }
}
