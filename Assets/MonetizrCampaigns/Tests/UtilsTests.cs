using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Monetizr.Campaigns;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using System;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

namespace MonetizrCampaigns.Tests
{
    public class UtilsTests
    {
        [Test]
        public void Test()
        {
            Assert.AreEqual(Utils.CompareVersions("0.0.15", "0.0.14"), 1);
            Assert.AreEqual(Utils.CompareVersions("0.0.11", "0.0.11"), 0);
            Assert.AreEqual(Utils.CompareVersions("0.0.1", "0.0.11"), -1);


            Assert.AreEqual(Utils.ConvertToIntArray("1.2.3")[2], 3);

            Assert.AreEqual(Utils.ConvertToIntArray("1,2,3", ',')[2], 3);


            Assert.AreEqual(Utils.EncodeStringIntoAscii(""), "");
            Assert.AreEqual(Utils.EncodeStringIntoAscii(null), "");
            Assert.AreEqual(Utils.EncodeStringIntoAscii("Hello World"), "Hello World");
            Assert.AreEqual(Utils.EncodeStringIntoAscii("Héllø Wørld"), "H\\u00e9ll\\u00f8 W\\u00f8rld");
            Assert.AreEqual(Utils.EncodeStringIntoAscii("ÄÖÜ"), "\\u00c4\\u00d6\\u00dc");

            //Assert.AreNotEqual(Utils.ShuffleList({"1,2,3"}), 3);
        }

        public void PrintDictionary(Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> entry in dict)
            {
                Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }

        [Test]
        public void ParseJsonTest()
        {
            Assert.AreEqual(Utils.ParseJson(null), new Dictionary<string, string>());

            var json1 = @"{""key1"": ""value1""}";

            var expectedResult1 = new Dictionary<string, string>
            {
                { "key1", "value1" },
            };

            Assert.AreEqual(Utils.ParseJson(json1), expectedResult1);

            var json2 = @"{""key1"": 0,""key2"": ""value2""}";

            var expectedResult2 = new Dictionary<string, string>
            {
                { "key1", "0" },
                { "key2", "value2" },
            };

            Assert.AreEqual(Utils.ParseJson(json2), expectedResult2);

            var campaignString =
                "{\"id\": \"ae37d078-2035-453e-b9e6-8b768a54a02e\", \"brand_id\": \"8650c7be8de6ad1fe15a6eea37c916e25656be74\", \"application_id\": \"ba0cc092-79f9-46c6-a715-a0d4cdaa6751\", \"title\": \"title\", \"content\": \"{\\\"bg_color\\\": \\\"#478EEB\\\", \\\"bg_color2\\\": \\\"#478EEB\\\", \\\"link_color\\\": \\\"#AAAAFF\\\", \\\"text_color\\\": \\\"#FFFFFF\\\", \\\"design_version\\\": \\\"2\\\", \\\"bg_border_color\\\": \\\"#8DCBF0\\\", \\\"settings_global\\\": \\\"true\\\", \\\"amount_of_teasers\\\": \\\"100\\\", \\\"amount_of_notifications\\\": \\\"100\\\", \\\"StartNotification.header_text\\\": \\\"<b>Rewards by Monetizr!</b>\\\", \\\"custom_missions\\\": \\\"{\"missions\": [{\"type\":\"VideoReward\",\"percent_amount\":\"100\",\"id\":\"0\"}]}\\\", \\\"min_sdk_version\\\": \\\"1.0.1\\\", \\\"mixpanel.testmode\\\": \\\"false\\\", \\\"StartNotification.button_text\\\": \\\"Learn more!\\\", \\\"StartNotification.content_text\\\": \\\"Join Monetizr challenges<br/>to get game rewards\\\", \\\"openrtb.sent_report_to_mixpanel\\\": \\\"true\\\", \\\"CongratsNotification.button_text\\\": \\\"Awesome!\\\", \\\"CongratsNotification.header_text\\\": \\\"Get your reward!\\\", \\\"CongratsNotification.content_text\\\": \\\"You have earned <b>%ingame_reward%</b> from Monetizr\\\", \\\"RewardCenter.show_for_one_mission\\\": \\\"false\\\", \\\"RewardCenter.VideoReward.content_text\\\": \\\"Watch video and get reward %ingame_reward%\\\", \\\"RewardCenter.do_not_claim_and_hide_missions\\\": \\\"true\\\"}\", \"end_date\": \"2024-07-26\", \"rewards\": [], \"requires_email_address\": false, \"claimed\": false, \"dar_tag\": \"\", \"testmode\": false, \"panel_key\": \"cda45517ed8266e804d4966a0e693d0d\", \"device_ip\": \"10.24.133.147\", \"frequency\": {\"impressions\": 0, \"days\": 0}}";

            var result = Utils.ParseJson(campaignString);

            Assert.AreEqual(result["title"], "title");

            result = Utils.ParseJson(result["content"]);

            Debug.Log(result["custom_missions"]);

            Assert.AreEqual( "{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}", result["custom_missions"]);
        }


    }
}