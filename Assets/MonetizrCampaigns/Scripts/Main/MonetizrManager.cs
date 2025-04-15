﻿using mixpanel;
using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.SDK.Core
{
    public class MonetizrManager : MonoBehaviour
    {

        #region Public Static Variables

        public static Action<string, Dictionary<string, string>> ExternalAnalytics { internal get; set; } = null;
        public static string temporaryEmail = "";
        public static bool claimForSkippedCampaigns;
        public static bool closeRewardCenterAfterEveryMission = false;
        public static string bundleId = null;
        public static int abTestSegment = 0;
        public static bool shouldAutoReconect = false;

        #endregion

        #region Private Static Variables

        internal static MonetizrManager Instance { get; private set; } = null;
        internal static MonetizrAnalytics Analytics => Instance.ConnectionsClient.Analytics;
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

        #endregion

        #region Public Variables

        public List<MissionDescription> sponsoredMissions { get; private set; }
        public delegate void UserDefinedEvent(string campaignId, string placement, EventType eventType);
        public delegate void OnComplete(OnCompleteStatus isSkipped);
        public UserDefinedEvent userDefinedEvent = null;
        public List<UnityEngine.Object> holdResources = new List<UnityEngine.Object>();

        #endregion

        #region Private Variables

        internal MonetizrClient ConnectionsClient { get; private set; }
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
        private Action _gameOnInitSuccess;

        internal static bool s_coppa = false;
        internal static bool s_gdpr = false;
        internal static bool s_us_privacy = false;
        internal static bool s_uoo = true;
        internal static string s_consent = "";

        #endregion

        #region Public Static Methods

        public static void SetUserConsentParameters (bool coppa, bool gdpr, bool us_privacy, bool uoo, string consent)
        {
            s_coppa = coppa;
            s_gdpr = gdpr;
            s_us_privacy = us_privacy;
            s_uoo = uoo;
            s_consent = consent;
        }

        public static void SetAdvertisingIds(string advertisingID, bool limitAdvertising)
        {
            MonetizrMobileAnalytics.isAdvertisingIDDefined = true;
            MonetizrMobileAnalytics.advertisingID = advertisingID;
            MonetizrMobileAnalytics.limitAdvertising = limitAdvertising;
            MonetizrLogger.Print($"MonetizrManager SetAdvertisingIds: {MonetizrMobileAnalytics.advertisingID} {MonetizrMobileAnalytics.limitAdvertising}");
        }

        public static void SetGameCoinAsset(RewardType rt, Sprite defaultRewardIcon, string title,
            Func<ulong> GetCurrencyFunc, Action<ulong> AddCurrencyAction, ulong maxAmount)
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

        public static MonetizrManager Initialize(Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null)
        {
            s_onRequestComplete = onRequestComplete;
            s_soundSwitch = soundSwitch;
            s_onUIVisible = onUIVisible;
            s_userEvent = userEvent;
            return _Initialize(onRequestComplete, soundSwitch, onUIVisible, userEvent, null);
        }

        public static void HoldResource(object o)
        {
            if (o is UnityEngine.Object)
            {
                UnityEngine.Object uo = (UnityEngine.Object)o;

                if (!Instance.holdResources.Contains(uo))
                {
                    Instance.holdResources.Add(uo);
                }
            }
        }

        public static void SetGameCoinMaximumReward(RewardType rt, ulong maxAmount)
        {
            GameReward reward = GetGameReward(rt);

            if (reward != null)
            {
                reward.maximumAmount = maxAmount;
                Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
                MonetizrManager.Instance.missionsManager.UpdateMissionsRewards(rt, reward);
            }
        }

        public static void LogGameRewards()
        {
            if (gameRewards == null || gameRewards.Count == 0)
            {
                MonetizrLogger.Print("gameRewards dictionary is null or empty.");
                return;
            }

            foreach (KeyValuePair<RewardType, GameReward> entry in gameRewards)
            {
                MonetizrLogger.Print($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }

        public static void ShowDebug()
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            debugAttempt++;

#if !UNITY_EDITOR
            if (debugAttempt != 10)
                return;
#endif

            debugAttempt = 0;
            Instance._uiController.ShowPanelFromPrefab("MonetizrDebugPanel", PanelId.DebugPanel);
        }

        public static void ShowStartupNotification(NotificationPlacement placement, Action<bool> onComplete)
        {
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
        }

        public static Canvas GetMainCanvas()
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            return Instance?._uiController?.GetMainCanvas();
        }

        public static void EngagedUserAction(OnComplete onComplete)
        {
            isUsingEngagedUserAction = true;
            MonetizrLogger.Print("Started EngageUserAction");

            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            var missions = Instance.missionsManager.GetMissionsForRewardCenter(Instance?.GetActiveCampaign());

            if (Instance.GetActiveCampaign() == null)
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

            MonetizrManager.ShowRewardCenter(null, (Action<bool>)((bool p) =>
            {
                MonetizrLogger.Print((object)"ShowRewardCenter OnComplete!");

                onComplete(hasCompletedEngagedUserAction ? OnCompleteStatus.Completed : OnCompleteStatus.Skipped);
                hasCompletedEngagedUserAction = false;
            }));
        }

        public static void OnEngagedUserActionComplete()
        {
            if (!isUsingEngagedUserAction) return;
            hasCompletedEngagedUserAction = true;
        }

        public static void ShowRewardCenter(Action UpdateGameUI, Action<bool> onComplete = null)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
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
        }

        public static void SetTeaserPosition(Vector2 pos)
        {
            teaserPosition = pos;
        }

        public static void SetTeaserRoot(Transform root)
        {
            teaserRoot = root;
        }

        public static void OnMainMenuShow(bool showNotifications = true)
        {
            canTeaserBeVisible = true;
            if (Instance == null) return;
            if (!Instance.HasCampaignsAndActive()) return;
            Instance.InitializeBuiltinMissionsForAllCampaigns();
            var campaign = Instance.FindBestCampaignToActivate();
            Instance.SetActiveCampaign(campaign);
            if (campaign == null) return;

            if (showNotifications)
            {
                HideTeaser();

                ShowStartupNotification(NotificationPlacement.MainMenuShowNotification, (bool isSkipped) =>
                {
                    if (isSkipped)
                        ShowTeaser();
                    else
                        ShowRewardCenter(null, null);
                });
            }
            else
            {
                ShowTeaser();
            }
        }

        public static void ShowCampaignNotificationAndEngage(OnComplete onComplete = null)
        {
            if (Instance == null || !Instance.HasCampaignsAndActive())
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }

            Instance.InitializeBuiltinMissionsForAllCampaigns();

            var campaign = MonetizrManager.Instance?.GetActiveCampaign();

            if (campaign == null)
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }

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
        }

        public static void OnMainMenuHide()
        {
            canTeaserBeVisible = false;
            HideTeaser();
        }

        public static void ShowTeaser(Action UpdateGameUI = null)
        {
            if (!MonetizrManager.canTeaserBeVisible) return;
            if (Instance == null) return;
            var campaign = Instance?.FindBestCampaignToActivate();

            if (campaign == null)
            {
                MonetizrLogger.Print($"No active campaigns for teaser");
                return;
            }

            Instance?.SetActiveCampaign(campaign);

            if (Instance.missionsManager.GetActiveMissionsNum(campaign) == 0)
            {
                MonetizrLogger.Print($"No active missions for teaser");
                return;
            }

            if (!campaign.HasAsset(AssetsType.TinyTeaserSprite) && !campaign.HasAsset(AssetsType.TeaserGifPathString) && !campaign.HasAsset(AssetsType.BrandRewardLogoSprite))
            {
                MonetizrLogger.Print("No texture for teaser. ");
                return;
            }

            if (campaign.serverSettings.GetParam("hide_teaser_button") == "true") return;

            var serverMaxAmount = campaign.serverSettings.GetIntParam("amount_of_teasers");
            var currentAmount = Instance.localSettings.GetSetting(campaign.id).amountTeasersShown;
            if (currentAmount > serverMaxAmount)
            {
                MonetizrLogger.Print($"Teaser impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                return;
            }

            MonetizrLogger.Print($"Teaser shown {currentAmount}/{serverMaxAmount}");
            Instance.localSettings.GetSetting(campaign.id).amountTeasersShown++;
            Instance.localSettings.SaveData();
            int uiVersion = 4;
            Instance._uiController.ShowTeaser(teaserRoot, teaserPosition, UpdateGameUI, uiVersion, campaign);
        }

        public static void HideTeaser(bool checkIfSomeMissionsAvailable = false)
        {
            if (Instance == null) return;
            if (checkIfSomeMissionsAvailable && Instance.missionsManager.GetActiveMissionsNum() > 0) return;
            if (!Instance._isActive) return;
            Instance._uiController.HidePanel(PanelId.TinyMenuTeaser);
        }

        public static bool IsActiveAndEnabled()
        {
            return Instance != null && Instance.HasCampaignsAndActive();
        }

        public static bool IsInitialized()
        {
            return Instance != null;
        }

        #endregion

        #region Private Static Methods

        private static MonetizrManager _Initialize(Action onRequestComplete, Action<bool> soundSwitch, Action<bool> onUIVisible, UserDefinedEvent userEvent, MonetizrClient connectionClient)
        {
            if (Instance != null) return Instance;

#if UNITY_EDITOR
            keepLocalClaimData = true;
            serverClaimForCampaigns = false;
            claimForSkippedCampaigns = true;
#else
            keepLocalClaimData = true;
            serverClaimForCampaigns = true;
            claimForSkippedCampaigns = false;
#endif
            if (soundSwitch == null)
            {
                soundSwitch = (bool isOn) =>
                {
                    MonetizrLogger.Print($"Audio listener pause state {!isOn}");
                    AudioListener.pause = !isOn;
                };
            }

            if (!IsInitializationSetupComplete())
            {
                MonetizrLogger.PrintLocalMessage(MessageEnum.M600);
                return null;
            }

            MonetizrLogger.Print($"Initializing MonetizrManager with: {MonetizrSettings.apiKey} {MonetizrSettings.bundleID} {MonetizrSettings.SDKVersion}");
            MonetizrManager monetizrManager = CreateMonetizrManagerInstance(onUIVisible, userEvent);
            monetizrManager.Initialize(onRequestComplete, soundSwitch, connectionClient);

            return Instance;
        }

        private static bool IsInitializationSetupComplete()
        {
            MonetizrSettingsMenu.LoadSettings();

            if (string.IsNullOrEmpty(MonetizrSettings.apiKey))
            {
                MonetizrLogger.PrintLocalMessage(MessageEnum.M601);
                return false;
            }

            if (string.IsNullOrEmpty(MonetizrSettings.bundleID))
            {
                MonetizrLogger.PrintLocalMessage(MessageEnum.M602);
                return false;
            }

            if (gameRewards == null || gameRewards.Count <= 0)
            {
                MonetizrLogger.PrintLocalMessage(MessageEnum.M603);
                return false;
            }

            foreach (KeyValuePair<RewardType, GameReward> gameReward in gameRewards)
            {
                if (!gameReward.Value.IsSetupValid())
                {
                    MonetizrLogger.PrintLocalMessage(MessageEnum.M606);
                    return false;
                }
            }

            if (!MonetizrMobileAnalytics.isAdvertisingIDDefined)
            {
                MonetizrLogger.PrintLocalMessage(MessageEnum.M604);
                return false;
            }

            return true;
        }

        private static MonetizrManager CreateMonetizrManagerInstance(Action<bool> onUIVisible, UserDefinedEvent userEvent)
        {
            var monetizrObject = new GameObject("MonetizrManager");
            var monetizrManager = monetizrObject.AddComponent<MonetizrManager>();
            var monetizrErrorLogger = monetizrObject.AddComponent<MonetizrErrorLogger>();
            var datadogManager = monetizrObject.AddComponent<GCPManager>();
            DontDestroyOnLoad(monetizrObject);
            Instance = monetizrManager;
            Instance.sponsoredMissions = null;
            Instance.userDefinedEvent = userEvent;
            Instance.onUIVisible = onUIVisible;
            return monetizrManager;
        }

        internal static void _CallUserDefinedEvent(string campaignId, string placement, EventType eventType)
        {
            try
            {
                Instance?.userDefinedEvent?.Invoke(campaignId, placement, eventType);
            }
            catch (Exception ex)
            {
                MonetizrLogger.PrintError($"Exception in _CallUserDefinedEvent\n{ex}");
            }
        }

        internal static GameReward GetGameReward(RewardType rt)
        {
            LogGameRewards();

            if (gameRewards.ContainsKey(rt))
            {
                return gameRewards[rt];
            }

            return null;
        }

        internal static MonetizrManager InitializeForTests(Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null, MonetizrClient connectionClient = null)
        {
            return _Initialize(onRequestComplete, soundSwitch, onUIVisible, userEvent, connectionClient);
        }

        internal static void ShowMessage(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            Instance._uiController.ShowPanelFromPrefab("MonetizrMessagePanel2", panelId, onComplete, true, m);
        }

        internal static void ShowNotification(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            Instance._uiController.ShowPanelFromPrefab("MonetizrNotifyPanel2", panelId, onComplete, true, m);
        }

        internal static void ShowEnterEmailPanel(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            Instance._uiController.ShowPanelFromPrefab("MonetizrEnterEmailPanel2", panelId, onComplete, true, m);
        }

        internal static async void ResetCampaign()
        {
            Mission m = Instance.missionsManager.GetFirstUnactiveMission();

            if (m == null)
            {
                MonetizrLogger.Print($"Nothing to reset in ResetCampaign");
                return;
            }

            string campaignId = m.campaignId;
            var lscreen = Instance._uiController.ShowLoadingScreen();
            lscreen._onComplete = (bool _) => { GameObject.Destroy(lscreen); };
            CancellationTokenSource s_cts = new CancellationTokenSource();

            try
            {
                s_cts.CancelAfter(10000);
                await Instance.ConnectionsClient.ResetCampaign(campaignId, s_cts.Token);
            }
            catch (OperationCanceledException)
            {
                MonetizrLogger.Print("\nTasks cancelled: timed out.\n");
            }
            finally
            {
                s_cts.Dispose();
            }

            lscreen.SetActive(false);
        }

        internal static async void WaitForEndRequestAndNotify(Action<bool> onComplete, Mission m, Action updateUIDelegate)
        {
            var lscreen = Instance._uiController.ShowLoadingScreen();
            lscreen._onComplete = (bool _) => { GameObject.Destroy(lscreen); };

            Action onSuccess = () =>
            {
                MonetizrLogger.Print("SUCCESS!");
                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, EventType.ButtonPressOk);
                MonetizrManager.Instance.OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);
            };

            Action onFail = () =>
            {
                MonetizrLogger.Print("FAIL!"); ;
                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, EventType.Error);
                ShowMessage((bool _) => { onComplete?.Invoke(false); }, m, PanelId.BadEmailMessageNotification);
            };

            if (serverClaimForCampaigns)
            {
                CancellationTokenSource s_cts = new CancellationTokenSource();

                try
                {
                    s_cts.CancelAfter(10000);

                    await Instance.ClaimReward(m.campaign, s_cts.Token, onSuccess, onFail);
                }
                catch (OperationCanceledException)
                {
                    MonetizrLogger.Print("\nTasks cancelled: timed out.\n");
                }
                finally
                {
                    s_cts.Dispose();
                }
            }
            else
            {
                onSuccess.Invoke();
            }

            temporaryEmail = null;
            lscreen.SetActive(false);
        }

        internal static void ShowCongratsNotification(Action<bool> onComplete, Mission m)
        {
            ShowNotification(onComplete, m, PanelId.CongratsNotification);
        }

        internal static void CleanUserDefinedMissions()
        {
            Instance.missionsManager.CleanUserDefinedMissions();
        }

        internal static void ShowCodeView(Action<bool> onComplete, Mission m = null)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (!Instance._isActive) return;
            Instance._uiController.ShowPanelFromPrefab("MonetizrEnterCodePanel2", PanelId.CodePanelView, onComplete, false, m);
        }

        internal static void ShowMinigame(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (!Instance._isActive) return;

            var panelNames = new Dictionary<MissionType, Tuple<PanelId, string>>()
            {
                {
                    MissionType.MinigameReward,
                    new Tuple<PanelId, string>(PanelId.CarMemoryGame, "MonetizrCarGamePanel")
                },
                {
                    MissionType.MemoryMinigameReward,
                    new Tuple<PanelId, string>(PanelId.MemoryGame, "MonetizrGamePanel")
                },
            };

            Instance._uiController.ShowPanelFromPrefab(panelNames[m.type].Item2, panelNames[m.type].Item1, onComplete, false, m);
        }

        internal static void ShowUnitySurvey(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (!Instance._isActive) return;
            Instance._uiController.ShowPanelFromPrefab("MonetizrUnitySurveyPanel", PanelId.SurveyUnityView, onComplete, false, m);
        }

        internal static void _ShowWebView(Action<bool> onComplete, PanelId id, Mission m = null)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (!Instance._isActive) return;
            Instance._uiController.ShowPanelFromPrefab("MonetizrWebViewPanel2", id, onComplete, false, m);
        }

        internal static void ShowActionView(Action<bool> onComplete, Mission m = null)
        {
            _ShowWebView(onComplete, PanelId.ActionHtmlPanelView, m);
        }

        internal static void ShowSurvey(Action<bool> onComplete, Mission m = null)
        {
            _ShowWebView(onComplete, PanelId.SurveyWebView, m);
        }

        internal static void ShowWebPage(Action<bool> onComplete, Mission m = null)
        {
            _ShowWebView(onComplete, PanelId.HtmlWebPageView, m);
        }

        internal static void ShowHTML5(Action<bool> onComplete, Mission m = null)
        {
            _ShowWebView(onComplete, PanelId.Html5WebView, m);
        }

        #endregion

        #region Public Methods

        public async void RequestCampaigns(Action<bool> onRequestComplete)
        {
            await ConnectionsClient.GetGlobalSettings();
            CheckMixpanelProxy();
            CheckCGPLogging();

            campaigns = new List<ServerCampaign>();
            try
            {
                campaigns = await ConnectionsClient.GetList();
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M401);
                MonetizrLogger.PrintError($"Exception while getting list of campaigns\n{e}");
                onRequestComplete?.Invoke(false);
            }

            if (campaigns == null)
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M401);
                MonetizrLogger.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]}");
                onRequestComplete?.Invoke(false);
            }

            var logConnectionErrors = ConnectionsClient.GlobalSettings.GetBoolParam("mixpanel.log_connection_errors", true);

            if (campaigns.Count > 0)
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M102);
                ConnectionsClient.SetTestMode(campaigns[0].testmode);
                ConnectionsClient.Analytics.Initialize(campaigns[0].testmode, campaigns[0].panel_key, logConnectionErrors);
                ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingStarts, EventType.Notification);
            }
            else
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M103);
                ConnectionsClient.Analytics.Initialize(false, null, logConnectionErrors);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(10000);
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

            foreach (ServerCampaign campaign in campaigns)
            {
                await campaign.LoadCampaignAssets();

                if (campaign.isLoaded)
                {
                    MonetizrLogger.PrintRemoteMessage(MessageEnum.M104);
                    MonetizrLogger.Print($"Campaign {campaign.id} successfully loaded");
                }
                else
                {
                    MonetizrLogger.PrintRemoteMessage(MessageEnum.M402);
                    MonetizrLogger.PrintError($"Campaign {campaign.id} loading failed with error {campaign.loadingError}!");
                    ConnectionsClient.Analytics.TrackEvent(campaign, null, AdPlacement.AssetsLoading, EventType.Error, new Dictionary<string, string> { { "loading_error", campaign.loadingError } });
                }
            }

            campaigns.RemoveAll(c => c.isLoaded == false);

