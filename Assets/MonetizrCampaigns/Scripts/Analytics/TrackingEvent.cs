using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using System.Collections.Generic;

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
