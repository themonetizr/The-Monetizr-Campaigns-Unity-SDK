using Monetizr.Campaigns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    public string id;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void OnClick()
    {
        var campaign = MonetizrManager.Instance.GetActiveCampaign();
        string url = null;

        if (campaign == null)
            return;

        url = campaign.serverSettings.GetParam("GiveawayEmailEnterNotification.terms_url_text");
        
        MonetizrManager.ShowWebPage(null, new Mission
        {
                campaignId = campaign.id,
                campaign = campaign,
                surveyUrl = url ?? id,
                campaignServerSettings = new SettingsDictionary<string, string>()
        });

#if UNITY_EDITOR_WIN
        Application.OpenURL(url);
#else
        
#endif
    }
}
