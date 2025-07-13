using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Monetizr.SDK.Utils
{
    public static class LocalTestCampaignManager
    {
        public static SettingsDictionary<string, string> GetGlobalSettings ()
        {
            TextAsset textFile = Resources.Load<TextAsset>("LocalTestCampaign/global_settings");
            MonetizrLogger.Print("LocalTestCampaign - Global Settings: " + textFile.ToString());
            return new SettingsDictionary<string, string>(MonetizrUtils.ParseContentString(textFile.ToString()));
        }

        public static async Task<List<ServerCampaign>> GetCampaigns ()
        {
            TextAsset textFile = Resources.Load<TextAsset>("LocalTestCampaign/campaigns");
            MonetizrLogger.Print("LocalTestCampaign - Campaigns: " + textFile.ToString());
            Campaigns.Campaigns campaigns = JsonUtility.FromJson<Campaigns.Campaigns>("{\"campaigns\":" + textFile.ToString() + "}");
            if (campaigns == null) return new List<ServerCampaign>();
            campaigns.campaigns = await CampaignManager.Instance.ProcessCampaigns(campaigns.campaigns);
            return campaigns.campaigns;
        }
    }
}