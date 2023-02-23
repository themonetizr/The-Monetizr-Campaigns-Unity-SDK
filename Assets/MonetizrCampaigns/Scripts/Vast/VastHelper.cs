using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
//using UnityEditor.Android;
using UnityEngine;
using UnityEngine.Networking;

using Monetizr.Campaigns.Vast42;
using MiniJSON;

namespace Monetizr.Campaigns
{
    internal class VastHelper
    {
        MonetizrClient client;

        internal class VastParams
        {
            internal int setID;
            internal int id;
            internal int pid;
        }

        internal VastHelper(MonetizrClient client)
        {
            this.client = client;
        }

        public VastParams GetVastParams()
        {
            if (string.IsNullOrEmpty(client.currentApiKey))
                return null;

            if (client.currentApiKey.Length == 43)
                return null;

            var p = Array.ConvertAll(client.currentApiKey.Split('-'), int.Parse);

            if (p.Length != 3)
                return null;

            return new VastParams() { id = p[0], setID = p[1], pid = p[2] };
        }


        [System.Serializable]
        internal class AdVerifications
        {
            public List<AdVerification> adVerifications = new List<AdVerification>();
        }

        [System.Serializable]
        internal class AdVerification
        {
            public string verificationParameters = "";
            public string vendorField = "";
            public List<VerificationExecutableResource> executableResource = new List<VerificationExecutableResource>();
            public List<VerificationJavaScriptResource> javaScriptResource = new List<VerificationJavaScriptResource>();
            public List<VerificationTracking> tracking = new List<VerificationTracking>();

        }

        [System.Serializable]
        internal class VerificationExecutableResource
        {
            public string apiFramework = "";
            public string type = "";
            public string value = "";

            public VerificationExecutableResource()
            {
            }

            public VerificationExecutableResource(Verification_typeExecutableResource er)
            {
                apiFramework = er.apiFramework;
                type = er.type;
                value = er.Value;
            }
        }

        [System.Serializable]
        internal class VerificationJavaScriptResource
        {
            public string apiFramework = "";
            public bool browserOptional = false;
            public bool browserOptionalSpecified = false;
            public string value = "";

            public VerificationJavaScriptResource()
            {
            }

            public VerificationJavaScriptResource(Verification_typeJavaScriptResource jsr)
            {
                apiFramework = jsr.apiFramework;
                browserOptional = jsr.browserOptional;
                browserOptionalSpecified = jsr.browserOptionalSpecified;
                value = jsr.Value;
            }
        }

        [System.Serializable]
        internal class VerificationTracking
        {
            public string @event = "";
            public string value = "";

            public VerificationTracking()
            {
            }

            public VerificationTracking(TrackingEvents_Verification_typeTracking te)
            {
                @event = te.@event;
                value = te.Value;
            }
        }
                        

        internal string CreateStringFromVerificationSettings(Verification_type[] adVerifications)
        {
            if (adVerifications.IsNullOrEmpty())
            {
                Log.PrintWarning("AdVerifications is not defined in the VAST xml!");
                return "{}";
            }

            AdVerifications adv = new AdVerifications();

            foreach (var av in adVerifications)
            {
                //Log.Print($"Vendor: [{av.vendor}] VerificationParameters: [{av.VerificationParameters}]");

                var jsrList = Utils.ArrayToList<Verification_typeJavaScriptResource, VerificationJavaScriptResource>(
                    av.JavaScriptResource,
                    (Verification_typeJavaScriptResource jsr) => { return new VerificationJavaScriptResource(jsr); },
                    new VerificationJavaScriptResource());

                var trackingList = Utils.ArrayToList<TrackingEvents_Verification_typeTracking, VerificationTracking>(
                    av.TrackingEvents,
                    (TrackingEvents_Verification_typeTracking te) => { return new VerificationTracking(te); },
                    new VerificationTracking());

                var execList = Utils.ArrayToList<Verification_typeExecutableResource, VerificationExecutableResource>(
                    av.ExecutableResource,
                    (Verification_typeExecutableResource er) => { return new VerificationExecutableResource(er); },
                    new VerificationExecutableResource());
                
                adv.adVerifications.Add(new AdVerification()
                {
                    executableResource = execList,
                    javaScriptResource = jsrList,
                    tracking = trackingList,
                    vendorField = av.vendor,
                    verificationParameters = av.VerificationParameters
                });
            }

            //string s = Json.Serialize(adv.adVerifications);

            return JsonUtility.ToJson(adv);
        }

