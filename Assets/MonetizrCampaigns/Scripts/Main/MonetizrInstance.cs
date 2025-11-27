using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.SDK
{
    public class MonetizrInstance : MonoBehaviour
    {
        public static MonetizrInstance Instance;

        internal MissionsManager missionsManager = null;
        internal UIController _uiController = null;

        private bool _isActive = false;
        private List<ServerCampaign> campaigns = new List<ServerCampaign>();
        private ServerCampaign _activeCampaignId = null;

        private void Awake ()
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        public ServerCampaign GetActiveCampaign ()
        {
            if (!IsActiveAndEnabled()) return null;
            return _activeCampaignId;
        }

        public bool IsActiveAndEnabled ()
        {
            return Instance != null && Instance.HasCampaignsAndActive();
        }

        internal bool HasCampaignsAndActive()
        {
            return _isActive && campaigns.Count > 0;
        }

        public void InitializeBuiltinMissionsForAllCampaigns()
        {
            /*
            if (!_isMissionsIsOutdated) return;
            missionsManager.LoadSerializedMissions();
            campaigns.ForEach((c) =>
            {
                Instance.InitializeBuiltinMissions(c);
            });
            missionsManager.SaveAndRemoveUnused();
            SetActiveCampaign(FindBestCampaignToActivate());
            _isMissionsIsOutdated = false;
            */
        }

        public void ShowRewardCenter (Action UpdateGameUI, Action<bool> onComplete = null)
        {
            /*
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            UpdateGameUI?.Invoke();
            var campaign = Instance?.FindBestCampaignToActivate();

            if (campaign == null)
            {
                MonetizrLogger.Print("SKIPPED - No campaigns.");
                onComplete?.Invoke(true);
                return;
            }

            Instance?.SetActiveCampaign(campaign);
            var missions = Instance.missionsManager.GetMissionsForRewardCenter(campaign);

            if (missions.Count == 0)
            {
                MonetizrLogger.Print("SKIPPED - No missions.");
                onComplete?.Invoke(true);
                return;
            }

            var m = missions[0];
            bool showRewardCenterForOneMission = missions[0].campaignServerSettings.GetBoolParam("RewardCenter.show_for_one_mission", false);

            if (missions.Count == 1 && !showRewardCenterForOneMission)
            {
                MonetizrLogger.Print($"Only one mission available and RewardCenter.show_for_one_mission is false");
                Instance._PressSingleMission(onComplete, m);
                return;
            }

            MonetizrLogger.Print($"ShowRewardCenter from campaign: {m?.campaignId}");
            string uiItemPrefab = "MonetizrRewardCenterPanel2";
            Instance._uiController.ShowPanelFromPrefab(uiItemPrefab, PanelId.RewardCenter, onComplete, true, m);
            */
        }

        public void ShowStartupNotification (NotificationPlacement placement, Action<bool> onComplete)
        {
            /*
            if (Instance._uiController.HasActivePanel(PanelId.StartNotification))
            {
                MonetizrLogger.Print($"ShowStartupNotification ContainsKey(PanelId.StartNotification) {placement}");
                return;
            }

            bool forceSkip = false;

            if (Instance == null || !Instance.HasActiveCampaign())
            {
                onComplete?.Invoke(true);
                return;
            }

            var missions = Instance.missionsManager.GetMissionsForRewardCenter(Instance.GetActiveCampaign());

            if (missions == null || missions?.Count == 0)
            {
                onComplete?.Invoke(true);
                return;
            }

            Mission mission = missions[0];

            if (placement == NotificationPlacement.ManualNotification)
            {
                ShowNotification(onComplete, mission, PanelId.StartNotification);
                return;
            }

            if (placement == NotificationPlacement.LevelStartNotification)
            {
                forceSkip = mission.campaignServerSettings.GetParam("no_start_level_notifications") == "true";

                if (forceSkip)
                    MonetizrLogger.Print($"No notifications on level start defined on server-side");
            }
            else if (placement == NotificationPlacement.MainMenuShowNotification)
            {
                forceSkip = mission.campaignServerSettings.GetParam("no_main_menu_notifications") == "true";

                if (forceSkip)
                    MonetizrLogger.Print($"No notifications in main menu defined on server-side");
            }

            if (mission.campaignServerSettings.GetParam("no_campaigns_notification") == "true")
            {
                MonetizrLogger.Print($"No notifications defined on serverside");
                forceSkip = true;
            }

            mission.amountOfNotificationsSkipped++;

            if (mission.amountOfNotificationsSkipped <=
                mission.campaignServerSettings.GetIntParam("amount_of_skipped_notifications"))
            {
                MonetizrLogger.Print($"Amount of skipped notifications less then {mission.amountOfNotificationsSkipped}");
                forceSkip = true;
            }

            var serverMaxAmount = mission.campaignServerSettings.GetIntParam("amount_of_notifications");
            var currentAmount = Instance.localSettings.GetSetting(mission.campaignId).amountNotificationsShown;
            if (currentAmount > serverMaxAmount)
            {
                MonetizrLogger.Print($"Startup notification impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                forceSkip = true;
            }

            var lastTimeShow = Instance.localSettings.GetSetting(mission.campaignId).lastTimeShowNotification;
            var serverDelay = mission.campaignServerSettings.GetIntParam("notifications_delay_time_sec");
            var lastTime = (DateTime.Now - lastTimeShow).TotalSeconds;

            if (lastTime < serverDelay)
            {
                MonetizrLogger.Print($"Startup notification last show time less then {serverDelay}");
                forceSkip = true;
            }

            if (forceSkip)
            {
                onComplete?.Invoke(true);
                return;
            }

            mission.amountOfNotificationsSkipped = 0;
            Instance.localSettings.GetSetting(mission.campaignId).lastTimeShowNotification = DateTime.Now;
            Instance.localSettings.GetSetting(mission.campaignId).amountNotificationsShown++;
            Instance.localSettings.SaveData();
            MonetizrLogger.Print($"Notification shown {currentAmount}/{serverMaxAmount} last time: {lastTime}/{serverDelay}");
            ShowNotification(onComplete, mission, PanelId.StartNotification);
            */
        }
    }
}