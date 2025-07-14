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

        internal PubmaticHelper(MonetizrHttpClient httpClient, string userAgent) : base(httpClient, userAgent) { }

        internal async Task<bool> GetOpenRTBResponseForCampaign(ServerCampaign currentCampaign)
        {
            if (ShouldUseCachedRequestData(currentCampaign))
            {
                MonetizrLogger.Print("PBR - Last successful request is within 12 hours cooldown - Loading cached data.");
                string cachedAdm = PlayerPrefs.GetString("Campaign_" + currentCampaign.id + "_lastSuccessfulRequestData");
                if (!string.IsNullOrEmpty(cachedAdm))
                {
                    currentCampaign.adm = cachedAdm;
                    return true;
                }
            }

            SettingsDictionary<string, string> settings = currentCampaign.serverSettings;
            if (!AreRequestParametersInSettings(settings)) return false;

            string openRTBEndpoint = settings.GetParam("openrtb.endpoint");
            string openRTBRequest = settings.GetParam("openrtb.request");

            if (string.IsNullOrEmpty(openRTBRequest) && settings.ContainsKey("openrtb.generator_url"))
            {
                string generatorUri = settings.GetParam("openrtb.generator_url");
                openRTBRequest = await GetOpenRTBRequestByRemoteGenerator(generatorUri + $"&ad_id={MonetizrMobileAnalytics.advertisingID}");
                if (string.IsNullOrEmpty(openRTBRequest))
                {
                    MonetizrLogger.Print("OpenRTB Request by Remote Generator failed.");
                    return false;
                }
            }

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
            currentCampaign.adm = adm;

            PlayerPrefs.SetString("Campaign_" + currentCampaign.id + "_lastSuccessfulRequestTime", DateTime.Now.ToString());
            PlayerPrefs.SetString("Campaign_" + currentCampaign.id + "_lastSuccessfulRequestData", adm);

            if (settings.ContainsKey("openrtb.sent_report_to_mixpanel"))
            {
                httpClient.Analytics.SendOpenRtbReportToMixpanel(openRTBRequest, "ok", responseContent, currentCampaign);
            }

            return true;
        }

        private static bool ShouldUseCachedRequestData (ServerCampaign currentCampaign)
        {
            string lastSuccessfulRequestTimeString = PlayerPrefs.GetString("Campaign_" + currentCampaign.id + "_lastSuccessfulRequestTime", "");
            if (!string.IsNullOrEmpty(lastSuccessfulRequestTimeString))
            {
                if (DateTime.TryParse(lastSuccessfulRequestTimeString, out DateTime lastSuccessfulRequestTime))
                {
                    double timeSinceLastSuccesfulRequest = (DateTime.Now - lastSuccessfulRequestTime).TotalHours;
                    int cooldownHours = 12;
                    if (timeSinceLastSuccesfulRequest < cooldownHours)
                    {
                        return true;
                    }
                }
            }
            return false;
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
            if (string.IsNullOrEmpty(openRTBRequest) && !settings.ContainsKey("openrtb.generator_url"))
            {
                MonetizrLogger.Print("PBR - No request nor generator defined.");
                return false;
            }

            MonetizrLogger.Print("PBR - Request Parameters found. Endpoint: " + openRtbUri + " / Request: " + openRTBRequest);
            return true;
        }

        internal async Task<string> GetOpenRTBRequestByRemoteGenerator(string generatorUri)
        {
            if (string.IsNullOrEmpty(generatorUri)) return null;

            httpClient.SetUserAgent(userAgent);
            string res = await httpClient.GetResponseStringFromUrl(generatorUri);
            httpClient.SetUserAgent(null);

            return res;
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