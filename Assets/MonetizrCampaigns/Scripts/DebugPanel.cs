using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class DebugPanel : PanelController
    {   
        public Button closeButton;
        public Dropdown apiKeysList;

        public Toggle keepLocalData;
        public Toggle serverClaims;
        public Toggle claimIfSkipped;

        public Text versionText;

        static public List<string> keys = null;
        static public Dictionary<string, string> keyNames = null;
        private bool isIDChanged;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this.onComplete = onComplete;
            panelId = id;

            keepLocalData.isOn = MonetizrManager.keepLocalClaimData;
            serverClaims.isOn = MonetizrManager.serverClaimForCampaigns;
            claimIfSkipped.isOn = MonetizrManager.claimForSkippedCampaigns;

            keepLocalData.onValueChanged.AddListener(OnToggleChanged);
            serverClaims.onValueChanged.AddListener(OnToggleChanged);
            claimIfSkipped.onValueChanged.AddListener(OnToggleChanged);

            apiKeysList.ClearOptions();

            //List<string> k2 = new List<string>();

            //for (int i = 0; i < keys.Count; i++)
            //    k2.Add($"{(i+1).ToString()}. {keys[i]}");

            apiKeysList.AddOptions(new List<string>(keyNames.Values));

            UpdateVersionText();

            var k = new List<string>(keyNames.Values);

            apiKeysList.value = k.FindIndex(0, (string v)=>
                {
                    if (!keyNames.ContainsKey(MonetizrManager.Instance.GetCurrentAPIkey()))
                        return false;

                    return v == keyNames[MonetizrManager.Instance.GetCurrentAPIkey()];
                });
        }

        public void UpdateVersionText()
        {
            versionText.text = $"App version: {Application.version} " +
             $"OS: {MonetizrAnalytics.osVersion}\n" +
             $"ADID: {MonetizrAnalytics.advertisingID}\n" +
             $"UserId: {MonetizrManager.Instance._challengesClient.analytics.GetUserId()}\n" +
             $"Limit ad tracking: {MonetizrAnalytics.limitAdvertising}\n" +
             $"Active campaign: {MonetizrManager.Instance.GetActiveCampaign()}";
        }

        public void OnToggleChanged(bool _)
        {
            MonetizrManager.keepLocalClaimData = keepLocalData.isOn;
            MonetizrManager.serverClaimForCampaigns = serverClaims.isOn;
            MonetizrManager.claimForSkippedCampaigns = claimIfSkipped.isOn;
        }

        public void DropdownValueChanged()
        {
            Log.Print("Dropdown: " + apiKeysList.value);

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
            MonetizrManager.Instance._challengesClient.analytics.RandomizeUserId();

            UpdateVersionText();

            isIDChanged = true;
        }

        public void OpenGame()
        {
            //var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            //Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);

            //MonetizrManager.ShowMinigame(null, PanelId.CarMemoryGame, null);
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
            PlayerPrefs.SetString("api_key", keys[apiKeysList.value]);
            PlayerPrefs.Save();

            //MonetizrManager.Instance.CleanRewardsClaims();

            var bundleId = keyNames[keys[apiKeysList.value]];

            if (bundleId.Contains("."))
                MonetizrManager.bundleId = bundleId;

            var changed = MonetizrManager.Instance.ChangeAPIKey(keys[apiKeysList.value]);

            if (isIDChanged && !changed)
                MonetizrManager.Instance.RestartClient();
        }

        //// Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }

}