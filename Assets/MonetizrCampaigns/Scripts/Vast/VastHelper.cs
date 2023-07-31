using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
//using UnityEditor.Android;
using UnityEngine;
using Monetizr.Campaigns.Vast42;
using Vector2 = UnityEngine.Vector2;

namespace Monetizr.Campaigns
{
    internal class VastHelper
    {
        internal static MonetizrClient client;
        //internal VastSettings vastSettings;

        internal class VastParams
        {
            internal int setID;
            internal int id;
            internal int pid;
        }

        internal VastHelper(MonetizrClient _client)
        {
            client = _client;
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

            if (!adVerifications.IsNullOrEmpty())
                foreach (var av in adVerifications)
                {
                    //Log.PrintV($"Vendor: [{av.vendor}] VerificationParameters: [{av.VerificationParameters}]");

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

        private static string DumpsVastSettings(VastSettings _vastSettings,
            List<TrackingEvent> _trackingEvents)
        {
            string res = JsonUtility.ToJson(_vastSettings);

            //,{"trackingEvents":[
            //{
            //    "event":"type",
            //    "url":"url"
            //},
            //]}

            string trackingEventsJson = ",\"trackingEvents\":[";

            //foreach (var te in events)
            for (int i = 0; i < _trackingEvents.Count; i++)
            {
                var te = _trackingEvents[i];

                trackingEventsJson += $"{{\"event\":\"{te.@event}\",\"url\":\"{te.value}\"}}";

                if (i < _trackingEvents.Count - 1)
                    trackingEventsJson += ",";
            }

            trackingEventsJson += "]";

            res = res.Insert(res.Length - 1, trackingEventsJson);

            Log.PrintV($"settings: {res}");

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
            internal struct PreferableVideoSize
            {
                internal int bitrate;
                internal int width;
                internal int height;

                public PreferableVideoSize(int bitrate, int width, int height)
                {
                    this.bitrate = bitrate;
                    this.width = width;
                    this.height = height;
                }
            }

            private readonly ServerCampaign _serverCampaign;
            private readonly Wrapper_type _wrapper;
            private readonly Inline_type _inline;
            private readonly Type _type;
            private readonly VastSettings _vastSettings;
            private readonly bool _loadVideoOnly;
            private List<TrackingEvent> _videoTrackingEvents;
            private readonly AdDefinitionBase_type _baseType;
            private ServerCampaign.Asset _videoAsset;
            private PreferableVideoSize _preferableVideoSize;

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
                PreferableVideoSize preferableVideoSize,
                bool loadVideoOnly)
            {
                _videoTrackingEvents = videoTrackingEvents;
                _vastSettings = vastSettings;
                _serverCampaign = serverCampaign;
                _baseType = adDefinition;
                _wrapper = adDefinition as Wrapper_type;
                _inline = adDefinition as Inline_type;
                _loadVideoOnly = loadVideoOnly;
                _preferableVideoSize = preferableVideoSize;

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
                        AddWrapperCreativesIntoTrackingEvents();
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

            public bool FindExactAssetAndCollectTrackingEvents(string videoFileName)
            {
                bool result = true;

                switch (_type)
                {
                    case Type.Inline:
                        result = FindVideoInLinearCreativesAndGrabEvents(videoFileName);
                        break;
                    case Type.Wrapper:
                        AddWrapperCreativesIntoTrackingEvents();
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

                return result;
            }

            private void AddWrapperCreativesIntoTrackingEvents()
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
                            Log.PrintV($"MediaFile is null in Linear creative");
                            break;
                        }

                        Linear_Inline_typeMediaFilesMediaFile mediaFile = it.MediaFiles.MediaFile[0];

                        float w = (float)_preferableVideoSize.width;
                        float h = (float)_preferableVideoSize.height;

                        Vector2 prefSize = new Vector2(w, h);

                        //choose media file close to preferable size and bitrate
                        Array.Sort(it.MediaFiles.MediaFile, (m1, m2) =>
                        {
                            if (m1 == null || m2 == null ||
                                string.IsNullOrEmpty(m1.width) || string.IsNullOrEmpty(m1.height) ||
                                string.IsNullOrEmpty(m2.width) || string.IsNullOrEmpty(m2.height))
                                return 0;

                            Vector2 v1 = new Vector2(float.Parse(m1.width), float.Parse(m1.height));
                            Vector2 v2 = new Vector2(float.Parse(m2.width), float.Parse(m2.height));

                            int compareSize = Vector2.Distance(v1, prefSize).CompareTo(Vector2.Distance(v2, prefSize));

                                            //if the same size, take a look on bit rate
                                            if (compareSize == 0)
                            {
                                if (string.IsNullOrEmpty(m1.bitrate) ||
                                    string.IsNullOrEmpty(m2.bitrate))
                                    return 0;

                                int br1 = Math.Abs(int.Parse(m1.bitrate) - _preferableVideoSize.bitrate);
                                int br2 = Math.Abs(int.Parse(m2.bitrate) - _preferableVideoSize.bitrate);

                                int result = br1.CompareTo(br2);
                                return result;
                            }

                            return compareSize;
                        });

                        if (it.MediaFiles.MediaFile.Length > 1)
                        {
                            mediaFile = Array.Find(it.MediaFiles.MediaFile,
                                (Linear_Inline_typeMediaFilesMediaFile a) =>
                                {
                                    return a.type.Equals("video/mp4");
                                });
                        }

                        Log.PrintV($"Chosen video file - type:{mediaFile.type} br:{mediaFile.bitrate} w:{mediaFile.width} h:{mediaFile.height} ");

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

                        //Log.PrintV(asset.ToString());
                        var adParameters = it.AdParameters;

                        LoadCampagnSettingsFromAdParams(it.AdParameters, _serverCampaign);

                        AddVastSettings(_vastSettings, adItem.AdVerifications, skipOffset, videoUrl);

                    }

                }


            }

