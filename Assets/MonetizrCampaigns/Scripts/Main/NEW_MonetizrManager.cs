using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using EventType = Monetizr.SDK.Core.EventType;

namespace Monetizr.SDK
{
    public static class NEW_MonetizrManager
    {
        private static bool s_coppa = false;
        private static bool s_gdpr = false;
        private static bool s_us_privacy = false;
        private static bool s_uoo = true;
        private static string s_consent = "";
        private static Dictionary<RewardType, GameReward> gameRewards = new Dictionary<RewardType, GameReward>();
        private static Vector2? teaserPosition = null;
        private static Transform teaserRoot;
        private static bool isUsingEngagedUserAction = false;
        private static bool hasCompletedEngagedUserAction = false;
        private static Action s_onRequestComplete = null;
        private static Action<bool> s_soundSwitch = null;
        private static Action<bool> s_onUIVisible = null;
        private static UserDefinedEvent s_userEvent = null;
        public delegate void UserDefinedEvent(string campaignId, string placement, EventType eventType);
        public delegate void OnComplete(OnCompleteStatus isSkipped);
        public static UserDefinedEvent userDefinedEvent = null;

        public static void Initialize (Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null)
        {
            s_onRequestComplete = onRequestComplete;
            s_soundSwitch = soundSwitch;
            s_onUIVisible = onUIVisible;
            s_userEvent = userEvent;
            //return _Initialize(onRequestComplete, soundSwitch, onUIVisible, userEvent, null);
        }

        public static void SetUserConsentParameters (bool coppa, bool gdpr, bool us_privacy, bool uoo, string consent)
        {
            s_coppa = coppa;
            s_gdpr = gdpr;
            s_us_privacy = us_privacy;
            s_uoo = uoo;
            s_consent = consent;
        }

        public static void SetAdvertisingIds (string advertisingID, bool limitAdvertising)
        {
            MonetizrMobileAnalytics.isAdvertisingIDDefined = true;
            MonetizrMobileAnalytics.advertisingID = advertisingID;
            MonetizrMobileAnalytics.limitAdvertising = limitAdvertising;
            MonetizrLogger.Print($"MonetizrManager SetAdvertisingIds: {MonetizrMobileAnalytics.advertisingID} {MonetizrMobileAnalytics.limitAdvertising}");
        }

        public static void SetGameCoinAsset (RewardType rt, Sprite defaultRewardIcon, string title, Func<ulong> GetCurrencyFunc, Action<ulong> AddCurrencyAction, ulong maxAmount)
        {
            GameReward gr = new GameReward()
            {
                icon = defaultRewardIcon,
                title = title,
                _GetCurrencyFunc = GetCurrencyFunc,
                _AddCurrencyAction = AddCurrencyAction,
                maximumAmount = maxAmount,
            };

            gameRewards[rt] = gr;
        }

        public static void SetGameCoinMaximumReward (RewardType rt, ulong maxAmount)
        {
            GameReward reward = GetGameReward(rt);

            if (reward != null)
            {
                reward.maximumAmount = maxAmount;
                Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
                MonetizrInstance.Instance.missionsManager.UpdateMissionsRewards(rt, reward);
            }
        }

        public static Canvas GetMainCanvas ()
        {
            Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            return MonetizrInstance.Instance?._uiController?.GetMainCanvas();
        }

        public static void SetTeaserPosition (Vector2 pos)
        {
            teaserPosition = pos;
        }

        public static void SetTeaserRoot (Transform root)
        {
            teaserRoot = root;
        }

        public static void EngagedUserAction (OnComplete onComplete)
        {
            isUsingEngagedUserAction = true;
            MonetizrLogger.Print("Started EngageUserAction");

            Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            var missions = MonetizrInstance.Instance.missionsManager.GetMissionsForRewardCenter(MonetizrInstance.Instance?.GetActiveCampaign());

            if (MonetizrInstance.Instance.GetActiveCampaign() == null)
            {
                MonetizrLogger.Print("SKIPPED - NO CAMPAIGN");
            }

            if (missions == null || missions.Count == 0)
            {
                MonetizrLogger.Print("SKIPPED - NO MISSIONS");
                onComplete(OnCompleteStatus.Skipped);
                return;
            }

            if (missions[0].amountOfRVOffersShown == 0)
            {
                MonetizrLogger.Print("SKIPPED - NO RV");
                onComplete(OnCompleteStatus.Skipped);
                return;
            }

            missions[0].amountOfRVOffersShown--;

            MonetizrInstance.Instance.ShowRewardCenter(null, (Action<bool>)((bool p) =>
            {
                MonetizrLogger.Print((object)"ShowRewardCenter OnComplete!");

                onComplete(hasCompletedEngagedUserAction ? OnCompleteStatus.Completed : OnCompleteStatus.Skipped);
                hasCompletedEngagedUserAction = false;
            }));
        }

        public static void ShowCampaignNotificationAndEngage (OnComplete onComplete = null)
        {
            if (MonetizrInstance.Instance == null || !MonetizrInstance.Instance.HasCampaignsAndActive())
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }

            MonetizrInstance.Instance.InitializeBuiltinMissionsForAllCampaigns();

            ServerCampaign campaign = MonetizrInstance.Instance?.GetActiveCampaign();

            if (campaign == null)
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }

            /*
            ShowStartupNotification(NotificationPlacement.ManualNotification, (bool isSkipped) =>
            {
                if (isSkipped)
                {
                    onComplete?.Invoke(OnCompleteStatus.Skipped);
                }
                else
                {
                    ShowRewardCenter(null, (bool isRCSkipped) =>
                    {
                        onComplete?.Invoke(isRCSkipped ? OnCompleteStatus.Skipped : OnCompleteStatus.Completed);
                    });
                }
            });
            */
        }

        private static GameReward GetGameReward (RewardType rt)
        {
            if (gameRewards.ContainsKey(rt))
            {
                return gameRewards[rt];
            }

            return null;
        }

    }
}