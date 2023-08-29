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
    
}
