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
        public Button campaignButton;
        public Button developmentButton;

        private bool isCampaignsList = true;

        private new void Awake ()
        {
            base.Awake();
        }

        internal override void PreparePanel (PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            panelId = id;

            keepLocalData.isOn = MonetizrInstance.Instance.keepLocalClaimData;
            serverClaims.isOn = MonetizrInstance.Instance.serverClaimForCampaigns;
            claimIfSkipped.isOn = MonetizrInstance.Instance.claimForSkippedCampaigns;

            keepLocalData.onValueChanged.AddListener(OnToggleChanged);
            serverClaims.onValueChanged.AddListener(OnToggleChanged);
            claimIfSkipped.onValueChanged.AddListener(OnToggleChanged);

            string debugAPIKey = PlayerPrefs.GetString("Debug_MonetizrAPIKey");
            if (!String.IsNullOrEmpty(debugAPIKey))
            {
                bool campaignContainsKey = DebugSettings.campaignKeys.Contains(debugAPIKey);
                isCampaignsList = campaignContainsKey;
                campaignButton.interactable = !campaignContainsKey;
                developmentButton.interactable = campaignContainsKey;
            }

            UpdateAPIKeyList();
            UpdateVersionText();
        }

        public void OnAPIKeyButtonClick (bool isCampaignsList)
        {
            this.isCampaignsList = isCampaignsList;
            campaignButton.interactable = !isCampaignsList;
            developmentButton.interactable = isCampaignsList;
            UpdateAPIKeyList();
        }

        public void UpdateAPIKeyList ()
        {
            apiKeysList.ClearOptions();
            List<string> keyList = new List<string>();
            Dictionary<string, string> keyDictionary = new Dictionary<string, string>();

            if (isCampaignsList)
            {
                keyList = DebugSettings.campaignKeys;
                keyDictionary = DebugSettings.campaignKeyNames;
            }
            else
            {
                keyList = DebugSettings.devKeys;
                keyDictionary = DebugSettings.devKeyNames;
            }

            apiKeysList.AddOptions(new List<string>(keyDictionary.Values));
            List<string> k = new List<string>(keyDictionary.Values);
            apiKeysList.value = k.FindIndex(0, (string v) =>
            {
                if (!keyDictionary.ContainsKey(MonetizrInstance.Instance.GetCurrentAPIkey())) return false;
                return v == keyDictionary[MonetizrInstance.Instance.GetCurrentAPIkey()];
            });
        }

        public void UpdateVersionText()
        {
            versionText.text = $"App version: {Application.version} " +
             $"OS: {MonetizrMobileAnalytics.osVersion} " +
             $"SDK: {MonetizrSettings.SDKVersion}\n" +
             $"ADID: {MonetizrMobileAnalytics.advertisingID}\n" +
             $"UserId: {MonetizrMobileAnalytics.GetUserId()}\n" +
             $"Limit ad tracking: {MonetizrMobileAnalytics.limitAdvertising}\n" +
             $"Active campaign: {MonetizrInstance.Instance.GetActiveCampaign()?.id}";
        }

        public void OnToggleChanged(bool _)
        {
            MonetizrInstance.Instance.keepLocalClaimData = keepLocalData.isOn;
            MonetizrInstance.Instance.serverClaimForCampaigns = serverClaims.isOn;
            MonetizrInstance.Instance.claimForSkippedCampaigns = claimIfSkipped.isOn;
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
            MonetizrMobileAnalytics.RandomizeUserId();
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

        public void OnClosePress()
        {
            ClosePanel();
        }

        internal override void FinalizePanel (PanelId id)
        {
            List<string> keyList = new List<string>();
            Dictionary<string, string> keyDictionary = new Dictionary<string, string>();

            if (isCampaignsList)
            {
                keyList = DebugSettings.campaignKeys;
                keyDictionary = DebugSettings.campaignKeyNames;
            }
            else
            {
                keyList = DebugSettings.devKeys;
                keyDictionary = DebugSettings.devKeyNames;
            }

            string apiKey = keyList[apiKeysList.value];
            PlayerPrefs.SetString("Debug_MonetizrAPIKey", apiKey);
            string bundleId = keyDictionary[apiKey];

            if (bundleId.Contains("."))
            {
                MonetizrManager.bundleId = bundleId;
            }
            else
            {
                MonetizrManager.bundleId = "com.monetizr.landslice";
            }

            PlayerPrefs.SetString("Debug_MonetizrBundleID", MonetizrManager.bundleId);
            PlayerPrefs.Save();

            MonetizrInstance.Instance.ChangeAPIKey(apiKey);
        }

    }

}