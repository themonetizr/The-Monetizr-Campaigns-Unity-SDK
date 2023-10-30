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

public class UtilsTests
{
    [Test]
    public void Test()
    {
        Assert.AreEqual(Utils.CompareVersions("0.0.15", "0.0.14"), 1);
        Assert.AreEqual(Utils.CompareVersions("0.0.11", "0.0.11"), 0);
        Assert.AreEqual(Utils.CompareVersions("0.0.1", "0.0.11"), -1);

        
        Assert.AreEqual(Utils.ConvertToIntArray("1.2.3")[2], 3);

        Assert.AreEqual(Utils.ConvertToIntArray("1,2,3",',')[2], 3);


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

        var json1 = @"{
        ""key1"": ""value1""
        }";

        var expectedResult1 = new Dictionary<string, string>
        {
            { "key1", "value1" },
        };

        Assert.AreEqual(Utils.ParseJson(json1), expectedResult1);

        var json2 = @"{
        ""key1"": 0,
        ""key2"": ""value2""
        }";

        var expectedResult2 = new Dictionary<string, string>
        {
            { "key1", "0" },
            { "key2", "value2" },
        };
                
        Assert.AreEqual(Utils.ParseJson(json2), expectedResult2);
    }


}
