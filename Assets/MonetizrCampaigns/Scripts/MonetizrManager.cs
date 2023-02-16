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
            { ErrorType.NotinitializedSDK, "You're trying to use Monetizer SDK before it's been initialized. Call MonetizerManager.Initalize first." },
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

    /*internal class ServerCampaignWithAssets
    {
        private static readonly Dictionary<AssetsType, System.Type> AssetsSystemTypes = new Dictionary<AssetsType, System.Type>()
        {
            { AssetsType.BrandLogoSprite, typeof(Sprite) },
            { AssetsType.BrandBannerSprite, typeof(Sprite) },
            { AssetsType.BrandRewardLogoSprite, typeof(Sprite) },
            { AssetsType.BrandRewardBannerSprite, typeof(Sprite) },
            { AssetsType.SurveyURLString, typeof(String) },
            //{ AssetsType.VideoURLString, typeof(String) },
            { AssetsType.VideoFilePathString, typeof(String) },
            { AssetsType.BrandTitleString, typeof(String) },
            { AssetsType.TinyTeaserTexture, typeof(Texture2D) },
            { AssetsType.TinyTeaserSprite, typeof(Sprite) },
            //{ AssetsType.Html5ZipURLString, typeof(String) },
            { AssetsType.Html5PathString, typeof(String) },
            { AssetsType.HeaderTextColor, typeof(Color) },
            { AssetsType.CampaignTextColor, typeof(Color) },
            { AssetsType.CampaignHeaderTextColor, typeof(Color) },
            { AssetsType.TiledBackgroundSprite, typeof(Sprite) },
            { AssetsType.CampaignBackgroundColor, typeof(Color) },
            { AssetsType.CustomCoinSprite, typeof(Sprite) },
            { AssetsType.CustomCoinString, typeof(String) },
            { AssetsType.LoadingScreenSprite, typeof(Sprite) },
            { AssetsType.TeaserGifPathString, typeof(String) },
            { AssetsType.RewardSprite, typeof(Sprite) },
            { AssetsType.IngameRewardSprite, typeof(Sprite) },
            { AssetsType.UnknownRewardSprite, typeof(Sprite) },
            { AssetsType.MinigameSprite1, typeof(Sprite) },
            { AssetsType.   , typeof(Sprite) },
            { AssetsType.MinigameSprite3, typeof(Sprite) },

        };


        public ServerCampaign campaign { get; private set; }
        private Dictionary<AssetsType, object> assets = new Dictionary<AssetsType, object>();
        private Dictionary<AssetsType, string> assetsUrl = new Dictionary<AssetsType, string>();

        public bool isChallengeLoaded;

        public ServerCampaignWithAssets(ServerCampaign challenge)
        {
            this.campaign = challenge;
            this.isChallengeLoaded = true;
        }

        public void SetAsset<T>(AssetsType t, object asset)
        {
            if (assets.ContainsKey(t))
            {
                Log.PrintWarning($"An item {t} already exist in the campaign {campaign.id}");
                return;
            }

            //Log.Print($"Adding asset {asset} into {t}");

            MonetizrManager.HoldResource(asset);

            assets.Add(t, asset);
        }

        public bool HasAsset(AssetsType t)
        {
            return assets.ContainsKey(t);
        }

        public string GetAssetUrl(AssetsType t)
        {
            return assetsUrl[t];
        }

        public T GetAsset<T>(AssetsType t)
        {
            if (AssetsSystemTypes[t] != typeof(T))
                throw new ArgumentException($"AssetsType {t} and {typeof(T)} do not match!");

            if (!assets.ContainsKey(t))
                //throw new ArgumentException($"Requested asset {t} doesn't exist in challenge!");
                return default(T);

            return (T)Convert.ChangeType(assets[t], typeof(T));
        }

        internal void SetAssetUrl(AssetsType t, string url)
        {
            assetsUrl.Add(t, url);
        }
    }*/

    /// <summary>
    /// Extention to support async/await in the DownloadAssetData
    /// </summary>
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

        //[SerializeField] internal SerializableDictionary<string,string> settings = new SerializableDictionary<string, string>();
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

        /*internal void SetParam(string campaign, string param, string val, bool saveData = true)
        {
            var camp = data.GetCampaign(campaign);

            if (camp == null)
            {
                data.campaigns.Add(new LocalCampaignSettings()
                { apiKey = MonetizrManager.Instance.GetCurrentAPIkey(),
                  sdkVersion = MonetizrManager.SDKVersion,
                  campId = campaign});

                camp = data.campaigns[data.campaigns.Count - 1];
            }

            camp.settings[param] = val;

            if(saveData)
                SaveData();
        }

        internal string GetParam(string campaign, string param)
        {
            var camp = data.GetCampaign(campaign);

            if (camp == null)
                return "";

            if (!camp.settings.ContainsKey(param))
                return "";

            return camp.settings[param]; 
        }*/

        internal void LoadOldAndUpdateNew(Dictionary<String, ServerCampaign> challenges)
        {
            //load old settings
            //сheck if apikey/sdkversion is old
            Load();

            //check if campaign is missing - remove it from data
            data.campaigns.RemoveAll((LocalCampaignSettings c) => !challenges.ContainsKey(c.campId));

            //add empty campaign into settings
            challenges.Values.ToList().ForEach(c => AddCampaign(c));

            SaveData();
        }
    }

    /// <summary>
    /// Main manager for Monetizr
    /// </summary>
    public class MonetizrManager : MonoBehaviour
    {
        public static readonly string SDKVersion = "0.0.19";

        internal static bool keepLocalClaimData;
        internal static bool serverClaimForCampaigns;
        public static bool claimForSkippedCampaigns;

        internal static int maximumCampaignAmount = 1;
        internal static int maximumMissionsAmount = 1;

        internal static bool isVastActive = false;
               

        //position relative to center with 1080x1920 screen resolution
        private static Vector2? tinyTeaserPosition = null;

        private static Transform teaserRoot;

        internal MonetizrClient _challengesClient { get; private set; }

        private static MonetizrManager instance = null;

        public List<MissionDescription> sponsoredMissions { get; private set; }

        private UIController uiController = null;

        private string activeChallengeId = null;

        private Action<bool> soundSwitch = null;
        private Action<bool> onRequestComplete = null;
        internal Action<bool> onUIVisible = null;

        private bool isActive = false;
        private bool isMissionsIsOudated = true;

        //Storing ids in separate list to get faster access (the same as Keys in challenges dictionary below)
        private List<string> campaignIds = new List<string>();
        private Dictionary<String, ServerCampaign> campaigns = new Dictionary<String, ServerCampaign>();
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

        static internal void _CallUserDefinedEvent(string campaignId, string placement, EventType eventType)
        {
            try
            {
                instance?.userDefinedEvent?.Invoke(campaignId, placement, eventType);
            }
            catch (Exception ex)
            {
                Log.Print($"Exception in userDefinedEvent {ex.ToString()}");
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
            internal Func<ulong> GetCurrencyFunc;
            internal Action<ulong> AddCurrencyAction;
            internal ulong maximumAmount;
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

        public static void SetGameCoinAsset(RewardType rt, Sprite defaultRewardIcon, string title, Func<ulong> GetCurrencyFunc, Action<ulong> AddCurrencyAction, ulong maxAmount)
        {
            GameReward gr = new GameReward()
            {
                icon = defaultRewardIcon,
                title = title,
                GetCurrencyFunc = GetCurrencyFunc,
                AddCurrencyAction = AddCurrencyAction,
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

                Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

                MonetizrManager.instance.missionsManager.UpdateMissionsRewards(rt, reward);
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

            Log.Print($"MonetizrManager SetAdvertisingIds: { MonetizrAnalytics.advertisingID} {MonetizrAnalytics.limitAdvertising}");
        }

        public static MonetizrManager Initialize(string apiKey,
            List<MissionDescription> sponsoredMissions = null,
            Action onRequestComplete = null,
            Action<bool> soundSwitch = null,
            Action<bool> onUIVisible = null,
            UserDefinedEvent userEvent = null)
        {
            if (instance != null)
            {
                //instance.RequestChallenges(onRequestComplete);
                return instance;
            }

            if (sponsoredMissions == null)
            {
                sponsoredMissions = new List<MissionDescription>()
                {
                    //new MissionDescription{ missionType = MissionType.VideoReward, reward = 1000, rewardCurrency = RewardType.Coins },
                    //new MissionDescription{ missionType = MissionType.MutiplyReward, reward = 500, rewardCurrency = RewardType.Coins },
                    //new MissionDescription{ mission = MissionType.MutiplyReward, reward = 1000, rewardCurrency = RewardType.Coins },
                    //new MissionDescription{ mission = MissionType.TwitterReward, reward = 1000, rewardCurrency = RewardType.Coins },
                    //new MissionDescription{ missionType = MissionType.VideoWithEmailGiveaway, reward = 20, rewardCurrency = RewardType.Coins },
                    //new MissionDescription(200, RewardType.Coins),
                };
            }

            //if (sponsoredMissions.Count > 1)
            //    sponsoredMissions = sponsoredMissions.GetRange(0, 1);

#if UNITY_EDITOR
            keepLocalClaimData = true;
            serverClaimForCampaigns = false;
            claimForSkippedCampaigns = true;
#else
            keepLocalClaimData = true;
            serverClaimForCampaigns = true;
            claimForSkippedCampaigns = false;
#endif
            if(soundSwitch == null)
            {
                soundSwitch = (bool isOn) =>
                {
                    Log.Print($"Audio listener pause state {!isOn}");
                    AudioListener.pause = !isOn;
                };
            }

            Log.Print($"MonetizrManager Initialize: {apiKey} {bundleId} {SDKVersion}");

            if(!MonetizrAnalytics.isAdvertisingIDDefined)
            {
                Log.PrintError($"MonetizrManager Initialize: Advertising ID is not defined");
                return null;
            }

            if(bundleId == null)
                bundleId = Application.identifier;

            var monetizrObject = new GameObject("MonetizrManager");
            var monetizrManager = monetizrObject.AddComponent<MonetizrManager>();

            DontDestroyOnLoad(monetizrObject);
            instance = monetizrManager;
            instance.sponsoredMissions = sponsoredMissions;
            instance.userDefinedEvent = userEvent;
            instance.onUIVisible = onUIVisible;

            monetizrManager.Initialize(apiKey, onRequestComplete, soundSwitch);



            return instance;
        }

        public static MonetizrManager Instance
        {
            get
            {
                return instance;
            }
        }

        internal static MonetizrAnalytics Analytics
        {
            get
            {
                return instance._challengesClient.analytics;
            }
        }

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

            localSettings = new LocalSettingsManager();

            missionsManager = new MissionsManager();

            //missionsManager.CleanUp();

            this.soundSwitch = soundSwitch;

            _challengesClient = new MonetizrClient(apiKey);

            InitializeUI();

            onRequestComplete = (bool isOk) =>
            {

                if (!isOk)
                {
                    Log.Print("ERROR: Request complete is not okay!");
                    return;
                }

                if(MonetizrManager.gameRewards.Count == 0)
                {
                    Log.PrintError($"ERROR: No in-game rewards defined. Don't forget to call MonetizrManager.SetGameCoinAsset after SDK initialization.");
                    return;
                }

                Log.Print("MonetizrManager initialization okay!");

                isActive = true;

                //moved together with showing teaser, because here in-game logic may not be ready
                //                createEmbedMissions();

                gameOnInitSuccess?.Invoke();

                if (tinyTeaserCanBeVisible)
                {
                    //isMissionsIsOudated = true;
                    //ShowTinyMenuTeaser(null);
                    OnMainMenuShow(false);
                }

            };

            RequestChallenges(onRequestComplete);
        }

        //TODO: add defines

        public void initializeBuiltinMissions()
        {
            if (isMissionsIsOudated)
                missionsManager.AddMissionsToCampaigns();

            isMissionsIsOudated = false;
            //RegisterSponsoredMission(RewardType.Coins, 1000);

            //RegisterSponsoredMission2(RewardType.Coins, 500);

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
            return _challengesClient.currentApiKey;
        }

        internal void RestartClient()
        {
            _challengesClient.Close();

            _challengesClient = new MonetizrClient(_challengesClient.currentApiKey);

            RequestCampaigns();
        }

        internal bool ChangeAPIKey(string apiKey)
        {
            if (apiKey == _challengesClient.currentApiKey)
                return true;

            Log.Print($"Changing api key to {apiKey}");

            _challengesClient.Close();

            _challengesClient = new MonetizrClient(apiKey);

            RequestCampaigns();

            return false;
        }

        internal void RequestCampaigns(bool callRequestComplete = true)
        {
            isActive = false;

            uiController.DestroyTinyMenuTeaser();

            missionsManager.CleanUp();

            campaigns.Clear();
            campaignIds.Clear();

            RequestChallenges(callRequestComplete ? onRequestComplete : null);
        }

        public void SoundSwitch(bool on)
        {
            soundSwitch?.Invoke(on);
        }

        private void InitializeUI()
        {
            uiController = new UIController();
        }

        private static void FillInfo(Mission m)
        {
            var ch = m.campaignId;//MonetizrManager.Instance.GetActiveChallenge();

            /*if (!MonetizrManager.Instance.HasCampaign(ch))
            {
                m.brandBanner = MonetizrManager.Instance.LoadSpriteFromCache(m.campaignId, m.brandBannerUrl);
                m.brandLogo = MonetizrManager.Instance.LoadSpriteFromCache(m.campaignId, m.brandLogoUrl);
                m.brandRewardBanner = MonetizrManager.Instance.LoadSpriteFromCache(m.campaignId, m.brandRewardBannerUrl);
                return;
            }

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandLogoSprite);
            m.brandName = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandRewardBannerSprite);*/
        }

        internal static void ShowMessage(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrMessagePanel2",
                panelId,
                onComplete,
                true,
                m);
        }

        internal static void ShowNotification(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrNotifyPanel2",
                panelId,
                onComplete,
                true,
                m);
        }

        internal static void ShowEnterEmailPanel(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            instance.uiController.ShowPanelFromPrefab("MonetizrEnterEmailPanel2",
                panelId,
                onComplete,
                true,
                m);
        }

        internal static async void ResetCampaign()
        {
            Mission m = instance.missionsManager.GetFirstUnactiveMission();

            if (m == null)
            {
                Log.Print($"Nothing to reset in ResetCampaign");
                return;
            }

            string campaignId = m.campaignId;

            //show screen to block
            var lscreen = instance.uiController.ShowLoadingScreen();

            lscreen.onComplete = (bool _) => { GameObject.Destroy(lscreen); };

            CancellationTokenSource s_cts = new CancellationTokenSource();

            try
            {
                s_cts.CancelAfter(10000);

                await instance._challengesClient.Reset(campaignId, s_cts.Token);
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

        internal static async void WaitForEndRequestAndNotify(Action<bool> onComplete, Mission m)
        {
            //show screen to block
            var lscreen = instance.uiController.ShowLoadingScreen();

            lscreen.onComplete = (bool _) => { GameObject.Destroy(lscreen); };

            Action onSuccess = () =>
            {
                Log.Print("SUCCESS!");

                //MonetizrManager.Analytics.TrackEvent("Enter email succeeded", m);

                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.ButtonPressOk);

                ShowCongratsNotification((bool _) =>
                {
                    //lscreen.SetActive(false);



                    m.state = MissionUIState.ToBeHidden;

                    m.isClaimed = ClaimState.Claimed;

                    instance.ClaimMissionData(m);

                    //instance.missionsManager.TryToActivateSurvey(m);


                    MonetizrManager.HideTinyMenuTeaser(true);


                    onComplete?.Invoke(false);
                },
                m);
            };

            Action onFail = () =>
            {
                Log.Print("FAIL!");

                //MonetizrManager.Analytics.TrackEvent("Email enter failed", m);

                //MonetizrManager.Analytics.TrackEvent(m, AdPlacement.EmailEnterInGameRewardScreen, MonetizrManager.EventType.Error);

                MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.Error);


                ShowMessage((bool _) =>
                {
                    onComplete?.Invoke(false);
                },
                m,
                PanelId.BadEmailMessageNotification);


            };


            if (serverClaimForCampaigns)
            {

                CancellationTokenSource s_cts = new CancellationTokenSource();

                try
                {
                    s_cts.CancelAfter(10000);

                    await instance.ClaimReward(m.campaignId, s_cts.Token, onSuccess, onFail);
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
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            debugAttempt++;

#if !UNITY_EDITOR
            if (debugAttempt != 10)
                return;
#endif

            debugAttempt = 0;

            instance.uiController.ShowPanelFromPrefab("MonetizrDebugPanel", PanelId.DebugPanel);
        }

        public static void ShowStartupNotification(int placement, Action<bool> onComplete)
        {
            if (instance.uiController.panels.ContainsKey(PanelId.StartNotification))
            {
                Log.Print($"ShowStartupNotification ContainsKey(PanelId.StartNotification) {placement}");
                return;
            }

            bool forceSkip = false;

            //Log.Print($"------ShowStartupNotification 1 {placement}");

            if (instance == null || !instance.HasCampaignsAndActive())
            {
                onComplete?.Invoke(true);
                return;
            }

            //Log.Print($"------ShowStartupNotification 2 {placement}");

            //Log.PrintWarning("ShowStartupNotification");

            //Mission sponsoredMsns = instance.missionsManager.missions.Find((Mission item) => { return item.isSponsored; });
            var missions = instance.missionsManager.GetMissionsForRewardCenter();

            if (missions == null || missions?.Count == 0)
            {
                onComplete?.Invoke(true);
                return;
            }

            Mission mission = missions[0];

            //manual notification calls, no limis
            if (placement == 2)
            {
                FillInfo(mission);
                ShowNotification(onComplete, mission, PanelId.StartNotification);
                return;
            }


            if (placement == 0)
            {
                forceSkip = mission.campaignServerSettings.GetParam("no_start_level_notifications") == "true";

                if (forceSkip)
                    Log.Print($"No notifications on level start defined on serverside");
            }
            else if (placement == 1)
            {
                forceSkip = mission.campaignServerSettings.GetParam("no_main_menu_notifications") == "true";

                if (forceSkip)
                    Log.Print($"No notifications in main menu defined on serverside");
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

            if (mission.amountOfNotificationsSkipped <= mission.campaignServerSettings.GetIntParam("amount_of_skipped_notifications"))
            {
                Log.Print($"Amount of skipped notifications less then {mission.amountOfNotificationsSkipped}");
                forceSkip = true;
            }

            //check if need to limit notifications amount
            var serverMaxAmount = mission.campaignServerSettings.GetIntParam("amount_of_notifications");
            var currentAmount = instance.localSettings.GetSetting(mission.campaignId).amountNotificationsShown;
            if (currentAmount > serverMaxAmount)
            {
                Log.Print($"Startup notification impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                forceSkip = true;
            }

            //check last time
            var lastTimeShow = instance.localSettings.GetSetting(mission.campaignId).lastTimeShowNotification;
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

            instance.localSettings.GetSetting(mission.campaignId).lastTimeShowNotification = DateTime.Now;
            instance.localSettings.GetSetting(mission.campaignId).amountNotificationsShown++;

            instance.localSettings.SaveData();

            //instance.missionsManager.SaveAll();

            //Log.Print($"------ShowStartupNotification 4 {placement}");

            //Log.PrintWarning("!!!!-------");

            Log.Print($"Notification shown {currentAmount}/{serverMaxAmount} last time: {lastTime}/{serverDelay}");

            FillInfo(mission);

            ShowNotification(onComplete, mission, PanelId.StartNotification);
        }

        internal static void ShowCongratsNotification(Action<bool> onComplete, Mission m)
        {
            ShowNotification(onComplete, m, PanelId.CongratsNotification);
        }

        internal static bool TryShowSurveyNotification(Action onComplete)
        {
            //MissionUIDescription sponsoredMsns = instance.missionsManager.getCampaignReadyForSurvey();

            Mission sponsoredMsns = instance.missionsManager.FindActiveSurveyMission();

            if (sponsoredMsns == null)
            {
                onComplete?.Invoke();
                return false;
            }

            FillInfo(sponsoredMsns);

            Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
                if (MonetizrManager.claimForSkippedCampaigns)
                    isSkipped = false;

                if (!isSkipped)
                {
                    //sponsoredMsns.AddPremiumCurrencyAction.Invoke(sponsoredMsns.reward);

                    //MonetizrManager.gameRewards[sponsoredMsns.rewardType].AddCurrencyAction(sponsoredMsns.reward);

                    //ShowCongratsNotification(onComplete, sponsoredMsns);

                    Instance.ClaimMission(sponsoredMsns, isSkipped, true, onComplete);
                }
                else
                {
                    onComplete?.Invoke();
                }


            };

            ShowNotification((bool _) => { ShowSurvey(onSurveyComplete, sponsoredMsns); },
                sponsoredMsns,
                PanelId.SurveyNotification);

            return true;
        }

        internal void ClaimMissionData(Mission m)
        {
            gameRewards[m.rewardType].AddCurrencyAction(m.reward);

            /*if (m.type == MissionType.VideoReward)
            {
                ShowRewardCenter(null);
                //m.AddPremiumCurrencyAction.Invoke(m.reward);

                gameRewards[m.rewardType].AddCurrencyAction(m.reward);
            }
            else if (m.type == MissionType.MutiplyReward)
            {
                m.reward *= 2;

                //m.AddNormalCurrencyAction.Invoke(m.reward);

                gameRewards[m.rewardType].AddCurrencyAction(m.reward);
            }
            if (m.type == MissionType.VideoWithEmailGiveaway)
            {
                //ShowRewardCenter(null);
                //m.AddPremiumCurrencyAction.Invoke(m.reward);

                gameRewards[m.rewardType].AddCurrencyAction(m.reward);
            }
            else if (m.type == MissionType.SurveyReward)
            {
                gameRewards[m.rewardType].AddCurrencyAction(m.reward);

                //ShowRewardCenter(null);
            }
            else if (m.type == MissionType.TwitterReward)
            {
                gameRewards[m.rewardType].AddCurrencyAction(m.reward);

                //ShowRewardCenter(null);
            }*/

            if (keepLocalClaimData)
                Instance.SaveClaimedReward(m);
        }

        internal void ClaimMission(Mission m, bool isSkipped, bool showCongratsScreen, Action onComplete)
        {
            if (claimForSkippedCampaigns)
                isSkipped = false;

            if (isSkipped)
                return;


            //m.isDisabled = true;

            ClaimMissionData(m);

            /*if (Instance.missionsManager.TryToActivateSurvey(m))
            {
                //UpdateUI();
            }*/

            if (!showCongratsScreen)
            {
                onComplete?.Invoke();
                return;
            }

            ShowCongratsNotification((bool _) =>
            {

                onComplete?.Invoke();

            }, m);



        }


        public static void RegisterUserDefinedMission(string missionTitle, string missionDescription, Sprite missionIcon, RewardType rt, ulong reward, float progress, Action onClaimButtonPress)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            Mission m = new Mission()
            {
                missionTitle = missionTitle,
                missionDescription = missionDescription,
                //missionIcon = missionIcon,
                rewardType = rt,
                reward = reward,
                progress = progress,
                isSponsored = false,
                onClaimButtonPress = onClaimButtonPress,
                //brandBanner = null,
            };

            instance.missionsManager.AddMission(m);
        }

        //TODO: need to connect now this mission and campaign from the server
        //next time once we register the mission it should connect with the same campaign
        /*public static void RegisterSponsoredMission(RewardType rt, ulong rewardAmount)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            Mission m = new Mission()
            {
                //sponsoredId = id,
                rewardType = rt,
                type = MissionType.VideoReward,
                //rewardIcon = rewardIcon,
                reward = rewardAmount,
                isSponsored = true,
                // AddPremiumCurrencyAction = AddPremiumCurrencyAction,
                //rewardTitle = rewardTitle,
            };

            //
            instance.missionsManager.AddMissionAndBindToCampaign(m);
        }*/

        /// <summary>
        /// You need to earn goal amount of money to double it
        /// </summary>
        /// <param name="rewardIcon">Coins icon</param>
        /// <param name="goalAmount">How much you need to earn</param>
        /// <param name="rewardTitle">Coins</param>
        /// <param name="GetNormalCurrencyFunc">Get coins func</param>
        /// <param name="AddNormalCurrencyAction">Add coins to user account</param>
        /*public static void RegisterSponsoredMission2(RewardType rt, ulong goalAmount)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            Mission m = new Mission()
            {
                //sponsoredId = id,
                rewardType = rt,
                startMoney = gameRewards[rt].GetCurrencyFunc(),
                type = MissionType.MutiplyReward,
                //rewardIcon = rewardIcon,
                reward = goalAmount,
                isSponsored = true,
                //AddNormalCurrencyAction = AddNormalCurrencyAction,
                //GetNormalCurrencyFunc = GetNormalCurrencyFunc,
                //rewardTitle = rewardTitle,
            };

            //
            instance.missionsManager.AddMissionAndBindToCampaign(m);
        }*/


        internal static void CleanUserDefinedMissions()
        {
            instance.missionsManager.CleanUserDefinedMissions();
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
            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter();

            if (missions != null && missions.Count > 0)
            {
                //no more offers, skipping
                if (missions[0].amountOfRVOffersShown == 0)
                {
                    onComplete(OnCompleteStatus.Skipped);
                }

                missions[0].amountOfRVOffersShown--;
            }

            MonetizrManager.ShowRewardCenter(null, (bool p) => { onComplete(p ? OnCompleteStatus.Skipped : OnCompleteStatus.Completed); });
        }


        public static void ShowRewardCenter(Action UpdateGameUI, Action<bool> onComplete = null)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            UpdateGameUI?.Invoke();

            var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            var m = instance.missionsManager.GetMission(challengeId);

            //no missions, consider as a skipped
            if (m == null)
            {
                onComplete?.Invoke(true);
                return;
            }

            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter();

            /*int i = 1;
            foreach (var m2 in Instance.missionsManager.missions)
            {
                Log.Print($"{i}:{m2.missionTitle}:{m2.campaignId}");
                i++;
            }*/

            if (missions.Count == 0)
            {
                onComplete?.Invoke(true);
                return;
            }

            bool showRewardCenterForOneMission = missions[0].campaignServerSettings.GetBoolParam("RewardCenter.show_for_one_mission", false);

            if (missions.Count == 1 && !showRewardCenterForOneMission)
            //if (Instance.missionsManager.missions.Count == 1)
            {
                //Log.Print($"---_PressSingleMission");

                Log.Print($"Only one mission available and showRewardCenterForOneMission is false");

                Instance._PressSingleMission(onComplete, m);
                return;
            }



            Log.Print($"ShowRewardCenter with {m?.campaignId}");

            string uiItemPrefab = "MonetizrRewardCenterPanel2";

            instance.uiController.ShowPanelFromPrefab(uiItemPrefab, PanelId.RewardCenter, onComplete, true, m);
        }

        internal static void HideRewardCenter()
        {
            instance.uiController.HidePanel(PanelId.RewardCenter);
        }

        internal void _PressSingleMission(Action<bool> onComplete, Mission m)
        {
            if (m.isClaimed == ClaimState.Claimed)
                return;

            //MonetizrManager.Instance.missionsManager.GetEmailGiveawayClaimAction(m, onComplete, null).Invoke();

            MonetizrManager.Instance.missionsManager.ClaimAction(m, onComplete, null).Invoke();
        }

        internal static void ShowMinigame(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.isActive)
                return;

            var panelNames = new Dictionary<MissionType, Tuple<PanelId, string>>()
            {
                {MissionType.MinigameReward, new Tuple<PanelId, string>(PanelId.CarMemoryGame,"MonetizrCarGamePanel")},
                {MissionType.MemoryMinigameReward, new Tuple<PanelId, string>(PanelId.MemoryGame,"MonetizrGamePanel")},
            };

            instance.uiController.ShowPanelFromPrefab(panelNames[m.type].Item2, panelNames[m.type].Item1, onComplete, false, m);
        }

        internal static void ShowUnitySurvey(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.isActive)
                return;

            instance.uiController.ShowPanelFromPrefab("MonetizrUnitySurveyPanel", PanelId.SurveyUnityView, onComplete, false, m);
        }


        internal static void _ShowWebView(Action<bool> onComplete, PanelId id, Mission m = null)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.isActive)
                return;

            instance.uiController.ShowPanelFromPrefab("MonetizrWebViewPanel2", id, onComplete, false, m);
        }

        internal static void GoToLink(Action<bool> onComplete, Mission m = null)
        {
            Application.OpenURL(m.surveyUrl);
            onComplete.Invoke(false);
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

        public static void OnStartGameLevel(Action onComplete)
        {
            onComplete?.Invoke();

            /*if (instance == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (!TryShowSurveyNotification(onComplete))
            {
                //if no survey, show notification



                ShowStartupNotification(0, (bool isSkipped) =>
                        {
                            if (isSkipped)
                                onComplete?.Invoke();
                            else
                                ShowRewardCenter(null, (bool b) => { onComplete?.Invoke(); });

                        });

            }*/
        }

        public static void OnNextLevel(Action<bool> onComplete)
        {
            //ShowStartupNotification((bool _) => { ShowRewardCenter(null, onComplete); });
        }

        /// <summary>
        /// Call this method to show Notification and if player close it teaser will be shown
        /// </summary>
        /// <param name="showNotifications"></param>
        public static void OnMainMenuShow(bool showNotifications = true)
        {
            tinyTeaserCanBeVisible = true;

            if (instance == null)
                return;


            if (!Instance.HasCampaignsAndActive())
                return;

            instance.initializeBuiltinMissions();

            if (showNotifications)
            {
                ShowStartupNotification(1, (bool isSkipped) =>
                        {
                            if (isSkipped)
                                ShowTinyMenuTeaser();
                            else
                                ShowRewardCenter(null, (bool _) => { ShowTinyMenuTeaser(); });
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
            if (instance == null)
                return;

            //tinyTeaserCanBeVisible = true;

            if (!Instance.HasCampaignsAndActive())
            {
                onComplete?.Invoke(OnCompleteStatus.Skipped);
                return;
            }

            instance.initializeBuiltinMissions();

            //Notification is shown
            ShowStartupNotification(2, (bool isSkipped) =>
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
            HideTinyMenuTeaser();

            //ShowStartupNotification((bool _) => {  });
        }

        public static void ShowTinyMenuTeaser(Action UpdateGameUI = null)
        {
            //Log.Print("ShowTinyMenuTeaser");

            //Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (instance == null)
                return;

            tinyTeaserCanBeVisible = true;

            //has some challanges
            if (!instance.HasCampaignsAndActive())
            {
                Log.Print($"No active campaigns for teaser");
                return;
            }

            //has some active missions
            if (instance.missionsManager.GetActiveMissionsNum() == 0)
            {
                Log.Print($"No active missions for teaser");
                return;
            }

            var challengeId = MonetizrManager.Instance.GetActiveCampaign();
            if (!instance.HasAsset(challengeId, AssetsType.TinyTeaserSprite) &&
                !instance.HasAsset(challengeId, AssetsType.TeaserGifPathString))
            {
                Log.Print("No texture for tiny teaser!");
                return;
            }

            var campaign = MonetizrManager.Instance.GetCampaign(challengeId);

            if (campaign.serverSettings.GetParam("hide_teaser_button") == "true")
                return;

            var serverMaxAmount = campaign.serverSettings.GetIntParam("amount_of_teasers");
            var currentAmount = instance.localSettings.GetSetting(campaign.id).amountTeasersShown;
            if (currentAmount > serverMaxAmount)
            {
                Log.Print($"Teaser impressions reached maximum limit {currentAmount}/{serverMaxAmount}");
                return;
            }

            Log.Print($"Teaser shown {currentAmount}/{serverMaxAmount}");

            instance.localSettings.GetSetting(campaign.id).amountTeasersShown++;
            instance.localSettings.SaveData();

            int uiVersion = campaign.serverSettings.GetIntParam("teaser_design_version", 2);

            instance.uiController.ShowTinyMenuTeaser(teaserRoot, tinyTeaserPosition, UpdateGameUI, uiVersion, campaign);

        }

        public static void HideTinyMenuTeaser(bool checkIfSomeMissionsAvailable = false)
        {
            //Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (instance == null)
                return;

            if (checkIfSomeMissionsAvailable && instance.missionsManager.GetActiveMissionsNum() > 0)
                return;

            tinyTeaserCanBeVisible = false;

            if (!instance.isActive)
                return;

            // MonetizrManager.Analytics.EndShowAdAsset(AdPlacement.TinyTeaser);

            //MonetizrManager.Analytics.TrackEvent(null, null, EventType.ImpressionEnds);

            instance.uiController.HidePanel(PanelId.TinyMenuTeaser);
        }

        internal void OnClaimRewardComplete(Mission mission, bool isSkipped, Action<bool> onComplete, Action updateUIDelegate)
        {
            if (claimForSkippedCampaigns)
                isSkipped = false;

            if (isSkipped)
                return;

            ShowCongratsNotification((bool _) =>
            {
                bool updateUI = false;


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
                        ClaimReward(mission.campaignId, CancellationToken.None, () =>
                        {
                            RequestCampaigns(false);


                        });

                    }
                }

                MonetizrManager.HideTinyMenuTeaser(true);

                onComplete?.Invoke(isSkipped);

                if (!updateUI)
                    return;

                updateUIDelegate?.Invoke();


            }, mission);

        }

        //TODO: shouldn't have possibility to show video directly by game
        /*internal static void _PlayVideo(string videoPath, Action<bool> onComplete)
        {
            instance.uiController.PlayVideo(videoPath, onComplete);
        }*/


     

        /// <summary>
        /// Request challenges from the server
        /// </summary>
        public async void RequestChallenges(Action<bool> onRequestComplete)
        {
            List<ServerCampaign> campaigns = new List<ServerCampaign>();

            try
            {
                campaigns = await _challengesClient.GetList();
            }
            catch (Exception e)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]} {e}");
                onRequestComplete?.Invoke(false);
            }

            if (campaigns == null)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]}");
                onRequestComplete?.Invoke(false);
            }

            campaignIds.Clear();

            if (campaigns.Count > 0)
            {
                _challengesClient.InitializeMixpanel(campaigns[0].testmode, campaigns[0].panel_key);

                _challengesClient.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.Impression);


                //_challengesClient.analytics.TrackEvent("Get List Started", campaigns[0]);
                //_challengesClient.analytics.StartTimedEvent("Get List Finished");
            }
            else
            {
                _challengesClient.InitializeMixpanel(false, null);
            }



