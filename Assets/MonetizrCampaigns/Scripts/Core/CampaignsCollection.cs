using System.Collections.Generic;
using UnityEngine;
using System;
using Monetizr.SDK.Campaigns;

namespace Monetizr.SDK.Core
{
    [Serializable]
    internal class CampaignsCollection : BaseCollection
    {
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