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

namespace Monetizr.Campaigns
{
    internal class ChallengesClient
    {
        //public PlayerInfo playerInfo { get; set; }

        private const string k_BaseUri = "https://api3.themonetizr.com/";
        private static readonly HttpClient Client = new HttpClient();
        
        public MonetizrAnalytics analytics { get; private set; }
        public string currentApiKey;

        public ChallengesClient(string apiKey, int timeout = 30)
        {
            currentApiKey = apiKey;
            analytics = new MonetizrAnalytics();

            Client.Timeout = TimeSpan.FromSeconds(timeout);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            

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
                    { "sdk-version", MonetizrManager.SDKVersion }
                }
            };

            HttpResponseMessage response = await Client.SendAsync(requestMessage);

            var challengesString = await response.Content.ReadAsStringAsync();

            string responseOk = response.IsSuccessStatusCode == true ? "OK" : "Not OK";

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
                    ch.additional_params = ParseContentString(ch.content);

                    foreach(var v in ch.additional_params)
                        Debug.Log($"!!!! {v.Key}={v.Value}");
                }

                var result = new List<ServerCampaign>(challenges.challenges);

                //remove all campaigns without assets
                result.RemoveAll(e =>
                {
                    return e.assets.Count == 0;
                });

                return result;
            }
            else
            {
                return null;
            }

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

                    Debug.Log($"-----{startId} {endId} {result}");

                    if (res.ContainsKey(result))
                    {
                        value = value.Replace($"%{result}%", res[result]);
                        Debug.Log($"-----replace {result} {res[result]}");
                    }
                    
                }

                res2.Add(key,value);         
            }

            return res2;
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
                    { "sdk-version", MonetizrManager.SDKVersion }
                }
            };

            
            string content = string.Empty;

            if (MonetizrManager.temporaryEmail != null && MonetizrManager.temporaryEmail.Length > 0)
            {
                //requestMessage.Headers.Add("email", MonetizrManager.temporaryEmail);
                content = $"{{\"email\":\"{MonetizrManager.temporaryEmail}\"}}";

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