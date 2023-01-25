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

        internal static void Track(string m, AdType type)
        {
            string darTagUrl = MonetizrManager.Instance.GetCampaign(m).dar_tag;

            if (darTagUrl.Length == 0)
                return;

            Dictionary<string, Func<AdType, string>> urlModifiers = new Dictionary<string, Func<AdType, string>>()
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

                   // { "${PLACEMENT_ID}", GetPlacementId },
                    { "${CY}", GetCY },
                    { "${CACHEBUSTER}", GetTimeStamp },

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

        internal static string GetPlacementName(AdType t)
        {
             switch (t)
            {
                case AdType.TinyTeaser: return "TinyTeaser";
                case AdType.Video:
                case AdType.Html5: return "Html5VideoScreen";
                case AdType.NotificationScreen:
                case AdType.SurveyNotificationScreen:
                    return "NotificationScreen";
                case AdType.EmailEnterInGameRewardScreen:
                case AdType.EmailEnterCouponRewardScreen:
                case AdType.EmailEnterSelectionRewardScreen: return "EmailEnterScreen";
                case AdType.CongratsNotificationScreen:
                case AdType.EmailCongratsNotificationScreen: return "CongratsScreen";
                case AdType.Minigame: return "MiniGameScreen";
                case AdType.Survey: return "SurveyScreen";
                case AdType.HtmlPage: return "HtmlPageScreen";
                case AdType.RewardsCenterScreen: return "RewardsCenterScreen";

                default:
                    return "";
       
            }
        }

        //{{PLACEMENT_ID=TinyTeaser:Monetizr_plc0001,NotificationScreen:Monetizr_plc0002,Html5VideoScreen:Monetizr_plc0003,EmailEnterScreen:Monetizr_plc0004,CongratsScreen:Monetizr_plc0005}}
        //{{PLACEMENT_ID=TinyTeaser:Monetizr_plc0001,NotificationScreen:Monetizr_plc0002,Html5VideoScreen:Monetizr_plc0003,EmailEnterScreen:Monetizr_plc0004,CongratsScreen:Monetizr_plc0005}}
        static string ReplacePlacementTag(string s, AdType t)
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

            var adStr = GetPlacementName(t);

            if (!DARPlacementTags.ContainsKey(adStr))
                return empty_res;

            return s1 + DARPlacementTags[adStr] + s3;
        }

        static string GetCreativeId(AdType t)
        {
            if (t == AdType.Video || t == AdType.Html5)
                return "video1";

            return "display1";
        }

        static string GetCY(AdType t)
        {
            if (t == AdType.Video || t == AdType.Html5)
                return "2";

            return "0";
        }

        static string GetOsGroup(AdType type)
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "ANDROID";
#else
            return "";
