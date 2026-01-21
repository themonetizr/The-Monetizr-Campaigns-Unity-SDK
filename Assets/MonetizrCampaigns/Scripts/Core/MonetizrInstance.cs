using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Prebid;
using Monetizr.SDK.Rewards;
using Monetizr.SDK.UI;
using Monetizr.SDK.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.SDK.Core
{
    public class MonetizrInstance : MonoBehaviour
    {
        public static MonetizrInstance Instance;

        public List<UnityEngine.Object> holdResources = new List<UnityEngine.Object>();

        internal MonetizrHttpClient ConnectionsClient { get; private set; }
        internal RewardSelectionType temporaryRewardTypeSelection = RewardSelectionType.Product;
        internal Action<string, Dictionary<string, string>> ExternalAnalytics = null;
        internal MissionsManager missionsManager = null;
        internal LocalSettingsManager localSettings = null;
        internal UIController uiController = null;
        internal Action<bool> onUIVisible = null;
        internal Vector2? teaserPosition = null;
        internal Transform teaserRoot;
        internal bool claimForSkippedCampaigns;
        internal bool serverClaimForCampaigns;
        internal bool canTeaserBeVisible;
        internal bool keepLocalClaimData;

        private List<ServerCampaign> campaigns = new List<ServerCampaign>();
        private MonetizrManager.UserDefinedEvent userEvent = null;
        private ServerCampaign _activeCampaignId = null;
        private Action<bool> soundSwitch = null;
        private Action onRequestComplete = null;
        private bool _isActive = false;
        private bool _isMissionsIsOutdated = true;
        private int debugAttempt = 0;

        private void Awake ()
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        private void OnApplicationQuit ()
        {
            MonetizrLogger.Print("SDK application ended.", true);
            MonetizrMobileAnalytics.OnApplicationQuit();
            ConnectionsClient.Close();
        }

        public void InitializeSDK (Action onRequestCompleteAction, Action<bool> soundSwitch, Action<bool> onUIVisible, MonetizrManager.UserDefinedEvent userEvent)
        {
            MonetizrLogger.Print($"Initializing Monetizr SDK with: {MonetizrSettings.apiKey} // {MonetizrSettings.bundleID} // {MonetizrSettings.SDKVersion}");

#if UNITY_EDITOR
            serverClaimForCampaigns = false;
            claimForSkippedCampaigns = true;
#else
            serverClaimForCampaigns = true;
            claimForSkippedCampaigns = false;
#endif

            keepLocalClaimData = true;
            this.soundSwitch = soundSwitch;
            this.onUIVisible = onUIVisible;
            this.userEvent = userEvent;

            localSettings = new LocalSettingsManager();
            missionsManager = new MissionsManager();
            uiController = new UIController();
            ConnectionsClient = new MonetizrHttpClient(MonetizrSettings.apiKey);
            MonetizrMobileAnalytics.SetupAnalytics();

            if (soundSwitch == null)
            {
                soundSwitch = (bool isOn) =>
                {
                    MonetizrLogger.Print($"AudioListener pause state: {!isOn}");
                    AudioListener.pause = !isOn;
                };
                this.soundSwitch = soundSwitch;
            }

            string consent = PrebidManager.GetIabConsentString();
            if (!string.IsNullOrEmpty(consent))
            {
                MonetizrManager.s_consent = consent;
                MonetizrLogger.Print($"Loaded GDPR_CONSENT: {consent}");
            }

            onRequestComplete = () =>
            {
                onRequestCompleteAction?.Invoke();

                if (canTeaserBeVisible)
                {
                    OnMainMenuShow(false);
                }
            };

            if (MonetizrSettings.apiKey == "LOCAL_TESTING")
            {
                MonetizrLogger.Print("Initializing Local Testing Campaign.");
                SetupLocalTestingCampaign(onRequestComplete);
                return;
            }

            RequestCampaigns(onRequestComplete);

            if (MonetizrSettings.requestCampaignTime > 0)
            {
                StartCoroutine(TryRequestCampaignsLater(MonetizrSettings.requestCampaignTime));
            }
        }

        private async void RequestCampaigns (Action onRequestComplete)
        {
            await ConnectionsClient.GetGlobalSettings();
            CheckAnalyticsProxy();
            CheckCGPLogging();

            campaigns = new List<ServerCampaign>();
            campaigns = await ConnectionsClient.GetList();
            campaigns = await CampaignManager.Instance.ProcessCampaigns(campaigns);

            if (campaigns == null || campaigns.Count <= 0)
            {
                MonetizrLogger.PrintWarning("No Campaigns available or error obtaining them.", true);
                onRequestComplete?.Invoke();
                return;
            }

            MonetizrLogger.Print("Campaigns succesfully downloaded.", true);
            ConnectionsClient.SetTestMode(campaigns[0].testmode);
            localSettings.LoadOldAndUpdateNew(campaigns);

            MonetizrLogger.Print($"Monetizr SDK initialized with {campaigns.Count} campaigns.");
            _isActive = true;
            onRequestComplete?.Invoke();
        }

        private async void SetupLocalTestingCampaign(Action onRequestComplete)
        {
            ConnectionsClient.GlobalSettings = LocalTestCampaignManager.GetGlobalSettings();
            campaigns = await LocalTestCampaignManager.GetCampaigns();
            localSettings.LoadOldAndUpdateNew(campaigns);
            _isActive = true;
            onRequestComplete?.Invoke();
        }

        private void CheckAnalyticsProxy()
        {
            string analyticsProxy = "";
            if (ConnectionsClient.GlobalSettings.TryGetValue("mixpanel_proxy_endpoint", out analyticsProxy))
            {
                if (!String.IsNullOrEmpty(analyticsProxy))
                {
                    MonetizrLogger.Print("Analytics Proxy set to: " + analyticsProxy);
                    MonetizrMobileAnalytics.SetProxyEndpoint(analyticsProxy);
                }
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

        private IEnumerator TryRequestCampaignsLater(float time)
        {
            while (true)
            {
                yield return new WaitForSeconds(time);
                if (campaigns.Count != 0) continue;
                _isActive = false;
                RequestCampaigns(onRequestComplete);
            }
        }

        public void OnMainMenuShow(bool showNotifications = true)
        {
            canTeaserBeVisible = true;
            if (Instance == null) return;
            if (!Instance.IsSDKActiveAndHasCampaigns()) return;
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

        public void OnMainMenuHide()
        {
            canTeaserBeVisible = false;
            HideTeaser();
        }

        internal async void ResetCampaign()
        {
            Mission m = Instance.missionsManager.GetFirstUnactiveMission();

            if (m == null)
            {
                MonetizrLogger.Print($"Nothing to reset in ResetCampaign");
                return;
            }

            string campaignId = m.campaignId;
            var lscreen = Instance.uiController.ShowLoadingScreen();
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

        internal async void WaitForEndRequestAndNotify(Action<bool> onComplete, Mission m, Action updateUIDelegate)
        {
            var lscreen = Instance.uiController.ShowLoadingScreen();
            lscreen._onComplete = (bool _) => { GameObject.Destroy(lscreen); };

            Action onSuccess = () =>
            {
                MonetizrLogger.Print("SUCCESS!");
                MonetizrMobileAnalytics.TrackEvent(m, m.adPlacement, Core.EventType.ButtonPressOk);
                OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);
            };

            Action onFail = () =>
            {
                MonetizrLogger.Print("FAIL!"); ;
                MonetizrMobileAnalytics.TrackEvent(m, m.adPlacement, Core.EventType.Error);
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

            MonetizrManager.temporaryEmail = null;
            lscreen.SetActive(false);
        }

        internal bool CheckFullCampaignClaim(Mission m)
        {
            return missionsManager.CheckFullCampaignClaim(m);
        }

        internal void RequestCampaigns(bool callRequestComplete = true)
        {
            MonetizrLogger.Print("Re-RequestCampaigns.");

            _isActive = false;
            _isMissionsIsOutdated = true;
            uiController.DestroyTeaser();
            missionsManager.CleanUp();
            campaigns.Clear();
            _activeCampaignId = null;

            if (ConnectionsClient.currentApiKey == "LOCAL_TESTING")
            {
                MonetizrLogger.Print("Reinitializing Local Testing Campaign.");
                SetupLocalTestingCampaign(onRequestComplete);
                return;
            }

            RequestCampaigns(callRequestComplete ? onRequestComplete : null);
        }

        internal void ClaimMissionData(Mission m)
        {
            MonetizrManager.gameRewards[m.rewardType].AddCurrencyAction(m.reward);
            if (keepLocalClaimData) Instance.SaveClaimedReward(m);
        }

        internal void _PressSingleMission(Action<bool> onComplete, Mission m)
        {
            if (m.isClaimed == ClaimState.Claimed) return;
            missionsManager.ClaimAction(m, onComplete, null).Invoke();
        }

        internal void OnClaimRewardComplete(Mission mission, bool isSkipped, Action<bool> onComplete, Action updateUIDelegate)
        {
            if (claimForSkippedCampaigns) isSkipped = false;

            if (isSkipped)
            {
                MonetizrLogger.Print("OnClaimRewardComplete");
                onComplete?.Invoke(true);
                return;
            }

            MonetizrLogger.Print($"OnClaimRewardComplete for {mission.serverId}");
            MonetizrLogger.Print("HasCongrats: " + mission.hasCongrats);

            if (!mission.hasCongrats)
            {
                MonetizrLogger.Print("Skipping Congrats");
                OnCongratsShowed(mission, isSkipped, onComplete, updateUIDelegate);
                return;
            }

            ShowCongratsNotification((bool _) =>
            {
                OnCongratsShowed(mission, isSkipped, onComplete, updateUIDelegate);

            }, mission);
        }

        private void OnCongratsShowed(Mission mission, bool isSkipped, Action<bool> onComplete, Action updateUIDelegate)
        {
            bool updateUI = false;
            MonetizrLogger.Print("OnCongratsShowed.");

            if (mission.campaignServerSettings.GetParam("RewardCenter.do_not_claim_and_hide_missions") != "true")
            {
                mission.state = MissionUIState.ToBeHidden;
                mission.isClaimed = ClaimState.Claimed;
            }

            ClaimMissionData(mission);
            if (missionsManager.UpdateMissionsActivity(mission)) updateUI = true;
            HideTeaser(true);
            onComplete?.Invoke(isSkipped);
            if (updateUI) updateUIDelegate?.Invoke();

            if (HasRemainingCampaigns()) return;

            // TODO: LEAVE PARAMETER ONLY
            bool shouldRestart = mission.campaign.campaignType == CampaignType.Fallback || mission.campaignServerSettings.GetBoolParam("should_restart", false);
            if (shouldRestart && serverClaimForCampaigns && CheckFullCampaignClaim(mission))
            {
                int restartDelay = mission.campaign.serverSettings.GetIntParam("restart_timer", 0);
                MonetizrLogger.Print("Will restart RequestCampaigns in " + restartDelay + " seconds.");
                RestartCampaignsFlow(mission, restartDelay);
            }
        }

        private async void RestartCampaignsFlow(Mission mission, int delayTime)
        {
            if (delayTime > 0) await Task.Delay(TimeSpan.FromSeconds(delayTime));
            MonetizrLogger.Print("Restarting RequestCampaigns.");
            _ = ClaimReward(mission.campaign, CancellationToken.None, () => { RequestCampaigns(true); }, () => { RequestCampaigns(true); });
        }

        internal ServerCampaign FindBestCampaignToActivate()
        {
            if (!IsActiveAndEnabled()) return null;

            if (_activeCampaignId != null)
            {
                ServerCampaign campaign = GetActiveCampaign();
                if (campaign.CanCampaignBeActivated()) return campaign;
            }

            foreach (ServerCampaign campaign in campaigns)
            {
                if (campaign == _activeCampaignId) continue;
                if (campaign.CanCampaignBeActivated()) return campaign;
            }

            return null;
        }

        internal void SetActiveCampaign(ServerCampaign campaign)
        {
            if (campaign == _activeCampaignId) return;
            if (campaign != _activeCampaignId) _isMissionsIsOutdated = true;
            _activeCampaignId = campaign;
            MonetizrManager.closeRewardCenterAfterEveryMission = campaign.serverSettings.GetBoolParam("RewardCenter.close_after_mission_completion", MonetizrManager.closeRewardCenterAfterEveryMission);
            MonetizrLogger.Print($"Active campaign: {_activeCampaignId.id}");
        }

        #region Check Methods

        internal bool HasCampaign(string campaignId)
        {
            return campaigns.FindIndex(c => c.id == campaignId) >= 0;
        }

        private bool HasRemainingCampaigns()
        {
            if (!IsSDKActiveAndHasCampaigns()) return false;
            ServerCampaign campaign = FindBestCampaignToActivate();
            if (campaign == null) return false;
            return missionsManager.GetActiveMissionsNum(campaign) > 0;
        }

        internal ServerCampaign GetActiveCampaign()
        {
            if (!IsActiveAndEnabled()) return null;
            return _activeCampaignId;
        }

        public bool HasActiveCampaign()
        {
            return _isActive && _activeCampaignId != null;
        }

        internal bool IsSDKActiveAndHasCampaigns()
        {
            return _isActive && campaigns.Count > 0;
        }

        public bool IsActiveAndEnabled()
        {
            return Instance != null && Instance.IsSDKActiveAndHasCampaigns();
        }

        internal string GetCurrentAPIkey()
        {
            return ConnectionsClient.currentApiKey;
        }

        #endregion

        #region UI Methods

        public void ShowStartupNotification(NotificationPlacement placement, Action<bool> onComplete)
        {
            if (Instance.uiController.HasActivePanel(PanelId.StartNotification))
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

        public void ShowRewardCenter(Action UpdateGameUI, Action<bool> onComplete = null)
        {
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
            Instance.uiController.ShowPanelFromPrefab(uiItemPrefab, PanelId.RewardCenter, onComplete, true, m);
        }

        public void ShowTeaser(Action UpdateGameUI = null)
        {
            if (!canTeaserBeVisible) return;
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
            Instance.uiController.ShowTeaser(teaserRoot, teaserPosition, UpdateGameUI, uiVersion, campaign);
        }

        public void HideTeaser(bool checkIfSomeMissionsAvailable = false)
        {
            if (Instance == null) return;
            if (checkIfSomeMissionsAvailable && Instance.missionsManager.GetActiveMissionsNum() > 0) return;
            if (!Instance._isActive) return;
            Instance.uiController.HidePanel(PanelId.TinyMenuTeaser);
        }

        internal void ShowMessage(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            Instance.uiController.ShowPanelFromPrefab("MonetizrMessagePanel2", panelId, onComplete, true, m);
        }

        internal void ShowNotification(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");

            // TODO: add IsCampaignHTML check next
            Instance.uiController.ShowPanelFromPrefab("MonetizrNotifyPanel2", panelId, onComplete, true, m);
        }

        internal void ShowEnterEmailPanel(Action<bool> onComplete, Mission m, PanelId panelId)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            Instance.uiController.ShowPanelFromPrefab("MonetizrEnterEmailPanel2", panelId, onComplete, true, m);
        }

        internal void ShowCongratsNotification(Action<bool> onComplete, Mission m)
        {
            ShowNotification(onComplete, m, PanelId.CongratsNotification);
        }

        internal void ShowCodeView(Action<bool> onComplete, Mission m = null)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            if (!Instance._isActive) return;
            Instance.uiController.ShowPanelFromPrefab("MonetizrEnterCodePanel2", PanelId.CodePanelView, onComplete, false, m);
        }

        internal void ShowMinigame(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
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

            Instance.uiController.ShowPanelFromPrefab(panelNames[m.type].Item2, panelNames[m.type].Item1, onComplete, false, m);
        }

        internal void ShowUnitySurvey(Action<bool> onComplete, Mission m)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            if (!Instance._isActive) return;
            Instance.uiController.ShowPanelFromPrefab("MonetizrUnitySurveyPanel", PanelId.SurveyUnityView, onComplete, false, m);
        }

        internal void ShowWebView(Action<bool> onComplete, PanelId id, Mission m = null)
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            if (!Instance._isActive) return;
            Instance.uiController.ShowPanelFromPrefab("MonetizrWebViewPanel2", id, onComplete, false, m);
        }

        internal void ShowActionView(Action<bool> onComplete, Mission m = null)
        {
            ShowWebView(onComplete, PanelId.ActionHtmlPanelView, m);
        }

        internal void ShowSurvey(Action<bool> onComplete, Mission m = null)
        {
            ShowWebView(onComplete, PanelId.SurveyWebView, m);
        }

        internal void ShowWebPage(Action<bool> onComplete, Mission m = null)
        {
            ShowWebView(onComplete, PanelId.HtmlWebPageView, m);
        }

        internal void ShowHTML5(Action<bool> onComplete, Mission m = null)
        {
            ShowWebView(onComplete, PanelId.Html5WebView, m);
        }

        #endregion

        #region Mission Methods

        internal void CleanUserDefinedMissions()
        {
            Instance.missionsManager.CleanUserDefinedMissions();
        }

        internal void InitializeBuiltinMissions(ServerCampaign campaign)
        {
            missionsManager.CreateMissionsFromCampaign(campaign);
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

        #endregion

        #region Rewards Methods

        internal void SaveClaimedReward(Mission m)
        {
            missionsManager.SaveAll();
        }

        internal void CleanRewardsClaims()
        {
            localSettings.ResetData();
            missionsManager.CleanRewardsClaims();
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

        internal GameReward GetGameReward(RewardType rt)
        {
            if (MonetizrManager.gameRewards.ContainsKey(rt))
            {
                return MonetizrManager.gameRewards[rt];
            }

            return null;
        }

        #endregion

        #region Other Methods

        public void SoundSwitch(bool on)
        {
            soundSwitch?.Invoke(on);
        }

        public void HoldResource(object o)
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

        public void ShowDebug()
        {
            Assert.IsNotNull(Instance, "Monetizr SDK has not been initialized. Call MonetizrManager.Initalize first.");
            debugAttempt++;

#if !UNITY_EDITOR
            if (debugAttempt != 10)
                return;
#endif

            debugAttempt = 0;
            Instance.uiController.ShowPanelFromPrefab("MonetizrDebugPanel", PanelId.DebugPanel);
        }

        public void OnEngagedUserActionComplete()
        {
            if (!MonetizrManager.isUsingEngagedUserAction) return;
            MonetizrManager.hasCompletedEngagedUserAction = true;
        }

        internal void CallUserDefinedEvent(string campaignId, string placement, Core.EventType eventType)
        {
            try
            {
                userEvent?.Invoke(campaignId, placement, eventType);
            }
            catch (Exception ex)
            {
                MonetizrLogger.PrintError($"Exception in _CallUserDefinedEvent\n{ex}");
            }
        }

        internal void ChangeAPIKey(string apiKey)
        {
            MonetizrLogger.Print($"Changing API key to: {apiKey}");
            ConnectionsClient.currentApiKey = apiKey;
            RestartClient();
        }

        internal void RestartClient()
        {
            MonetizrLogger.Print("Restarting Client.");
            ConnectionsClient.Close();
            ConnectionsClient = new MonetizrHttpClient(ConnectionsClient.currentApiKey);
            RequestCampaigns();
        }

        #endregion

    }
}