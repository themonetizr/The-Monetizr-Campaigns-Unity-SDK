using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Utils;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System;
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
                    {"coppa", MonetizrManager.Instance.coppa.ToString()},
                    {"gdpr", MonetizrManager.Instance.gdpr.ToString()},
                    {"us_privacy", MonetizrManager.Instance.us_privacy.ToString()},
                    {"uoo", MonetizrManager.Instance.uoo.ToString()},
                    {"consent", MonetizrManager.Instance.consent}
                }
            };

            output.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MonetizrManager.Instance.ConnectionsClient.currentApiKey);
            output.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (string.IsNullOrEmpty(userAgent)) return output;
            output.Headers.Add("User-Agent", userAgent);
            return output;
        }
    }
}