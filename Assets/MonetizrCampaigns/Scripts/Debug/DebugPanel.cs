using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using Monetizr.SDK.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.Debug
{
    internal class DebugPanel : PanelController
    {
        public Button closeButton;
        public Dropdown apiKeysList;
        public Toggle keepLocalData;
        public Toggle serverClaims;
        public Toggle claimIfSkipped;
        public Text versionText;

        private bool isCampaignsList = true;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            panelId = id;

            keepLocalData.isOn = MonetizrManager.keepLocalClaimData;
            serverClaims.isOn = MonetizrManager.serverClaimForCampaigns;
            claimIfSkipped.isOn = MonetizrManager.claimForSkippedCampaigns;

            keepLocalData.onValueChanged.AddListener(OnToggleChanged);
            serverClaims.onValueChanged.AddListener(OnToggleChanged);
            claimIfSkipped.onValueChanged.AddListener(OnToggleChanged);

            UpdateAPIKeyList();
            UpdateVersionText();
        }

        public void OnAPIKeyButtonClick (bool isCampaignsList)
        {
            this.isCampaignsList = isCampaignsList;
            UpdateAPIKeyList();
        }

        public void UpdateAPIKeyList ()
        {
            apiKeysList.ClearOptions();

            if (isCampaignsList)
            {
                apiKeysList.AddOptions(new List<string>(DebugSettings.campaignKeyNames.Values));

                List<string> k = new List<string>(DebugSettings.campaignKeyNames.Values);
                apiKeysList.value = k.FindIndex(0, (string v) =>
                {
                    if (!DebugSettings.campaignKeyNames.ContainsKey(MonetizrInstance.Instance.GetCurrentAPIkey()))
                        return false;

                    return v == DebugSettings.campaignKeyNames[MonetizrInstance.Instance.GetCurrentAPIkey()];
                });
            }
            else
            {
                apiKeysList.AddOptions(new List<string>(DebugSettings.devKeyNames.Values));

                List<string> k = new List<string>(DebugSettings.devKeyNames.Values);
                apiKeysList.value = k.FindIndex(0, (string v) =>
                {
                    if (!DebugSettings.devKeyNames.ContainsKey(MonetizrInstance.Instance.GetCurrentAPIkey()))
                        return false;

                    return v == DebugSettings.devKeyNames[MonetizrInstance.Instance.GetCurrentAPIkey()];
                });
            }
        }

        public void UpdateVersionText()
        {
            versionText.text = $"App version: {Application.version} " +
             $"OS: {MonetizrMobileAnalytics.osVersion} " +
             $"SDK: {MonetizrSettings.SDKVersion}\n" +
             $"ADID: {MonetizrMobileAnalytics.advertisingID}\n" +
             $"UserId: {MonetizrInstance.Instance.ConnectionsClient.Analytics.GetUserId()}\n" +
             $"Limit ad tracking: {MonetizrMobileAnalytics.limitAdvertising}\n" +
             $"Active campaign: {MonetizrInstance.Instance.GetActiveCampaign()?.id}";
        }

        public void OnToggleChanged(bool _)
        {
            MonetizrManager.keepLocalClaimData = keepLocalData.isOn;
            MonetizrManager.serverClaimForCampaigns = serverClaims.isOn;
            MonetizrManager.claimForSkippedCampaigns = claimIfSkipped.isOn;
        }

        public void DropdownValueChanged()
        {
            MonetizrLogger.Print("Dropdown: " + apiKeysList.value);

        }

        public void ResetLocalClaimData()
        {
            MonetizrInstance.Instance.CleanRewardsClaims();
        }

        public void ResetCampaigns()
        {
            MonetizrInstance.Instance.ResetCampaign();
        }

        public void ResetId()
        {
            MonetizrInstance.Instance.ConnectionsClient.Analytics.RandomizeUserId();
            UpdateVersionText();
        }

        public void TurnOnOffColorBg()
        {
            GameObject obj = GameObject.Find("KeyBackgroundCanvas");
            obj.GetComponent<Canvas>().enabled = !obj.GetComponent<Canvas>().enabled;
        }
        
        private void ClosePanel()
        {
            SetActive(false);
        }

        private new void Awake()
        {
            base.Awake();
        }

        public void OnClosePress()
        {
            ClosePanel();
        }

        internal override void FinalizePanel(PanelId id)
        {
            if (isCampaignsList)
            {
                PlayerPrefs.SetString("MonetizrAPIKey", DebugSettings.campaignKeys[apiKeysList.value]);
                PlayerPrefs.Save();
                string bundleId = DebugSettings.keyNames[DebugSettings.campaignKeys[apiKeysList.value]];

                if (bundleId.Contains("."))
                {
                    MonetizrManager.bundleId = bundleId;
                }
                else
                {
                    MonetizrManager.bundleId = "com.monetizr.landslice";
                }

                bool changed = MonetizrInstance.Instance.ChangeAPIKey(DebugSettings.campaignKeys[apiKeysList.value]);
                MonetizrInstance.Instance.RestartClient();
            }
            else
            {
                PlayerPrefs.SetString("MonetizrAPIKey", DebugSettings.devKeys[apiKeysList.value]);
                PlayerPrefs.Save();
                string bundleId = DebugSettings.keyNames[DebugSettings.devKeys[apiKeysList.value]];

                if (bundleId.Contains("."))
                {
                    MonetizrManager.bundleId = bundleId;
                }
                else
                {
                    MonetizrManager.bundleId = "com.monetizr.landslice";
                }

                bool changed = MonetizrInstance.Instance.ChangeAPIKey(DebugSettings.devKeys[apiKeysList.value]);
                MonetizrInstance.Instance.RestartClient();
            }
        }

    }

}