        internal async Task<ServerCampaign> PrepareServerCampaign(VAST v)
        {
            if (v.Items == null || v.Items.Length == 0)
                return null;

            if (!(v.Items[0] is VASTAD))
                return null;

            VASTAD vad = (VASTAD)v.Items[0];

            //ServerCampaign serverCampaign = new ServerCampaign() { id = $"{v.Ad[0].id}-{UnityEngine.Random.Range(1000,2000)}", dar_tag = "" };
            ServerCampaign serverCampaign = new ServerCampaign() { id = $"{vad.id}", dar_tag = "" };


            if (!(vad.Item is Inline_type))
                return null;

            bool hasSettings = false;
            bool hasVideo = false;

            ServerCampaign.Asset videoAsset = null;

            var inLine = (Inline_type)vad.Item;



            //string s = JSON.Serialize(inLine.adVerificationsField);

            string s = CreateStringFromVerificationSettings(inLine.AdVerifications);

            Log.Print(s);

            serverCampaign.vastAdVerificationParams = s;

            //serverCampaign.id = v.Ad[0].id;

            foreach (var c in inLine.Creatives)
            {
                ServerCampaign.Asset asset = null;

                if (c.NonLinearAds != null)
                {
                    var it = c.NonLinearAds;

                    foreach (var nl in it.NonLinear)
                    {
                        string adParameters = nl.AdParameters.Value;
                        var staticRes = nl.StaticResource[0];

                        //Log.Print($"{staticRes.Value}");

                        asset = new ServerCampaign.Asset()
                        {
                            id = $"{c.id} {nl.id}",
                            url = staticRes.Value,
                            type = adParameters,
                            fname = ConvertCreativeToFname(staticRes.Value),
                            fext = ConvertCreativeToExt(staticRes.creativeType, staticRes.Value),
                        };

                        //Log.Print(asset.ToString());

                        serverCampaign.assets.Add(asset);

                    }

                }

                if (c.Linear != null)
                {
                    var it = c.Linear;

                    //Log.Print(it.MediaFiles[0].Value);

                    Log.Print(it.AdParameters);
                    string value = it.MediaFiles.MediaFile[0].Value;
                    string type = it.MediaFiles.MediaFile[0].type;

                    videoAsset = new ServerCampaign.Asset()
                    {
                        id = c.id,
                        url = value,
                        fpath = ConvertCreativeToFname(value),
                        fname = "video",
                        fext = ConvertCreativeToExt(type, value),
                        type = "html",
                        mainAssetName = $"{ConvertCreativeToFname(value)}/index.html"
                    };

                    serverCampaign.assets.Add(videoAsset);

                    hasVideo = true;

                    //Log.Print(asset.ToString());

                    if (it.AdParameters != null)
                    {
                        string adp = it.AdParameters.Value;

                        adp = adp.Replace("\n", "");

                        var dict = Json.Deserialize(adp) as Dictionary<string, object>;

                        serverCampaign.serverSettings = new SettingsDictionary<string, string>(Utils.ParseContentString(adp, dict));
                    }
                }
                /*else if (c.Item is VASTADInLineCreativeCompanionAds)
                {

                }*/


            }

            if (!hasSettings && videoAsset != null)
            {
                serverCampaign.serverSettings = new SettingsDictionary<string, string>();

                serverCampaign.serverSettings.dictionary.Add("custom_missions", "{'missions': [{'type':'VideoReward','percent_amount':'100','id':'5'}]}");
                serverCampaign.serverSettings.dictionary.Add("design_version", "2");
                serverCampaign.serverSettings.dictionary.Add("amount_of_teasers", "100");
                serverCampaign.serverSettings.dictionary.Add("teaser_design_version", "3");
                serverCampaign.serverSettings.dictionary.Add("amount_of_notifications", "100");
                serverCampaign.serverSettings.dictionary.Add("RewardCenter.show_for_one_mission", "true");

                serverCampaign.serverSettings.dictionary.Add("bg_color", "#124674");
                serverCampaign.serverSettings.dictionary.Add("bg_color2", "#124674");
                serverCampaign.serverSettings.dictionary.Add("link_color", "#AAAAFF");
                serverCampaign.serverSettings.dictionary.Add("text_color", "#FFFFFF");
                serverCampaign.serverSettings.dictionary.Add("bg_border_color", "#FFFFFF");
                serverCampaign.serverSettings.dictionary.Add("RewardCenter.reward_text_color", "#2196F3");

                serverCampaign.serverSettings.dictionary.Add("CongratsNotification.button_text", "Awesome!");
                serverCampaign.serverSettings.dictionary.Add("CongratsNotification.content_text", "You have earned <b>%ingame_reward%</b> from Monetizr");
                serverCampaign.serverSettings.dictionary.Add("CongratsNotification.header_text", "Get your awesome reward!");

                serverCampaign.serverSettings.dictionary.Add("StartNotification.SurveyReward.header_text", "<b>Survey by Monetizr</b>");
                serverCampaign.serverSettings.dictionary.Add("StartNotification.button_text", "Learn more!");
                serverCampaign.serverSettings.dictionary.Add("StartNotification.content_text", "Join Monetizr<br/>to get game rewards");
                serverCampaign.serverSettings.dictionary.Add("StartNotification.header_text", "<b>Rewards by Monetizr</b>");

                serverCampaign.serverSettings.dictionary.Add("RewardCenter.VideoReward.content_text", "Watch video and get reward %ingame_reward%");

                //create folder with video name

                //dowload video and video player into that folder

                //download videoplayer
                string campPath = Application.persistentDataPath + "/" + serverCampaign.id;

                if (!Directory.Exists(campPath))
                    Directory.CreateDirectory(campPath);

                string zipFolder = campPath + "/" + videoAsset.fpath;

                if (Directory.Exists(zipFolder))
                    ServerCampaign.DeleteDirectory(zipFolder);

                Directory.CreateDirectory(zipFolder);

                byte[] data = await DownloadHelper.DownloadAssetData("https://image.themonetizr.com/videoplayer/html.zip");

                File.WriteAllBytes(zipFolder + "/html.zip", data);

                //ZipFile.ExtractToDirectory(zipFolder + "/html.zip", zipFolder);

                Utils.ExtractAllToDirectory(zipFolder + "/html.zip", zipFolder);

                File.Delete(zipFolder + "/html.zip");

                //--------------

                string index_path = $"{zipFolder}/index.html";

                if (!File.Exists(index_path))
                {
                    Log.PrintError("index.html in video player is not exist!!!");
                }
                else
                {
                    var str = File.ReadAllText(index_path);

                    str = str.Replace("${MON_VAST_COMPONENT}", $"{serverCampaign.vastAdVerificationParams}");

                    File.WriteAllText(index_path, str);
                }

                //---------------

                if (!serverCampaign.HasAssetInList("tiny_teaser"))
                {
                    serverCampaign.assets.Add(new ServerCampaign.Asset()
                    {
                        url = "https://image.themonetizr.com/default_assets/monetizr_teaser.gif",
                        type = "tiny_teaser_gif",

                    });
                }

                /*serverCampaign.assets.Add(new ServerCampaign.Asset()
                {
                    url = "https://image.themonetizr.com/default_assets/monetizr_banner.jpg",
                    type = "banner",

                });*/

                /*if (!serverCampaign.HasAssetInList("logo"))
                {
                    serverCampaign.assets.Add(new ServerCampaign.Asset()
                    {
                        //url = "https://image.themonetizr.com/default_assets/monetizr_logo.png",
                        url = "https://storage.googleapis.com/middleware-media-files/challenge_asset/64072ae1-4d45-4037-b704-f68b6411caf9.png",
                        type = "logo",

                    });
                }*/

            }

            return serverCampaign;
        }

