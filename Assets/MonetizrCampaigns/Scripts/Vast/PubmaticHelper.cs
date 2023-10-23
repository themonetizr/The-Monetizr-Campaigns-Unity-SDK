using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Monetizr.Campaigns.Vast42;
using SimpleJSON;
using UnityEngine;

namespace Monetizr.Campaigns
{
    internal class PubmaticHelper : VastHelper
    {
        [Serializable]
        public class OpenRTBResponse
        {
            public string id;
            public SeatBid[] seatbid;
            private static JSONNode _root;

            private OpenRTBResponse(JSONNode jsonNode)
            {
                _root = jsonNode;
            }

            public static OpenRTBResponse Load(string json)
            {
                var root = SimpleJSON.JSON.Parse(json);

                var response = new OpenRTBResponse(root);

                return response;
            }

            public string GetAdm()
            {
                if (_root == null)
                    return "";

                var seatbidsArray = _root["seatbid"];

                if (seatbidsArray == null || seatbidsArray.Count == 0)
                    return "";

                var firstSeatBid = seatbidsArray[0];

                if (firstSeatBid == null)
                    return "";

                var bidsArray = firstSeatBid["bid"];

                if (bidsArray == null || bidsArray.Count == 0)
                    return "";

                var firstBid = bidsArray[0];

                if (firstBid == null)
                    return "";

                var admNode = firstBid["adm"];

                if (admNode == null)
                    return "";

                //string result = admNode.Value.Replace("\\\"", "\"");

                return Utils.UnescapeString(admNode.Value);
            }

            public string GetId()
            {
                return _root == null ? "" : _root["id"].ToString();
            }
        }

        [Serializable]
        public class Bid
        {
            public string adm;
        }

        [Serializable]
        public class SeatBid
        {
            public Bid[] bid;
        }

        [System.Serializable]
        public class NativeData
        {
            public Native native;

            public static NativeData Load(string json)
            {
                return JsonUtility.FromJson<NativeData>(json);
            }
        }

        [System.Serializable]
        public class Native
        {
            public List<Asset> assets;
        }

        internal enum AssetType
        {
            Unknown,
            Data,
            Image,
            Title,
            Video
        }

        [System.Serializable]
        public class Asset
        {
            public AssetType type;
            public int id;
            public Data data = null;
            public Img img = null;
            public Title title = null;
            public Video video = null;

            public AssetType GetAssetType()
            {
                if (!string.IsNullOrEmpty(img?.url))
                    return AssetType.Image;

                if (!string.IsNullOrEmpty(data?.value))
                    return AssetType.Data;

                if (!string.IsNullOrEmpty(title?.text))
                    return AssetType.Title;

                if (video != null)
                    return AssetType.Video;

                return AssetType.Unknown;
            }
        }

        [System.Serializable]
        public class Data
        {
            public string value;
        }

        [System.Serializable]
        public class Img
        {
            public float h, w;
            public string url;
        }

        [System.Serializable]
        public class Title
        {
            public string text;
        }

        [System.Serializable]
        public class Video
        {
            public string vasttag;
        }

        internal PubmaticHelper(MonetizrClient client, string userAgent) : base(client, userAgent)
        {
        }

        internal async Task<string> GetOpenRtbRequestByRemoteGenerator(string generatorUri)
        {
            if (string.IsNullOrEmpty(generatorUri))
                return null;

            var requestMessage = client.GetHttpRequestMessage(generatorUri, userAgent);

            Log.PrintV($"Generator message: {requestMessage}");

            HttpResponseMessage response = await client.GetHttpClient().SendAsync(requestMessage);

            Log.PrintV($"Generator response: {response}");

            var result = await response.Content.ReadAsStringAsync();

            Log.PrintV($"Generator result: {result}");

            if (response.IsSuccessStatusCode && result.Length > 0)
                return result;

            return null;
        }

