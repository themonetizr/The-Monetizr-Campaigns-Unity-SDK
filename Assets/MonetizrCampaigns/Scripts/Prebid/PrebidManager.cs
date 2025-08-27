using System;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidManager
    {
        public static string testData = "prebid-demo-video-interstitial-320-480-original-api";
        public static string defaultHost = "https://rtb.monetizr.com/openrtb2/auction";

        private class CallbackProxy : AndroidJavaProxy
        {
            private readonly Action<string> _onResult;

            public CallbackProxy (Action<string> onResult)
                : base("com.monetizr.prebidbridge.PrebidBridge$UnityCallback")
            {
                _onResult = onResult;
            }

            public void onResult (string vastUrl)
            {
                MonetizrLogger.Print($"[Prebid] onResult (len={vastUrl?.Length ?? 0})");
                _onResult?.Invoke(vastUrl ?? "");
            }
        }

        public static void InitializePrebid (string hostUrl)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
            bridge.CallStatic("init", activity, hostUrl);
#endif
        }

        public static void FetchDemand (string storedRequestId, Action<string> onResult)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                var proxy = new CallbackProxy(onResult);
                bridge.CallStatic("fetchDemand", activity, storedRequestId, proxy);
            }
            catch (Exception ex)
            {
                onResult?.Invoke("");
            }
#else
            onResult?.Invoke("");
#endif
        }
    }
}
