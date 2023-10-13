#define UNI_WEB_VIEW

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//using UniWebView;

namespace Monetizr.Campaigns
{
    internal class WebViewPanel : PanelController
    {
        public TextAsset closeButtonImageAsset;
        public Button closeButton;
#if UNI_WEB_VIEW
        private UniWebView _webView = null;
#endif
        private string _webUrl;
        private string _rewardWebUrl;
        //private Mission currentMission;
        private string eventsPrefix;
        private AdPlacement adType;
        private bool isAnalyticsNeeded = true;
        public Image background;

        public GameObject claimButton;

        public Animator crossButtonAnimator;

        private int _pagesSwitchesAmount = -1;

        public string successReason;
        public bool claimPageReached = false;
        public string programmaticStatus;
        private int _claimButtonDelay;

        internal override AdPlacement? GetAdPlacement()
        {
            return adType;
        }

        //private Action _onComplete;
#if UNI_WEB_VIEW
        internal void PrepareWebViewComponent(bool fullScreen, bool useSafeFrame)
        {


#if UNITY_EDITOR
            fullScreen = false;
            //UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;
#endif

            if (Log.isVerbose)
                UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;


            UniWebView.SetAllowAutoPlay(true);
            UniWebView.SetAllowInlinePlay(true);

#if UNITY_EDITOR
            UniWebView.SetWebContentsDebuggingEnabled(true);
#endif

            UniWebView.SetJavaScriptEnabled(true);
            UniWebView.SetAllowUniversalAccessFromFileURLs(true);


            MonetizrManager.Instance.SoundSwitch(false);

            _webView = gameObject.AddComponent<UniWebView>();

            var w = Screen.width;
            var h = Screen.width * 1.5f;
            var x = 0;
            var y = (Screen.height - h) / 2;

            float aspect = (float)Screen.height / (float)Screen.width;


            if (aspect < 1.777)
            {
                h = Screen.height * 0.8f;
                y = (Screen.height - h) / 2;
            }

#if UNITY_EDITOR
            _webView.Frame = new Rect(0,0, 600, 800);
#else
            if (fullScreen)
            {
                //webView.Frame = useSafeFrame ? Screen.safeArea : new Rect(0, 0, Screen.width, Screen.height);
                _webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            }
            else
            {
                _webView.Frame = new Rect(x, y, w, h);
            }
#endif

            Log.Print($"frame: {fullScreen} {_webView.Frame}");

            _webView.OnMessageReceived += OnMessageReceived;
            _webView.OnPageStarted += OnPageStarted;
            _webView.OnPageFinished += OnPageFinished;
            _webView.OnPageErrorReceived += OnPageErrorReceived;

            _webView.Alpha = 0;
        }
#endif


        internal void PrepareSurveyPanel(Mission m)
        {
            _rewardWebUrl = "themonetizr.com";

            //TrackEvent("Survey started");

            Log.PrintV($"currentMissionDesc: {currentMission == null}");
            //webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
            // eventsPrefix = "Survey";

            _webUrl = m.campaignServerSettings.GetParam(m.surveyId);

            _webView.Load(_webUrl);

        }

        internal void PrepareWebViewPanel(Mission m)
        {
            //MonetizrManager.Analytics.TrackEvent("Survey webview", currentMissionDesc);

            closeButton.gameObject.SetActive(true);

            //Log.Print($"currentMissionDesc: {currentMissionDesc == null}");
            _webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
                                 // eventsPrefix = "Survey";

            _webView.Load(_webUrl);

            //isAnalyticsNeeded = false;

        }

        internal void GetActionMissionParameters(Mission m)
        {
            _webUrl = m.campaignServerSettings.GetParam("ActionReward.url");
            _rewardWebUrl = m.campaignServerSettings.GetParam("ActionReward.reward_url");
            _claimButtonDelay = m.campaignServerSettings.GetIntParam("ActionReward.reward_time", 0);
            _pagesSwitchesAmount = m.campaignServerSettings.GetIntParam("ActionReward.reward_pages", 0);

            if (string.IsNullOrEmpty(_webUrl))
            {
                _webUrl = m.campaignServerSettings.GetParam($"ActionReward.{m.serverId}.url");
                _rewardWebUrl = m.campaignServerSettings.GetParam($"ActionReward.{m.serverId}.reward_url");
                _claimButtonDelay = m.campaignServerSettings.GetIntParam($"ActionReward.{m.serverId}.reward_time", 0);
                _pagesSwitchesAmount = m.campaignServerSettings.GetIntParam($"ActionReward.{m.serverId}.reward_pages", 0);
            }
        }

