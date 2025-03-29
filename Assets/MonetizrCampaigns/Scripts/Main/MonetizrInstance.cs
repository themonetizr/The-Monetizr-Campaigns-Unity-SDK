using System;
using UnityEngine;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Networking;
using static Monetizr.SDK.Core.MonetizrManager;
using Monetizr.SDK.Missions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Monetizr.SDK
{
    public class MonetizrInstance : MonoBehaviour
    {
        public static MonetizrInstance Instance;

        private UserDefinedEvent userEvent = null;
        private Action<bool> onUIVisible = null;

        private NetworkManager networkManager;
        private CampaignManager campaignManager;
        private GCPManager gcpManager;

        private SettingsDictionary<string, string> globalSettings = new SettingsDictionary<string, string>();
        private List<ServerCampaign> campaigns = new List<ServerCampaign>();

        private void Awake ()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void Setup (Action<bool> onUIVisible, UserDefinedEvent userEvent)
        {
            this.userEvent = userEvent;
            this.onUIVisible = onUIVisible;

            networkManager = gameObject.AddComponent<NetworkManager>();
            campaignManager = gameObject.AddComponent<CampaignManager>();
            gcpManager = gameObject.AddComponent<GCPManager>();

            networkManager.Setup(MonetizrSettings.apiKey);
        }

        public async void StartSDKProcess ()
        {
            globalSettings = await networkManager.GetGlobalSettings();
            campaigns = await networkManager.GetCampaigns();
            campaignManager.ProcessCampaigns(campaigns);
            if (campaigns.Count <= 0)
            {
                MonetizrLogger.Print("No campaigns were successfully processed.");
                return;
            }

        }

    }
}