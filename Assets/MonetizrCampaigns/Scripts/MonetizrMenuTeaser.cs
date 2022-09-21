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
        private bool hasTextureAnimation = false;
        private bool hasAnimation = true;

        private Mission currentMission;

        public Image singleBackgroundImage;
        public Image watchVideoIcon;
        public RectTransform buttonTextRect;

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

        public void OnButtonClick()
        {
            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.TinyTeaser), MonetizrManager.EventType.ButtonPressOk);

            MonetizrManager.Analytics.TrackEvent("Tiny teaser pressed", currentMission);
            MonetizrManager.ShowRewardCenter(null);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            currentMission = m;

            PreparePanelVersion2(id, onComplete, m);

            /*switch (uiVersion)
            {
                case 2:
                case 3:
                    PreparePanelVersion2(id, onComplete, m); break;
                default:
                    PreparePanelDefaultVersion(id, onComplete, m); break;
            }*/

            Log.PrintWarning($"{m.campaignId} {m}");
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.TinyTeaser, m);

            MonetizrManager.Analytics.TrackEvent("Tiny teaser shown", m);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.TinyTeaser), MonetizrManager.EventType.Impression);
        }

        internal void UpdateTransform(Mission m)
        {
            string teaser_transform = m.additionalParams.GetParam("teaser_transform");

            if (teaser_transform == null)
                return;

            RectTransform rt = GetComponent<RectTransform>();

            //float[] values = null;


            string[] arr = teaser_transform.Split(',');

            if (arr.Length == 0)
                return;

            var values = new List<float>(0);

            Array.ForEach(arr,s =>
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
                if(values[2] != 0 && values[3] != 0)
                    rt.sizeDelta = new Vector2(values[2], values[3]);
            } 

            if (values.Count == 5)
                rt.localScale = Vector3.one * (values[4] / 100.0f);
        }

        internal void PreparePanelVersion2(PanelId id, Action<bool> onComplete, Mission m)
        {
            bool noVideo = false;

            if (m.additionalParams.GetParam("email_giveaway_mission_without_video") == "true")
                noVideo = true;

            bool isSinglePicture = m.additionalParams.GetParam("teaser_single_picture") == "true";
                        

            UpdateTransform(m);

            if (isSinglePicture)
            {
                Array.ForEach(gameObject.GetComponentsInChildren<RectTransform>(),
                    (RectTransform r) => { if (r.gameObject != gameObject) r.gameObject.SetActive(false); });

                

                /*foreach (RectTransform o in gameObject.GetComponentsInChildren<RectTransform>())
                {
                    o.gameObject.SetActive(false);
                }*/

                //gameObject.SetActive(true);

                singleBackgroundImage.enabled = true;


                if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.TinyTeaserSprite))
                    singleBackgroundImage.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.TinyTeaserSprite);

                return;
            }

            if (uiVersion == 3)
            {
                //TODO: action without video
                if (noVideo)
                {
                    watchVideoIcon.gameObject.SetActive(false);
                    buttonTextRect.anchoredPosition = new Vector2(0, 0);
                }
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.TeaserGifPathString))
            {
                string url = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.TeaserGifPathString);
                    
                gifImage.SetGifFromUrl(url);
            }

            
            //var campaign = MonetizrManager.Instance.GetCampaign(challengeId);

            /*if (m.additionalParams.GetParam("teaser_no_texture_animation") == "true")
            {
                hasTextureAnimation = false;
            }

            if (m.additionalParams.GetParam("teaser_no_animation") == "true")
            {
                hasAnimation = false;
            }*/

            bool showReward = false;

           /*if (m.additionalParams.GetParam("show_reward_on_teaser") == "true")
            {
                hasTextureAnimation = false;
                showReward = true;
            }*/

            if (!hasTextureAnimation)
            {
                teaserImage.uvRect = new Rect(0, 0, 1, 1);
            }

            if (!hasAnimation)
            {
                scaleAnimator.speed = 0;
                scaleAnimator.enabled = false;
            }

            earnText.gameObject.SetActive(true);
            rewardImage.gameObject.SetActive(false);
            rewardText.gameObject.SetActive(true);

            if (!showReward)
            {
                //teaserImage.texture = MonetizrManager.Instance.GetAsset<Texture2D>(m.campaignId, AssetsType.TinyTeaserTexture);
            }
            else
            {
                gifImage.enabled = false;
                gifImage.gameObject.SetActive(false);

                rewardImage.sprite = MonetizrManager.gameRewards[m.rewardType].icon;
                rewardImage.gameObject.SetActive(true);
                //rewardImage.sprite = MonetizrManager.gameRewards[m.rewardType].icon;
                //rewardText.text = $"+{m.reward}";
            }
                        

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

 


            /*MonetizrManager.Analytics.TrackEvent("Reward center opened", m);
            MonetizrManager.Analytics.TrackEvent("Html5 completed", m);
            MonetizrManager.Analytics.TrackEvent("Html5 skipped", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 1 shown", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 2 shown", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 3 shown", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 4 shown", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 5 shown", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 6 shown", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 7 shown", m);
            
            MonetizrManager.Analytics.TrackEvent("Branded Mission 1 pressed", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 2 pressed", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 3 pressed", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 4 pressed", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 5 pressed", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 6 pressed", m);
            MonetizrManager.Analytics.TrackEvent("Branded Mission 7 pressed", m);
            
            MonetizrManager.Analytics.TrackEvent("Minigame started", m);
            MonetizrManager.Analytics.TrackEvent("Minigame skipped", m);
            MonetizrManager.Analytics.TrackEvent("Minigame completed", m);
            MonetizrManager.Analytics.TrackEvent("Survey started", m);
            MonetizrManager.Analytics.TrackEvent("Survey skipped", m);
            MonetizrManager.Analytics.TrackEvent("Survey completed", m);
            MonetizrManager.Analytics.TrackEvent("Enter email submitted", m);
            MonetizrManager.Analytics.TrackEvent("Email enter skipped", m);
            MonetizrManager.Analytics.TrackEvent("Email congrats shown", m);

            MonetizrManager.Analytics.TrackEvent("Custom event shown", m);
            MonetizrManager.Analytics.TrackEvent("Custom event pressed", m);*/


        }

        /*internal void PreparePanelDefaultVersion(PanelId id, Action<bool> onComplete, Mission m)
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

            

        }*/

        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(AdType.TinyTeaser);
        }
    }

}