        internal void PrepareActionPanel(Mission m)
        {
            //MonetizrManager.Analytics.TrackEvent("Survey webview", currentMissionDesc);

            closeButton.gameObject.SetActive(true);

            GetActionMissionParameters(m);

            if(string.IsNullOrEmpty(_webUrl))
            {
                Log.PrintError($"ActionReward.url is null");
            }

            _webView.Load(_webUrl);

            //isAnalyticsNeeded = false;
            
#if UNITY_EDITOR
            _claimButtonDelay = 3;
#endif

            StartCoroutine(ShowClaimButtonCoroutine(_claimButtonDelay));

            
        }

        internal IEnumerator ShowClaimButtonCoroutine(int delay)
        {
            yield return new WaitForSeconds(delay);

            successReason = "timer";

            ShowClaimButton();
        }

        internal void ShowClaimButton()
        {
            if(!claimButton.activeSelf)
                claimButton.SetActive(true);
        }



        private async void PrepareHtml5Panel()
        {
            var campaign = currentMission.campaign;

            bool hasVideo = campaign.HasAsset(AssetsType.Html5PathString);

            _webUrl = "file://" + campaign.GetAsset<string>(AssetsType.Html5PathString);

            var hasProgrammatic = campaign.serverSettings.GetBoolParam("programmatic", false);

            if (!hasProgrammatic && !hasVideo)
            {
                Log.PrintError($"Video expected, but didn't loaded for campaign {campaign.id}");
                _OnSkipPress();
                return;
            }
            
            var showWebview = true;
            //var isSkipped = false;

            if (hasProgrammatic)
            {
                showWebview = false;
                var ph = new PubmaticHelper(MonetizrManager.Instance.Client, _webView.GetUserAgent());

                var programmaticOk = await ph.GetOpenRtbResponseForCampaign(campaign);
                
                if (campaign.TryGetAssetInList("programmatic_video", out var videoAsset))
                {
                    _webUrl = $"file://{campaign.GetCampaignPath($"/fpath/index.html")}";
                    showWebview = true;
                }
            }
#if UNITY_EDITOR_WIN
                showWebview = false;
#endif
            if (showWebview)
            {
                programmaticStatus = "no_programmatic_or_success";
                _webView.Load(_webUrl);
                _webView.Show();
                Log.PrintV($"Url to show {_webUrl}");
            }
            else
            {
                programmaticStatus = "failed";
                //OnCompleteEvent();
                
                //Log.PrintError("_OnSkipPress");
                _OnSkipPress();
            }

        }
               

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
#if UNI_WEB_VIEW
            this._onComplete = onComplete;
            panelId = id;
            currentMission = m;
            
            bool fullScreen = true;
            bool useSafeFrame = false;

            if (id == PanelId.HtmlWebPageView)
                fullScreen = false;

            if (id == PanelId.SurveyWebView)
            {
                fullScreen = false;
                useSafeFrame = true;
            }

            if(id == PanelId.ActionHtmlPanelView)
            {
                fullScreen = false;
            }

            claimButton.SetActive(false);

            PrepareWebViewComponent(fullScreen, useSafeFrame);

            closeButton.gameObject.SetActive(!fullScreen);

            int closeButtonDelay = m.campaignServerSettings.GetIntParam("email_enter_close_button_delay", 0);

            StartCoroutine(ShowCloseButton(closeButtonDelay));

            background.color = id == PanelId.Html5WebView ? Color.black : Color.white;

            adType = getAdPlacement();

            eventsPrefix = adType.ToString();

            switch (id)
            {
                case PanelId.SurveyWebView: 
                    PrepareSurveyPanel(m); 
                    break;
                
                case PanelId.HtmlWebPageView: 
                    PrepareWebViewPanel(m); 
                    break;

                case PanelId.ActionHtmlPanelView: 
                    PrepareActionPanel(m); 
                    break;
            }

        
            if (!string.IsNullOrEmpty(_webUrl))
            {
                // Load a URL.
                Log.PrintV($"Url to show {_webUrl}");
                _webView.Show();
            }
            
            //Show it separately because it async method
            if(id == PanelId.Html5WebView)
                PrepareHtml5Panel();
            
#endif
        }

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

        IEnumerator ShowCloseButton(float time)
        {
            yield return new WaitForSeconds(time);

            crossButtonAnimator.enabled = true;
        }

#if UNI_WEB_VIEW
        void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            Log.PrintV($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");

            if(message.RawMessage.Contains("close"))
            {
                OnCompleteEvent();

                //ClosePanel();
            }

            if (message.RawMessage.Contains("skip"))
            {
                _OnSkipPress();

                //ClosePanel();
            }
        }

