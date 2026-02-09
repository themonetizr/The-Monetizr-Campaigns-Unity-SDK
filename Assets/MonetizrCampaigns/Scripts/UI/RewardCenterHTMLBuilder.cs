using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RewardCenterHTMLBuilder
{
    [Serializable]
    private class RewardCenterModel
    {
        public string campaignId;
        public List<MissionEntry> missions = new List<MissionEntry>();
    }

    [Serializable]
    private class MissionEntry
    {
        public int serverId;
        public string type;
    }

    public static string BuildHTML (ServerCampaign campaign, string htmlTemplateIndexPath)
    {
        if (campaign == null)
        {
            MonetizrLogger.PrintError($"Campaign is null.");
            return null;
        }

        if (string.IsNullOrEmpty(htmlTemplateIndexPath))
        {
            MonetizrLogger.PrintError($"HTML Template is null.");
            return null;
        }

        if (!File.Exists(htmlTemplateIndexPath))
        {
            MonetizrLogger.PrintError($"Index.html not found: {htmlTemplateIndexPath}");
            return null;
        }

        List<Mission> missions = MonetizrInstance.Instance.missionsManager.GetAllMissions(campaign);
        if (missions == null) missions = new List<Mission>();

        RewardCenterModel model = new RewardCenterModel { campaignId = campaign.id };
        foreach (Mission m in missions)
        {
            model.missions.Add(new MissionEntry
            {
                serverId = m.serverId,
                type = m.type.ToString()
            });
        }

        string modelJson = JsonUtility.ToJson(model);
        string html = File.ReadAllText(htmlTemplateIndexPath);
        const string placeholder = "${MNTZR_MODEL_JSON}";

        if (!html.Contains(placeholder))
        {
            MonetizrLogger.PrintError($"RewardCenterHTMLBuilder: placeholder {placeholder} not found in template. Add it to index.html.");
            return null;
        }

        html = html.Replace(placeholder, modelJson);
        File.WriteAllText(htmlTemplateIndexPath, html);
        return htmlTemplateIndexPath;
    }
}
