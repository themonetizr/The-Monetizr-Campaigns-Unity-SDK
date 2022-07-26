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

        public Sprite defaultBoosterIcon;

        public Image backgroundImage;
        public Image giftIcon;

        RewardCenterPanel rewardCenterPanel;
        internal Mission mission;

        bool updateWithTimer = false;
        DateTime lastUpdateTime;

        [HideInInspector]
        public int currectProgress;
        public int maxProgress;
        public bool showGift = false;

        public RectTransform rect;

        internal void UpdateWithDescription(RewardCenterPanel panel, Mission md)
        {
            rewardCenterPanel = panel;
            mission = md;

            md.brandBanner = null;

            banner.gameObject.SetActive(md.brandBanner != null);
            banner.sprite = md.brandBanner;

            

            if (md.brandBanner == null)
            {
                var rect = GetComponent<RectTransform>();

                rect.sizeDelta = new Vector2(1010, 410);
            }
                    
            if(!md.isSponsored)
            { 
                buttonText.text = "Claim reward";
            }
            else
            {                
                buttonText.text = md.claimButtonText;
            }


            brandIcon.sprite = md.missionIcon;
            rewardTitle.text = md.missionTitle;
            rewardDescription.text = md.missionDescription;

            //custom colors
            var ch = MonetizrManager.Instance.GetActiveCampaign();

            if (ch != null)
            {

                var color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.CampaignHeaderTextColor);

                if (color != default(Color))
                    rewardTitle.color = color;

                color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.CampaignTextColor);

                if (color != default(Color))
                {
                    rewardDescription.color = color;
                    boosterNumber.color = color;
                }

                color = MonetizrManager.Instance.GetAsset<Color>(ch, AssetsType.CampaignBackgroundColor);

                if (color != default(Color))
                    backgroundImage.color = color;

            }

            //active-deactive
            if (md.type == MissionType.SurveyReward)
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

            boosterNumber.text = $"+{md.reward}";

            Sprite rewardIcon = MonetizrManager.gameRewards[md.rewardType].icon;

            Sprite customCoin = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.CustomCoinSprite);

            if (md.rewardType == RewardType.Coins && customCoin != null)
                rewardIcon = customCoin;

            boosterIcon.sprite = rewardIcon == null ? defaultBoosterIcon : rewardIcon;

            boosterIcon.gameObject.SetActive(!showGift);

            giftIcon.gameObject.SetActive(showGift);

            rewardLine.fillAmount = md.progress;

            //rewardPercent.text = $"{md.progress*100.0f:F1}%";

            rewardPercent.text = $"{currectProgress}/{maxProgress}";

            if (md.progress < 1.0f) //reward isn't completed
            {
                progressBar.SetActive(true);
                actionButton.gameObject.SetActive(false);
            }
            else
            {
                progressBar.SetActive(false);
                actionButton.gameObject.SetActive(true);
            }
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