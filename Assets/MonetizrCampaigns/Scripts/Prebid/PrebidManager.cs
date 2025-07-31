using System;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidManager
    {
        private class PrebidListener : AndroidJavaProxy
        {
            private readonly Action<string> _onComplete;

            public PrebidListener(Action<string> onComplete)
                : base("com.monetizr.prebidbridge.PrebidBridge$UnityCallback")
            {
                _onComplete = onComplete;
            }

            public void onKeywordsReady(string json)
            {
                MonetizrLogger.Print($"[PrebidManager] Keywords received: {json}");
                _onComplete?.Invoke(json);
            }
        }

        public static void FetchDemand (string jsonConfig, Action<string> onKeywordsReady)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                var listener = new PrebidListener(onKeywordsReady);

                MonetizrLogger.Print("[PrebidManager] Sending config to Java bridge...");
                bridge.CallStatic("fetchDemand", activity, jsonConfig, listener);
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[PrebidManager] Exception: {ex.Message}");
                onKeywordsReady?.Invoke("{}");
            }
#else
            MonetizrLogger.PrintWarning("[PrebidManager] Skipped on non-Android platform.");
            onKeywordsReady?.Invoke("{}");
#endif
        }
    }
}
