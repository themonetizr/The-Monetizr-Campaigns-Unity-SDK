using Monetizr.SDK.Missions;
using Monetizr.SDK.Utils;
using NUnit.Framework;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Monetizr.Tests
{
    public class NetworkingTests
    {
        private string baseApiUrl = "https://api.themonetizr.com";
        private string campaignsAPIurl = "https://api.themonetizr.com/api/campaigns";
        private string settingsAPIurl = "https://api.themonetizr.com/settings";

        private string testBaseAPIurl = "https://api-test.themonetizr.com";
        private string testCampaignsAPIurl = "https://api-test.themonetizr.com/api/campaigns";
        private string testSettingsAPIurl = "https://api-test.themonetizr.com/settings";

        [SetUp]
        public void Setup ()
        {
            TestManager.Setup();
        }

        [UnityTest, Order(1)]
        public IEnumerator SimpleAPICall ()
        {
            UnityWebRequest request = UnityWebRequest.Get("https://official-joke-api.appspot.com/random_joke");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Assert.NotNull(responseText);
            }
            else
            {
                Assert.Fail();
            }
        }

        [UnityTest, Order(2)]
        public IEnumerator SimpleMonetizrAPICall ()
        {
            UnityWebRequest request = UnityWebRequest.Get(baseApiUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Assert.NotNull(responseText);
            }
            else
            {
                Assert.Fail();
            }
        }

        [UnityTest, Order(3)]
        public IEnumerator MonetizrSettingsAPICall ()
        {
            UnityWebRequest request = TestUtils.GetUnityWebRequest(settingsAPIurl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Assert.Fail(request.downloadHandler.text);
            }

            Assert.IsTrue(request.downloadHandler.text.Contains("bg_color"));
            SettingsDictionary<string, string> settingsDictionary = new SettingsDictionary<string, string>(MonetizrUtils.ParseContentString(request.downloadHandler.text));
            Assert.IsNotNull(settingsDictionary);
        }

        [UnityTest, Order(4)]
        public IEnumerator MonetizrCampaignAPICall ()
        {
            UnityWebRequest request = TestUtils.GetUnityWebRequest(campaignsAPIurl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Assert.NotNull(request.downloadHandler.text);
            }
            else
            {
                Assert.Fail(request.downloadHandler.text);
            }


        }

    }
}