using System;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidManager
    {
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
                MonetizrLogger.Print($"Prebid - onResult: " + vastUrl);
                _onResult?.Invoke(vastUrl ?? "");
            }
        }

        public static void InitializePrebid (string host = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
            if (string.IsNullOrEmpty(host))
                bridge.CallStatic("initPrebid"); // default host
            else
                bridge.CallStatic("initPrebid", host);
#endif
        }

        public static void FetchDemand (string prebidData, string prebidHost, Action<string> onResult)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                var proxy = new CallbackProxy(onResult);
                bridge.CallStatic("fetchDemand", activity, prebidData, prebidHost, proxy);
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid] FetchDemand exception: {ex.Message}");
                onResult?.Invoke("");
            }
#else
            onResult?.Invoke("");
#endif
        }
    }
}
