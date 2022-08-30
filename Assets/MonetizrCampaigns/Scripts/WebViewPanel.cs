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
        private bool isAnalyticsNeeded = false;




        //private Action onComplete;
#if UNI_WEB_VIEW
        internal void PrepareWebViewComponent(bool fullScreen)
        {
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
            webView.Frame = new Rect(0,0, 1080.0f*0.9f, 1920.0f*0.9f);
#else
            if(fullScreen)
                webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            else
                webView.Frame = new Rect(x, y, w, h);
#endif

            webView.OnMessageReceived += OnMessageReceived;
            webView.OnPageStarted += OnPageStarted;
            webView.OnPageFinished += OnPageFinished;
            webView.OnPageErrorReceived += OnPageErrorReceived;
        }


        internal void PrepareSurveyPanel(Mission m)
        {
            TrackEvent("Survey started");

            Debug.Log($"currentMissionDesc: {currentMissionDesc == null}");
            webUrl = m.surveyUrl;//MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.SurveyURLString);
                                 // eventsPrefix = "Survey";

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

            isAnalyticsNeeded = false;

        }

        private void PrepareHtml5Panel()
        {
            webUrl = "file://" + MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.Html5PathString);
            // eventsPrefix = "Html5";

            webView.Load(webUrl);
        }

        private void PrepareVideoPanel()
        {
            webUrl = "file://" + MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.VideoFilePathString);

            var htmlFile = MonetizrManager.Instance.GetAsset<string>(currentMissionDesc.campaignId, AssetsType.VideoFilePathString);

            var videoName = Path.GetFileName(htmlFile);

            var page = $@"

<html>
<head>

<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, shrink-to-fit=no'>

<style type='text/css'>
html, body {{
  overflow-x: hidden;
  overflow-y: hidden;
}}
body {{
        overflow: hidden;
        position: relative;
        margin: 0;
        padding: 0;
        background-color: black;
    }}
.video {{
        margin: 0;
        padding: 0;
        top: 0;
        left: 0;
        width: 100%;
        height: auto;
        object-fit: cover;
        min-width: 100%;
    }}
.videoBg {{
        display: flex;  
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        z-index: 10;
    }}
.skipButton
{{
    position: absolute;
    top: 15px;
    left: 15px;

    /*background-color: rgba(0,255, 255, 0.5);*/
    
    /*background-image: url('zzz.png');*/
    width: 40px;
    height: 40px;
    z-index:500;
    border-radius: 50%;
    opacity: 0.5;
}}
.countdown
{{
    position: absolute;
    top: 15px;
    right: 15px;
    background-color: rgba(255,255, 255, 0.5);
    border: none;
    color: rgba(0,0, 0, 0.5);
    width: 40px;
    height: 40px;
    line-height: 40px;
    text-align: center;
    text-decoration: none;
    display: inline-block;
    font-size: 20px;
    border-radius: 50%;
    z-index:500;
    font-family: Verdana, sans-serif;
}}

.toastInfo
{{
    position: absolute;
    bottom: 0px;
    left: 0;
    right: 0;
    margin: auto;
    background-color: rgba(255, 255, 255, 0.5);
    border: none;
    
    width: 100%;
    height: 28px;
    text-align: center;
    display: inline-block;
    font-size: 18px;
    padding-top: 4px;
    color: rgba(128,128, 128, 0.75);
    font-family: Verdana, sans-serif;
    
    z-index:500;
}}

.text
{{
    display: none;
}}

</style>

</head>

<body>

    <img src='close_button.png' draggable='false' onclick='skipHandler()' type='button' class='skipButton'>x</button>
    <div class='countdown' id='videoTimer'></div>
    <div class='toastInfo'>Watch a full video to get a reward!</div>
<div class='videoBg'>
    <video poster='noposter' class='video' playsinline autoplay webkit-playsinline disablePictureInPicture controlsList='nodownload nofullscreen noremoteplayback' id='myVideo' onEnded='endHandler()'>
        <source src = '{videoName}' type = 'video/mp4'/>
        Your browser does not support the video tag.
    </video>
</div>

