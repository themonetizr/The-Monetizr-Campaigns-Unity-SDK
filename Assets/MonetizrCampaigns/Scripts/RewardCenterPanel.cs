using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class RewardCenterPanel : PanelController
    {
        public Transform contentRoot;
        public RectTransform contentRect;
        public MonetizrRewardedItem itemUI;
        //private bool hasSponsoredChallenges;
        public Text headerText;
        public Text moneyText;
        public Image background;

        public Image mainBanner;
        public Image mainLogo;

        public GameObject banner;
        public RectTransform scrollViewRect;
        public ScrollRect scrollViewElement;

        public GameObject termsAndCondPrefab;


        //private RectTransform termsAndCondRect;

        private List<MonetizrRewardedItem> missionItems = new List<MonetizrRewardedItem>();

        private int amountOfItems = 0;
        private bool scrollListHasBanner;
        private float bannerHeight = 1150;

        private bool showNotClaimedDisabled = false;
        private List<Mission> missionsForRewardCenter;
        private ServerCampaign currentCampaign;

        //private bool isLandscape = false;

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
            Log.PrintV("UpdateUI");

            CleanListView();

            if (MonetizrManager.Instance.HasActiveCampaign())
            {
                //hasSponsoredChallenges = true;
                AddSponsoredChallenges();
            }

            //AddUserdefineChallenges();
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            currentMission = m;
            currentCampaign = MonetizrManager.Instance.GetActiveCampaign();

            //MonetizrManager.CallUserDefinedEvent(currentCampaign,
            //     NielsenDar.GetPlacementName(AdPlacement.RewardsCenterScreen),
            //     MonetizrManager.EventType.Impression);

            //string uiItemPrefab = "MonetizrRewardedItem";

            //if (uiVersion == 2)
            string uiItemPrefab = "MonetizrRewardedItem2";

            itemUI = (Resources.Load(uiItemPrefab) as GameObject).GetComponent<MonetizrRewardedItem>();


            //hasSponsoredChallenges = false;

            //this.missionsDescriptions = missionsDescriptions;

            //MonetizrManager.Analytics.BeginShowAdAsset(AdPlacement.RewardsCenterScreen, m);
            //MonetizrManager.Analytics.TrackEvent("Reward center opened",m);

            MonetizrManager.HideTinyMenuTeaser();

            this._onComplete = onComplete;

            UpdateUI();
        }

        private void AddSponsoredChallenges()
        {

            var campaign = MonetizrManager.Instance.GetActiveCampaign();

            if (campaign == null)
            {
                Log.PrintWarning("No active campaigns for RC!");
                return;
            }


            if (Utils.IsInLandscapeMode())
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

            var activeCampaign = MonetizrManager.Instance.GetActiveCampaign();

            missionsForRewardCenter = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(activeCampaign, true);

            if (missionsForRewardCenter.Count == 0)
            {
                Log.PrintWarning("No sponsored challenges for RC!");
                return;
            }

            var camp = missionsForRewardCenter[0].campaign;

            //mainBanner.sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            amountOfItems = 0;

            var hasBanner = camp.HasAsset(AssetsType.BrandBannerSprite);


            if (Utils.IsInLandscapeMode())
            {
                if (hasBanner)
                {
                    mainBanner.sprite = camp.GetAsset<Sprite>(AssetsType.BrandBannerSprite);

                    bool hasLogo = camp.HasAsset(AssetsType.BrandRewardLogoSprite);

                    if (hasLogo)
                    {
                        mainLogo.sprite = camp.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);
                    }
                }
                else
                {
                    bannerLayoutElement.SetActive(false);
                }
            }

            scrollListHasBanner = Utils.IsInLandscapeMode() ? false : hasBanner;

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

                bannerHeight = 1250;
            }
            else
            {
                //bannerHeight = 120;
            }

            //go.GetComponent<Image>().sprite = MonetizrManager.Instance.GetAsset<Sprite>(campId, AssetsType.BrandBannerSprite);

            foreach (var m in missionsForRewardCenter)
            {
                var ch = m.campaignId;

                m.showHidden = m.isDisabled;//showNotClaimedDisabled && m.isDisabled && m.isClaimed != ClaimState.Claimed;

                if (ch == missionsForRewardCenter[0].campaignId)
                {
                    var bgSprite = m.campaign.GetAsset<Sprite>(AssetsType.TiledBackgroundSprite);

                    if (bgSprite != default(Sprite))
                        background.sprite = bgSprite;
                }

                m.rewardCenterItem = null;

                if (m.state == MissionUIState.Hidden)
                    continue;

                AddSponsoredChallenge(m, amountOfItems);
                amountOfItems++;
            }

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            //Log.PrintError("UpdateStatusBar");

            var camp = MonetizrManager.Instance.GetActiveCampaign();

            if (camp == null)
                return;

            var statusText = camp.serverSettings.GetParam("RewardCenter.missions_num_text", "%claimed_missions%/%total_missions%");

            int claimed = 0;

            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(camp, true);

            double totalRewardsValue = 0;
            double claimedRewardsValue = 0;
            double possibleRewardsValue = 0;

            foreach (var m in missions)
            {
                totalRewardsValue += m.reward;
                if (m.isClaimed == ClaimState.Claimed)
                {
                    claimed++;
                    claimedRewardsValue += m.reward;
                }
                else
                {
                    possibleRewardsValue += m.reward;
                }
            }

            statusText = new StringBuilder(statusText)
                .Replace("%claimed_missions%", claimed.ToString())
                .Replace("%total_missions%", missions.Count.ToString())
                .ToString();

            var money = camp.serverSettings.GetParam("RewardCenter.money_num_text", "%claimed_reward_value%/%possible_reward_value%");

            var playerMoney = MonetizrManager.gameRewards[RewardType.Coins].GetCurrencyFunc();

            money = new StringBuilder(money)
                .Replace("%total_money%", $"{Utils.ScoresToString(playerMoney)}")
                .Replace("%total_rewards_value%", $"{Utils.ScoresToString(totalRewardsValue)}")
                .Replace("%claimed_reward_value%", $"{Utils.ScoresToString(claimedRewardsValue)}")
                .Replace("%possible_reward_value%", $"{Utils.ScoresToString(possibleRewardsValue)}")
                .ToString();

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
            m.missionDescription = $"Watch video by {brandName} and earn {Utils.ScoresToString(m.reward)} {rewardTitle}";
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

            Func<ulong> getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} multiply";
            m.missionDescription = $"Earn {Utils.ScoresToString(m.reward)} {rewardTitle} and double it with {brandName}";
            m.progress = ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;
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

            Func<ulong> getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} survey";
            m.missionDescription =
                $"Complete survey and earn {Utils.ScoresToString(m.reward)} {rewardTitle} with {brandName}";
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

            Func<ulong> getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} giveaway";

            bool needToPlayVideo = !(m.campaignServerSettings.GetParam("email_giveaway_mission_without_video") == "true");

