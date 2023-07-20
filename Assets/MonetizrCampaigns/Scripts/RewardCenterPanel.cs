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
        public Text moneyText;
        public Image background;
        
        public Image mainBanner;
        public Image mainLogo;

        public GameObject banner;
        public RectTransform scrollViewRect;
        public ScrollRect scrollViewElement;

        public GameObject termsAndCondPrefab;


        private RectTransform termsAndCondRect;

        private List<MonetizrRewardedItem> missionItems = new List<MonetizrRewardedItem>();

        private int amountOfItems = 0;
        private bool scrollListHasBanner;
        private float bannerHeight = 1150;

        private bool showNotClaimedDisabled = false;
        private List<Mission> missionsForRewardCenter;
        private string currentCampaign;

        private bool isLandscape = false;

        private GameObject bannerObject;
        private RectTransform bannerObjectRect;

        public GameObject bannerLayoutElement;

        //private Mission currentMission;

        //public List<MissionUIDescription> missionsDescriptions;

        private new void Awake()
        {
            base.Awake();

        }

        internal override AdPlacement? GetAdPlacement()
        {
            return AdPlacement.RewardsCenterScreen;
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
            currentMission = m;
            currentCampaign =  MonetizrManager.Instance.GetActiveCampaign();

            /*if (Utils.isInLandscapeMode())
            {
                scrollViewElement.horizontal = false;
                scrollViewElement.vertical = true;
            }*/



            //MonetizrManager.CallUserDefinedEvent(currentCampaign,
             //     NielsenDar.GetPlacementName(AdPlacement.RewardsCenterScreen),
             //     MonetizrManager.EventType.Impression);

            //string uiItemPrefab = "MonetizrRewardedItem";

            //if (uiVersion == 2)
            string uiItemPrefab = "MonetizrRewardedItem2";

            itemUI = (Resources.Load(uiItemPrefab) as GameObject).GetComponent<MonetizrRewardedItem>();
            

            hasSponsoredChallenges = false;

            //this.missionsDescriptions = missionsDescriptions;

            //MonetizrManager.Analytics.BeginShowAdAsset(AdPlacement.RewardsCenterScreen, m);
            //MonetizrManager.Analytics.TrackEvent("Reward center opened",m);

            MonetizrManager.HideTinyMenuTeaser();

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

            var campaignId = MonetizrManager.Instance.GetActiveCampaign();

            var campaign = MonetizrManager.Instance.GetCampaign(campaignId);

            if (campaign == null)
            {
                Log.PrintWarning("No active campaigns for RC!");
                return;
            }


            if (Utils.isInLandscapeMode())
            {
                //r = campaign.serverSettings.GetRectParam("RewardCenter.transform_landscape", new List<float> { 0, 0, 0, 0 });
            }
            else
            {
                var r = campaign.serverSettings.GetRectParam("RewardCenter.transform", new List<float> { 30, 0, 0, 0 });

                /*Left*/
                scrollViewRect.offsetMin = new Vector2(r[0], r[3]);
                /*Bottom*/
                //scrollView.offsetMin.y = r[3];

                /*Right*/
                scrollViewRect.offsetMax = new Vector2(-r[2], r[1]);
                /*Top*/
                // scrollView.offsetMax.y = r[1];
            }

            showNotClaimedDisabled = campaign.serverSettings.GetBoolParam("RewardCenter.show_disabled_missions", true);

            missionsForRewardCenter = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(true);

            if (missionsForRewardCenter.Count == 0)
            {
                Log.PrintWarning("No sponsored challenges for RC!");
                return;
            }

            var camp = missionsForRewardCenter[0].campaign;

            //mainBanner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            amountOfItems = 0;

            var hasBanner = camp.HasAsset(AssetsType.BrandBannerSprite);
                        
            
            if (Utils.isInLandscapeMode())
            {
                if (hasBanner)
                {
                    mainBanner.sprite = camp.GetAsset<Sprite>(AssetsType.BrandBannerSprite);
                    
                    bool hasLogo = camp.HasAsset(AssetsType.BrandRewardLogoSprite);

                    if(hasLogo)
                    {
                        mainLogo.sprite = camp.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);
                    }
                }
                else
                {
                    bannerLayoutElement.SetActive(false);
                }
            }

            scrollListHasBanner = Utils.isInLandscapeMode() ? false : hasBanner;

            if (scrollListHasBanner)
            {
                bannerObject = GameObject.Instantiate<GameObject>(banner, contentRoot);

                bannerObjectRect = bannerObject.GetComponent<RectTransform>();

                var images = bannerObject.GetComponentsInChildren<Image>();

                images[0].sprite = camp.GetAsset<Sprite>(AssetsType.BrandBannerSprite);

                bool hasLogo = camp.HasAsset(AssetsType.BrandRewardLogoSprite);

                images[1].gameObject.SetActive(hasLogo);

                if (hasLogo)
                    images[1].sprite = camp.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);

                bannerHeight = 1150;
            }
            else
            {
                //bannerHeight = 120;
            }

            //go.GetComponent<Image>().sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            foreach (var m in missionsForRewardCenter)
            {
                var ch = m.campaignId;

                m.showHidden = showNotClaimedDisabled && m.isDisabled && m.isClaimed != ClaimState.Claimed;
                    
                if (ch == missionsForRewardCenter[0].campaignId)
                //if (ch == activeChallenge)
                {
                   /* var color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.HeaderTextColor);

                    if (color != default(Color))
                        headerText.color = color;*/


                    var bgSprite = m.campaign.GetAsset<Sprite>(AssetsType.TiledBackgroundSprite);

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

            UpdateStatusBar();


            var t = GameObject.Instantiate<GameObject>(termsAndCondPrefab, contentRoot);

            termsAndCondRect = t.GetComponent<RectTransform>();
        }

        private void UpdateStatusBar()
        {
            var camp = MonetizrManager.Instance.GetCampaign(currentCampaign);

            var statusText = camp.serverSettings.GetParam("RewardCenter.missions_num_text", "%claimed_missions%/%total_missions%");
 
            int claimed = 0;

            var missions = MonetizrManager.Instance.missionsManager.missions;

            foreach (var m in missions)
                if (m.isClaimed == ClaimState.Claimed)
                    claimed++;

            statusText = statusText.Replace("%claimed_missions%",claimed.ToString());
            statusText = statusText.Replace("%total_missions%", missions.Count.ToString());

            var money = camp.serverSettings.GetParam("RewardCenter.money_num_text", "%total_money%");

            var playerMoney = MonetizrManager.gameRewards[RewardType.Coins].GetCurrencyFunc();

            money = money.Replace("%total_money%", $"{MonetizrRewardedItem.ScoreShow(playerMoney)}");

            headerText.text = statusText;
            moneyText.text = money;
        }

        private void AddRewardedVideoChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            //m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(campaignId, AssetsType.BrandBannerSprite);
            m.missionTitle = $"{brandName} video";
            m.missionDescription = $"Watch video by {brandName} and earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle}";
            m.progress = 1;
            m.brandName = brandName;
            m.claimButtonText = "Watch video";

            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);
        }

        private void AddMultiplyCoinsChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

                       
            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} multiply";
            m.missionDescription = $"Earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} and double it with {brandName}";
            m.progress = ((float)(getCurrencyFunc() - m.startMoney))/(float)m.reward;
            m.brandName = brandName;
            m.claimButtonText = "Claim reward";
            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);

            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;
        }

        private void AddSurveyChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} survey";
            m.missionDescription =
                $"Complete survey and earn {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} with {brandName}";
            m.progress = 1.0f; // ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;
            m.brandName = brandName;
            m.claimButtonText = "Start survey";
            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);
        }

        private void AddVideoGiveawayChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

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

            m.progress = 1;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;
            m.brandName = brandName;
            m.claimButtonText = needToPlayVideo ? "Watch video!" : "Claim reward!";
            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m,null,AddNewUIMissions);

            item.showGift = true;
            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;

            item.giftIcon.sprite = MissionsManager.GetMissionRewardImage(m);
            
            /*if (m.campaign.HasAsset(AssetsType.RewardSprite))
            {
                item.giftIcon.sprite = m.campaign.GetAsset<Sprite>(AssetsType.RewardSprite);
            }*/

        }

        private void AddMinigameChallenge(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            var getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} challenge";
            m.missionDescription = $"Complete challenge and get {MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle} from {brandName}";
            m.progress = 1;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;
            m.brandName = brandName;
            m.claimButtonText = "Play!";
            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);

        }


        private void AddSponsoredChallenge(Mission m, int missionId)
        {
            var go = GameObject.Instantiate<GameObject>(itemUI.gameObject, contentRoot);
            go.name = $"MonetizrRewardedItem{missionId}";
            
            var item = go.GetComponent<MonetizrRewardedItem>();

            m.rewardCenterItem = item;

            missionItems.Add(item);

            switch (m.type)
            {
                case MissionType.VideoReward: AddRewardedVideoChallenge(item, m,missionId); break;
                case MissionType.MutiplyReward: AddMultiplyCoinsChallenge(item, m,missionId); break;
                case MissionType.SurveyReward: AddSurveyChallenge(item, m, missionId); break;
                //case MissionType.TwitterReward: AddTwitterChallenge(item, m, missionId); break;
                //case MissionType.GiveawayWithMail: AddGiveawayChallenge(item, m, missionId); break;
                case MissionType.VideoWithEmailGiveaway: AddVideoGiveawayChallenge(item, m, missionId); break;
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward:
                case MissionType.ActionReward:
                    AddMinigameChallenge(item, m, missionId); break;
            }

            Log.Print(m.missionTitle);

            item.UpdateWithDescription(this, m, missionId);

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
            isSkipped = true;
            //MonetizrManager.CallUserDefinedEvent(currentCampaign,
            //  NielsenDar.GetPlacementName(AdPlacement.RewardsCenterScreen),
            //  MonetizrManager.EventType.ButtonPressSkip);

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

            //MonetizrManager.CallUserDefinedEvent(currentCampaign,
            //  NielsenDar.GetPlacementName(AdPlacement.RewardsCenterScreen),
            //  MonetizrManager.EventType.ButtonPressOk);

            MonetizrManager.Analytics.TrackEvent(currentMission, this, MonetizrManager.EventType.ButtonPressOk);

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

            UpdateStatusBar();
        }

        public void OnVideoPlayPress(Mission m, Action<bool> onComplete)
        {
            MonetizrManager.Instance.missionsManager.OnVideoPlayPress(m, onComplete);
        }

        //TODO: not sure if everything correct here
        internal override void FinalizePanel(PanelId id)
        {
            //MonetizrManager.Analytics.EndShowAdAsset(AdPlacement.RewardsCenterScreen, currentMission);

            //if(MonetizrManager.tinyTeaserCanBeVisible)
            MonetizrManager.ShowTinyMenuTeaser(null);

            if (!uiController.isVideoPlaying)
            {
                MonetizrManager.CleanUserDefinedMissions();
            }

            //if (hasSponsoredChallenges)
            //{
            //    MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
            //}

            
        }

        void UpdatePortraitMode()
        {
            if (!scrollListHasBanner && Utils.isInLandscapeMode())
                bannerHeight = 0;

                //float z = 0;
            Vector2 pos = new Vector2(510, -bannerHeight);


            foreach (var it in missionItems)
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

            //termsAndCondRect.anchoredPosition = pos;
            //pos.y -= termsAndCondRect.sizeDelta.y;

            contentRect.sizeDelta = -pos;
        }

        void UpdateLandscapeMode()
        {
            float shiftDelta = 45;

            if(scrollListHasBanner)
                bannerObjectRect.anchoredPosition = new Vector2(75 + shiftDelta, -940);

            float screenReferenceSizeX = 1920;
            //float screenReferenceSizeY = 1080;

            float blockDistanceY = 150;
            float blockDistanceX = 50;

            bannerHeight = 970 + shiftDelta;

            if (!scrollListHasBanner)
                bannerHeight = 0;

            float blockWidth = 1100;

            //float z = 0;

            float startX = bannerHeight + blockWidth / 2 + blockDistanceX;

            Vector2 pos = new Vector2(startX, 0);
            Vector2 size = Vector2.zero;
            Vector2 originalSize = contentRect.sizeDelta;

            int id = 0;

            foreach (var it in missionItems)
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

                if (id % 2 == 0)
                {
                    pos.y -= it.rect.sizeDelta.y + blockDistanceY;
                }
                else
                {
                   
                    pos.x += it.rect.sizeDelta.x + blockDistanceX;
                    pos.y = 0;
                }

                id++;
            }

            //termsAndCondRect.anchoredPosition = pos;

            //Log.Print(pos);
            //pos.y -= termsAndCondRect.sizeDelta.y;

            contentRect.sizeDelta = new Vector2(pos.x + blockWidth/2, originalSize.y);


        }

        //// Update is called once per frame
        void Update()
        {
            //if (Utils.isInLandscapeMode())
            //    UpdateLandscapeMode();
            //else
                UpdatePortraitMode();

        }
    }

}