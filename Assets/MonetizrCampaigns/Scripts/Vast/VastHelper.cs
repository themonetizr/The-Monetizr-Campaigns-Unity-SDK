using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

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

        internal async Task<ServerCampaign> PrepareServerCampaign(VAST v)
        {
            if (v.Ad == null || v.Ad.Length == 0)
                return null;

            //ServerCampaign serverCampaign = new ServerCampaign() { id = $"{v.Ad[0].id}-{UnityEngine.Random.Range(1000,2000)}", dar_tag = "" };
            ServerCampaign serverCampaign = new ServerCampaign() { id = $"{v.Ad[0].id}", dar_tag = "" };


            if (!(v.Ad[0].Item is VASTADInLine))
                return null;

            bool hasSettings = false;
            bool hasVideo = false;

            ServerCampaign.Asset videoAsset = null;

            VASTADInLine inLine = (VASTADInLine)v.Ad[0].Item;

            //serverCampaign.id = v.Ad[0].id;

            foreach (var c in inLine.Creatives)
            {
                ServerCampaign.Asset asset = null;

                if (c.Item is VASTADInLineCreativeNonLinearAds)
                {
                    VASTADInLineCreativeNonLinearAds it = (VASTADInLineCreativeNonLinearAds)c.Item;

                    foreach (var nl in it.NonLinear)
                    {
                        if (nl.Item is NonLinear_typeStaticResource)
                        {
                            NonLinear_typeStaticResource staticRes = (NonLinear_typeStaticResource)nl.Item;

                            //Log.Print($"{staticRes.Value}");

                            asset = new ServerCampaign.Asset()
                            {
                                id = $"{c.id} {nl.id}",
                                url = staticRes.Value,
                                type = nl.AdParameters,
                                fname = ConvertCreativeToFname(staticRes.Value),
                                fext = ConvertCreativeToExt(staticRes.creativeType, staticRes.Value),
                            };

                            //Log.Print(asset.ToString());

                            serverCampaign.assets.Add(asset);
                        }
                    }

                }
                else if (c.Item is VASTADInLineCreativeLinear)
                {
                    VASTADInLineCreativeLinear it = (VASTADInLineCreativeLinear)c.Item;

                    Log.Print(it.MediaFiles[0].Value);

                    Log.Print(it.AdParameters);

                    videoAsset = new ServerCampaign.Asset()
                    {
                        id = c.id,
                        url = it.MediaFiles[0].Value,
                        fpath = ConvertCreativeToFname(it.MediaFiles[0].Value),
                        fname = "video",
                        fext = ConvertCreativeToExt(it.MediaFiles[0].type, it.MediaFiles[0].Value),
                        type = "html",
                        mainAssetName = $"{ConvertCreativeToFname(it.MediaFiles[0].Value)}/index.html"
                    };

                    serverCampaign.assets.Add(videoAsset);

                    hasVideo = true;

                    //Log.Print(asset.ToString());

                    if (it.AdParameters != null)
                    {
                        it.AdParameters = it.AdParameters.Replace("\n", "");

                        var dict = Json.Deserialize(it.AdParameters) as Dictionary<string, object>;

                        serverCampaign.serverSettings = new SettingsDictionary<string, string>(Utils.ParseContentString(it.AdParameters, dict));
                    }
                }
                else if (c.Item is VASTADInLineCreativeCompanionAds)
                {

                }


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

                serverCampaign.serverSettings.dictionary.Add("StartNotification.SurveyReward.header_text","<b>Survey by Monetizr</b>");
                serverCampaign.serverSettings.dictionary.Add("StartNotification.button_text","Learn more!");
                serverCampaign.serverSettings.dictionary.Add("StartNotification.content_text","Join Monetizr<br/>to get game rewards");
                serverCampaign.serverSettings.dictionary.Add("StartNotification.header_text","<b>Rewards by Monetizr</b>");

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

                ZipFile.ExtractToDirectory(zipFolder + "/html.zip", zipFolder);

                File.Delete(zipFolder + "/html.zip");

                //--------------

                if (!serverCampaign.HasAssetInList("tiny_teaser"))
                {
                    serverCampaign.assets.Add(new ServerCampaign.Asset()
                    {
                        url = "https://image.themonetizr.com/default_assets/monetizr_teaser.gif",
                        type = "tiny_teaser_gif",

                    });
                }

                serverCampaign.assets.Add(new ServerCampaign.Asset()
                {
                    url = "https://image.themonetizr.com/default_assets/monetizr_banner.jpg",
                    type = "banner",

                });

                if (!serverCampaign.HasAssetInList("logo"))
                {
                    serverCampaign.assets.Add(new ServerCampaign.Asset()
                    {
                        //url = "https://image.themonetizr.com/default_assets/monetizr_logo.png",
                        url = "https://storage.googleapis.com/middleware-media-files/challenge_asset/64072ae1-4d45-4037-b704-f68b6411caf9.png",
                        type = "logo",

                    });
                }

            }

            return serverCampaign;
        }

        private string ConvertCreativeToExt(string type, string url)
        {
            if (Path.HasExtension(url))
            {
                return Path.GetExtension(url);
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

            string uri = $"https://servedbyadbutler.com/vast.spark?setID={vp.setID}&ID={vp.id}&pid={vp.pid}";

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
