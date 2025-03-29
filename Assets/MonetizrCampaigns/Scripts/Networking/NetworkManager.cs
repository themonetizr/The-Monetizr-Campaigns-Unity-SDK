using System.Net;
using System;
using System.Net.Http;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Debug;
using System.Threading.Tasks;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Campaigns;
using System.Collections.Generic;
using System.Security.Policy;

namespace Monetizr.SDK.Networking
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance;

        private static readonly HttpClient httpClient = new HttpClient();

        private string currentAPIKey = "";
        private string userAgent = "";

        private string baseURL = "https://api.themonetizr.com";
        private string testURL = "https://api-test.themonetizr.com";
        private string campaignsURL = "/api/campaigns";
        private string settingsURL = "/settings";

        private void Awake ()
        {
            Instance = this;
        }

        private void OnDisable ()
        {
            httpClient?.CancelPendingRequests();
        }

        public void Setup (string apiKey)
        {
            currentAPIKey = apiKey;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClient.Timeout = TimeSpan.FromSeconds(30f);
        }

        public async Task<SettingsDictionary<string, string>> GetGlobalSettings ()
        {
            string responseString = await GetResponseStringFromURL(baseURL + settingsURL);

            if (string.IsNullOrEmpty(responseString))
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M400);
                return new SettingsDictionary<string, string>();
            }

            MonetizrLogger.PrintRemoteMessage(MessageEnum.M101);
            MonetizrLogger.Print("Global Settings: " + responseString);

            return new SettingsDictionary<string, string>(MonetizrUtils.ParseContentString(responseString));
        }

        public async Task<List<ServerCampaign>> GetCampaigns()
        {
            string responseString = await GetResponseStringFromURL(baseURL + campaignsURL);
            if (string.IsNullOrEmpty(responseString))
            {
                MonetizrLogger.PrintError("No campaigns were received from request.");
                return new List<ServerCampaign>();
            }

            Campaigns.Campaigns parsedCampaigns = JsonUtility.FromJson<Campaigns.Campaigns>("{\"campaigns\":" + responseString + "}");
            if (parsedCampaigns == null)
            {
                MonetizrLogger.PrintError("No campaigns were correctly parsed.");
                return new List<ServerCampaign>();
            }

            MonetizrLogger.Print("Received Campaigns Count: " + parsedCampaigns.campaigns.Count);
            return parsedCampaigns.campaigns;
        }

        private async Task<string> GetResponseStringFromURL (string url)
        {
            HttpRequestMessage requestMessage = NetworkingUtils.GenerateHttpRequestMessage(userAgent, url, false, currentAPIKey);
            MonetizrLogger.Print("Sending request: " + requestMessage);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            string responseString = await response.Content.ReadAsStringAsync();
            MonetizrLogger.Print("Response Status: " + response.StatusCode + " / Response String: " + responseString);

            if (!response.IsSuccessStatusCode)
            {
                MonetizrLogger.PrintError("Request failed with code: " + response.StatusCode + " / URL: " + url);
                return "";
            }

            if (responseString.Length == 0)
            {
                MonetizrLogger.PrintError("Request returned empty. / URL: " + url);
                return "";
            }

            return responseString;
        }

    }
}