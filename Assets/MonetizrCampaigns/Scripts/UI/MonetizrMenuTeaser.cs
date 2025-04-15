using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.GIF;
using Monetizr.SDK.Missions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.UI;
using EventType = Monetizr.SDK.Core.EventType;

namespace Monetizr.SDK.UI
{
    internal class MonetizrMenuTeaser : PanelController
    {
        public GifImage gifImage;
        public Button button;
        public RawImage teaserImage;
        public float delayTime = 5f;
        public float moveTime = 1f;
        public RectTransform rectTransform;
        public Animator scaleAnimator;
        public Text earnText;
        public Image rewardImage;
        public Text rewardText;
        public Image singleBackgroundImage;
        public Image watchVideoIcon;
        public RectTransform buttonTextRect;
        public GameObject animatableBannerRoot;
        public Image rectangeBannerImage;
        public Image bannerRewardImage;
        public Text missionsNum;
        public GameObject buttonObject;
        public GameObject numberObject;
        public GameObject raysObject;

        internal override AdPlacement? GetAdPlacement()
        {
            return AdPlacement.TinyTeaser;
        }
        public void OnButtonClick()
        {
            MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.ButtonPressOk);
            MonetizrManager.ShowRewardCenter(null);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            currentMission = m;
            var noVideo = !m.hasVideo;
            if (!noVideo && m.isVideoShown) noVideo = true;
            if (MonetizrManager.Instance.missionsManager.GetActiveMissionsNum(m.campaign) > 1) noVideo = true;
            var isSinglePicture = m.campaignServerSettings.GetBoolParam("teaser.single_picture", false);
            UpdateTransform(m);

            if (isSinglePicture)
            {
                Array.ForEach(gameObject.GetComponentsInChildren<RectTransform>(),
                    (RectTransform r) => { if (r.gameObject != gameObject) r.gameObject.SetActive(false); });

                singleBackgroundImage.enabled = true;

                if (m.campaign.HasAsset(AssetsType.TinyTeaserSprite))
                    singleBackgroundImage.sprite = m.campaign.GetAsset<Sprite>(AssetsType.TinyTeaserSprite);

                return;
            }

            noVideo = false;

            var noVideoSign = m.campaignServerSettings.GetBoolParam("teaser.no_video_sign", true);

            if (noVideo || noVideoSign)
            {
                watchVideoIcon.gameObject.SetActive(false);
                buttonTextRect.anchoredPosition = new Vector2(0, 0);
            }

            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(m.campaign, true);
            var numText = currentMission.campaignServerSettings.GetParam("teaser.num_text", "%total_missions%");
            numText = numText.Replace("%total_missions%", $"{missions.Count}");
            UpdateMissionAmountText();

            var hasGif = m.campaign.HasAsset(AssetsType.TeaserGifPathString);
            var showButton = currentMission.campaignServerSettings.GetBoolParam("teaser.show_button", true);
            buttonObject.SetActive(showButton);
            var showNumber = currentMission.campaignServerSettings.GetBoolParam("teaser.show_number", true);
            numberObject.SetActive(showNumber);

            if (hasGif)
            {
                string url = m.campaign.GetAsset<string>(AssetsType.TeaserGifPathString);
                gifImage.gameObject.SetActive(true);
                gifImage.SetGifFromUrl(url);
                animatableBannerRoot?.SetActive(false);
            }
            else
            {
                gifImage.gameObject.SetActive(false);
                animatableBannerRoot.SetActive(true);
                bannerRewardImage.sprite = MissionsManager.GetMissionRewardImage(m);
                rectangeBannerImage.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);
            }

            earnText.gameObject.SetActive(true);
            rewardImage.gameObject.SetActive(false);
            rewardText.gameObject.SetActive(true);
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;
            rewardText.text = rewardText.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
            MonetizrLogger.Print($"PreparePanel teaser: {m.campaignId} {m}");
        }

        private void UpdateMissionAmountText()
        {
            var missions = MonetizrManager.Instance.missionsManager.GetAllMissions();
            int totalMissionCount = missions.Count;
            int claimed = 0;

            foreach (var m in missions)
            {
                if (m.isClaimed == ClaimState.Claimed)
                {
                    claimed++;
                }
            }

            int currentMissionCount = Mathf.Max(1, (totalMissionCount - claimed));
            missionsNum.text = currentMissionCount.ToString();
        }

        internal void UpdateTransform(Mission m)
        {
            string teaser_transform = m.campaignServerSettings.GetParam("teaser_transform");

            if (teaser_transform == null)
                return;

            RectTransform rt = GetComponent<RectTransform>();

            string[] arr = teaser_transform.Split(',');

            if (arr.Length == 0)
                return;

            var values = new List<float>(0);

            Array.ForEach(arr, s =>
             {
                 float f = 0;

                 if (!float.TryParse(s, out f))
                     return;

                 values.Add(f);
             });

            if (values.Count >= 2)
            {
                if (values[0] != 0 && values[1] != 0)
                    rt.anchoredPosition = new Vector2(values[0], values[1]);
            }

            if (values.Count >= 4)
            {
                if (values[2] != 0 && values[3] != 0)
                    rt.sizeDelta = new Vector2(values[2], values[3]);
            }

            if (values.Count == 5)
                rt.localScale = Vector3.one * (values[4] / 100.0f);
        }

        internal override void FinalizePanel(PanelId id)
        {
            
        }

    }

}