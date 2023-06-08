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
            cs = gameObject.GetComponent<CanvasScaler>();
            initialRefRes = cs.referenceResolution;

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