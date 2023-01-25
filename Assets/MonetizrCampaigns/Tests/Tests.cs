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

    [UnityTest]
    public IEnumerator SDKInitializationTest()
    {
        int loadSuccess = 0;

        Sprite img = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        MonetizrManager.bundleId = "com.monetizer.sample";

        MonetizrManager.SetGameCoinAsset(RewardType.Coins, img, "Coins", 
            () => { return 0; },  
            (ulong reward) => { }, 
            10000);

        MonetizrManager.Initialize("t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo", null, () =>
        {
            if (MonetizrManager.Instance.HasCampaignsAndActive())
            {
                //we can show teaser manually, but better to use TeaserHelper script
                //see DummyMainUI object in SampleScene
                //dummyUI.SetActive(true);

                //MonetizrManager.ShowTinyMenuTeaser();
                //Do something

                loadSuccess = 1;
            }
        },null, null);

        while(loadSuccess == 0)
            yield return null;

        //Assert.IsNotNull(o);

        Debug.Log("done");

        yield return null;
    }
}
