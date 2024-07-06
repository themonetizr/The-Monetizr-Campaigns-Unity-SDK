using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using CustomUniWebView;

namespace Monetizr.SDK.VAST
{
    internal class VastHelper
    {
        internal static MonetizrClient httpClient;
        internal static string userAgent;
        private string _omidJsServiceContent;

        internal class VastParams
        {
            internal int setID;
            internal int id;
            internal int pid;
        }

        internal VastHelper(MonetizrClient httpClient, string _userAgent)
        {
            VastHelper.httpClient = httpClient;
            userAgent = _userAgent;
        }

        public VastParams GetVastParams()
        {
            if (string.IsNullOrEmpty(httpClient.currentApiKey))
                return null;

            if (httpClient.currentApiKey.Length == 43)
                return null;

            var p = Array.ConvertAll(httpClient.currentApiKey.Split('-'), int.Parse);

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
            public string videoClickThroughUrl = "";

            public VideoSettings()
            {

            }

            public VideoSettings(VideoSettings vidSettings)
            {
                isSkippable = vidSettings.isSkippable;
                skipOffset = vidSettings.skipOffset;
                isAutoPlay = vidSettings.isAutoPlay;
                position = vidSettings.position;
                videoUrl = vidSettings.videoUrl;
            }
        }

        [System.Serializable]
        internal class VastSettings
        {
            public string vendorName = "Themonetizr";
            public string sdkVersion = MonetizrConfiguration.SDKVersion;

            public VideoSettings videoSettings = new VideoSettings();

            public List<AdVerification> adVerifications = new List<AdVerification>();

            public List<TrackingEvent> videoTrackingEvents = new List<TrackingEvent>();

            internal bool IsEmpty()
            {
                return string.IsNullOrEmpty(videoSettings.videoUrl);
            }

            internal VastSettings()
            {

            }

            internal void ReplaceVastTags(TagsReplacer replacer)
            {
                foreach (var a in adVerifications)
                {
                    foreach (var er in a.executableResource)
                    {
                        er.value = replacer.Replace(er.value);
                    }

                    foreach (var jsr in a.javaScriptResource)
                    {
                        jsr.value = replacer.Replace(jsr.value);
                    }

                    foreach (var te in a.tracking)
                    {
                        te.value = replacer.Replace(te.value);
                    }
                }

                foreach (var vte in videoTrackingEvents)
                {
                    vte.value = replacer.Replace(vte.value);
                }

                videoSettings.videoClickThroughUrl = replacer.Replace(videoSettings.videoClickThroughUrl);
            }

            internal VastSettings(VastSettings settingsToCopy)
            {
                vendorName = settingsToCopy.vendorName;
                sdkVersion = settingsToCopy.sdkVersion;

                videoSettings = new VideoSettings(settingsToCopy.videoSettings);

                adVerifications = new List<AdVerification>();
                settingsToCopy.adVerifications.ForEach((v) => adVerifications.Add(new AdVerification(v)));

                videoTrackingEvents = new List<TrackingEvent>();
                settingsToCopy.videoTrackingEvents.ForEach((e) => videoTrackingEvents.Add(new TrackingEvent(e)));
            }
        }

        [System.Serializable]
        internal class AdVerification
        {
            public string verificationParameters = "";
            public string vendorField = "";
            public List<VerificationExecutableResource> executableResource = new List<VerificationExecutableResource>();
            public List<VerificationJavaScriptResource> javaScriptResource = new List<VerificationJavaScriptResource>();
            public List<TrackingEvent> tracking = new List<TrackingEvent>();

            public AdVerification()
            {

            }

            public AdVerification(AdVerification adVerificationToCopy)
            {
                verificationParameters = adVerificationToCopy.verificationParameters;
                vendorField = adVerificationToCopy.vendorField;

                executableResource = new List<VerificationExecutableResource>();
                adVerificationToCopy.executableResource.ForEach(item => executableResource.Add(new VerificationExecutableResource(item)));

                javaScriptResource = new List<VerificationJavaScriptResource>();
                adVerificationToCopy.javaScriptResource.ForEach(item => javaScriptResource.Add(new VerificationJavaScriptResource(item)));

                tracking = new List<TrackingEvent>();
                adVerificationToCopy.tracking.ForEach(item => tracking.Add(new TrackingEvent(item)));
            }
        }

