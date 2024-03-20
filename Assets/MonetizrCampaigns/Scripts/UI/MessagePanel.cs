using Monetizr.SDK.Missions;
using System;
using UnityEngine.UI;

namespace Monetizr.SDK
{

    internal class MessagePanel : PanelController
    {
        public Text title;
        public Text text;
        public Button closeButton;
        public Text buttonText;
        public Button crossButton;

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            closeButton.onClick.AddListener(OnClosePress);
            crossButton?.onClick.AddListener(OnCrossPress);

        }

        internal override void FinalizePanel(PanelId id)
        {
           
        }
        
        private new void Awake()
        {
            base.Awake();
        }

        public void OnCrossPress()
        {
            isSkipped = true;
            SetActive(false);
        }

        public void OnClosePress()
        {
            isSkipped = false;
            SetActive(false);
        }
                
    }

}