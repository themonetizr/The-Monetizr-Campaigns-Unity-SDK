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
        CampaignHeaderTextColor,
        CampaignTextColor,
        HeaderTextColor,
        CampaignBackgroundColor,
        CustomCoinSprite,
        CustomCoinString,
        LoadingScreenSprite,
        TeaserGifPathString,
        RewardSprite,
        IngameRewardSprite,
        UnknownRewardSprite,
    }

    internal class ServerCampaignWithAssets
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
    }

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
        Coins,
        AdditionalCoins,
        PremiumCurrency

    }

    /// <summary>
    /// Main manager for Monetizr
    /// </summary>
    public class MonetizrManager : MonoBehaviour
    {
        public static readonly string SDKVersion = "0.0.6";

        internal static bool keepLocalClaimData;
        internal static bool serverClaimForCampaigns;
        internal static bool claimForSkippedCampaigns;

        internal static int maximumCampaignAmount = 1;
        internal static int maximumMissionsAmount = 1;


        //position relative to center with 1080x1920 screen resolution
        private static Vector2 tinyTeaserPosition = new Vector2(-430, 600);

        internal ChallengesClient _challengesClient { get; private set; }

        private static MonetizrManager instance = null;

        public List<MissionDescription> sponsoredMissions { get; private set; }

        private UIController uiController = null;

        private string activeChallengeId = null;

        private Action<bool> soundSwitch;
        private Action<bool> onRequestComplete;

        private bool isActive = false;
        private bool isMissionsIsOudated = true;

        //Storing ids in separate list to get faster access (the same as Keys in challenges dictionary below)
        private List<string> campaignIds = new List<string>();
        private Dictionary<String, ServerCampaignWithAssets> challenges = new Dictionary<String, ServerCampaignWithAssets>();
        internal static bool tinyTeaserCanBeVisible;

        internal MissionsManager missionsManager = null;

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
            internal Func<int> GetCurrencyFunc;
            internal Action<int> AddCurrencyAction;
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
        internal static int abTestSegment = 0;

        public static void SetGameCoinAsset(RewardType rt, Sprite defaultRewardIcon, string title, Func<int> GetCurrencyFunc, Action<int> AddCurrencyAction)
        {
            GameReward gr = new GameReward()
            {
                icon = defaultRewardIcon,
                title = title,
                GetCurrencyFunc = GetCurrencyFunc,
                AddCurrencyAction = AddCurrencyAction,
            };

            gameRewards[rt] = gr;
        }


        public static MonetizrManager Initialize(string apiKey, List<MissionDescription> sponsoredMissions, Action onRequestComplete, Action<bool> soundSwitch)
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
                    new MissionDescription(20, RewardType.Coins),
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


            Log.Print($"MonetizrManager Initialize: {apiKey}");

            var monetizrObject = new GameObject("MonetizrManager");
            var monetizrManager = monetizrObject.AddComponent<MonetizrManager>();

            DontDestroyOnLoad(monetizrObject);
            instance = monetizrManager;
            instance.sponsoredMissions = sponsoredMissions;

            monetizrManager.Initalize(apiKey, onRequestComplete, soundSwitch);



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
            Analytics.OnApplicationQuit();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initalize(string apiKey, Action gameOnInitSuccess, Action<bool> soundSwitch)
        {
#if USING_WEBVIEW
            if (!UniWebView.IsWebViewSupported)
            {
                Log.Print("WebView isn't supported on current platform!");
            }
#endif

            missionsManager = new MissionsManager();

            //missionsManager.CleanUp();

            this.soundSwitch = soundSwitch;

            _challengesClient = new ChallengesClient(apiKey);

            InitializeUI();

            onRequestComplete = (bool isOk) =>
            {

                if (!isOk)
                {
                    Log.Print("ERROR: Request complete is not okay!");
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
                    OnMainMenuShow();
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
            missionsManager.CleanRewardsClaims();
        }

        internal string GetCurrentAPIkey()
        {
            return _challengesClient.currentApiKey;
        }

        internal void ChangeAPIKey(string apiKey)
        {
            if (apiKey == _challengesClient.currentApiKey)
                return;

            _challengesClient.Close();

            _challengesClient = new ChallengesClient(apiKey);

            RequestCampaigns();
        }

        internal void RequestCampaigns()
        {
            isActive = false;

            uiController.DestroyTinyMenuTeaser();

            missionsManager.CleanUp();

            challenges.Clear();
            campaignIds.Clear();

            RequestChallenges(onRequestComplete);
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

            if (!MonetizrManager.Instance.HasCampaign(ch))
            {
                m.brandBanner = MonetizrManager.Instance.LoadSpriteFromCache(m.campaignId, m.brandBannerUrl);
                m.brandLogo = MonetizrManager.Instance.LoadSpriteFromCache(m.campaignId, m.brandLogoUrl);
                m.brandRewardBanner = MonetizrManager.Instance.LoadSpriteFromCache(m.campaignId, m.brandRewardBannerUrl);
                return;
            }

            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandBannerSprite);
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandLogoSprite);
            m.brandName = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(ch, AssetsType.BrandRewardBannerSprite);
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
                Debug.Log($"Nothing to reset in ResetCampaign");
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
                Debug.Log("\nTasks cancelled: timed out.\n");

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
                Debug.Log("SUCCESS!");

                //MonetizrManager.Analytics.TrackEvent("Enter email succeeded", m);

                ShowCongratsNotification((bool _) =>
                {
                    //lscreen.SetActive(false);



                    m.state = MissionUIState.ToBeHidden;

                    m.isClaimed = ClaimState.Claimed;

                    instance.ClaimMissionData(m);

                    instance.missionsManager.TryToActivateSurvey(m);


                    MonetizrManager.HideTinyMenuTeaser();


                    onComplete?.Invoke(false);
                },
                m);
            };

            Action onFail = () =>
            {
                Debug.Log("FAIL!");

                MonetizrManager.Analytics.TrackEvent("Email enter failed", m);

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
                    Debug.Log("\nTasks cancelled: timed out.\n");

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

        internal static void ShowDebug()
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
                Debug.Log($"------ShowStartupNotification ContainsKey(PanelId.StartNotification) {placement}");
                return;
            }

            bool forceSkip = false;

            //Debug.Log($"------ShowStartupNotification 1 {placement}");

            if (instance == null || !instance.HasCampaignsAndActive())
            {
                onComplete?.Invoke(false);
                return;
            }

            //Debug.Log($"------ShowStartupNotification 2 {placement}");

            //Debug.LogWarning("ShowStartupNotification");

            //Mission sponsoredMsns = instance.missionsManager.missions.Find((Mission item) => { return item.isSponsored; });
            Mission mission = instance.missionsManager.FindMissionForStartNotify();

            if (mission == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (placement == 0)
            {
                forceSkip = mission.additionalParams.GetParam("no_start_level_notifications") == "true";
            }
            else if (placement == 1)
            {
                forceSkip = mission.additionalParams.GetParam("no_main_menu_notifications") == "true";
            }

           // Debug.Log($"------ShowStartupNotification 3 {placement}");

            //var campaign = MonetizrManager.Instance.GetCampaign(mission.campaignId);

            if (mission.additionalParams.GetParam("no_campaigns_notification") == "true")
            {
                forceSkip = true;
            }

            //Debug.Log($"Notifications sk {mission.amountOfNotificationsSkipped} shown {mission.amountOfNotificationsShown}");
            
            mission.amountOfNotificationsSkipped++;

            if (mission.amountOfNotificationsSkipped <= mission.additionalParams.GetIntParam("amount_of_skipped_notifications"))
            {
                forceSkip = true;
            }
            
            //check if need to limit notifications amount
            if (mission.amountOfNotificationsShown == 0)
            {
                forceSkip = true;
            }

            if (forceSkip)
            {
                onComplete?.Invoke(true);
                return;
            }

            mission.amountOfNotificationsSkipped = 0;

            mission.amountOfNotificationsShown--;


            //instance.missionsManager.SaveAll();

            //Debug.Log($"------ShowStartupNotification 4 {placement}");

            //Debug.LogWarning("!!!!-------");

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
            if (m.type == MissionType.VideoReward)
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
            }

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

            if (Instance.missionsManager.TryToActivateSurvey(m))
            {
                //UpdateUI();
            }

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


        public static void RegisterUserDefinedMission(string missionTitle, string missionDescription, Sprite missionIcon, RewardType rt, int reward, float progress, Action onClaimButtonPress)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            Mission m = new Mission()
            {
                missionTitle = missionTitle,
                missionDescription = missionDescription,
                missionIcon = missionIcon,
                rewardType = rt,
                reward = reward,
                progress = progress,
                isSponsored = false,
                onClaimButtonPress = onClaimButtonPress,
                brandBanner = null,
            };

            instance.missionsManager.AddMission(m);
        }

        //TODO: need to connect now this mission and campaign from the server
        //next time once we register the mission it should connect with the same campaign
        public static void RegisterSponsoredMission(RewardType rt, int rewardAmount)
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
        }

        /// <summary>
        /// You need to earn goal amount of money to double it
        /// </summary>
        /// <param name="rewardIcon">Coins icon</param>
        /// <param name="goalAmount">How much you need to earn</param>
        /// <param name="rewardTitle">Coins</param>
        /// <param name="GetNormalCurrencyFunc">Get coins func</param>
        /// <param name="AddNormalCurrencyAction">Add coins to user account</param>
        public static void RegisterSponsoredMission2(RewardType rt, int goalAmount)
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
        }


        internal static void CleanUserDefinedMissions()
        {
            instance.missionsManager.CleanUserDefinedMissions();
        }

        public delegate void OnComplete(bool isSkipped);

        public static void EngagedUserAction(OnComplete onComplete)
        {
            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter();

            if(missions != null && missions.Count > 0)
            {
                //no more offers, skipping
                if (missions[0].amountOfRVOffersShown == 0)
                {
                    onComplete(false);
                }

                missions[0].amountOfRVOffersShown--;
            }

            MonetizrManager.ShowRewardCenter(null, (bool p) => { onComplete(p); });
        }


        public static void ShowRewardCenter(Action UpdateGameUI, Action<bool> onComplete = null)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            UpdateGameUI?.Invoke();

            var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            var m = instance.missionsManager.GetMission(challengeId);

            if (m == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            var missions = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter();

            /*int i = 1;
            foreach (var m2 in Instance.missionsManager.missions)
            {
                Debug.Log($"{i}:{m2.missionTitle}:{m2.campaignId}");
                i++;
            }*/

            if (missions.Count == 0)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (missions.Count == 1)
            //if (Instance.missionsManager.missions.Count == 1)
            {
                //Debug.Log($"---_PressSingleMission");

                Instance._PressSingleMission(onComplete, m);
                return;
            }



            Log.Print($"ShowRewardCenter with {m?.campaignId}");

            string uiItemPrefab = "MonetizrRewardCenterPanel";

            instance.uiController.ShowPanelFromPrefab(uiItemPrefab, PanelId.RewardCenter, onComplete, true, m);
        }

        internal static void HideRewardCenter()
        {
            instance.uiController.HidePanel(PanelId.RewardCenter);
        }

        internal void _PressSingleMission(Action<bool> onComplete, Mission m)
        {
            //if notification is alredy visible - do nothing
            //if (uiController.panels.ContainsKey(PanelId.TwitterNotification))
            //    return;

            if (m.isClaimed == ClaimState.Claimed)
                return;

            MonetizrManager.Instance.missionsManager.GetEmailGiveawayClaimAction(m, onComplete, null).Invoke();

            /* Action<bool> onTaskComplete = (bool isSkipped) =>
             {
                 MonetizrManager.Analytics.TrackEvent("Campaign rewarded", m);

                 m.isClaimed = ClaimState.Claimed;
                 missionsManager.SaveAll();

                 OnClaimRewardComplete(m, isSkipped, null);

                 HideTinyMenuTeaser();
             };


             if (m.isClaimed == ClaimState.NotClaimed)
             {
                 MonetizrManager.Analytics.TrackEvent("Campaign shown", m);

                 ShowNotification((bool isSkipped) => 
                     {
                         if (!isSkipped)
                         {
                             m.isClaimed = ClaimState.CompletedNotClaimed;
                             missionsManager.SaveAll();

                             MonetizrManager.Analytics.TrackEvent("Campaign claimed", m);

                             MonetizrManager.GoToLink(onTaskComplete, m);
                         }
                     },

                     m,
                     PanelId.TwitterNotification);
             }
             else
             {
                 onTaskComplete.Invoke(false);
             }*/


        }

        internal static void ShowMinigame(Action<bool> onComplete, PanelId id, Mission m = null)
        {
            Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);

            if (!instance.isActive)
                return;

            instance.uiController.ShowPanelFromPrefab("MonetizrGamePanel", id, onComplete, false, m);
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

        public static void OnStartGameLevel(Action onComplete)
        {
            if (instance == null)
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

            }
        }

        public static void OnNextLevel(Action<bool> onComplete)
        {
            //ShowStartupNotification((bool _) => { ShowRewardCenter(null, onComplete); });
        }

        public static void OnMainMenuShow()
        {
            if (instance == null)
                return;

            tinyTeaserCanBeVisible = true;

            if (!Instance.HasCampaignsAndActive())
                return;

            instance.initializeBuiltinMissions();

            //Debug.Log("------OnMainMenuShow 1");

            ShowStartupNotification(1, (bool isSkipped) =>
            {
                //Debug.Log($"------OnMainMenuShow 2 {isSkipped}");

                if (isSkipped)
                    ShowTinyMenuTeaser();
                else
                    ShowRewardCenter(null, (bool _) => { ShowTinyMenuTeaser(); });
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
                return;

            //has some active missions
            if (instance.missionsManager.missions.Find((Mission m) => { return m.isClaimed != ClaimState.Claimed; }) == null)
                return;

            var challengeId = MonetizrManager.Instance.GetActiveCampaign();
            if (!instance.HasAsset(challengeId, AssetsType.TinyTeaserTexture))
            {
                Log.Print("No texture for tiny teaser!");
                return;
            }

            var campaign = MonetizrManager.Instance.GetCampaign(challengeId);

            if (campaign.GetParam("hide_teaser_button") != "true")
            {
                int uiVersion = campaign.GetIntParam("teaser_design_version",2);

                instance.uiController.ShowTinyMenuTeaser(tinyTeaserPosition, UpdateGameUI, uiVersion, campaign);
            }
        }

        public static void HideTinyMenuTeaser()
        {
            //Assert.IsNotNull(instance, MonetizrErrors.msg[ErrorType.NotinitializedSDK]);
            if (instance == null)
                return;

            tinyTeaserCanBeVisible = false;

            if (!instance.isActive)
                return;

            instance.uiController.HidePanel(PanelId.TinyMenuTeaser);
        }

        internal void OnClaimRewardComplete(Mission mission, bool isSkipped, Action updateUIDelegate)
        {
            if (claimForSkippedCampaigns)
                isSkipped = false;

            if (isSkipped)
                return;

            ShowCongratsNotification((bool _) =>
            {
                bool updateUI = false;

                mission.state = MissionUIState.ToBeHidden;

                mission.isClaimed = ClaimState.Claimed;

                ClaimMissionData(mission);

                if (missionsManager.TryToActivateSurvey(mission))
                {
                    //UpdateUI();
                    updateUI = true;
                }

                if (serverClaimForCampaigns && CheckFullCampaignClaim(mission))
                {
                    ClaimReward(mission.campaignId, CancellationToken.None, () =>
                    {
                        RequestCampaigns();


                    });

                }

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


        public Sprite LoadSpriteFromCache(string campaignId, string assetUrl)
        {
            string fname = Path.GetFileName(assetUrl);
            string fpath = Application.persistentDataPath + "/" + campaignId + "/" + fname;

            if (!File.Exists(fpath))
                return null;

            byte[] data = File.ReadAllBytes(fpath);

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);
            tex.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        /// <summary>
        /// Helper function to download and assign graphics assets
        /// </summary>
        private async Task AssignAssetTextures(ServerCampaignWithAssets ech, ServerCampaign.Asset asset, AssetsType texture, AssetsType sprite, bool isOptional = false)
        {
            if (asset.url == null || asset.url.Length == 0)
            {
                Debug.LogWarning($"Resource {texture} {sprite} has no url in path!");
                ech.isChallengeLoaded = false;
                return;
            }

            string path = Application.persistentDataPath + "/" + ech.campaign.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;

            //Debug.Log(fpath);

            byte[] data = null;

            if (!File.Exists(fpath))
            {
                data = await DownloadHelper.DownloadAssetData(asset.url);

                if (data == null)
                {
                    if (!isOptional)
                        ech.isChallengeLoaded = false;

                    return;
                }

                File.WriteAllBytes(fpath, data);

                //Log.Print("saving: " + fpath);
            }
            else
            {
                data = File.ReadAllBytes(fpath);

                if (data == null)
                {
                    if (!isOptional)
                        ech.isChallengeLoaded = false;

                    return;
                }

                //Log.Print("reading: " + fpath);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(1000);
#endif

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);
            tex.wrapMode = TextureWrapMode.Clamp;

            if (texture != AssetsType.Unknown)
                ech.SetAsset<Texture2D>(texture, tex);

            Sprite s = null;
            if (sprite != AssetsType.Unknown)
            {
                s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                ech.SetAsset<Sprite>(sprite, s);
            }

            ech.SetAssetUrl(sprite, asset.url);

            bool texStatus = tex != null;
            bool spriteStatus = s != null;

            //Debug.Log($"Adding texture:{texture}={texStatus} sprite:{sprite}={spriteStatus} into:{ech.campaign.id}");
        }

        private async Task PreloadAssetToCache(ServerCampaignWithAssets ech, ServerCampaign.Asset asset, /*AssetsType urlString,*/ AssetsType fileString, bool required = true)
        {
            if (asset.url == null || asset.url.Length == 0)
            {
                Debug.LogWarning($"Malformed URL for {fileString} {ech.campaign.id}");
                return;
            }

            string path = Application.persistentDataPath + "/" + ech.campaign.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;
            string zipFolder = null;
            string fileToCheck = fpath;

            //Log.Print(fname);

            if (fname.Contains("zip"))
            {
                zipFolder = path + "/" + fname.Replace(".zip", "");
                fileToCheck = zipFolder + "/index.html";

                //Log.Print($"archive: {zipFolder} {fileToCheck} {File.Exists(fileToCheck)}");
            }

            byte[] data = null;

            if (!File.Exists(fileToCheck))
            {
                data = await DownloadHelper.DownloadAssetData(asset.url);

                if (data == null)
                {
                    if (required)
                        ech.isChallengeLoaded = false;

                    return;
                }

                File.WriteAllBytes(fpath, data);

                if (zipFolder != null)
                {
                    //Log.Print("extracting to: " + zipFolder);

                    if (Directory.Exists(zipFolder))
                        DeleteDirectory(zipFolder);

                    //if (!Directory.Exists(zipFolder))
                    Directory.CreateDirectory(zipFolder);

                    ZipFile.ExtractToDirectory(fpath, zipFolder);

                    File.Delete(fpath);
                }


                //Log.Print("saving: " + fpath);
            }

            if (zipFolder != null)
                fpath = fileToCheck;

            //Log.Print($"resource {fileString} {fpath}");

            //ech.SetAsset<string>(urlString, asset.url);
            ech.SetAsset<string>(fileString, fpath);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                //File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        /// <summary>
        /// Request challenges from the server
        /// </summary>
        public async void RequestChallenges(Action<bool> onRequestComplete)
        {
            List<ServerCampaign> _challenges = new List<ServerCampaign>();

            try
            {

                _challenges = await _challengesClient.GetList();
            }
            catch (Exception e)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]} {e}");
                onRequestComplete?.Invoke(false);
            }

            if (_challenges == null)
            {
                Log.Print($"{MonetizrErrors.msg[ErrorType.ConnectionError]}");
                onRequestComplete?.Invoke(false);
            }

            campaignIds.Clear();



#if TEST_SLOW_LATENCY
            await Task.Delay(10000);
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif
            Color c;

            foreach (var ch in _challenges)
            {
                var ech = new ServerCampaignWithAssets(ch);

                if (this.challenges.ContainsKey(ch.id))
                    continue;

                string path = Application.persistentDataPath + "/" + ech.campaign.id;

                Debug.Log($"Campaign path: {path}");

                foreach (var asset in ch.assets)
                {
                    switch (asset.type)
                    {
                        case "icon":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandLogoSprite);

                            break;
                        case "banner":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandBannerSprite);

                            break;
                        case "logo":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandRewardLogoSprite);

                            break;
                        case "reward_banner":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.BrandRewardBannerSprite);

                            break;

                        case "tiny_teaser":
                            await AssignAssetTextures(ech, asset, AssetsType.TinyTeaserTexture, AssetsType.TinyTeaserSprite);

                            break;

                        case "survey":
                            ech.SetAsset<string>(AssetsType.SurveyURLString, asset.url);

                            break;
                        case "video":
                            await PreloadAssetToCache(ech, asset, AssetsType.VideoFilePathString, true);

                            break;
                        case "text":
                            ech.SetAsset<string>(AssetsType.BrandTitleString, asset.title);

                            break;

                        case "html":
                            await PreloadAssetToCache(ech, asset, AssetsType.Html5PathString, false);

                            break;

                        case "tiny_teaser_gif":
                            await PreloadAssetToCache(ech, asset, AssetsType.TeaserGifPathString, false);

                            break;

                        case "campaign_text_color":

                            if (ColorUtility.TryParseHtmlString(asset.title, out c))
                                ech.SetAsset<Color>(AssetsType.CampaignTextColor, c);

                            break;

                        case "campaign_header_text_color":

                            if (ColorUtility.TryParseHtmlString(asset.title, out c))
                                ech.SetAsset<Color>(AssetsType.CampaignHeaderTextColor, c);

                            break;

                        case "header_text_color":

                            if (ColorUtility.TryParseHtmlString(asset.title, out c))
                                ech.SetAsset<Color>(AssetsType.HeaderTextColor, c);

                            break;

                        case "campaign_background_color":

                            if (ColorUtility.TryParseHtmlString(asset.title, out c))
                                ech.SetAsset<Color>(AssetsType.CampaignBackgroundColor, c);

                            break;

                        case "tiled_background":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.TiledBackgroundSprite, true);

                            break;

                        case "custom_coin_title":
                            ech.SetAsset<string>(AssetsType.CustomCoinString, asset.title);

                            break;

                        case "custom_coin_icon":
                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.CustomCoinSprite, true);

                            break;

                        case "loading_screen":

                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.LoadingScreenSprite, true);

                            break;

                        case "reward_image":

                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.RewardSprite, true);

                            break;

                        case "ingame_reward_image":

                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.IngameRewardSprite, true);

                            break;

                        case "unknown_reward_image":

                            await AssignAssetTextures(ech, asset, AssetsType.Unknown, AssetsType.IngameRewardSprite, true);

                            break;

                    }

                }


                //TODO: check if all resources available

                /*if (!ech.HasAsset(AssetsType.VideoFilePathString) && !ech.HasAsset(AssetsType.Html5PathString))
                {
                    Log.Print($"ERROR: Campaign {ch.id} has neither video, nor html5 asset");
                    ech.isChallengeLoaded = false;
                }*/

                if (ech.HasAsset(AssetsType.SurveyURLString) && ech.GetAsset<string>(AssetsType.SurveyURLString).Length == 0)
                {
                    Log.Print($"ERROR: Campaign {ch.id} has survey asset, but url is empty");
                    ech.isChallengeLoaded = false;
                }

                if (ech.isChallengeLoaded)
                {

                    this.challenges.Add(ch.id, ech);
                    campaignIds.Add(ch.id);
                }
            }

            activeChallengeId = campaignIds.Count > 0 ? campaignIds[0] : null;

            isMissionsIsOudated = true;

