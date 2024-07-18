using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Analytics;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.UI;

namespace Monetizr.SDK.Core
{
    public partial class MonetizrManager : MonoBehaviour
    {
        internal static MonetizrManager Instance { get; private set; } = null;
        internal static MonetizrAnalytics Analytics => Instance.ConnectionsClient.Analytics;
        public static Action<string, Dictionary<string, string>> ExternalAnalytics { internal get; set; } = null;
        internal MonetizrClient ConnectionsClient { get; private set; }
        public List<MissionDescription> sponsoredMissions { get; private set; }

        public delegate void UserDefinedEvent(string campaignId, string placement, EventType eventType);
        public delegate void OnComplete(OnCompleteStatus isSkipped);
        public UserDefinedEvent userDefinedEvent = null;
        public List<UnityEngine.Object> holdResources = new List<UnityEngine.Object>();
        public static string temporaryEmail = "";
        public static bool claimForSkippedCampaigns;
        public static bool closeRewardCenterAfterEveryMission = false;
        public static int defaultRewardAmount = 1000;
        public static string defaultTwitterLink = "";
        public static string bundleId = null;
        public static int abTestSegment = 0;
        internal static bool keepLocalClaimData;
        internal static bool serverClaimForCampaigns;
        internal static int maximumCampaignAmount = 1;
        internal static bool isVastActive = false;
        internal static bool tinyTeaserCanBeVisible;
        internal static RewardSelectionType temporaryRewardTypeSelection = RewardSelectionType.Product;
        internal static Dictionary<RewardType, GameReward> gameRewards = new Dictionary<RewardType, GameReward>();
        private static int debugAttempt = 0;
        private static Vector2? tinyTeaserPosition = null;
        private static Transform teaserRoot;
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

        private void OnApplicationQuit ()
        {
            Analytics?.OnApplicationQuit();
        }

        public static void SetAdvertisingIds (string advertisingID, bool limitAdvertising)
        {
            MonetizrMobileAnalytics.isAdvertisingIDDefined = true;
            MonetizrMobileAnalytics.advertisingID = advertisingID;
            MonetizrMobileAnalytics.limitAdvertising = limitAdvertising;
            Log.Print($"MonetizrManager SetAdvertisingIds: {MonetizrMobileAnalytics.advertisingID} {MonetizrMobileAnalytics.limitAdvertising}");
        }

        public static void SetGameCoinAsset (RewardType rt, Sprite defaultRewardIcon, string title,
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

        public static MonetizrManager Initialize (string apiKey, List<MissionDescription> sponsoredMissions = null, Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null)
        {
            return _Initialize(apiKey, sponsoredMissions, onRequestComplete, soundSwitch, onUIVisible, userEvent, null);
        }

        private static MonetizrManager _Initialize (string apiKey, List<MissionDescription> sponsoredMissions, Action onRequestComplete, Action<bool> soundSwitch, Action<bool> onUIVisible, UserDefinedEvent userEvent, MonetizrClient connectionClient)
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
                    Log.Print($"Audio listener pause state {!isOn}");
                    AudioListener.pause = !isOn;
                };
            }

            Log.Print($"MonetizrManager Initialize: {apiKey} {bundleId} {MonetizrSettings.SDKVersion}");

            if (!MonetizrMobileAnalytics.isAdvertisingIDDefined)
            {
                Log.PrintError($"MonetizrManager Initialize: Advertising ID is not defined. Be sure you called MonetizrManager.SetAdvertisingIds before Initialize call.");
                return null;
            }

            if (string.IsNullOrEmpty(bundleId)) bundleId = Application.identifier;

            var monetizrObject = new GameObject("MonetizrManager");
            var monetizrManager = monetizrObject.AddComponent<MonetizrManager>();
            var monetizrErrorLogger = monetizrObject.AddComponent<MonetizrErrorLogger>();

            DontDestroyOnLoad(monetizrObject);

            Instance = monetizrManager;
            Instance.sponsoredMissions = sponsoredMissions;
            Instance.userDefinedEvent = userEvent;
            Instance.onUIVisible = onUIVisible;

            monetizrManager.Initialize(apiKey, onRequestComplete, soundSwitch, connectionClient);

