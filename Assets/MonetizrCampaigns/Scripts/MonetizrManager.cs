//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;
using UnityEngine.Networking;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Assertions;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Linq;
using mixpanel;


namespace Monetizr.Campaigns
{
    internal enum ErrorType
    {
        NotinitializedSDK,
        SimultaneusAdAssets,
        AdAssetStillShowing,
        ConnectionError,
    };

    internal static class MonetizrErrors
    {
        public static readonly Dictionary<ErrorType, string> msg = new Dictionary<ErrorType, string>()
        {
            { ErrorType.NotinitializedSDK, "You're trying to use Monetizr SDK before it's been initialized. Call MonetizerManager.Initalize first." },
            { ErrorType.SimultaneusAdAssets, "Simultaneous display of multiple ads is not supported!" },
            { ErrorType.AdAssetStillShowing, "Some ad asset are still showing." },
            { ErrorType.ConnectionError, "Connection error while getting list of campaigns!" }
        };
    }

    /// <summary>
    /// Predefined asset types for easier access
    /// </summary>
    public enum AssetsType
    {
        Unknown,
        BrandLogoSprite, //icon
        BrandBannerSprite, //banner
        BrandRewardLogoSprite, //logo
        BrandRewardBannerSprite, //reward_banner
        SurveyURLString, //survey
        //VideoURLString, //video url
        VideoFilePathString, //video url
        BrandTitleString, //text
        TinyTeaserTexture, //text
        TinyTeaserSprite,
        //Html5ZipURLString,
        Html5PathString,
        TiledBackgroundSprite,
        //CampaignHeaderTextColor,
        //CampaignTextColor,
        //HeaderTextColor,
        //CampaignBackgroundColor,
        CustomCoinSprite,
        CustomCoinString,
        LoadingScreenSprite,
        TeaserGifPathString,
        RewardSprite,
        IngameRewardSprite,
        UnknownRewardSprite,

        MinigameSprite1,
        MinigameSprite2,
        MinigameSprite3,
        LeaderboardBannerSprite,

    }

