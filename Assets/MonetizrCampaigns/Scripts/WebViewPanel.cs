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
        private Mission currentMissionDesc;
        private string eventsPrefix;
        private AdType adType;
        private bool isAnalyticsNeeded = true;
        public Image background;


        //private Action onComplete;
#if UNI_WEB_VIEW
        internal void PrepareWebViewComponent(bool fullScreen, bool useSafeFrame)
        {


#if UNITY_EDITOR
    fullScreen = false;
#endif

            UniWebView.SetAllowAutoPlay(true);
            UniWebView.SetAllowInlinePlay(true);
            UniWebView.SetWebContentsDebuggingEnabled(true);

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

            Debug.Log($"frame: {fullScreen} {webView.Frame}");

            webView.OnMessageReceived += OnMessageReceived;
            webView.OnPageStarted += OnPageStarted;
            webView.OnPageFinished += OnPageFinished;
            webView.OnPageErrorReceived += OnPageErrorReceived;

            webView.Alpha = 0;
        }
#endif


        internal void PrepareSurveyPanel(Mission m)
        {
            

            TrackEvent("Survey started");

            Debug.Log($"currentMissionDesc: {currentMissionDesc == null}");
            //webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
            // eventsPrefix = "Survey";

            webUrl = m.campaignServerSettings.GetParam(m.surveyId);

            webView.Load(webUrl);

        }

        internal void PrepareWebViewPanel(Mission m)
        {
            //MonetizrManager.Analytics.TrackEvent("Survey webview", currentMissionDesc);

            closeButton.gameObject.SetActive(true);

            //Debug.Log($"currentMissionDesc: {currentMissionDesc == null}");
            webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
                                 // eventsPrefix = "Survey";

            webView.Load(webUrl);

            //isAnalyticsNeeded = false;

        }

        private void PrepareHtml5Panel()
        {
            
            webUrl = "file://" + MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.Html5PathString);
            // eventsPrefix = "Html5";

            webView.Load(webUrl);
        }
               

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
#if UNI_WEB_VIEW
            this.onComplete = onComplete;
            panelId = id;
            currentMissionDesc = m;

            bool fullScreen = true;
            bool useSafeFrame = false;

            if (id == PanelId.HtmlWebPageView)
                fullScreen = false;

            if (id == PanelId.SurveyWebView)
            {
                fullScreen = false;
                useSafeFrame = true;
            }

            PrepareWebViewComponent(fullScreen, useSafeFrame);

            closeButton.gameObject.SetActive(!fullScreen);

            if (id == PanelId.Html5WebView)
                background.color = Color.black;
            else
                background.color = Color.white;

            switch (id)
            {
                case PanelId.SurveyWebView: adType = AdType.Survey; PrepareSurveyPanel(m); break;
                //case PanelId.VideoWebView: adType = AdType.Video; PrepareVideoPanel(); break;
                case PanelId.Html5WebView: adType = AdType.Html5; PrepareHtml5Panel(); break;
                case PanelId.HtmlWebPageView: adType = AdType.HtmlPage; PrepareWebViewPanel(m); break;
            }

            //eventsPrefix = MonetizrAnalytics.adTypeNames[adType];

            eventsPrefix = adType.ToString();

            MonetizrManager.CallUserDefinedEvent(m.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);

            // Load a URL.
            Debug.Log($"Url to show {webUrl}");
            webView.Show();

            if (isAnalyticsNeeded)
            {
                TrackEvent($"{eventsPrefix} started");
                MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMissionDesc);
            }
#endif
        }


#if UNI_WEB_VIEW
        void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            Debug.Log($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");

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
            Debug.Log($"OnPageStarted: { url} ");
        }

        void OnPageFinished(UniWebView webView, int statusCode, string url)
        {
            Debug.Log($"OnPageFinished: {url} code: {statusCode}");

            webView.AddUrlScheme("mntzr");

            if (statusCode >= 300)
            {
                TrackEvent($"{eventsPrefix} error");

                ClosePanel();

                return;
            }

            webView.Alpha = 1;
        }

        void OnPageErrorReceived(UniWebView webView, int errorCode, string url)
        {
            Debug.Log($"OnPageErrorReceived: {url} code: {errorCode}");

            TrackEvent($"{eventsPrefix} error");

            ClosePanel();
        }

        private void Update()
        {
            if (webView != null && panelId == PanelId.SurveyWebView)
            {
                var currentUrl = webView.Url;

                if (!webUrl.Equals(currentUrl))
                {
                    webUrl = currentUrl;
                    Debug.Log("Update: " + webView.Url);


                    /* if (webUrl.Contains("withdraw-consent") ||
                        webUrl.Contains("reportabuse") ||
                        webUrl.Contains("google.com/forms/about"))
                    {
                        OnSkipPress();
                        return;
                    }*/


                    if (webUrl.Contains("end-page-gateway") ||
                        webUrl.Contains("themonetizr.com") ||
                        webUrl.Contains("uniwebview"))
                    {
                        OnCompleteEvent();
                        return;
                    }

                }
            }
        }


        private void OnCompleteEvent()
        {
            MonetizrManager.CallUserDefinedEvent(currentMissionDesc.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressOk);

            TrackEvent($"{eventsPrefix} completed");
            isSkipped = false;

            ClosePanel();

        }

        private void ClosePanel()
        {
            Debug.Log($"Closing webview isSkipped: {isSkipped}");
            Destroy(webView);
            webView = null;

            SetActive(false);
        }


        private new void Awake()
        {
            base.Awake();


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
                    this.currentMissionDesc,
                    PanelId.SurveyCloseConfirmation);

            });
            
        }

        public void _OnSkipPress()
        {
            isSkipped = true;

            MonetizrManager.CallUserDefinedEvent(currentMissionDesc.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressSkip);

            TrackEvent($"{eventsPrefix} skipped");

            ClosePanel();
        }


#endif
        private void TrackEvent(string eventName)
        {
            if (!isAnalyticsNeeded)
                return;

            MonetizrManager.Analytics.TrackEvent(eventName, currentMissionDesc);
        }


        internal override void FinalizePanel(PanelId id)
        {
            if (isAnalyticsNeeded)
                MonetizrManager.Analytics.EndShowAdAsset(adType, currentMissionDesc);

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