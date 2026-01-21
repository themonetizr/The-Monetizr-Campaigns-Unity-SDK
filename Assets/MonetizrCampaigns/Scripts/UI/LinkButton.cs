using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using UnityEngine;

namespace Monetizr.SDK.UI {
    public class LinkButton : MonoBehaviour
    {
        public string id;

        public void OnClick()
        {
            var campaign = MonetizrInstance.Instance.GetActiveCampaign();
            string url = null;

            if (campaign == null)
                return;

            url = campaign.serverSettings.GetParam("GiveawayEmailEnterNotification.terms_url_text");

            MonetizrInstance.Instance.ShowWebPage(null, new Mission
            {
                campaignId = campaign.id,
                campaign = campaign,
                surveyUrl = url ?? id,
                campaignServerSettings = new SettingsDictionary<string, string>()
            });

#if UNITY_EDITOR_WIN
            Application.OpenURL(url);    
#endif
        }

    }

}