    internal static class ExtensionMethods
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }

    internal class DownloadHelper
    {
        /// <summary>
        /// Downloads any type of asset and returns its data as an array of bytes
        /// </summary>
        public static async Task<byte[]> DownloadAssetData(string url, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            await uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Log.PrintError($"Network error {uwr.error} with {url}");
                onDownloadFailed?.Invoke();
                return null;
            }

            return uwr.downloadHandler.data;
        }
    }

    public enum RewardType
    {
        Undefined = 0,
        Coins = 1,
        Reward1 = 1,
        AdditionalCoins = 2,
        Reward2 = 2,
        PremiumCurrency = 3,
        Reward3 = 3,
    }

    [Serializable]
    internal class LocalCampaignSettings
    {
        [SerializeField] internal string campId;
        [SerializeField] internal string apiKey;
        [SerializeField] internal string sdkVersion;
        [SerializeField] internal UDateTime lastTimeShowNotification;
        [SerializeField] internal int amountNotificationsShown;
        [SerializeField] internal int amountTeasersShown;

        [SerializeField] internal SerializableDictionary<string,string> settings = new SerializableDictionary<string, string>();
    }

    [Serializable]
    internal class CampaignsCollection : BaseCollection
    {
        //campaign id and settings
        [SerializeField]
        internal List<LocalCampaignSettings> campaigns =
           new List<LocalCampaignSettings>();

        internal override void Clear()
        {
            campaigns.Clear();
        }

        internal LocalCampaignSettings GetCampaign(string id)
        {
            return campaigns.Find(c => c.campId == id);
        }
    };

    internal class LocalSettingsManager : LocalSerializer<CampaignsCollection>
    {
        internal LocalSettingsManager()
        {
            data = new CampaignsCollection();
        }

        internal override string GetDataKey()
        {
            return "campaigns";
        }

        internal void Load()
        {
            //ResetData();

            LoadData();

            int deleted = data.campaigns.RemoveAll((LocalCampaignSettings m) =>
                { return m.apiKey != MonetizrManager.Instance.GetCurrentAPIkey(); });

            deleted += data.campaigns.RemoveAll((LocalCampaignSettings m) =>
                { return m.sdkVersion != MonetizrManager.SDKVersion; });

            if (deleted > 0)
            {
                SaveData();
            }
        }

        internal void AddCampaign(ServerCampaign campaign)
        {
            var camp = data.GetCampaign(campaign.id);

            if (camp == null)
            {
                data.campaigns.Add(new LocalCampaignSettings()
                {
                    apiKey = MonetizrManager.Instance.GetCurrentAPIkey(),
                    sdkVersion = MonetizrManager.SDKVersion,
                    lastTimeShowNotification = DateTime.Now,
                    campId = campaign.id
                });
            }
        }

        internal LocalCampaignSettings GetSetting(string campaign)
        {
            var camp = data.GetCampaign(campaign);

            Debug.Assert(camp != null);

            return camp;
        }

        /*internal void LoadOldAndUpdateNew(Dictionary<String, ServerCampaign> campaigns)
        {
            //load old settings
            //сheck if apikey/sdkversion is old
            Load();

            //check if campaign is missing - remove it from data
            data.campaigns.RemoveAll((LocalCampaignSettings c) => !campaigns.ContainsKey(c.campId));

            //add empty campaign into settings
            campaigns.Values.ToList().ForEach(c => AddCampaign(c));

            SaveData();
        }*/

        internal void LoadOldAndUpdateNew(List<ServerCampaign> campaigns)
        {
            //load old settings
            //сheck if apikey/sdkversion is old
            Load();

            //check if campaign is missing - remove it from data
            data.campaigns.RemoveAll((LocalCampaignSettings localCampaigns) => 
                campaigns.FindIndex(serverCampaigns => serverCampaigns.id == localCampaigns.campId) < 0);

            //add empty campaign into settings
            campaigns.ForEach(c => AddCampaign(c));

            SaveData();
        }
    }

    /// <summary>
    /// Main manager for Monetizr
    /// </summary>
    public class MonetizrManager : MonoBehaviour
    {
        public static float requestCampaignTime = 5 * 60;
        public static readonly string SDKVersion = "1.0.4";

        internal static bool keepLocalClaimData;
        internal static bool serverClaimForCampaigns;
        public static bool claimForSkippedCampaigns;

        public static bool closeRewardCenterAfterEveryMission = false;

        internal static int maximumCampaignAmount = 1;

        internal static bool isVastActive = false;


        //position relative to center with 1080x1920 screen resolution
        private static Vector2? tinyTeaserPosition = null;

        private static Transform teaserRoot;

        internal MonetizrClient Client { get; private set; }

        public List<MissionDescription> sponsoredMissions { get; private set; }

        private UIController _uiController = null;

        private ServerCampaign _activeCampaignId = null;

        private Action<bool> _soundSwitch = null;
        private Action<bool> _onRequestComplete = null;
        internal Action<bool> onUIVisible = null;

        private bool _isActive = false;
        private bool _isMissionsIsOutdated = true;

        //Storing ids in separate list to get faster access (the same as Keys in campaigns dictionary below)
        //private List<string> _campaignIds = new List<string>();
        //private Dictionary<string, ServerCampaign> _campaigns = new Dictionary<string, ServerCampaign>();
        
        private List<ServerCampaign> campaigns = new List<ServerCampaign>();

        internal static bool tinyTeaserCanBeVisible;

        internal MissionsManager missionsManager = null;

        internal LocalSettingsManager localSettings = null;

        public enum EventType
        {
            Impression,
            ImpressionEnds,
            ButtonPressSkip,
            ButtonPressOk,
            Error,
            ActionSuccess,
        }

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
                Log.PrintError($"Exception in userDefinedEvent {ex}");
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

        internal class GameReward
        {
            internal Sprite icon;
            internal string title;
            internal Func<ulong> _GetCurrencyFunc;
            internal Action<ulong> _AddCurrencyAction;
            internal ulong maximumAmount;

            internal ulong GetCurrencyFunc()
            {
                try
                {
                    return _GetCurrencyFunc();
                }
                catch (Exception exception)
                {
                    Log.PrintError($"Exception {exception} in getting current amount of {title}");
                    return 0;
                }
            }

            internal void AddCurrencyAction(ulong amount)
            {
                try
                {
                    _AddCurrencyAction(amount);
                }
                catch (Exception exception)
                {
                    Log.PrintError($"Exception {exception} in adding {amount} to {title}");
                }
            }
        }

        public static string temporaryEmail = "";

        internal enum RewardSelectionType
        {
            Product,
            Ingame,
        }

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

        internal static GameReward GetGameReward(RewardType rt)
        {
            if (gameRewards.ContainsKey(rt))
                return gameRewards[rt];

            return null;
        }

        public static void SetAdvertisingIds(string advertisingID, bool limitAdvertising)
        {
            MonetizrAnalytics.isAdvertisingIDDefined = true;

            MonetizrAnalytics.advertisingID = advertisingID;
            MonetizrAnalytics.limitAdvertising = limitAdvertising;

            Log.Print(
                $"MonetizrManager SetAdvertisingIds: {MonetizrAnalytics.advertisingID} {MonetizrAnalytics.limitAdvertising}");
        }

        public static MonetizrManager Initialize(string apiKey,
            List<MissionDescription> sponsoredMissions = null,
            Action onRequestComplete = null,
            Action<bool> soundSwitch = null,
            Action<bool> onUIVisible = null,
            UserDefinedEvent userEvent = null)
        {
            if (Instance != null)
            {
                return Instance;
            }

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

            if (!MonetizrAnalytics.isAdvertisingIDDefined)
            {
                Log.PrintError(
                    $"MonetizrManager Initialize: Advertising ID is not defined. Be sure you called MonetizrManager.SetAdvertisingIds before Initialize call.");
                return null;
            }

            if(string.IsNullOrEmpty(bundleId))
                bundleId = Application.identifier;

            var monetizrObject = new GameObject("MonetizrManager");
            var monetizrManager = monetizrObject.AddComponent<MonetizrManager>();

            DontDestroyOnLoad(monetizrObject);
            Instance = monetizrManager;
            Instance.sponsoredMissions = sponsoredMissions;
            Instance.userDefinedEvent = userEvent;
            Instance.onUIVisible = onUIVisible;

            monetizrManager.Initialize(apiKey, onRequestComplete, soundSwitch);



            return Instance;
        }

        internal static MonetizrManager Instance { get; private set; } = null;

        internal static MonetizrAnalytics Analytics => Instance.Client.analytics;

        void OnApplicationQuit()
        {
            Analytics?.OnApplicationQuit();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize(string apiKey, Action gameOnInitSuccess, Action<bool> soundSwitch)
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

            Client = new MonetizrClient(apiKey);

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
            return Client.currentApiKey;
        }

        internal void RestartClient()
        {
            Client.Close();

            Client = new MonetizrClient(Client.currentApiKey);

            RequestCampaigns();
        }

        internal bool ChangeAPIKey(string apiKey)
        {
            if (apiKey == Client.currentApiKey)
                return true;

            Log.Print($"Changing api key to {apiKey}");

            Client.Close();

            Client = new MonetizrClient(apiKey);

            RequestCampaigns();

            return false;
        }

        internal void RequestCampaigns(bool callRequestComplete = true)
        {
            _isActive = false;

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

                await Instance.Client.Reset(campaignId, s_cts.Token);
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

                //MonetizrManager.Analytics.TrackEvent("Enter email succeeded", m);

                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.ButtonPressOk);

                MonetizrManager.Instance.OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);
            };

            Action onFail = () =>
            {
                Log.Print("FAIL!");

                //MonetizrManager.Analytics.TrackEvent("Email enter failed", m);

                //MonetizrManager.Analytics.TrackEvent(m, AdPlacement.EmailEnterInGameRewardScreen, MonetizrManager.EventType.Error);

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

        public enum NotificationPlacement
        {
            LevelStartNotification = 0,
            MainMenuShowNotification = 1,
            ManualNotification = 2
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

        public enum OnCompleteStatus
        {
            //if player rejected the offer or haven't seen anything
            Skipped,

            //if player completed the offer
            Completed
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
            if (missions[0].amountOfRVOffersShown == 0) { 
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

            // MonetizrManager.Analytics.EndShowAdAsset(AdPlacement.TinyTeaser);

            //MonetizrManager.Analytics.TrackEvent(null, null, EventType.ImpressionEnds);

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
            campaigns = new List<ServerCampaign>();

            try
            {
                campaigns = await Client.GetList();
            }
            catch (Exception e)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]} {e}");

                if (Client.GlobalSettings.GetBoolParam("openrtb.sent_error_report_to_slack", true))
                {
                    Client.SendErrorToRemoteServer("Campaign error",
                        "Campaign error",
                        $"Campaign error: loading error:\nApp: {bundleId}\nApp version: {Application.version}\nSystem language: {Application.systemLanguage}\n\n{e.ToString()}");
                }

                onRequestComplete?.Invoke(false);
            }

            if (campaigns == null)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]}");
                onRequestComplete?.Invoke(false);
            }

            //_campaignIds.Clear();

            if (campaigns.Count > 0)
            {
                Client.InitializeMixpanel(campaigns[0].testmode, campaigns[0].panel_key);

                Client.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.Impression);


                //_client.analytics.TrackEvent("Get List Started", campaigns[0]);
                //_client.analytics.StartTimedEvent("Get List Finished");
            }
            else
            {
                Client.InitializeMixpanel(false, null);
            }



