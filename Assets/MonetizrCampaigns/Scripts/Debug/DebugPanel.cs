using Monetizr.SDK.Analytics;
using Monetizr.SDK.Missions;
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

        private bool isIDChanged;

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

            apiKeysList.ClearOptions();

            apiKeysList.AddOptions(new List<string>(DebugSettings.keyNames.Values));

            UpdateVersionText();

            var k = new List<string>(DebugSettings.keyNames.Values);

            apiKeysList.value = k.FindIndex(0, (string v) =>
                {
                    if (!DebugSettings.keyNames.ContainsKey(MonetizrManager.Instance.GetCurrentAPIkey()))
                        return false;

                    return v == DebugSettings.keyNames[MonetizrManager.Instance.GetCurrentAPIkey()];
                });
        }

        public void UpdateVersionText()
        {
            versionText.text = $"App version: {Application.version} " +
             $"OS: {MonetizrMobileAnalytics.osVersion} " +
             $"SDK: {MonetizrManager.SDKVersion}\n" +
             $"ADID: {MonetizrMobileAnalytics.advertisingID}\n" +
             $"UserId: {MonetizrManager.Instance.ConnectionsClient.Analytics.GetUserId()}\n" +
             $"Limit ad tracking: {MonetizrMobileAnalytics.limitAdvertising}\n" +
             $"Active campaign: {MonetizrManager.Instance.GetActiveCampaign()?.id}";
        }

        public void OnToggleChanged(bool _)
        {
            MonetizrManager.keepLocalClaimData = keepLocalData.isOn;
            MonetizrManager.serverClaimForCampaigns = serverClaims.isOn;
            MonetizrManager.claimForSkippedCampaigns = claimIfSkipped.isOn;
        }

        public void DropdownValueChanged()
        {
            Log.PrintV("Dropdown: " + apiKeysList.value);

        }
        public void ResetLocalClaimData()
        {
            MonetizrManager.Instance.CleanRewardsClaims();
        }

        public void ResetCampaigns()
        {
            MonetizrManager.ResetCampaign();
        }

        public void ResetId()
        {
            MonetizrManager.Instance.ConnectionsClient.Analytics.RandomizeUserId();

            UpdateVersionText();

            isIDChanged = true;
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
            PlayerPrefs.SetString("api_key", DebugSettings.keys[apiKeysList.value]);
            PlayerPrefs.Save();

            //MonetizrManager.Instance.CleanRewardsClaims();

            var bundleId = DebugSettings.keyNames[DebugSettings.keys[apiKeysList.value]];

            if (bundleId.Contains("."))
            {
                MonetizrManager.bundleId = bundleId;
            }
            else
            {
                MonetizrManager.bundleId = "com.monetizr.landslice";
            }

            var changed = MonetizrManager.Instance.ChangeAPIKey(DebugSettings.keys[apiKeysList.value]);

            //if (isIDChanged || changed)
            MonetizrManager.Instance.RestartClient();
        }
    }
}