            return Instance;
        }

        private void Initialize (string apiKey, Action gameOnInitSuccess, Action<bool> soundSwitch, MonetizrClient connectionClient)
        {
#if USING_WEBVIEW
            if (!UniWebView.IsWebViewSupported)
            {
                Log.Print("WebView isn't supported on current platform!");
            }
#endif

            localSettings = new LocalSettingsManager();
            missionsManager = new MissionsManager();
            this._soundSwitch = soundSwitch;
            ConnectionsClient = connectionClient ?? new MonetizrHttpClient(apiKey);
            ConnectionsClient.Initialize();
            InitializeUI();
            _gameOnInitSuccess = gameOnInitSuccess;

            _onRequestComplete = (bool isOk) =>
            {
                gameOnInitSuccess?.Invoke();
                gameOnInitSuccess = null;

                if (tinyTeaserCanBeVisible)
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

        public async void RequestCampaigns(Action<bool> onRequestComplete)
        {
            await ConnectionsClient.GetGlobalSettings();
            campaigns = new List<ServerCampaign>();

            try
            {
                campaigns = await ConnectionsClient.GetList();
            }
            catch (Exception e)
            {
                Log.PrintError($"Exception while getting list of campaigns\n{e}");
                onRequestComplete?.Invoke(false);
            }

            if (campaigns == null)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]}");
                onRequestComplete?.Invoke(false);
            }

            var logConnectionErrors = ConnectionsClient.GlobalSettings.GetBoolParam("mixpanel.log_connection_errors", true);

            if (campaigns.Count > 0)
            {
                ConnectionsClient.SetTestMode(campaigns[0].testmode);
                ConnectionsClient.Analytics.Initialize(campaigns[0].testmode, campaigns[0].panel_key, logConnectionErrors);
                ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingStarts, EventType.Notification);
            }
            else
            {
                ConnectionsClient.Analytics.Initialize(false, null, logConnectionErrors);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(10000);
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

            foreach (var campaign in campaigns)
            {
                await campaign.LoadCampaignAssets();

                if (campaign.isLoaded)
                {
                    Log.Print($"Campaign {campaign.id} successfully loaded");
                }
                else
                {
                    Log.PrintError($"Campaign {campaign.id} loading failed with error {campaign.loadingError}!");
                    ConnectionsClient.Analytics.TrackEvent(campaign, null, AdPlacement.AssetsLoading, EventType.Error, new Dictionary<string, string> { { "loading_error", campaign.loadingError } });
                }
            }

            campaigns.RemoveAll(c => c.isLoaded == false);

#if TEST_SLOW_LATENCY
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

            localSettings.LoadOldAndUpdateNew(campaigns);
            Log.Print($"RequestCampaigns completed with {campaigns.Count} campaigns.");
            if (campaigns.Count > 0) ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingEnds, EventType.Notification);

            if (gameRewards.Count == 0)
            {
                Log.PrintError($"No in-game rewards defined. Don't forget to call MonetizrManager.SetGameCoinAsset after SDK initialization.");
                return;
            }

            foreach (var i in gameRewards)
            {
                if (!i.Value.IsSetupValid()) return;
            }

            Log.Print("MonetizrManager initialization okay!");
            _isActive = true;
            onRequestComplete?.Invoke(true);
        }

        private IEnumerator TryRequestCampaignsLater (float time)
        {
            while (true)
            {
                yield return new WaitForSeconds(time);
                if (campaigns.Count != 0) continue;
                _isActive = false;
                RequestCampaigns(_onRequestComplete);
            }
        }

        internal static void _CallUserDefinedEvent(string campaignId, string placement, EventType eventType)
        {
            try
            {
                Instance?.userDefinedEvent?.Invoke(campaignId, placement, eventType);
            }
            catch (Exception ex)
            {
                Log.PrintError($"Exception in _CallUserDefinedEvent\n{ex}");
            }
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

        internal static GameReward GetGameReward (RewardType rt)
        {
            LogGameRewards();

            if (gameRewards.ContainsKey(rt))
            {
                return gameRewards[rt];
            }

            return null;
        }

        public static void LogGameRewards()
        {
            if (gameRewards == null || gameRewards.Count == 0)
            {
                UnityEngine.Debug.Log("gameRewards dictionary is null or empty.");
                return;
            }

            foreach (KeyValuePair<RewardType, GameReward> entry in gameRewards)
            {
                UnityEngine.Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }

        internal static MonetizrManager InitializeForTests(string apiKey, List<MissionDescription> sponsoredMissions = null, Action onRequestComplete = null, Action<bool> soundSwitch = null, Action<bool> onUIVisible = null, UserDefinedEvent userEvent = null, MonetizrClient connectionClient = null)
        {
            return _Initialize(apiKey, sponsoredMissions, onRequestComplete, soundSwitch, onUIVisible, userEvent, connectionClient);
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
            Log.Print($"Changing api key to {apiKey}");
            ConnectionsClient.currentApiKey = apiKey;
            return true;
        }

        internal void RequestCampaigns(bool callRequestComplete = true)
        {
            _isActive = false;
            _isMissionsIsOutdated = true;
            _uiController.DestroyTinyMenuTeaser();
            missionsManager.CleanUp();
            campaigns.Clear();
            _activeCampaignId = null;
            RequestCampaigns(callRequestComplete ? _onRequestComplete : null);
        }

        public void SoundSwitch(bool on)
        {
            _soundSwitch?.Invoke(on);
        }

        private void InitializeUI()
        {
            _uiController = new UIController();
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
                Log.Print($"Nothing to reset in ResetCampaign");
                return;
            }

            string campaignId = m.campaignId;
            var lscreen = Instance._uiController.ShowLoadingScreen();
            lscreen._onComplete = (bool _) => { GameObject.Destroy(lscreen); };
            CancellationTokenSource s_cts = new CancellationTokenSource();

            try
            {
                s_cts.CancelAfter(10000);
                await Instance.ConnectionsClient.Reset(campaignId, s_cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Print("\nTasks cancelled: timed out.\n");
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
                Log.Print("SUCCESS!");
                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.ButtonPressOk);
                MonetizrManager.Instance.OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);
            };

            Action onFail = () =>
            {
                Log.Print("FAIL!");;
                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.Error);
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
                    Log.Print("\nTasks cancelled: timed out.\n");
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
                Log.Print($"ShowStartupNotification ContainsKey(PanelId.StartNotification) {placement}");
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
                    Log.Print($"No notifications on level start defined on server-side");
            }
            else if (placement == NotificationPlacement.MainMenuShowNotification)
            {
                forceSkip = mission.campaignServerSettings.GetParam("no_main_menu_notifications") == "true";

                if (forceSkip)
                    Log.Print($"No notifications in main menu defined on server-side");
            }

            if (mission.campaignServerSettings.GetParam("no_campaigns_notification") == "true")
            {
                Log.Print($"No notifications defined on serverside");
                forceSkip = true;
            }

            mission.amountOfNotificationsSkipped++;

            if (mission.amountOfNotificationsSkipped <=
                mission.campaignServerSettings.GetIntParam("amount_of_skipped_notifications"))
            {
                Log.Print($"Amount of skipped notifications less then {mission.amountOfNotificationsSkipped}");
                forceSkip = true;
            }

            var serverMaxAmount = mission.campaignServerSettings.GetIntParam("amount_of_notifications");
            var currentAmount = Instance.localSettings.GetSetting(mission.campaignId).amountNotificationsShown;
            if (currentAmount > serverMaxAmount)
            {
                Log.Print($"Startup notification impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                forceSkip = true;
            }

            var lastTimeShow = Instance.localSettings.GetSetting(mission.campaignId).lastTimeShowNotification;
            var serverDelay = mission.campaignServerSettings.GetIntParam("notifications_delay_time_sec");
            var lastTime = (DateTime.Now - lastTimeShow).TotalSeconds;

            if (lastTime < serverDelay)
            {
                Log.Print($"Startup notification last show time less then {serverDelay}");
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
            Log.Print($"Notification shown {currentAmount}/{serverMaxAmount} last time: {lastTime}/{serverDelay}");
            ShowNotification(onComplete, mission, PanelId.StartNotification);
        }

        internal static void ShowCongratsNotification(Action<bool> onComplete, Mission m)
        {
            ShowNotification(onComplete, m, PanelId.CongratsNotification);
        }

        internal void ClaimMissionData(Mission m)
        {
            gameRewards[m.rewardType].AddCurrencyAction(m.reward);
            if (keepLocalClaimData) Instance.SaveClaimedReward(m);
        }

        public static Canvas GetMainCanvas()
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            return Instance?._uiController?.GetMainCanvas();
        }

        internal static void CleanUserDefinedMissions()
        {
            Instance.missionsManager.CleanUserDefinedMissions();
        }

        public static void EngagedUserAction(OnComplete onComplete)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            var missions = Instance.missionsManager.GetMissionsForRewardCenter(Instance?.GetActiveCampaign());

            if (missions == null || missions.Count == 0)
            {
                onComplete(OnCompleteStatus.Skipped);
                return;
            }

            if (missions[0].amountOfRVOffersShown == 0)
            {
                onComplete(OnCompleteStatus.Skipped);
                return;
            }

            missions[0].amountOfRVOffersShown--;

            MonetizrManager.ShowRewardCenter(null,(bool p) =>
            {
                Log.PrintV("ShowRewardCenter OnComplete!");
                onComplete(p ? OnCompleteStatus.Skipped : OnCompleteStatus.Completed);
            });
        }

        public static void ShowRewardCenter(Action UpdateGameUI, Action<bool> onComplete = null)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            UpdateGameUI?.Invoke();
            var campaign = Instance?.FindBestCampaignToActivate();

            if (campaign == null)
            {
                onComplete?.Invoke(true);
                Log.Print($"No active campaigns for reward center");
                return;
            }

            Instance?.SetActiveCampaign(campaign);
            var missions = Instance.missionsManager.GetMissionsForRewardCenter(campaign);

            if (missions.Count == 0)
            {
                onComplete?.Invoke(true);
                return;
            }

            var m = missions[0];
            bool showRewardCenterForOneMission = missions[0].campaignServerSettings.GetBoolParam("RewardCenter.show_for_one_mission", false);

            if (missions.Count == 1 && !showRewardCenterForOneMission)
            {
                Log.Print($"Only one mission available and RewardCenter.show_for_one_mission is false");

                Instance._PressSingleMission(onComplete, m);
                return;
            }

            Log.PrintV($"ShowRewardCenter from campaign: {m?.campaignId}");
            string uiItemPrefab = "MonetizrRewardCenterPanel2";
            Instance._uiController.ShowPanelFromPrefab(uiItemPrefab, PanelId.RewardCenter, onComplete, true, m);
        }

        internal static void HideRewardCenter()
        {
            Instance._uiController.HidePanel(PanelId.RewardCenter);
        }

        internal void _PressSingleMission(Action<bool> onComplete, Mission m)
        {
            if (m.isClaimed == ClaimState.Claimed) return;
            MonetizrManager.Instance.missionsManager.ClaimAction(m, onComplete, null).Invoke();
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

        internal static void GoToLink(Action<bool> onComplete, Mission m = null)
        {
            Application.OpenURL(m.surveyUrl);
            onComplete.Invoke(false);
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

        internal static void ShowWebVideo(Action<bool> onComplete, Mission m = null)
        {
            _ShowWebView(onComplete, PanelId.VideoWebView, m);
        }

        public static void SetTeaserPosition(Vector2 pos)
        {
            tinyTeaserPosition = pos;
        }

        public static void SetTeaserRoot(Transform root)
        {
            teaserRoot = root;
        }

        public static void OnMainMenuShow(bool showNotifications = true)
        {
            tinyTeaserCanBeVisible = true;
            if (Instance == null) return;
            if (!Instance.HasCampaignsAndActive()) return;
            Instance.InitializeBuiltinMissionsForAllCampaigns();
            var campaign = Instance.FindBestCampaignToActivate();
            Instance.SetActiveCampaign(campaign);
            if (campaign == null) return;

            if (showNotifications)
            {
                HideTinyMenuTeaser();

                ShowStartupNotification(NotificationPlacement.MainMenuShowNotification, (bool isSkipped) =>
                {
                    if (isSkipped)
                        ShowTinyMenuTeaser();
                    else
                        ShowRewardCenter(null, null);
                });
            }
            else
            {
                ShowTinyMenuTeaser();
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
            tinyTeaserCanBeVisible = false;
            HideTinyMenuTeaser();
        }

        public static void ShowTinyMenuTeaser(Action UpdateGameUI = null)
        {
            if (!MonetizrManager.tinyTeaserCanBeVisible) return;
            if (Instance == null) return;
            var campaign = Instance?.FindBestCampaignToActivate();

            if (campaign == null)
            {
                Log.Print($"No active campaigns for teaser");
                return;
            }

            Instance?.SetActiveCampaign(campaign);

            if (Instance.missionsManager.GetActiveMissionsNum(campaign) == 0)
            {
                Log.Print($"No active missions for teaser");
                return;
            }

            if (!campaign.HasAsset(AssetsType.TinyTeaserSprite) &&
                !campaign.HasAsset(AssetsType.TeaserGifPathString) &&
                !campaign.HasAsset(AssetsType.BrandRewardLogoSprite))
            {
                Log.Print("No texture for tiny teaser!");
                return;
            }

            if (campaign.serverSettings.GetParam("hide_teaser_button") == "true") return;

            var serverMaxAmount = campaign.serverSettings.GetIntParam("amount_of_teasers");
            var currentAmount = Instance.localSettings.GetSetting(campaign.id).amountTeasersShown;
            if (currentAmount > serverMaxAmount)
            {
                Log.Print($"Teaser impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                return;
            }

            Log.Print($"Teaser shown {currentAmount}/{serverMaxAmount}");
            Instance.localSettings.GetSetting(campaign.id).amountTeasersShown++;
            Instance.localSettings.SaveData();
            int uiVersion = 4;
            Instance._uiController.ShowTinyMenuTeaser(teaserRoot, tinyTeaserPosition, UpdateGameUI, uiVersion, campaign);
        }

        public static void HideTinyMenuTeaser(bool checkIfSomeMissionsAvailable = false)
        {
            if (Instance == null) return;
            if (checkIfSomeMissionsAvailable && Instance.missionsManager.GetActiveMissionsNum() > 0) return;
            if (!Instance._isActive) return;
            Instance._uiController.HidePanel(PanelId.TinyMenuTeaser);
        }

        internal void OnClaimRewardComplete(Mission mission, bool isSkipped, Action<bool> onComplete,
            Action updateUIDelegate)
        {
            if (claimForSkippedCampaigns)
                isSkipped = false;

            if (isSkipped)
            {
                Log.PrintV("OnClaimRewardComplete");
                onComplete?.Invoke(true);
                return;
            }

            Log.PrintV($"OnClaimRewardComplete for {mission.serverId}");

            ShowCongratsNotification((bool _) =>
            {
                bool updateUI = false;

                Log.PrintV($"OnClaimRewardComplete --> ShowCongratsNotification {mission.serverId}");

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

                MonetizrManager.HideTinyMenuTeaser(true);
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

        public static bool IsActiveAndEnabled()
        {
            return Instance != null && Instance.HasCampaignsAndActive();
        }
        
        internal void SetActiveCampaign(ServerCampaign camp)
        {
            if (camp == _activeCampaignId) return;
            if (camp != _activeCampaignId) _isMissionsIsOutdated = true;
            _activeCampaignId = camp;
            closeRewardCenterAfterEveryMission = camp.serverSettings.GetBoolParam("RewardCenter.close_after_mission_completion", closeRewardCenterAfterEveryMission);
            Log.PrintV($"Active campaign: {_activeCampaignId}");
        }
        
        public bool HasActiveCampaign()
        {
            return _isActive && _activeCampaignId != null;
        }

        public static bool IsInitialized()
        {
            return Instance != null;
        }
        
        internal bool HasCampaign(string campaignId)
        {
            return campaigns.FindIndex(c => c.id == campaignId) >= 0;
        }

        internal async Task ClaimReward(ServerCampaign campaign, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            try
            {
                await ConnectionsClient.Claim(campaign, ct, onSuccess, onFailure);
            }
            catch (Exception e)
            {
                Log.PrintError($"Exception in ConnectionsClient.Claim for {campaign.id}\n{e}");
                onFailure.Invoke();
            }
        }

    }

}