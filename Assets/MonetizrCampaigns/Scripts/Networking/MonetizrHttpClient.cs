using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using Monetizr.Raygun4Unity;
using UnityEngine.Networking;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Analytics;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.VAST;

namespace Monetizr.SDK.Networking
{
    internal partial class MonetizrHttpClient : MonetizrClient
    {
        private string _baseApiUrl = "https://api.themonetizr.com";

        private string CampaignsApiUrl => _baseApiUrl + "/api/campaigns";
        private string SettingsApiUrl => _baseApiUrl + "/settings";

        private readonly string _baseTestApiUrl = "https://api-test.themonetizr.com";
        private static readonly HttpClient Client = new HttpClient();
        
        private CancellationTokenSource downloadCancellationTokenSource;
        
        private static async Task RequestEnd(UnityWebRequest request, CancellationToken token)
        {
            request.SendWebRequest();

            Log.PrintV($"Location request sent");

            while (!request.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                await Task.Yield();
            }
        }

        internal async Task<IpApiData> GetIpApiData()
        {
            IpApiData ipApiData = null;

            string uri = $"https://ipapi.co/json/";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                downloadCancellationTokenSource = new CancellationTokenSource();
                downloadCancellationTokenSource.CancelAfter(1000);
                var token = this.downloadCancellationTokenSource.Token;

                try
                {
                    await RequestEnd(webRequest, token);
                }
                catch (OperationCanceledException)
                {
                    Log.PrintV("\nTasks cancelled: timed out.\n");
                }
                finally
                {
                    downloadCancellationTokenSource.Dispose();
                }

                try
                {
                    ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Log.PrintError($"Exception in IpApiData.CreateFromJSON from {webRequest.downloadHandler.text}\n{e}");
                }

                if (ipApiData != null)
                    Log.PrintV($"Location: {ipApiData.country_code} {ipApiData.region_code}");
            }

            return ipApiData;
        }

        public MonetizrHttpClient(string apiKey, int timeout = 30)
        {
            System.Net.ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            currentApiKey = apiKey;
            
            Client.Timeout = TimeSpan.FromSeconds(timeout);
        }

        internal override void Initialize()
        {
            Analytics = new MonetizrMobileAnalytics();
        }

        internal override void SetTestMode(bool testEnvironment)
        {
            if (testEnvironment)
                _baseApiUrl = _baseTestApiUrl;
        }

        internal override void Close()
        {
            Client.CancelPendingRequests();
        }

