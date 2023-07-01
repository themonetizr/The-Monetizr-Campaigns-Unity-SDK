using System;
using System.Collections.Generic;
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
                //Log.Print("json->"+json);

                //json = json.Replace("\\\"", "\'");
                
                var root = SimpleJSON.JSON.Parse(json);
             
                //Log.Print("root->"+root.ToString());
                
                var response = new OpenRTBResponse(root);

                return response;

                //return JsonUtility.FromJson<OpenRTBResponse>(json);
            }

            public string GetAdm()
            {
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
                
                string result = admNode.Value.Replace("\\\"","\"");
                
                //Log.Print("---->"+_root);
                return result;
                //openRtbResponse.seatbid[0]?.bid[0]?.adm;
                //throw new NotImplementedException();
            }

            public string GetId()
            {
                return _root["id"];
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

        internal PubmaticHelper(MonetizrClient client) : base(client)
        {
        }

        internal async Task<string> GetOpenRtbRequest(HttpClient httpClient, SettingsDictionary<string, string> globalSettings)
        {
            string result = default(string);
            
            var url = globalSettings.GetParam("openrtb.generator_url");

            if (string.IsNullOrEmpty(url))
                return result;
            
            Log.Print(url);
            
            var requestMessage = MonetizrClient.GetHttpRequestMessage(url);
            
            Log.Print(requestMessage);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            Log.Print(response);
            
            result = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
                return default(string);

            if (result.Length == 0)
                return default(string);

            return result;
        }
        
        internal new async Task<(bool isSuccess,List<ServerCampaign> result)> GetProgrammaticCampaign(MonetizrClient monetizrClient)
        {
            //if (GetVastParams() == null)
            //    return false;

            var resultCampaignList = new List<ServerCampaign>();
            
            //loading settings
            SettingsDictionary<string, string> globalSettings = await client.DownloadGlobalSettings();
            
            //
            var testmode = globalSettings.GetBoolParam("mixpanel.testmode", false);
            var mixpanelKey = globalSettings.GetParam("mixpanel.apikey", "");
            var apiUrl = globalSettings.GetParam("api_url", "");
            monetizrClient.InitializeMixpanel(testmode, mixpanelKey, apiUrl);
            
            
            //getting openrtb campaign from monetizr proxy or with ssp endpoin
            Log.Print(globalSettings.dictionary.ToString());
            
            if(globalSettings.GetBoolParam("openrtb.disabled", false))
                return (false, new List<ServerCampaign>());
            
            string uri = "https://programmatic-serve-stineosy7q-uc.a.run.app/?test=1&native&pmp";
            var requestMessage = MonetizrClient.GetHttpRequestMessage(uri);
            string openRtbRequest = "";
                
            if (globalSettings.GetBoolParam("openrtb.send_by_client", false) &&
                globalSettings.HasParam("openrtb.endpoint"))
            {
                openRtbRequest = await GetOpenRtbRequest(monetizrClient.GetClient(), globalSettings);

                if (!string.IsNullOrEmpty(openRtbRequest))
                {
                    Log.PrintWarning($"request: {openRtbRequest}");

                    uri = globalSettings.GetParam("openrtb.endpoint");
                    
                    requestMessage = MonetizrClient.GetOpenRtbRequestMessage(uri, openRtbRequest, HttpMethod.Post);
                }
            }

            Log.Print($"Requesting OpenRTB campaign with url: {uri}");

            var response = await MonetizrClient.DownloadUrlAsString(requestMessage);

#if UNITY_EDITOR
            uri = "http://127.0.0.1:8000/?test=3";
            requestMessage = MonetizrClient.GetOpenRtbRequestMessage(uri, "", HttpMethod.Get);
            response = await MonetizrClient.DownloadUrlAsString(requestMessage);
#endif

            if (!response.isSuccess)
            {
#if !UNITY_EDITOR
                if (globalSettings.HasParam("openrtb.sent_report_to_mixpanel"))
                    monetizrClient.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "NoContent");
#endif                

                return (false,new List<ServerCampaign>());
            }


            string res = response.content;
            
            if (res.Contains("Request failed!"))
                return (false, new List<ServerCampaign>());
            
            
#if !UNITY_EDITOR            
            if (globalSettings.HasParam("openrtb.sent_report_to_mixpanel"))
            {
                monetizrClient.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, res);
            }

            if (globalSettings.GetBoolParam("openrtb.sent_report_to_slack", false))
            {
//#if !UNITY_EDITOR            
                monetizrClient.SendErrorToRemoteServer("Notify",
                    "Openrtb request successfully received",
                                $"Notify: Openrtb request successfully received (test mode: {testmode}) ");
//#endif
            }
#endif
            
            var openRtbResponse = OpenRTBResponse.Load(res);


            var adm = openRtbResponse.GetAdm();
            
            Log.Print($"{openRtbResponse} {adm}");

            if (string.IsNullOrEmpty(adm))
                return (false, new List<ServerCampaign>());
            
            string vastString = null;
            string nativeString = null;
            
            if (adm.Contains("vasttag"))
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
            else if(adm.StartsWith("<VAST"))
            {
                vastString = adm;
            }

            //*/

            ServerCampaign serverCampaign = await PrepareServerCampaign(openRtbResponse.GetId(), vastString, true);
            
            
            serverCampaign.serverSettings.MergeSettingsFrom(globalSettings);

            //Log.Print($"vast {vastString}\n\n{nativeString}");

            if (nativeString != null)
            {
                LoadAdditionalNativeAssets(nativeString, serverCampaign);
            }

            if (serverCampaign != null)
            {
                resultCampaignList.Add(serverCampaign);
            }

            //Log.Print($"Culture: {System.Globalization.CultureInfo.CurrentCulture.Name}");

            return (true,resultCampaignList);
        }

        private void LoadAdditionalNativeAssets(string result, ServerCampaign serverCampaign)
        {
            var nativeData = NativeData.Load(result);

            //sc.id = openRtbResponse.id;

            foreach (var a in nativeData.native.assets)
            {
                Log.Print($"asset: {a.id} {a.GetAssetType().ToString()}");

                //if (a.img == null || string.IsNullOrEmpty(a.img.url))
                //    continue;

                string url = a.img.url;

                Log.Print($"url: {url}");
                Log.Print($"title: {a.title.text}");
                Log.Print($"data: {a.data.value}");

                switch (a.GetAssetType())
                {
                    case AssetType.Unknown:
                        break;

                    case AssetType.Data:
                        if (a.data.value.Length > 15)
                            serverCampaign.serverSettings.dictionary["RewardCenter.VideoReward.content_text"] = a.data.value;

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
                        serverCampaign.serverSettings.dictionary["TinyMenuTeaser.button_text"] = a.title.text;
                        break;
                    case AssetType.Video:
                        break;
                }


                //Log.Print(asset.ToString());
            }
        }
    }
}