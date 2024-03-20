//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using System.Collections.Generic;
using UnityEngine;
using System;
using Monetizr.SDK.Campaigns;


namespace Monetizr.SDK
{
    [Serializable]
    internal class CampaignsCollection : BaseCollection
    {
        //campaign id and settings
        [SerializeField]
        internal List<LocalCampaignSettings> campaigns =
           new List<LocalCampaignSettings>();

        internal override void Clear()
        {
            campaigns.Clear();
        }

        internal LocalCampaignSettings GetCampaign(string id)
        {
            return campaigns.Find(c => c.campId == id);
        }
    };

}