            private bool FindVideoInLinearCreativesAndGrabEvents(string videoName)
            {
                var adItem = _inline;

                foreach (var c in adItem.Creatives)
                {
                    //load videos
                    if (c.Linear == null) continue;
                    
                    var it = c.Linear;

                    if (it.MediaFiles?.MediaFile == null || it.MediaFiles.MediaFile.Length == 0)
                    {
                        Log.PrintV($"MediaFile is null in Linear creative");
                        return false;
                    }

                    Linear_Inline_typeMediaFilesMediaFile mediaFile = null;

                    if (it.MediaFiles.MediaFile.Length == 0)
                        return false;
                    
                    if (it.MediaFiles.MediaFile.Length > 1)
                    {
                        mediaFile = Array.Find(it.MediaFiles.MediaFile,
                            (Linear_Inline_typeMediaFilesMediaFile a) => a.Value.Contains(videoName));
                    }

                    //mediaFile = null;
                    
                    if (mediaFile == null)
                    {
                        string filesList = string.Join("\n",it.MediaFiles.MediaFile.Select(x => $"{x.Value}#{x.bitrate}").ToArray());
                        
                        if (client.GlobalSettings.HasParam("openrtb.sent_report_to_mixpanel"))
                            client.analytics.SendOpenRtbReportToMixpanel(filesList, "media error", "media",null);
     
                        mediaFile = it.MediaFiles.MediaFile[0];
                        //return false;
                    }

                    var videoUrl = mediaFile.Value;
                    var skipOffset = it.skipoffset;

                    Utils.AddArrayToList(
                        _videoTrackingEvents,
                        it.TrackingEvents,
                        te =>
                        {
                            return te.Value.IndexOf(".", StringComparison.Ordinal) >= 0 ? new TrackingEvent(te) : null;
                        },
                        new TrackingEvent());
                    
                    var adParameters = it.AdParameters;

                    //LoadCampagnSettingsFromAdParams(it.AdParameters, _serverCampaign);

                    AddVastSettings(_vastSettings, adItem.AdVerifications, skipOffset, videoUrl);

                }

                return true;
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


            Log.PrintV($"Vast settings: {vastJsonSettings}");

            serverCampaign.vastAdParameters = vastJsonSettings;

            await InitializeOMSDK(serverCampaign.vastAdParameters);


            await PrepareVideoAsset(serverCampaign);



            return serverCampaign;
        }

        internal async Task<bool> InitializeServerCampaignForProgrammatic(ServerCampaign campaign, string vastContent)
        {
            if (!campaign.TryGetAssetInList("video", out var video)) 
                return false;


            var videoTrackingEvents = new List<TrackingEvent>();
            var vastSettings = new VastSettings();

            var vastJsonSettings =
                await LoadVastAndFindVideoAsset(vastContent, campaign, video, vastSettings, videoTrackingEvents);

            if (string.IsNullOrEmpty(vastJsonSettings))
                return false;

            Log.PrintV($"Vast settings: {vastJsonSettings}");

            campaign.vastAdParameters = vastJsonSettings;


            campaign.EmbedVastParametersIntoVideoPlayer(video);

            await InitializeOMSDK(campaign.vastAdParameters);



            return true;
        }

