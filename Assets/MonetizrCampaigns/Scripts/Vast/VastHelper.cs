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
    class VastHelper
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

                            //Debug.Log($"{staticRes.Value}");

                            asset = new ServerCampaign.Asset()
                            {
                                id = $"{c.id} {nl.id}",
                                url = staticRes.Value,
                                type = nl.AdParameters,
                                fname = ConvertCreativeToFname(staticRes.Value),
                                fext = ConvertCreativeToExt(staticRes.creativeType),
                            };

                            //Debug.Log(asset.ToString());

                            serverCampaign.assets.Add(asset);
                        }
                    }

                }
                else if (c.Item is VASTADInLineCreativeLinear)
                {
                    VASTADInLineCreativeLinear it = (VASTADInLineCreativeLinear)c.Item;

                    Debug.Log(it.MediaFiles[0].Value);

                    Debug.Log(it.AdParameters);

                    videoAsset = new ServerCampaign.Asset()
                    {
                        id = c.id,
                        url = it.MediaFiles[0].Value,
                        fpath = ConvertCreativeToFname(it.MediaFiles[0].Value),
                        fname = "video",
                        fext = ConvertCreativeToExt(it.MediaFiles[0].type),
                        type = "html",
                        mainAssetName = $"{ConvertCreativeToFname(it.MediaFiles[0].Value)}/index.html"
                    };

                    serverCampaign.assets.Add(videoAsset);

                    hasVideo = true;

                    //Debug.Log(asset.ToString());

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
                serverCampaign.serverSettings.dictionary.Add("bg_color2","#124674");
                serverCampaign.serverSettings.dictionary.Add("link_color","#AAAAFF");
                serverCampaign.serverSettings.dictionary.Add("text_color","#FFFFFF");
                serverCampaign.serverSettings.dictionary.Add("bg_border_color","#FFFFFF");
                serverCampaign.serverSettings.dictionary.Add("RewardCenter.reward_text_color","#2196F3");

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
       
                serverCampaign.assets.Add(new ServerCampaign.Asset()
                {
                    url = "https://image.themonetizr.com/default_assets/monetizr_teaser.gif",
                    type = "tiny_teaser_gif",

                });

                serverCampaign.assets.Add(new ServerCampaign.Asset()
                {
                    url = "https://image.themonetizr.com/default_assets/monetizr_banner.jpg",
                    type = "banner",

                });

                serverCampaign.assets.Add(new ServerCampaign.Asset()
                {
                    url = "https://image.themonetizr.com/default_assets/monetizr_logo.png",
                    type = "logo",

                });

            }

            return serverCampaign;
        }

        private string ConvertCreativeToExt(string type)
        {
           return type.Substring(type.LastIndexOf('/') + 1);
        }

        private string ConvertCreativeToFname(string url)
        {
            return url.Substring(url.LastIndexOf('=') + 1);
        }

        internal async Task GetVastCampaign(List<ServerCampaign> campList)
        {
            VastParams vp = GetVastParams();
            
            if (vp == null)
                return;

            //string uri = $"https://servedbyadbutler.com/vast.spark?setID=31328&ID=184952&pid=165154";
            //https://servedbyadbutler.com/vast.spark?setID=31328&ID=184952&pid=165154

            string uri = $"https://servedbyadbutler.com/vast.spark?setID={vp.setID}&ID={vp.id}&pid={vp.pid}";

            Debug.Log($"Requesting VAST campaign with url {uri}");

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            string res = null;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                await webRequest.SendWebRequest();

                //Debug.Log(webRequest.downloadHandler.text);

                xmlDoc.LoadXml(webRequest.downloadHandler.text);

                res = webRequest.downloadHandler.text;
            }

            /*XmlNodeList elemList = xmlDoc.GetElementsByTagName("Creative");
            for (int i = 0; i < elemList.Count; i++)
            {
                Debug.Log($"{i}------{elemList[i].InnerXml}");
            }*/

            VAST vastData = null;

            var ser = new XmlSerializer(typeof(VAST));

            using (var reader = new StringReader(res))
            {
                vastData = (VAST)ser.Deserialize(reader);
            }


            //Debug.Log(v.Ad[0].Item.GetType());

            ServerCampaign serverCampaign = await PrepareServerCampaign(vastData);

            if(serverCampaign != null)
                campList.Add(serverCampaign);
         
        }
    }
}
