using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
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

        private Sprite brandBanner;
        private Sprite missionIcon;

        internal void UpdateWithDescription(RewardCenterPanel panel, Mission m, int id = 999)
        {
            rewardCenterPanel = panel;
            mission = m;
            missionIcon = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);

            brandBanner = null;

            banner.gameObject.SetActive(brandBanner != null);
            banner.sprite = brandBanner;
                        
            if (brandBanner == null)
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

            brandIcon.sprite = missionIcon;
            rewardTitle.text = m.missionTitle;
            rewardDescription.text = m.missionDescription;
          
            hideOverlay.SetActive(m.showHidden);

            if(m.activateTime != DateTime.MinValue && m.deactivateTime != DateTime.MaxValue)
            {
                updateWithTimer = true;
                lastUpdateTime = DateTime.Now.AddSeconds(1);
                updateButtonTimer();
            }

            actionButton.clickReceiver = this;
            boosterNumber.text = $"+{MonetizrUtils.ScoresToString(m.reward)}";
            Sprite rewardIcon = MissionsManager.GetMissionRewardImage(m);;

            boosterIcon.sprite = rewardIcon;
            giftIcon.sprite = rewardIcon;

            if (m.reward == 1 || m.reward == 0)
            {
                showGift = true;
            }

            boosterNumber.gameObject.SetActive(!showGift);
            boosterIcon.gameObject.SetActive(!showGift);
            giftIcon.gameObject.SetActive(showGift);

            rewardLine.fillAmount = m.progress;
            rewardPercent.text = $"{currectProgress}/{maxProgress}";

            if (m.progress < 1.0f)
            {
                progressBar.SetActive(true);
                actionButton.gameObject.SetActive(false);
            }
            else
            {
                progressBar.SetActive(false);
                actionButton.gameObject.SetActive(true);
            }

            actionButton.gameObject.name = $"RewardCenterButtonClaim{id}";

            UIController.PrepareCustomColors(backgroundImage, borderImage, m.campaignServerSettings, PanelId.RewardCenter);
            UIController.PrepareCustomColors(null, backgroundImage2, m.campaignServerSettings, PanelId.RewardCenter);

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