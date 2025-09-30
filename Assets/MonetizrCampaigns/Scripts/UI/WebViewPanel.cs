#define UNI_WEB_VIEW

using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Prebid;
using Monetizr.SDK.Utils;
using Monetizr.SDK.VAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using EventType = Monetizr.SDK.Core.EventType;

namespace Monetizr.SDK.UI
{
    internal class WebViewPanel : PanelController
    {
        [Header("General Settings")]
        public TextAsset closeButtonImageAsset;
        public Button closeButton;
        public Image background;
        public GameObject claimButton;
        public Animator crossButtonAnimator;
        public string successReason;
        public bool claimPageReached = false;
        public string programmaticStatus;
        public RectTransform safeArea;

        [Header("Start Notification Settings")]
        [SerializeField] private GameObject startNotificationParent;
        [SerializeField] private Image logo;
        [SerializeField] private Text rewardTitle;
        [SerializeField] private Text rewardText;
        [SerializeField] private Image rewardImage;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text confirmButtonText;

        [Header("Congrats Notification Settings")]
        [SerializeField] private GameObject congratsNotificationParent;
        [SerializeField] private Image congratsLogo;
        [SerializeField] private Text congratsRewardTitle;
        [SerializeField] private Text congratsRewardText;
        [SerializeField] private Image congratsRewardImage;
        [SerializeField] private Button congratsConfirmButton;
        [SerializeField] private Text congratsConfirmButtonText;

        private string _webUrl;
        private string _rewardWebUrl;
        private string eventsPrefix;
        private AdPlacement adType;
        private int _pagesSwitchesAmount = -1;
        private int _claimButtonDelay;
        private bool impressionStarts = false;
        private int _closeButtonDelay;
        private bool isOMSDKactive = false;

#if UNI_WEB_VIEW
        private UniWebView _webView = null;
#endif

        private new void Awake ()
        {
            base.Awake();
        }

        private void Update ()
        {
            bool panelForCheckUrl = (panelId == PanelId.SurveyWebView) ||
                                    (panelId == PanelId.ActionHtmlPanelView) ||
                                    (panelId == PanelId.Html5WebView);

            if (_webView == null || !panelForCheckUrl || string.IsNullOrEmpty(_webUrl)) return;

            string currentUrl = _webView.Url;

            if (string.IsNullOrEmpty(currentUrl) || _webUrl.Equals(currentUrl)) return;

            _webUrl = currentUrl;
            _pagesSwitchesAmount--;

            MonetizrLogger.Print($"Update: [{_webUrl}] [{currentUrl}] [{_pagesSwitchesAmount}]");

            if (_pagesSwitchesAmount == 0)
            {
                successReason = "page_switch";

                ShowClaimButton();
            }

            if (_webUrl.Contains("uniwebview"))
            {
                OnCompleteEvent();
            }
            else if (!string.IsNullOrEmpty(_rewardWebUrl) && _webUrl.Contains(_rewardWebUrl))
            {
                successReason = "reward_page_reached";
                claimPageReached = true;
                OnCompleteEvent();
            }
        }

