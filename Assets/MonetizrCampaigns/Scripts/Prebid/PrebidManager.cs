using System;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidManager
    {
        public static string testJson =
            "{\"pbsHost\":\"https://rtb.monetizr.com/openrtb2/auction\"," +
            "\"inlineSeatbidObj\":{\"bid\":[{\"h\":640,\"w\":480,\"id\":\"044d8818-4377-452c-ac69-f0c66d4e50df\"," +
            "\"adm\":\"<VAST version=\\\"2.0\\\"><Ad id=\\\"wrapped-ad\\\"><Wrapper><AdSystem>Monetizr AdWrapper</AdSystem><VASTAdTagURI><![CDATA[https://api.themonetizr.com/vast?ca=b89c8d8f-a4e5-42dd-8d48-062e124b951a&abi=&sv=&pi=&key=K8wiuvZD1ogk6qHFY2sjVnplRztovm8pUuFHktTlXNA&od=ORDER-MJ=&do=&og=&dp=&dm=&dmo=&dn=&ic=&sd=&sh=&sw=&ua=&v=4.0]]></VASTAdTagURI></Wrapper></Ad></VAST>\"," +
            "\"ext\":{\"prebid\":{\"type\":\"video\"}},\"crid\":\"11111\",\"impid\":\"##PBSIMPID##\",\"price\":1.0}]," +
            "\"seat\":\"testbide\",\"group\":0},\"width\":768,\"height\":1024,\"mimes\":[\"video/mp4\"]}";

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

        public static void FetchDemand (string jsonConfig, Action<string> onResult)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                var proxy = new CallbackProxy(onResult);
                bridge.CallStatic("fetchStoredVastAsync", activity, jsonConfig, proxy);
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid] FetchDemand exception: {ex.Message}");
                onResult?.Invoke("");
            }
#else
            MonetizrLogger.PrintWarning("[Prebid] FetchDemand skipped (not Android).");
            onResult?.Invoke("");
#endif
        }
    }
}