#if TEST_SLOW_LATENCY
            await Task.Delay(10000);
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif
            //Color c;



            foreach (var campaign in campaigns)
            {
                //if (this._campaigns.ContainsKey(campaign.id))
                //    continue;

                string path = Application.persistentDataPath + "/" + campaign.id;

                Log.Print($"Campaign path: {path}");

                await campaign.LoadCampaignAssets();

                if (campaign.isLoaded)
                {
                    Log.Print($"Campaign {campaign.id} successfully loaded");

                    //this._campaigns.Add(campaign.id, campaign);
                    //_campaignIds.Add(campaign.id);
                }
                else
                {
                    Log.PrintError($"Campaign {campaign.id} loading failed with error {campaign.loadingError}!");

                    Client.analytics.TrackEvent(campaign, null,
                        AdPlacement.AssetsLoading,
                        EventType.Error,
                        new Dictionary<string, string> { { "loading_error", campaign.loadingError } });

                    if (Client.GlobalSettings.GetBoolParam("openrtb.sent_error_report_to_slack", true))
                    {
                        Client.SendErrorToRemoteServer("Campaign loading assets error",
                            "Campaign loading assets error",
                            $"Campaign {campaign.id} loading error:\nApp: {bundleId}\nApp version: {Application.version}\nSystem language: {Application.systemLanguage}\n\n{campaign.loadingError}");

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
                //_client.analytics.TrackEvent("Get List Finished", activeChallengeId, true);

                Client.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.ImpressionEnds);
            }
            else
            {
                if (campaigns.Count > 0)
                {
                    //_client.analytics.TrackEvent("Get List Load Failed", campaigns[0]);

                    Client.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.Error);
                }
            }*/

            if (campaigns.Count > 0)
                Client.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.ImpressionEnds);

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

    /// <summary>
    /// Get Challenge by Id
    /// </summary>
    /// <returns></returns>
        /*internal ServerCampaign GetCampaign(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            if (_campaigns.TryGetValue(id, out var campaign))
                return campaign;

            Log.PrintWarning($"You're trying to get campaign {id} which is not exist!");

            return null;
        }*/

        internal ServerCampaign GetActiveCampaign()
        {
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

        /*internal List<string> GetAvailableCampaigns()
        {
            return _campaignIds;
        }*/

        internal bool HasCampaignsAndActive()
        {
            return _isActive && campaigns.Count > 0;
        }

        public static bool IsActiveAndEnabled()
        {
            return Instance != null && Instance.HasCampaignsAndActive();
        }

       /* internal string GetActiveCampaignId()
        {
            return _activeCampaignId;
        }*/

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

        /*internal void SetActiveCampaign(ServerCampaign camp)
        {
            SetActiveCampaignId(camp?.id);
        }*/

        public bool HasActiveCampaign()
        {
            return _isActive && _activeCampaignId != null;
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

                await Client.Claim(campaign, ct, onSuccess, onFailure);
            }
            catch (Exception e)
            {
                Log.Print($"An error occured: {e.Message}");

                onFailure.Invoke();
            }
        }



    }

}