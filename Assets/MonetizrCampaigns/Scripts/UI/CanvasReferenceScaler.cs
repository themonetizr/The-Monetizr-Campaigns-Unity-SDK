using UnityEngine;
using UnityEngine.UI;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.UI
{
    public class CanvasReferenceScaler : MonoBehaviour
    {
        private CanvasScaler _cs = null;
        private Vector2 _initialRefRes;
        private ScreenOrientation _orientation;
        private Vector2Int _displaySize;

        private void UpdatePortrait()
        {
            float aspect = (float)Screen.height / (float)Screen.width;

            if (aspect >= 1.777)
            {
                _cs.referenceResolution = new Vector2(_initialRefRes.x, _initialRefRes.x * aspect);
            }
            else
            {
                _cs.matchWidthOrHeight = 1;
            }
        }

        private void UpdateLandscape()
        {
            float aspect = (float)Screen.width/ (float)Screen.height;
            _cs.referenceResolution = new Vector2(_initialRefRes.x*aspect, _initialRefRes.x);
        }

        private void Start()
        {
            _displaySize = new Vector2Int(Screen.width,Screen.height);
            _cs = gameObject.GetComponent<CanvasScaler>();
            _initialRefRes = _cs.referenceResolution;

            if (MonetizrUtils.IsInLandscapeMode())
                UpdateLandscape();
            else
                UpdatePortrait();
            
            InvokeRepeating("CheckForChange", 1, 1);
        }
        
        private void CheckForChange()
        {
            if (_displaySize.x != Screen.width || _displaySize.y != Screen.height) {
                _displaySize = new Vector2Int(Screen.width,Screen.height);
                OnOrientationChanged(_orientation);
            }
        }

        private void OnOrientationChanged(ScreenOrientation orientation)
        {
            Log.PrintV("Orientation changed!");
            
            if (MonetizrUtils.IsInLandscapeMode())
                UpdateLandscape();
            else
                UpdatePortrait();
        }

        public Vector2 GetScreenReferenceResolution()
        {
            return _cs.referenceResolution;
        }

    }

}