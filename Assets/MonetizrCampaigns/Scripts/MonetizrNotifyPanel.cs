using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class MonetizrNotifyPanel : PanelController
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

        //[HideInInspector]
        //public Mission currentMission;
        private AdPlacement adType;

        //private string eventPrefix = null;
        private Sprite brandBanner;
        private Sprite brandRewardBanner;
        private Sprite brandLogo;

        public Image leaderboardImage;
        private Sprite rewardIcon;

        //private Action _onComplete;

        internal override AdPlacement? GetAdPlacement()
        {
            return adType;
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;
            //this.eventPrefix = null;
           
            closeButton.onClick.AddListener(OnButtonPress);
            noThanksButton?.onClick.AddListener(OnNoThanksPress);

            brandLogo = m.campaign.GetAsset<Sprite>(AssetsType.BrandLogoSprite);
            brandRewardBanner = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardBannerSprite);
            brandBanner = m.campaign.GetAsset<Sprite>(AssetsType.BrandBannerSprite);

            leaderboardImage.sprite = m.campaign.GetAsset<Sprite>(AssetsType.LeaderboardBannerSprite);
            leaderboardImage.gameObject.SetActive(leaderboardImage.sprite != null);

            rewardIcon = MissionsManager.GetMissionRewardImage(m);
            
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
                //case PanelId.TwitterNotification: PrepareTwitterNotificationPanel(m); break;
                
            }

            //MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);
        }

        internal override void FinalizePanel(PanelId id)
        {
            //MonetizrManager.Analytics.EndShowAdAsset(adType, currentMission);
        }
               
        private void PrepareNotificationPanel(Mission m)
        {
            //eventPrefix = "Notification";

            //----

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = brandBanner;
            logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

            rewardAmount.text = m.reward.ToString();

            string brandTitle = m.brandName;

            
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            gift?.gameObject.SetActive(false);
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

            adType = AdPlacement.NotificationScreen;
            //MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);
            //MonetizrManager.Analytics.TrackEvent("Notification shown", m);
            

        }

        private void PrepareCongratsPanel(Mission m)
        {
            //Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            /*Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
            {
                rewardIcon = customCoin;
            }*/



            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            if (brandRewardBanner != null)
                banner.sprite = brandRewardBanner;
            else
                banner.sprite = brandBanner;

            if (brandLogo != null)
                logo.sprite = brandLogo;
            else
                logo.gameObject.SetActive(false);

            rewardAmount.text = m.reward.ToString();

            string rewardNumber = $"{m.reward} ";

            if (m.reward == 1)
                rewardNumber = "";

            rewardImage.gameObject.SetActive(true);

            rewardImageBackgroud.gameObject.SetActive(m.reward != 1);
            rewardAmount.gameObject.SetActive(m.reward != 1);

            noThanksButton?.gameObject.SetActive(false);

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
            logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);
            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);


            adType = AdPlacement.CongratsNotificationScreen;
            //MonetizrManager.Analytics.TrackEvent("Congrats screen shown", m);
            //MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);


            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }

        private void PrepareSurveyNotificationPanel(Mission m)
        {
            //eventPrefix = "Survey notification";

            //----

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = brandRewardBanner;
            logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);
            rewardAmount.text = m.reward.ToString();

            //title.text = $"Survey!";
            //text.text = $"Please spend some time and  <color=#F05627>{m.reward} {m.rewardTitle}</color> from {m.brandName}";

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }
            

            rewardImage.gameObject.SetActive(true);
            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);
            noThanksButton?.gameObject.SetActive(true);

            gift?.gameObject.SetActive(false);

            

            /*Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(challengeId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;*/

            rewardImage.sprite = rewardIcon;


            adType = AdPlacement.SurveyNotificationScreen;
            //MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);

            //MonetizrManager.Analytics.TrackEvent("Survey notification shown", m);
            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.RewardBanner, currentMission);

        }

        private void PrepareGiveawayCongratsPanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            if (brandRewardBanner != null)
                banner.sprite = brandRewardBanner;
            else
                banner.sprite = brandBanner;

            banner.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandBannerSprite);

            if (brandLogo != null)
                logo.sprite = brandLogo;
            else
                logo.gameObject.SetActive(false);

           
            var r = m.campaign.rewards.Find((ServerCampaign.Reward obj) => obj.claimable == true);

            rewardImage.sprite = rewardIcon;

            //v2 updates
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
                       
            rewardImage.gameObject.SetActive(rewardImage.sprite != null);

            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);

            noThanksButton?.gameObject.SetActive(false);

            //rewardImage.sprite = rewardIcon;

            logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

            gift?.gameObject.SetActive(false);


            adType = AdPlacement.EmailCongratsNotificationScreen;
            //MonetizrManager.Analytics.TrackEvent("Email congrats shown", m);
            //MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMission);
                        
        }

        private new void Awake()
        {
            base.Awake();


        }

        public void OnNoThanksPress()
        {
            //MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressSkip);

            //MonetizrManager.
            //
            //ytics.TrackEvent("Twitter cancel", currentMission);

            isSkipped = true;
            SetActive(false);
        }

        public void OnButtonPress()
        {
            //MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressOk);

            //if (eventPrefix != null)
            //    MonetizrManager.Analytics.TrackEvent($"{eventPrefix} pressed", currentMission);

            isSkipped = false;
            SetActive(false);
        }

    }

}