        internal override void PreparePanel (PanelId id, Action<bool> onComplete, Mission m)
        {
#if UNI_WEB_VIEW
            this._onComplete = onComplete;
            panelId = id;
            currentMission = m;

            bool fullScreen = true;
            bool useSafeFrame = false;

            if (id == PanelId.HtmlWebPageView)
            {
                fullScreen = false;
            }

            if (id == PanelId.SurveyWebView)
            {
                fullScreen = false;
                useSafeFrame = true;
            }

            claimButton.SetActive(false);
            PrepareWebViewComponent(fullScreen, useSafeFrame);
            closeButton.gameObject.SetActive(!fullScreen);

            _closeButtonDelay = m.campaignServerSettings.GetIntParam(
                new List<string>()
                {
                    "email_enter_close_button_delay",
                    "VideoReward.close_button_delay"
                }, 0);

            StartCoroutine(ShowCloseButton(_closeButtonDelay));
            background.color = Color.black;
            adType = getAdPlacement();
            eventsPrefix = adType.ToString();

            switch (id)
            {
                case PanelId.SurveyWebView:
                    MonetizrLogger.Print("Preparing Survey.");
                    PrepareSurveyPanel(m);
                    MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
                    impressionStarts = true;
                    break;

                case PanelId.HtmlWebPageView:
                    MonetizrLogger.Print("Preparing HTML.");
                    PrepareWebViewPanel(m);
                    MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
                    impressionStarts = true;
                    break;

                case PanelId.ActionHtmlPanelView:
                    MonetizrLogger.Print("Preparing Action/ENDCARD.");
                    PrepareActionPanel(m);
                    MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
                    impressionStarts = true;
                    break;

                case PanelId.StartNotification:
                    MonetizrLogger.Print("Preparing StartNotification.");
                    PrepareStartNotificationPanel(m);
                    MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
                    impressionStarts = true;
                    break;

                case PanelId.CongratsNotification:
                    MonetizrLogger.Print("Preparing CongratsNotification.");
                    PrepareCongratsNotificationPanel(m);
                    MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
                    impressionStarts = true;
                    break;
            }

            if (!string.IsNullOrEmpty(_webUrl))
            {
                MonetizrLogger.Print($"Url to show {_webUrl}");
                _webView.Show();
            }

            if (id == PanelId.Html5WebView) PrepareHtml5Panel();
#endif
        }

#if UNI_WEB_VIEW
        internal void PrepareWebViewComponent(bool fullScreen, bool useSafeFrame)
        {
#if UNITY_EDITOR
            fullScreen = false;
#endif

            if (MonetizrLogger.isEnabled) UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;
            UniWebView.SetAllowAutoPlay(true);
            UniWebView.SetAllowInlinePlay(true);

#if UNITY_EDITOR
            UniWebView.SetWebContentsDebuggingEnabled(true);
#endif

            UniWebView.SetJavaScriptEnabled(true);
            UniWebView.SetAllowUniversalAccessFromFileURLs(true);
            MonetizrManager.Instance.SoundSwitch(false);

            UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;
            UniWebView.SetForwardWebConsoleToNativeOutput(true);

            _webView = gameObject.AddComponent<UniWebView>();
            //_webView.ReferenceRectTransform = safeArea;

            SetWebviewFrame(fullScreen, useSafeFrame);

            MonetizrLogger.Print($"frame: {fullScreen} {_webView.Frame}");

            _webView.OnMessageReceived += OnMessageReceived;
            _webView.OnPageStarted += OnPageStarted;
            _webView.OnPageFinished += OnPageFinished;
            _webView.OnPageErrorReceived += OnPageErrorReceived;
            _webView.Alpha = 0;
        }
#endif

        internal AdPlacement getAdPlacement()
        {
            switch (panelId)
            {
                case PanelId.SurveyWebView:
                    return AdPlacement.Survey;

                case PanelId.Html5WebView:
                    return AdPlacement.Html5;

                case PanelId.HtmlWebPageView:
                    return AdPlacement.HtmlPage;

                case PanelId.ActionHtmlPanelView:
                    return AdPlacement.ActionScreen;
            }

            return AdPlacement.Html5;
        }

        internal void SetWebviewFrame(bool fullScreen, bool useSafeFrame)
        {
            Rect frame = new Rect(0, 0, 0, 0);

            var w = Screen.width;
            var h = Screen.width * 1.5f;
            var x = 0;
            var y = (Screen.height - h) / 2;

            float aspect = (float)Screen.height / (float)Screen.width;

            //if (aspect < 1.777)
            {
                h = Screen.height * 0.8f;
                y = (Screen.height - h) / 2;
            }

#if UNITY_EDITOR
            if (fullScreen)
            {
                frame = new Rect(0, 0, 600, 1200);
            }
            else
            {
                frame = new Rect(0, 0, 600, 800);
            }
#else
            if (fullScreen)
            {
                frame = new Rect(0, 0, Screen.width, Screen.height);
            }
            else
            {
                frame = new Rect(x, y, w, h);
            }
#endif

            _webView.Frame = frame;
            StartCoroutine(SetFrameWithDelay(frame));
        }

        private IEnumerator SetFrameWithDelay(Rect frame)
        {
            yield return new WaitForEndOfFrame();
            _webView.Frame = frame;
        }

