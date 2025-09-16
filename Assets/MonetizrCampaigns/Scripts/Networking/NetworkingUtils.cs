using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Prebid;
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

            Dictionary<string, string> resolvedParams = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in queryParams)
            {
                resolvedParams[kv.Key] = MacroUtils.ResolveValue(kv.Value, campaign);
            }

            void Ensure(string key, string value)
            {
                if (!resolvedParams.ContainsKey(key) || string.IsNullOrEmpty(resolvedParams[key]))
                    resolvedParams[key] = value;
            }

            Ensure("app.bundle", MonetizrSettings.bundleID);
            Ensure("app.name", Application.productName);
            Ensure("device.model", SystemInfo.deviceModel);
            Ensure("device.make", SystemInfo.deviceName);
            Ensure("device.os", Application.platform.ToString());
            Ensure("device.osv", SystemInfo.operatingSystem);
            Ensure("device.ifa", MonetizrMobileAnalytics.advertisingID);
            Ensure("device.ua", MonetizrManager.Instance.ConnectionsClient.userAgent);
            Ensure("site.domain", Application.identifier);
            Ensure("site.page", $"https://{Application.identifier}");
            Ensure("regs.gdpr", MonetizrManager.s_gdpr ? "1" : "0");
            Ensure("regs.coppa", MonetizrManager.s_coppa ? "1" : "0");
            Ensure("regs.us_privacy", settings.GetParam("us_privacy", PrebidConsentBridge.GetIabUsPrivacySafe()));
            string consent = MonetizrManager.s_consent ?? PrebidManager.GetIabConsentString() ?? "";
            Ensure("gdpr_consent", consent);

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