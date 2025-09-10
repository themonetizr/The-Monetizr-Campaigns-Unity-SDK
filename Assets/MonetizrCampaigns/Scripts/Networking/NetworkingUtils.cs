using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Utils;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UnityEngine;

namespace Monetizr.SDK.Networking
{
    public static class NetworkingUtils
    {
        public static string GetInternetConnectionType ()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    return "no_connection";
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "mobile";
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "lan";
                default:
                    return "unknown";
            }
        }

        public static bool IsInternetReachable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public static HttpRequestMessage GenerateOpenRTBRequestMessage(string url, string content, HttpMethod method)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(method, url);
            httpRequest.Headers.Add("x-openrtb-version", "2.5");
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return httpRequest;
        }

        public static HttpRequestMessage GenerateHttpRequestMessage(string userAgent, string uri, bool isPost = false)
        {
            var httpMethod = isPost ? HttpMethod.Post : HttpMethod.Get;

            var output = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(uri),
                Headers =
                {
                    {"player-id", MonetizrMobileAnalytics.deviceIdentifier},
                    {"app-bundle-id", MonetizrManager.bundleId},
                    {"sdk-version", MonetizrSettings.SDKVersion},
                    {"os-group", MonetizrMobileAnalytics.GetOsGroup()},
                    {"ad-id", MonetizrMobileAnalytics.advertisingID},
                    {"screen-width", Screen.width.ToString()},
                    {"screen-height", Screen.height.ToString()},
                    {"screen-dpi", Screen.dpi.ToString(CultureInfo.InvariantCulture)},
                    {"device-group",MonetizrMobileAnalytics.GetDeviceGroup().ToString().ToLower()},
                    {"device-memory",SystemInfo.systemMemorySize.ToString()},
                    {"device-model",MonetizrUtils.EncodeStringIntoAscii(SystemInfo.deviceModel)},
                    {"device-name",MonetizrUtils.EncodeStringIntoAscii(SystemInfo.deviceName)},
                    {"internet-connection",NetworkingUtils.GetInternetConnectionType()},
                    {"local-time-stamp",((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()},
                    {"coppa", MonetizrManager.s_coppa.ToString()},
                    {"gdpr", MonetizrManager.s_gdpr.ToString()},
                    {"us_privacy", MonetizrManager.s_us_privacy.ToString()},
                    {"uoo", MonetizrManager.s_uoo.ToString()},
                    {"consent", MonetizrManager.s_consent}
                }
            };

            output.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MonetizrManager.Instance.ConnectionsClient.currentApiKey);
            output.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (string.IsNullOrEmpty(userAgent)) return output;
            output.Headers.Add("User-Agent", userAgent);
            return output;
        }

        public static string BuildEndpointURL (ServerCampaign campaign, string baseUrlOverride = "")
        {
            SettingsDictionary<string, string> settings = campaign.serverSettings;

            string baseUrl = !string.IsNullOrEmpty(baseUrlOverride) ? baseUrlOverride : settings.GetParam("endpoint_base", "");
            if (string.IsNullOrEmpty(baseUrl))
            {
                MonetizrLogger.PrintError("Endpoint base URL missing.");
                return null;
            }

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            string rawParams = settings.GetParam("endpoint_params", "");
            if (!string.IsNullOrEmpty(rawParams))
            {
                try
                {
                    JSONNode json = JSON.Parse(rawParams);
                    if (json != null && json.IsObject)
                    {
                        foreach (KeyValuePair<string, JSONNode> kv in json.AsObject)
                        {
                            queryParams[kv.Key] = kv.Value.Value;
                        }
                    }
                }
                catch (Exception e)
                {
                    MonetizrLogger.PrintError($"Failed to parse endpoint_params JSON: {e}");
                }
            }

            string ResolveMacro(string key, string value)
            {
                if (string.IsNullOrEmpty(value)) return "";

                switch (value)
                {
                    case "${APP_BUNDLE}": return MonetizrSettings.bundleID;
                    case "${APP_NAME}": return Application.productName;
                    case "${APP_STOREURL}": return campaign.serverSettings.GetParam("app.storeurl", "");
                    case "${OS}": return Application.platform.ToString();
                    case "${OS_VERSION}": return SystemInfo.operatingSystem;
                    case "${DEVICE_MODEL}": return SystemInfo.deviceModel;
                    case "${DEVICE_MAKE}": return SystemInfo.deviceName;
                    case "${PLAYER_ID}": return MonetizrMobileAnalytics.advertisingID;
                    case "${GDPR}": return MonetizrManager.s_gdpr.ToString();
                    case "${GDPR_CONSENT}": return MonetizrManager.s_consent;
                    case "${COPPA}": return MonetizrManager.s_coppa.ToString();
                    case "${CCPA}": return MonetizrManager.s_us_privacy.ToString();
                    case "${US_PRIVACY_CONSENT}": return MonetizrManager.s_us_privacy.ToString();
                    case "${DNT}": return "0";
                    case "${LMT}": return (String.IsNullOrEmpty(MonetizrMobileAnalytics.advertisingID) ? "1" : "0");
                    default: return value;
                }
            }

            Dictionary<string, string> resolvedParams = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in queryParams)
            {
                resolvedParams[kv.Key] = ResolveMacro(kv.Key, kv.Value);
            }

            if (!resolvedParams.ContainsKey("app.bundle")) resolvedParams["app.bundle"] = MonetizrSettings.bundleID;
            if (!resolvedParams.ContainsKey("app.name")) resolvedParams["app.name"] = Application.productName;
            if (!resolvedParams.ContainsKey("device.model")) resolvedParams["device.model"] = SystemInfo.deviceModel;
            if (!resolvedParams.ContainsKey("device.make")) resolvedParams["device.make"] = SystemInfo.deviceName;
            if (!resolvedParams.ContainsKey("device.os")) resolvedParams["device.os"] = Application.platform.ToString();
            if (!resolvedParams.ContainsKey("device.osv")) resolvedParams["device.osv"] = SystemInfo.operatingSystem;
            if (!resolvedParams.ContainsKey("device.ifa")) resolvedParams["device.ifa"] = MonetizrMobileAnalytics.advertisingID;
            if (!resolvedParams.ContainsKey("regs.gdpr")) resolvedParams["regs.gdpr"] = MonetizrManager.s_gdpr.ToString();
            if (!resolvedParams.ContainsKey("regs.coppa")) resolvedParams["regs.coppa"] = MonetizrManager.s_coppa.ToString();
            if (!resolvedParams.ContainsKey("gdpr_consent")) resolvedParams["gdpr_consent"] = MonetizrManager.s_consent ?? "";


            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in resolvedParams)
            {
                if (builder.Length > 0) builder.Append("&");
                builder.Append(Uri.EscapeDataString(kv.Key));
                builder.Append("=");
                builder.Append(Uri.EscapeDataString(kv.Value ?? ""));
            }

            string finalUrl = baseUrl + "?" + builder.ToString();
            MonetizrLogger.Print($"Endpoint - Built URL: {finalUrl}");
            return finalUrl;
        }
    }
}