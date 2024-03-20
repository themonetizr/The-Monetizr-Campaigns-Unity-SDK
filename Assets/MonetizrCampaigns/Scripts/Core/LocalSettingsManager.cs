//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using System.Collections.Generic;
using UnityEngine;
using System;


namespace Monetizr.SDK
{
    internal class LocalSettingsManager : LocalSerializer<CampaignsCollection>
    {
        internal LocalSettingsManager()
        {
            data = new CampaignsCollection();
        }

        internal override string GetDataKey()
        {
            return "campaigns";
        }

        internal void Load()
        {
            //ResetData();

            LoadData();

            int deleted = data.campaigns.RemoveAll((LocalCampaignSettings m) =>
                { return m.apiKey != MonetizrManager.Instance.GetCurrentAPIkey(); });

            deleted += data.campaigns.RemoveAll((LocalCampaignSettings m) =>
                { return m.sdkVersion != MonetizrManager.SDKVersion; });

            if (deleted > 0)
            {
                SaveData();
            }
        }

        internal void AddCampaign(ServerCampaign campaign)
        {
            var camp = data.GetCampaign(campaign.id);

            if (camp == null)
            {
                data.campaigns.Add(new LocalCampaignSettings()
                {
                    apiKey = MonetizrManager.Instance.GetCurrentAPIkey(),
                    sdkVersion = MonetizrManager.SDKVersion,
                    lastTimeShowNotification = DateTime.Now,
                    campId = campaign.id
                });
            }
        }

        internal LocalCampaignSettings GetSetting(string campaign)
        {
            var camp = data.GetCampaign(campaign);

            UnityEngine.Debug.Assert(camp != null);

            return camp;
        }

        /*internal void LoadOldAndUpdateNew(Dictionary<String, ServerCampaign> campaigns)
        {
            //load old settings
            //сheck if apikey/sdkversion is old
            Load();

            //check if campaign is missing - remove it from data
            data.campaigns.RemoveAll((LocalCampaignSettings c) => !campaigns.ContainsKey(c.campId));

            //add empty campaign into settings
            campaigns.Values.ToList().ForEach(c => AddCampaign(c));

            SaveData();
        }*/

        internal void LoadOldAndUpdateNew(List<ServerCampaign> campaigns)
        {
            //load old settings
            //сheck if apikey/sdkversion is old
            Load();

            //check if campaign is missing - remove it from data
            data.campaigns.RemoveAll((LocalCampaignSettings localCampaigns) =>
                campaigns.FindIndex(serverCampaigns => serverCampaigns.id == localCampaigns.campId) < 0);

            //add empty campaign into settings
            campaigns.ForEach(c => AddCampaign(c));

            SaveData();
        }
    }

}