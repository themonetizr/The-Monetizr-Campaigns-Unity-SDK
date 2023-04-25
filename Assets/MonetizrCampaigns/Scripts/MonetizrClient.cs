using System;
using System.Collections.Generic;
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

        private string k_BaseUri = "https://api.themonetizr.com/";
        private static readonly HttpClient Client = new HttpClient();
        
        public MonetizrAnalytics analytics { get; private set; }
        public string currentApiKey;
                

        private CancellationTokenSource downloadCancellationTokenSource;
              
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

                if(ipApiData != null)
                    Log.Print($"Location: {ipApiData.country_code} {ipApiData.region_code}");
            }

            return ipApiData;
        }

        public MonetizrClient(string apiKey, int timeout = 30)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                      
            currentApiKey = apiKey;

            analytics = new MonetizrAnalytics();

            Client.Timeout = TimeSpan.FromSeconds(timeout);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //Client.DefaultRequestHeaders.Add("player-id", analytics.GetUserId());
            //Client.DefaultRequestHeaders.Add("app-bundle-id", MonetizrManager.bundleId);
            //Client.DefaultRequestHeaders.Add("sdk-version", MonetizrManager.SDKVersion);
            //Client.DefaultRequestHeaders.Add("os-group", MonetizrAnalytics.GetOsGroup());
        }

        public void InitializeMixpanel(bool testEnvironment, string mixPanelApiKey)
        {           
            string key = "cda45517ed8266e804d4966a0e693d0d";

            k_BaseUri = "https://api.themonetizr.com/";

            if (testEnvironment)
            {                
                key = "d4de97058730720b3b8080881c6ba2e0";
                k_BaseUri = "https://api-test.themonetizr.com/";
            }


            if (mixPanelApiKey != null)
            {
                //checking corrupted mixpanel key
                if (mixPanelApiKey.Length == 0 || mixPanelApiKey.IndexOf("\n") >= 0)
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

        /// <summary>
        /// Returns a list of challenges available to the player.
        /// </summary>
        public async Task<List<ServerCampaign>> GetList()
        {
            MonetizrManager.isVastActive = false;
            List<ServerCampaign> result = new List<ServerCampaign>();

            VastHelper v = new VastHelper(this);
            //KevelHelper v = new KevelHelper(this);

            if (v != null)
            {
                await v.GetCampaign(result,Client);

                //await v.GetVastCampaign(result);

                if (result.Count != 0)
                {
                    MonetizrManager.isVastActive = true;
                    MonetizrManager.maximumCampaignAmount = result.Count;

                    return result;
                }
            }

            var requestMessage = GetHttpRequestMessage(k_BaseUri + "api/campaigns");

            Log.Print($"Sent request: {requestMessage.ToString()}");

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var challengesString = await response.Content.ReadAsStringAsync();

            string responseOk = response.IsSuccessStatusCode == true ? "OK" : "Not OK";

            //---


            Log.Print($"Response is: {responseOk} {response.StatusCode}");
            Log.Print(challengesString);

            if (!response.IsSuccessStatusCode)
                return result;

            if (challengesString.Length == 0)
                return result;


            var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

            //analytics.Update(new List<Challenge>(challenges.challenges));


            foreach (var ch in challenges.challenges)
            {
                ch.serverSettings = new SettingsDictionary<string, string>(Utils.ParseContentString(ch.content));

                //foreach(var v in ch.additional_params)
                //    Log.Print($"!!!! {v.Key}={v.Value}");

                Log.Print($"Loaded campaign: {ch.id}");

            }

            result = new List<ServerCampaign>(challenges.challenges);

            //remove all campaigns without assets
            result.RemoveAll(e =>
            {
                return e.assets.Count == 0;
            });

            //remove all campaign with SDK version lower than current
            result.RemoveAll(e =>
            {
                string minSdkVersion = e.serverSettings.GetParam("min_sdk_version");

                if (minSdkVersion != null)
                {
                    return Utils.CompareVersions(MonetizrManager.SDKVersion, minSdkVersion) < 0;
                }

                return false;
            });

            //MonetizrAnalytics.advertisingID = "dbdf5873-750a-41a9-a1d4-adf7bb77d9fb";

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

            //if there's some campaigns, filter them by location
            if (result.Count > 0)
            {
                bool needFilter = result[0].serverSettings.GetBoolParam("filter_campaigns_by_location", false);

                if (needFilter)
                {
                    analytics.locationData = await GetIpApiData();

                    if (analytics.locationData != null)
                    {
                        result.RemoveAll(e =>
                        {
                            return !e.IsCampaignInsideLocation(analytics.locationData);
                        });

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

            foreach (var ch in result)
                Log.Print($"Campaign passed filters: {ch.id}");

            return result;


        }

        internal static HttpRequestMessage GetHttpRequestMessage(string uri)
        {
            return new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers =
                {
                    {"player-id",MonetizrAnalytics.deviceIdentifier},
                    {"app-bundle-id", MonetizrManager.bundleId},
                    {"sdk-version", MonetizrManager.SDKVersion},
                    {"os-group", MonetizrAnalytics.GetOsGroup()},
                    {"ad-id", MonetizrAnalytics.advertisingID}
                }
            };
        }

        /// <summary>
        /// Reset the challenge as claimed by the player.
        /// </summary>
        public async Task Reset(string campaignId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage =
                GetHttpRequestMessage(k_BaseUri + "api/campaigns/" + campaignId + "/reset");

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
        public async Task Claim(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage =
                GetHttpRequestMessage(k_BaseUri + "api/campaigns/" + challenge.id + "/claim");
            
            string content = string.Empty;

            if (MonetizrManager.temporaryEmail != null && MonetizrManager.temporaryEmail.Length > 0)
            {
                bool ingame = MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Product ? false : true;

                ServerCampaign.Reward reward = challenge.rewards.Find(
                    (ServerCampaign.Reward r) =>
                    {
                        return r.in_game_only == ingame;
                    });

                if(reward == null)
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

            HttpResponseMessage response = await Client.SendAsync(requestMessage,ct);

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
    }
}