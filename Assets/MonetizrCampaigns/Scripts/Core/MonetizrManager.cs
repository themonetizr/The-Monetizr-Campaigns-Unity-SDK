//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.SDK;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Assertions;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Linq;
using mixpanel;
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
        public static float requestCampaignTime = 5 * 60;
        public static readonly string SDKVersion = "1.0.5";

        internal static bool keepLocalClaimData;
        internal static bool serverClaimForCampaigns;
        public static bool claimForSkippedCampaigns;

        public static bool closeRewardCenterAfterEveryMission = false;

        internal static int maximumCampaignAmount = 1;

        internal static bool isVastActive = false;


        //position relative to center with 1080x1920 screen resolution
        private static Vector2? tinyTeaserPosition = null;

        private static Transform teaserRoot;

        internal MonetizrClient ConnectionsClient { get; private set; }

        public List<MissionDescription> sponsoredMissions { get; private set; }

        private UIController _uiController = null;

        private ServerCampaign _activeCampaignId = null;

        private Action<bool> _soundSwitch = null;
        private Action<bool> _onRequestComplete = null;
        internal Action<bool> onUIVisible = null;

        private bool _isActive = false;
        private bool _isMissionsIsOutdated = true;

        private List<ServerCampaign> campaigns = new List<ServerCampaign>();

        internal static bool tinyTeaserCanBeVisible;

        internal MissionsManager missionsManager = null;

        internal LocalSettingsManager localSettings = null;

        public delegate void UserDefinedEvent(string campaignId, string placement, EventType eventType);

        /// <summary>
        /// This delegate is using for tracking user defines events from SDK side
        /// </summary>
        public UserDefinedEvent userDefinedEvent = null;

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


        //Hold resources to prevent automatic unload
        public static void HoldResource(object o)
        {
            //if(o.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
            if (o is UnityEngine.Object)
            {
                UnityEngine.Object uo = (UnityEngine.Object)o;

                if (!Instance.holdResources.Contains(uo))
                {
                    Instance.holdResources.Add(uo);
                }
            }
        }

        public List<UnityEngine.Object> holdResources = new List<UnityEngine.Object>();

        public static string temporaryEmail = "";

        internal static RewardSelectionType temporaryRewardTypeSelection = RewardSelectionType.Product;

        public static int defaultRewardAmount = 1000;
        public static string defaultTwitterLink = "";

        internal static Dictionary<RewardType, GameReward> gameRewards = new Dictionary<RewardType, GameReward>();
        private static int debugAttempt = 0;
        public static int abTestSegment = 0;

        public static string bundleId = null;
        private Action _gameOnInitSuccess;

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

        public static void SetAdvertisingIds(string advertisingID, bool limitAdvertising)
        {
            MonetizrMobileAnalytics.isAdvertisingIDDefined = true;

            MonetizrMobileAnalytics.advertisingID = advertisingID;
            MonetizrMobileAnalytics.limitAdvertising = limitAdvertising;

            Log.Print(
                $"MonetizrManager SetAdvertisingIds: {MonetizrMobileAnalytics.advertisingID} {MonetizrMobileAnalytics.limitAdvertising}");
        }

        public static MonetizrManager Initialize(string apiKey,
            List<MissionDescription> sponsoredMissions = null,
            Action onRequestComplete = null,
            Action<bool> soundSwitch = null,
            Action<bool> onUIVisible = null,
            UserDefinedEvent userEvent = null)
        {
            return _Initialize(apiKey, sponsoredMissions, onRequestComplete, soundSwitch, onUIVisible, userEvent, null);
        }

        internal static MonetizrManager InitializeForTests(string apiKey,
            List<MissionDescription> sponsoredMissions = null,
            Action onRequestComplete = null,
            Action<bool> soundSwitch = null,
            Action<bool> onUIVisible = null,
            UserDefinedEvent userEvent = null,
            MonetizrClient connectionClient = null)
        {
            return _Initialize(apiKey, sponsoredMissions, onRequestComplete, soundSwitch, onUIVisible, userEvent, connectionClient);
        }

        private static MonetizrManager _Initialize(string apiKey,
            List<MissionDescription> sponsoredMissions,
            Action onRequestComplete,
            Action<bool> soundSwitch,
            Action<bool> onUIVisible,
            UserDefinedEvent userEvent,
            MonetizrClient connectionClient)
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

            Log.Print($"MonetizrManager Initialize: {apiKey} {bundleId} {SDKVersion}");

            if (!MonetizrMobileAnalytics.isAdvertisingIDDefined)
            {
                Log.PrintError(
                    $"MonetizrManager Initialize: Advertising ID is not defined. Be sure you called MonetizrManager.SetAdvertisingIds before Initialize call.");
                return null;
            }

            if (string.IsNullOrEmpty(bundleId))
                bundleId = Application.identifier;

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

        internal static MonetizrManager Instance { get; private set; } = null;

        internal static MonetizrAnalytics Analytics => Instance.ConnectionsClient.Analytics;

        void OnApplicationQuit()
        {
            Analytics?.OnApplicationQuit();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize(string apiKey, Action gameOnInitSuccess, Action<bool> soundSwitch, MonetizrClient connectionClient)
        {
#if USING_WEBVIEW
            if (!UniWebView.IsWebViewSupported)
            {
                Log.Print("WebView isn't supported on current platform!");
            }
#endif
            //ExternalAnalytics = null;

            localSettings = new LocalSettingsManager();

            missionsManager = new MissionsManager();

            //missionsManager.CleanUp();

            this._soundSwitch = soundSwitch;

            ConnectionsClient = connectionClient ?? new MonetizrHttpClient(apiKey);
            
            ConnectionsClient.Initialize();

            InitializeUI();

            _gameOnInitSuccess = gameOnInitSuccess;

            _onRequestComplete = (bool isOk) =>
            {
                //moved together with showing teaser, because here in-game logic may not be ready
                //                createEmbedMissions();

                gameOnInitSuccess?.Invoke();
                gameOnInitSuccess = null;

                if (tinyTeaserCanBeVisible)
                {
                    //isMissionsIsOudated = true;
                    //ShowTinyMenuTeaser(null);
                    OnMainMenuShow(false);
                }

            };

            RequestCampaigns(_onRequestComplete);

            if (requestCampaignTime > 0)
                StartCoroutine(TryRequestCampaignsLater(requestCampaignTime));
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
            //if (_isMissionsIsOutdated)
            missionsManager.CreateMissionsFromCampaign(campaign);

            //_isMissionsIsOutdated = false;
        }


        //check if all mission with current campain claimed
        internal bool CheckFullCampaignClaim(Mission m)
        {
            return missionsManager.CheckFullCampaignClaim(m);
        }

        internal void SaveClaimedReward(Mission m)
        {
            //missionsManager.SaveClaimedReward(m);

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
            if (apiKey == ConnectionsClient.currentApiKey)
                return false;

            Log.Print($"Changing api key to {apiKey}");

            //RestartClient();

            //RequestCampaigns();

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
            //_campaigns.Clear();
            //_campaignIds.Clear();

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

            Instance._uiController.ShowPanelFromPrefab("MonetizrMessagePanel2",
                panelId,
                onComplete,
                true,
                m);
        }

        internal static void ShowNotification(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            Instance._uiController.ShowPanelFromPrefab("MonetizrNotifyPanel2",
                panelId,
                onComplete,
                true,
                m);
        }

        internal static void ShowEnterEmailPanel(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            Instance._uiController.ShowPanelFromPrefab("MonetizrEnterEmailPanel2",
                panelId,
                onComplete,
                true,
                m);
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

            //show screen to block
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

                //onFail.Invoke();
            }
            finally
            {
                s_cts.Dispose();
            }


            lscreen.SetActive(false);
        }

        internal static async void WaitForEndRequestAndNotify(Action<bool> onComplete, Mission m,
            Action updateUIDelegate)
        {
            //show screen to block
            var lscreen = Instance._uiController.ShowLoadingScreen();

            lscreen._onComplete = (bool _) => { GameObject.Destroy(lscreen); };

            Action onSuccess = () =>
            {
                Log.Print("SUCCESS!");

                //MonetizrManager.analytics.TrackEvent("Enter email succeeded", m);

                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.ButtonPressOk);

                MonetizrManager.Instance.OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);
            };

            Action onFail = () =>
            {
                Log.Print("FAIL!");

                //MonetizrManager.analytics.TrackEvent("Email enter failed", m);

                //MonetizrManager.analytics.TrackEvent(m, AdPlacement.EmailEnterInGameRewardScreen, MonetizrManager.EventType.Error);

                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.Error);


                ShowMessage((bool _) => { onComplete?.Invoke(false); },
                    m,
                    PanelId.BadEmailMessageNotification);


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

                    //onFail.Invoke();
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

            //GameObject.Destroy(lscreen);

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

            //Log.Print($"------ShowStartupNotification 1 {placement}");

            if (Instance == null || !Instance.HasActiveCampaign())
            {
                onComplete?.Invoke(true);
                return;
            }

            //Log.Print($"------ShowStartupNotification 2 {placement}");

            //Log.PrintWarning("ShowStartupNotification");

            //Mission sponsoredMsns = instance.missionsManager.missions.Find((Mission item) => { return item.isSponsored; });
            var missions = Instance.missionsManager.GetMissionsForRewardCenter(Instance.GetActiveCampaign());

            if (missions == null || missions?.Count == 0)
            {
                onComplete?.Invoke(true);
                return;
            }

            Mission mission = missions[0];

            //manual notification calls, no limits
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

            // Log.Print($"------ShowStartupNotification 3 {placement}");

            //var campaign = MonetizrManager.Instance.GetCampaign(mission.campaignId);

            if (mission.campaignServerSettings.GetParam("no_campaigns_notification") == "true")
            {
                Log.Print($"No notifications defined on serverside");
                forceSkip = true;
            }

            //Log.Print($"Notifications sk {mission.amountOfNotificationsSkipped} shown {mission.amountOfNotificationsShown}");

            mission.amountOfNotificationsSkipped++;

            if (mission.amountOfNotificationsSkipped <=
                mission.campaignServerSettings.GetIntParam("amount_of_skipped_notifications"))
            {
                Log.Print($"Amount of skipped notifications less then {mission.amountOfNotificationsSkipped}");
                forceSkip = true;
            }

            //check if need to limit notifications amount
            var serverMaxAmount = mission.campaignServerSettings.GetIntParam("amount_of_notifications");
            var currentAmount = Instance.localSettings.GetSetting(mission.campaignId).amountNotificationsShown;
            if (currentAmount > serverMaxAmount)
            {
                Log.Print($"Startup notification impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                forceSkip = true;
            }

            //check last time
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

            //mission.amountOfNotificationsShown--;

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

            if (keepLocalClaimData)
                Instance.SaveClaimedReward(m);
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

        public delegate void OnComplete(OnCompleteStatus isSkipped);

        /// <summary>
        /// This method helps to show Monetizr offer instead of RV ads
        /// If player skipped Monetizr offer or haven't seen anything 
        /// OnComplete callback will be called with the parameter OnCompleteStatus.Skipped
        /// </summary>
        /// <param name="onComplete">Depending on whether the player completed the task this method calls with the corresponding parameter
        /// </param>
        /// 
        public static void EngagedUserAction(OnComplete onComplete)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            var missions = Instance.missionsManager.GetMissionsForRewardCenter(Instance?.GetActiveCampaign());

            if (missions == null || missions.Count == 0)
            {
                onComplete(OnCompleteStatus.Skipped);
                return;
            }

            //no more offers, skipping
            if (missions[0].amountOfRVOffersShown == 0)
            {
                onComplete(OnCompleteStatus.Skipped);
                return;
            }

            missions[0].amountOfRVOffersShown--;


            MonetizrManager.ShowRewardCenter(null,
                (bool p) =>
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

            //has some challanges
            if (campaign == null)
            {
                onComplete?.Invoke(true);
                Log.Print($"No active campaigns for reward center");
                return;
            }

            Instance?.SetActiveCampaign(campaign);

            //var challengeId = Instance.GetActiveCampaignId();

            /*var m = Instance.missionsManager.GetMission(challengeId);

            //no missions, consider as a skipped
            if (m == null)
            {
                onComplete?.Invoke(true);
                return;
            }*/

            var missions = Instance.missionsManager.GetMissionsForRewardCenter(campaign);

            if (missions.Count == 0)
            {
                onComplete?.Invoke(true);
                return;
            }

            var m = missions[0];

            bool showRewardCenterForOneMission = missions[0].campaignServerSettings
                .GetBoolParam("RewardCenter.show_for_one_mission", false);

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
            if (m.isClaimed == ClaimState.Claimed)
                return;

            //MonetizrManager.Instance.missionsManager.GetEmailGiveawayClaimAction(m, _onComplete, null).Invoke();

            MonetizrManager.Instance.missionsManager.ClaimAction(m, onComplete, null).Invoke();
        }

        internal static void ShowCodeView(Action<bool> onComplete, Mission m = null)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!Instance._isActive)
                return;

            Instance._uiController.ShowPanelFromPrefab("MonetizrEnterCodePanel2", PanelId.CodePanelView, onComplete, false, m);
        }

        internal static void ShowMinigame(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!Instance._isActive)
                return;

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

            Instance._uiController.ShowPanelFromPrefab(panelNames[m.type].Item2, panelNames[m.type].Item1, onComplete,
                false, m);
        }

        internal static void ShowUnitySurvey(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!Instance._isActive)
                return;

            Instance._uiController.ShowPanelFromPrefab("MonetizrUnitySurveyPanel", PanelId.SurveyUnityView, onComplete,
                false, m);
        }


        internal static void _ShowWebView(Action<bool> onComplete, PanelId id, Mission m = null)
        {
            Assert.IsNotNull(Instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!Instance._isActive)
                return;

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

        public static Action<string, Dictionary<string, string>> ExternalAnalytics { internal get; set; } = null;


        /// <summary>
        /// Call this method to show Notification and if player close it teaser will be shown
        /// </summary>
        /// <param name="showNotifications"></param>
        public static void OnMainMenuShow(bool showNotifications = true)
        {
            tinyTeaserCanBeVisible = true;

            if (Instance == null)
                return;


            if (!Instance.HasCampaignsAndActive())
                return;


            Instance.InitializeBuiltinMissionsForAllCampaigns();

            var campaign = Instance.FindBestCampaignToActivate();

            Instance.SetActiveCampaign(campaign);

            if (campaign == null)
                return;


            //Instance.InitializeBuiltinMissions(campaign);

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

        /// <summary>
        /// Shows campaign notification and if player press Ok button - he sees an offer
        /// </summary>
        /// <param name="onComplete">
        /// IF there's no campaigns, if player closed notification or if player do not complete task 
        /// - OnComplete called with parameter OnCompleteStatus.Skipped
        /// IF campaign task is completed
        /// - OnComplete called with parameter OnCompleteStatus.Completed
        /// </param>
        public static void ShowCampaignNotificationAndEngage(OnComplete onComplete = null)
        {
            if (Instance == null || !Instance.HasCampaignsAndActive())
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }

            //tinyTeaserCanBeVisible = true;
            Instance.InitializeBuiltinMissionsForAllCampaigns();


            var campaign = MonetizrManager.Instance?.GetActiveCampaign();

            if (campaign == null)
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }


            //Instance.InitializeBuiltinMissions(campaign);

            //Notification is shown
            ShowStartupNotification(NotificationPlacement.ManualNotification, (bool isSkipped) =>
            {
                //If notification is closed
                if (isSkipped)
                {
                    onComplete?.Invoke(OnCompleteStatus.Skipped);
                }
                //If notification isn't closed
                else
                {
                    ShowRewardCenter(null, (bool isRCSkipped) =>
                    {
                        //Does player complete the task?
                        onComplete?.Invoke(isRCSkipped ? OnCompleteStatus.Skipped : OnCompleteStatus.Completed);
                    });
                }
            });



        }

        public static void OnMainMenuHide()
        {
            tinyTeaserCanBeVisible = false;

            HideTinyMenuTeaser();

            //ShowStartupNotification((bool _) => {  });
        }

        public static void ShowTinyMenuTeaser(Action UpdateGameUI = null)
        {
            //Log.Print("ShowTinyMenuTeaser");
            if (!MonetizrManager.tinyTeaserCanBeVisible)
                return;

            //Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            //
            if (Instance == null)
                return;

            //tinyTeaserCanBeVisible = true;

            var campaign = Instance?.FindBestCampaignToActivate();

            //has some challanges
            if (campaign == null)
            {
                Log.Print($"No active campaigns for teaser");
                return;
            }

            Instance?.SetActiveCampaign(campaign);

            //has some active missions
            if (Instance.missionsManager.GetActiveMissionsNum(campaign) == 0)
            {
                Log.Print($"No active missions for teaser");
                return;
            }

            //var challengeId = MonetizrManager.Instance.GetActiveCampaignId();
            //var campaign = MonetizrManager.Instance.GetCampaign(challengeId);

            if (!campaign.HasAsset(AssetsType.TinyTeaserSprite) &&
                !campaign.HasAsset(AssetsType.TeaserGifPathString) &&
                !campaign.HasAsset(AssetsType.BrandRewardLogoSprite))
            {
                Log.Print("No texture for tiny teaser!");
                return;
            }

            if (campaign.serverSettings.GetParam("hide_teaser_button") == "true")
                return;

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

            int uiVersion = 4; //campaign.serverSettings.GetIntParam("teaser_design_version", 2);

            Instance._uiController.ShowTinyMenuTeaser(teaserRoot, tinyTeaserPosition, UpdateGameUI, uiVersion,
                campaign);

        }

        public static void HideTinyMenuTeaser(bool checkIfSomeMissionsAvailable = false)
        {
            //Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (Instance == null)
                return;

            if (checkIfSomeMissionsAvailable && Instance.missionsManager.GetActiveMissionsNum() > 0)
                return;

            //tinyTeaserCanBeVisible = false;

            if (!Instance._isActive)
                return;

            // MonetizrManager.analytics.EndShowAdAsset(AdPlacement.TinyTeaser);

            //MonetizrManager.analytics.TrackEvent(null, null, EventType.ImpressionEnds);

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

                /*if (missionsManager.TryToActivateSurvey(mission))
                {
                    //UpdateUI();
                    updateUI = true;
                }*/


                if (mission.campaignServerSettings.GetBoolParam("claim_for_new_after_campaign_is_done", false))
                {
                    if (serverClaimForCampaigns && CheckFullCampaignClaim(mission))
                    {
                        ClaimReward(mission.campaign, CancellationToken.None, () => { RequestCampaigns(false); });

                    }
                }

                MonetizrManager.HideTinyMenuTeaser(true);

                onComplete?.Invoke(isSkipped);

                if (!updateUI)
                    return;

                updateUIDelegate?.Invoke();


            }, mission);

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

                    ConnectionsClient.Analytics.TrackEvent(campaign, null,
                        AdPlacement.AssetsLoading,
                        EventType.Error,
                        new Dictionary<string, string> { { "loading_error", campaign.loadingError } });

                    if (ConnectionsClient.GlobalSettings.GetBoolParam("openrtb.sent_error_report_to_slack", true))
                    {
                        //ConnectionsClient.SendErrorToRemoteServer("Campaign loading assets error",
                        //    "Campaign loading assets error",
                        //    $"Campaign {campaign.id} loading error:\nApp: {bundleId}\nApp version: {Application.version}\nSystem language: {Application.systemLanguage}\n\n{campaign.loadingError}");

                    }
                }
            }

            campaigns.RemoveAll(c => c.isLoaded == false);

            // _activeCampaignId = _campaignIds.Count > 0 ? _campaignIds[0] : null;

            //_isMissionsIsOutdated = true;

