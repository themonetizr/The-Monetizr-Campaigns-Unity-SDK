using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
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
        
        private AdPlacement _adType;
        private Sprite _brandBanner;
        private Sprite _brandRewardBanner;
        private Sprite _brandLogo;
        private Sprite _rewardIcon;

        internal override AdPlacement? GetAdPlacement()
        {
            return _adType;
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;
            //this.eventPrefix = null;
           
            closeButton.onClick.AddListener(OnButtonPress);
            noThanksButton?.onClick.AddListener(OnNoThanksPress);

            _brandLogo = m.campaign.GetAsset<Sprite>(AssetsType.BrandLogoSprite);
            _brandRewardBanner = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardBannerSprite);
            _brandBanner = m.campaign.GetAsset<Sprite>(AssetsType.BrandBannerSprite);
            _rewardIcon = MissionsManager.GetMissionRewardImage(m);
            
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
        }

        internal override void FinalizePanel(PanelId id)
        {

        }
               
        private void PrepareNotificationPanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = _brandBanner;
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

            rewardImage.sprite = _rewardIcon;

            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");

            _adType = AdPlacement.NotificationScreen;
        }

        private void PrepareCongratsPanel(Mission m)
        {
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }
            
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = _brandRewardBanner != null ? _brandRewardBanner : _brandBanner;

            if (_brandLogo != null)
                logo.sprite = _brandLogo;
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

            rewardImage.sprite = _rewardIcon;

            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                if (m.campaignServerSettings.ContainsKey("CongratsNotification.content_text2"))
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
            
            _adType = AdPlacement.CongratsNotificationScreen;
        }

        private void PrepareSurveyNotificationPanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = _brandRewardBanner;
            logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);
            rewardAmount.text = m.reward.ToString();
            
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

            rewardImage.sprite = _rewardIcon;

            _adType = AdPlacement.SurveyNotificationScreen;
        }

        private void PrepareGiveawayCongratsPanel(Mission m)
        {
            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            if (_brandRewardBanner != null)
                banner.sprite = _brandRewardBanner;
            else
                banner.sprite = _brandBanner;

            banner.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandBannerSprite);

            if (_brandLogo != null)
                logo.sprite = _brandLogo;
            else
                logo.gameObject.SetActive(false);

           
            var r = m.campaign.rewards.Find((ServerCampaign.Reward obj) => obj.claimable == true);

            rewardImage.sprite = _rewardIcon;

            //v2 updates
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
                       
            rewardImage.gameObject.SetActive(rewardImage.sprite != null);

            rewardImageBackgroud.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);

            noThanksButton?.gameObject.SetActive(false);
            
            logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

            gift?.gameObject.SetActive(false);


            _adType = AdPlacement.EmailCongratsNotificationScreen;
                      
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