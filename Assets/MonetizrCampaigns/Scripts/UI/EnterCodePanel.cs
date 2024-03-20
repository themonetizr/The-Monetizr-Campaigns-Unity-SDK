using Monetizr.SDK.Analytics;
using Monetizr.SDK.Missions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK
{
    internal class EnterCodePanel : PanelController
    {
        public Image rewardImage;
        public Text title;
        public Text text;
        public Image logo;
        public Button okButton;
        public Text buttonText;
        public Button closeButton;
        public InputField inputField;

        public Animator crossButtonAnimator;
        private const AdPlacement AdType = AdPlacement.CodeEnterRewardScreen;

        internal override AdPlacement? GetAdPlacement()
        {
            return AdType;
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;
            this.triggersButtonEventsOnDeactivate = false;

            okButton.onClick.AddListener(OnOkPress);
            closeButton.onClick.AddListener(OnClosePress);

            inputField.onValueChanged.AddListener(OnInputFieldChanged);
            okButton.interactable = false;

            var closeButtonDelay = m.campaignServerSettings.GetIntParam("email_enter_close_button_delay", 0);

            StartCoroutine(ShowCloseButton(closeButtonDelay));

            PreparePanel(m);

            UpdateEnterFieldVisibility(false);
        }

        private IEnumerator ShowCloseButton(float time)
        {
            crossButtonAnimator.enabled = false;

            yield return new WaitForSeconds(time);

            crossButtonAnimator.enabled = true;
        }

        internal void OnInputFieldChanged(string s)
        {
            var isValid = s == currentMission.campaign.serverSettings.GetParam("CodeReward.code");

            UpdateEnterFieldVisibility(isValid);
        }

        internal void UpdateEnterFieldVisibility(bool isTextValid)
        {
            okButton.interactable = isTextValid;
        }

        internal override void FinalizePanel(PanelId id)
        {

        }

        private void PreparePanel(Mission m)
        {
            m.adPlacement = AdType;
            
            if (m.campaign.HasAsset(AssetsType.BrandRewardLogoSprite))
            {
                logo.sprite = m.campaign.GetAsset<Sprite>(AssetsType.BrandRewardLogoSprite);
            }
            else
            {
                logo.gameObject.SetActive(false);
            }
            
            rewardImage.sprite = MissionsManager.GetMissionRewardImage(m);
            
            closeButton.gameObject.SetActive(true);
        }

        private new void Awake()
        {
            base.Awake();
        }

        private void _OnNoThanksPress()
        {
            isSkipped = true;
            SetActive(false);
        }

        public void OnClosePress()
        {
            MonetizrManager.ShowMessage((bool _isSkipped) =>
                {
                    if (!_isSkipped)
                    {
                        _OnNoThanksPress();
                    }
                },
                this.currentMission,
                PanelId.EmailEnterCloseConfirmation);

        }

        public void OnOkPress()
        {
            isSkipped = false;
            SetActive(false);
        }

    }

}