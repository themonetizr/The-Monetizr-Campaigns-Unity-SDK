using mixpanel;
using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Prebid;
using Monetizr.SDK.UI;
using Monetizr.SDK.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.SDK.Core
{
    public static class MonetizrManager
    {
        public static Action<string, Dictionary<string, string>> ExternalAnalytics { internal get; set; } = null;
        public static string temporaryEmail = "";
        public static bool claimForSkippedCampaigns;
        public static bool closeRewardCenterAfterEveryMission = false;
        public static string bundleId = null;
        public static int abTestSegment = 0;
        public static bool shouldAutoReconect = false;

        internal static bool s_coppa = false;
        internal static bool s_gdpr = false;
        internal static bool s_us_privacy = false;
        internal static bool s_uoo = true;
        internal static string s_consent = "";

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
            GameReward reward = MonetizrInstance.Instance.GetGameReward(rt);

            if (reward != null)
            {
                reward.maximumAmount = maxAmount;
                Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
                MonetizrInstance.Instance.missionsManager.UpdateMissionsRewards(rt, reward);
            }
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

        private static void CreateMonetizrManagerInstance (Action<bool> onUIVisible, UserDefinedEvent userEvent)
        {
            /*
            GameObject monetizrObject = new GameObject("MonetizrManager");
            MonetizrManager monetizrManager = monetizrObject.AddComponent<MonetizrManager>();
            GCPManager datadogManager = monetizrObject.AddComponent<GCPManager>();
            CampaignManager campaignManager = monetizrObject.AddComponent<CampaignManager>();
            DontDestroyOnLoad(monetizrObject);
            Instance = monetizrManager;
            Instance.sponsoredMissions = null;
            Instance.userDefinedEvent = userEvent;
            Instance.onUIVisible = onUIVisible;
            */
        }

        public static Canvas GetMainCanvas()
        {
            Assert.IsNotNull(MonetizrInstance.Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            return MonetizrInstance.Instance?._uiController?.GetMainCanvas();
        }

        public static void SetTeaserPosition(Vector2 pos)
        {
            teaserPosition = pos;
        }

        public static void SetTeaserRoot(Transform root)
        {
            teaserRoot = root;
        }

        public static void Initialize (Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null)
        {
            s_onRequestComplete = onRequestComplete;
            s_soundSwitch = soundSwitch;
            s_onUIVisible = onUIVisible;
            s_userEvent = userEvent;
            //return _Initialize(onRequestComplete, soundSwitch, onUIVisible, userEvent, null);
        }

        public static void EngagedUserAction(OnComplete onComplete)
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
                MonetizrLogger.Print((object)"ShowRewardCenter OnComplete!");

                onComplete(hasCompletedEngagedUserAction ? OnCompleteStatus.Completed : OnCompleteStatus.Skipped);
                hasCompletedEngagedUserAction = false;
            }));
        }

        public static bool IsActiveAndEnabled()
        {
            return MonetizrInstance.Instance != null && MonetizrInstance.Instance.HasCampaignsAndActive();
        }

        public static void ShowCampaignNotificationAndEngage(OnComplete onComplete = null)
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






        


        //internal static MonetizrMobileAnalytics Analytics => MonetizrInstance.ConnectionsClient.Analytics;
        internal static bool keepLocalClaimData;
        internal static bool serverClaimForCampaigns;
        internal static bool canTeaserBeVisible;
        internal static RewardSelectionType temporaryRewardTypeSelection = RewardSelectionType.Product;
        internal static Dictionary<RewardType, GameReward> gameRewards = new Dictionary<RewardType, GameReward>();
        private static int debugAttempt = 0;
        private static Vector2? teaserPosition = null;
        private static Transform teaserRoot;
        private static bool isUsingEngagedUserAction = false;
        private static bool hasCompletedEngagedUserAction = false;
        private static float statusCheckTime = 30f;
        private static Action s_onRequestComplete = null;
        private static Action<bool> s_soundSwitch = null;
        private static Action<bool> s_onUIVisible = null;

        private static UserDefinedEvent s_userEvent = null;
        public static UserDefinedEvent userDefinedEvent = null;
        public delegate void UserDefinedEvent(string campaignId, string placement, EventType eventType);
        public delegate void OnComplete(OnCompleteStatus isSkipped);

        /*
        public List<MissionDescription> sponsoredMissions { get; private set; }
        public List<UnityEngine.Object> holdResources = new List<UnityEngine.Object>();


        internal MonetizrHttpClient ConnectionsClient { get; private set; }
        internal Action<bool> onUIVisible = null;
        internal MissionsManager missionsManager = null;
        internal LocalSettingsManager localSettings = null;

        private UIController _uiController = null;
        private ServerCampaign _activeCampaignId = null;
        private Action<bool> _soundSwitch = null;
        private Action<bool> _onRequestComplete = null;
        private bool _isActive = false;
        private bool _isMissionsIsOutdated = true;
        private List<ServerCampaign> campaigns = new List<ServerCampaign>();
        */




    }

}