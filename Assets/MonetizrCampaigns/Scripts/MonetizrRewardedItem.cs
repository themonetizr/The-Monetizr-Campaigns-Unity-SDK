using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class MonetizrRewardedItem : MonoBehaviour
    {
        public Image banner;
        public Image brandIcon;
        public Text rewardTitle;
        public Text rewardDescription;
        public ButtonController actionButton;
        public Image actionButtonImage;
        public Text boosterNumber;
        public Image boosterIcon;
        public GameObject progressBar;
        public Image rewardLine;
        public Text rewardPercent;
        public Text buttonText;
        //public Image backgoundImage;
        public Image backgroundImage2;
        public Image borderImage;

        public Sprite defaultBoosterIcon;

        public Image backgroundImage;
        public Image giftIcon;

        RewardCenterPanel rewardCenterPanel;
        internal Mission mission;

        bool updateWithTimer = false;
        DateTime lastUpdateTime;

        [HideInInspector]
        public ulong currectProgress;
        public ulong maxProgress;
        public bool showGift = false;

        public RectTransform rect;

        public GameObject hideOverlay;

        internal static string ScoreShow(double Score)
        {
            string result;
            string[] ScoreNames = new string[] { "", "k", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az", "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", };
            int i;

            for (i = 0; i < ScoreNames.Length; i++)
                if (Score < 900)
                    break;
                else Score = System.Math.Floor(Score / 100f) / 10f;

            if (Score == System.Math.Floor(Score))
                result = Score.ToString() + ScoreNames[i];
            else result = Score.ToString("F1") + ScoreNames[i];
            return result;
        }

        internal void UpdateWithDescription(RewardCenterPanel panel, Mission m)
        {
            rewardCenterPanel = panel;
            mission = m;

            m.brandBanner = null;

            banner.gameObject.SetActive(m.brandBanner != null);
            banner.sprite = m.brandBanner;

            

            if (m.brandBanner == null)
            {
                var rect = GetComponent<RectTransform>();

                rect.sizeDelta = new Vector2(1010, 410);
            }
                    
            if(!m.isSponsored)
            { 
                buttonText.text = "Claim reward";
            }
            else
            {                
                buttonText.text = m.claimButtonText;
            }


            brandIcon.sprite = m.missionIcon;
            rewardTitle.text = m.missionTitle;
            rewardDescription.text = m.missionDescription;

          
            hideOverlay.SetActive(m.showHidden);

            

            //active-deactive
            if(m.activateTime != DateTime.MinValue && m.deactivateTime != DateTime.MaxValue)
            //if (m.type == MissionType.SurveyReward)
            {
                updateWithTimer = true;
                lastUpdateTime = DateTime.Now.AddSeconds(1);

                //if(!isButtonClickable())
                //{
                    
                    updateButtonTimer();
                //}
            }

            actionButton.clickReceiver = this;

            //actionButton.onClick.AddListener( ()=> { md.onClaimButtonPress.Invoke(); });

            boosterNumber.text = $"+{ScoreShow(m.reward)}";

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.IngameRewardSprite) && m.isRewardIngame)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.IngameRewardSprite);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.RewardSprite) && !m.isRewardIngame)
            {
                rewardIcon = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.RewardSprite);
            }


            boosterIcon.sprite = rewardIcon == null ? defaultBoosterIcon : rewardIcon;

            boosterIcon.gameObject.SetActive(!showGift);

            giftIcon.gameObject.SetActive(showGift);

            rewardLine.fillAmount = m.progress;

            //rewardPercent.text = $"{md.progress*100.0f:F1}%";

            rewardPercent.text = $"{currectProgress}/{maxProgress}";

            if (m.progress < 1.0f) //reward isn't completed
            {
                progressBar.SetActive(true);
                actionButton.gameObject.SetActive(false);
            }
            else
            {
                progressBar.SetActive(false);
                actionButton.gameObject.SetActive(true);
            }

            //----

            UIController.PrepareCustomColors(backgroundImage, borderImage, m.campaignServerSettings.dictionary, PanelId.RewardCenter);
            UIController.PrepareCustomColors(null, backgroundImage2, m.campaignServerSettings.dictionary, PanelId.RewardCenter);


            foreach (var t in gameObject.GetComponents<PanelTextItem>())
                t.InitializeByParent(PanelId.RewardCenter, m);
        }

        internal void ButtonPressed(ButtonController buttonController)
        {
            if(isButtonClickable())
                rewardCenterPanel.ButtonPressed(buttonController, mission);
        }

        private bool isButtonClickable()
        {
            DateTime nowTime = DateTime.Now;

            return (nowTime >= mission.activateTime && nowTime <= mission.deactivateTime);
        }

        private void updateButtonTimer()
        {
            DateTime nowTime = DateTime.Now;

            if (nowTime <= mission.activateTime)
            {
                var dt = mission.activateTime - DateTime.Now;

                buttonText.text = $"Ready in {dt.Hours:D2}:{dt.Minutes:D2}:{dt.Seconds:D2}";

                actionButtonImage.color = Color.black;
            }
            else if(nowTime >= mission.activateTime && nowTime <= mission.deactivateTime)
            {
                var dt = mission.deactivateTime - DateTime.Now;

                buttonText.text = $"Claim in {dt.Hours:D2}:{dt.Minutes:D2}:{dt.Seconds:D2}";

                actionButtonImage.color = Color.white;
            }
            else if (nowTime >= mission.deactivateTime)
            {
                buttonText.text = $"Offer is over!";
                actionButtonImage.color = Color.black;
            }
        }
        // Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        void Update()
        {
            if(updateWithTimer)
            {
                if(DateTime.Now > mission.deactivateTime)
                {
                    updateButtonTimer();

                    updateWithTimer = false;
                }

                if(DateTime.Now > lastUpdateTime)
                {
                    lastUpdateTime = DateTime.Now.AddSeconds(1);

                    updateButtonTimer();
                }


            }
        }
    }
}