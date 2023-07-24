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
        private UniWebView webView;
#endif
        private string webUrl;
        private string rewardWebUrl;
        //private Mission currentMission;
        private string eventsPrefix;
        private AdPlacement adType;
        private bool isAnalyticsNeeded = true;
        public Image background;

        public GameObject claimButton;

        public Animator crossButtonAnimator;

        private int pagesSwitch = -1;

        public string successReason;
        public bool claimPageReached = false;

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
            UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;
#endif
   
            UniWebView.SetAllowAutoPlay(true);
            UniWebView.SetAllowInlinePlay(true);

#if UNITY_EDITOR
            UniWebView.SetWebContentsDebuggingEnabled(true);
#endif

            UniWebView.SetJavaScriptEnabled(true);
            UniWebView.SetAllowUniversalAccessFromFileURLs(true);


            MonetizrManager.Instance.SoundSwitch(false);

            webView = gameObject.AddComponent<UniWebView>();

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
            webView.Frame = new Rect(0,0, 600, 800);
#else
            if (fullScreen)
            {
                //webView.Frame = useSafeFrame ? Screen.safeArea : new Rect(0, 0, Screen.width, Screen.height);
                webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            }
            else

                webView.Frame = new Rect(x, y, w, h);
#endif

            Log.Print($"frame: {fullScreen} {webView.Frame}");

            webView.OnMessageReceived += OnMessageReceived;
            webView.OnPageStarted += OnPageStarted;
            webView.OnPageFinished += OnPageFinished;
            webView.OnPageErrorReceived += OnPageErrorReceived;

            webView.Alpha = 0;
        }
