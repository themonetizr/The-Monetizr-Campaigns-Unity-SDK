using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
            public Challenge[] challenges;
        }

        /// <summary>
        /// Returns a list of challenges available to the player.
        /// </summary>
        public async Task<List<Challenge>> GetList()
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
                    return new List<Challenge>();
                }

                var challenges = JsonUtility.FromJson<Challenges>("{\"challenges\":" + challengesString + "}");

                //analytics.Update(new List<Challenge>(challenges.challenges));

                foreach (var ch in challenges.challenges)
                {
                    ch.additional_params = ParseContentString(ch.content);

                    foreach(var v in ch.additional_params)
                        Debug.Log($"!!!! {v.Key}={v.Value}");
                }
                
                return new List<Challenge>(challenges.challenges);
            }
            else
            {
                return null;
            }

        }

        //Unity FromJson doesn't support Dictionaries
        private Dictionary<string, string> ParseContentString(string content)
        {
            /*content = content.Replace(@"\", string.Empty);
            //content.Replace(@" ", string.Empty);

            content = content.Replace("{", string.Empty);
            content = content.Replace("}", string.Empty);
            content = content.Replace(" ", string.Empty);
            content = content.Replace("\"", string.Empty);*/

            var replacements = new[] { @"\","{", "}"," ", "\"" }; // "\\{} \"";
            var output = new StringBuilder(content);
            foreach (var r in replacements)
                output.Replace(r, String.Empty);

            content = output.ToString();

            Debug.LogWarning("!!!!: " + content);
            //if (content.Contains('{'))
            //{

             //   return JsonConvert.JsonUtility.FromJson<Dictionary<string, string>>(content);
            //}

            string[] eq = new[] { "=",":"};
            string[] seps = new[] { ","};

            //<p>show_teaser_button=false</p>\r\n\r\n<p>teaser_type=button</p>\r\n\r\n<p>show_campaigns_notification=true</p>\r\n\r\n<p>&nbsp;</p>
            return content.Split(seps, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Split(eq, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(v => v.First(), v => v.Last());
        }

        /// <summary>
        /// Marks the challenge as claimed by the player.
        /// </summary>
        public async Task Claim(Challenge challenge, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
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