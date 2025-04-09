using System;
using System.Collections.Generic;
using System.Net.Http;
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
using UnityEngine.Networking;
using System.Linq;

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
            MonetizrLogger.Print($"Download response is: {result} {response.StatusCode}");
            if (!response.IsSuccessStatusCode) return (false,"");
            if (result.Length == 0) return (false,"");
            return (true, result);
        }

        internal override async Task<string> GetResponseStringFromUrl(string url)
        {
            var requestMessage = NetworkingUtils.GenerateHttpRequestMessage(userAgent, url);
            MonetizrLogger.Print($"Sent request: {requestMessage}");
            HttpResponseMessage response = await Client.SendAsync(requestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            MonetizrLogger.Print($"Response is: {response.StatusCode}");
            MonetizrLogger.Print(responseString);

            if (!response.IsSuccessStatusCode)
            {
                MonetizrLogger.PrintError($"GetStringFromUrl failed with code {response.StatusCode} for {url}");
                return "";
            }

            if (responseString.Length == 0)
            {
                return "";
            }

            return responseString;
        }

        public static async Task<byte[]> DownloadAssetData(string url, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            uwr.timeout = 10;

            await uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.DataProcessingError)
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M403);
                MonetizrLogger.PrintError($"Network error {uwr.error} with {url}");
                onDownloadFailed?.Invoke();
                return null;
            }

            return uwr.downloadHandler.data;
        }

        internal override async Task GetGlobalSettings()
        {
            GlobalSettings = await DownloadGlobalSettings();
            RaygunCrashReportingPostService.defaultApiEndPointForCr = GlobalSettings.GetParam("crash_reports.endpoint", "");
            _baseApiUrl = GlobalSettings.GetParam("base_api_endpoint",_baseApiUrl);
            MonetizrLogger.Print($"Api endpoint: {_baseApiUrl}");
        }

        private async Task<SettingsDictionary<string, string>> DownloadGlobalSettings()
        {
            var responseString = await GetResponseStringFromUrl(SettingsApiUrl);

            if (string.IsNullOrEmpty(responseString))
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M400);
                return new SettingsDictionary<string, string>();
            }

            MonetizrLogger.PrintRemoteMessage(MessageEnum.M101);
            MonetizrLogger.Print("Global Settings: " + responseString);
            return new SettingsDictionary<string, string>(MonetizrUtils.ParseContentString(responseString));
        }

        internal override async Task<List<ServerCampaign>> GetList()
        {
            List<ServerCampaign> campaigns = await GetServerCampaignsFromMonetizr();
            campaigns = await ProcessCampaigns(campaigns);
            campaigns = CampaignUtils.FilterInvalidCampaigns(campaigns);
            foreach (ServerCampaign campaign in campaigns)
            {
                MonetizrLogger.Print($"Campaign passed filters: {campaign.id}");
            }
            return campaigns;
        }

        private async Task<List<ServerCampaign>> GetServerCampaignsFromMonetizr()
        {
            string responseString = await GetResponseStringFromUrl(CampaignsApiUrl);
            if (string.IsNullOrEmpty(responseString)) return new List<ServerCampaign>();

            Campaigns.Campaigns campaigns = JsonUtility.FromJson<Campaigns.Campaigns>("{\"campaigns\":" + responseString + "}");
            if (campaigns == null) return new List<ServerCampaign>();

            MonetizrLogger.Print("Received Campaigns Count: " + campaigns.campaigns.Count);

            return campaigns.campaigns;
        }

        private async Task<List<ServerCampaign>> ProcessCampaigns(List<ServerCampaign> campaigns)
        {
            for (int i = 0; i < campaigns.Count; i++)
            {
                CampaignUtils.SetupCampaignType(campaigns[i]);
                MonetizrLogger.Print(campaigns[i].id + " / Type: " + campaigns[i].campaignType + "\n" + CampaignUtils.PrintAssetsTypeList(campaigns[i]));

                switch (campaigns[i].campaignType)
                {
                    case CampaignType.MonetizrBackend:
                        campaigns[i] = ProcessBackendCampaign(campaigns[i]);
                        break;
                    case CampaignType.ADM:
                        campaigns[i] = await ProcessADMCampaign(campaigns[i]);
                        break;
                    case CampaignType.Programmatic:
                        campaigns[i] = await ProcessProgrammaticCampaign(campaigns[i]);
                        break;
                    default:
                        MonetizrLogger.PrintError("CampaignID: " + campaigns[i].id + " - No CampaignType was assigned.");
                        break;
                }
            }

            campaigns = campaigns.Where(c => c != null).ToList();

            return campaigns;
        }

        private async Task<ServerCampaign> ProcessADMCampaign (ServerCampaign campaign)
        {
            campaign = await RecreateCampaignFromADM(campaign);
            if (campaign == null) return null;
            campaign.PostCampaignLoad();
            campaign.campaignTimeoutStart = Time.time;
            campaign.hasMadeEarlyBidRequest = true;
            return campaign;
        }

        private ServerCampaign ProcessBackendCampaign (ServerCampaign campaign)
        {
            campaign.PostCampaignLoad();
            return campaign;
        }

        private async Task<ServerCampaign> ProcessProgrammaticCampaign (ServerCampaign campaign)
        {
            campaign.PostCampaignLoad();
            campaign = await MakeEarlyProgrammaticBidRequest(campaign);
            if (campaign == null) return null;
            campaign.campaignTimeoutStart = Time.time;
            campaign = await RecreateCampaignFromADM(campaign);
            return campaign;
        }

        internal async Task<ServerCampaign> RecreateCampaignFromADM (ServerCampaign campaign)
        {
            PubmaticHelper pubmaticHelper = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");
            campaign = await pubmaticHelper.PrepareServerCampaign(campaign.id, campaign.adm, false);
            return campaign;
        }

        internal async Task<ServerCampaign> MakeEarlyProgrammaticBidRequest(ServerCampaign campaign)
        {
            MonetizrLogger.Print("PBR - Started");
            PubmaticHelper pubmaticHelper = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");

            if (!String.IsNullOrEmpty(campaign.adm))
            {
                bool initializeResult = await pubmaticHelper.InitializeServerCampaignForProgrammatic(campaign, campaign.adm);
                campaign.hasMadeEarlyBidRequest = initializeResult;

                if (initializeResult)
                {
                    MonetizrLogger.Print("Programmatic with ADM initialization successful.");
                }
                else
                {
                    MonetizrLogger.Print("Programmatic with ADM initialization failed.");
                }

                return null;
            }

            bool isProgrammaticOK = false;
            try
            {
                isProgrammaticOK = await pubmaticHelper.TEST_GetOpenRtbResponseForCampaign(campaign);
            }
            catch (DownloadUrlAsStringException e)
            {
                MonetizrLogger.PrintError($"PBR - Exception DownloadUrlAsStringException in campaign {campaign.id}\n{e}");
                isProgrammaticOK = false;
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintError($"PBR - Exception in GetOpenRtbResponseForCampaign in campaign {campaign.id}\n{e}");
                isProgrammaticOK = false;
            }

            MonetizrLogger.Print(isProgrammaticOK ? "PBR - COMPLETED" : "PBR - FAILED");
            campaign.hasMadeEarlyBidRequest = isProgrammaticOK;
            return campaign;
        }

        internal override async Task ResetCampaign(string campaignId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage = NetworkingUtils.GenerateHttpRequestMessage(userAgent, $"{CampaignsApiUrl}/{campaignId}/reset");
            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);
            string s = await response.Content.ReadAsStringAsync();
            MonetizrLogger.Print($"Reset response: {response.IsSuccessStatusCode} -- {s} -- {response}");

            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
            }
        }

        internal override async Task ClaimReward(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage = NetworkingUtils.GenerateHttpRequestMessage(userAgent, $"{CampaignsApiUrl}/{challenge.id}/claim",true);
            string content = string.Empty;

            if (MonetizrManager.temporaryEmail != null && MonetizrManager.temporaryEmail.Length > 0)
            {
                bool ingame = MonetizrManager.temporaryRewardTypeSelection == RewardSelectionType.Product ? false : true;
                Reward reward = challenge.rewards.Find((Reward r) => { return r.in_game_only == ingame; });

                if (reward == null)
                {
                    MonetizrLogger.PrintError($"Product reward doesn't found for campaign {ingame}");
                    onFailure?.Invoke();
                    return;
                }

                MonetizrLogger.Print($"Reward {reward.id} found in_game_only {reward.in_game_only}");
                content = $"{{\"email\":\"{MonetizrManager.temporaryEmail}\",\"reward_id\":\"{reward.id}\"}}";
                MonetizrManager.temporaryEmail = "";
            }

            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            MonetizrLogger.Print($"Request:\n[{requestMessage}] content:\n[{content}]");
            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);
            string s = await response.Content.ReadAsStringAsync();
            MonetizrLogger.Print($"Response: {response.IsSuccessStatusCode} -- {s} -- {response}");

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