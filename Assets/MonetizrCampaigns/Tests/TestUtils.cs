using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Utils;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.Tests
{
    public static class TestUtils
    {
        public static Sprite CreateMockSprite ()
        {
            int measure = 256;
            Texture2D texture = new Texture2D(measure, measure);
            Color[] pixels = new Color[measure * measure];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, measure, measure), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        public static UnityWebRequest GetUnityWebRequest(string uri, bool isPost = false)
        {
            UnityWebRequest request = isPost ? UnityWebRequest.Post(uri, "") : UnityWebRequest.Get(uri);

            request.SetRequestHeader("player-id", MonetizrMobileAnalytics.deviceIdentifier);
            request.SetRequestHeader("app-bundle-id", MonetizrManager.bundleId);
            request.SetRequestHeader("sdk-version", MonetizrSDKConfiguration.SDKVersion);
            request.SetRequestHeader("os-group", MonetizrMobileAnalytics.GetOsGroup());
            request.SetRequestHeader("ad-id", MonetizrMobileAnalytics.advertisingID);
            request.SetRequestHeader("screen-width", Screen.width.ToString());
            request.SetRequestHeader("screen-height", Screen.height.ToString());
            request.SetRequestHeader("screen-dpi", Screen.dpi.ToString(CultureInfo.InvariantCulture));
            request.SetRequestHeader("device-group", MonetizrMobileAnalytics.GetDeviceGroup().ToString().ToLower());
            request.SetRequestHeader("device-memory", SystemInfo.systemMemorySize.ToString());
            request.SetRequestHeader("device-model", MonetizrUtils.EncodeStringIntoAscii(SystemInfo.deviceModel));
            request.SetRequestHeader("device-name", MonetizrUtils.EncodeStringIntoAscii(SystemInfo.deviceName));
            request.SetRequestHeader("internet-connection", MonetizrMobileAnalytics.GetInternetConnectionType());
            request.SetRequestHeader("local-time-stamp", ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString());
            request.SetRequestHeader("Authorization", "Bearer " + MonetizrManager.Instance.ConnectionsClient.currentApiKey);
            request.SetRequestHeader("Accept", "application/json");

            return request;
        }

    }
}