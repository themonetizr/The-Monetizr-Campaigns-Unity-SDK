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
        public void MiscTest()
        {
            Assert.AreEqual(MonetizrUtils.CompareVersions("0.0.15", "0.0.14"), 1);
            Assert.AreEqual(MonetizrUtils.CompareVersions("0.0.11", "0.0.11"), 0);
            Assert.AreEqual(MonetizrUtils.CompareVersions("0.0.1", "0.0.11"), -1);


            Assert.AreEqual(MonetizrUtils.ConvertToIntArray("1.2.3")[2], 3);
            Assert.AreEqual(MonetizrUtils.ConvertToIntArray("1,2,3", ',')[2], 3);


            Assert.AreEqual(MonetizrUtils.EncodeStringIntoAscii(""), "");
            Assert.AreEqual(MonetizrUtils.EncodeStringIntoAscii(null), "");
            Assert.AreEqual(MonetizrUtils.EncodeStringIntoAscii("Hello World"), "Hello World");
            Assert.AreEqual(MonetizrUtils.EncodeStringIntoAscii("Héllø Wørld"), "H\\u00e9ll\\u00f8 W\\u00f8rld");
            Assert.AreEqual(MonetizrUtils.EncodeStringIntoAscii("ÄÖÜ"), "\\u00c4\\u00d6\\u00dc");

            //Assert.AreNotEqual(Utils.ShuffleList({"1,2,3"}), 3);
        }

        public void PrintDictionary(Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> entry in dict)
            {
                Debug.LogWarning($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }

        [Test]
        public void ParseJsonTest()
        {
            var result = new Dictionary<string, string>();

            Assert.AreEqual(MonetizrUtils.ParseJson(null), new Dictionary<string, string>());

            var json1 = @"{""key1"": ""value1""}";

            result = new Dictionary<string, string>
            {
                { "key1", "value1" },
            };

            Assert.AreEqual(MonetizrUtils.ParseJson(json1), result);

            var json2 = @"{""key1"": 0,""key2"": ""value2""}";

            result = new Dictionary<string, string>
            {
                { "key1", "0" },
                { "key2", "value2" },
            };

            Assert.AreEqual(MonetizrUtils.ParseJson(json2), result);
            
            var campaignString =
                "{\"id\": \"ae37d078-2035-453e-b9e6-8b768a54a02e\", \"brand_id\": \"8650c7be8de6ad1fe15a6eea37c916e25656be74\", \"application_id\": \"ba0cc092-79f9-46c6-a715-a0d4cdaa6751\", \"title\": \"title\", \"content\": \"{\\\"bg_color\\\": \\\"#478EEB\\\", \\\"bg_color2\\\": \\\"#478EEB\\\", \\\"link_color\\\": \\\"#AAAAFF\\\", \\\"text_color\\\": \\\"#FFFFFF\\\", \\\"design_version\\\": \\\"2\\\", \\\"bg_border_color\\\": \\\"#8DCBF0\\\", \\\"settings_global\\\": \\\"true\\\", \\\"amount_of_teasers\\\": \\\"100\\\", \\\"amount_of_notifications\\\": \\\"100\\\", \\\"StartNotification.header_text\\\": \\\"<b>Rewards by Monetizr!</b>\\\", \\\"custom_missions\\\": \\\"{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}\\\", \\\"min_sdk_version\\\": \\\"1.0.1\\\", \\\"mixpanel.testmode\\\": \\\"false\\\", \\\"StartNotification.button_text\\\": \\\"Learn more!\\\", \\\"StartNotification.content_text\\\": \\\"Join Monetizr challenges<br/>to get game rewards\\\", \\\"openrtb.sent_report_to_mixpanel\\\": \\\"true\\\", \\\"CongratsNotification.button_text\\\": \\\"Awesome!\\\", \\\"CongratsNotification.header_text\\\": \\\"Get your reward!\\\", \\\"CongratsNotification.content_text\\\": \\\"You have earned <b>%ingame_reward%</b> from Monetizr\\\", \\\"RewardCenter.show_for_one_mission\\\": \\\"false\\\", \\\"RewardCenter.VideoReward.content_text\\\": \\\"Watch video and get reward %ingame_reward%\\\", \\\"RewardCenter.do_not_claim_and_hide_missions\\\": \\\"true\\\"}\", \"end_date\": \"2024-07-26\", \"rewards\": [], \"requires_email_address\": false, \"claimed\": false, \"dar_tag\": \"\", \"testmode\": false, \"panel_key\": \"cda45517ed8266e804d4966a0e693d0d\", \"device_ip\": \"10.24.133.147\", \"frequency\": {\"impressions\": 0, \"days\": 0}}";

            result = MonetizrUtils.ParseJson(campaignString);

            Assert.AreEqual( "title", result["title"]);

            result = MonetizrUtils.ParseJson(result["content"]);
            
            Assert.AreEqual("{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}", result["custom_missions"]);

            //Test macros

            var jsonList = new List<string>()
            {
                "{\"test_key\": \"${TEST_MACRO}\"}",
                "{\\\"test_key\\\": \\\"${TEST_MACRO}\\\"}",
                "{\\\\\"test_key\\\\\": \\\\\"${TEST_MACRO}\\\\\"}",
                "{\\\\\\\"test_key\\\\\\\": \\\\\\\"${TEST_MACRO}\\\\\\\"}"
            };


            foreach (var js in jsonList)
            {
                result = MonetizrUtils.ParseJson(js);
                Assert.AreEqual("${TEST_MACRO}", result["test_key"]);
            }

            //Test custom missions
            
            jsonList = new List<string>()
            {
                "{\"test_key\": \"{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}\"}",
                "{\\\"test_key\\\": \\\"{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}\\\"}",
            };


            foreach (var js in jsonList)
            {
                result = MonetizrUtils.ParseJson(js);

                Assert.AreEqual("{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}", result["test_key"]);
            }
        }

        [Test]
        public void ServerDefinedMissionsTest()
        {
            var testData = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(0, ""),
                new Tuple<int, string>(0, null),
                new Tuple<int, string>(0, "{dwedwedweed"),
                new Tuple<int, string>(1, "{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}"),
                new Tuple<int, string>(1, "{\"missions\": [{\"type\":\"VideoReward\",\"percent_amount\":\"100\",\"id\":\"0\"}]}"),
                new Tuple<int, string>(1, "{\\\"missions\\\": [{\\\"type\\\":\\\"VideoReward\\\",\\\"percent_amount\\\":\\\"100\\\",\\\"id\\\":\\\"0\\\"}]}"),
            };

            foreach (var td in testData)
            {
                //Debug.Log($"{td.Item2}");
                Assert.AreEqual(td.Item1, MissionsManager.ServerMissionsHelper.CreateFromJson(td.Item2).missions.Count);
            }
            
        }

        [Test]
        public void ParseContentStringTest()
        {
            string jsonContent = "";
            Dictionary<string, string> expected = new Dictionary<string, string>();

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = null;
            expected = new Dictionary<string, string>();

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));
            
            jsonContent = "{\"key1\":\"value1\",\"key2\":\"%key1%\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value1" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"value1\",\"key2\":\"%key3%\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "%key3%" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"%key2%\",\"key2\":\"%key3%\",\"key3\":\"value\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "value" },
                { "key2", "value" },
                { "key3", "value" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"value1\",\"key2\":\"%key1\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "%key1" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"%key2%\",\"key2\":\"%key3%\",\"key3\":\"value\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "value" },
                { "key2", "value" },
                { "key3", "value" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"%key2%\",\"key2\":\"%key3%\",\"key3\":\"%key4%\",\"key4\":\"finalValue\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "finalValue" },
                { "key2", "finalValue" },
                { "key3", "finalValue" },
                { "key4", "finalValue" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"%key2%\",\"key2\":\"%key1%\"}"; 
            
            expected = new Dictionary<string, string>
            {
                { "key1", "%key1%" }, 
                { "key2", "%key2%" }
            };
            
            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"%key2%\",\"key2\":\"%key3% value\",\"key3\":\"Hello\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "Hello value" },
                { "key2", "Hello value" },
                { "key3", "Hello" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));

            jsonContent = "{\"key1\":\"Hello %key2%\",\"key2\":\"world with %key3%\",\"key3\":\"additional %key4%\",\"key4\":\"content\"}";
            expected = new Dictionary<string, string>
            {
                { "key1", "Hello world with additional content" },
                { "key2", "world with additional content" },
                { "key3", "additional content" },
                { "key4", "content" }
            };

            Assert.AreEqual(expected, MonetizrUtils.ParseContentString(jsonContent));
        }


        [Test]
        public void ScoreConvertTest()
        {
            double score = 1234.5;
            string expected = "1.2k";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 5;
            expected = "5";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 123;
            expected = "123";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 1234.5678;
            expected = "1.2k";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 1_000_000;
            expected = "1M";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 1_123_000;
            expected = "1.1M";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 1e+30;
            expected = "1af";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = 0;
            expected = "0";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));

            score = -1500;
            expected = "-1.5k";
            Assert.AreEqual(expected, MonetizrUtils.ScoresToString(score));


        }

        [Test]
        public void ArrayToListTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
                MonetizrUtils.AddArrayToList<string, string>(null, Array.Empty<string>(), str => str, null));

            Assert.Throws<ArgumentNullException>(() =>
                MonetizrUtils.AddArrayToList(new List<string>(), Array.Empty<string>(), null, null));

            var list = new List<string>();
            MonetizrUtils.AddArrayToList<string,string>(list, null, str => str, "default");

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("default", list[0]);

            var list2 = new List<string>();
            MonetizrUtils.AddArrayToList<string, string>(list2, null, str => str, null);
            
            Assert.AreEqual(0, list2.Count);
            

            var listInts = new List<int>();
            var array = new[] { "1", "2", "3" };
            MonetizrUtils.AddArrayToList<string, int>(listInts, array, str => int.Parse(str), 0);
            
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, listInts);

            var array3 = new[] { "1", null, "3" };
            var list3 = new List<string>();
            MonetizrUtils.AddArrayToList(list3, array3, str => str, null);

            CollectionAssert.AreEqual(new[] { "1", "3" }, list3);
        }

    }
}