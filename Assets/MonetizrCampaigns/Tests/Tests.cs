using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Monetizr.Campaigns;

public class Tests
{    
    [Test]
    public void UtilsTests()
    {
        Assert.AreEqual(Utils.CompareVersions("0.0.15", "0.0.14"), 1);
        Assert.AreEqual(Utils.CompareVersions("0.0.11", "0.0.11"), 0);
        Assert.AreEqual(Utils.CompareVersions("0.0.1", "0.0.11"), -1);

        string ints = "1.2.3";
        Assert.AreEqual(Utils.ConvertToIntArray(ints)[2], 3);
    }
    
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestsWithEnumeratorPasses()
    {
        object o = null;

        Assert.IsNotNull(o);


        yield return null;
    }
}
