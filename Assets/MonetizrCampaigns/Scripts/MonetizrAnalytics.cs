 //#define USING_FACEBOOK
#define USING_AMPLITUDE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mixpanel;
using System;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UIElements;

using System.Reflection;
using UnityEditor;


#if UNITY_IOS
    using UnityEngine.iOS;
//    using Unity.Advertisement.IosSupport;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.Campaigns
{
    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }

    }
    internal static class NielsenDar
    {
        internal static readonly Dictionary<DeviceSizeGroup, string> sizeGroupsDict = new Dictionary<DeviceSizeGroup, string>()
        {
            {DeviceSizeGroup.Phone,"PHN"},
            {DeviceSizeGroup.Tablet,"TAB"},
            {DeviceSizeGroup.Unknown,"UNWN"},
        };

        internal static Dictionary<string, string> DARPlacementTags = null;

        internal static void Track(ServerCampaign serverCampaign, AdPlacement type)
        {
            string darTagUrl = serverCampaign.dar_tag;

            if (darTagUrl.Length == 0)
                return;

            Dictionary<string, Func<AdPlacement, string>> urlModifiers = new Dictionary<string, Func<AdPlacement, string>>()
            {
                    { "${CREATIVE_ID}", GetCreativeId },
                    //{ "${SITE_ID}", GetSiteId },
                    { "${OS_GROUP}", GetOsGroup },
                    { "${DEVICE_GROUP}", GetDeviceGroup },
                    { "${ADVERTISING_ID}", GetAdvertisingId },
                    { "${PLATFORM}", GetPlatform },
                    { "${APP_VERSION}", GetAppVersion },
                    { "${OS_VERSION}", GetOsVersion },
                    { "${OPT_OUT}", GetOptOut },
                    { "${VENDOR_ID}", (AdPlacement at) => { return MonetizrAnalytics.deviceIdentifier; } },

                   // { "${PLACEMENT_ID}", GetPlacementId },
                    { "${CY}", GetCY },
                    { "${CACHEBUSTER}", GetTimeStamp },
                    { "${CAMP_ID}", (AdPlacement at) => { return serverCampaign.id; } },

             };
                        
            foreach (var v in urlModifiers)
            {
                darTagUrl = darTagUrl.Replace(v.Key, v.Value(type));
            }

            darTagUrl = ReplacePlacementTag(darTagUrl, type);

            Log.Print($"DAR: {darTagUrl}");

#if !UNITY_EDITOR
            UnityWebRequest www = UnityWebRequest.Get(darTagUrl);
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();

            operation.completed += BundleOperation_CompletedHandler;
#endif

            ///https://secure-cert.imrworldwide.com/cgi-bin/m?ci=nlsnci535&am=3&at=view&rt=banner&st=image&ca=nlsn13134&
            ///cr=${CREATIVE_ID}&
            ///ce=${SITE_ID}&pc=2f112aaa-cd96-4bc9-88b7-433ad7589bec_p&
            ///c7=osgrp,${OS_GROUP}&
            ///c8=devgrp,${DEVICE_GROUP}&c9=devid,
            ///${ADVERTISING_ID}&c10=plt,
            ///${PLATFORM}&c12=apv,
            ///${APP_VERSION}&c13=asid,PC037B691-09AE-4633-BCCC-CB6D4BBB7C21&c14=osver,
            ///${OS_VERSION}&uoo=
            ///${OPT_OUT}&r=1651670762.6459386
        }

        static void BundleOperation_CompletedHandler(AsyncOperation obj)
        {
            Log.Print($"DAR: {obj.isDone}");
        }
               

        //{{PLACEMENT_ID=TinyTeaser:Monetizr_plc0001,NotificationScreen:Monetizr_plc0002,Html5VideoScreen:Monetizr_plc0003,EmailEnterScreen:Monetizr_plc0004,CongratsScreen:Monetizr_plc0005}}
        //{{PLACEMENT_ID=TinyTeaser:Monetizr_plc0001,NotificationScreen:Monetizr_plc0002,Html5VideoScreen:Monetizr_plc0003,EmailEnterScreen:Monetizr_plc0004,CongratsScreen:Monetizr_plc0005}}
        static string ReplacePlacementTag(string s, AdPlacement t)
        {
            int startId = s.IndexOf("${{");
            int endId = s.IndexOf("}}");

            //no braces
            if (startId < 0 || endId < 0)
                return s;

            //split string into pieces
            string s1 = s.Substring(0, startId);
            string s2 = s.Substring(startId + 3, endId - startId - 3);
            string s3 = s.Substring(s1.Length + s2.Length + 5, s.Length - (s1.Length + s2.Length + 5));

            string empty_res = s1 + s3;

            //if DAR tag is null, create it
            if (DARPlacementTags == null)
            {
                var arr = s2.Split('=');

                //substring is wrong
                if (arr.Length != 2 || arr[0] != "PLACEMENT_ID" || arr[1].IndexOf(':') == -1)
                    return empty_res;

                //creating dictionary
                DARPlacementTags = arr[1].Split(',').Select(v => v.Split(':')).ToDictionary(v => v.First(), v => v.Last());
            }

            var adStr = MonetizrAnalytics.GetPlacementName(t);

            if (!DARPlacementTags.ContainsKey(adStr))
                return empty_res;

            return s1 + DARPlacementTags[adStr] + s3;
        }

        static string GetCreativeId(AdPlacement t)
        {
            if (t == AdPlacement.Video || t == AdPlacement.Html5)
                return "video1";

            return "display1";
        }

        static string GetCY(AdPlacement t)
        {
            if (t == AdPlacement.Video || t == AdPlacement.Html5)
                return "2";

            return "0";
        }

        static string GetOsGroup(AdPlacement type)
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "ANDROID";
#else
            return "";
#endif
        }

        private static string GetDeviceGroup(AdPlacement arg)
        {
            return sizeGroupsDict[MonetizrAnalytics.deviceSizeGroup];
        }

        private static string GetOptOut(AdPlacement arg)
        {
            return MonetizrAnalytics.limitAdvertising ? "1" : "0";
        }

        private static string GetOsVersion(AdPlacement arg)
        {
            return MonetizrAnalytics.osVersion;
        }

        private static string GetAppVersion(AdPlacement arg)
        {
            return Application.version;
        }

        private static string GetPlatform(AdPlacement arg)
        {
            return "MBL";
        }

        private static string GetAdvertisingId(AdPlacement arg)
        {
            return MonetizrAnalytics.advertisingID;
        }

        private static string GetSiteId(AdPlacement arg)
        {
            return "";
        }

        private static string GetTimeStamp(AdPlacement arg)
        {
            var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            return Timestamp.ToString();
        }
    }

    internal enum AdPlacement
    {
        //IntroBanner,
        //BrandLogo,
        TinyTeaser,
        //RewardLogo,
        //Video,
        //RewardBanner,
        //Survey,
        Html5,
        HtmlPage,
        //LoadingScreen,
        
        NotificationScreen,
        EmailEnterInGameRewardScreen,
        EmailEnterCouponRewardScreen,
        EmailEnterSelectionRewardScreen,
        EmailErrorScreen,
        CongratsNotificationScreen,
        EmailCongratsNotificationScreen,
        Video,
        Survey,
        SurveyNotificationScreen,
        Minigame,
        RewardsCenterScreen,

        AssetsLoading,
    }

    internal enum DeviceSizeGroup
    {
        Phone,
        Tablet,
        Unknown,
    }

    /*internal class VisibleAdAsset
    {
        internal AdPlacement adPlacement;
        internal ServerCampaign campaign;
        internal DateTime activateTime;

    }*/

    internal class AdElement
    {
        internal AdPlacement placement;
        internal ServerCampaign campaign;
        internal DateTime activateTime;
        internal string eventName;

        /*public AdElement(AdPlacement placement, ServerCampaign campaign)
        {
            this.placement = placement;
            this.campaign = campaign;
        }*/

        public AdElement(string eventName, AdPlacement placement, ServerCampaign campaign)
        {
            this.eventName = eventName;
            this.placement = placement;
            this.campaign = campaign;
            this.activateTime = DateTime.Now;
        }
    }

    internal class MonetizrAnalytics
    {
        internal IpApiData locationData = null;

        /*public static readonly Dictionary<AdType, string> adTypeNames = new Dictionary<AdType, string>()
        {
            { AdType.IntroBanner, "Intro banner" },
            { AdType.BrandLogo, "Banner logo" },
            { AdType.TinyTeaser, "Tiny teaser" },
            { AdType.RewardLogo, "Reward logo" },
            { AdType.Video, "Video" },
            { AdType.Survey, "Survey" },
            { AdType.Html5, "Html5" },
            { AdType.HtmlPage, "HtmlPage" },
            { AdType.RewardBanner, "Reward banner" },
            { AdType.LoadingScreen, "Loading screen" },
        };*/

        public static string osVersion;
        public static string advertisingID = "";
        public static bool limitAdvertising = false;
        internal static DeviceSizeGroup deviceSizeGroup = DeviceSizeGroup.Unknown;
        public static bool isMixpanelInitialized = false;

        HashSet<AdElement> adNewElements = new HashSet<AdElement>();
        internal static bool isAdvertisingIDDefined = false;

        internal static string GetPlacementName(AdPlacement t)
        {
            switch (t)
            {
                case AdPlacement.TinyTeaser: return "TinyTeaser";
                case AdPlacement.Video:
                case AdPlacement.Html5: return "Html5VideoScreen";
                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                    return "NotificationScreen";
                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                    return "EmailEnterScreen";
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.EmailCongratsNotificationScreen: return "CongratsScreen";
                case AdPlacement.Minigame: return "MiniGameScreen";
                case AdPlacement.Survey: return "SurveyScreen";
                case AdPlacement.HtmlPage: return "HtmlPageScreen";
                case AdPlacement.RewardsCenterScreen: return "RewardsCenterScreen";

                default:
                    return t.ToString();

            }
        }

        public static readonly Dictionary<DeviceSizeGroup, string> deviceSizeGroupNames = new Dictionary<DeviceSizeGroup, string>()
        {
            { DeviceSizeGroup.Phone, "phone" },
            { DeviceSizeGroup.Tablet, "tablet" },
            { DeviceSizeGroup.Unknown, "unknown" },
        };

        //        private Dictionary<string, ChallengeTimes> challengesWithTimes = new Dictionary<string, ChallengeTimes>();

        //        private const int SECONDS_IN_DAY = 24 * 60 * 60;


        //private Dictionary<AdType,VisibleAdAsset> visibleAdAsset = new Dictionary<AdType, VisibleAdAsset>();

        //AdType and ChallengeId
        //private Dictionary<AdElement, VisibleAdAsset> visibleAdAsset = new Dictionary<AdElement, VisibleAdAsset>();

        private HashSet<AdElement> visibleAdAsset = new HashSet<AdElement>();

        internal static string deviceIdentifier = "";

#if USING_AMPLITUDE
        private Amplitude amplitude;
       
#endif

        private static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        private static DeviceSizeGroup GetDeviceGroup()
        {
#if UNITY_IOS
    bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
            {
                return DeviceSizeGroup.Tablet;
            }
 
            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");
            if (deviceIsIphone)
            {
                return DeviceSizeGroup.Phone;
            }
#elif UNITY_ANDROID

            float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            bool isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);

            if (isTablet)
            {
                return DeviceSizeGroup.Tablet;
            }
            else
            {
                return DeviceSizeGroup.Phone;
            }

#endif
            return DeviceSizeGroup.Unknown;

        }

        internal static string GetOsGroup()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#else
            return "";
