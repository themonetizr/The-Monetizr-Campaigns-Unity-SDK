using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Monetizr.SDK.Utils
{
    internal static class MacroUtils
    {
        private static readonly Regex MacroRx = new Regex(@"\$\{([A-Z0-9_\.]+)\}", RegexOptions.Compiled);

        internal static string ResolveToken (string token, ServerCampaign campaign)
        {
            if (campaign == null) campaign = new ServerCampaign("", "", new SettingsDictionary<string, string>());

            string ua = MonetizrManager.Instance.ConnectionsClient.userAgent ?? campaign.serverSettings.GetParam("app.clientua", campaign.serverSettings.GetParam("app.deviceua", "UnityPlayer"));
            string storeUrl = campaign.serverSettings.GetParam("app.storeurl", "");
            string sitePage = campaign.serverSettings.GetParam("site.page", string.IsNullOrEmpty(storeUrl) ? "https://unity3d.com" : storeUrl);
            string siteDomain = campaign.serverSettings.GetParam("site.domain", "");

            if (string.IsNullOrEmpty(siteDomain))
            {
                try { Uri u = new Uri(sitePage); siteDomain = u.Host; } catch { siteDomain = MonetizrSettings.bundleID; }
            }

            string gdprConsent = !string.IsNullOrEmpty(MonetizrManager.s_consent) ? MonetizrManager.s_consent : PrebidConsentBridge.GetIabTcfConsentSafe();
            string usPrivacy = campaign.serverSettings.GetParam("us_privacy", PrebidConsentBridge.GetIabUsPrivacySafe());
            string geoLat = "", geoLon = "";

            try
            {
                if (Input.location.status == LocationServiceStatus.Running)
                {
                    geoLat = Input.location.lastData.latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    geoLon = Input.location.lastData.longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch { }

            switch (token)
            {
                // App / site
                case "APP_BUNDLE": return MonetizrSettings.bundleID;
                case "APP_NAME": return Application.productName;
                case "APP_VERSION": return Application.version;
                case "APP_STOREURL": return storeUrl;
                case "SITE_PAGE": return sitePage;
                case "SITE_DOMAIN": return siteDomain;

                // Device / OS
                case "OS": return Application.platform.ToString();
                case "OS_VERSION": return SystemInfo.operatingSystem;
                case "DEVICE_MODEL": return SystemInfo.deviceModel;
                case "DEVICE_MAKE": return GetDeviceMake();
                case "DEVICE_UA":
                case "CLIENTUA": return ua;
                case "SERVERUA": return campaign.serverSettings.GetParam("app.serverua", "");
                case "DEVICE_IP": return campaign.serverSettings.GetParam("app.deviceip", campaign.device_ip ?? "");

                // IDs & privacy
                case "IFA":
                case "PLAYER_ID":
                case "AAID":
                case "ANDROID_DEVICE_ID":
                case "IDFA":
                case "iOS_DEVICE_ID": return MonetizrMobileAnalytics.advertisingID ?? "";
                case "LMT":
                case "LAT": return MonetizrMobileAnalytics.limitAdvertising ? "1" : "0";
                case "DNT": return "0"; // If you track your own DNT flag, map it here
                case "COPPA": return MonetizrManager.s_coppa ? "1" : "0";
                case "GDPR": return MonetizrManager.s_gdpr ? "1" : "0";
                case "GDPR_CONSENT": return gdprConsent ?? "";
                case "US_PRIVACY_CONSENT":
                case "IABUSPrivacy_String": return usPrivacy ?? "";

                // GEO
                case "GEO_LAT": return geoLat;
                case "GEO_LON": return geoLon;

                default:
                    // allow custom server-provided app.* overrides (e.g., app.someparam)
                    string v = campaign.serverSettings.GetParam($"app.{token}", "");
                    return v;
            }
        }

        internal static string ResolveValue (string value, ServerCampaign campaign)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (!value.StartsWith("${")) return value;
            var m = MacroRx.Match(value);
            return m.Success ? ResolveToken(m.Groups[1].Value, campaign) : value;
        }

        internal static string ExpandMacrosInText (string text, ServerCampaign campaign)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return MacroRx.Replace(text, m => ResolveToken(m.Groups[1].Value, campaign) ?? "");
        }

        private static string GetDeviceMake ()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var b = new AndroidJavaClass("android.os.Build")) return b.GetStatic<string>("MANUFACTURER"); }
            catch { return SystemInfo.deviceName; }
#elif UNITY_IOS && !UNITY_EDITOR
            return "Apple";
#else
            return SystemInfo.deviceName;
#endif
        }
    }

    internal static class PrebidConsentBridge
    {
        internal static string GetIabTcfConsentSafe ()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var cls = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge"))
                    return cls.CallStatic<string>("getIabTcfConsent"); }
            catch { return ""; }
#else
            return "";
#endif
        }

        internal static string GetIabUsPrivacySafe ()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var cls = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge"))
                    return cls.CallStatic<string>("getIabUsPrivacy"); }
            catch { return ""; }
#else
            return "";
#endif
        }
    }
}