#if TEST_SLOW_LATENCY
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

            localSettings.LoadOldAndUpdateNew(campaigns);
            MonetizrLogger.Print($"RequestCampaigns completed with {campaigns.Count} campaigns.");
            if (campaigns.Count > 0) ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingEnds, EventType.Notification);

            MonetizrLogger.Print("MonetizrManager initialization okay!");
            _isActive = true;
            onRequestComplete?.Invoke(true);
        }

        private void CheckMixpanelProxy ()
        {
            string mixpanelProxy = "";
            if (ConnectionsClient.GlobalSettings.TryGetValue("mixpanel_proxy_endpoint", out mixpanelProxy))
            {
                if (!String.IsNullOrEmpty(mixpanelProxy)) MixpanelSettings.Instance.APIHostAddress = mixpanelProxy;
            }
        }

        private void CheckCGPLogging()
        {
            string gcpLoggingKillswitch = "";
            if (ConnectionsClient.GlobalSettings.TryGetValue("unity_logging", out gcpLoggingKillswitch))
            {
                if (gcpLoggingKillswitch == "0") GCPManager.Instance.EnableLogging();
            }
        }

        public void SoundSwitch(bool on)
        {
            _soundSwitch?.Invoke(on);
        }

        public bool HasActiveCampaign()
        {
            return _isActive && _activeCampaignId != null;
        }

        #endregion

        #region Private Methods

        private void OnApplicationQuit()
        {
            MonetizrLogger.PrintRemoteMessage(MessageEnum.M105);
            Analytics?.OnApplicationQuit();
        }

        private void Start()
        {
            if (!shouldAutoReconect) return;
            InvokeRepeating(nameof(VerifySDKStatus), statusCheckTime, statusCheckTime);
        }

        private void VerifySDKStatus ()
        {
            if (IsInitialized()) 
            {
                if (!NetworkingUtils.IsInternetReachable())
                {
                    Instance.Initialize(s_onRequestComplete, _soundSwitch, null);
                }
            }
            else
            {
                _Initialize(s_onRequestComplete, s_soundSwitch, s_onUIVisible, s_userEvent, null);
            }
        }

        private void Initialize(Action gameOnInitSuccess, Action<bool> soundSwitch, MonetizrClient connectionClient)
        {

#if USING_WEBVIEW
            if (!UniWebView.IsWebViewSupported) Log.Print("WebView isn't supported on current platform!");
#endif

            localSettings = new LocalSettingsManager();
            missionsManager = new MissionsManager();
            this._soundSwitch = soundSwitch;
            ConnectionsClient = connectionClient ?? new MonetizrHttpClient(MonetizrSettings.apiKey);
            ConnectionsClient.Initialize();
            InitializeUI();
            _gameOnInitSuccess = gameOnInitSuccess;

            _onRequestComplete = (bool isOk) =>
            {
                gameOnInitSuccess?.Invoke();
                gameOnInitSuccess = null;

                if (canTeaserBeVisible)
                {
                    OnMainMenuShow(false);
                }
            };

            RequestCampaigns(_onRequestComplete);

            if (MonetizrSettings.requestCampaignTime > 0)
            {
                StartCoroutine(TryRequestCampaignsLater(MonetizrSettings.requestCampaignTime));
            }
        }

        private IEnumerator TryRequestCampaignsLater(float time)
        {
            while (true)
            {
                yield return new WaitForSeconds(time);
                if (campaigns.Count != 0) continue;
                _isActive = false;
                RequestCampaigns(_onRequestComplete);
            }
        }

        internal void InitializeBuiltinMissions(ServerCampaign campaign)
        {
            missionsManager.CreateMissionsFromCampaign(campaign);
        }

        internal bool CheckFullCampaignClaim(Mission m)
        {
            return missionsManager.CheckFullCampaignClaim(m);
        }

        internal void SaveClaimedReward(Mission m)
        {
            missionsManager.SaveAll();
        }

        internal void CleanRewardsClaims()
        {
            localSettings.ResetData();
            missionsManager.CleanRewardsClaims();
        }

        internal string GetCurrentAPIkey()
        {
            return ConnectionsClient.currentApiKey;
        }

        internal void RestartClient()
        {
            ConnectionsClient.Close();
            ConnectionsClient = new MonetizrHttpClient(ConnectionsClient.currentApiKey);
            ConnectionsClient.Initialize();
            RequestCampaigns();
        }

        internal bool ChangeAPIKey(string apiKey)
        {
            if (apiKey == ConnectionsClient.currentApiKey) return false;
            MonetizrLogger.Print($"Changing api key to {apiKey}");
            ConnectionsClient.currentApiKey = apiKey;
            return true;
        }

        internal void RequestCampaigns(bool callRequestComplete = true)
        {
            _isActive = false;
            _isMissionsIsOutdated = true;
            _uiController.DestroyTeaser();
            missionsManager.CleanUp();
            campaigns.Clear();
            _activeCampaignId = null;
            RequestCampaigns(callRequestComplete ? _onRequestComplete : null);
        }

        private void InitializeUI()
        {
            _uiController = new UIController();
        }

        internal void ClaimMissionData(Mission m)
        {
            gameRewards[m.rewardType].AddCurrencyAction(m.reward);
            if (keepLocalClaimData) Instance.SaveClaimedReward(m);
        }

        internal void _PressSingleMission(Action<bool> onComplete, Mission m)
        {
            if (m.isClaimed == ClaimState.Claimed) return;
            MonetizrManager.Instance.missionsManager.ClaimAction(m, onComplete, null).Invoke();
        }

        internal void OnClaimRewardComplete(Mission mission, bool isSkipped, Action<bool> onComplete,
            Action updateUIDelegate)
        {
            if (claimForSkippedCampaigns)
                isSkipped = false;

            if (isSkipped)
            {
                MonetizrLogger.Print("OnClaimRewardComplete");
                onComplete?.Invoke(true);
                return;
            }

            MonetizrLogger.Print($"OnClaimRewardComplete for {mission.serverId}");

            ShowCongratsNotification((bool _) =>
            {
                bool updateUI = false;

                MonetizrLogger.Print($"OnClaimRewardComplete --> ShowCongratsNotification {mission.serverId}");

                if (mission.campaignServerSettings.GetParam("RewardCenter.do_not_claim_and_hide_missions") != "true")
                {
                    mission.state = MissionUIState.ToBeHidden;
                    mission.isClaimed = ClaimState.Claimed;
                }

                ClaimMissionData(mission);

                if (missionsManager.UpdateMissionsActivity(mission))
                {
                    updateUI = true;
                }

                if (mission.campaignServerSettings.GetBoolParam("claim_for_new_after_campaign_is_done", false))
                {
                    if (serverClaimForCampaigns && CheckFullCampaignClaim(mission))
                    {
                        ClaimReward(mission.campaign, CancellationToken.None, () => { RequestCampaigns(false); });
                    }
                }

                MonetizrManager.HideTeaser(true);
                onComplete?.Invoke(isSkipped);
                if (!updateUI) return;
                updateUIDelegate?.Invoke();

            }, mission);

        }

        internal void InitializeBuiltinMissionsForAllCampaigns()
        {
            if (!_isMissionsIsOutdated) return;
            missionsManager.LoadSerializedMissions();
            campaigns.ForEach((c) =>
            {
                Instance.InitializeBuiltinMissions(c);
            });
            missionsManager.SaveAndRemoveUnused();
            SetActiveCampaign(FindBestCampaignToActivate());
            _isMissionsIsOutdated = false;
        }

        internal ServerCampaign GetActiveCampaign()
        {
            if (!IsActiveAndEnabled()) return null;
            return _activeCampaignId;
        }

        internal ServerCampaign FindBestCampaignToActivate()
        {
            if (!IsActiveAndEnabled()) return null;

            if (_activeCampaignId != null)
            {
                var campaign = GetActiveCampaign();
                if (campaign.IsCampaignActivate()) return campaign;
            }

            foreach (var campaign in campaigns)
            {
                if (campaign == _activeCampaignId) continue;
                if (campaign.IsCampaignActivate()) return campaign;
            }

            return null;
        }

        internal bool HasCampaignsAndActive()
        {
            return _isActive && campaigns.Count > 0;
        }

        internal void SetActiveCampaign(ServerCampaign camp)
        {
            if (camp == _activeCampaignId) return;
            if (camp != _activeCampaignId) _isMissionsIsOutdated = true;
            _activeCampaignId = camp;
            closeRewardCenterAfterEveryMission = camp.serverSettings.GetBoolParam("RewardCenter.close_after_mission_completion", closeRewardCenterAfterEveryMission);
            MonetizrLogger.Print($"Active campaign: {_activeCampaignId}");
        }

        internal bool HasCampaign(string campaignId)
        {
            return campaigns.FindIndex(c => c.id == campaignId) >= 0;
        }

        internal async Task ClaimReward(ServerCampaign campaign, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            try
            {
                await ConnectionsClient.ClaimReward(campaign, ct, onSuccess, onFailure);
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintError($"Exception in ConnectionsClient.Claim for {campaign.id}\n{e}");
                onFailure.Invoke();
            }
        }

        #endregion

    }

}