#if TEST_SLOW_LATENCY
            Log.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
#endif

            Log.Print($"RequestChallenges completed with count: {campaignIds.Count} active: {activeChallengeId}");

            //Ok, even if response empty
            onRequestComplete?.Invoke(/*challengesId.Count > 0*/true);
        }

        /// <summary>
        /// Get Challenge by Id
        /// TODO: Don't give access to challenge itself, update progress internally
        /// </summary>
        /// <returns></returns>
        internal ServerCampaign GetCampaign(String chId)
        {
            if (!challenges.ContainsKey(chId))
            {
                Debug.LogWarning($"You're trying to get campaign {chId} which is not exist!");
                return null;
            }

            return challenges[chId].campaign;
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

            if (!challenges.ContainsKey(challengeId))
            {
                Log.Print($"You requesting asset for challenge {challengeId} that not exist!");
                return default(T);
            }

            if (!HasAsset(challengeId, t))
            {
                //Log.Print($"{challengeId} has no asset {t}");
                return default(T);
            }

            return challenges[challengeId].GetAsset<T>(t);
        }

        public string GetAssetUrl(String challengeId, AssetsType t)
        {
            return challenges[challengeId].GetAssetUrl(t);
        }

        public bool HasCampaign(String challengeId)
        {
            return challenges.ContainsKey(challengeId);
        }

        public bool HasAsset(String challengeId, AssetsType t)
        {
            return challenges[challengeId].HasAsset(t);
        }

        /// <summary>
        /// Single update for reward and claim
        /// </summary>
        public async Task ClaimReward(String challengeId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            var challenge = challenges[challengeId].campaign;

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