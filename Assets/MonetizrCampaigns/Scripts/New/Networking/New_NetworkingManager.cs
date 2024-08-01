using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Utils;
using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.SDK.New
{
    public class New_NetworkingManager
    {
        public static async Task<byte[]> DownloadAssetData(string url, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            uwr.timeout = 10;

            await uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                MonetizrLog.PrintError($"Network error {uwr.error} with {url}");
                onDownloadFailed?.Invoke();
                return null;
            }

            return uwr.downloadHandler.data;
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
                    {"internet-connection",New_NetworkingUtils.GetInternetConnectionType()},
                    {"local-time-stamp",((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()}
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