<script>

    var counter = {{
        // HELPER - CREATE MIN/SEC CELL
        // txt : text for the cell (all small letters)
        square : (txt) => {{
            let cell = document.createElement('div');
            cell.className = `cell ${{txt}}`;
            cell.innerHTML = `<div class=""digits"">0</div><div class=""text""></div>`;
            return cell;
        }},

        // INITIALIZE COUNTDOWN TIMER
        //  target : target html container
        //  remain : seconds to countdown
        //  after : function, do this when countdown end (optional)
        attach : (instance) => {{
            // (B1) GENERATE HTML
            instance.target.className = 'countdown';
            if (instance.remain >= 60) {{
                instance.target.appendChild(counter.square('mins'));
                instance.mins = instance.target.querySelector('.mins .digits');
            }}
instance.target.appendChild(counter.square('secs'));
instance.secs = instance.target.querySelector('.secs .digits');

// TIMER
instance.timer = setInterval(() => {{ counter.ticker(instance); }}, 1000);
        }},

        // COUNTDOWN TICKER
        ticker: (instance) => {{
            // TIMER STOP
            instance.remain--;
            if (instance.remain <= 0)
            {{
                clearInterval(instance.timer);
                instance.remain = 0;
                if (typeof instance.after == 'function') {{ instance.after(); }}
            }}

            // CALCULATE REMAINING MINS/SECS
            // 1 min = 60 secs
            let secs = instance.remain;
            let mins = Math.floor(secs / 60);
            secs -= mins * 60;

            // (C3) UPDATE HTML
            instance.secs.innerHTML = secs;
            if (instance.mins !== undefined) {{ instance.mins.innerHTML = mins; }}
        }},

        // CONVERT DATE/TIME TO REMAINING SECONDS
        toSecs: (till) => {{
            till = Math.floor(till / 1000);
            let remain = till - Math.floor(Date.now() / 1000);
            return remain < 0 ? 0 : remain;
        }}
    }};


function endHandler()
{{
    location.href = 'uniwebview://action?key=close';
}}

function skipHandler()
{{
    location.href = 'uniwebview://action?key=skip';
}}

document.addEventListener('DOMContentLoaded', function(){{
    var video = document.getElementById('myVideo');

    // Assume 'video' is the video node
    var i = setInterval(function() {{
        if (video.readyState > 0)
        {{
            var seconds = Math.round(video.duration % 60);

            // (Put the minutes and seconds in the display)
            counter.attach({{
            target: document.getElementById('videoTimer'),
                    remain: seconds
                }});
            clearInterval(i);
        }}
    }}, 200);

}});

</script>
</body>";


            htmlFile = Path.GetDirectoryName(htmlFile) + "/" + Path.GetFileNameWithoutExtension(htmlFile) + ".html";

            Debug.Log("----------------" + htmlFile);

            if (File.Exists(htmlFile))
                File.Delete(htmlFile);

            File.WriteAllBytes(htmlFile, Encoding.ASCII.GetBytes(page));

            var closeButtonFileName = Path.GetDirectoryName(htmlFile) + "/" + "close_button.png";

            if (!File.Exists(closeButtonFileName))
            {
                File.WriteAllBytes(closeButtonFileName, closeButtonImageAsset.bytes);
            }


            webView.Load("file://"+htmlFile);

           // eventsPrefix = "Video";
        }
#endif

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
#if UNI_WEB_VIEW
            this.onComplete = onComplete;
            panelId = id;
            currentMissionDesc = m;

            bool fullScreen = true;

            if (id == PanelId.SurveyWebView || id == PanelId.HtmlWebPageView)
                fullScreen = false;

            PrepareWebViewComponent(fullScreen);

            closeButton.gameObject.SetActive(!fullScreen);
                        
            switch (id)
            {
                case PanelId.SurveyWebView: adType = AdType.Survey; PrepareSurveyPanel(m); break;
                case PanelId.VideoWebView: adType = AdType.Video; PrepareVideoPanel(); break;
                case PanelId.Html5WebView: adType = AdType.Html5; PrepareHtml5Panel(); break;
                case PanelId.HtmlWebPageView: adType = AdType.HtmlPage; PrepareWebViewPanel(m); break;
            }

            eventsPrefix = MonetizrAnalytics.adTypeNames[adType];

            // Load a URL.
            Debug.Log($"Url to show {webUrl}");
            webView.Show();

            if(isAnalyticsNeeded)
                MonetizrManager.Analytics.BeginShowAdAsset(adType, currentMissionDesc);
#endif
        }


#if UNI_WEB_VIEW
        void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            Debug.Log($"OnMessageReceived: {message.RawMessage} {message.Args.ToString()}");

            if(message.RawMessage.Contains("close"))
            {
                OnCompleteEvent();

                ClosePanel();
            }

            if (message.RawMessage.Contains("skip"))
            {
                OnSkipPress();

                ClosePanel();
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
            }
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


                    if (webUrl.Contains("withdraw-consent") ||
                        webUrl.Contains("reportabuse") ||
                        webUrl.Contains("google.com/forms/about"))
                    {
                        OnSkipPress();
                        return;
                    }


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
            isSkipped = true;

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