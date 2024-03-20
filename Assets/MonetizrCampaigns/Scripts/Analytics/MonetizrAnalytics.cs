using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using Monetizr.SDK.UI;
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
    internal abstract class MonetizrAnalytics
    {
        internal abstract string GetUserId();
        internal abstract void RandomizeUserId();
        internal abstract void SendErrorToMixpanel(string condition, string stackTrace, ServerCampaign getActiveCampaign);
        internal abstract void SendOpenRtbReportToMixpanel(string openRtbRequest, string status, string openRtbResponse, ServerCampaign campaign);
        internal abstract void Initialize(bool testEnvironment, string mixPanelApiKey, bool logConnectionErrors);
        internal abstract void TrackEvent(Mission mission, 
            PanelController panel,
            MonetizrManager.EventType eventType, 
            Dictionary<string, string> additionalValues = null);

        internal abstract void TrackEvent(Mission currentMission, 
            AdPlacement adPlacement,
            MonetizrManager.EventType eventType, 
            Dictionary<string, string> additionalValues = null);

        internal abstract void TrackEvent(ServerCampaign currentCampaign, 
            Mission currentMission,
            AdPlacement adPlacement, 
            MonetizrManager.EventType eventType,
            Dictionary<string, string> additionalValues = null);

        internal abstract void _TrackEvent(string name,
            ServerCampaign campaign,
            bool timed = false,
            Dictionary<string, string> additionalValues = null,
            double duration = -1.0);

        internal abstract void OnApplicationQuit();
       
    }
}
