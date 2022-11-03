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
using static System.Net.WebRequestMethods;
using UnityEngine.Networking;

namespace Monetizr.Campaigns
{
    [Serializable]
    public class IpApiData
    {
        public string country_name;

        public static IpApiData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<IpApiData>(jsonString);
        }
    }


    

    internal class ChallengesClient
    {
        //public PlayerInfo playerInfo { get; set; }

        private string k_BaseUri = "https://api.themonetizr.com/";
        private static readonly HttpClient Client = new HttpClient();
        
        public MonetizrAnalytics analytics { get; private set; }
        public string currentApiKey;

        internal async Task<IpApiData> GetIpApiData()
        {
            IpApiData ipApiData = null;

            //string ip = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            string uri = $"https://ipapi.co/json/";
                      
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                await webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);

                Debug.Log(ipApiData.country_name);
            }

            return ipApiData;
        }

        public ChallengesClient(string apiKey, int timeout = 30)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                      
            currentApiKey = apiKey;

            analytics = new MonetizrAnalytics();

            Client.Timeout = TimeSpan.FromSeconds(timeout);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
       
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
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(k_BaseUri + "api/challenges"),
                Headers =
                {
                    //{"location", playerInfo.location},
                    //{"age", playerInfo.age.ToString()},
                    //{"game-type", playerInfo.gameType},
                    {"player-id", analytics.GetUserId()},
                    { "app-bundle-id", Application.identifier },
                    { "sdk-version", MonetizrManager.SDKVersion },
                    
                }
            };

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var challengesString = await response.Content.ReadAsStringAsync();

            string responseOk = response.IsSuccessStatusCode == true ? "OK" : "Not OK";

            //---

            var locData = await GetIpApiData();


            Log.Print($"Response is: {responseOk} {response.StatusCode}");
            Log.Print(challengesString);

            if (response.IsSuccessStatusCode)
            {
                if(challengesString.Length == 0)
                {
                    return new List<ServerCampaign>();
                }

                var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

                //analytics.Update(new List<Challenge>(challenges.challenges));


                foreach (var ch in challenges.challenges)
                {
                    ch.serverSettings = new SettingsDictionary<string,string>(ParseContentString(ch.content));

                    //foreach(var v in ch.additional_params)
                    //    Debug.Log($"!!!! {v.Key}={v.Value}");
                }
                
                var result = new List<ServerCampaign>(challenges.challenges);

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
                        return CompareVersions(MonetizrManager.SDKVersion, minSdkVersion) < 0;
                    }

                    return false;
                });



                return result;
            }
            else
            {
                return null;
            }

        }

        private int CompareVersions(string First, string Second)
        {
            var f = Array.ConvertAll(First.Split('.'), (v) => { int k = 0; return int.TryParse(v, out k) ? k : 0; });
            var s = Array.ConvertAll(Second.Split('.'), (v) => { int k = 0; return int.TryParse(v, out k) ? k : 0; });
              
            for(int i = 0; i < 3; i++)
            {
                int f_i = 0;

                if (f.Length > i)
                    f_i = f[i];

                int s_i = 0;

                if (s.Length > i)
                    s_i = s[i];

                if (f_i > s_i)
                    return 1;

                if (f_i < s_i)
                    return -1;
            }

            return 0;
        }

        private Dictionary<string, string> ParseJson(string content)
        {
            content = content.Trim(new[] { '{', '}' }).Replace('\'', '\"');

            var trimmedChars = new[] { ' ', '\"' };

            //regex to split only unquoted separators
            Regex regxComma = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            Regex regxColon = new Regex(":(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] commaSplit = regxComma.Split(content);

            return regxComma.Split(content)
                            .Select(v => regxColon.Split(v))
                            .ToDictionary(v => v.First().Trim(trimmedChars), v => v.Last().Trim(trimmedChars));
        }

        //Unity FromJson doesn't support Dictionaries
        private Dictionary<string, string> ParseContentString(string content)
        {
            Dictionary<string, string> res = ParseJson(content);

            Dictionary<string, string> res2 = new Dictionary<string, string>();

            foreach (var p in res)
            {
                string value = p.Value;
                string key = p.Key;

                for(int i = 0; i < 5; i++)
                {
                    int startId = value.IndexOf('%');

                    if (startId == -1)
                        break;

                    int endId = value.IndexOf('%', startId + 1);

                    if (endId == -1)
                        break;

                    string result = value.Substring(startId + 1, endId - startId - 1);

                    //Debug.Log($"-----{startId} {endId} {result}");

                    if (res.ContainsKey(result))
                    {
                        value = value.Replace($"%{result}%", res[result]);
                        //Debug.Log($"-----replace {result} {res[result]}");
                    }
                    
                }

                res2.Add(key,value);         
            }

            return res2;
        }

        /// <summary>
        /// Reset the challenge as claimed by the player.
        /// </summary>
        public async Task Reset(string campaignId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(k_BaseUri + "api/campaigns/" + campaignId + "/reset"),
                Headers =
                {
                    {"player-id", analytics.GetUserId()},
                    {"app-bundle-id", Application.identifier },
                    {"sdk-version", MonetizrManager.SDKVersion },
                    
                }
            };
                        
            HttpResponseMessage response = await Client.SendAsync(requestMessage, ct);

            string s = await response.Content.ReadAsStringAsync();

            Debug.Log($"Reset response: {response.IsSuccessStatusCode} -- {s} -- {response}");

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
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(k_BaseUri + "api/campaigns/" + challenge.id + "/claim"),
                Headers =
                {
                    //{"location", playerInfo.location},
                    //{"age", playerInfo.age.ToString()},
                    //{"game-type", playerInfo.gameType},
                    {"player-id", analytics.GetUserId()},
                    //{"duration", analytics.GetElapsedTime(challenge).ToString()}

                    { "app-bundle-id", Application.identifier },
                    { "sdk-version", MonetizrManager.SDKVersion },
                  
                }
            };

            
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
                    onFailure?.Invoke();
                    return;
                }

                Debug.Log($"Reward {reward.id} found in_game_only {reward.in_game_only}");

                content = $"{{\"email\":\"{MonetizrManager.temporaryEmail}\",\"reward_id\":\"{reward.id}\"}}";

                MonetizrManager.temporaryEmail = "";
            }

            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

            
            Debug.Log($"Request: {requestMessage}");

            HttpResponseMessage response = await Client.SendAsync(requestMessage,ct);

            string s = await response.Content.ReadAsStringAsync();

            Debug.Log($"Response: {response.IsSuccessStatusCode} -- {s} -- {response}");

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