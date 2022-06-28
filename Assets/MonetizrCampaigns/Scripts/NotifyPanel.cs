using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class NotifyPanel : PanelController
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

        [HideInInspector]
        public Mission currentMission;

        //private Action onComplete;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this.onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            closeButton.onClick.AddListener(OnButtonPress);
            noThanksButton?.onClick.AddListener(OnNoThanksPress);

            switch (id)
            {
                case PanelId.CongratsNotification:
                    if (m.type == MissionType.GiveawayWithMail)
                        PrepareGiveawayCongratsPanel(m);
                    else
                        PrepareCongratsPanel(m);
                    break;

                case PanelId.StartNotification: PrepareNotificationPanel(m); break;
                case PanelId.SurveyNotification: PrepareSurveyNotificationPanel(m); break;
                case PanelId.TwitterNotification: PrepareTwitterNotificationPanel(m); break;
                
            }
        }

        internal override void FinalizePanel(PanelId id)
        {
            switch (id)
            {
                case PanelId.CongratsNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.RewardBanner, currentMission);
                    break;

                case PanelId.StartNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner, currentMission);
                    break;

                case PanelId.SurveyNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.RewardBanner, currentMission);
                    break;

                case PanelId.TwitterNotification:
                    MonetizrManager.Analytics.EndShowAdAsset(AdType.RewardBanner, currentMission);
                    break;

                
            }


        }



        private void PrepareNotificationPanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandBanner;
            logo.sprite = m.brandLogo;
            rewardAmount.text = m.reward.ToString();

            string brandTitle = m.brandName;

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


            title.text = $"{brandTitle} video";
            text.text = $"<color=#F05627>Watch video</color> by {brandTitle} to earn <color=#F05627>{m.reward} {rewardTitle}</color>";

            //buttonText.text = "Learn More";
            buttonText.text = "Got it!";

            rewardImage.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);
            rewardImageBackgroud.gameObject.SetActive(false);
            noThanksButton?.gameObject.SetActive(false);

            rewardImage.sprite = rewardIcon;


            MonetizrManager.Analytics.TrackEvent("Notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, currentMission);

        }

        private void PrepareCongratsPanel(Mission m)
        {
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



            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            if (m.brandRewardBanner != null)
                banner.sprite = m.brandRewardBanner;
            else
                banner.sprite = m.brandBanner;

            if (m.brandLogo != null)
                logo.sprite = m.brandLogo;
            else
                logo.gameObject.SetActive(false);

            rewardAmount.text = m.reward.ToString();

            string rewardNumber = $"{m.reward} ";

            if (m.reward == 1)
                rewardNumber = "";

            title.text = $"Congrats!";
            text.text = $"You earn <color=#F05627>{rewardNumber}{rewardTitle}</color> from {m.brandName}";

            //buttonText.text = "Learn More";
            buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(true);

            rewardImageBackgroud.gameObject.SetActive(m.reward != 1);
            rewardAmount.gameObject.SetActive(m.reward != 1);

            noThanksButton?.gameObject.SetActive(false);

            rewardImage.sprite = rewardIcon;


            MonetizrManager.Analytics.TrackEvent("Reward notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }

        private void PrepareSurveyNotificationPanel(Mission m)
        {

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandRewardBanner;
            logo.sprite = m.brandLogo;
            rewardAmount.text = m.reward.ToString();

            title.text = $"Survey!";
            //text.text = $"Please spend some time and  <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(challengeId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(challengeId, AssetsType.CustomCoinString);
            }

            text.text = $"<color=#F05627>Complete the survey</color>\nby {m.brandName} to earn\n<color=#F05627>{m.reward} {rewardTitle}</color>";

            //buttonText.text = "Learn More";
            buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(true);
            rewardImageBackgroud.gameObject.SetActive(true);
            rewardAmount.gameObject.SetActive(true);
            noThanksButton?.gameObject.SetActive(false);

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;

            rewardImage.sprite = rewardIcon;



            MonetizrManager.Analytics.TrackEvent("Survey notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }


        private void PrepareTwitterNotificationPanel(Mission m)
        {

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandBanner;

            //logo.sprite = m.brandLogo;

            logo.gameObject.SetActive(false);

            rewardAmount.text = m.reward.ToString();

            title.text = $"Follow us!";
            //text.text = $"Please spend some time and  <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(challengeId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(challengeId, AssetsType.CustomCoinString);
            }

            string rewardNumber = $"{m.reward} ";

            if (m.reward == 1)
                rewardNumber = "";

            text.text = $"Follow <color=#F05627>{m.brandName} Twitter</color>\nto earn <color=#F05627>{rewardNumber}{rewardTitle}</color>";
            

            //buttonText.text = "Learn More";
            buttonText.text = "Go to Twitter";


            rewardImage.gameObject.SetActive(true);

            rewardImageBackgroud.gameObject.SetActive(m.reward != 1);
            rewardAmount.gameObject.SetActive(m.reward != 1);
            

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;

            rewardImage.sprite = rewardIcon;



            MonetizrManager.Analytics.TrackEvent("Twitter notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }

        private void PrepareGiveawayCongratsPanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            if (m.brandRewardBanner != null)
                banner.sprite = m.brandRewardBanner;
            else
                banner.sprite = m.brandBanner;

            banner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandBannerSprite);

            if (m.brandLogo != null)
                logo.sprite = m.brandLogo;
            else
                logo.gameObject.SetActive(false);

           
            var r = MonetizrManager.Instance.GetCampaign(m.campaignId).rewards.Find((Challenge.Reward obj) => { return obj.claimable == true; });


            title.text = $"Congrats!";
            text.text = $"You earned <color=#F05627>{r.title}</color> from {m.brandName}";

            //buttonText.text = "Learn More";
            buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(false);

            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);

            noThanksButton?.gameObject.SetActive(false);

            //rewardImage.sprite = rewardIcon;


            MonetizrManager.Analytics.TrackEvent("Reward notification shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnNoThanksPress()
        {
            //MonetizrManager.Analytics.TrackEvent("Twitter cancel", currentMission);

            isSkipped = true;
            SetActive(false);
        }

        public void OnButtonPress()
        {
            //MonetizrManager.Analytics.TrackEvent("Twitter follow", currentMission);

            isSkipped = false;
            SetActive(false);
        }

    }

}