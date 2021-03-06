using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class EnterEmailPanel : PanelController
    {
        public Image banner;
        public Image rewardImageBackgroud;
        public Image rewardImage;
        public Text rewardAmount;
        public Text title;
        public Text text;
        public Image logo;
        public Button closeButton;
        public Text buttonText;
        public Button noThanksButton;
        public InputField inputField;

        [HideInInspector]
        public Mission currentMission;
        private Regex validateEmailRegex;
        private string result;

        //private Action onComplete;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this.onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            closeButton.onClick.AddListener(OnButtonPress);
            noThanksButton?.onClick.AddListener(OnNoThanksPress);

            inputField.onValueChanged.AddListener(OnInputFieldChanged);
            closeButton.interactable = false;

            validateEmailRegex = new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$");

            PreparePanel(m);


        }

        internal void OnInputFieldChanged(string s)
        {
            //check email with regex
            var isValid = validateEmailRegex.IsMatch(s);

            //double check with MailAddress
            if (isValid)
            {
                try
                {
                    MailAddress address = new MailAddress(s);
                    isValid = (address.Address == s);
                }
                catch (FormatException)
                {
                    // address is invalid
                    isValid = false;
                }
            }

            //email valid, but country code is too short
            if (s.Length - s.LastIndexOf('.') <= 2)
                isValid = false;

            closeButton.interactable = isValid;

            result = s;
        }

        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.temporaryEmail = result;
            MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner, currentMission);
        }



        private void PreparePanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandBannerSprite);
            banner.color = Color.white;

            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite);

            rewardAmount.text = m.reward.ToString();

            string brandTitle = m.brandName;

            var r = MonetizrManager.Instance.GetCampaign(m.campaignId).rewards.Find((Challenge.Reward obj) => { return obj.claimable == true;  });

            string giveawayTitle = "";

            if (r != null)
            {
                giveawayTitle = r.title;
            }

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.CustomCoinString);
            }

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
            {
                rewardIcon = customCoin;
            }


            title.text = $"Get {giveawayTitle}!";
            text.text = $"<color=#F05627>Enter your e-mail</color> to get giveaway from {brandTitle}.\nDon't miss out!";

            //buttonText.text = "Learn More";
            buttonText.text = "Claim!";

            rewardImage.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);
            rewardImageBackgroud.gameObject.SetActive(false);
            noThanksButton?.gameObject.SetActive(true);

            rewardImage.sprite = rewardIcon;


            MonetizrManager.Analytics.TrackEvent("Enter email shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, currentMission);

        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnNoThanksPress()
        {
            isSkipped = true;
            SetActive(false);
        }

        public void OnButtonPress()
        {
            isSkipped = false;
            SetActive(false);
        }

    }

}