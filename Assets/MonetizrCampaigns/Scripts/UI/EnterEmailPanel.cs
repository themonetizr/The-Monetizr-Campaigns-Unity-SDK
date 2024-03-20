using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class EnterEmailPanel : PanelController
    {
        public Image banner;
        public Image rewardImageBackgroud;
        public Image rewardImage;
        public Text rewardAmount;
        public Text title;
        public Text text;
        public Image logo;
        public Button closeButton;
        public Text buttonText;
        public Button noThanksButton;
        public InputField inputField;

        public GameObject singleRewardRoot;
        public GameObject selectRewardRoot;

        public Image selection1Icon;
        public Image selection2Icon;

        public Animator crossButtonAnimator;

        //[HideInInspector]
        //public Mission currentMission;
        private Regex validateEmailRegex;
        private string result;

        private EnterEmailType enterEmailType;

        private MonetizrManager.RewardSelectionType selection;
        private AdPlacement adType;

        public Toggle termsToggle;
        private bool termsTogglePressed;
        //private Action _onComplete;

        internal override AdPlacement? GetAdPlacement()
        {
            return adType;
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;
            this.triggersButtonEventsOnDeactivate = false;

            closeButton.onClick.AddListener(OnButtonPress);
            noThanksButton?.onClick.AddListener(OnNoThanksPress);

            inputField.onValueChanged.AddListener(OnInputFieldChanged);
            closeButton.interactable = false;

            validateEmailRegex = new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$");

            int closeButtonDelay = m.campaignServerSettings.GetIntParam("email_enter_close_button_delay",0);

            StartCoroutine(ShowCloseButton(closeButtonDelay));

            PreparePanel(m);

            UpdateEnterFieldVisibility(false);
            

            StartCoroutine(UpdateHorizontalLayoutHack());
        }

        IEnumerator UpdateHorizontalLayoutHack()
        {
            yield return 0;

            var hg = termsToggle.GetComponent<HorizontalLayoutGroup>();
            hg.enabled = false;
            hg.enabled = true;
        }

        IEnumerator ShowCloseButton(float time)
        {
            crossButtonAnimator.enabled = false;

            yield return new WaitForSeconds(time);

            crossButtonAnimator.enabled = true;
        }

        internal void OnInputFieldChanged(string s)
        {
            var isValid = false;
            string lowerCaseMail = null;

            if (s != null && s.Length > 0)
            {
                lowerCaseMail = s.ToLower();

                //check email with regex
                isValid = validateEmailRegex.IsMatch(lowerCaseMail);
            }

            //double check with MailAddress
            if (isValid)
            {
                try
                {
                    MailAddress address = new MailAddress(lowerCaseMail);
                    isValid = (address.Address == lowerCaseMail);
                }
                catch (FormatException)
                {
                    // address is invalid
                    isValid = false;
                }
            }

            //email valid, but country code is too short
            if (isValid && lowerCaseMail.Length - lowerCaseMail.LastIndexOf('.') <= 2)
                isValid = false;

            UpdateEnterFieldVisibility(isValid);

            result = s;
        }

        internal void UpdateEnterFieldVisibility(bool isTextValid)
        {
            bool isOn = termsToggle.gameObject.activeSelf ? termsToggle.isOn : true;

            closeButton.interactable = isTextValid && isOn;
        }

        internal override void FinalizePanel(PanelId id)
        {
            //fail test
            if (result == "aa@aa.aa")
                result = "asdfqe qe qefqwe";

            MonetizrManager.temporaryEmail = result;
            MonetizrManager.temporaryRewardTypeSelection = selection;

            //MonetizrManager.analytics.EndShowAdAsset(adType, currentMission);
        }

        internal static EnterEmailType GetPanelType(Mission m)
        {
            var s = m.campaignServerSettings.GetParam("email_giveaway_type");

            switch(s)
            {
                case "product_reward":
                    //selection = MonetizrManager.RewardSelectionType.Product;
                    return EnterEmailType.ProductReward;
                case "ingame_reward":
                    //selection = MonetizrManager.RewardSelectionType.Ingame;
                    return EnterEmailType.IngameReward;

                case "selection_reward": return EnterEmailType.SelectionReward;
            }

            return EnterEmailType.ProductReward;
        }

        public void OnFirstToggle(bool v)
        {
            if(v) selection = MonetizrManager.RewardSelectionType.Ingame;
        }
        
        public void OnSecondToggle(bool v)
        {
            if(v) selection = MonetizrManager.RewardSelectionType.Product;
        }

        public void OnTermsToggle(bool v)
        {
            OnInputFieldChanged(result);
        }

        private void PreparePanel(Mission m)
        {
            EnterEmailType type = GetPanelType(m);

            if (type == EnterEmailType.ProductReward)
            {
                selection = MonetizrManager.RewardSelectionType.Product;
                adType = AdPlacement.EmailEnterCouponRewardScreen;
            }
            else if (type == EnterEmailType.IngameReward)
            {
                selection = MonetizrManager.RewardSelectionType.Ingame;
                adType = AdPlacement.EmailEnterInGameRewardScreen;
            }
            else
            {
                adType = AdPlacement.EmailEnterSelectionRewardScreen;
            }

            m.adPlacement = adType;

            var challengeId = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            banner.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandBannerSprite);
            banner.color = Color.white;

            if (m.campaign.HasAsset(AssetsType.BrandRewardLogoSprite))
            {
                logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);
            }
            else
            {
                logo.gameObject.SetActive(false);
            }

            rewardAmount.text = m.reward.ToString();

            string brandTitle = m.brandName;

            var r = m.campaign.rewards.Find((ServerCampaign.Reward obj) => { return obj.claimable == true;  });

            string giveawayTitle = "";

            if (r != null)
            {
                giveawayTitle = r.title;
            }

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;
            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            if (m.rewardType == RewardType.Coins && m.campaign.HasAsset(AssetsType.CustomCoinString))
            {
                rewardTitle = m.campaign.GetAsset<string>(AssetsType.CustomCoinString);
            }

            Sprite customCoin = m.campaign.GetAsset<Sprite>( AssetsType.CustomCoinSprite);

            if (m.rewardType == RewardType.Coins && customCoin != null)
            {
                rewardIcon = customCoin;
            }

            string url = m.campaign.serverSettings.GetParam("GiveawayEmailEnterNotification.terms_url_text");

            if (url == null)
                termsToggle.gameObject.SetActive(false);

            
            text.text = text.text.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");

            if (type == EnterEmailType.ProductReward)
            {
                singleRewardRoot.SetActive(true);
                selectRewardRoot.SetActive(false);

                rewardImage.sprite = m.campaign.GetAsset<Sprite>(AssetsType.RewardSprite);
            }

            if (type == EnterEmailType.IngameReward)
            {
                singleRewardRoot.SetActive(true);
                selectRewardRoot.SetActive(false);

                rewardImage.sprite = MonetizrManager.gameRewards[m.rewardType].icon;

                if (m.campaign.HasAsset(AssetsType.IngameRewardSprite))
                    rewardImage.sprite = m.campaign.GetAsset<Sprite>(AssetsType.IngameRewardSprite);
            }

            if(type == EnterEmailType.SelectionReward)
            {
                singleRewardRoot.SetActive(false);
                selectRewardRoot.SetActive(true);
                
                selection1Icon.sprite = MonetizrManager.gameRewards[m.rewardType].icon;

                if (m.campaign.HasAsset(AssetsType.IngameRewardSprite))
                    selection1Icon.sprite = m.campaign.GetAsset<Sprite>(AssetsType.IngameRewardSprite);

                selection2Icon.sprite = m.campaign.GetAsset<Sprite>(AssetsType.RewardSprite);
            }

            //TODO: needs to be checked carefully!!!
            rewardImage.sprite = MissionsManager.GetMissionRewardImage(m);
            
            
            //rewardImage.gameObject.SetActive(false);
            rewardAmount.gameObject.SetActive(false);
            rewardImageBackgroud.gameObject.SetActive(false);
            noThanksButton?.gameObject.SetActive(true);

        }

        private new void Awake()
        {
            base.Awake();


        }

        public void _OnNoThanksPress()
        {
            isSkipped = true;
            SetActive(false);
        }

        public void OnNoThanksPress()
        {
            MonetizrManager.ShowMessage((bool _isSkipped) =>
                {
                    if(!_isSkipped)
                    {
                        _OnNoThanksPress();
                    }
                },
                this.currentMission,
                PanelId.EmailEnterCloseConfirmation);
           
        }

        public void OnButtonPress()
        {
            isSkipped = false;
            SetActive(false);
        }

    }

}