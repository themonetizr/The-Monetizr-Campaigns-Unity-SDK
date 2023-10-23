using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using Mindscape.Raygun4Unity;
using UnityEngine.Networking;

namespace Monetizr.Campaigns
{
    [Serializable]
    internal class IpApiData
    {
        public string country_name;
        public string country_code;
        public string region_code;

        public static IpApiData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<IpApiData>(jsonString);
        }
    }

    internal class MonetizrClient
    {
        //public PlayerInfo playerInfo { get; set; }

        private string _baseApiUrl = "https://api.themonetizr.com";

        private string CampaignsApiUrl
        {
            get
            {
                return _baseApiUrl + "/api/campaigns";
            }
        }
        
        private string SettingsApiUrl
        {
            get
            {
                return _baseApiUrl + "/settings";
            }
        }

        private readonly string _baseTestApiUrl = "https://api-test.themonetizr.com";
        private static readonly HttpClient Client = new HttpClient();

        public MonetizrAnalytics analytics { get; private set; }
        public string currentApiKey;


        private CancellationTokenSource downloadCancellationTokenSource;
        
        public static string currentUserAgent;

        internal HttpClient GetHttpClient()
        {
            return Client;
        }
        
        private static async Task RequestEnd(UnityWebRequest request, CancellationToken token)
        {
            request.SendWebRequest();

            Log.PrintV($"Location request sent");

            while (!request.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    //Log.Print("Task {0} cancelled");
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

        public MonetizrClient(string apiKey, int timeout = 30)
        {
            System.Net.ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            currentApiKey = apiKey;

            GlobalSettings = new SettingsDictionary<string, string>();
                
            analytics = new MonetizrAnalytics();

            Client.Timeout = TimeSpan.FromSeconds(timeout);
            //Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void InitializeMixpanel(bool testEnvironment, string mixPanelApiKey, string apiUri = null)
        {
            string key = "cda45517ed8266e804d4966a0e693d0d";
            
            //k_BaseUri = "https://api.themonetizr.com/api/campaigns";

            if (!string.IsNullOrEmpty(apiUri))
                _baseApiUrl = apiUri;

            if (testEnvironment)
            {
                key = "d4de97058730720b3b8080881c6ba2e0";
                _baseApiUrl = _baseTestApiUrl;
            }
            
            if (!string.IsNullOrEmpty(mixPanelApiKey))
            {
                //checking corrupted mixpanel key
                if (mixPanelApiKey.IndexOf("\n", StringComparison.Ordinal) >= 0)
                    mixPanelApiKey = null;

                key = mixPanelApiKey;
            }

            analytics.InitializeMixpanel(key);
        }

        public void Close()
        {
            Client.CancelPendingRequests();
        }

        [Serializable]
        private class Campaigns
        {
            public List<ServerCampaign> campaigns;
        }

        internal class DownloadUrlAsStringException : Exception
        {
            public DownloadUrlAsStringException()
            {
            }

            public DownloadUrlAsStringException(string message)
                : base(message)
            {
            }

            public DownloadUrlAsStringException(string message, Exception inner)
                : base(message, inner)
            {
            }
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
            var responseString = await RequestStringFromUrl(SettingsApiUrl);

            if (string.IsNullOrEmpty(responseString))
            {
                Log.PrintV($"Unable to load settings!");
                return new SettingsDictionary<string, string>();
            }

            Log.PrintV($"Settings: {responseString}");

            return new SettingsDictionary<string, string>(Utils.ParseContentString(responseString));
        }
        

        internal async Task<List<ServerCampaign>> LoadCampaignsListFromServer()
        {
            MonetizrManager.isVastActive = false;

            //await DownloadGlobalSettings();
            
            var loadResult = await GetServerCampaignsFromMonetizr();
            
            Log.PrintV($"GetServerCampaignsFromMonetizr result {loadResult.Count}");
            
            return loadResult;
        }

        internal async Task GetGlobalSettings()
        {
            GlobalSettings = await DownloadGlobalSettings();

            RaygunCrashReportingPostService.defaultApiEndPointForCr = GlobalSettings.GetParam("crash_reports.endpoint",
                "https://api.raygun.com/entries");
            
            _baseApiUrl = GlobalSettings.GetParam("base_api_endpoint",_baseApiUrl);
        }

        internal SettingsDictionary<string, string> GlobalSettings { get; private set; }

        public async Task<List<ServerCampaign>> GetList()
        {
            var result = await LoadCampaignsListFromServer();
            
            RemoveCampaignsWithNoAssets(result);

            RemoveCampaignsWithWrongSDKVersion(result);

            CheckAllowedDevices(result);
            
            //disabled due to API changes
            //await FilterCampaignsByLocation(result);

            foreach (var ch in result)
                Log.Print($"Campaign passed filters: {ch.id}");

            return result;
        }

        private async Task FilterCampaignsByLocation(List<ServerCampaign> result)
        {
            if (result.Count > 0)
            {
                bool needFilter = result[0].serverSettings.GetBoolParam("filter_campaigns_by_location", false);

                if (needFilter)
                {
                    analytics.locationData = await GetIpApiData();

                    if (analytics.locationData != null)
                    {
                        result.RemoveAll(e => { return !e.IsCampaignInsideLocation(analytics.locationData); });

                        if (result.Count > 0)
                        {
                            Log.Print($"{result.Count} campaigns passed location filter");
                        }
                    }
                    else
                    {
                        Log.Print($"No location data");
                    }
                }
                else
                {
                    Log.Print($"Geo-filtering disabled");
                }
            }
        }

        private static void CheckAllowedDevices(List<ServerCampaign> result)
        {
#if !UNITY_EDITOR
            //keep campaigns only for allowed devices

            var hasAdId = !string.IsNullOrEmpty(MonetizrAnalytics.advertisingID);

            if (hasAdId)
            {
                result.RemoveAll(e =>
                {
                    /*if (e.testmode)
                    {
                        Log.PrintV($"Campaign {e.id} in test mode, no device filtering");
                        return false;
                    }*/

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
                            if (id == MonetizrAnalytics.advertisingID)
                                isKeyFound = true;
                        });

                        //if (!allowed_device_id.Contains(MonetizrAnalytics.advertisingID))
                        if (!isKeyFound)
                        {
                            Log.Print($"Device {MonetizrAnalytics.advertisingID} isn't allowed for campaign {e.id}");
                            return true;
                        }
                        else
                        {
                            Log.Print($"Device {MonetizrAnalytics.advertisingID} is OK for campaign {e.id}");
                            return false;
                        }

                        //return !allowed_device_id.Contains(MonetizrAnalytics.advertisingID);
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
            //remove all campaign with SDK version lower than current
            result.RemoveAll(e =>
            {
                string minSdkVersion = e.serverSettings.GetParam("min_sdk_version");

                if (minSdkVersion != null)
                {
                    bool sdkVersionCheck = Utils.CompareVersions(MonetizrManager.SDKVersion, minSdkVersion) < 0;

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
            //remove all campaigns without assets
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

        private async Task<string> RequestStringFromUrl(string url)
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
                Log.PrintError($"RequestStringFromUrl failed with code {response.StatusCode} for {url}");
                return "";
            }

            if (responseString.Length == 0)
            {
                //Log.PrintError($"RequestStringFromUrl has empty response {response.StatusCode} for {url}");
                return "";
            }

            return responseString;
        }


        private async Task<List<ServerCampaign>> GetServerCampaignsFromMonetizr()
        {
            var responseString = await RequestStringFromUrl(CampaignsApiUrl);
            
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

            var ph = new PubmaticHelper(MonetizrManager.Instance.Client, "");

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

        internal HttpRequestMessage GetHttpRequestMessage(string uri, string userAgent = null, bool isPost = false)
        {
            var httpMethod = isPost ? HttpMethod.Post : HttpMethod.Get;
            
            var output = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(uri),
                Headers =
                {
                    {"player-id", MonetizrAnalytics.deviceIdentifier},
                    {"app-bundle-id", MonetizrManager.bundleId},
                    {"sdk-version", MonetizrManager.SDKVersion},
                    {"os-group", MonetizrAnalytics.GetOsGroup()},
                    {"ad-id", MonetizrAnalytics.advertisingID},
                    {"screen-width", Screen.width.ToString()},
                    {"screen-height", Screen.height.ToString()},
                    {"screen-dpi", Screen.dpi.ToString(CultureInfo.InvariantCulture)},
                    {"device-group",MonetizrAnalytics.GetDeviceGroup().ToString().ToLower()},
                    {"device-memory",SystemInfo.systemMemorySize.ToString()},
                    {"device-model",Utils.EncodeStringIntoAscii(SystemInfo.deviceModel)},
                    {"device-name",Utils.EncodeStringIntoAscii(SystemInfo.deviceName)},
                    {"internet-connection",MonetizrAnalytics.GetInternetConnectionType()}
                }
            };

            output.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MonetizrManager.Instance.Client.currentApiKey);
            output.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (string.IsNullOrEmpty(userAgent)) 
                return output;
            
            //Log.PrintError(userAgent);
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

        /// <summary>
        /// Reset the challenge as claimed by the player.
        /// </summary>
        public async Task Reset(string campaignId, CancellationToken ct, Action onSuccess = null,
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

        /// <summary>
        /// Marks the challenge as claimed by the player.
        /// </summary>
        public async Task Claim(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null,
            Action onFailure = null)
        {
            HttpRequestMessage requestMessage =
                GetHttpRequestMessage($"{CampaignsApiUrl}/{challenge.id}/claim","",true);

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

        /*public void SendErrorToRemoteServer(string type, string shortDescription, string fullDescription)
        {
            if (!string.IsNullOrEmpty(fullDescription) && fullDescription.Length > 1024)
                fullDescription = fullDescription.Substring(0, 1024);

            string url = $"https://unity-notification-channel-to-slack-stineosy7q-uc.a.run.app/?message=\"{fullDescription}\"";

            var requestMessage = MonetizrClient.GetHttpRequestMessage(url);

            _ = Client.SendAsync(requestMessage);
        }*/

        public void SendReportToMixpanel(string openRtbRequest, string res)
        {
            
        }
    }
}