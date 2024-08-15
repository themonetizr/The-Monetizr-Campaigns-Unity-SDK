using UnityEngine;

namespace Monetizr.SDK.Utils
{
    public static class MobileUtils
    {
        public static bool IsInLandscapeMode()
        {
            return (Screen.width > Screen.height);
        }

        public static float GetDeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
            return diagonalInches;
        }
    }
}