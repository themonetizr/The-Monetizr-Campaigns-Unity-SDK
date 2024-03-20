using System;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.SDK
{
    internal class AdElement
    {
        internal AdPlacement placement;
        internal ServerCampaign campaign;
        internal DateTime activateTime;
        internal string eventName;
        
        internal AdElement(string eventName, AdPlacement placement, ServerCampaign campaign)
        {
            this.eventName = eventName;
            this.placement = placement;
            this.campaign = campaign;
            this.activateTime = DateTime.Now;
        }
    }
}