#if TEST_SLOW_LATENCY
            await Task.Delay(10000);
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif
            //Color c;

            foreach (var campaign in campaigns)
            {
                if (this.campaigns.ContainsKey(campaign.id))
                    continue;

                string path = Application.persistentDataPath + "/" + campaign.id;

                Log.Print($"Campaign path: {path}");

                await campaign.LoadCampaignAssets();               

                Log.Print($"Loading finished {campaign.isLoaded}");

                if (campaign.isLoaded)
                {
                    this.campaigns.Add(campaign.id, campaign);
                    campaignIds.Add(campaign.id);
                }
            }

            activeChallengeId = campaignIds.Count > 0 ? campaignIds[0] : null;

            Log.Print($"Active challenge {activeChallengeId}");

            isMissionsIsOudated = true;

#if TEST_SLOW_LATENCY
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif
            localSettings.LoadOldAndUpdateNew(this.campaigns);

            Log.Print($"RequestChallenges completed with count: {campaignIds.Count} active: {activeChallengeId}");

            if (activeChallengeId != null)
            {
                //_challengesClient.analytics.TrackEvent("Get List Finished", activeChallengeId, true);

                _challengesClient.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.ImpressionEnds);
            }
            else
            {
                if (campaigns.Count > 0)
                {
                    //_challengesClient.analytics.TrackEvent("Get List Load Failed", campaigns[0]);

                    _challengesClient.analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoading, EventType.Error);
                }
            }

            //Ok, even if response empty
            onRequestComplete?.Invoke(/*challengesId.Count > 0*/true);
        }

        /// <summary>
        /// Get Challenge by Id
        /// </summary>
        /// <returns></returns>
        internal ServerCampaign GetCampaign(String chId)
        {
            if (string.IsNullOrEmpty(chId))
                return null;

            if (!campaigns.ContainsKey(chId))
            {
                Log.PrintWarning($"You're trying to get campaign {chId} which is not exist!");
                return null;
            }

            return campaigns[chId];
        }

        /// <summary>
        /// Get list of the available challenges
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableCampaigns()
        {
            return campaignIds;
        }

        public bool HasCampaignsAndActive()
        {
            return isActive && campaignIds.Count > 0;
        }

        public static bool IsActiveAndEnabled()
        {
            if (instance == null)
                return false;

            return instance.HasCampaignsAndActive();
        }

        public string GetActiveCampaign()
        {
            return activeChallengeId;
        }

        public void SetActiveCampaignId(string id)
        {
            activeChallengeId = id;
        }

        public void Enable(bool enable)
        {
            isActive = enable;
        }

        /// <summary>
        /// Get Asset from the challenge
        /// </summary>
        public T GetAsset<T>(String challengeId, AssetsType t)
        {
            if (challengeId == null)
            {
                Log.Print($"You requesting asset for empty challenge.");
                return default(T);
            }

            if (!campaigns.ContainsKey(challengeId))
            {
                Log.Print($"You requesting asset for challenge {challengeId} that not exist!");
                return default(T);
            }

            if (!HasAsset(challengeId, t))
            {
                //Log.Print($"{challengeId} has no asset {t}");
                return default(T);
            }

            return campaigns[challengeId].GetAsset<T>(t);
        }

        /*public string GetAssetUrl(String challengeId, AssetsType t)
        {
            return campaigns[challengeId].GetAssetUrl(t);
        }*/

        public bool HasCampaign(String challengeId)
        {
            return campaigns.ContainsKey(challengeId);
        }

        public bool HasAsset(String challengeId, AssetsType t)
        {
            if (campaigns == null)
                return false;

            if (!campaigns.ContainsKey(challengeId))
                return false;

            return campaigns[challengeId].HasAsset(t);
        }

        /// <summary>
        /// Single update for reward and claim
        /// </summary>
        public async Task ClaimReward(String challengeId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            var challenge = campaigns[challengeId];

            try
            {
                //await Task.Delay(15000,ct);

                await _challengesClient.Claim(challenge, ct, onSuccess, onFailure);
            }
            catch (Exception e)
            {
                Log.Print($"An error occured: {e.Message}");

                onFailure.Invoke();
            }
        }

       
    }

}