//#define USING_FACEBOOK
//#define USING_AMPLITUDE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mixpanel;
using System;
using UnityEngine.Assertions;
using UnityEngine.Networking;


#if UNITY_IOS
    using UnityEngine.iOS;
    using Unity.Advertisement.IosSupport;
#endif

#if UNITY_ANDROID
    using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.Campaigns
{
    internal static class NielsenDar
    {
        internal static readonly Dictionary<DeviceSizeGroup, string> sizeGroupsDict = new Dictionary<DeviceSizeGroup, string>()
        {
            {DeviceSizeGroup.Phone,"PHN"},
            {DeviceSizeGroup.Tablet,"TAB"},
            {DeviceSizeGroup.Unknown,"UNWN"},
        };


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
             };
                        
            foreach (var v in urlModifiers)
            {
                darTagUrl = darTagUrl.Replace(v.Key, v.Value(type));
            }

            Log.Print($"DAR: {darTagUrl}");

            UnityWebRequest www = UnityWebRequest.Get(darTagUrl);
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();

            operation.completed += BundleOperation_CompletedHandler;

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

        static string GetCreativeId(AdType t)
        {
            if (t == AdType.Video || t == AdType.Html5)
                return "video1";

            return "display1";
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

    }

    internal enum AdType
    {
        IntroBanner,
        BrandLogo,
        TinyTeaser,
        RewardLogo,
        Video,
        RewardBanner,
        Survey,
        Html5,
        HtmlPage,
        LoadingScreen
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
        public static readonly Dictionary<AdType, string> adTypeNames = new Dictionary<AdType, string>()
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
        };

        public static string osVersion;
        public static string advertisingID = null;
        public static bool limitAdvertising = false;
        internal static DeviceSizeGroup deviceSizeGroup = DeviceSizeGroup.Unknown;

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



        public MonetizrAnalytics()
        {
            Log.Print($"MonetizrAnalytics initialized with user id: {GetUserId()}");

            
            osVersion = "0.0";

//#if !UNITY_EDITOR
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
//#endif
            deviceSizeGroup = GetDeviceGroup();

            Log.Print($"OS Version {osVersion} Ad id: {advertisingID} Limit ads: {limitAdvertising} Device group: {deviceSizeGroup}");

#if USING_AMPLITUDE
            amplitude = Amplitude.Instance;
            amplitude.logging = true;
            amplitude.init("6a1fad35d3813820b6b68af48b36e9d5");
            amplitude.setOnceUserProperty("user_segment", MonetizrManager.abTestSegment);
#endif

            //Mixpanel.SetToken("cda45517ed8266e804d4966a0e693d0d");
            Mixpanel.Init();
            Mixpanel.SetToken("cda45517ed8266e804d4966a0e693d0d");
            

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

        

        /*public void BeginShowAdAsset(AdType type, MissionUIDescription m)
        {
            if(m == null)
            {
                Log.Print($"MonetizrAnalytics BeginShowAdAsset: MissionUIDescription shouldn't be null");
                return;
            }

            Log.Print($"MonetizrAnalytics BeginShowAdAsset: {type} {m.campaignId}");

            //Key value pair for duplicated types with different challenge ids
            KeyValuePair<AdType, MissionUIDescription> adElement = new KeyValuePair<AdType, MissionUIDescription>(type, m);

            if (visibleAdAsset.ContainsKey(adElement))
            {
                Log.Print(MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);
            }

            //Assert.IsFalse(visibleAdAsset.ContainsKey(type), MonetizrErrors.msg[ErrorType.AdAssetStillShowing]);

            //var ch = (m == null) ? MonetizrManager.Instance.GetActiveChallenge() : challengeId;

            var adAsset = new VisibleAdAsset() {
                adType = type,
                mission = m,
                activateTime = DateTime.Now
            };

            visibleAdAsset[adElement] = adAsset;

            Mixpanel.StartTimedEvent($"[UNITY_SDK] [AD] {adTypeNames[type]}");

        }

        //if challengeId is define, end only specified ad types, if not - end all
        public void EndShowAdAsset(AdType type, MissionUIDescription m, bool removeElement = true)
        {     
            //Assert.IsNotNull(visibleAdAsset);
            //Assert.AreEqual(type, visibleAdAsset.adType, MonetizrErrors.msg[ErrorType.SimultaneusAdAssets]);

            //if challenge id isn't null, sent analytics event with this exact id
            if (m != null)
            {
                KeyValuePair<AdType, MissionUIDescription> key = new KeyValuePair<AdType, MissionUIDescription>(type, m);


                _EndShowAdAsset(key);

                if(removeElement)
                    visibleAdAsset.Remove(key);
            }
            //if challenge id is null, send events for all active ad assets with the same type
            else
            {
                List< KeyValuePair<AdType, MissionUIDescription>> toRemove = new List<KeyValuePair<AdType, MissionUIDescription>>();

                foreach(var adAssetElement in visibleAdAsset)
                {
                    if(adAssetElement.Key.Key == type)
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

        private void _EndShowAdAsset(KeyValuePair<AdType, MissionUIDescription> adAsset)
        {
            Log.Print($"MonetizrAnalytics EndShowAdAsset: {adAsset.Key} {adAsset.Value}");

            string brandName = visibleAdAsset[adAsset].mission.brandName;// MonetizrManager.Instance.GetAsset<string>(visibleAdAsset[adAsset].challengeId, AssetsType.BrandTitleString);

            //var challenge = MonetizrManager.Instance.GetChallenge(visibleAdAsset[adAsset].challengeId);
            var challenge = visibleAdAsset[adAsset].mission;

            if (challenge == null)
            {
                Log.Print($"MonetizrAnalytics _EndShowAdAsset: MissionUIDescription shouldn't be null");
                return;
            }

            var props = new Value();
            props["application_id"] = challenge.appId;
            props["bundle_id"] = Application.identifier;
            props["player_id"] = GetUserId();
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["impressions"] = "1";
            //props["campaign_id"] = visibleAdAsset[adAsset].challengeId;
            props["camp_id"] = challenge.campaignId;
            props["brand_id"] = challenge.brandId;
            props["brand_name"] = brandName;
            props["type"] = adTypeNames[adAsset.Key];
            props["ab_segment"] = MonetizrManager.abTestSegment;
            //props["duration"] = (DateTime.Now - visibleAdAsset[type].activateTime).TotalSeconds;

            string eventName = $"[UNITY_SDK] [AD] {adTypeNames[adAsset.Key]}";
                       
            Mixpanel.Track(eventName, props);

#if !UNITY_EDITOR
            NielsenDar.Track(challenge, adAsset.Key);
#endif

#if USING_AMPLITUDE
            Dictionary<string, object> eventProps = new Dictionary<string, object>();
            eventProps.Add("camp_id", challenge.campaignId);
            eventProps.Add("brand_id", challenge.brandId);
            eventProps.Add("brand_name", brandName);
            eventProps.Add("type", adTypeNames[adAsset.Key]);
            eventProps.Add("duration", (DateTime.Now - visibleAdAsset[adAsset].activateTime).TotalSeconds);
            eventProps.Add("apiKey", MonetizrManager.Instance.GetCurrentAPIkey());
            eventProps.Add("ab_segment", MonetizrManager.abTestSegment);

            amplitude.logEvent(eventName, eventProps);
#endif

            //if (removeElement)
            //    visibleAdAsset.Remove(adAsset);
        }*/

        public void BeginShowAdAsset(AdType type, Mission m)
        {
            BeginShowAdAsset(type, m.campaignId);
        }

        public void BeginShowAdAsset(AdType type, string campaignId)
        {
            if (campaignId == null)
            {
                Log.Print($"MonetizrAnalytics BeginShowAdAsset: MissionUIDescription shouldn't be null");
                return;
            }

           //Log.Print($"MonetizrAnalytics BeginShowAdAsset: {type} {campaignId}");

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

            Mixpanel.StartTimedEvent($"[UNITY_SDK] [AD] {adTypeNames[type]}");

        }

        public void EndShowAdAsset(AdType type, Mission m, bool removeElement = true)
        {
            EndShowAdAsset(type, m.campaignId, removeElement);
        }

        //if challengeId is define, end only specified ad types, if not - end all
        public void EndShowAdAsset(AdType type, string campaignId = null, bool removeElement = true)
        {
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

        private void _EndShowAdAsset(KeyValuePair<AdType, string> adAsset)
        {
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
            props["application_id"] = challenge.application_id;
            props["bundle_id"] = Application.identifier;
            props["player_id"] = GetUserId();
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["impressions"] = "1";
            //props["campaign_id"] = visibleAdAsset[adAsset].challengeId;
            props["camp_id"] = challenge.id;
            props["brand_id"] = challenge.brand_id;
            props["brand_name"] = brandName;
            props["type"] = adTypeNames[adAsset.Key];
            props["ab_segment"] = MonetizrManager.abTestSegment;
            props["device_size"] = deviceSizeGroupNames[deviceSizeGroup];
            //props["duration"] = (DateTime.Now - visibleAdAsset[type].activateTime).TotalSeconds;

            string eventName = $"[UNITY_SDK] [AD] {adTypeNames[adAsset.Key]}";

            Mixpanel.Identify(challenge.brand_id);
            Mixpanel.Track(eventName, props);

#if !UNITY_EDITOR
            NielsenDar.Track(challenge.id, adAsset.Key);
#endif

#if USING_AMPLITUDE
            Dictionary<string, object> eventProps = new Dictionary<string, object>();
            eventProps.Add("camp_id", challenge.id);
            eventProps.Add("brand_id", challenge.brand_id);
            eventProps.Add("brand_name", brandName);
            eventProps.Add("type", adTypeNames[adAsset.Key]);
            eventProps.Add("duration", (DateTime.Now - visibleAdAsset[adAsset].activateTime).TotalSeconds);
            eventProps.Add("apiKey", MonetizrManager.Instance.GetCurrentAPIkey());
            eventProps.Add("ab_segment", MonetizrManager.abTestSegment);
            eventProps.Add("device_size", deviceSizeGroupNames[deviceSizeGroup]);

            amplitude.logEvent(eventName, eventProps);
#endif

            //if (removeElement)
            //    visibleAdAsset.Remove(adAsset);
        }

        public string GetUserId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public void TrackEvent(string name, Mission currentMissionDesc)
        {
            TrackEvent(name, currentMissionDesc.campaignId);
        }


        public void TrackEvent(string name, string campaign)
        {
            //Log.Print($"TrackEvent: {name} {campaign}");

            var eventName = $"[UNITY_SDK] {name}";

            string campaign_id = "none";
            string brand_id = "none";
            string app_id = "none";
            string brand_name = "none";

            if (campaign == null)
            {
                Log.Print($"MonetizrAnalytics TrackEvent: MissionUIDescription shouldn't be null");
                return;
            }

            var challenge = MonetizrManager.Instance.GetCampaign(campaign);


            //if (currentMissionDesc != null)
            //{
            var ch = challenge.id;

            brand_id = challenge.brand_id;// MonetizrManager.Instance.GetChallenge(ch).brand_id;
            app_id = challenge.application_id;// MonetizrManager.Instance.GetChallenge(ch).application_id;
            brand_name = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.BrandTitleString);
            campaign_id = ch;

            //}


            var props = new Value();
            props["application_id"] = app_id;
            props["bundle_id"] = Application.identifier;
            props["player_id"] = SystemInfo.deviceUniqueIdentifier;
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            //props["campaign_id"] = campaign_id;
            props["camp_id"] = campaign_id;
            props["brand_id"] = brand_id;
            props["brand_name"] = brand_name;
            props["ab_segment"] = MonetizrManager.abTestSegment;
            props["device_size"] = deviceSizeGroupNames[deviceSizeGroup];

            Mixpanel.Identify(challenge.brand_id);
            Mixpanel.Track(eventName, props);

#if USING_AMPLITUDE
            Dictionary<string, object> eventProps = new Dictionary<string, object>();
            eventProps.Add("camp_id", campaign_id);
            eventProps.Add("brand_id", brand_id);
            eventProps.Add("brand_name", brand_name);
            eventProps.Add("apiKey", MonetizrManager.Instance.GetCurrentAPIkey());
            eventProps.Add("ab_segment", MonetizrManager.abTestSegment);
            eventProps.Add("device_size", deviceSizeGroupNames[deviceSizeGroup]);

            amplitude.logEvent(eventName, eventProps);
#endif
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