        internal void PrepareSurveyPanel(Mission m)
        {
            _rewardWebUrl = "themonetizr.com";
            MonetizrLogger.Print($"currentMissionDesc: {currentMission == null}");
            _webUrl = m.campaignServerSettings.GetParam(m.surveyId);
            _webView.Load(_webUrl);
        }

        internal void PrepareWebViewPanel(Mission m)
        {
            closeButton.gameObject.SetActive(true);
            _webUrl = m.surveyUrl;
            _webView.Load(_webUrl);
        }

        internal void PrepareCongratsNotificationPanel (Mission mission)
        {
            claimButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(true);
            congratsNotificationParent.gameObject.SetActive(true);
            congratsConfirmButton.onClick.AddListener(OnCongratsNotificationConfirmButton);
            closeButton.onClick.AddListener(_OnSkipPress);
        }

        internal void OnCongratsNotificationConfirmButton()
        {
            isSkipped = false;
            closeButton.gameObject.SetActive(false);
            additionalEventValues.Clear();
            DestroyWebView();
        }

        internal void PrepareStartNotificationPanel (Mission mission)
        {
            claimButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(true);
            startNotificationParent.gameObject.SetActive(true);
            confirmButton.onClick.AddListener(OnStartNotificationConfirmButton);
            closeButton.onClick.AddListener(_OnSkipPress);
        }

        internal void OnStartNotificationConfirmButton ()
        {
            MonetizrManager.OnEngagedUserActionComplete();
            isSkipped = false;
            closeButton.gameObject.SetActive(false);
            additionalEventValues.Clear();
            DestroyWebView();
        }

        internal void PrepareActionPanel(Mission m)
        {
            GetActionMissionParameters(m);
            StopCoroutine(ShowCloseButton(_closeButtonDelay));

            closeButton.gameObject.SetActive(false);
            claimButton.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(_webUrl)) MonetizrLogger.PrintError($"ActionReward.url is null");
            _webView.Load(_webUrl);
        }

        internal void GetActionMissionParameters(Mission m)
        {
            _webUrl = m.campaignServerSettings.GetParam("ActionReward.url");
            _rewardWebUrl = m.campaignServerSettings.GetParam("ActionReward.reward_url");
            _claimButtonDelay = m.campaignServerSettings.GetIntParam("ActionReward.reward_time", 0);
            _pagesSwitchesAmount = m.campaignServerSettings.GetIntParam("ActionReward.reward_pages", 0);

            MonetizrLogger.Print("GetActionMissionParameters: " + _claimButtonDelay);

            if (string.IsNullOrEmpty(_webUrl))
            {
                _webUrl = m.campaignServerSettings.GetParam($"ActionReward.{m.serverId}.url");
                _rewardWebUrl = m.campaignServerSettings.GetParam($"ActionReward.{m.serverId}.reward_url");
                _claimButtonDelay = m.campaignServerSettings.GetIntParam($"ActionReward.{m.serverId}.reward_time", 0);
                _pagesSwitchesAmount = m.campaignServerSettings.GetIntParam($"ActionReward.{m.serverId}.reward_pages", 0);
            }
        }

        internal IEnumerator ShowClaimButtonCoroutine(int delay)
        {
            yield return new WaitForSeconds(delay);
            successReason = "timer";
            ShowClaimButton();
        }

        internal void ShowClaimButton()
        {
            if (!claimButton.activeSelf)
            {
                claimButton.SetActive(true);
            }
        }

