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
        public GameObject banner;
        public RectTransform scrollView;

        private List<MonetizrRewardedItem> missionItems = new List<MonetizrRewardedItem>();

        private int amountOfItems = 0;
        private readonly int bannerHeight = 1050+100;

        private bool showNotClaimedDisabled = false;

        //public List<MissionUIDescription> missionsDescriptions;

        private new void Awake()
        {
            base.Awake();

        }

        internal void UpdateUI()
        {
            Log.Print("UpdateUI");

            CleanListView();

            if (MonetizrManager.Instance.HasCampaignsAndActive())
            {
                hasSponsoredChallenges = true;
                AddSponsoredChallenges();
            }

            AddUserdefineChallenges();
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            
            //string uiItemPrefab = "MonetizrRewardedItem";

            //if (uiVersion == 2)
            string uiItemPrefab = "MonetizrRewardedItem2";

            itemUI = (Resources.Load(uiItemPrefab) as GameObject).GetComponent<MonetizrRewardedItem>();
            

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
            //var activeChallenge = MonetizrManager.Instance.GetActiveChallenge();
            //int curChallenge = 0;

            //if (challenges.Count == 0)
            //    return;

            //put active challenge to the first place
            //challenges.Remove(activeChallenge);
            //challenges.Insert(0, activeChallenge);

            var campaignId = MonetizrManager.Instance.GetActiveCampaign();

            var campaign = MonetizrManager.Instance.GetCampaign(campaignId);

            if (campaign == null)
            {
                Debug.LogWarning("No active campaigns for RC!");
                return;
            }

            showNotClaimedDisabled = campaign.serverSettings.GetBoolParam("RewardCenter.show_disabled_missions", true);

            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(true);

            if(missions.Count == 0)
            {
                Debug.LogWarning("No sponsored challenges for RC!");
                return;
            }

            var campId = missions[0].campaignId;

            //mainBanner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            amountOfItems = 0;


            var go = GameObject.Instantiate<GameObject>(banner, contentRoot);

            var images = go.GetComponentsInChildren<Image>();

            images[0].sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);
            images[1].sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandRewardLogoSprite);

            //go.GetComponent<Image>().sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            foreach (var m in missions)
            {
                var ch = m.campaignId;

                m.showHidden = showNotClaimedDisabled && m.isDisabled && m.isClaimed != ClaimState.Claimed;
                    
                if (ch == missions[0].campaignId)
                //if (ch == activeChallenge)
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
            m.missionDescription = $"Watch video by {brandName} and earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);
            m.progress = 1;
            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Watch video";

           /* Action<bool> onVideoComplete = (bool isSkipped) => { OnClaimRewardComplete(m, isSkipped, AddNewUIMissions); };

            //show video, then claim rewards if it's completed
            m.onClaimButtonPress = () =>
            {
                OnVideoPlayPress(m, onVideoComplete);

            };*/

            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);


            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.UpdateWithDescription(this, m);

            //if(m.brandBanner != null)
            //    MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
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
            m.missionDescription = $"Earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} and double it with {brandName}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = ((float)(getCurrencyFunc() - m.startMoney))/(float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Claim reward";

            //show video, then claim rewards if it's completed
            //m.onClaimButtonPress = () => { OnClaimRewardComplete(m, false, AddNewUIMissions); };

            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;
            
            Log.Print(m.missionTitle);

            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;
            item.UpdateWithDescription(this, m);

            //if (m.brandBanner != null)
            //    MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
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
            m.missionDescription = $"Complete survey and earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} with {brandName}";
            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = 1.0f;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);
            m.claimButtonText = "Start survey";


           /* Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
               OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
            };*/

            //show video, then claim rewards if it's completed
            /*m.onClaimButtonPress = () => {
                

                MonetizrManager.ShowNotification((bool _) => { MonetizrManager.ShowSurvey(onSurveyComplete, m); },
                        m,
                        PanelId.SurveyNotification);

            };*/

            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);
            
                //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

                //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.UpdateWithDescription(this, m);

            //if (m.brandBanner != null)
            //    MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
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
            m.missionDescription = $"Follow twitter and earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} with {brandName}";
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

            //if (m.brandBanner != null)
            //    MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
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

            //var campaign = MonetizrManager.Instance.GetCampaign(m.campaignId);

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} giveaway";

            bool needToPlayVideo = !(m.campaignServerSettings.GetParam("email_giveaway_mission_without_video") == "true");

