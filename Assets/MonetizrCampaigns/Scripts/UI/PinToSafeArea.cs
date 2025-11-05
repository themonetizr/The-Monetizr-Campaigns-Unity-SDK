using Monetizr.SDK.Debug;
using UnityEngine;

namespace Monetizr.SDK.UI {
    public class PinToSafeArea : MonoBehaviour
    {
        private RectTransform _panel;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;

        private void Awake ()
        {
            _panel = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update ()
        {
            if (_lastSafeArea != Screen.safeArea || _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea ()
        {
            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _panel.anchorMin = anchorMin;
            _panel.anchorMax = anchorMax;
            _panel.offsetMin = Vector2.zero;
            _panel.offsetMax = Vector2.zero;
        }

    }

}