#endif


        internal void PrepareSurveyPanel(Mission m)
        {
            rewardWebUrl = "themonetizr.com";

            //TrackEvent("Survey started");

            Log.Print($"currentMissionDesc: {currentMission == null}");
            //webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
            // eventsPrefix = "Survey";

            webUrl = m.campaignServerSettings.GetParam(m.surveyId);

            webView.Load(webUrl);

        }

        internal void PrepareWebViewPanel(Mission m)
        {
            //MonetizrManager.Analytics.TrackEvent("Survey webview", currentMissionDesc);

            closeButton.gameObject.SetActive(true);

            //Log.Print($"currentMissionDesc: {currentMissionDesc == null}");
            webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
                                 // eventsPrefix = "Survey";

            webView.Load(webUrl);

            //isAnalyticsNeeded = false;

        }

        internal void PrepareActionPanel(Mission m)
        {
            //MonetizrManager.Analytics.TrackEvent("Survey webview", currentMissionDesc);

            closeButton.gameObject.SetActive(true);

            //Log.Print($"currentMissionDesc: {currentMissionDesc == null}");
            //webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
                                 // eventsPrefix = "Survey";

            webUrl = m.campaignServerSettings.GetParam("ActionReward.url");

            if(string.IsNullOrEmpty(webUrl))
            {
                Log.PrintError($"ActionReward.url is null");
            }

            rewardWebUrl = m.campaignServerSettings.GetParam("ActionReward.reward_url");

            webView.Load(webUrl);

            //isAnalyticsNeeded = false;

            int delay = m.campaignServerSettings.GetIntParam("ActionReward.reward_time", 0);

            StartCoroutine(ShowClaimButtonCoroutine(delay));

            pagesSwitch = m.campaignServerSettings.GetIntParam("ActionReward.reward_pages", 0);
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



        private void PrepareHtml5Panel()
        {
            
            webUrl = "file://" + currentMission.campaign.GetAsset<string>(AssetsType.Html5PathString);
            // eventsPrefix = "Html5";

            webView.Load(webUrl);
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

            if (id == PanelId.Html5WebView)
                background.color = Color.black;
            else
                background.color = Color.white;

            switch (id)
            {
                case PanelId.SurveyWebView: adType = AdPlacement.Survey; PrepareSurveyPanel(m); break;
                //case PanelId.VideoWebView: adType = AdType.Video; PrepareVideoPanel(); break;
                case PanelId.Html5WebView: adType = AdPlacement.Html5; PrepareHtml5Panel(); break;
                case PanelId.HtmlWebPageView: adType = AdPlacement.HtmlPage; PrepareWebViewPanel(m); break;
                case PanelId.ActionHtmlPanelView: adType = AdPlacement.ActionScreen; PrepareActionPanel(m); break;
            }

            //eventsPrefix = MonetizrAnalytics.adTypeNames[adType];

            eventsPrefix = adType.ToString();

            //MonetizrManager.CallUserDefinedEvent(m.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);

            // Load a URL.
            Log.Print($"Url to show {webUrl}");
            webView.Show();

            if (isAnalyticsNeeded)
            {
                //TrackEvent($"{eventsPrefix} started");
                //MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMissionDesc);
            }
#endif
        }

        IEnumerator ShowCloseButton(float time)
        {
            yield return new WaitForSeconds(time);

            crossButtonAnimator.enabled = true;
        }

#if UNI_WEB_VIEW
        void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            Log.Print($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");

            if(message.RawMessage.Contains("close"))
            {
                OnCompleteEvent();

                //ClosePanel();
            }

            if (message.RawMessage.Contains("skip"))
            {
                OnSkipPress();

                //ClosePanel();
            }
        }

        void OnPageStarted(UniWebView webView, string url)
        {
            Log.Print($"OnPageStarted: { url} ");
        }

        void OnPageFinished(UniWebView webView, int statusCode, string url)
        {
            Log.Print($"OnPageFinished: {url} code: {statusCode}");

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
            Log.Print($"OnPageErrorReceived: {url} code: {errorCode}");

            TrackErrorEvent($"{eventsPrefix} error", errorCode);

            successReason = $"error {errorCode}";

            _OnSkipPress();
        }

        private void Update()
        {
            bool panelForCheckUrl = (panelId == PanelId.SurveyWebView) || 
                                    (panelId == PanelId.ActionHtmlPanelView);
                
            if (webView != null && panelForCheckUrl)
            {
                var currentUrl = webView.Url;
                
                if (!webUrl.Equals(currentUrl))
                {
                    webUrl = currentUrl;
                    pagesSwitch--;
                    
                    Log.Print($"Update: {webUrl} {pagesSwitch}");

                    if(pagesSwitch == 0)
                    {
                        successReason = "page_switch";

                        ShowClaimButton();
                    }

                    if (/*webUrl.Contains("themonetizr.com") ||*/
                        webUrl.Contains("uniwebview") ||
                        webUrl.Contains(rewardWebUrl))
                    {
                        if (webUrl.Contains(rewardWebUrl))
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

            Log.Print($"Stopping OMID ad session at time: {Time.time}"); 

            webView.StopOMIDAdSession();

            float time = currentMission.campaignServerSettings.GetFloatParam("omid_destroy_delay",1.0f);

            if (panelId != PanelId.Html5WebView)
                time = 0;

            Invoke("DestroyWebView", time);
        }

        private void DestroyWebView()
        {
            Log.Print($"Destroying webview isSkipped: {isSkipped} current time: {Time.time}");

            Destroy(webView);
            webView = null;

            SetActive(false);
        }


        private new void Awake()
        {
            base.Awake();


        }

       
        public void OnClaimRewardPress()
        {
            Log.Print("OnClaimRewardPress");

            OnCompleteEvent();
        }


        public void OnSkipPress()
        {
            if (panelId != PanelId.SurveyWebView)
            {
                _OnSkipPress();
                return;
            }


            webView.Hide(true, UniWebViewTransitionEdge.Top, 0.4f, () =>
            {
                MonetizrManager.ShowMessage((bool _isSkipped) =>
                {
                    if (!_isSkipped)
                    {
                        _OnSkipPress();
                    }
                    else
                    {
                        webView.Show(true, UniWebViewTransitionEdge.Top, 0.4f);
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

            p.Add("url", webUrl);
            
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



        //// Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }

}