        void OnPageStarted(UniWebView webView, string url)
        {
            Log.PrintV($"OnPageStarted: { url} ");
        }

        void OnPageFinished(UniWebView webView, int statusCode, string url)
        {
            Log.PrintV($"OnPageFinished: {url} code: {statusCode}");

            webView.AddUrlScheme("mntzr");

            if (statusCode >= 300)
            {
                TrackErrorEvent($"{eventsPrefix} error", statusCode);

                successReason = $"error {statusCode}";

               _OnSkipPress();

                return;
            }

            webView.Alpha = 1;
        }

        void OnPageErrorReceived(UniWebView webView, int errorCode, string url)
        {
            Log.PrintError($"OnPageErrorReceived: {url} code: {errorCode}");

            TrackErrorEvent($"{eventsPrefix} error", errorCode);

            successReason = $"error {errorCode}";

            _OnSkipPress();
        }

        private void Update()
        {
            bool panelForCheckUrl = (panelId == PanelId.SurveyWebView) || 
                                    (panelId == PanelId.ActionHtmlPanelView);
                
            if (_webView != null && panelForCheckUrl)
            {
                var currentUrl = _webView.Url;
                
                if (!_webUrl.Equals(currentUrl))
                {
                    _webUrl = currentUrl;
                    _pagesSwitchesAmount--;
                    
                    Log.PrintV($"Update: {_webUrl} {_pagesSwitchesAmount}");

                    if(_pagesSwitchesAmount == 0)
                    {
                        successReason = "page_switch";

                        ShowClaimButton();
                    }

                    if (/*webUrl.Contains("themonetizr.com") ||*/
                        _webUrl.Contains("uniwebview") ||
                        _webUrl.Contains(_rewardWebUrl))
                    {
                        if (_webUrl.Contains(_rewardWebUrl))
                        {
                            successReason = "reward_page_reached";
                            claimPageReached = true;
                        }

                        OnCompleteEvent();
                        return;
                    }

                }
            }
        }


        private void OnCompleteEvent()
        {
            //MonetizrManager.CallUserDefinedEvent(currentMissionDesc.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressOk);

            //TrackEvent($"{eventsPrefix} completed");
            isSkipped = false;
                       
            
            ClosePanel();

        }

        private void ClosePanel()
        {
            additionalEventValues.Clear();

            if (panelId == PanelId.ActionHtmlPanelView)
            {
                additionalEventValues.Add("success_reason", successReason);
                additionalEventValues.Add("claim_page_reached", claimPageReached.ToString());
            }

            additionalEventValues.Add("url", _webUrl);
            additionalEventValues.Add("programmatic_status", programmaticStatus);

            Log.PrintV($"Stopping OMID ad session at time: {Time.time}"); 

            _webView.StopOMIDAdSession();

            float time = currentMission.campaignServerSettings.GetFloatParam("omid_destroy_delay",1.0f);

            if (panelId != PanelId.Html5WebView)
                time = 0;

            Invoke("DestroyWebView", time);
        }

        private void DestroyWebView()
        {
            Log.PrintV($"Destroying webview isSkipped: {isSkipped} current time: {Time.time}");

            Destroy(_webView);
            _webView = null;

            SetActive(false);
        }


        private new void Awake()
        {
            base.Awake();


        }

       
        public void OnClaimRewardPress()
        {
            Log.PrintV("OnClaimRewardPress");

            OnCompleteEvent();
        }


        public void OnSkipPress()
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

        public void _OnSkipPress()
        {
            //additionalEventValues.Clear();

            isSkipped = true;

            //MonetizrManager.CallUserDefinedEvent(currentMissionDesc.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressSkip);

            //TrackEvent($"{eventsPrefix} skipped");

            ClosePanel();
        }


#endif
        private void TrackErrorEvent(string eventName, int statusCode = 0)
        {
            //if (!isAnalyticsNeeded)
            //    return;

            if (currentMission.campaignId == null)
                return;

            Dictionary<string, string> p = new Dictionary<string, string>();

            p.Add("url", _webUrl);
            
            if(statusCode > 0)
                p.Add("url_status_code", statusCode.ToString());

            //var campaign = MonetizrManager.Instance.GetCampaign(currentMission.campaignId);

            MonetizrManager.Analytics.TrackEvent(currentMission, this, MonetizrManager.EventType.Error, p);

            //MonetizrManager.Analytics.TrackEvent(eventName, currentMissionDesc);
        }


        internal override void FinalizePanel(PanelId id)
        {
            //if (isAnalyticsNeeded)
            //    MonetizrManager.Analytics.EndShowAdAsset(adType, currentMissionDesc);

            MonetizrManager.Instance.SoundSwitch(true);
        }

    }

}