        private string ConvertCreativeToExt(string type, string url)
        {
            if (Path.HasExtension(url))
            {
                //remove starting dot
                return Path.GetExtension(url).Substring(1);
            }

            int i = type.LastIndexOf('/');

            return type.Substring(i + 1);
        }

        private string ConvertCreativeToFname(string url)
        {
            int i = url.LastIndexOf('=');

            if (i <= 0)
                return Path.GetFileNameWithoutExtension(url);

            return url.Substring(i + 1);
        }

        internal VAST CreateVastFromXml(string xml)
        {
            //XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            //xmlDoc.LoadXml(webRequest.downloadHandler.text);

            VAST vastData = null;

            try
            {
                var ser = new XmlSerializer(typeof(VAST));

                using (var reader = new StringReader(xml))
                {
                    vastData = (VAST)ser.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Log.Print(e);
            }

            return vastData;
        }

        internal async Task GetCampaign(List<ServerCampaign> campList)
        {
            VastParams vp = GetVastParams();

            if (vp == null)
                return;

            //string uri = $"https://servedbyadbutler.com/vast.spark?setID=31328&ID=184952&pid=165154";
            //https://servedbyadbutler.com/vast.spark?setID=31328&ID=184952&pid=165154

            //string uri = $"https://servedbyadbutler.com/vast.spark?setID={vp.setID}&ID={vp.id}&pid={vp.pid}";

            string uri = "https://vast-serve-stineosy7q-uc.a.run.app";

            Log.Print($"Requesting VAST campaign with url {uri}");



            string res = null;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                await webRequest.SendWebRequest();

                //Log.Print(webRequest.downloadHandler.text);



                res = webRequest.downloadHandler.text;
            }

            /*XmlNodeList elemList = xmlDoc.GetElementsByTagName("Creative");
            for (int i = 0; i < elemList.Count; i++)
            {
                Log.Print($"{i}------{elemList[i].InnerXml}");
            }*/

            VAST vastData = CreateVastFromXml(res);


            //Log.Print(v.Ad[0].Item.GetType());

            ServerCampaign serverCampaign = await PrepareServerCampaign(vastData);

            if (serverCampaign != null)
                campList.Add(serverCampaign);

        }
    }
}