#if UNITY_EDITOR_WIN
            needToPlayVideo = false;
#endif

            if (needToPlayVideo)
                // m.missionDescription = $"Watch video and get {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} from {brandName}";
                m.missionDescription = $"Watch video and get $3 OFF Coupon from {brandName}";
            else
                m.missionDescription = $"Get {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} from {brandName}";

            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = 1;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);


            m.claimButtonText = needToPlayVideo ? "Watch video!" : "Claim reward!";


            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m,null,AddNewUIMissions);

           

            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            item.showGift = true;
            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.RewardSprite))
            {
                item.giftIcon.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.RewardSprite);
            }

            item.UpdateWithDescription(this, m);

            //if (m.brandBanner != null)
            //    MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }

        private void AddMinigameChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && MonetizrManager.Instance.HasAsset(campaignId, AssetsType.CustomCoinString))
            {
                rewardTitle = MonetizrManager.Instance.GetAsset<string>(campaignId, AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            //var campaign = MonetizrManager.Instance.GetCampaign(m.campaignId);

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} minigame";

                        
            m.missionDescription = $"Play minigame and get {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} from {brandName}";
            

            m.missionIcon = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardLogoSprite);

            m.progress = 1;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;

            m.brandName = brandName;
            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandLogoSprite); ;
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandRewardBannerSprite);


            m.claimButtonText = "Play!";


            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);



            //var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);

            //var item = go.GetComponent<MonetizrRewardedItem>();

            if (missionId != 0)
                m.brandBanner = null;

            Log.Print(m.missionTitle);

            //item.showGift = true;
            //item.currectProgress = getCurrencyFunc() - m.startMoney;
            //item.maxProgress = m.reward;

            item.UpdateWithDescription(this, m);

            //if (m.brandBanner != null)
            //    MonetizrManager.Analytics.BeginShowAdAsset(AdType.IntroBanner, m);
        }


        private void AddSponsoredChallenge(Mission m, int missionId)
        {
            var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);
            var item = go.GetComponent<MonetizrRewardedItem>();

            m.rewardCenterItem = item;

            missionItems.Add(item);

            switch (m.type)
            {
                case MissionType.VideoReward: AddRewardedVideoChallenge(item, m,missionId); break;
                case MissionType.MutiplyReward: AddMultiplyCoinsChallenge(item, m,missionId); break;
                case MissionType.SurveyReward: AddSurveyChallenge(item, m, missionId); break;
                case MissionType.TwitterReward: AddTwitterChallenge(item, m, missionId); break;
                //case MissionType.GiveawayWithMail: AddGiveawayChallenge(item, m, missionId); break;
                case MissionType.VideoWithEmailGiveaway: AddVideoGiveawayChallenge(item, m, missionId); break;
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward:
                    AddMinigameChallenge(item, m, missionId); break;
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
            MonetizrManager.Instance.OnClaimRewardComplete(mission, isSkipped, null, updateUIDelegate);
        }

        public void AddNewUIMissions()
        {
            //try to update UI
            foreach (var m in MonetizrManager.Instance.missionsManager.missions)
            {
                if (m.state == MissionUIState.ToBeShown && m.isClaimed != ClaimState.Claimed)
                {
                    if (!showNotClaimedDisabled)
                    {
                        AddSponsoredChallenge(m, amountOfItems);
                        amountOfItems++;
                        
                    }
                    else
                    {
                        m.rewardCenterItem.hideOverlay.SetActive(false);
                    }

                    m.state = MissionUIState.Visible;
                    m.isDisabled = false;
                }
                
            }
        }

        public void OnVideoPlayPress(Mission m, Action<bool> onComplete)
        {
            MonetizrManager.Instance.missionsManager.OnVideoPlayPress(m, onComplete);
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

            //if (hasSponsoredChallenges)
            //{
            //    MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
            //}

            
        }

        // Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        void Update()
        {
            float z = 0;
            Vector2 pos = new Vector2(0,-bannerHeight);
            

            foreach(var it in missionItems)
            {
                if (it.mission == null)
                    continue;

                //if (it.mission.isDisabled)
                //    continue;

                if (it.mission.state == MissionUIState.Hidden)
                    continue;

                if (it.mission.state == MissionUIState.ToBeHidden)
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