using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class LoadingScreenPanel : PanelController
    {
        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            
        }

        internal override void FinalizePanel(PanelId id)
        {

        }

        private new void Awake()
        {
            base.Awake();
        }
    }
}