#if TEST_SLOW_LATENCY
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif
            localSettings.LoadOldAndUpdateNew(campaigns);


            Log.Print($"RequestCampaigns completed with {campaigns.Count} campaigns.");

            /*if (_activeCampaignId != null)
            {
                //httpClient.analytics.TrackEvent("Get List Finished", activeChallengeId, true);

                ConnectionsClient.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.ImpressionEnds);
            }
            else
            {
                if (campaigns.Count > 0)
                {
                    //httpClient.analytics.TrackEvent("Get List Load Failed", campaigns[0]);

                    ConnectionsClient.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.Error);
                }
            }*/

            if (campaigns.Count > 0)
                ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingEnds, EventType.Notification);

            /* if (!isOk)
             {
                 Log.Print("Request complete is not okay!");
                 return;
             }*/

            if (gameRewards.Count == 0)
            {
                Log.PrintError(
                    $"No in-game rewards defined. Don't forget to call MonetizrManager.SetGameCoinAsset after SDK initialization.");
                return;
            }

            foreach (var i in gameRewards)
            {
                if (!i.Value.Validate())
                {
                    return;
                }
            }


            Log.Print("MonetizrManager initialization okay!");

            _isActive = true;


            //Ok, even if response empty
            onRequestComplete?.Invoke( /*challengesId.Count > 0*/true);
        }

        internal void InitializeBuiltinMissionsForAllCampaigns()
        {
            if (!_isMissionsIsOutdated)
                return;

            missionsManager.LoadSerializedMissions();

            campaigns.ForEach((c) =>
            {
                //_isMissionsIsOutdated = true;
                Instance.InitializeBuiltinMissions(c);
            });

            missionsManager.SaveAndRemoveUnused();

            SetActiveCampaign(FindBestCampaignToActivate());

            _isMissionsIsOutdated = false;
        }
        
        internal ServerCampaign GetActiveCampaign()
        {
            if (!IsActiveAndEnabled())
                return null;

            return _activeCampaignId;
        }

        internal ServerCampaign FindBestCampaignToActivate()
        {
            if (!IsActiveAndEnabled())
                return null;

            if (_activeCampaignId != null)
            {
                var campaign = GetActiveCampaign();

                if (campaign.IsCampaignActivate())
                    return campaign;
            }

            foreach (var campaign in campaigns)
            {
                if (campaign == _activeCampaignId)
                    continue;

                if (campaign.IsCampaignActivate())
                    return campaign;
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
            if (camp == _activeCampaignId)
                return;

            if (camp != _activeCampaignId)
                _isMissionsIsOutdated = true;

            _activeCampaignId = camp;

            closeRewardCenterAfterEveryMission =
                camp.serverSettings.GetBoolParam("RewardCenter.close_after_mission_completion",
                    closeRewardCenterAfterEveryMission);

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

        /// <summary>
        /// Single update for reward and claim
        /// </summary>
        internal async Task ClaimReward(ServerCampaign campaign, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            //var challenge = _campaigns[challengeId];

            try
            {
                //await Task.Delay(15000,ct);

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