#endif
        }

        internal MonetizrAnalytics()
        {
            LoadUserId();

            Log.Print($"MonetizrAnalytics initialized with user id: {GetUserId()}");

            
            osVersion = "0.0";

#if !UNITY_EDITOR
    #if UNITY_ANDROID
                   AndroidJavaClass versionInfo = new AndroidJavaClass("android/os/Build$VERSION");

                   osVersion = versionInfo.GetStatic<string>("RELEASE");
    #elif UNITY_IOS
                   osVersion = UnityEngine.iOS.Device.systemVersion;
    #endif
#endif

            /*#if !UNITY_EDITOR
            #if UNITY_ANDROID
                           AndroidJavaClass versionInfo = new AndroidJavaClass("android/os/Build$VERSION");

                           osVersion = versionInfo.GetStatic<string>("RELEASE");

                           AndroidJavaClass up = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
                           AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
                           AndroidJavaClass client = new AndroidJavaClass ("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                           AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject> ("getAdvertisingIdInfo",currentActivity);

                           advertisingID = adInfo.Call<string> ("getId").ToString();   
                           limitAdvertising = (adInfo.Call<bool> ("isLimitAdTrackingEnabled"));

            #elif UNITY_IOS
                           osVersion = UnityEngine.iOS.Device.systemVersion;
                           limitAdvertising = !(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
                           advertisingID = Device.advertisingIdentifier;

            #endif
            #endif*/
            deviceSizeGroup = GetDeviceGroup();

            Log.Print($"OS Version {osVersion} Ad id: {advertisingID} Limit ads: {limitAdvertising} Device group: {deviceSizeGroup}");

#if USING_AMPLITUDE
            amplitude = Amplitude.Instance;
            amplitude.logging = true;
            amplitude.init("6a1fad35d3813820b6b68af48b36e9d5");
            amplitude.setOnceUserProperty("user_segment", MonetizrManager.abTestSegment);
#endif
            isMixpanelInitialized = false;

            //Mixpanel.SetToken("cda45517ed8266e804d4966a0e693d0d");
            

#if USING_FACEBOOK
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                FB.Init(() =>
                {
                    FB.ActivateApp();

                    Log.Print("[FB] Activated!");
                });
            }
