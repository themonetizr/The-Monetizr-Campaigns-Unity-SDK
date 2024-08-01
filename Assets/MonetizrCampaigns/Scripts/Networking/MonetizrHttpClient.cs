using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using Monetizr.Raygun4Unity;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Analytics;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.VAST;
using Monetizr.SDK.New;

namespace Monetizr.SDK.Networking
{
    internal class MonetizrHttpClient : MonetizrClient
    {
        private string _baseApiUrl = "https://api.themonetizr.com";
        private string CampaignsApiUrl => _baseApiUrl + "/api/campaigns";
        private string SettingsApiUrl => _baseApiUrl + "/settings";
        private readonly string _baseTestApiUrl = "https://api-test.themonetizr.com";
        private static readonly HttpClient Client = new HttpClient();
        private CancellationTokenSource downloadCancellationTokenSource;

        public MonetizrHttpClient(string apiKey, int timeout = 30)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            currentApiKey = apiKey;
            Client.Timeout = TimeSpan.FromSeconds(timeout);
        }

        internal override void Initialize()
        {
            Analytics = new MonetizrMobileAnalytics();
        }

        internal override void SetTestMode(bool testEnvironment)
        {
            if (testEnvironment) _baseApiUrl = _baseTestApiUrl;
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
            MonetizrLog.Print($"Download response is: {result} {response.StatusCode}");
            if (!response.IsSuccessStatusCode) return (false,"");
            if (result.Length == 0) return (false,"");
            return (true, result);
        }

        internal override async Task<string> GetResponseStringFromUrl(string url)
        {
            var requestMessage = New_NetworkingManager.GenerateHttpRequestMessage(userAgent, url);
            MonetizrLog.Print($"Sent request: {requestMessage}");
            HttpResponseMessage response = await Client.SendAsync(requestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            MonetizrLog.Print($"Response is: {response.StatusCode}");
            MonetizrLog.Print(responseString);

            if (!response.IsSuccessStatusCode)
            {
                MonetizrLog.PrintError($"GetStringFromUrl failed with code {response.StatusCode} for {url}");
                return "";
            }

            if (responseString.Length == 0)
            {
                return "";
            }

            return responseString;
        }

        private async Task<SettingsDictionary<string, string>> DownloadGlobalSettings()
        {
            var responseString = await GetResponseStringFromUrl(SettingsApiUrl);

            if (string.IsNullOrEmpty(responseString))
            {
                MonetizrLog.Print($"Unable to load settings!");
                return new SettingsDictionary<string, string>();
            }

            MonetizrLog.Print($"Settings: {responseString}");
            return new SettingsDictionary<string, string>(MonetizrUtils.ParseContentString(responseString));
        }

        internal async Task<List<ServerCampaign>> LoadCampaignsListFromServer()
        {
            MonetizrManager.isVastActive = false;
            var loadResult = await GetServerCampaignsFromMonetizr();
            MonetizrLog.Print($"GetServerCampaignsFromMonetizr result {loadResult.Count}");
            return loadResult;
        }

        internal override async Task GetGlobalSettings()
        {
            GlobalSettings = await DownloadGlobalSettings();
            RaygunCrashReportingPostService.defaultApiEndPointForCr = GlobalSettings.GetParam("crash_reports.endpoint", "");
            _baseApiUrl = GlobalSettings.GetParam("base_api_endpoint",_baseApiUrl);
            MonetizrLog.Print($"Api endpoint: {_baseApiUrl}");
        }

        internal override async Task<List<ServerCampaign>> GetList()
        {
            var result = await LoadCampaignsListFromServer();
            New_CampaignUtils.FilterInvalidCampaigns(result);
            foreach (var ch in result)
            {
                MonetizrLog.Print($"Campaign passed filters: {ch.id}");
            }
            return result;
        }

        private async Task<List<ServerCampaign>> GetServerCampaignsFromMonetizr()
        {
            var responseString = await GetResponseStringFromUrl(CampaignsApiUrl);
            if (string.IsNullOrEmpty(responseString))
            {
                return new List<ServerCampaign>();
            }
            
            var campaigns = JsonUtility.FromJson<Campaigns>("{\"campaigns\":" + responseString + "}");
            if (campaigns == null)
            {
                return new List<ServerCampaign>();
            }

            if(GlobalSettings.GetBoolParam("campaign.use_adm",true)) campaigns.campaigns = await TryRecreateCampaignsFromAdm(campaigns.campaigns);
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
                if (admCampaign != null) admCampaigns.Add(admCampaign);
            }

            if (admCampaigns.Count <= 0)  return campaigns;
            campaigns.Clear();
            return admCampaigns;
        }
        
        internal static HttpRequestMessage GetOpenRtbRequestMessage(string url, string content, HttpMethod method)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(method, url);
            httpRequest.Headers.Add("x-openrtb-version", "2.5");
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return httpRequest;
        }

        internal override async Task Reset(string campaignId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage = New_NetworkingManager.GenerateHttpRequestMessage(userAgent, $"{CampaignsApiUrl}/{campaignId}/reset");
            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);
            string s = await response.Content.ReadAsStringAsync();
            MonetizrLog.Print($"Reset response: {response.IsSuccessStatusCode} -- {s} -- {response}");

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

        internal override async Task Claim(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage = New_NetworkingManager.GenerateHttpRequestMessage(userAgent, $"{CampaignsApiUrl}/{challenge.id}/claim",true);
            string content = string.Empty;

            if (MonetizrManager.temporaryEmail != null && MonetizrManager.temporaryEmail.Length > 0)
            {
                bool ingame = MonetizrManager.temporaryRewardTypeSelection == RewardSelectionType.Product ? false : true;
                Reward reward = challenge.rewards.Find((Reward r) => { return r.in_game_only == ingame; });

                if (reward == null)
                {
                    MonetizrLog.PrintError($"Product reward doesn't found for campaign {ingame}");
                    onFailure?.Invoke();
                    return;
                }

                MonetizrLog.Print($"Reward {reward.id} found in_game_only {reward.in_game_only}");
                content = $"{{\"email\":\"{MonetizrManager.temporaryEmail}\",\"reward_id\":\"{reward.id}\"}}";
                MonetizrManager.temporaryEmail = "";
            }

            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            MonetizrLog.Print($"Request:\n[{requestMessage}] content:\n[{content}]");
            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);
            string s = await response.Content.ReadAsStringAsync();
            MonetizrLog.Print($"Response: {response.IsSuccessStatusCode} -- {s} -- {response}");

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

    }

}