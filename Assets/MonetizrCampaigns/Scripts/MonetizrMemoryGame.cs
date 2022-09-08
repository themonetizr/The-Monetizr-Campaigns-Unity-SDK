using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class MonetizrMemoryGame: PanelController
    {
        private Mission currentMission;
        void Update()
        {
            

        }

        void SetProgress(float a)
        {
            //uvRect.y = 0.5f * (1.0f - Tween(a));
            //teaserImage.uvRect = uvRect;
        }

        float Tween(float k)
        {
            return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
        }

        public void OnButtonClick()
        {
            MonetizrManager.Analytics.TrackEvent("Minigame pressed", currentMission);
            MonetizrManager.ShowRewardCenter(null);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            currentMission = m;



            

            Log.PrintWarning($"{m.campaignId} {m}");
            MonetizrManager.Analytics.BeginShowAdAsset(AdType.MinigameScreen, m);

            MonetizrManager.Analytics.TrackEvent("Minigame shown", m);
        }

        

       

       

        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(AdType.MinigameScreen);
        }
    }

}