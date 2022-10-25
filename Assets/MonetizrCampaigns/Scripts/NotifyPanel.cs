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
        public Image gift;

        [HideInInspector]
        public Mission currentMission;
        private AdType adType;

        private string eventPrefix = null;

        //private Action onComplete;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this.onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;
            this.eventPrefix = null;
           

            closeButton.onClick.AddListener(OnButtonPress);
            noThanksButton?.onClick.AddListener(OnNoThanksPress);

            switch (id)
            {
                case PanelId.CongratsNotification:
                    if (/*m.type == MissionType.GiveawayWithMail ||*/ m.type == MissionType.VideoWithEmailGiveaway)
                    {
                        if(MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Product)
                            PrepareGiveawayCongratsPanel(m);
                        else
                            PrepareCongratsPanel(m);
                    }
                    else
                    {
                        PrepareCongratsPanel(m);
                    }
                    break;

                case PanelId.StartNotification: PrepareNotificationPanel(m); break;
                case PanelId.SurveyNotification: PrepareSurveyNotificationPanel(m); break;
                case PanelId.TwitterNotification: PrepareTwitterNotificationPanel(m); break;
                
            }

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);
        }

        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(adType, currentMission);
        }
               
        private void PrepareNotificationPanel(Mission m)
        {
            eventPrefix = "Notification";

            //----

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandBanner;
            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

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

            gift?.gameObject.SetActive(false);

            //title.text = $"{brandTitle} video";
            //text.text = $"<color=#F05627>Watch video</color> by {brandTitle} to earn <color=#F05627>{m.reward} {rewardTitle}</color>";

            //buttonText.text = "Learn More";
            //buttonText.text = "Got it!";

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.IngameRewardSprite) &&
                EnterEmailPanel.GetPanelType(m) == EnterEmailType.IngameReward)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.IngameRewardSprite);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.RewardSprite) &&
               EnterEmailPanel.GetPanelType(m) == EnterEmailType.ProductReward)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.RewardSprite);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.UnknownRewardSprite) &&
               EnterEmailPanel.GetPanelType(m) == EnterEmailType.SelectionReward)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.UnknownRewardSprite);
            }


            rewardImage.gameObject.SetActive(true);
            rewardAmount.gameObject.SetActive(false);
            rewardImageBackgroud.gameObject.SetActive(false);
            noThanksButton?.gameObject.SetActive(true);

            if (EnterEmailPanel.GetPanelType(m) == EnterEmailType.SelectionReward)
            {
                gift?.gameObject.SetActive(true);
                rewardImage?.gameObject.SetActive(false);
            }

            rewardImage.sprite = rewardIcon;

            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");

            adType = AdType.NotificationScreen;
            MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);
            MonetizrManager.Analytics.TrackEvent("Notification shown", m);
            

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

            //title.text = $"Congrats!";
            //text.text = $"You earn <color=#F05627>{rewardNumber}{rewardTitle}</color> from {m.brandName}";

            //buttonText.text = "Learn More";
            //buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(true);

            rewardImageBackgroud.gameObject.SetActive(m.reward != 1);
            rewardAmount.gameObject.SetActive(m.reward != 1);

            noThanksButton?.gameObject.SetActive(false);



            //v2 updates
            //FIXME
            if (m.type != MissionType.VideoWithEmailGiveaway)
                MonetizrManager.temporaryRewardTypeSelection = MonetizrManager.RewardSelectionType.Ingame;


            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.IngameRewardSprite) &&
                MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.IngameRewardSprite);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.RewardSprite) &&
               MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Product)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.RewardSprite);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.UnknownRewardSprite) &&
               EnterEmailPanel.GetPanelType(m) == EnterEmailType.SelectionReward)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.UnknownRewardSprite);
            }

            if (rewardIcon != null)
                rewardImage.sprite = rewardIcon;

            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                if (m.campaignServerSettings.dictionary.ContainsKey("CongratsNotification.content_text2"))
                {
                    text.text = m.campaignServerSettings.GetParam("CongratsNotification.content_text2");
                }
            }

            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");

            gift?.gameObject.SetActive(false);
            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);
            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);


            adType = AdType.CongratsNotificationScreen;
            MonetizrManager.Analytics.TrackEvent("Congrats screen shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);


            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }

        private void PrepareSurveyNotificationPanel(Mission m)
        {
            eventPrefix = "Survey notification";

            //----

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandRewardBanner;
            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);
            rewardAmount.text = m.reward.ToString();

            //title.text = $"Survey!";
            //text.text = $"Please spend some time and  <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(challengeId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(challengeId, AssetsType.CustomCoinString);
            }

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.IngameRewardSprite))
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.IngameRewardSprite);
            }

            //text.text = $"<color=#F05627>Complete the survey</color>\nby {m.brandName} to earn\n<color=#F05627>{m.reward} {rewardTitle}</color>";

            //buttonText.text = "Learn More";
            //buttonText.text = "Awesome!";

            rewardImage.gameObject.SetActive(true);
            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);
            noThanksButton?.gameObject.SetActive(true);

            gift?.gameObject.SetActive(false);

            

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;

            rewardImage.sprite = rewardIcon;


            adType = AdType.SurveyNotificationScreen;
            MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);

            MonetizrManager.Analytics.TrackEvent("Survey notification shown", m);
            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }


        private void PrepareTwitterNotificationPanel(Mission m)
        {

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.brandBanner;

            //logo.sprite = m.brandLogo;

            logo.gameObject.SetActive(false);

            rewardAmount.text = m.reward.ToString();

            //title.text = $"Follow us!";
            //text.text = $"Please spend some time and  <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(challengeId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(challengeId, AssetsType.CustomCoinString);
            }

            string rewardNumber = $"{m.reward} ";

            if (m.reward == 1)
                rewardNumber = "";

            //text.text = $"Follow <color=#F05627>{m.brandName} Twitter</color>\nto earn <color=#F05627>{rewardNumber}{rewardTitle}</color>";
            

            //buttonText.text = "Learn More";
            //buttonText.text = "Go to Twitter";


            rewardImage.gameObject.SetActive(true);

            rewardImageBackgroud.gameObject.SetActive(m.reward != 1);
            rewardAmount.gameObject.SetActive(m.reward != 1);
            

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;

            rewardImage.sprite = rewardIcon;


            //adType = AdType.SurveyNotificationScreen;
            //MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);

            //MonetizrManager.Analytics.TrackEvent("Twitter notification shown", m);
            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

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

           
            var r = MonetizrManager.Instance.GetCampaign(m.campaignId).rewards.Find((ServerCampaign.Reward obj) => { return obj.claimable == true; });


            //title.text = $"Congrats!";
            //text.text = $"You earned <color=#F05627>{r.title}</color> from {m.brandName}";

            //buttonText.text = "Learn More";
            //buttonText.text = "Awesome!";

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon; ; 


            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.IngameRewardSprite) &&
                MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.IngameRewardSprite);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.RewardSprite) &&
               MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Product)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.RewardSprite);
            }

            rewardImage.sprite = rewardIcon;

            //v2 updates
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
                       
            rewardImage.gameObject.SetActive(rewardImage.sprite != null);

            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);

            noThanksButton?.gameObject.SetActive(false);

            //rewardImage.sprite = rewardIcon;

            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

            gift?.gameObject.SetActive(false);


            adType = AdType.EmailCongratsNotificationScreen;
            MonetizrManager.Analytics.TrackEvent("Email congrats shown", m);
            MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);
                        
        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnNoThanksPress()
        {
            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressSkip);

            //MonetizrManager.
            //
            //ytics.TrackEvent("Twitter cancel", currentMission);

            isSkipped = true;
            SetActive(false);
        }

        public void OnButtonPress()
        {
            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressOk);

            if (eventPrefix != null)
                MonetizrManager.Analytics.TrackEvent($"{eventPrefix} pressed", currentMission);

            isSkipped = false;
            SetActive(false);
        }

    }

}