        public static async Task<(bool isSuccess,string content)> DownloadUrlAsString(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response = null;
            
            try
            {
                response = await Client.SendAsync(requestMessage);
            }
            catch (Exception e)
            {
                throw new DownloadUrlAsStringException($"DownloadUrlAsString exception\nHttpRequestMessage: {requestMessage}\n{e}", e);
            }

            string result = await response.Content.ReadAsStringAsync();

            Log.PrintV($"Download response is: {result} {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
                return (false,"");

            if (result.Length == 0)
                return (false,"");
            
            return (true,result);
        }

        private async Task<SettingsDictionary<string, string>> DownloadGlobalSettings()
        {
            var responseString = await GetStringFromUrl(SettingsApiUrl);

            if (string.IsNullOrEmpty(responseString))
            {
                Log.PrintV($"Unable to load settings!");
                return new SettingsDictionary<string, string>();
            }

            Log.PrintV($"Settings: {responseString}");

            return new SettingsDictionary<string, string>(MonetizrUtils.ParseContentString(responseString));
        }
        

        internal async Task<List<ServerCampaign>> LoadCampaignsListFromServer()
        {
            MonetizrManager.isVastActive = false;
            
            var loadResult = await GetServerCampaignsFromMonetizr();
            
            Log.PrintV($"GetServerCampaignsFromMonetizr result {loadResult.Count}");
            
            return loadResult;
        }

        internal override async Task GetGlobalSettings()
        {
            GlobalSettings = await DownloadGlobalSettings();

            RaygunCrashReportingPostService.defaultApiEndPointForCr = GlobalSettings.GetParam("crash_reports.endpoint",
                "");
            
            _baseApiUrl = GlobalSettings.GetParam("base_api_endpoint",_baseApiUrl);

            Log.PrintV($"Api endpoint: {_baseApiUrl}");
        }

        internal override async Task<List<ServerCampaign>> GetList()
        {
            var result = await LoadCampaignsListFromServer();
            
            RemoveCampaignsWithNoAssets(result);

            RemoveCampaignsWithWrongSDKVersion(result);

            CheckAllowedDevices(result);

            foreach (var ch in result)
                Log.Print($"Campaign passed filters: {ch.id}");

            return result;
        }

        private static void CheckAllowedDevices(List<ServerCampaign> result)
        {
#if !UNITY_EDITOR

            var hasAdId = !string.IsNullOrEmpty(MonetizrMobileAnalytics.advertisingID);

            if (hasAdId)
            {
                result.RemoveAll(e =>
                {
                    string allowed_device_id = e.serverSettings.GetParam("allowed_ad_id", "");

                    if (allowed_device_id.Length == 0)
                    {
                        Log.Print($"Campaign {e.id} has no allowed list");
                        return false;
                    }
                    else
                    {
                        Log.Print($"Campaign {e.id} has allowed list: {allowed_device_id}");

                        bool isKeyFound = false;

                        Array.ForEach(allowed_device_id.Split(';'), id =>
                        {
                            if (id == MonetizrMobileAnalytics.advertisingID)
                                isKeyFound = true;
                        });

                        if (!isKeyFound)
                        {
                            Log.Print($"Device {MonetizrMobileAnalytics.advertisingID} isn't allowed for campaign {e.id}");
                            return true;
                        }
                        else
                        {
                            Log.Print($"Device {MonetizrMobileAnalytics.advertisingID} is OK for campaign {e.id}");
                            return false;
                        }
                    }
                });
            }
            else
            {
                Log.Print($"No ad id defined to filter campaigns. Please allow ad tracking!");
            }
#endif
        }

        private static void RemoveCampaignsWithWrongSDKVersion(List<ServerCampaign> result)
        {
            result.RemoveAll(e =>
            {
                string minSdkVersion = e.serverSettings.GetParam("min_sdk_version");

                if (minSdkVersion != null)
                {
                    bool sdkVersionCheck = MonetizrUtils.CompareVersions(MonetizrManager.SDKVersion, minSdkVersion) < 0;

                    if (sdkVersionCheck)
                    {
                        Log.Print(
                            $"Removing campaign {e.id} because SDK version {MonetizrManager.SDKVersion} less then required SDK version {minSdkVersion}");
                    }

                    return sdkVersionCheck;
                }

                return false;
            });
        }

        private static void RemoveCampaignsWithNoAssets(List<ServerCampaign> result)
        {
            result.RemoveAll(e =>
            {
                bool noAssets = e.assets.Count == 0;

                if (noAssets)
                {
                    Log.Print($"Removing campaign {e.id} with no assets");
                }

                return noAssets;
            });
        }

        internal override async Task<string> GetStringFromUrl(string url)
        {
            var requestMessage = GetHttpRequestMessage(url);

            Log.PrintV($"Sent request: {requestMessage}");

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var responseString = await response.Content.ReadAsStringAsync();

            string responseOk = response.IsSuccessStatusCode == true ? "OK" : "Not OK";

            Log.Print($"Response is: {response.StatusCode}");
            Log.PrintV(responseString);

            if (!response.IsSuccessStatusCode)
            {
                Log.PrintError($"GetStringFromUrl failed with code {response.StatusCode} for {url}");
                return "";
            }

            if (responseString.Length == 0)
            {
                return "";
            }

            return responseString;
        }


        private async Task<List<ServerCampaign>> GetServerCampaignsFromMonetizr()
        {
            var responseString = await GetStringFromUrl(CampaignsApiUrl);
            
            if (string.IsNullOrEmpty(responseString))
            {
                return new List<ServerCampaign>();
            }
            
            var campaigns = JsonUtility.FromJson<Campaigns>("{\"campaigns\":" + responseString + "}");

            if (campaigns == null)
            {
                return new List<ServerCampaign>();
            }

            if(GlobalSettings.GetBoolParam("campaign.use_adm",true))
                campaigns.campaigns = await TryRecreateCampaignsFromAdm(campaigns.campaigns);

            campaigns.campaigns.ForEach(c => c.PostCampaignLoad());
            
            return campaigns.campaigns;
        }

        internal async Task<List<ServerCampaign>> TryRecreateCampaignsFromAdm(List<ServerCampaign> campaigns)
        {
            var admCampaigns = new List<ServerCampaign>();

            var ph = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");

            foreach (var c in campaigns)
            {
                if (string.IsNullOrEmpty(c.adm)) continue;

                var admCampaign = await ph.PrepareServerCampaign(c.id, c.adm, false);

                if(admCampaign != null)
                    admCampaigns.Add(admCampaign);
            }

            if (admCampaigns.Count <= 0) 
                return campaigns;
            
            campaigns.Clear();
            return admCampaigns;
        }

        private HttpRequestMessage GetHttpRequestMessage(string uri, bool isPost = false)
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
                    {"sdk-version", MonetizrManager.SDKVersion},
                    {"os-group", MonetizrMobileAnalytics.GetOsGroup()},
                    {"ad-id", MonetizrMobileAnalytics.advertisingID},
                    {"screen-width", Screen.width.ToString()},
                    {"screen-height", Screen.height.ToString()},
                    {"screen-dpi", Screen.dpi.ToString(CultureInfo.InvariantCulture)},
                    {"device-group",MonetizrMobileAnalytics.GetDeviceGroup().ToString().ToLower()},
                    {"device-memory",SystemInfo.systemMemorySize.ToString()},
                    {"device-model",MonetizrUtils.EncodeStringIntoAscii(SystemInfo.deviceModel)},
                    {"device-name",MonetizrUtils.EncodeStringIntoAscii(SystemInfo.deviceName)},
                    {"internet-connection",MonetizrMobileAnalytics.GetInternetConnectionType()},
                    {"local-time-stamp",((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()}
                }
            };

            output.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MonetizrManager.Instance.ConnectionsClient.currentApiKey);
            output.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (string.IsNullOrEmpty(userAgent)) 
                return output;
            
