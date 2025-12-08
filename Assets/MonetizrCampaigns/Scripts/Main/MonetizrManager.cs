using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using EventType = Monetizr.SDK.Core.EventType;

namespace Monetizr.SDK
{
    public static class MonetizrManager
    {
        public static string bundleId = null;
        public static string temporaryEmail = "";
        public static bool closeRewardCenterAfterEveryMission = false;
        public static int abTestSegment = 0;
        public static bool shouldAutoReconect = false;

        public static UserDefinedEvent userDefinedEvent = null;
        public delegate void UserDefinedEvent(string campaignId, string placement, EventType eventType);
        public delegate void OnComplete(OnCompleteStatus isSkipped);

        internal static bool s_coppa = false;
        internal static bool s_gdpr = false;
        internal static bool s_us_privacy = false;
        internal static bool s_uoo = true;
        internal static string s_consent = "";
        internal static bool isUsingEngagedUserAction = false;
        internal static bool hasCompletedEngagedUserAction = false;
        internal static Dictionary<RewardType, GameReward> gameRewards = new Dictionary<RewardType, GameReward>();
        internal static Vector2? teaserPosition;
        internal static Transform teaserRoot;

        public static void EnableLogger ()
        {
            MonetizrLogger.isEnabled = true;
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

        public static void SetGameCoinAsset (RewardType rewardType, Sprite defaultRewardIcon, string title, Func<ulong> GetCurrencyFunc, Action<ulong> AddCurrencyAction, ulong maxAmount)
        {
            GameReward gameReward = new GameReward()
            {
                icon = defaultRewardIcon,
                title = title,
                _GetCurrencyFunc = GetCurrencyFunc,
                _AddCurrencyAction = AddCurrencyAction,
                maximumAmount = maxAmount,
            };

            gameRewards[rewardType] = gameReward;
        }

        public static void Initialize (Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null)
        {
            if (MonetizrInstance.Instance != null)
            {
                MonetizrLogger.PrintError("MonetizrManager has already been initialized.");
                return;
            }

            if (!IsInitializationSetupComplete())
            {
                MonetizrLogger.PrintError("SDK Setup Incomplete - Please, verify and provide the missing parameters.");
                return;
            }

            CreateMonetizrInstance();
            MonetizrInstance.Instance.InitializeSDK(onRequestComplete, soundSwitch, onUIVisible, userEvent);
        }

        private static bool IsInitializationSetupComplete ()
        {
            MonetizrSettingsMenu.LoadSettings();

            string manualDebugKey = PlayerPrefs.GetString("MonetizrAPIKey");
            if (!String.IsNullOrEmpty(manualDebugKey))
            {
                MonetizrSettings.apiKey = manualDebugKey;
            }

            if (string.IsNullOrEmpty(MonetizrSettings.apiKey))
            {
                MonetizrLogger.PrintError("Missing SDK Settings - API Key. Please, provide API Key through ProjectSettings -> Monetizr.");
                return false;
            }

            if (string.IsNullOrEmpty(MonetizrSettings.bundleID))
            {
                MonetizrLogger.PrintError("Missing SDK Settings - Bundle ID. Please, provide Bundle ID through ProjectSettings -> Monetizr.");
                return false;
            }

            if (gameRewards == null || gameRewards.Count <= 0)
            {
                MonetizrLogger.PrintError("Missing SDK Settings - GameReward. Please, setup at least one Game Reward before SDK initialization.");
                return false;
            }

            foreach (KeyValuePair<RewardType, GameReward> gameReward in gameRewards)
            {
                if (!gameReward.Value.IsSetupValid())
                {
                    MonetizrLogger.PrintError("Invalid GameReward Setup - Please, make sure that the Game Rewards setup is valid.");
                    return false;
                }
            }

            if (!MonetizrMobileAnalytics.isAdvertisingIDDefined)
            {
                MonetizrLogger.PrintError("Missing SDK Settings - Advertising ID. Please, call MonetizrManager.SetAdvertisingIds before SDK initialization.");
                return false;
            }

            return true;
        }

        private static void CreateMonetizrInstance ()
        {
            GameObject monetizrObject = new GameObject("MonetizrInstance");
            MonetizrInstance monetizrManager = monetizrObject.AddComponent<MonetizrInstance>();
            GCPManager gcpManager = monetizrObject.AddComponent<GCPManager>();
            CampaignManager campaignManager = monetizrObject.AddComponent<CampaignManager>();

            if (teaserRoot != null)
            {
                MonetizrInstance.Instance.teaserRoot = teaserRoot;
            }

            if (teaserPosition.HasValue)
            {
                MonetizrInstance.Instance.teaserPosition = teaserPosition;
            }
        }

        public static bool IsActiveAndEnabled ()
        {
            return MonetizrInstance.Instance != null && MonetizrInstance.Instance.HasCampaignsAndActive();
        }

        public static void SetGameCoinMaximumReward (RewardType rewardType, ulong maxAmount)
        {
            GameReward gameReward = MonetizrInstance.Instance.GetGameReward(rewardType);

            if (gameReward != null)
            {
                gameReward.maximumAmount = maxAmount;
                Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
                MonetizrInstance.Instance.missionsManager.UpdateMissionsRewards(rewardType, gameReward);
            }
        }

        public static Canvas GetMainCanvas ()
        {
            Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            return MonetizrInstance.Instance.uiController.GetMainCanvas();
        }

        public static void SetTeaserPosition (Vector2 position)
        {
            if (MonetizrInstance.Instance)
            {
                MonetizrInstance.Instance.teaserPosition = position;
            }
            else
            {
                teaserPosition = position;
            }
        }

        public static void SetTeaserRoot (Transform root)
        {
            if (MonetizrInstance.Instance)
            {
                MonetizrInstance.Instance.teaserRoot = root;
            }
            else
            {
                teaserRoot = root;
            }
        }

        public static void EngagedUserAction (OnComplete onComplete)
        {
            isUsingEngagedUserAction = true;
            MonetizrLogger.Print("Started EngageUserAction");
            Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            List<Mission> missions = MonetizrInstance.Instance.missionsManager.GetMissionsForRewardCenter(MonetizrInstance.Instance?.GetActiveCampaign());

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
                MonetizrLogger.Print("ShowRewardCenter OnComplete!");
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

            MonetizrInstance.Instance.ShowStartupNotification(NotificationPlacement.ManualNotification, (bool isSkipped) =>
            {
                if (isSkipped)
                {
                    onComplete?.Invoke(OnCompleteStatus.Skipped);
                }
                else
                {
                    MonetizrInstance.Instance.ShowRewardCenter(null, (bool isRCSkipped) =>
                    {
                        onComplete?.Invoke(isRCSkipped ? OnCompleteStatus.Skipped : OnCompleteStatus.Completed);
                    });
                }
            });
        }

    }

}