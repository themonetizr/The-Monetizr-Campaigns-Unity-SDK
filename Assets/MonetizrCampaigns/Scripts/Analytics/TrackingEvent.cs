using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using System.Collections.Generic;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.SDK.Analytics
{
    internal class TrackingEvent
    {
        private Mission mission;
        private AdPlacement adPlacement;
        private MonetizrManager.EventType eventType;
        private Dictionary<string, string> additionalValues;

    }

}