#endif
        }

        internal void InitializeMixpanel(string apikey)
        {
            if (isMixpanelInitialized)
                return;

            isMixpanelInitialized = true;

            Mixpanel.Init();
            Mixpanel.SetToken(apikey);

            Debug.Log("Mixpanel init called");
        }


        /*private void BeginShowAdAsset(AdPlacement type, Mission m)
        {
            BeginShowAdAsset(type, m.campaignId);
        }*/

        private void BeginShowAdAsset(string eventName, AdPlacement placement, ServerCampaign campaign)
        {
            if (campaign == null)
            {
                Log.Print($"MonetizrAnalytics BeginShowAdAsset: MissionUIDescription shouldn't be null");
                return;
            }

           //Log.Print($"MonetizrAnalytics BeginShowAdAsset: {placement} {campaign.id}");

            //Key value pair for duplicated types with different challenge ids
            AdElement adElement = new AdElement(eventName, placement, campaign);

            if (visibleAdAsset.Contains(adElement))
            {
                Log.Print(MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);
            }

            //Assert.IsFalse(visibleAdAsset.ContainsKey(type), MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);

            //var ch = (m == null) ? MonetizrManager.Instance.GetActiveChallenge() : challengeId;

            /*var adAsset = new VisibleAdAsset()
            {
                adPlacement = placement,
                campaign = campaign,
                activateTime = DateTime.Now
            };*/

            //visibleAdAsset[adElement] = adAsset;

            visibleAdAsset.Add(adElement);

            StartTimedEvent(eventName);

            //Mixpanel.StartTimedEvent($"[UNITY_SDK] [TIMED] {type.ToString()}");
        }

        private void StartTimedEvent(string eventName)
        {
            Mixpanel.StartTimedEvent($"[UNITY_SDK] [TIMED] {eventName}");
        }

        /*private void EndShowAdAsset(AdPlacement type, Mission m, bool removeElement = true)
        {
            EndShowAdAsset(type, m.campaignId, removeElement);
        }*/

        //if challengeId is define, end only specified ad types, if not - end all
        private void EndShowAdAsset(AdPlacement placement, ServerCampaign campaign, bool removeElement = true)
        {
            //Log.Print($"MonetizrAnalytics EndShowAdAsset: {placement} {campaign.id}");

            //Assert.IsNotNull(visibleAdAsset);
            //Assert.AreEqual(type, visibleAdAsset.adType, MonetizrErrors.msg[ErrorType.SimultaneusAdAssets]);

            visibleAdAsset.RemoveWhere((AdElement a) => {

                //if campaign is null, we ignore it
                ServerCampaign c = campaign == null ? null : a.campaign;
                
                bool remove = a.placement == placement && c == campaign;

                if(remove)
                {
                    _EndShowAdAsset(a);
                }

                return remove;
            });


            //if challenge id isn't null, sent analytics event with this exact id
           /* if (campaign != null)
            {
                AdElement key = new AdElement(placement, campaign);


                _EndShowAdAsset(key);

                if (removeElement)
                    visibleAdAsset.Remove(key);
            }
            //if challenge id is null, send events for all active ad assets with the same type
            else
            {
                List<AdElement> toRemove = new List<AdElement>();

                foreach (var adAssetElement in visibleAdAsset)
                {
                    if (adAssetElement.placement == placement)
                    {
                        _EndShowAdAsset(adAssetElement);

                        if (removeElement)
                            toRemove.Add(adAssetElement);
                    }
                }

                foreach (var i in toRemove)
                    visibleAdAsset.Remove(i);

            }*/


        }

        private void AddDefaultMixpanelValues(Value props, ServerCampaign campaign, string brandName)
        {            
            props["application_id"] = campaign.application_id;
            props["bundle_id"] = MonetizrManager.bundleId;
            props["player_id"] = GetUserId();

            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["impressions"] = "1";
            
            props["camp_id"] = campaign.id;
            props["brand_id"] = campaign.brand_id;
            props["brand_name"] = brandName;
            //props["type"] = adTypeNames[adAsset.Key];
            //props["type"] = adAsset.Key.ToString();
            props["ab_segment"] = MonetizrManager.abTestSegment;
            props["device_size"] = deviceSizeGroupNames[deviceSizeGroup];

            props["api_key"] = MonetizrManager.Instance.GetCurrentAPIkey();
            props["sdk_version"] = MonetizrManager.SDKVersion;

            props["ad_id"] = MonetizrAnalytics.advertisingID;

            props["camp_title"] = campaign.title;

            if (locationData != null)
            {
                props["country_code"] = locationData.country_code;
                props["region_code"] = locationData.region_code;
                props["country_name"] = locationData.country_name;
            }
           
            foreach (var s in campaign.serverSettings.dictionary)
            {
                string key = s.Key;

                if (!key.EndsWith("_text") && key != "custom_missions")
                {
                    props[$"cs_{s.Key}"] = s.Value;
                }
            }
        }

        

        private void MixpanelTrackAndMaybeFlush(ServerCampaign camp, string eventName, Value props, bool darTag = false)
        {
            props["dar_tag_sent"] = darTag.ToString();

            Debug.Log($"--->Mixpanel track {eventName}");

            Mixpanel.Identify(camp.brand_id);
            Mixpanel.Track(eventName, props);

            if (camp.serverSettings.GetBoolParam("mixpanel_fast_flush",false))
                Mixpanel.Flush();

#if USING_AMPLITUDE
            Dictionary<string, object> eventProps = new Dictionary<string, object>();

            foreach(var v in props.GetFieldValue<Dictionary<string, Value>>("_container"))
            {
                var value = v.Value.GetFieldValue<string>("_string");

                eventProps.Add(v.Key, value);

                //Debug.Log($"params: {v.Key} {value}");
            }
            
            amplitude.logEvent(eventName, eventProps);
#endif
        }

        private void _EndShowAdAsset(AdElement adAsset)
        {
            Debug.Assert(isMixpanelInitialized);

            //Log.Print($"MonetizrAnalytics EndShowAdAsset: {adAsset.Key} {adAsset.Value}");

            //string brandName = visibleAdAsset[adAsset].mission.brandName;//
            //
            /* string brandName = MonetizrManager.Instance.GetAsset<string>(visibleAdAsset[adAsset].campaignId, AssetsType.BrandTitleString);

             var challenge = MonetizrManager.Instance.GetCampaign(visibleAdAsset[adAsset].campaignId);

             //var challenge = visibleAdAsset[adAsset].mission;

             if (challenge == null)
             {
                 Log.Print($"MonetizrAnalytics _EndShowAdAsset: MissionUIDescription shouldn't be null");
                 return;
             }

             var props = new Value();


             AddDefaultMixpanelValues(props, challenge, brandName);

             props["type"] = adAsset.Key.ToString();

             string eventName = $"[UNITY_SDK] [TIMED] {adAsset.Key.ToString()}";

             //Mixpanel.Identify(challenge.brand_id);
             //Mixpanel.Track(eventName, props);

             MixpanelTrackAndMaybeFlush(challenge, eventName, props, true);*/

            string placementName = adAsset.placement.ToString();

            _TrackEvent(adAsset.eventName, adAsset.campaign, true, new Dictionary<string, string>() { { "type", placementName } });

            //if (removeElement)
            //    visibleAdAsset.Remove(adAsset);
        }

        private string RemoveContentInBrackets(string str)
        {
            int i = str.IndexOf('[');
            int j = str.LastIndexOf(']');

            string result = str.Remove(i, j - i + 1).Trim();

            return result;
        }

        internal void LoadUserId()
        {
            if(PlayerPrefs.HasKey("Monetizr.user_id"))
                deviceIdentifier = PlayerPrefs.GetString("Monetizr.user_id");
            else
                deviceIdentifier = SystemInfo.deviceUniqueIdentifier;

            PlayerPrefs.SetString("Monetizr.user_id", deviceIdentifier);

            PlayerPrefs.Save();
        }
                
        internal void RandomizeUserId()
        {
            var _deviceIdentifier = deviceIdentifier.ToCharArray();

            for (int i = 0; i < _deviceIdentifier.Length; i++)
            {
                var temp = _deviceIdentifier[i];
                var randomIndex = UnityEngine.Random.Range(i, deviceIdentifier.Length);

                if (temp == randomIndex && randomIndex == '-')
                    continue;

                _deviceIdentifier[i] = _deviceIdentifier[randomIndex];
                _deviceIdentifier[randomIndex] = temp;
            }

            deviceIdentifier = new string(_deviceIdentifier);
            PlayerPrefs.SetString("Monetizr.user_id", deviceIdentifier);

            PlayerPrefs.Save();
        }

        internal string GetUserId()
        {
            return deviceIdentifier;
            //return SystemInfo.deviceUniqueIdentifier;
        }

        internal void TrackEvent(Mission currentMission, PanelController panel, MonetizrManager.EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            if (panel.GetAdPlacement() == null)
                return;

            var adPlacement = panel.GetAdPlacement().Value;

            var campaign = MonetizrManager.Instance.GetCampaign(currentMission.campaignId);

            TrackEvent(campaign, currentMission, adPlacement, eventType, additionalValues);
        }

        internal void TrackEvent(Mission currentMission, AdPlacement adPlacement, MonetizrManager.EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            var campaign = MonetizrManager.Instance.GetCampaign(currentMission.campaignId);

            TrackEvent(campaign, currentMission, adPlacement, eventType, additionalValues);
        }

        internal void TrackEvent(ServerCampaign currentCampaign, Mission currentMission, AdPlacement adPlacement, MonetizrManager.EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            Debug.Log($"------Track event: {currentCampaign} {adPlacement} {eventType}");

            Debug.Assert(currentCampaign != null);
            
            string placementName = GetPlacementName(adPlacement);

            MonetizrManager._CallUserDefinedEvent(currentCampaign.id, placementName, eventType);

            //-------

            //EmailErrorScreen,
            //Video,

            var eventNames = new Dictionary<AdPlacement, string>()
            {
                { AdPlacement.TinyTeaser,"Tiny teaser" },
                { AdPlacement.Minigame, "Minigame" },
                { AdPlacement.Survey, "Survey" },
                { AdPlacement.Html5, "Html5" },
                { AdPlacement.HtmlPage, "HtmlPage" },
                { AdPlacement.RewardsCenterScreen, "Reward center" },
                { AdPlacement.NotificationScreen, "Notification" },
                { AdPlacement.CongratsNotificationScreen, "Congrats screen" },
                { AdPlacement.SurveyNotificationScreen, "Survey notification" },
                { AdPlacement.EmailCongratsNotificationScreen, "Email congrats" },
                { AdPlacement.EmailEnterCouponRewardScreen, "Enter email" },
                { AdPlacement.EmailEnterInGameRewardScreen, "Enter email" },
                { AdPlacement.EmailEnterSelectionRewardScreen, "Enter email" },
                { AdPlacement.AssetsLoading, "Assets loading" }
            };

            
            string completedOrPressed(AdPlacement p) 
            {
                switch (p)
                {
                    case AdPlacement.Minigame:
                    case AdPlacement.Survey:
                    case AdPlacement.Html5:
                    case AdPlacement.HtmlPage:
                        return "completed";

                    case AdPlacement.EmailEnterCouponRewardScreen:
                    case AdPlacement.EmailEnterInGameRewardScreen:
                    case AdPlacement.EmailEnterSelectionRewardScreen:
                        return "submitted";

                    default: break;
                }

                return "pressed";
            };

            var eventTypes = new Dictionary<MonetizrManager.EventType, string>()
            {
                { MonetizrManager.EventType.ButtonPressOk, completedOrPressed(adPlacement) },
                { MonetizrManager.EventType.ButtonPressSkip, "skipped" },
                { MonetizrManager.EventType.Impression, "shown" },
                { MonetizrManager.EventType.Error, "failed" }
            };

            TrackNewEvents(currentCampaign, currentMission, adPlacement, placementName, eventType, additionalValues);

            
            if (eventType == MonetizrManager.EventType.Impression)
            {
                NielsenDar.Track(currentCampaign, adPlacement);

                MonetizrManager.Analytics.BeginShowAdAsset($"{adPlacement}", adPlacement, currentCampaign);
            }
            
            //No regular track event if we track end of impression
            if (eventType == MonetizrManager.EventType.ImpressionEnds)
            {
                MonetizrManager.Analytics.EndShowAdAsset(adPlacement, currentCampaign);
                return;
            }

            string eventName = $"{eventNames[adPlacement]} {eventTypes[eventType]}";

            //var campaign = MonetizrManager.Instance.GetCampaign(currentMission.campaignId);

            _TrackEvent(eventName, currentCampaign, false, additionalValues);

            
        }


        internal void TrackNewEvents(ServerCampaign campaign,
            Mission currentMission,
            AdPlacement adPlacement,
            string placementName,
            MonetizrManager.EventType eventType,
            Dictionary<string, string> additionalValues)
        {
            if (additionalValues == null)
                additionalValues = new Dictionary<string, string>();

            additionalValues.Add("placement", placementName);
            additionalValues.Add("placement_group", GetPlacementGroup(adPlacement));

            var eventName = "";
            bool timed = false;

            switch(adPlacement)
            {
                case AdPlacement.EmailEnterCouponRewardScreen:
                    additionalValues.Add("reward_type", "product");
                    break;

                case AdPlacement.EmailEnterInGameRewardScreen:
                    additionalValues.Add("reward_type", "ingame");
                    break;

                case AdPlacement.EmailEnterSelectionRewardScreen:
                    additionalValues.Add("reward_type", MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame ? "ingame" : "product");
                    break;

                default: break;
            }

            double duration = 0.0;

            switch(eventType)
            {
                case MonetizrManager.EventType.ButtonPressOk:
                    eventName = "Action";
                    additionalValues.Add("action", "ok");
                    break;

                case MonetizrManager.EventType.ButtonPressSkip:
                    additionalValues.Add("action", "skip");
                    eventName = "Action";

                    break;

                case MonetizrManager.EventType.Impression:
                    eventName = "ImpressionStarts";
                    //Mixpanel.StartTimedEvent($"[UNITY_SDK] [TIMED] {ImpressionEnds}");
                    adNewElements.Add(new AdElement("ImpressionEnds", adPlacement, campaign));

                    break;

                case MonetizrManager.EventType.ImpressionEnds:
                    timed = true;
                    eventName = "ImpressionEnds";

                    adNewElements.RemoveWhere((AdElement a) => {
                       if(adPlacement == a.placement && campaign == a.campaign)
                       {
                            //additionalValues.Add("$duration", (DateTime.Now - a.activateTime).TotalSeconds.ToString());

                            duration = (DateTime.Now - a.activateTime).TotalSeconds;

                            return true;
                       }

                        return false;
                    });

                    break;

                case MonetizrManager.EventType.Error:
                    eventName = eventType.ToString();
                    break;

            };

            _TrackEvent(eventName, campaign, timed, additionalValues, duration);
        }

        internal string GetPlacementGroup(AdPlacement adPlacement)
        {
           
                switch (adPlacement)
                {
                    case AdPlacement.TinyTeaser:
                        return "MiniBanners";

                    case AdPlacement.NotificationScreen:
                    case AdPlacement.SurveyNotificationScreen:
                    case AdPlacement.CongratsNotificationScreen:
                    case AdPlacement.RewardsCenterScreen:
                    case AdPlacement.EmailCongratsNotificationScreen:
                        return "StaticScreens";

                    case AdPlacement.Minigame: 
                    case AdPlacement.Video:
                    case AdPlacement.Html5:
                    case AdPlacement.Survey:
                        return "EngagementScreens";
                    
                    case AdPlacement.EmailEnterInGameRewardScreen:
                    case AdPlacement.EmailEnterCouponRewardScreen:
                    case AdPlacement.EmailEnterSelectionRewardScreen:
                        return "ActionScreens";
                   
                    
                    //case AdPlacement.HtmlPage: return "";
                    
                    default:
                        return "Other";

                }
            
        }

        internal void _TrackEvent(string name, ServerCampaign campaign, bool timed = false, Dictionary<string,string> additionalValues = null, double duration = -1.0)
        {
            Debug.Assert(isMixpanelInitialized);

            string logString = $"--->SendEvent: name:{name} id:{campaign.id}";

            if(additionalValues != null)
            {
                if (additionalValues.ContainsKey("placement")) logString += " placement:" + additionalValues["placement"];
                if (additionalValues.ContainsKey("placement_group")) logString += " group:" + additionalValues["placement_group"];
                if (additionalValues.ContainsKey("action")) logString += " action:" + additionalValues["action"];
                if (additionalValues.ContainsKey("$duration")) logString += " duration:" + additionalValues["$duration"];
            }

            Log.PrintWarning(logString);

            var eventName = $"[UNITY_SDK] {name}";

            if (timed)
            {
                eventName = $"[UNITY_SDK] [TIMED] {name}";
            }

            string brand_name = "none";

            if (campaign == null)
            {
                Log.Print($"MonetizrAnalytics TrackEvent: ServerCampaign shouldn't be null");
                return;
            }


            var ch = campaign.id;

            if(MonetizrManager.Instance.HasAsset(ch, AssetsType.BrandTitleString))
                brand_name = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);

           
            var props = new Value();
            
            AddDefaultMixpanelValues(props, campaign, brand_name);

            if (additionalValues != null)
            {
                foreach (var s in additionalValues)
                    props[$"{s.Key}"] = s.Value;
            }

            if(duration > 0.0)
            {
                props["$duration"] = duration;
            }


            //Mixpanel.Identify(campaign.brand_id);
            //Mixpanel.Track(eventName, props);

            MixpanelTrackAndMaybeFlush(campaign, eventName, props);
        }

        public void OnApplicationQuit()
        {
            foreach (var ad in visibleAdAsset)
            {
                _EndShowAdAsset(ad);
            }

            Mixpanel.Flush();
        }

        
    }



}
