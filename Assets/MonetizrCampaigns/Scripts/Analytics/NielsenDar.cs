using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Campaigns;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.SDK.Analytics
{
    internal static class NielsenDar
    {
        internal static readonly Dictionary<DeviceSizeGroup, string> sizeGroupsDict =
            new Dictionary<DeviceSizeGroup, string>()
            {
                { DeviceSizeGroup.Phone, "PHN" },
                { DeviceSizeGroup.Tablet, "TAB" },
                { DeviceSizeGroup.Unknown, "UNWN" },
            };

        internal static Dictionary<string, string> DARPlacementTags = null;

        internal static void Track(ServerCampaign serverCampaign, AdPlacement type)
        {
            string darTagUrl = ReplaceMacros(serverCampaign.dar_tag, serverCampaign, type, "");

            Log.PrintV($"DAR: {darTagUrl}");

#if !UNITY_EDITOR
            UnityWebRequest www = UnityWebRequest.Get(darTagUrl);
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();

            //operation.completed += BundleOperation_CompletedHandler;
#endif
        }

        internal static string ReplaceMacros(string originalString, ServerCampaign serverCampaign, AdPlacement type,
            string userAgent)
        {
            if (string.IsNullOrEmpty(originalString))
                return originalString;

            Dictionary<string, Func<AdPlacement, string>> urlModifiers = new Dictionary<string, Func<AdPlacement, string>>()
            {
                    { "${CREATIVE_ID}", GetCreativeId },
                    { "${OS_GROUP}", GetOsGroup },
                    { "${DEVICE_GROUP}", GetDeviceGroup },
                    { "${ADVERTISING_ID}", GetAdvertisingId },
                    { "${PLATFORM}", GetPlatform },
                    { "${APP_VERSION}", GetAppVersion },
                    { "${OS_VERSION}", GetOsVersion },
                    { "${OPT_OUT}", GetOptOut },
                    { "${VENDOR_ID}", (AdPlacement at) => MonetizrMobileAnalytics.deviceIdentifier },
                    { "${CY}", GetCY },
                    { "${CACHEBUSTER}", GetTimeStamp },
                    { "${CAMP_ID}", (AdPlacement at) => serverCampaign.id },
                    { "${USER_AGENT}", (AdPlacement at) => userAgent },
                    { "${DEVICE_IP}", (AdPlacement at) => serverCampaign.device_ip },
                    { "${DEVICE_OS}", GetDeviceOs },
                    { "${APPBUNDLE}", (AdPlacement at) => MonetizrManager.bundleId },
                    { "${STOREID}", (AdPlacement at) => serverCampaign.serverSettings.GetParam("app.storeid") },
                    { "${STOREURL}", (AdPlacement at) => serverCampaign.serverSettings.GetParam("app.storeurl") },
                    { "${APPNAME}", (AdPlacement at) => serverCampaign.serverSettings.GetParam("app.name") },
                    { "${TAGID}", (AdPlacement at) => serverCampaign.serverSettings.GetParam("imp.tagid") },
                    { "${APPID}", (AdPlacement at) => serverCampaign.serverSettings.GetParam("app.id") },
             };

            var sb = new StringBuilder(originalString);

            foreach (var v in urlModifiers)
            {
                sb.Replace(v.Key, v.Value(type));
            }

            string output = ReplacePlacementTag(sb.ToString(), type);


            return output;
        }

        //{{PLACEMENT_ID=TinyTeaser:Monetizr_plc0001,NotificationScreen:Monetizr_plc0002,Html5VideoScreen:Monetizr_plc0003,EmailEnterScreen:Monetizr_plc0004,CongratsScreen:Monetizr_plc0005}}
        private static string ReplacePlacementTag(string s, AdPlacement t)
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

            var adStr = MonetizrMobileAnalytics.GetPlacementName(t);

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
            return GetDeviceOs(type).ToUpper(CultureInfo.InvariantCulture);
        }

        static string GetDeviceOs(AdPlacement type)
        {
#if UNITY_IOS
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "";
#endif
        }

        private static string GetDeviceGroup(AdPlacement arg)
        {
            return sizeGroupsDict[MonetizrMobileAnalytics.deviceSizeGroup];
        }

        private static string GetOptOut(AdPlacement arg)
        {
            return MonetizrMobileAnalytics.limitAdvertising ? "1" : "0";
        }

        private static string GetOsVersion(AdPlacement arg)
        {
            return MonetizrMobileAnalytics.osVersion;
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
            return MonetizrMobileAnalytics.advertisingID;
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
}
