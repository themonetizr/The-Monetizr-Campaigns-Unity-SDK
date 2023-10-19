using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using Monetizr.Campaigns;
using UnityEngine;


namespace Monetizr.Campaigns
{

    public class MonetizrWebViewBanner : MonoBehaviour
    {
        private UniWebView _webView;

        // Start is called before the first frame update
        void Start()
        {
            PrepareBanner();
        }

        private async void PrepareBanner()
        {
            var currentCampaign = MonetizrManager.Instance.GetActiveCampaign();

            var uri = "http://openbid.pubmatic.com/translator?pubId=163063"; //globalSettings.GetParam("openrtb.endpoint");
            var openRtbRequest = "{\"id\": \"${CAMP_ID}\", \"at\": 1, \"tmax\": 500, \"test\": 1, \"device\": {\"ip\": \"${DEVICE_IP}\", \"ua\": \"${USER_AGENT}\", \"carrier\": \"\", \"os\": \"${DEVICE_OS}\", \"lmt\": \"${OPT_OUT}\", \"osv\": \"${OS_VERSION}\", \"ifa\": \"${ADVERTISING_ID}\"}, \"imp\": [{\"id\": \"1\", \"tagid\": \"5231582\", \"bidfloor\": 1.0, \"secure\": 1, " +
                                 "\"banner\": {\"w\": 300, \"h\": 300}, \"pmp\": {\"private_auction\": 1, \"deals\": [{\"id\": \"PM-XQSO-4604\", \"at\": 1, \"bidfloor\": 1.0, \"bidfloorcur\": \"USD\"}]}}], \"source\": {\"schain\": {\"complete\": 1, \"nodes\": [{\"sid\": \"28632c8b-9f61-4c23-aaa3-a040cb342b3a\", \"asi\": \"themonetizr.com\", \"hp\": 1}, {\"sid\": \"163063\", \"asi\": \"pubmatic.com\", \"hp\": 1}], \"ver\": \"1.0\"}}, \"app\": {\"id\": \"1063264\", \"name\": \"Idle Workout Master: Boxbun\", \"bundle\": \"com.nextsol.workout.master\", \"storeurl\": \"https://play.google.com/store/apps/details?id=com.nextsol.workout.master\"}}";

            var requestMessage = MonetizrClient.GetOpenRtbRequestMessage(uri, openRtbRequest, HttpMethod.Post);

            var response = await MonetizrClient.DownloadUrlAsString(requestMessage);

            string res = response.content;

            if (!response.isSuccess || res.Contains("Request failed!"))
            {
                Log.PrintWarning($"Response unsuccessful with content: {res}");
                return;
            }

            var openRtbResponse = PubmaticHelper.OpenRTBResponse.Load(res);

            var adm = openRtbResponse.GetAdm();

            if (string.IsNullOrEmpty(adm))
                return;

            Log.PrintWarning($"Open RTB response loaded with adm: {adm}");

            _webView = gameObject.AddComponent<UniWebView>();
            
            _webView.Frame = new Rect((Screen.width - 700)/2, 200, 700, 700);

            _webView.OnMessageReceived += OnMessageReceived;
            _webView.OnPageStarted += OnPageStarted;
            _webView.OnPageFinished += OnPageFinished;
            _webView.OnPageErrorReceived += OnPageErrorReceived;

            //_webView.Alpha = 0;

            _webView.LoadHTMLString(adm, "https://monetizr.com");
        }

        private void OnPageErrorReceived(UniWebView webview, int errorcode, string errormessage)
        {
            
        }

        public void HideBanner()
        {
            if(_webView != null)
                _webView.Hide(true);
        }

        private void OnPageFinished(UniWebView webview, int statuscode, string url)
        {
            Log.PrintWarning($"Banner OnPageFinished {statuscode}");
            //_webView.Alpha = 1;
            _webView.Show(true);
        }

        private void OnPageStarted(UniWebView webview, string url)
        {
            
        }

        private void OnMessageReceived(UniWebView webview, UniWebViewMessage message)
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
