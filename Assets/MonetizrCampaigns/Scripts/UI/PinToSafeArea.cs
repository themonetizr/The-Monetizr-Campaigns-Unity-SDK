using Monetizr.SDK.Debug;
using UnityEngine;

namespace Monetizr.SDK.UI {
    public class PinToSafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            lastSafeArea = Screen.safeArea;

            Vector2 anchorMin = lastSafeArea.position / new Vector2(Screen.width, Screen.height);
            Vector2 anchorMax = (lastSafeArea.position + lastSafeArea.size) / new Vector2(Screen.width, Screen.height);

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

    }

}