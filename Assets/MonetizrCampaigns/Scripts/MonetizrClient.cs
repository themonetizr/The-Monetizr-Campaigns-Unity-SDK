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

        internal HttpClient GetHttpClient()
        {
            return Client;
        }
        
        private static async Task RequestEnd(UnityWebRequest request, CancellationToken token)
        {
            request.SendWebRequest();
            Log.Print($"Location request sent");

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
                    Log.Print("\nTasks cancelled: timed out.\n");
                }
                finally
                {
                    downloadCancellationTokenSource.Dispose();
                }

                try
                {
                    ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);
                }
                catch (Exception)
                {
                }

                if (ipApiData != null)
                    Log.Print($"Location: {ipApiData.country_code} {ipApiData.region_code}");
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
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //Client.DefaultRequestHeaders.Add("player-id", analytics.GetUserId());
            //Client.DefaultRequestHeaders.Add("app-bundle-id", MonetizrManager.bundleId);
            //Client.DefaultRequestHeaders.Add("sdk-version", MonetizrManager.SDKVersion);
            //Client.DefaultRequestHeaders.Add("os-group", MonetizrAnalytics.GetOsGroup());
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
        private class Challenges
        {
            public ServerCampaign[] challenges;
        }

        public static async Task<(bool isSuccess,string content)> DownloadUrlAsString(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            string result = await response.Content.ReadAsStringAsync();

            Log.Print($"Download response is: {result} {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
                return (false,"");

            if (result.Length == 0)
                return (false,"");
            
            return (true,result);
        }
        
        public async Task<SettingsDictionary<string, string>> DownloadGlobalSettings()
        {
            var requestMessage = GetHttpRequestMessage(SettingsApiUrl);

            Log.Print($"Sent settings: {requestMessage.ToString()}");

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var resultString = await response.Content.ReadAsStringAsync();

            string responseOk = response.IsSuccessStatusCode == true ? "OK" : "Not OK";

            //---

            Log.Print($"Settings response is: {response.StatusCode}");
            Log.Print($"Settings: {resultString}");

            SettingsDictionary<string, string> result = new SettingsDictionary<string, string>();

            if (!response.IsSuccessStatusCode)
                return result;

            if (resultString.Length == 0)
                return result;

            //var dict = Json.Deserialize(adp) as Dictionary<string, object>;
            result.dictionary = Utils.ParseContentString(resultString);

            return result;
        }

        public async Task<List<ServerCampaign>> LoadCampaignsListFromServer()
        {
            MonetizrManager.isVastActive = false;
            //List<ServerCampaign> result = new List<ServerCampaign>();

            //loading settings
            GlobalSettings = await DownloadGlobalSettings();
            
            //load regular campaigns
            
            var loadResult = await GetServerCampaignsFromMonetizr();
            
            Log.PrintVerbose($"GetServerCampaignsFromMonetizr result {loadResult.isSuccess}");
            
            if(loadResult.isSuccess)
            { 
                return loadResult.result;
            }

            //VastHelper v = new VastHelper(this);
            //KevelHelper v = new KevelHelper(this);
            PubmaticHelper v = new PubmaticHelper(this);

            var programmaticCampaignResult = await v.GetProgrammaticCampaign(this);
            if (programmaticCampaignResult.isSuccess && 
                programmaticCampaignResult.result.Count > 0)
            {
                MonetizrManager.isVastActive = true;
                MonetizrManager.maximumCampaignAmount = programmaticCampaignResult.result.Count;
                
                return programmaticCampaignResult.result;
            }

            return new List<ServerCampaign>();
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

            if (!string.IsNullOrEmpty(MonetizrAnalytics.advertisingID))
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

        private async Task<(bool isSuccess,List<ServerCampaign> result)> GetServerCampaignsFromMonetizr()
        {
            var requestMessage = GetHttpRequestMessage(CampaignsApiUrl);

            Log.Print($"Sent request: {requestMessage.ToString()}");

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var challengesString = await response.Content.ReadAsStringAsync();

            string responseOk = response.IsSuccessStatusCode == true ? "OK" : "Not OK";

            //---


            Log.Print($"Response is: {responseOk} {response.StatusCode}");
            Log.Print(challengesString);

            if (!response.IsSuccessStatusCode)
            {
                //list = result;
                return (false,new List<ServerCampaign>());
            }

            if (challengesString.Length == 0)
            {
                //list = result;
                return (false, new List<ServerCampaign>());
            }
            
            var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

            if (challenges.challenges.Length == 0)
            {
                return (false, new List<ServerCampaign>());
            }
            
            foreach (var ch in challenges.challenges)
            {
                Log.Print($"-----{ch.content}");
                var localSettings = new SettingsDictionary<string, string>(Utils.ParseContentString(ch.content));
                ch.serverSettings = localSettings;
                Log.Print($"Loaded campaign: {ch.id}");
            }

            return (true,new List<ServerCampaign>(challenges.challenges));
        }

        internal static HttpRequestMessage GetHttpRequestMessage(string uri, bool isPost = false)
        {
            var httpMethod = isPost ? HttpMethod.Post : HttpMethod.Get;

           

            return new HttpRequestMessage
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
                    {"device-model",SystemInfo.deviceModel},
                    {"device-name",SystemInfo.deviceName},
                    {"internet-connection",MonetizrAnalytics.GetInternetConnectionType()}
                }
            };
        }
        
        internal static HttpRequestMessage GetOpenRtbRequestMessage(string url, string content, HttpMethod method)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(method, url);
            httpRequest.Headers.Add("x-openrtb-version", "2.5");
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

            Log.Print($"Reset response: {response.IsSuccessStatusCode} -- {s} -- {response}");

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
                    Log.Print($"Product reward doesn't found for campaign {ingame}");

                    onFailure?.Invoke();
                    return;
                }

                Log.Print($"Reward {reward.id} found in_game_only {reward.in_game_only}");

                content = $"{{\"email\":\"{MonetizrManager.temporaryEmail}\",\"reward_id\":\"{reward.id}\"}}";

                MonetizrManager.temporaryEmail = "";
            }

            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");


            Log.Print($"Request:\n[{requestMessage}] content:\n[{content}]");

            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);

            string s = await response.Content.ReadAsStringAsync();

            Log.Print($"Response: {response.IsSuccessStatusCode} -- {s} -- {response}");

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

        public void SendErrorToRemoteServer(string type, string shortDescription, string fullDescription)
        {
            string url = $"https://unity-notification-channel-to-slack-stineosy7q-uc.a.run.app/?message=\"{fullDescription}\"";

            var requestMessage = MonetizrClient.GetHttpRequestMessage(url);

            _ = Client.SendAsync(requestMessage);
        }

        public void SendReportToMixpanel(string openRtbRequest, string res)
        {
            
        }
    }
}