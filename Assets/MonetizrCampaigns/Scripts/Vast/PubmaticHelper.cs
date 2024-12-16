using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Utils;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Monetizr.SDK.VAST
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
                if (_root == null) return "";

                var seatbidsArray = _root["seatbid"];
                if (seatbidsArray == null || seatbidsArray.Count == 0) return "";

                var firstSeatBid = seatbidsArray[0];
                if (firstSeatBid == null) return "";

                var bidsArray = firstSeatBid["bid"];
                if (bidsArray == null || bidsArray.Count == 0) return "";

                var firstBid = bidsArray[0];
                if (firstBid == null) return "";

                var admNode = firstBid["adm"];
                if (admNode == null) return "";

                return MonetizrUtils.UnescapeString(admNode.Value);
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

        internal PubmaticHelper(MonetizrClient httpClient, string userAgent) : base(httpClient, userAgent) { }

        internal async Task<string> GetOpenRtbRequestByRemoteGenerator(string generatorUri)
        {
            if (string.IsNullOrEmpty(generatorUri)) return null;
            
            httpClient.SetUserAgent(userAgent);
            string res = await httpClient.GetResponseStringFromUrl(generatorUri);
            httpClient.SetUserAgent(null);

            return res;
        }
        
        internal async Task<bool> GetOpenRtbResponseForCampaign(ServerCampaign currentCampaign, string currentMissionOpenRtbRequest, string openrtbEndpoint)
        {
            var settings = currentCampaign.serverSettings;
            var openRtbUri = settings.GetParam("openrtb.endpoint");

            MonetizrLogger.Print("PBR - Endpoint Param: " + openrtbEndpoint);

            if (string.IsNullOrEmpty(openRtbUri))
            {
                openRtbUri = openrtbEndpoint;
            }

            if (string.IsNullOrEmpty(openRtbUri))
            {
                MonetizrLogger.Print("PBR - No endpoint defined.");
                return false;
            }

            MonetizrLogger.Print("PBR - Endpoint found.");

            var requestParameterName = string.IsNullOrEmpty(currentMissionOpenRtbRequest) ? "openrtb.request" : currentMissionOpenRtbRequest;
            var timeParameterName = $"openrtb.last_request.{requestParameterName}";

#if !UNITY_EDITOR
            if (DateTime.TryParse(MonetizrManager.Instance.localSettings.GetSetting(currentCampaign.id).settings[timeParameterName], out var lastTime))
            {
                var delay = (DateTime.Now - lastTime).TotalSeconds;
                var targetDelay = 10;
                if (delay < targetDelay)
                {
                    MonetizrLogger.Print($"Last programmatic request was earlier than {targetDelay} {delay}");
                    return false;
                }
            }
#endif

            MonetizrManager.Instance.localSettings.GetSetting(currentCampaign.id).settings[timeParameterName] = DateTime.Now.ToString();
            MonetizrManager.Instance.localSettings.SaveData();
            var openRtbRequest = settings.GetParam(requestParameterName);
            
            if (string.IsNullOrEmpty(openRtbRequest) && settings.ContainsKey("openrtb.generator_url"))
            {
                string generatorUri = settings.GetParam("openrtb.generator_url");
                openRtbRequest = await GetOpenRtbRequestByRemoteGenerator(generatorUri + $"&ad_id={MonetizrMobileAnalytics.advertisingID}");
            }

            if (string.IsNullOrEmpty(openRtbRequest))
            {
                MonetizrLogger.PrintError("PBR - Can't create openRTB request for campaign {currentCampaign}!");
                return false;
            }

            openRtbRequest = MonetizrUtils.UnescapeString(openRtbRequest);
            openRtbRequest = NielsenDar.ReplaceMacros(openRtbRequest, currentCampaign, AdPlacement.Html5, userAgent);

            MonetizrLogger.Print($"OpenRTB request: {openRtbRequest}");
            MonetizrLogger.Print($"Requesting OpenRTB campaign with url: {openRtbUri}");

            var requestMessage = NetworkingUtils.GenerateOpenRTBRequestMessage(openRtbUri, openRtbRequest, HttpMethod.Post);
            var response = await MonetizrHttpClient.DownloadUrlAsString(requestMessage);
            string res = response.content;

            if (!response.isSuccess || res.Contains("Request failed!") || res.Length <= 0)
            {
                if (settings.ContainsKey("openrtb.sent_report_to_mixpanel")) httpClient.Analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "error", "NoContent", currentCampaign);
                MonetizrLogger.Print($"Response unsuccessful with content: {res}");
                return false;
            }

            currentCampaign.openRtbRawResponse = res;
            MonetizrLogger.Print($"Open RTB Raw Response: {res}");
            var openRtbResponse = OpenRTBResponse.Load(res);
            var adm = openRtbResponse.GetAdm();

            if (string.IsNullOrEmpty(adm)) return false;

            MonetizrLogger.Print($"Open RTB response loaded with adm: {adm}");

            if (!adm.Contains("<VAST"))
            {
                MonetizrLogger.PrintError($"Open RTB response is not a VAST");
                return false;
            }
            
            var initializeResult = await InitializeServerCampaignForProgrammatic(currentCampaign, adm);

            if (!initializeResult)
            {
                MonetizrLogger.Print($"InitializeServerCampaignForProgrammatic failed.");
                return false;
            }

            MonetizrLogger.Print($"GetOpenRTBResponseForCampaign {currentCampaign.id} successfully loaded.");

            if (settings.ContainsKey("openrtb.sent_report_to_mixpanel"))
            {
                httpClient.Analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "ok", res, currentCampaign);
            }

            return true;
        }

        internal async Task<bool> TEST_GetOpenRtbResponseForCampaign(ServerCampaign currentCampaign)
        {
            SettingsDictionary<string, string> settings = currentCampaign.serverSettings;
            if (!AreRequestParametersInSettings(settings)) return false;

            string openRTBEndpoint = settings.GetParam("openrtb.endpoint");
            string openRTBRequest = settings.GetParam("openrtb.request");

            openRTBRequest = MonetizrUtils.UnescapeString(openRTBRequest);
            openRTBRequest = NielsenDar.ReplaceMacros(openRTBRequest, currentCampaign, AdPlacement.Html5, userAgent);
            MonetizrLogger.Print("PBR - Final request: " + openRTBRequest);

            string responseContent = await GetOpenRTBResponseAsync(openRTBEndpoint, openRTBRequest);
            if (string.IsNullOrEmpty(responseContent))
            {
                if (settings.ContainsKey("openrtb.sent_report_to_mixpanel")) httpClient.Analytics.SendOpenRtbReportToMixpanel(openRTBRequest, "error", "NoContent", currentCampaign);
                return false;
            }

            currentCampaign.openRtbRawResponse = responseContent;
            OpenRTBResponse openRtbResponse = OpenRTBResponse.Load(responseContent);
            
            string adm = openRtbResponse.GetAdm();
            if (!IsADMValid(adm)) return false;

            bool initializeResult = await InitializeServerCampaignForProgrammatic(currentCampaign, adm);
            if (!initializeResult)
            {
                MonetizrLogger.Print("PBR - InitializeServerCampaignForProgrammatic failed.");
                return false;
            }

            if (settings.ContainsKey("openrtb.sent_report_to_mixpanel")) httpClient.Analytics.SendOpenRtbReportToMixpanel(openRTBRequest, "ok", responseContent, currentCampaign);

            return true;
        }

        internal bool AreRequestParametersInSettings (SettingsDictionary<string, string> settings)
        {
            string openRtbUri = settings.GetParam("openrtb.endpoint");
            if (string.IsNullOrEmpty(openRtbUri))
            {
                MonetizrLogger.Print("PBR - No endpoint defined.");
                return false;
            }

            string openRTBRequest = settings.GetParam("openrtb.request");
            if (string.IsNullOrEmpty(openRTBRequest))
            {
                MonetizrLogger.Print("PBR - No request defined.");
                return false;
            }

            MonetizrLogger.Print("PBR - Request Parameters found. Endpoint: " + openRtbUri + " / Request: " + openRTBRequest);
            return true;
        }

        internal async Task<string> GetOpenRTBResponseAsync (string endpoint, string request)
        {
            HttpRequestMessage requestMessage = NetworkingUtils.GenerateOpenRTBRequestMessage(endpoint, request, HttpMethod.Post);
            var response = await MonetizrHttpClient.DownloadUrlAsString(requestMessage);
            string responseContent = response.content;
            MonetizrLogger.Print("PBR - Response Succesful: " + response.isSuccess + " / Content: " + responseContent);

            if (!response.isSuccess || responseContent.Contains("Request failed!") || responseContent.Length <= 0) return "";

            return responseContent;
        }

        internal bool IsADMValid (string adm)
        {
            if (string.IsNullOrEmpty(adm))
            {
                MonetizrLogger.Print("PBR - ADM not found.");
                return false;
            }

            if (!adm.Contains("<VAST"))
            {
                MonetizrLogger.PrintError("PBR - Response is not a VAST.");
                return false;
            }

            MonetizrLogger.Print("PBR - ADM is valid: " + adm);
            return true;
        }
    }
}