#if UNITY_EDITOR_WIN
            needToPlayVideo = false;
#endif

            if (needToPlayVideo)
                // m.missionDescription = $"Watch video and get {Utils.ScoresToString(m.reward)} {rewardTitle} from {brandName}";
                m.missionDescription = $"Watch video and get $3 OFF Coupon from {brandName}";
            else
                m.missionDescription = $"Get {Utils.ScoresToString(m.reward)} {rewardTitle} from {brandName}";

            m.progress = 1;// ((float)(getCurrencyFunc() - m.startMoney)) / (float)m.reward;
            m.brandName = brandName;
            m.claimButtonText = needToPlayVideo ? "Watch video!" : "Claim reward!";
            m.onClaimButtonPress = MonetizrManager.Instance.missionsManager.ClaimAction(m, null, AddNewUIMissions);

            item.showGift = true;
            item.currectProgress = getCurrencyFunc() - m.startMoney;
            item.maxProgress = m.reward;

            item.giftIcon.sprite = MissionsManager.GetMissionRewardImage(m);

            /*if (m.campaign.HasAsset(AssetsType.RewardSprite))
            {
                item.giftIcon.sprite = m.campaign.GetAsset<Sprite>(AssetsType.RewardSprite);
            }*/

        }

        private void AddMission(MonetizrRewardedItem item, Mission m, int missionId)
        {
            string campaignId = m.campaignId;

            string brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;


            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            Func<ulong> getCurrencyFunc = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc;

            m.missionTitle = $"{brandName} challenge";
            m.missionDescription = $"Complete challenge and get {Utils.ScoresToString(m.reward)} {rewardTitle} from {brandName}";
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
                case MissionType.VideoReward: AddRewardedVideoChallenge(item, m, missionId); break;
                case MissionType.MutiplyReward: AddMultiplyCoinsChallenge(item, m, missionId); break;
                case MissionType.SurveyReward: AddSurveyChallenge(item, m, missionId); break;
                case MissionType.VideoWithEmailGiveaway: AddVideoGiveawayChallenge(item, m, missionId); break;
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward:
                case MissionType.ActionReward:
                    AddMission(item, m, missionId); break;
            }

            Log.PrintV(m.missionTitle);

            item.UpdateWithDescription(this, m, missionId);

            item.gameObject.SetActive(m.state != MissionUIState.Hidden);
            
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
            var activeCampaign = MonetizrManager.Instance.GetActiveCampaign();

            //try to update UI
            //var ml = MonetizrManager.Instance.missionsManager.missions;
            var ml = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(activeCampaign, true);

            foreach (var m in ml)
            {
                if (m.state == MissionUIState.ToBeShown && m.isClaimed != ClaimState.Claimed)
                {
                    m.state = MissionUIState.Visible;
                    m.isDisabled = false;
                    m.showHidden = m.isDisabled;

                    if (m.rewardCenterItem == null)
                    {
                        AddSponsoredChallenge(m, amountOfItems);
                        amountOfItems++;
                    }
                    else
                    {
                        //m.rewardCenterItem.gameObject.SetActive(true);
                        m.rewardCenterItem.hideOverlay.SetActive(false);
                    }
                    
                }
            }

            //var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(activeCampaign,true);

            if (ml.Count == 0)
                OnButtonPress();

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


            //MonetizrManager.ShowTinyMenuTeaser(null);

            if(MonetizrManager.tinyTeaserCanBeVisible)
                MonetizrManager.ShowTinyMenuTeaser(null);

            /*if (!uiController.isVideoPlaying)
            {
                MonetizrManager.CleanUserDefinedMissions();
            }*/

            //if (hasSponsoredChallenges)
            //{
            //    MonetizrManager.Analytics.EndShowAdAsset(AdType.IntroBanner);
            //}
           

        }

        void UpdateList()
        {
            if (Utils.IsInLandscapeMode())
                bannerHeight = 0;
            else if (!scrollListHasBanner)
                bannerHeight = 260;

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

        void Update()
        {
            UpdateList();
        }
    }

}