        internal async Task<(bool isSuccess, List<ServerCampaign> result)> GetProgrammaticCampaign(MonetizrClient monetizrClient)
        {
            //if (GetVastParams() == null)
            //    return false;

            var resultCampaignList = new List<ServerCampaign>();

            var globalSettings = client.GlobalSettings;
            //
            var testmode = globalSettings.GetBoolParam("mixpanel.testmode", false);
            var mixpanelKey = globalSettings.GetParam("mixpanel.apikey", "");
            var apiUrl = globalSettings.GetParam("api_url");
            var videoOnly = globalSettings.GetBoolParam("openrtb.video_only", true);

            monetizrClient.InitializeMixpanel(testmode, mixpanelKey, apiUrl);

            //getting openrtb campaign from monetizr proxy or with ssp endpoind
            //Log.PrintV(globalSettings.dictionary.ToString());

            if (!globalSettings.ContainsKey("openrtb.endpoint"))
            {
                Log.PrintV($"No programmatic endpoint defined! Programmatic disabled!");
                return (false, new List<ServerCampaign>());
            }

            string uri = globalSettings.GetParam("openrtb.endpoint");

            var requestMessage = client.GetHttpRequestMessage(uri);

            string openRtbRequest = "";

            if (globalSettings.GetBoolParam("openrtb.send_by_client", false) &&
                globalSettings.ContainsKey("openrtb.endpoint") &&
                globalSettings.ContainsKey("openrtb.generator_url"))
            {
                string generatorUri = globalSettings.GetParam("openrtb.generator_url");

                openRtbRequest = await GetOpenRtbRequestByRemoteGenerator(generatorUri);

                if (!string.IsNullOrEmpty(openRtbRequest))
                {
                    Log.PrintWarning($"request: {openRtbRequest}");
                }
            }

            Log.PrintV($"Requesting OpenRTB campaign with url: {uri}");

            requestMessage = MonetizrClient.GetOpenRtbRequestMessage(uri, openRtbRequest, HttpMethod.Post);

            var response = await MonetizrClient.DownloadUrlAsString(requestMessage);

            /*#if UNITY_EDITOR
                        uri = "http://127.0.0.1:8000/?test=3";
                        requestMessage = MonetizrClient.GetOpenRtbRequestMessage(uri, "", HttpMethod.Post);
                        response = await MonetizrClient.DownloadUrlAsString(requestMessage);
            #endif*/

            if (!response.isSuccess)
            {
                //#if !UNITY_EDITOR
                if (globalSettings.ContainsKey("openrtb.sent_report_to_mixpanel"))
                    monetizrClient.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "error", "NoContent", null);
                //#endif                

                return (false, new List<ServerCampaign>());
            }


            string res = response.content;

            if (res.Contains("Request failed!"))
                return (false, new List<ServerCampaign>());


            var openRtbResponse = OpenRTBResponse.Load(res);


            var adm = openRtbResponse.GetAdm();

            if (string.IsNullOrEmpty(adm))
                return (false, new List<ServerCampaign>());

            Log.PrintV($"Open RTB response loaded with adm: {adm}");

            string vastString = null;
            //string nativeString = null;

            /*if (adm.Contains("vasttag"))
            {
                
                //extracting vast tag out of json, because parse is not working with xml inside
                string input = adm;
                string startTag = "vasttag\":\"";
                string endTag = "\"}";

                int start = input.LastIndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
                int end = input.IndexOf(endTag, start, StringComparison.Ordinal);

                vastString = input.Substring(start, end - start);
                nativeString = input.Remove(start, end - start);
            }
            else */
            if (adm.StartsWith("<VAST"))
            {
                vastString = adm;
            }
            else
            {
                Log.PrintV($"Open RTB response is not a VAST");
                return (false, new List<ServerCampaign>());
            }

            //*/

            ServerCampaign serverCampaign = await PrepareServerCampaign(openRtbResponse.GetId(), vastString, videoOnly);

            if (serverCampaign == null)
            {
                Log.PrintV($"PrepareServerCampaign failed.");
                return (false, new List<ServerCampaign>());
            }

            serverCampaign.serverSettings.MergeSettingsFrom(globalSettings);

            //Log.PrintV($"vast {vastString}\n\n{nativeString}");

            /*if (nativeString != null)
            {
                LoadAdditionalNativeAssets(nativeString, serverCampaign);
            }*/

            //#if !UNITY_EDITOR            
            if (globalSettings.ContainsKey("openrtb.sent_report_to_mixpanel"))
            {
                monetizrClient.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "ok", res, null);
            }

            /* if (globalSettings.GetBoolParam("openrtb.sent_report_to_slack", false))
             {
                 monetizrClient.SendErrorToRemoteServer("Notify",
                     "Openrtb request successfully received",
                                 $"Notify: Openrtb request successfully received (test mode: {testmode}) ");
             }*/
            //#endif

            resultCampaignList.Add(serverCampaign);


            //Log.PrintV($"Culture: {System.Globalization.CultureInfo.CurrentCulture.Name}");

