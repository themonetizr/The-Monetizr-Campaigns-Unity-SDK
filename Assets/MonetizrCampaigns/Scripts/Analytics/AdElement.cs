using Monetizr.SDK.Campaigns;
using System;

namespace Monetizr.SDK.Analytics
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
