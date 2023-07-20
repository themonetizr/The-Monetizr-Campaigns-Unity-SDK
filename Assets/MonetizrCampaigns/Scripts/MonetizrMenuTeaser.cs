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
        //private Material m = null;
        private float delayTimeEnd = 0f;
        private float speed = 1f;
        private Rect uvRect = new Rect(0, 0.5f, 1.0f, 0.5f);
        private bool hasTextureAnimation = false;
        private bool hasAnimation = true;

        //rivate Mission currentMission;

        public Image singleBackgroundImage;
        public Image watchVideoIcon;
        public RectTransform buttonTextRect;

        public GameObject animatableBannerRoot;
        public Image rectangeBannerImage;
        public Image bannerRewardImage;

        public Text missionsNum;

        public GameObject buttonObject;
        public GameObject numberObject;

        internal override AdPlacement? GetAdPlacement()
        {
            return AdPlacement.TinyTeaser;
        }

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
            //MonetizrManager.Analytics.TrackEvent(currentMission, AdPlacement.TinyTeaser, MonetizrManager.EventType.ButtonPressOk);

            //MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.TinyTeaser), MonetizrManager.EventType.ButtonPressOk);

            MonetizrManager.Analytics.TrackEvent(currentMission, this, MonetizrManager.EventType.ButtonPressOk);

            //MonetizrManager.Analytics.TrackEvent("Tiny teaser pressed", currentMission);
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

            Log.Print($"PreparePanel teaser: {m.campaignId} {m}");



            //MonetizrManager.Analytics.BeginShowAdAsset(AdPlacement.TinyTeaser, m);
            //MonetizrManager.Analytics.TrackEvent("Tiny teaser shown", m);
            //MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdPlacement.TinyTeaser), MonetizrManager.EventType.Impression);
        }

        internal void UpdateTransform(Mission m)
        {
            string teaser_transform = m.campaignServerSettings.GetParam("teaser_transform");

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
            bool noVideo = !m.hasVideo;

            //if there's video and it's already shown
            if (!noVideo && m.isVideoShown)
                noVideo = true;

            //more than one mission - no video sign
            if (MonetizrManager.Instance.missionsManager.GetActiveMissionsNum() > 1)
                noVideo = true;

            //if (m.campaignServerSettings.GetParam("email_giveaway_mission_without_video") == "true")
            //    noVideo = true;

            bool isSinglePicture = m.campaignServerSettings.GetParam("teaser_single_picture") == "true";
                        

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

            if (uiVersion == 3)
            {
                //TODO: action without video
                if (noVideo)
                {
                    watchVideoIcon.gameObject.SetActive(false);
                    buttonTextRect.anchoredPosition = new Vector2(0, 0);
                }
            }

            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(true);

            var numText = currentMission.campaignServerSettings.GetParam("teaser.num_text", "%total_missions%");

            numText = numText.Replace("%total_missions%", $"{missions.Count}");

            missionsNum.text = numText;

            var hasGif = m.campaign.HasAsset(AssetsType.TeaserGifPathString);

            var showButton = currentMission.campaignServerSettings.GetBoolParam("teaser.show_button", true);

            buttonObject.SetActive(showButton);

            var showNumber = currentMission.campaignServerSettings.GetBoolParam("teaser.show_number", true);



            numberObject.SetActive(showNumber);

            //hasGif = false;

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
            }
                        

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            rewardText.text = rewardText.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
           
        }

        

        internal override void FinalizePanel(PanelId id)
        {
            //Moved to HideTinyMenuTeaser
            //MonetizrManager.Analytics.EndShowAdAsset(AdType.TinyTeaser);
        }

       
    }

}