        private async void PrepareHtml5Panel ()
        {
            MonetizrLogger.Print("Preparing HTML/VIDEO.");

            ServerCampaign campaign = currentMission.campaign;

            if (campaign.campaignType == CampaignType.Fallback && campaign.isDirectVASTinjection)
            {
                await HandleFallbackCampaign(campaign);
                return;
            }

            if (campaign.campaignType != CampaignType.MonetizrBackend && campaign.HasTimeoutPassed())
            {
                MonetizrLogger.Print("CampaignID: " + campaign.id + " video has timed out.");
                HandleProgrammaticFailure(campaign);
                return;
            }

            bool hasProgrammaticVideo = campaign.TryGetAssetInList(new List<string>() {"programmatic_video" }, out Asset programmaticVideoAsset);
            bool hasVideo = campaign.TryGetAssetInList(new List<string>() { "video", "html" }, out Asset videoAsset);
            MonetizrLogger.Print("CampaignID: " + campaign.id + " / hasVideo: " + hasVideo + " / hasProgrammaticVideo: " + hasProgrammaticVideo);

            if (!hasVideo && !hasProgrammaticVideo)
            {
                MonetizrLogger.PrintError("No video asset loaded for CampaignID: " + campaign.id);
                if (campaign.campaignType == CampaignType.Programmatic)
                {
                    HandleProgrammaticFailure(campaign);
                }
                else
                {
                    OnSkipPress();
                }
                return;
            }

            bool showWebview = true;
            VastHelper.VastSettings oldVastSettings = new VastHelper.VastSettings(campaign.vastSettings);
            string userAgent = _webView.GetUserAgent();
            PubmaticHelper ph = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, userAgent);

            if (hasProgrammaticVideo)
            {
                showWebview = AssignProgrammaticVideoAssetPath(campaign, programmaticVideoAsset);
            }
            else
            {
                _webUrl = "file://" + campaign.GetAsset<string>(AssetsType.Html5PathString);
                campaign.vastSettings.videoSettings.videoUrl = videoAsset.url;
            }

            bool verifyWithOMSDK = campaign.serverSettings.GetBoolParam("omsdk.verify_videos", true);
            bool hasDownloaded = false;

            if (!campaign.vastSettings.IsEmpty() && showWebview)
            {
                MonetizrLogger.Print("CampaignID: " + campaign.id + " / Will embed VAST into VideoPlayer.");

                VastTagsReplacer replacer = new VastTagsReplacer(campaign, videoAsset, userAgent);
                campaign.vastSettings.ReplaceVastTags(replacer);
                campaign.vastAdParameters = campaign.DumpsVastSettings(replacer);
                campaign.EmbedVastParametersIntoVideoPlayer(videoAsset);

                if (verifyWithOMSDK)
                {
                    hasDownloaded = await ph.DownloadOMSDKServiceContent();
                    if (hasDownloaded)
                    {
                        ph.InitializeOMSDK(campaign.vastAdParameters);
                        isOMSDKactive = true;
                    }
                }
            }
            else
            {
                MonetizrLogger.PrintError("CampaignID: " + campaign.id + " / VastSettings not empty: " + !campaign.vastSettings.IsEmpty() + " / showWebView: " + showWebview);
            }

            campaign.vastSettings = oldVastSettings;
            if (!hasDownloaded) showWebview = false;

#if UNITY_EDITOR_WIN
            showWebview = false;
#endif

            if (showWebview)
            {
                programmaticStatus = "no_programmatic_or_success";
                _webView.Load(_webUrl);
                _webView.Show();
                MonetizrLogger.Print($"Url to show {_webUrl}");
                MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
                impressionStarts = true;
            }
            else
            {
                HandleProgrammaticFailure(campaign);
            }
        }

        private void HandleProgrammaticFailure(ServerCampaign campaign)
        {
            programmaticStatus = "failed";
            if (campaign.serverSettings.GetBoolParam("openrtb.give_reward_on_programmatic_fail", true))
            {
                OnCompleteEvent();
            }
            else
            {
                _OnSkipPress();
            }
        }

        private bool AssignProgrammaticVideoAssetPath (ServerCampaign campaign, Asset videoAsset)
        {
            Asset programmaticVideoAsset = null;
            bool showWebview = false;
            campaign.vastSettings = new VastHelper.VastSettings();

            if (campaign.TryGetAssetInList("programmatic_video", out programmaticVideoAsset))
            {
                _webUrl = $"file://{campaign.GetCampaignPath($"{programmaticVideoAsset.fpath}/index.html")}";
                videoAsset = programmaticVideoAsset;
                showWebview = true;
            }
            else
            {
                MonetizrLogger.PrintError("No video asset in campaign.");
            }

            return showWebview;
        }

        private IEnumerator ShowCloseButton(float time)
        {
            yield return new WaitForSeconds(time);

            crossButtonAnimator.enabled = true;
        }

        private async Task HandleFallbackCampaign (ServerCampaign campaign)
        {
            MonetizrLogger.Print("CampaignID: " + campaign.id + " / Preparing fallback raw VAST.");
            string indexPath = campaign.GetCampaignPath("index.html");
            campaign.DirectVASTInjectionIntoVideoPlayer();
            bool verifyWithOMSDK = campaign.serverSettings.GetBoolParam("omsdk.verify_videos", true);

            if (verifyWithOMSDK)
            {
                MonetizrLogger.Print("CampaignID: " + campaign.id + " / Will verify with OMSDK.");
                VastHelper vastHelper = new VastHelper(MonetizrManager.Instance.ConnectionsClient, _webView.GetUserAgent());
                bool hasDownloaded = await vastHelper.DownloadOMSDKServiceContent();
                if (hasDownloaded)
                {
                    string omidJson = MonetizrUtils.BuildFromRaw(campaign);
                    vastHelper.InitializeOMSDK(omidJson);
                    isOMSDKactive = true;
                }
            }

            _webUrl = "file://" + indexPath;
            _webView.Load(_webUrl);
            _webView.Show();
            MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Impression);
            TrackingUtils.FireTrackers(campaign.trackingURLs);
            impressionStarts = true;
        }

