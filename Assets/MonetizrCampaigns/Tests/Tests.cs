using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Monetizr.Campaigns;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;

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

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("Default Camera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        go.transform.SetAsFirstSibling();

    }

    [TearDown]
    public void Teardown()
    {
        
    }

    GameObject FindObjectByName(string name)
    {
        GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));

        for (int i = 0; i < gos.Length; i++)
            if (gos[i].name.Contains(name))
               return gos[i];

        return null;
    }

    [UnityTest]
    public IEnumerator SDKInitializationTest()
    {
        PlayerPrefs.SetString("campaigns", "");
        PlayerPrefs.SetString("missions", "");

        bool isTestDone = false;

        int loadSuccess = 0;

        Sprite img = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        MonetizrManager.bundleId = "com.monetizr.sample";
                

        MonetizrManager.SetGameCoinAsset(RewardType.Coins, img, "Coins", 
            () => { return 0; },  
            (ulong reward) => { }, 
            10000);

        MonetizrManager.Initialize("t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo", null, () =>
        {
            Assert.IsTrue(MonetizrManager.Instance.HasCampaignsAndActive(),"Campaign should have active campaigns");

            if (MonetizrManager.Instance.HasCampaignsAndActive())
            {
                //we can show teaser manually, but better to use TeaserHelper script
                //see DummyMainUI object in SampleScene
                //dummyUI.SetActive(true);

                MonetizrManager.ShowTinyMenuTeaser();
                //Do something

                loadSuccess = 1;
            }
        },null, null);

        while(loadSuccess == 0)
            yield return null;

        //clicking teaser button

        FindButtonAndClickIt("MonetizrMenuTeaser");

        //closing RC

        FindButtonAndClickIt("CloseButton");


        //while (!isTestDone)
        //    yield return null;
        //Assert.IsNotNull(o);

        Debug.Log("done");

        //yield return null;
    }

    void FindButtonAndClickIt(string name)
    {        
        var buttonObject = FindObjectByName(name);

        Assert.IsNotNull(buttonObject);

        Button b = buttonObject.GetComponent<Button>();

        Assert.IsNotNull(b);

        b.onClick.Invoke();

        Debug.Log("");
    }
}