            return (true, resultCampaignList);
        }
        
        private void LoadAdditionalNativeAssets(string result, ServerCampaign serverCampaign)
        {
            var nativeData = NativeData.Load(result);

            //sc.id = openRtbResponse.id;

            foreach (var a in nativeData.native.assets)
            {
                Log.PrintV($"asset: {a.id} {a.GetAssetType().ToString()}");

                //if (a.img == null || string.IsNullOrEmpty(a.img.url))
                //    continue;

                string url = a.img.url;

                Log.PrintV($"url: {url}");
                Log.PrintV($"title: {a.title.text}");
                Log.PrintV($"data: {a.data.value}");

                switch (a.GetAssetType())
                {
                    case AssetType.Unknown:
                        break;

                    case AssetType.Data:
                        if (a.data.value.Length > 15)
                            serverCampaign.serverSettings["RewardCenter.VideoReward.content_text"] = a.data.value;

                        break;

                    case AssetType.Image:
                        var asset = new ServerCampaign.Asset()
                        {
                            id = $"{a.id}",
                            url = url,
                            type = "banner",
                            fname = Utils.ConvertCreativeToFname(url),
                            fext = Utils.ConvertCreativeToExt("", url),
                        };
                        serverCampaign.assets.Add(asset);

                        break;
                    case AssetType.Title:
                        serverCampaign.serverSettings["TinyMenuTeaser.button_text"] = a.title.text;
                        break;
                    case AssetType.Video:
                        break;
                }


                //Log.PrintV(asset.ToString());
            }
        }
        
        internal async Task<bool> GetOpenRtbResponseForCampaign(ServerCampaign currentCampaign,
            string currentMissionOpenRtbRequest)
        {
            var settings = currentCampaign.serverSettings; //client.GlobalSettings;

           //var apiUrl = globalSettings.GetParam("api_url");
            var openRtbUri = settings.GetParam("openrtb.endpoint");

            if (string.IsNullOrEmpty(openRtbUri))
            {
                Log.PrintV($"No programmatic endpoint defined! Programmatic disabled!");
                return false;
            }

            var requestParameterName = string.IsNullOrEmpty(currentMissionOpenRtbRequest)
                ? "openrtb.request"
                : currentMissionOpenRtbRequest;

            var timeParameterName = $"openrtb.last_request.{requestParameterName}";

#if !UNITY_EDITOR
            if (DateTime.TryParse(MonetizrManager.Instance.localSettings.GetSetting(currentCampaign.id)
                    .settings[timeParameterName], out var lastTime))
            {
                var delay = (DateTime.Now - lastTime).TotalSeconds;

                var targetDelay = currentCampaign.serverSettings.GetIntParam("openrtb.delay", 300);
                if (delay < targetDelay)
                {
                    Log.PrintV($"Last programmatic request was earlier than {targetDelay} {delay}");
                    return false;
                }
            }
#endif

            MonetizrManager.Instance.localSettings.GetSetting(currentCampaign.id).settings[timeParameterName] =
                DateTime.Now.ToString();
            MonetizrManager.Instance.localSettings.SaveData();

            //var requestMessage = MonetizrClient.GetHttpRequestMessage(openRtbUri, userAgent);

            
            var openRtbRequest = settings.GetParam(requestParameterName);
            
            if (string.IsNullOrEmpty(openRtbRequest) && settings.ContainsKey("openrtb.generator_url"))
            {
                string generatorUri = settings.GetParam("openrtb.generator_url");

                openRtbRequest = await GetOpenRtbRequestByRemoteGenerator(generatorUri + $"&ad_id={MonetizrAnalytics.advertisingID}");
            }

            if (string.IsNullOrEmpty(openRtbRequest))
            {
                Log.PrintError($"Can't create openRTB request for campaign {currentCampaign}!");
                return false;
            }

            openRtbRequest = Utils.UnescapeString(openRtbRequest);

            openRtbRequest = NielsenDar.ReplaceMacros(openRtbRequest, currentCampaign, AdPlacement.Html5, userAgent);

            Log.PrintV($"OpenRTB request: {openRtbRequest}");
            Log.PrintV($"Requesting OpenRTB campaign with url: {openRtbUri}");

            var requestMessage = MonetizrClient.GetOpenRtbRequestMessage(openRtbUri, openRtbRequest, HttpMethod.Post);
            var response = await MonetizrClient.DownloadUrlAsString(requestMessage);


            string res = response.content;

            if (!response.isSuccess || res.Contains("Request failed!") || res.Length <= 0)
            {
                if (settings.ContainsKey("openrtb.sent_report_to_mixpanel"))
                    client.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "error", "NoContent", currentCampaign);

                Log.PrintV($"Response unsuccessful with content: {res}");
                return false;
            }

            var openRtbResponse = OpenRTBResponse.Load(res);

            var adm = openRtbResponse.GetAdm();

            if (string.IsNullOrEmpty(adm))
                return false;

            Log.PrintV($"Open RTB response loaded with adm: {adm}");

            //string vastString;

            if (!adm.Contains("<VAST"))
            {
                Log.PrintError($"Open RTB response is not a VAST");
                return false;
            }
            
            var initializeResult = await InitializeServerCampaignForProgrammatic(currentCampaign, adm);

            if (!initializeResult)
            {
                Log.PrintV($"InitializeServerCampaignForProgrammatic failed.");
                return false;
            }

            Log.PrintV($"GetOpenRTBResponseForCampaign {currentCampaign.id} successfully loaded.");

            if (settings.ContainsKey("openrtb.sent_report_to_mixpanel"))
            {
                client.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "ok", res, currentCampaign);
            }

            return true;
        }
    }
}