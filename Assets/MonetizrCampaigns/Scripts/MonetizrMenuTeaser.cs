using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
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

        private int state = 0;
        private float progress = 0f;
        private Material m = null;
        private float delayTimeEnd = 0f;
        private float speed = 1f;
        private Rect uvRect = new Rect(0, 0.5f, 1.0f, 0.5f);
        private bool hasTextureAnimation = true;
        private bool hasAnimation = true;

        void Update()
        {
            if (!hasTextureAnimation)
                return;

            switch (state)
            {
                case 0:
                    progress += speed * Time.deltaTime / moveTime;

                    if (progress > 1.0f || progress < 0.0f)
                    {
                        progress = Mathf.Clamp(progress, 0f, 1f);
                        speed *= -1;
                        delayTimeEnd = Time.time + delayTime;
                        state = 1;
                    }
                    SetProgress(progress);

                    break;

                case 1:

                    if (Time.time > delayTimeEnd)
                    {
                        state = 0;
                    }

                    break;
            }

        }

        void SetProgress(float a)
        {
            uvRect.y = 0.5f * (1.0f - Tween(a));
            teaserImage.uvRect = uvRect;
        }

        float Tween(float k)
        {
            return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            switch(uiVersion)
            {
                case 2: PreparePanelVersion2(id, onComplete, m); break;
                default:
                    PreparePanelDefaultVersion(id, onComplete, m); break;
            }
        }

        internal void PreparePanelVersion2(PanelId id, Action<bool> onComplete, Mission m)
        {

//TODO: update gif loader
//#if !UNITY_EDITOR
            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.TeaserGifPathString))
            {
                string url = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.TeaserGifPathString);

                gifImage.SetGifFromUrl(url);
            }
//#endif


            //var campaign = MonetizrManager.Instance.GetCampaign(challengeId);

            if (m.additionalParams.GetParam("teaser_no_texture_animation") == "true")
            {
                hasTextureAnimation = false;
            }

            if (m.additionalParams.GetParam("teaser_no_animation") == "true")
            {
                hasAnimation = false;
            }

            bool showReward = false;

            if (m.additionalParams.GetParam("show_reward_on_teaser") == "true")
            {
                hasTextureAnimation = false;
                showReward = true;
            }

            if (!hasTextureAnimation)
            {
                teaserImage.uvRect = new Rect(0, 0, 1, 1);
            }

            if (!hasAnimation)
            {
                scaleAnimator.speed = 0;
                scaleAnimator.enabled = false;
            }

            if (!showReward)
            {
                teaserImage.texture = MonetizrManager.Instance.GetAsset<Texture2D>(m.campaignId, AssetsType.TinyTeaserTexture);
            }
            else
            {
                rewardImage.sprite = MonetizrManager.gameRewards[m.rewardType].icon;
                //rewardText.text = $"+{m.reward}";
            }

            earnText.gameObject.SetActive(true);
            rewardImage.gameObject.SetActive(false);
            rewardText.gameObject.SetActive(true);

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            rewardText.text = rewardText.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
            //if (!showReward)
            //{
            //    teaserImage.texture = MonetizrManager.Instance.GetAsset<Texture2D>(challengeId, AssetsType.TinyTeaserTexture);
            //}
            //else
            //{

            //string header = m.additionalParams.GetParam("TinyMenuTeaser.header");

            //if(header.Length > 0)
            //{
            //    earnText.text = header;
            //}

            //rewardImage.sprite = MonetizrManager.gameRewards[m.rewardType].icon;
            //rewardText.text = $"Watch {m.brandName} video &\nget $3 coupon!";
            //}

            Log.PrintWarning($"{m.campaignId} {m}");
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.TinyTeaser, m);

        }

        internal void PreparePanelDefaultVersion(PanelId id, Action<bool> onComplete, Mission m)
        {
            
            //var campaign = MonetizrManager.Instance.GetCampaign(challengeId);

            if (m.additionalParams.GetParam("teaser_no_texture_animation") == "true")
            {
                hasTextureAnimation = false;
            }

            if(m.additionalParams.GetParam("teaser_no_animation") == "true")
            {
                hasAnimation = false;
            }

            bool showReward = false;

            if(m.additionalParams.GetParam("show_reward_on_teaser") == "true")
            {
                hasTextureAnimation = false;
                showReward = true;
            }

            if (!hasTextureAnimation)
            {
                teaserImage.uvRect = new Rect(0, 0, 1, 1);
            }

            if (!hasAnimation)
            {
                scaleAnimator.speed = 0;
                scaleAnimator.enabled = false;
            }

            earnText.gameObject.SetActive(showReward);
            rewardImage.gameObject.SetActive(showReward);
            rewardText.gameObject.SetActive(showReward);

            if (!showReward)
            {
                teaserImage.texture = MonetizrManager.Instance.GetAsset<Texture2D>(m.campaignId, AssetsType.TinyTeaserTexture);
            }
            else
            {
                rewardImage.sprite = MonetizrManager.gameRewards[m.rewardType].icon;
                rewardText.text = $"+{m.reward}";
            }

            Log.PrintWarning($"{m.campaignId} {m}");
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.TinyTeaser, m);

        }

        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(AdType.TinyTeaser);
        }
    }

}