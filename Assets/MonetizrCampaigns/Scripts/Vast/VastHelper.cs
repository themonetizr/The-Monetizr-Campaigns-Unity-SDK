                using System;
                using System.Collections.Generic;
                using System.IO;
                using System.IO.Compression;
                using System.Linq;
                using System.Net.Http;
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
                        internal MonetizrClient client;
                        //internal VastSettings vastSettings;

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
                        internal class VideoSettings
                        {
                            public bool isSkippable = true;
                            public string skipOffset = "";
                            public bool isAutoPlay = true;
                            public string position = "preroll";
                            public string videoUrl = "";
                        }
                                

                        /*[System.Serializable]
                        internal class VastSettings
                        {
                            public OtherSettings otherSettings;
                            public VastSettings adVerifications;
                        }*/

                        [System.Serializable]
                        internal class VastSettings
                        {
                            public string vendorName = "Themonetizr";
                            public string sdkVersion = MonetizrManager.SDKVersion;

                            public VideoSettings videoSettings;

                            public List<AdVerification> adVerifications = new List<AdVerification>();
                        }

                        [System.Serializable]
                        internal class AdVerification
                        {
                            public string verificationParameters = "";
                            public string vendorField = "";
                            public List<VerificationExecutableResource> executableResource = new List<VerificationExecutableResource>();
                            public List<VerificationJavaScriptResource> javaScriptResource = new List<VerificationJavaScriptResource>();
                            public List<TrackingEvent> tracking = new List<TrackingEvent>();

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
                        internal class TrackingEvent
                        {
                            public string @event = "";
                            public string value = "";

                            public TrackingEvent()
                            {
                            }

                            public TrackingEvent(TrackingEvents_Verification_typeTracking te)
                            {
                                @event = te.@event;
                                value = te.Value;
                            }

                            public TrackingEvent(TrackingEvents_typeTracking te)
                            {
                                @event = te.@event.ToString();
                                value = te.Value;
                            }

                            public TrackingEvent(string eventTitle, string value)
                            {
                                this.@event = eventTitle;
                                this.value = value;
                            }

                            public static TrackingEvent CreateImpressionEvent(Impression_type impressionType)
                            {
                                return new TrackingEvent("impression", impressionType.Value);
                            }
                        }
                                            

                        internal static void AddVastSettings(VastSettings _vastSettings, Verification_type[] adVerifications, string _skipOffset, string _videoUrl)
                        {
                            //adVerifications = null;

                            if (adVerifications.IsNullOrEmpty())
                            {
                                Log.PrintWarning("AdVerifications is not defined in the VAST xml!");
                                    
                            }

                            if(!adVerifications.IsNullOrEmpty())
                                foreach (var av in adVerifications)
                                {
                                    //Log.Print($"Vendor: [{av.vendor}] VerificationParameters: [{av.VerificationParameters}]");

                                    var jsrList = Utils.CreateListFromArray<Verification_typeJavaScriptResource, VerificationJavaScriptResource>(
                                        av.JavaScriptResource,
                                        (Verification_typeJavaScriptResource jsr) => { return new VerificationJavaScriptResource(jsr); },
                                        new VerificationJavaScriptResource());

                                    var trackingList = Utils.CreateListFromArray<TrackingEvents_Verification_typeTracking, TrackingEvent>(
                                        av.TrackingEvents,
                                        (TrackingEvents_Verification_typeTracking te) => { return new TrackingEvent(te); },
                                        new TrackingEvent());

                                    var execList = Utils.CreateListFromArray<Verification_typeExecutableResource, VerificationExecutableResource>(
                                        av.ExecutableResource,
                                        (Verification_typeExecutableResource er) => { return new VerificationExecutableResource(er); },
                                        new VerificationExecutableResource());
                                    
                                    _vastSettings.adVerifications.Add(new AdVerification()
                                    {
                                        executableResource = execList,
                                        javaScriptResource = jsrList,
                                        tracking = trackingList,
                                        vendorField = av.vendor,
                                        verificationParameters = av.VerificationParameters
                                    });
                                }

                            //string s = Json.Serialize(adv.adVerifications);

                            _vastSettings.videoSettings = new VideoSettings() { skipOffset = _skipOffset, videoUrl = _videoUrl };
                            

                        }

                        private static string DumpsVastSettings(VastSettings _vastSettings, List<TrackingEvent> _trackingEvents)
                        {
                            string res = JsonUtility.ToJson(_vastSettings);

                            //TODO: insert at the end
                            //}-> ,{"trackingEvents":{"name1":"url1","name2":"url2",...}}

                            string trackingEventsJson = ",\"trackingEvents\":{";

                            //foreach (var te in events)
                            for (int i = 0; i < _trackingEvents.Count; i++)
                            {
                                var te = _trackingEvents[i];

                                trackingEventsJson += $"\"{te.@event}\":\"{te.value}\"";

                                if (i < _trackingEvents.Count - 1)
                                    trackingEventsJson += ",";
                            }

                            trackingEventsJson += "}";

                            res = res.Insert(res.Length - 1, trackingEventsJson);

                            Log.Print($"settings: {res}");
                      
                            return res;
                        }

                        internal SettingsDictionary<string, string> GetDefaultSettingsForProgrammatic()
                        {
                            var serverSettings = new SettingsDictionary<string, string>();

                            serverSettings.dictionary.Add("design_version", "2");
                            serverSettings.dictionary.Add("amount_of_teasers", "100");
                            serverSettings.dictionary.Add("teaser_design_version", "3");
                            serverSettings.dictionary.Add("amount_of_notifications", "100");
                            serverSettings.dictionary.Add("RewardCenter.show_for_one_mission", "true");

                            serverSettings.dictionary.Add("bg_color", "#124674");
                            serverSettings.dictionary.Add("bg_color2", "#124674");
                            serverSettings.dictionary.Add("link_color", "#AAAAFF");
                            serverSettings.dictionary.Add("text_color", "#FFFFFF");
                            serverSettings.dictionary.Add("bg_border_color", "#FFFFFF");
                            serverSettings.dictionary.Add("RewardCenter.reward_text_color", "#2196F3");

                            serverSettings.dictionary.Add("CongratsNotification.button_text", "Awesome!");
                            serverSettings.dictionary.Add("CongratsNotification.content_text", "You have earned <b>%ingame_reward%</b> from Monetizr");
                            serverSettings.dictionary.Add("CongratsNotification.header_text", "Get your awesome reward!");

                            serverSettings.dictionary.Add("StartNotification.SurveyReward.header_text", "<b>Survey by Monetizr</b>");
                            serverSettings.dictionary.Add("StartNotification.button_text", "Learn more!");
                            serverSettings.dictionary.Add("StartNotification.content_text", "Join Monetizr<br/>to get game rewards");
                            serverSettings.dictionary.Add("StartNotification.header_text", "<b>Rewards by Monetizr</b>");

                            serverSettings.dictionary.Add("RewardCenter.VideoReward.content_text", "Watch video and get reward %ingame_reward%");

                            return serverSettings;
                        }

                        
                        class VastAdItem
                        {
                            private readonly ServerCampaign _serverCampaign;
                            private readonly Wrapper_type _wrapper;
                            private readonly Inline_type _inline;
                            private readonly Type _type;
                            private readonly VastSettings _vastSettings;
                            private readonly bool _loadVideoOnly;
                            private List<TrackingEvent> _videoTrackingEvents;
                            private readonly AdDefinitionBase_type _baseType;
                            private ServerCampaign.Asset _videoAsset;
                            
                            enum Type
                            {
                                Inline,
                                Wrapper,
                                Unknown
                            }

                            internal bool InUnknownAdType()
                            {
                                return _type == Type.Unknown;
                            }

                            internal ServerCampaign.Asset GetVideoAsset()
                            {
                                return _videoAsset;
                            }
                            internal VastAdItem(AdDefinitionBase_type adDefinition, 
                                ServerCampaign serverCampaign, 
                                VastSettings vastSettings,
                                List<TrackingEvent> videoTrackingEvents,
                                bool loadVideoOnly)
                            {
                                _videoTrackingEvents = videoTrackingEvents;
                                _vastSettings = vastSettings;
                                _serverCampaign = serverCampaign;
                                _baseType = adDefinition;
                                _wrapper = adDefinition as Wrapper_type;
                                _inline = adDefinition as Inline_type;
                                _loadVideoOnly = loadVideoOnly;
                            
                                _type = Type.Unknown;
                                
                                if (_wrapper != null)
                                    _type = Type.Wrapper;
                                
                                if (_inline != null)
                                    _type = Type.Inline;

                            }


                            public void AssignCreativesIntoAssets()
                            {
                                switch (_type)
                                {
                                    case Type.Inline:
                                        AddInlineCreativesIntoAssets();
                                        break;
                                    case Type.Wrapper:
                                        AddWrapperCreativesIntoAssets();
                                        break;
                                    case Type.Unknown:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                
                                foreach (var e in _baseType.Impression)
                                {
                                    var impEvent = TrackingEvent.CreateImpressionEvent(e);
                                    _videoTrackingEvents.Add(impEvent);
                                }
                            }

                            private void AddWrapperCreativesIntoAssets()
                            {
                                var adItem = _wrapper;
                                
                                foreach (var c in adItem.Creatives)
                                {
                                    if (c.Linear == null) continue;
                                    
                                    var it = c.Linear;

                                    Utils.AddArrayToList(
                                        _videoTrackingEvents,
                                        it.TrackingEvents,
                                        te =>
                                        {
                                            return te.Value.IndexOf(".", StringComparison.Ordinal) >= 0 ? new TrackingEvent(te) : null;
                                        },
                                        new TrackingEvent());
                                }
                            }

                            internal string WrapperAdTagUri => _type == Type.Wrapper ? _wrapper.VASTAdTagURI : null;
                            
                            private void AddInlineCreativesIntoAssets()
                            {
                                var adItem = _inline;
                                
                                foreach (var c in adItem.Creatives)
                                {
                                    //load non linear campaions
                                    if (c.NonLinearAds != null && !_loadVideoOnly)
                                    {
                                        var it = c.NonLinearAds;

                                        foreach (var nl in it.NonLinear)
                                        {
                                            AddCampaignAssetFromNonLinearCreative(nl, c, _serverCampaign);
                                        }

                                    }

                                    //load videos
                                    if (c.Linear != null)
                                    {
                                        var it = c.Linear;
                                        
                                        if (it.MediaFiles?.MediaFile == null || it.MediaFiles.MediaFile.Length == 0)
                                        {
                                            Log.Print($"MediaFile is null in Linear creative");
                                            break;
                                        }
                                            
                                        Linear_Inline_typeMediaFilesMediaFile mediaFile = it.MediaFiles.MediaFile[0];

                                        if (it.MediaFiles.MediaFile.Length > 1)
                                        {
                                            mediaFile = Array.Find(it.MediaFiles.MediaFile,
                                                (Linear_Inline_typeMediaFilesMediaFile a) => a.type.Equals("video/mp4"));
                                        }
                                            
                                        string value = mediaFile.Value;
                                        string type = mediaFile.type;
                                        
                                        _videoAsset = new ServerCampaign.Asset()
                                        {
                                            id = c.id,
                                            url = value,
                                            fpath = Utils.ConvertCreativeToFname(value),
                                            fname = "video",
                                            fext = Utils.ConvertCreativeToExt(type, value),
                                            type = "html",
                                            mainAssetName = $"index.html"
                                        };

                                        _serverCampaign.assets.Add(_videoAsset);

                                        var videoUrl = value;
                                        var skipOffset = it.skipoffset;
                                            
                                        Utils.AddArrayToList(
                                            _videoTrackingEvents,
                                            it.TrackingEvents,
                                            te =>
                                            {
                                                return te.Value.IndexOf(".", StringComparison.Ordinal) >= 0 ? new TrackingEvent(te) : null;
                                            },
                                            new TrackingEvent());
                                            
                                        //Log.Print(asset.ToString());
                                        var adParameters = it.AdParameters;

                                        LoadCampagnSettingsFromAdParams(it.AdParameters, _serverCampaign);
                                        
                                        AddVastSettings(_vastSettings, adItem.AdVerifications, skipOffset, videoUrl);

                                    }

                                }
                                
                                
                            }
                        }
                        internal async Task<ServerCampaign> PrepareServerCampaign(string campaignId, string vastContent, bool videoOnly = false)
                        {
                            //ServerCampaign serverCampaign = new ServerCampaign() { id = $"{v.Ad[0].id}-{UnityEngine.Random.Range(1000,2000)}", dar_tag = "" };
                            ServerCampaign serverCampaign = new ServerCampaign(
                                campaignId,
                                "",
                                GetDefaultSettingsForProgrammatic()
                            );
                         
                            var videoTrackingEvents = new List<TrackingEvent>();
                            var vastSettings = new VastSettings();
                            
                            //---
                            
                            if (!await LoadVastContent(vastContent, videoOnly, serverCampaign, vastSettings, videoTrackingEvents)) 
                                return null;

                            //-----
                            
                            string vastJsonSettings = DumpsVastSettings(vastSettings, videoTrackingEvents);
                            
                            
                            Log.Print($"Vast settings: {vastJsonSettings}");

                            serverCampaign.vastAdVerificationParams = vastJsonSettings;

                            await InitializeOMSDK(serverCampaign.vastAdVerificationParams);

                            Log.Print("Loading video player");

                            await PrepareVideoAsset(serverCampaign);

                            
                            
                            return serverCampaign;
                        }

                        private async Task<bool> LoadVastContent(string vastContent, bool videoOnly, ServerCampaign serverCampaign,
                            VastSettings vastSettings, List<TrackingEvent> videoTrackingEvents)
                        {
                            VAST vastData = CreateVastFromXml(vastContent);

                            if (vastData == null)
                            {
                                Log.PrintError("Vast isn't loaded");
                                return false;
                            }

                            if (vastData.Items == null || vastData.Items.Length == 0)
                                return false;

                            //load only first ad item
                            if (!(vastData.Items[0] is VASTAD vad))
                                return false;

                            var adItem = new VastAdItem(vad.Item,
                                serverCampaign,
                                vastSettings,
                                videoTrackingEvents,
                                videoOnly);

                            if (adItem.InUnknownAdType())
                                return false;

                            adItem.AssignCreativesIntoAssets();

                            if (!string.IsNullOrEmpty(adItem.WrapperAdTagUri))
                            {
                                Log.Print($"Loading wrapper with the url {adItem.WrapperAdTagUri}");
                                
                                var result = await MonetizrClient.DownloadUrlAsString(new HttpRequestMessage(HttpMethod.Get, adItem.WrapperAdTagUri));

                                if (!result.isSuccess)
                                    return false;

                                if (!await LoadVastContent(result.content, videoOnly, serverCampaign, vastSettings,
                                        videoTrackingEvents))
                                    return false;
                                
                                //var _vastData = CreateVastFromXml(result.content);
                            }

                            return true;
                        }

                        private async Task PrepareVideoAsset(ServerCampaign serverCampaign)
                        {
                            if(!serverCampaign.TryGetAssetInList("html", out var videoAsset))
                                return;
                            
                            serverCampaign.serverSettings.dictionary.Add("custom_missions",
                                    "{'missions': [{'type':'VideoReward','percent_amount':'100','id':'5'}]}");

                                await DownloadAndPrepareHtmlVideoPlayer(serverCampaign, videoAsset);

                                //---------------

                                if (!serverCampaign.HasAssetInList("tiny_teaser"))
                                {
                                    serverCampaign.assets.Add(new ServerCampaign.Asset()
                                    {
                                        url = "https://image.themonetizr.com/default_assets/monetizr_teaser.gif",
                                        type = "tiny_teaser_gif",
                                    });
                                }
                            
                        }

                        private static void LoadCampagnSettingsFromAdParams(AdParameters_type adParameters, ServerCampaign serverCampaign)
                        {
                            if (adParameters == null) return;
                            
                            Log.Print(adParameters.Value);

                            string adp = adParameters.Value;

                            adp = adp.Replace("\n", "");

                            var dict = Json.Deserialize(adp) as Dictionary<string, object>;

                            var parsedDict = Utils.ParseContentString("", dict);

                            foreach (var i in parsedDict)
                            {
                                serverCampaign.serverSettings.dictionary.Add(i.Key, i.Value);

                                Log.Print($"Additional settings from AdParameters [{i.Key}:{i.Value}]");
                            }

                            //serverCampaign.serverSettings = new SettingsDictionary<string, string>(Utils.ParseContentString(adp, dict));
                        }

                        private static async Task DownloadAndPrepareHtmlVideoPlayer(ServerCampaign serverCampaign, ServerCampaign.Asset videoAsset)
                        {
                            string campPath = Application.persistentDataPath + "/" + serverCampaign.id;

                            string zipFolder = campPath + "/" + videoAsset.fpath;

                            Log.Print($"{campPath} {zipFolder}");

                            //if (Directory.Exists(zipFolder))
                            //    ServerCampaign.DeleteDirectory(zipFolder);

                            if (!Directory.Exists(zipFolder))
                                Directory.CreateDirectory(zipFolder);

                            byte[] data = await DownloadHelper.DownloadAssetData("https://image.themonetizr.com/videoplayer/html.zip");

                            File.WriteAllBytes(zipFolder + "/html.zip", data);

                            //ZipFile.ExtractToDirectory(zipFolder + "/html.zip", zipFolder);

                            Utils.ExtractAllToDirectory(zipFolder + "/html.zip", zipFolder);

                            File.Delete(zipFolder + "/html.zip");

                            //--------------

                            string indexPath = $"{zipFolder}/index.html";

                            if (!File.Exists(indexPath))
                            {
                                Log.PrintError("index.html in video player is not exist!!!");
                            }
                            else
                            {
                                var str = File.ReadAllText(indexPath);

                                str = str.Replace("\"${MON_VAST_COMPONENT}\"", $"{serverCampaign.vastAdVerificationParams}");

                                File.WriteAllText(indexPath, str);
                            }
                        }

                        private static void AddCampaignAssetFromNonLinearCreative(NonLinearAd_Inline_type nl, Creative_Inline_type c,
                            ServerCampaign serverCampaign)
                        {
                            string adParameters;

                            //No parameters
                            //TODO: define 
                            if (nl.AdParameters == null || string.IsNullOrEmpty(nl.AdParameters.Value))
                            {
                                adParameters = "tiny_teaser";
                            }
                            else
                            {
                                adParameters = nl.AdParameters.Value;
                            }

                            var staticRes = nl.StaticResource[0];

                            //Log.Print($"{staticRes.Value}");

                            ServerCampaign.Asset asset = new ServerCampaign.Asset()
                            {
                                id = $"{c.id} {nl.id}",
                                url = staticRes.Value,
                                type = adParameters,
                                fname = Utils.ConvertCreativeToFname(staticRes.Value),
                                fext = Utils.ConvertCreativeToExt(staticRes.creativeType, staticRes.Value),
                            };

                            //Log.Print(asset.ToString());

                            serverCampaign.assets.Add(asset);

                            /*ServerCampaign.Asset a2 = asset.Clone();
                                         a2.type = "banner";
                                         serverCampaign.assets.Add(a2);
                                         
                                         ServerCampaign.Asset a3 = asset.Clone();
                                         a3.type = "logo";
                                         serverCampaign.assets.Add(a3);*/
                        }

                        //TODO!
                        private async Task InitializeOMSDK(string vastAdVerificationParams)
                        {
                            byte[] data = await DownloadHelper.DownloadAssetData("https://image.themonetizr.com/omsdk/omsdk-v1.js");

                            if (data == null)
                            {
                                Log.PrintWarning("Download of omsdk-v1.js failed!");
                                return;
                            }

                            string omidJSServiceContent = Encoding.UTF8.GetString(data);

                            //Log.Print($"InitializeOMSDK {vastAdVerificationParams}");
                            Log.Print($"InitializeOMSDK {omidJSServiceContent}");

                            UniWebViewInterface.InitOMSDK(vastAdVerificationParams, omidJSServiceContent);
                        }

                        public class XmlReaderNoNamespaces : XmlTextReader
                        {
                            public XmlReaderNoNamespaces(StringReader stream) : base(stream)
                            {
                            }

                            public override string Name => LocalName;

                            public override string NamespaceURI => string.Empty;

                            public override string Prefix => string.Empty;
                        }
                        
                        internal VAST CreateVastFromXml(string xml)
                        {
                            if (string.IsNullOrEmpty(xml))
                                return null;
                            
                            VAST vastData = null;
                            
                            var ser = new XmlSerializer(typeof(VAST));
                            
                            using (var streamReader = new StringReader(xml))
                            using (var reader = new XmlReaderNoNamespaces(streamReader))
                            {
                                vastData = (VAST)ser.Deserialize(reader);
                            }

                            return vastData;
                        }

                        /*internal async Task GetCampaign(List<ServerCampaign> campList, HttpClient httpClient)
                        {
                            VastParams vp = GetVastParams();

                            if (vp == null)
                                return;

                            //string uri = $"https://servedbyadbutler.com/vast.spark?setID=31328&ID=184952&pid=165154";
                            //https://servedbyadbutler.com/vast.spark?setID=31328&ID=184952&pid=165154

                            //string uri = $"https://servedbyadbutler.com/vast.spark?setID={vp.setID}&ID={vp.id}&pid={vp.pid}";

                            //Pubmatic VAST
                            //string uri = "https://programmatic-serve-stineosy7q-uc.a.run.app/?test&native"; 

                                
                            //OMSDK certification site
                            string uri = "https://vast-serve-stineosy7q-uc.a.run.app";

                            Log.Print($"Requesting VAST campaign with url {uri}");

                            var requestMessage = MonetizrClient.GetHttpRequestMessage(uri);
                                
                            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                            var res = await response.Content.ReadAsStringAsync();

                            Log.Print($"Proxy response is: {res} {response.StatusCode}");
                            Log.Print(res);

                            if (!response.IsSuccessStatusCode)
                                return;

                            if (res.Length == 0)
                                return;

                            VAST vastData = CreateVastFromXml(res);


                            //Log.Print(v.Ad[0].Item.GetType());

                            if (vastData != null)
                            {
                                Log.Print("VAST data successfully loaded!");
                            }

                            ServerCampaign serverCampaign = await PrepareServerCampaign(null,res);

                            if (serverCampaign == null)
                            {
                                Log.Print("PrepareServerCampaign failed!");
                                return;
                            }
                                
                            if (serverCampaign != null)
                                campList.Add(serverCampaign);

                        }*/
                    }
                }