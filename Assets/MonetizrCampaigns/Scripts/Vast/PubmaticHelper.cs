using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Monetizr.Campaigns.Vast42;
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


            public static OpenRTBResponse Load(string json)
            {
                return JsonUtility.FromJson<OpenRTBResponse>(json);
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
            var panelKey = globalSettings.GetParam("mixpanel.testmode", "");
            monetizrClient.InitializeMixpanel(testmode, panelKey);
            
            
            //getting openrtb campaign from monetizr proxy or with ssp endpoin
            Log.Print(globalSettings.dictionary.ToString());
            
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
                    requestMessage = MonetizrClient.GetOpenRtbRequestMessage(uri, openRtbRequest);
                }
            }

            Log.Print($"Requesting OpenRTB campaign with url: {uri}");

            var response = await MonetizrClient.DownloadUrlAsString(requestMessage);

            if (!response.isSuccess)
            {
                if (globalSettings.HasParam("openrtb.sent_report_to_mixpanel"))
                    monetizrClient.analytics.SendOpenRtbReportToMixpanel(openRtbRequest, "NoContent");
                
                return (false,new List<ServerCampaign>());
            }

            string res = response.content;
            
            if (res.Contains("Request failed!"))
                return (false, new List<ServerCampaign>());

            //TODO:
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
            
            var openRtbResponse = OpenRTBResponse.Load(res);

            var adm = openRtbResponse.seatbid[0]?.bid[0]?.adm;

            if (string.IsNullOrEmpty(adm))
                return (false, new List<ServerCampaign>());

            //adm = adm.Replace("\\\\n", "\n");
            //adm = adm.Replace("\\\"", "'");

            Log.Print($"{openRtbResponse.id} {openRtbResponse.seatbid[0].bid[0].adm}");

            string json =
                "{\"native\": {\"assets\": [{\"data\":{\"value\":\"TEST_TEXT_TEST_TEXT_TEST_TEXT_TEST_TEXT_TEST_TEXT_TEST_TEXT_TEST_TEXT_TEST_TEXT\"},\"id\":3},{\"data\":{\"value\":\"install\"},\"id\":4},{\"id\":2, \"img\": { \"h\":80, \"url\": \"https://cdn.splicky.com/720298803/test-banner8080.jpg\", \"w\": 80} }, {  \"id \":1,  \"title\":{\"text\":\"TEST_TITLE\"} },{\"video\":{\"vasttag\":\"\"}}]}}";

            //var nativeData = NativeData.Load(json);


            //*
            
            //extracting vast tag out of json, because parse is not working with xml inside
            string input = adm;
            string startTag = "vasttag\":\"";
            string endTag = "\"}";

            int start = input.LastIndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
            int end = input.IndexOf(endTag, start, StringComparison.Ordinal);

            string vast = input.Substring(start, end - start);
            string result = input.Remove(start, end - start);

            //*/
            VAST vastData = CreateVastFromXml(vast);

            ServerCampaign serverCampaign = await PrepareServerCampaign(openRtbResponse.id, vastData, true);
            
            if (vastData != null)
                Log.Print("vast loaded");
            
            serverCampaign.serverSettings.MergeSettingsFrom(globalSettings);

            Log.Print($"vast {vast}\n\n{result}");

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
                        if(a.data.value.Length > 15)
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

            if (serverCampaign != null)
            {
                resultCampaignList.Add(serverCampaign);
            }

            //Log.Print($"Culture: {System.Globalization.CultureInfo.CurrentCulture.Name}");

            return (true,resultCampaignList);
        }
    }
}