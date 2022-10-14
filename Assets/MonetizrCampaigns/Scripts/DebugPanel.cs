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
      
        static public readonly List<string> keys = new List<string>()
        {
            "zcJgPFt_Pe4XIs7aqbZbbN2KRcJziAWkRFzYYo7qVdE",
            "4vdWpekbjsTcZF8EJFOSD5nzC82GL4NFrzY93KfUiGU", //design@monetizr.io ?
            //"9-JosxHvT8ds9H0A3SOcOSSQl25yab5vSBItAlY6ags", //andris
            //"PUHzF8UQLXJUuaW0vX0D0lTAFlWU2G0J2NaN2SHk6AA", //martins.jansevskis@themonetizr.com 
            //"oRE6-DIXqfHgoU5TEohXycVkthRv2Tt3pG8hG8q8O9U", 

            //"XgmYrf0Hki-slLhzYyIbfAoDaYDt-6MMOeyTJNk3dYg", //monta@themonetizr.com
            //"e_ESSXx8PK_aVFr8wwW2Sur31yjQKLtaNIUDS5X9rKo",  //martins.jansevskis@gmail.com 
            //"mnfie-kWEAzhor9sUeOk5ohlnSCDKTefer2IarKd7zs"   //artem
            //"1BKIRvztaZFq0cklZfY7W-_yIGuSWgj2AHKfFTntzBU", // nauris@themonetizr.com
            //"jZrNLvD9pSWZ-oU7nrIivaIHW_PLnZX-KDWx1Ks8NnY" // gita@themonetizr.com
        };

        static public readonly Dictionary<string, string> keyNames = new Dictionary<string, string>()
        {
            {"zcJgPFt_Pe4XIs7aqbZbbN2KRcJziAWkRFzYYo7qVdE","FEBREZE" },
            {"4vdWpekbjsTcZF8EJFOSD5nzC82GL4NFrzY93KfUiGU","TIDE" },
            
        };

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

            versionText.text = $"App version: {Application.version} " +
                $"OS: {MonetizrAnalytics.osVersion}\n" +
                $"ADID: {MonetizrAnalytics.advertisingID}\n" +
                $"Limit ad tracking: {MonetizrAnalytics.limitAdvertising}\n" +
                $"Active campaign: {MonetizrManager.Instance.GetActiveCampaign()}";

            apiKeysList.value = keys.FindIndex(0, (string v)=>
                {
                    if (!keyNames.ContainsKey(MonetizrManager.Instance.GetCurrentAPIkey()))
                        return false;

                    return v == keyNames[MonetizrManager.Instance.GetCurrentAPIkey()];
                });
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

        public void OpenGame()
        {
            //var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            //Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);

            MonetizrManager.ShowMinigame(null, PanelId.MemoryGame, null);
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

            MonetizrManager.Instance.ChangeAPIKey(keys[apiKeysList.value]);
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