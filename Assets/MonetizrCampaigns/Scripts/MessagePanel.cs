using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class MessagePanel : PanelController
    {
        public Text title;
        public Text text;
        public Button closeButton;
        public Text buttonText;
        public Button crossButton;

        //[HideInInspector]
        //public Mission currentMission;

        //private Action _onComplete;

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