        [System.Serializable]
        internal class VerificationExecutableResource
        {
            public string apiFramework = "";
            public string type = "";
            public string value = "";

            public VerificationExecutableResource(VerificationExecutableResource original)
            {
                apiFramework = original.apiFramework;
                type = original.type;
                value = original.value;
            }

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

            public VerificationJavaScriptResource(VerificationJavaScriptResource original)
            {
                apiFramework = original.apiFramework;
                browserOptional = original.browserOptional;
                browserOptionalSpecified = original.browserOptionalSpecified;
                value = original.value;
            }

            public VerificationJavaScriptResource(string apiFramework, bool browserOptional, bool browserOptionalSpecified, string value)
            {
                this.apiFramework = apiFramework;
                this.browserOptional = browserOptional;
                this.browserOptionalSpecified = browserOptionalSpecified;
                this.value = value;
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

            public TrackingEvent(TrackingEvent trackingEventToCopy)
            {
                @event = trackingEventToCopy.@event;
                value = trackingEventToCopy.value;
            }

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

        internal static void AddVastVerificationSettings(VastSettings _vastSettings, Verification_type[] adVerifications)
        {
            if (adVerifications.IsNullOrEmpty())
            {
                Log.PrintWarning("AdVerifications is not defined in the VAST xml!");
                return;
            }

            foreach (var av in adVerifications)
            {
                var jsrList = MonetizrUtils.CreateListFromArray<Verification_typeJavaScriptResource, VerificationJavaScriptResource>(
                    av.JavaScriptResource,
                    (Verification_typeJavaScriptResource jsr) => { return new VerificationJavaScriptResource(jsr); },
                    new VerificationJavaScriptResource());

                var trackingList = MonetizrUtils.CreateListFromArray<TrackingEvents_Verification_typeTracking, TrackingEvent>(
                    av.TrackingEvents,
                    (TrackingEvents_Verification_typeTracking te) => { return new TrackingEvent(te); },
                    new TrackingEvent());

                var execList = MonetizrUtils.CreateListFromArray<Verification_typeExecutableResource, VerificationExecutableResource>(
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
        }

        internal SettingsDictionary<string, string> GetDefaultSettingsForProgrammatic()
        {
            var settings = new Dictionary<string, string>()
            {
                { "design_version", "2" },
                { "amount_of_teasers", "100" },
                { "teaser_design_version", "3" },
                { "amount_of_notifications", "100" },
                { "RewardCenter.show_for_one_mission", "true" },

                { "bg_color", "#124674" },
                { "bg_color2", "#124674" },
                { "link_color", "#AAAAFF" },
                { "text_color", "#FFFFFF" },
                { "bg_border_color", "#FFFFFF" },
                { "RewardCenter.reward_text_color", "#2196F3" },

                { "CongratsNotification.button_text", "Awesome!" },
                { "CongratsNotification.content_text", "You have earned <b>%ingame_reward%</b> from Monetizr" },
                { "CongratsNotification.header_text", "Get your awesome reward!" },

                { "StartNotification.SurveyReward.header_text", "<b>Survey by Monetizr</b>" },
                { "StartNotification.button_text", "Learn more!" },
                { "StartNotification.content_text", "Join Monetizr<br/>to get game rewards" },
                { "StartNotification.header_text", "<b>Rewards by Monetizr</b>" },

                { "RewardCenter.VideoReward.content_text", "Watch video and get reward %ingame_reward%" }
            };

            return new SettingsDictionary<string, string>(settings);
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
            private readonly bool _loadVideoOnly;
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
            internal VastAdItem (AdDefinitionBase_type adDefinition, ServerCampaign serverCampaign, PreferableVideoSize preferableVideoSize, bool loadVideoOnly)
            {
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
                        LoadExtentions(_inline.Extensions);
                        AddCreativesIntoAssets();
                        break;
                    case Type.Wrapper:
                        LoadExtentions(_wrapper.Extensions);
                        AddWrapperCreativesIntoTrackingEvents();
                        break;
                    case Type.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (_baseType.Impression == null)
                    return;

                foreach (var e in _baseType.Impression)
                {
                    var impEvent = TrackingEvent.CreateImpressionEvent(e);
                    _serverCampaign.vastSettings.videoTrackingEvents.Add(impEvent);
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
                    _serverCampaign.vastSettings.videoTrackingEvents.Add(impEvent);
                }

                return result;
            }

            private void AddVerificationSettingsFromXmlElement(XmlElement element)
            {
                XmlNodeList verificationNodes = element.SelectNodes(".//Verification");

                for (int i = 0; i < verificationNodes.Count; i++)
                {
                    XmlNode verificationNode = verificationNodes.Item(i);
                    if (verificationNode == null) continue;
                    if (verificationNode.Attributes == null) continue;

                    string vendor = verificationNode.Attributes["vendor"]?.Value;

                    XmlNode jsResourceNode = verificationNode.SelectSingleNode("JavaScriptResource");
                    string apiFramework = "";
                    bool browserOptional = false;
                    string jsResource = "";

                    if (jsResourceNode != null)
                    {
                        if (jsResourceNode.Attributes != null)
                        {
                            apiFramework = jsResourceNode.Attributes["apiFramework"]?.Value;
                            bool.TryParse(jsResourceNode.Attributes["browserOptional"]?.Value, out browserOptional);
                        }

                        jsResource = jsResourceNode.InnerText.Trim();
                    }

                    XmlNode verificationParamsNode = verificationNode.SelectSingleNode("VerificationParameters");

                    var verificationParams = verificationParamsNode?.InnerText.Trim();


                    _serverCampaign.vastSettings.adVerifications.Add(new AdVerification()
                    {
                        javaScriptResource = new List<VerificationJavaScriptResource>()
                            {
                                new VerificationJavaScriptResource(apiFramework,browserOptional,false,jsResource)
                            },
                        vendorField = vendor,
                        verificationParameters = verificationParams
                    });
                }
            }

            private void AddWrapperCreativesIntoTrackingEvents()
            {
                var adItem = _wrapper;

                if (adItem.Creatives == null)
                    return;

                foreach (var c in adItem.Creatives)
                {
                    if (c.Linear == null) continue;

                    var it = c.Linear;

                    MonetizrUtils.AddArrayToList(
                        _serverCampaign.vastSettings.videoTrackingEvents,
                        it.TrackingEvents,
                        te =>
                        {
                            return te.Value.IndexOf(".", StringComparison.Ordinal) >= 0 ? new TrackingEvent(te) : null;
                        },
                        new TrackingEvent());
                }
            }

            internal string WrapperAdTagUri => _type == Type.Wrapper ? _wrapper.VASTAdTagURI : null;

            private void LoadExtentions(AdDefinitionBase_typeExtension[] extensions)
            {
                if (extensions.IsNullOrEmpty()) return;

                foreach (var ad in extensions)
                {
                    foreach (var av in ad.Any)
                    {
                        if (av.Name == "MonetizrCampaignSettings")
                        {
                            string campaignSettings = av.InnerText.Trim();

                            var cs = MonetizrUtils.ParseContentString(campaignSettings);

                            if (cs.TryGetValue("content", out var c))
                                _serverCampaign.content = c;
                        }
                        else
                        {
                            AddVerificationSettingsFromXmlElement(av);
                        }
                    }
                }
            }

            private void AddNonLinearCreatives(Creative_Inline_typeNonLinearAds it)
            {
                foreach (var nl in it.NonLinear)
                {
                    AddCampaignAssetFromNonLinearCreative(nl, _serverCampaign);
                }
            }

            private void AddLinearCreatives(Linear_Inline_type it, string cId, Verification_type[] adItemAdVerifications)
            {
                if (ServerCampaign.Asset.ValidateAssetJson(it.AdParameters?.Value))
                {
                    _serverCampaign.assets.Add(new ServerCampaign.Asset(it.AdParameters?.Value, true));
                    return;
                }

                if (it.MediaFiles?.MediaFile == null || it.MediaFiles.MediaFile.Length == 0)
                {
                    Log.PrintV($"MediaFile is null in Linear creative");
                    return;
                }

                Linear_Inline_typeMediaFilesMediaFile mediaFile = it.MediaFiles.MediaFile[0];

                float w = (float)_preferableVideoSize.width;
                float h = (float)_preferableVideoSize.height;

                Vector2 prefSize = new Vector2(w, h);

                Array.Sort(it.MediaFiles.MediaFile, (m1, m2) =>
                {
                    if (m1 == null || m2 == null ||
                        string.IsNullOrEmpty(m1.width) || string.IsNullOrEmpty(m1.height) ||
                        string.IsNullOrEmpty(m2.width) || string.IsNullOrEmpty(m2.height))
                        return 0;

                    Vector2 v1 = new Vector2(float.Parse(m1.width), float.Parse(m1.height));
                    Vector2 v2 = new Vector2(float.Parse(m2.width), float.Parse(m2.height));

                    int compareSize = Vector2.Distance(v1, prefSize).CompareTo(Vector2.Distance(v2, prefSize));

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
                    id = cId,
                    url = value,
                    fpath = MonetizrUtils.ConvertCreativeToFname(value),
                    fname = "video",
                    fext = MonetizrUtils.ConvertCreativeToExt(type, value),
                    type = "programmatic_video",
                    mainAssetName = $"index.html",
                    mediaType = type,
                };

                _serverCampaign.assets.Add(_videoAsset);

                var videoUrl = value;
                var skipOffset = it.skipoffset;

                MonetizrUtils.AddArrayToList(
                    _serverCampaign.vastSettings.videoTrackingEvents,
                    it.TrackingEvents,
                    te =>
                    {
                        return te.Value.IndexOf(".", StringComparison.Ordinal) >= 0 ? new TrackingEvent(te) : null;
                    },
                    new TrackingEvent());

                var adParameters = it.AdParameters;
                _serverCampaign.vastSettings.videoSettings = new VideoSettings() { skipOffset = skipOffset, videoUrl = videoUrl };

                if (it.VideoClicks != null && _serverCampaign.serverSettings.GetBoolParam("openrtb.click_through", false))
                {
                    _serverCampaign.vastSettings.videoSettings.videoClickThroughUrl = it.VideoClicks.ClickThrough?.Value;
                    
                     MonetizrUtils.AddArrayToList(
                            _serverCampaign.vastSettings.videoTrackingEvents,
                            it.VideoClicks.ClickTracking,  
                           te => new TrackingEvent("click",te.Value),
                           null);
                    
                }
                
                AddVastVerificationSettings(_serverCampaign.vastSettings, adItemAdVerifications);
            }

            private void AddCreativesIntoAssets()
            {
                var adItem = _inline;

                foreach (var c in adItem.Creatives)
                {
                    if (c.NonLinearAds != null && !_loadVideoOnly)
                    {
                        AddNonLinearCreatives(c.NonLinearAds);
                    }
                    if (c.Linear != null)
                    {
                        AddLinearCreatives(c.Linear,c.id,adItem.AdVerifications);
                    }
                }
            }

            private bool FindVideoInLinearCreativesAndGrabEvents(string videoName)
            {
                var adItem = _inline;

                foreach (var c in adItem.Creatives)
                {
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

                    if (mediaFile == null)
                    {
                        string filesList = string.Join("\n", it.MediaFiles.MediaFile.Select(x => $"{x.Value}#{x.bitrate}").ToArray());

                        if (httpClient.GlobalSettings.ContainsKey("openrtb.sent_report_to_mixpanel"))
                            httpClient.Analytics.SendOpenRtbReportToMixpanel(filesList, "media error", "media", null);

                        mediaFile = it.MediaFiles.MediaFile[0];
                    }

                    var videoUrl = mediaFile.Value;
                    var skipOffset = it.skipoffset;

                    MonetizrUtils.AddArrayToList(
                        _serverCampaign.vastSettings.videoTrackingEvents,
                        it.TrackingEvents,
                        te =>
                        {
                            return te.Value.IndexOf(".", StringComparison.Ordinal) >= 0 ? new TrackingEvent(te) : null;
                        },
                        new TrackingEvent());

                    var adParameters = it.AdParameters;
                    AddAssetFromAdParameters(it?.AdParameters?.Value, _serverCampaign);
                    _serverCampaign.vastSettings.videoSettings = new VideoSettings() { skipOffset = skipOffset, videoUrl = videoUrl };
                    AddVastVerificationSettings(_serverCampaign.vastSettings, adItem.AdVerifications);
                }

                return true;
            }
        }
        internal async Task<ServerCampaign> PrepareServerCampaign(string campaignId, string vastContent, bool videoOnly = false)
        {
            ServerCampaign serverCampaign = new ServerCampaign(campaignId, "", GetDefaultSettingsForProgrammatic());
            if (!await LoadVastContent(vastContent, videoOnly, serverCampaign, true)) return null;
            string vastJsonSettings = serverCampaign.DumpsVastSettings(null);
            serverCampaign.vastAdParameters = vastJsonSettings;
            await PrepareVideoAsset(serverCampaign);
            return serverCampaign;
        }

        internal async Task<bool> InitializeServerCampaignForProgrammatic(ServerCampaign campaign, string vastContent)
        {
            campaign.RemoveAssetsByTypeFromList("programmatic_video");
            await LoadVastAndFindVideoAsset(vastContent, campaign);

            if (!campaign.TryGetAssetInList("programmatic_video", out var video))
            {
                return false;
            }
            
            await campaign.PreloadVideoPlayerForProgrammatic(video);
            return true;
        }

        private async Task LoadVastAndFindVideoAsset(string vastContent, ServerCampaign serverCampaign)
        {
            VAST vastData = CreateVastFromXml(vastContent);

            if (vastData == null)
            {
                Log.PrintError("Vast isn't loaded");
                return;
            }

            if (vastData.Items == null || vastData.Items.Length == 0)
                return;

            if (!(vastData.Items[0] is VASTAD vad))
                return;

            int prefBitRate = httpClient.GlobalSettings.GetIntParam("openrtb.pref_bitrate", 10000);
            int prefWidth = httpClient.GlobalSettings.GetIntParam("openrtb.pref_width", 1920);
            int prefHeight = httpClient.GlobalSettings.GetIntParam("openrtb.pref_height", 1080);

            var adItem = new VastAdItem(vad.Item,
                serverCampaign,
                new VastAdItem.PreferableVideoSize(prefBitRate, prefWidth, prefHeight),
                true);

            

            if (adItem.InUnknownAdType())
                return;

            adItem.AssignCreativesIntoAssets();

            if (string.IsNullOrEmpty(adItem.WrapperAdTagUri))
                return;

            Log.PrintV($"Loading wrapper with the url {adItem.WrapperAdTagUri}");

            var result = await MonetizrHttpClient.DownloadUrlAsString(new HttpRequestMessage(HttpMethod.Get, adItem.WrapperAdTagUri));

            if (!result.isSuccess)
                return;

            await LoadVastAndFindVideoAsset(result.content, serverCampaign);

            return;
        }

        private async Task<bool> LoadVastContent(string vastContent, bool videoOnly, ServerCampaign serverCampaign, bool isFirstCall)
        {
            VAST vastData = CreateVastFromXml(vastContent);
            serverCampaign.openRtbRawResponse = vastContent;

            if (vastData == null)
            {
                Log.PrintError("Vast isn't loaded");
                return false;
            }

            if (vastData.Items == null || vastData.Items.Length == 0) return false;
            if (!(vastData.Items[0] is VASTAD vad)) return false;

            int prefBitRate = httpClient.GlobalSettings.GetIntParam("openrtb.pref_bitrate", 10000);
            int prefWidth = httpClient.GlobalSettings.GetIntParam("openrtb.pref_width", 1920);
            int prefHeight = httpClient.GlobalSettings.GetIntParam("openrtb.pref_height", 1080);
            var adItem = new VastAdItem(vad.Item, serverCampaign, new VastAdItem.PreferableVideoSize(prefBitRate, prefWidth, prefHeight), videoOnly);

            if (adItem.InUnknownAdType()) return false;

            adItem.AssignCreativesIntoAssets();

            if (!string.IsNullOrEmpty(adItem.WrapperAdTagUri))
            {
                string url = adItem.WrapperAdTagUri;
                Log.PrintV($"Loading wrapper with the url {url}");
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                
                var result = await MonetizrHttpClient.DownloadUrlAsString(httpRequest);

                if (!result.isSuccess)
                {
                    Log.PrintV($"Can't load wrapper with the url {url}");
                    return false;
                }

                if (!await LoadVastContent(result.content, videoOnly, serverCampaign, false)) return false;
            }

            return true;
        }

        private async Task PrepareVideoAsset(ServerCampaign serverCampaign)
        {
            Log.PrintV("Loading video player");

            if (!serverCampaign.TryGetAssetInList(new List<string>() { "html", "video" }, out var videoAsset))
            {
                return;
            }

            await DownloadAndPrepareHtmlVideoPlayer(serverCampaign, videoAsset);
        }

        private static void LoadCampagnSettingsFromAdParams(AdParameters_type adParameters, ServerCampaign serverCampaign)
        {
            if (adParameters == null)
                return;

            Log.PrintV(adParameters.Value);
            string adp = adParameters.Value;
            adp = adp.Replace("\n", "");
            var parsedDict = MonetizrUtils.ParseContentString(adp);

            foreach (var i in parsedDict)
            {
                serverCampaign.serverSettings.Add(i.Key, i.Value);
                Log.PrintV($"Additional settings from AdParameters [{i.Key}:{i.Value}]");
            }
        }

        private static async Task DownloadAndPrepareHtmlVideoPlayer(ServerCampaign serverCampaign, ServerCampaign.Asset videoAsset)
        {
            string campPath = Application.persistentDataPath + "/" + serverCampaign.id;
            string zipFolder = campPath + "/" + videoAsset.fpath;
            Log.PrintV($"{campPath} {zipFolder}");
            if (!Directory.Exists(zipFolder)) Directory.CreateDirectory(zipFolder);
            byte[] data = await DownloadHelper.DownloadAssetData("https://image.themonetizr.com/videoplayer/html.zip");
            File.WriteAllBytes(zipFolder + "/html.zip", data);
            MonetizrUtils.ExtractAllToDirectory(zipFolder + "/html.zip", zipFolder);
            File.Delete(zipFolder + "/html.zip");
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

        private static bool AddAssetFromAdParameters(string adParametersValue, ServerCampaign serverCampaign)
        {
            if (!ServerCampaign.Asset.ValidateAssetJson(adParametersValue))
                return false;

            serverCampaign.assets.Add(new ServerCampaign.Asset(adParametersValue, false));
            return true;
        }

        private static void AddCampaignAssetFromNonLinearCreative(NonLinearAd_Inline_type nl, ServerCampaign serverCampaign)
        {
            AddAssetFromAdParameters(nl?.AdParameters?.Value, serverCampaign);
        }

        internal async Task DownloadOMSDKServiceContent()
        {
            var url = "https://image.themonetizr.com/omsdk/omsdk-v1.js";

            byte[] data = await DownloadHelper.DownloadAssetData(url);

            if (data == null)
            {
                Log.PrintWarning($"InitializeOMSDK failed! Download of {url} failed!");
                return;
            }

            _omidJsServiceContent = Encoding.UTF8.GetString(data);
        }

        internal void InitializeOMSDK(string vastAdVerificationParams)
        {
            Log.PrintV($"InitializeOMSDK with {vastAdVerificationParams}");
            Log.PrintV($"Service content: {_omidJsServiceContent}");

            UniWebViewInterface.InitOMSDK(vastAdVerificationParams, _omidJsServiceContent);
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
    }
}