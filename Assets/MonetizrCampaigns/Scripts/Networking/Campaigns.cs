using Monetizr.SDK.Campaigns;
using System;
using System.Collections.Generic;

namespace Monetizr.SDK.Networking
{
    internal partial class MonetizrHttpClient
    {
        [Serializable]
        private class Campaigns
        {
            public List<ServerCampaign> campaigns;
        }
    }
}