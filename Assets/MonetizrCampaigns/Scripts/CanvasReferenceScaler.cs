using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Monetizr.Campaigns
{
    public class CanvasReferenceScaler : MonoBehaviour
    {
        private CanvasScaler cs = null;
        private float aspect = 0;
        private Vector2 initialRefRes;
        private ScreenOrientation _orientation;
        private Vector2Int _displaySize;


        private void UpdatePortrait()
        {
            float aspect = (float)Screen.height / (float)Screen.width;

            if (aspect >= 1.777)
            {
                cs.referenceResolution = new Vector2(initialRefRes.x, initialRefRes.x * aspect);
            }
            else
            {
                cs.matchWidthOrHeight = 1;
            }
        }

        private void UpdateLandscape()
        {
            float aspect = (float)Screen.width/ (float)Screen.height;

            //if (aspect >= 1.777)
            //{
                cs.referenceResolution = new Vector2(initialRefRes.x*aspect, initialRefRes.x);
            //}
            //else
            //{
            //    cs.matchWidthOrHeight = 1;
            //}
        }

        private void Start()
        {
            _displaySize = new Vector2Int(Screen.width,Screen.height);
            cs = gameObject.GetComponent<CanvasScaler>();
            initialRefRes = cs.referenceResolution;

            if (Utils.isInLandscapeMode())
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
            Log.Print("Orientation changed!");
            
            if (Utils.isInLandscapeMode())
                UpdateLandscape();
            else
                UpdatePortrait();
        }

        public Vector2 GetScreenReferenceResolution()
        {
            return cs.referenceResolution;
        }

    }
}