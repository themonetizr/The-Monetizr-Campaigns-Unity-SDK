using Monetizr.SDK;
using System.Collections.Generic;

namespace MonetizrCampaigns.Tests
{
    internal class MonetizrTestAnalytics : MonetizrAnalytics
    {
        internal override string GetUserId()
        {
            return "test_user_id";
        }

        internal override void RandomizeUserId()
        {

        }

        internal override void SendErrorToMixpanel(string condition, string stackTrace, ServerCampaign getActiveCampaign)
        {

        }

        internal override void SendOpenRtbReportToMixpanel(string openRtbRequest, string status, string openRtbResponse, ServerCampaign campaign)
        {

        }

        internal override void Initialize(bool testEnvironment, string mixPanelApiKey, bool logConnectionErrors)
        {

        }

        internal override void TrackEvent(Mission mission, PanelController panel, MonetizrManager.EventType eventType, Dictionary<string, string> additionalValues = null)
        {

        }

        internal override void TrackEvent(Mission currentMission, AdPlacement adPlacement, MonetizrManager.EventType eventType,
            Dictionary<string, string> additionalValues = null)
        {

        }

        internal override void TrackEvent(ServerCampaign currentCampaign, Mission currentMission, AdPlacement adPlacement, MonetizrManager.EventType eventType,
            Dictionary<string, string> additionalValues = null)
        {

        }

        internal override void _TrackEvent(string name, ServerCampaign campaign, bool timed = false, Dictionary<string, string> additionalValues = null,
            double duration = -1)
        {

        }

        internal override void OnApplicationQuit()
        {
            
        }
    }
}