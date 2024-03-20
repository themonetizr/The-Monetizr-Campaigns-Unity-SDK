using System;
using System.Collections.Generic;

namespace Monetizr.Campaigns
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