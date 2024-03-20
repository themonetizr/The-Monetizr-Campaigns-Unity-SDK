using System;
using System.Collections.Generic;

namespace Monetizr.SDK
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