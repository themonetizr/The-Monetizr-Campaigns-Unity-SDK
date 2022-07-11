using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class RewardCenterPanel : PanelController
    {
        public Transform contentRoot;
        public RectTransform contentRect;
        public MonetizrRewardedItem itemUI;
        private bool hasSponsoredChallenges;
        public Text headerText;
        public Image background;
        public Image mainBanner;

        private List<MonetizrRewardedItem> missionItems = new List<MonetizrRewardedItem>();

        private int amountOfItems = 0;

        //public List<MissionUIDescription> missionsDescriptions;

        private new void Awake()
        {
            base.Awake();

        }

        internal void UpdateUI()
        {
            Log.Print("UpdateUI");

            CleanListView();

            if (MonetizrManager.Instance.HasChallengesAndActive())
            {
                hasSponsoredChallenges = true;
                AddSponsoredChallenges();
            }

            AddUserdefineChallenges();
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            hasSponsoredChallenges = false;

            //this.missionsDescriptions = missionsDescriptions;
                        
            MonetizrManager.Analytics.TrackEvent("Reward center opened",m);

            //MonetizrManager.HideTinyMenuTeaser();

            this.onComplete = onComplete;

            UpdateUI();
        }

        private void AddUserdefineChallenges()
        {
            foreach (var m in MonetizrManager.Instance.missionsManager.missions)
            {
                if (m.isSponsored)
                    continue;

                var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

                var item = go.GetComponent<MonetizrRewardedItem>();


                Log.Print(m.missionTitle);

                item.UpdateWithDescription(this, m);
            }
        }

        private void AddSponsoredChallenges()
        {
            //var challenges = MonetizrManager.Instance.GetAvailableChallenges();
            var activeChallenge = MonetizrManager.Instance.GetActiveChallenge();
            //int curChallenge = 0;

            //if (challenges.Count == 0)
            //    return;

            //put active challenge to the first place
            //challenges.Remove(activeChallenge);
            //challenges.Insert(0, activeChallenge);

            var campId = MonetizrManager.Instance.missionsManager.missions[0].campaignId;

            mainBanner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            amountOfItems = 0;

            foreach (var m in MonetizrManager.Instance.missionsManager.missions)
            {
                if (!m.isSponsored)
                    continue;

                if (m.isClaimed == ClaimState.Claimed)
                    continue;

                if (m.isDisabled)
                    continue;

                if (!m.isDisabled && DateTime.Now > m.deactivateTime)
                    continue;

                var ch = m.campaignId;

                if (ch == activeChallenge)
                {
                    var color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.HeaderTextColor);

                    if (color != default(Color))
                        headerText.color = color;


                    var bgSprite = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.TiledBackgroundSprite);

                    if (bgSprite != default(Sprite))
                        background.sprite = bgSprite;
                }
               
                AddSponsoredChallenge(m, amountOfItems);

                amountOfItems++;
                //curChallenge++;

                //if there's no room for sponsored campagn
                //if (challenges.Count == curChallenge)
                //    break;
            }
        }

        private void AddRewardedVideoChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} video";
            m.missionDescription = $"Watch video by {brandName} and earn {m.reward} {rewardTitle}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);
            m.progress = 1;
            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Watch video";

            Action<bool> onVideoComplete = (bool isSkipped) => { OnClaimRewardComplete(m, isSkipped, AddNewUIMissions); };

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () =>
            {
                OnVideoPlayPress(campaignId, m, onVideoComplete);

            };

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.UpdateWithDescription(this, m);

            if(m.brandBanner != null)
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }

        private void AddMultiplyCoinsChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

                       
            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} multiply";
            m.missionDescription = $"Earn {m.reward} {rewardTitle} and double it with {brandName}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = ((float)(getCurrencyFunc() - m.startMoney))/(float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Claim reward";

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () => { OnClaimRewardComplete(m, false, AddNewUIMissions); };

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;
            
            Log.Print(m.missionTitle);

            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;
            item.UpdateWithDescription(this, m);

            if (m.brandBanner != null)
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }

        private void AddSurveyChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} survey";
            m.missionDescription = $"Complete survey and earn {m.reward} {rewardTitle} with {brandName}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = 1.0f;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Start survey";


            Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
               OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
            };

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () => {
                /*OnClaimRewardComplete(m, false);*/

                MonetizrManager.ShowNotification((bool _) => { MonetizrManager.ShowSurvey(onSurveyComplete, m); },
                        m,
                        PanelId.SurveyNotification);

            };

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.UpdateWithDescription(this, m);

            if (m.brandBanner != null)
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }

        private void AddTwitterChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} twitter";
            m.missionDescription = $"Follow twitter and earn {m.reward} {rewardTitle} with {brandName}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = 1.0f; // ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Follow twitter";


            Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
                OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
            };

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () => {
                /*OnClaimRewardComplete(m, false);*/

                MonetizrManager.ShowNotification((bool _) => { MonetizrManager.GoToLink(onSurveyComplete, m); },
                        m,
                        PanelId.TwitterNotification);

            };

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.UpdateWithDescription(this, m);

            if (m.brandBanner != null)
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }

        private void AddGiveawayChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} giveaway";
            m.missionDescription = $"Earn {m.reward} {rewardTitle} and get giveaway from {brandName}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Claim reward!";


            Action<bool> onComplete = (bool isSkipped) =>
            {
                OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
            };

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () => {
                /*OnClaimRewardComplete(m, false);*/

                MonetizrManager.ShowEnterEmailPanel(
                    (bool isSkipped) =>
                    {
                        if (!isSkipped)
                        {
                            MonetizrManager.WaitForEndRequestAndNotify(onComplete, m);

                            //Debug.Log("Request completed!");
                            //MonetizrManager.GoToLink(onSurveyComplete, m);
                        }
                    },
                    m,
                    PanelId.GiveawayEmailEnterNotification);

            };

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.showGift = true;
            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;

            item.UpdateWithDescription(this, m);

            if (m.brandBanner != null)
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }

        private void AddVideoGiveawayChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            var campaign = MonetizrManager.Instance.GetCampaign(m.campaignId);

            bool needToPlayVideo = !(campaign.GetParam("videomail_giveaway_mission_without_video") == "true");


            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} giveaway";

            if(needToPlayVideo)
                m.missionDescription = $"Watch video and get {m.reward} {rewardTitle} from {brandName}";
            else
                m.missionDescription = $"Get {m.reward} {rewardTitle} from {brandName}";

            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = 1;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);


            m.claimButtonText = needToPlayVideo ? "Watch video!" : "Claim reward!";


            Action<bool> onComplete = (bool isSkipped) =>
            {
                OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
            };

            Action<bool> onVideoComplete = (bool isVideoSkipped) => {
                /*OnClaimRewardComplete(m, false);*/

                if (MonetizrManager.claimForSkippedCampaigns)
                    isVideoSkipped = false;

                if (isVideoSkipped)
                    return;

                MonetizrManager.ShowEnterEmailPanel(
                    (bool isMailSkipped) =>
                    {
                        if (isMailSkipped)
                            return;
                        
                        MonetizrManager.WaitForEndRequestAndNotify(onComplete, m);

                    },
                    m,
                    PanelId.GiveawayEmailEnterNotification);

            };

            //var campaign = MonetizrManager.Instance.GetCampaign(m.campaignId);

            //bool needToPlayVideo = !(campaign.GetParam("videomail_giveaway_mission_without_video") == "true");

                //Action<bool> onVideoComplete = (bool isSkipped) => { OnClaimRewardComplete(m, isSkipped, AddNewUIMissions); };

                //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () =>
            {
                if (needToPlayVideo)
                {
                    OnVideoPlayPress(campaignId, m, onVideoComplete);
                }
                else
                {
                    onVideoComplete(false);
                }

            };

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.showGift = true;
            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;

            item.UpdateWithDescription(this, m);

            if (m.brandBanner != null)
                MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }


        private void AddSponsoredChallenge(Mission m, int missionId)
        {
            var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);
            var item = go.GetComponent<MonetizrRewardedItem>();

            missionItems.Add(item);

            switch (m.type)
            {
                case MissionType.VideoReward: AddRewardedVideoChallenge(item, m,missionId); break;
                case MissionType.MutiplyReward: AddMultiplyCoinsChallenge(item, m,missionId); break;
                case MissionType.SurveyReward: AddSurveyChallenge(item, m, missionId); break;
                case MissionType.TwitterReward: AddTwitterChallenge(item, m, missionId); break;
                case MissionType.GiveawayWithMail: AddGiveawayChallenge(item, m, missionId); break;
                case MissionType.VideoWithEmailGiveaway: AddVideoGiveawayChallenge(item, m, missionId); break;
            }

        }

        

        private void CleanListView()
        {
            foreach (var c in contentRoot.GetComponentsInChildren<Transform>())
            {
                if (c != contentRoot)
                    Destroy(c.gameObject);
            }
        }

        public void OnButtonPress()
        {
            SetActive(false);
        }

        public void OnDebugMenuPress()
        {
             MonetizrManager.ShowDebug();
        }

        internal void ButtonPressed(ButtonController buttonController, Mission missionDescription)
        {
            if (!missionDescription.isSponsored)
                MonetizrManager.CleanUserDefinedMissions();

            //play video or claim ready user-defined mission
            missionDescription.onClaimButtonPress.Invoke();

            if (!missionDescription.isSponsored)
                UpdateUI();

        }

        public void OnClaimRewardComplete(Mission mission, bool isSkipped, Action updateUIDelegate)
        {
            MonetizrManager.Instance.OnClaimRewardComplete(mission, isSkipped, updateUIDelegate);
        }

        public void AddNewUIMissions()
        {
            //try to update UI
            foreach (var m in MonetizrManager.Instance.missionsManager.missions)
            {
                if (m.state == MissionUIState.ToBeShown)
                {
                    AddSponsoredChallenge(m, amountOfItems);
                    amountOfItems++;
                    m.state = MissionUIState.Visible;
                    m.isDisabled = false;
                }
            }
        }

        public void OnVideoPlayPress(string campaignId, Mission m, Action<bool> onComplete)
        {
            MonetizrManager.Analytics.TrackEvent("Claim button press",m);

            var htmlPath = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.Html5PathString);

            if (htmlPath != null)
            {
                MonetizrManager.ShowHTML5((bool isSkipped) => { onComplete(isSkipped); }, m);
            }
            else
            {
                var videoPath = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.VideoFilePathString);

                //MonetizrManager._PlayVideo(videoPath, (bool isSkipped) => { OnClaimRewardComplete(m, isSkipped); });

                MonetizrManager.ShowWebVideo((bool isSkipped) => { onComplete(isSkipped); }, m);
            }
        }

        //TODO: not sure if everything correct here
        internal override void FinalizePanel(PanelId id)
        {
            //if(MonetizrManager.tinyTeaserCanBeVisible)
            //MonetizrManager.ShowTinyMenuTeaser(null);

            if (!uiController.isVideoPlaying)
            {
                MonetizrManager.CleanUserDefinedMissions();
            }

            if (hasSponsoredChallenges)
            {
                MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
            }

            
        }

        // Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        void Update()
        {
            float z = 0;
            Vector2 pos = new Vector2();
            

            foreach(var it in missionItems)
            {
                if (it.mission == null)
                    continue;

                if (it.mission.isDisabled)
                    continue;

                if(it.mission.state == MissionUIState.ToBeHidden)
                {
                    if (it.gameObject.activeSelf)
                    {
                        it.gameObject.SetActive(false);
                        it.mission.isDisabled = true;
                        it.mission.state = MissionUIState.Hidden;
                    }

                    continue;
                }

                it.rect.anchoredPosition = pos;     

                pos.y -= it.rect.sizeDelta.y;
            }

            contentRect.sizeDelta = -pos;

        }
    }

}