#endif
        }

        private static string GetDeviceGroup(AdType arg)
        {
            return sizeGroupsDict[MonetizrAnalytics.deviceSizeGroup];
        }

        private static string GetOptOut(AdType arg)
        {
            return MonetizrAnalytics.limitAdvertising ? "1" : "0";
        }

        private static string GetOsVersion(AdType arg)
        {
            return MonetizrAnalytics.osVersion;
        }

        private static string GetAppVersion(AdType arg)
        {
            return Application.version;
        }

        private static string GetPlatform(AdType arg)
        {
            return "MBL";
        }

        private static string GetAdvertisingId(AdType arg)
        {
            return MonetizrAnalytics.advertisingID;
        }

        private static string GetSiteId(AdType arg)
        {
            return "";
        }

        private static string GetTimeStamp(AdType arg)
        {
            var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            return Timestamp.ToString();
        }
    }

    internal enum AdType
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
    }

    internal enum DeviceSizeGroup
    {
        Phone,
        Tablet,
        Unknown,
    }

    internal class VisibleAdAsset
    {
        public AdType adType;
        public string mission;
        public DateTime activateTime;

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
        private Dictionary<KeyValuePair<AdType, string>, VisibleAdAsset> visibleAdAsset = new Dictionary<KeyValuePair<AdType, string>, VisibleAdAsset>();
        private string deviceIdentifier;

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
#endif
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


        internal void BeginShowAdAsset(AdType type, Mission m)
        {
            BeginShowAdAsset(type, m.campaignId);
        }

        internal void BeginShowAdAsset(AdType type, string campaignId)
        {
            if (campaignId == null)
            {
                Log.Print($"MonetizrAnalytics BeginShowAdAsset: MissionUIDescription shouldn't be null");
                return;
            }

           Log.Print($"MonetizrAnalytics BeginShowAdAsset: {type} {campaignId}");

            //Key value pair for duplicated types with different challenge ids
            KeyValuePair<AdType, string> adElement = new KeyValuePair<AdType, string>(type, campaignId);

            if (visibleAdAsset.ContainsKey(adElement))
            {
                Log.Print(MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);
            }

            //Assert.IsFalse(visibleAdAsset.ContainsKey(type), MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);

            //var ch = (m == null) ? MonetizrManager.Instance.GetActiveChallenge() : challengeId;

            var adAsset = new VisibleAdAsset()
            {
                adType = type,
                mission = campaignId,
                activateTime = DateTime.Now
            };

            visibleAdAsset[adElement] = adAsset;

            StartTimedEvent(type.ToString());

            //Mixpanel.StartTimedEvent($"[UNITY_SDK] [TIMED] {type.ToString()}");
        }

        internal void StartTimedEvent(string eventName)
        {
            Mixpanel.StartTimedEvent($"[UNITY_SDK] [TIMED] {eventName}");
        }

        internal void EndShowAdAsset(AdType type, Mission m, bool removeElement = true)
        {
            EndShowAdAsset(type, m.campaignId, removeElement);
        }

        //if challengeId is define, end only specified ad types, if not - end all
        internal void EndShowAdAsset(AdType type, string campaignId = null, bool removeElement = true)
        {
            Log.Print($"MonetizrAnalytics EndShowAdAsset: {type} {campaignId}");

            //Assert.IsNotNull(visibleAdAsset);
            //Assert.AreEqual(type, visibleAdAsset.adType, MonetizrErrors.msg[ErrorType.SimultaneusAdAssets]);

            //if challenge id isn't null, sent analytics event with this exact id
            if (campaignId != null)
            {
                KeyValuePair<AdType, string> key = new KeyValuePair<AdType, string>(type, campaignId);


                _EndShowAdAsset(key);

                if (removeElement)
                    visibleAdAsset.Remove(key);
            }
            //if challenge id is null, send events for all active ad assets with the same type
            else
            {
                List<KeyValuePair<AdType, string>> toRemove = new List<KeyValuePair<AdType, string>>();

                foreach (var adAssetElement in visibleAdAsset)
                {
                    if (adAssetElement.Key.Key == type)
                    {
                        _EndShowAdAsset(adAssetElement.Key);

                        if (removeElement)
                            toRemove.Add(adAssetElement.Key);
                    }
                }

                foreach (var i in toRemove)
                    visibleAdAsset.Remove(i);

            }


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

        

        private void MixpanelTrackAndMaybeFlush(ServerCampaign camp, string eventName, Value props)
        {
            Mixpanel.Identify(camp.brand_id);
            Mixpanel.Track(eventName, props);

            if (camp.serverSettings.GetBoolParam("mixpanel_fast_flush",true))
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

        private void _EndShowAdAsset(KeyValuePair<AdType, string> adAsset)
        {
            Debug.Assert(isMixpanelInitialized);

            //Log.Print($"MonetizrAnalytics EndShowAdAsset: {adAsset.Key} {adAsset.Value}");

            //string brandName = visibleAdAsset[adAsset].mission.brandName;//
            //
            string brandName = MonetizrManager.Instance.GetAsset<string>(visibleAdAsset[adAsset].mission, AssetsType.BrandTitleString);

            var challenge = MonetizrManager.Instance.GetCampaign(visibleAdAsset[adAsset].mission);

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

            MixpanelTrackAndMaybeFlush(challenge, eventName, props);


            NielsenDar.Track(challenge.id, adAsset.Key);

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

        internal void TrackEvent(string name, Mission currentMissionDesc)
        {
            if (currentMissionDesc.campaignId == null)
                return;

            TrackEvent(name, currentMissionDesc.campaignId);
        }


        internal void TrackEvent(string name, string campaign, bool timed = false)
        {
            var campaignName = MonetizrManager.Instance.GetCampaign(campaign);

            TrackEvent(name, campaignName, timed);
        }

        internal void TrackEvent(string name, ServerCampaign campaign, bool timed = false, Dictionary<string,string> additionalValues = null)
        {
            Debug.Assert(isMixpanelInitialized);

            Log.Print($"TrackEvent: {name} {campaign.id}");

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


            //Mixpanel.Identify(campaign.brand_id);
            //Mixpanel.Track(eventName, props);

            MixpanelTrackAndMaybeFlush(campaign, eventName, props);
        }

        public void OnApplicationQuit()
        {
            foreach (var ad in visibleAdAsset)
            {
                _EndShowAdAsset(ad.Key);
            }

            Mixpanel.Flush();
        }
        
    }



}
