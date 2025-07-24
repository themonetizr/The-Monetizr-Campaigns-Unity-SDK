using Monetizr.SDK.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidManager
    {
        public static void Init(string accountId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
    using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

    using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
    bridge.CallStatic("init", activity, accountId);
    MonetizrLogger.Print("[PrebidTest] PrebidManager Initializing.");
#endif
        }

    }
}