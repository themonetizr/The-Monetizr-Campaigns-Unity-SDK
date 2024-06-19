using System.Collections.Generic;
using System;
using Monetizr.SDK.Campaigns;

namespace Monetizr.SDK.Core
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
            LoadData();

            int deleted = data.campaigns.RemoveAll((LocalCampaignSettings m) =>
                { return m.apiKey != MonetizrManager.Instance.GetCurrentAPIkey(); });

            deleted += data.campaigns.RemoveAll((LocalCampaignSettings m) =>
                { return m.sdkVersion != MonetizrSDKConfiguration.SDKVersion; });

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
                    sdkVersion = MonetizrSDKConfiguration.SDKVersion,
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

        internal void LoadOldAndUpdateNew(List<ServerCampaign> campaigns)
        {
            Load();
            data.campaigns.RemoveAll((LocalCampaignSettings localCampaigns) => campaigns.FindIndex(serverCampaigns => serverCampaigns.id == localCampaigns.campId) < 0);
            campaigns.ForEach(c => AddCampaign(c));
            SaveData();
        }
    }

}