        private async Task<string> LoadVastAndFindVideoAsset(string vastContent, ServerCampaign serverCampaign, ServerCampaign.Asset videoAsset, VastSettings vastSettings, List<TrackingEvent> videoTrackingEvents)
        {
            VAST vastData = CreateVastFromXml(vastContent);

            if (vastData == null)
            {
                Log.PrintError("Vast isn't loaded");
                return null;
            }

            if (vastData.Items == null || vastData.Items.Length == 0)
                return null;

            //load only first ad item
            if (!(vastData.Items[0] is VASTAD vad))
                return null;

            int prefBitRate = client.GlobalSettings.GetIntParam("openrtb.pref_bitrate", 10000);
            int prefWidth = client.GlobalSettings.GetIntParam("openrtb.pref_width", 1920);
            int prefHeight = client.GlobalSettings.GetIntParam("openrtb.pref_height", 1080);

            var adItem = new VastAdItem(vad.Item,
                serverCampaign,
                vastSettings,
                videoTrackingEvents,
                new VastAdItem.PreferableVideoSize(prefBitRate, prefWidth, prefHeight),
                true);

            if (adItem.InUnknownAdType())
                return null;

            //adItem.AssignCreativesIntoAssets();

            string videoFileName = videoAsset.fpath;

            if (!adItem.FindExactAssetAndCollectTrackingEvents(videoFileName))
                return null;

            if (string.IsNullOrEmpty(adItem.WrapperAdTagUri))
                return DumpsVastSettings(vastSettings, videoTrackingEvents);

            Log.PrintV($"Loading wrapper with the url {adItem.WrapperAdTagUri}");

            var result = await MonetizrClient.DownloadUrlAsString(new HttpRequestMessage(HttpMethod.Get, adItem.WrapperAdTagUri));

            if (!result.isSuccess)
                return null;

            return await LoadVastAndFindVideoAsset(result.content, serverCampaign, videoAsset, vastSettings, videoTrackingEvents);
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

            int prefBitRate = client.GlobalSettings.GetIntParam("openrtb.pref_bitrate", 10000);
            int prefWidth = client.GlobalSettings.GetIntParam("openrtb.pref_width", 1920);
            int prefHeight = client.GlobalSettings.GetIntParam("openrtb.pref_height", 1080);

            var adItem = new VastAdItem(vad.Item,
                serverCampaign,
                vastSettings,
                videoTrackingEvents,
                new VastAdItem.PreferableVideoSize(prefBitRate, prefWidth, prefHeight),
                videoOnly);

            if (adItem.InUnknownAdType())
                return false;

            adItem.AssignCreativesIntoAssets();

            if (!string.IsNullOrEmpty(adItem.WrapperAdTagUri))
            {
                Log.PrintV($"Loading wrapper with the url {adItem.WrapperAdTagUri}");

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
            Log.PrintV("Loading video player");

            if (!serverCampaign.TryGetAssetInList("html", out var videoAsset))
            {
                return;
            }

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
            if (adParameters == null) 
                return;

            Log.PrintV(adParameters.Value);

            string adp = adParameters.Value;

            adp = adp.Replace("\n", "");

            //var dict = Utils.ParseJson(adp); //Json.Deserialize(adp) as Dictionary<string, object>);

            var parsedDict = Utils.ParseContentString(adp);

            foreach (var i in parsedDict)
            {
                serverCampaign.serverSettings.dictionary.Add(i.Key, i.Value);

                Log.PrintV($"Additional settings from AdParameters [{i.Key}:{i.Value}]");
            }

            //serverCampaign.serverSettings = new SettingsDictionary<string, string>(Utils.ParseContentString(adp, dict));
        }

        private static async Task DownloadAndPrepareHtmlVideoPlayer(ServerCampaign serverCampaign, ServerCampaign.Asset videoAsset)
        {
            string campPath = Application.persistentDataPath + "/" + serverCampaign.id;

            string zipFolder = campPath + "/" + videoAsset.fpath;

            Log.PrintV($"{campPath} {zipFolder}");

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

                str = str.Replace("\"${MON_VAST_COMPONENT}\"", $"{serverCampaign.vastAdParameters}");

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

            //Log.PrintV($"{staticRes.Value}");

            ServerCampaign.Asset asset = new ServerCampaign.Asset()
            {
                id = $"{c.id} {nl.id}",
                url = staticRes.Value,
                type = adParameters,
                fname = Utils.ConvertCreativeToFname(staticRes.Value),
                fext = Utils.ConvertCreativeToExt(staticRes.creativeType, staticRes.Value),
            };

            //Log.PrintV(asset.ToString());

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

            //Log.PrintV($"InitializeOMSDK {vastAdVerificationParams}");
            Log.PrintV($"InitializeOMSDK {omidJSServiceContent}");

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

            Log.PrintV($"Requesting VAST campaign with url {uri}");

            var requestMessage = MonetizrClient.GetHttpRequestMessage(uri);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            var res = await response.Content.ReadAsStringAsync();

            Log.PrintV($"Proxy response is: {res} {response.StatusCode}");
            Log.PrintV(res);

            if (!response.IsSuccessStatusCode)
                return;

            if (res.Length == 0)
                return;

            VAST vastData = CreateVastFromXml(res);


            //Log.PrintV(v.Ad[0].Item.GetType());

            if (vastData != null)
            {
                Log.PrintV("VAST data successfully loaded!");
            }

            ServerCampaign serverCampaign = await PrepareServerCampaign(null,res);

            if (serverCampaign == null)
            {
                Log.PrintV("PrepareServerCampaign failed!");
                return;
            }

            if (serverCampaign != null)
                campList.Add(serverCampaign);

        }*/
    }
}