#if UNI_WEB_VIEW
        private void OnMessageReceived (UniWebView webView, UniWebViewMessage message)
        {
            MonetizrLogger.Print($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");

            if (message.RawMessage.Contains("close"))
            {
                OnCompleteEvent();
            }

            if (message.RawMessage.Contains("skip"))
            {
                _OnSkipPress();
            }

            if (message.RawMessage.Contains("load"))
            {
                _webUrl = _rewardWebUrl;
                _webView.Load(_webUrl);
            }
        }

        private void OnPageStarted (UniWebView webView, string url)
        {
            MonetizrLogger.Print($"OnPageStarted: { url} ");
        }

        private void OnPageFinished (UniWebView webView, int statusCode, string url)
        {
            MonetizrLogger.Print($"OnPageFinished: {url} code: {statusCode}");

            if (url.Contains("monetizr_key=show_ui"))
            {
                _webUrl = url;
                int claimButtonDelay = currentMission.campaignServerSettings.GetIntParam("VideoReward.reward_time", 0);
                _rewardWebUrl = currentMission.campaignServerSettings.GetParam("VideoReward.reward_url");
                
                MonetizrLogger.Print("WILL SHOW CLOSE BUTTON IN: " + claimButtonDelay);

                StartCoroutine(ShowClaimButtonCoroutine(claimButtonDelay));
                StartCoroutine(ShowCloseButton(_closeButtonDelay));
                
                closeButton.gameObject.SetActive(true);
                SetWebviewFrame(false, false);
            }
            
            webView.AddUrlScheme("mntzr");

            if (statusCode >= 300)
            {
                TrackErrorEvent($"{eventsPrefix} error", statusCode);
                successReason = $"error {statusCode}";
                _OnSkipPress();
                return;
            }

            webView.Alpha = 1;

            if (panelId == PanelId.ActionHtmlPanelView)
            {
                MonetizrLogger.Print("WILL INJECT CLAIM BUTTON IN: " + _claimButtonDelay);
                InjectCloseButton();
                Invoke(nameof(InjectClaimButton), _claimButtonDelay);
            }

        }

        private void OnPageErrorReceived (UniWebView webView, int errorCode, string url)
        {
            MonetizrLogger.PrintError($"OnPageErrorReceived: {url} code: {errorCode}");
            TrackErrorEvent($"{eventsPrefix} error", errorCode);
            successReason = $"error {errorCode}";
            _OnSkipPress();
        }
#endif

        private void InjectCloseButton ()
        {
            string defaultButton = "(function(){var e=document.createElement('a');e.href='uniwebview://action?key=skip';e.innerText='×';e.style='position:fixed;top:16px;right:16px;width:40px;height:40px;line-height:40px;text-align:center;text-decoration:none;background:rgba(0,0,0,0.5);color:white;font-size:24px;border-radius:50%;font-weight:bold;z-index:10000;pointer-events:auto;';document.body.appendChild(e);})();";
            string js = defaultButton;

            if (currentMission.campaign.serverSettings.ContainsKey("close_button_html"))
            {
                js = currentMission.campaign.serverSettings.GetParam("close_button_html");
            }
            
            _webView.EvaluateJavaScript(js, payload => {
                MonetizrLogger.Print("Injected close button: " + payload.resultCode);
            });
        }

        private void InjectClaimButton ()
        {
            string defaultButton = "(function(){var e=document.createElement('a');e.href='uniwebview://action?key=close';e.innerText='Claim';e.style='position:fixed;bottom:32px;left:50%;transform:translateX(-50%);z-index:10000;text-decoration:none;background:#28a745;color:white;padding:12px 24px;border-radius:8px;font-size:16px;font-weight:bold;pointer-events:auto;';document.body.appendChild(e);})();";
            string js = defaultButton;

            if (currentMission.campaign.serverSettings.ContainsKey("claim_button_html"))
            {
                js = currentMission.campaign.serverSettings.GetParam("claim_button_html");
            }

            _webView.EvaluateJavaScript(js, payload => {
                MonetizrLogger.Print("Injected claim button: " + payload.resultCode);
            });
        }

        private void OnCompleteEvent ()
        {
            isSkipped = false;
            ClosePanel();
        }

        private void ClosePanel ()
        {
            CancelInvoke(nameof(InjectClaimButton));
            claimButton.SetActive(false);
            closeButton.gameObject.SetActive(false);            
            additionalEventValues.Clear();

            if (panelId == PanelId.ActionHtmlPanelView)
            {
                additionalEventValues.Add("success_reason", successReason);
                additionalEventValues.Add("claim_page_reached", claimPageReached.ToString());
            }

            additionalEventValues.Add("url", _webUrl);
            additionalEventValues.Add("programmatic_status", programmaticStatus);

            bool verifyWithOMSDK = currentMission.campaign.serverSettings.GetBoolParam("omsdk.verify_videos", true);
            if (verifyWithOMSDK && isOMSDKactive)
            {
                MonetizrLogger.Print($"Stopping OMID ad session at time: {Time.time}");
                _webView.StopOMIDAdSession();
                isOMSDKactive = false;
            }

            float time = currentMission.campaignServerSettings.GetFloatParam("omid_destroy_delay", 1.0f);
            if (panelId != PanelId.Html5WebView || !verifyWithOMSDK) time = 0;
            Invoke("DestroyWebView", time);
            triggersButtonEventsOnDeactivate = false;

            if (impressionStarts)
            {
                triggersButtonEventsOnDeactivate = true;
                MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.ImpressionEnds);
                impressionStarts = false;
            }
        }

        private void DestroyWebView ()
        {
            MonetizrLogger.Print($"Destroying webview isSkipped: {isSkipped} current time: {Time.time}");
            Destroy(_webView);
            _webView = null;
            SetActive(false);
        }

        public void OnSkipPress ()
        {
            if (panelId != PanelId.SurveyWebView)
            {
                _OnSkipPress();
                return;
            }

            _webView.Hide(true, UniWebViewTransitionEdge.Top, 0.4f, () =>
            {
                MonetizrManager.ShowMessage((bool _isSkipped) =>
                {
                    if (!_isSkipped)
                    {
                        _OnSkipPress();
                    }
                    else
                    {
                        _webView.Show(true, UniWebViewTransitionEdge.Top, 0.4f);
                    }
                },
                    this.currentMission,
                    PanelId.SurveyCloseConfirmation);

            });

        }

        public void _OnSkipPress ()
        {
            isSkipped = true;
            ClosePanel();
        }

        private void TrackErrorEvent (string eventName, int statusCode = 0)
        {
            if (currentMission.campaignId == null) return;
            Dictionary<string, string> p = new Dictionary<string, string>();
            p.Add("url", _webUrl);
            if (statusCode > 0) p.Add("url_status_code", statusCode.ToString());
            MonetizrManager.Analytics.TrackEvent(currentMission, this, EventType.Error, p);
        }

        internal override bool SendImpressionEventManually ()
        {
            return true;
        }

        internal override AdPlacement? GetAdPlacement ()
        {
            return adType;
        }

        internal override void FinalizePanel (PanelId id)
        {
            MonetizrManager.Instance.SoundSwitch(true);
        }

    }

}