            output.Headers.Add("User-Agent", userAgent);

            return output;
        }
        
        internal static HttpRequestMessage GetOpenRtbRequestMessage(string url, string content, HttpMethod method)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(method, url);
            httpRequest.Headers.Add("x-openrtb-version", "2.5");
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return httpRequest;
        }

        internal override async Task Reset(string campaignId, CancellationToken ct, Action onSuccess = null,
            Action onFailure = null)
        {
            HttpRequestMessage requestMessage =
                GetHttpRequestMessage($"{CampaignsApiUrl}/{campaignId}/reset");

            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);

            string s = await response.Content.ReadAsStringAsync();

            Log.PrintV($"Reset response: {response.IsSuccessStatusCode} -- {s} -- {response}");

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

        internal override async Task Claim(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null,
            Action onFailure = null)
        {
            HttpRequestMessage requestMessage =
                GetHttpRequestMessage($"{CampaignsApiUrl}/{challenge.id}/claim",true);

            string content = string.Empty;

            if (MonetizrManager.temporaryEmail != null && MonetizrManager.temporaryEmail.Length > 0)
            {
                bool ingame =
                    MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Product
                        ? false
                        : true;

                ServerCampaign.Reward reward = challenge.rewards.Find(
                    (ServerCampaign.Reward r) => { return r.in_game_only == ingame; });

                if (reward == null)
                {
                    Log.PrintError($"Product reward doesn't found for campaign {ingame}");

                    onFailure?.Invoke();
                    return;
                }

                Log.PrintV($"Reward {reward.id} found in_game_only {reward.in_game_only}");

                content = $"{{\"email\":\"{MonetizrManager.temporaryEmail}\",\"reward_id\":\"{reward.id}\"}}";

                MonetizrManager.temporaryEmail = "";
            }

            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");


            Log.PrintV($"Request:\n[{requestMessage}] content:\n[{content}]");

            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);

            string s = await response.Content.ReadAsStringAsync();

            Log.PrintV($"Response: {response.IsSuccessStatusCode} -- {s} -- {response}");

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

        public void SendReportToMixpanel(string openRtbRequest, string res)
        {
            
        }

    }

}