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

public class Tests
{
    int loadSuccess = 0;
    Dictionary<string, int> eventsAmount = new Dictionary<string, int>();

    [Test]
    public void UtilsTests()
    {
        Assert.AreEqual(Utils.CompareVersions("0.0.15", "0.0.14"), 1);
        Assert.AreEqual(Utils.CompareVersions("0.0.11", "0.0.11"), 0);
        Assert.AreEqual(Utils.CompareVersions("0.0.1", "0.0.11"), -1);

        
        Assert.AreEqual(Utils.ConvertToIntArray("1.2.3")[2], 3);

        Assert.AreEqual(Utils.ConvertToIntArray("1,2,3",',')[2], 3);
    }

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("Default Camera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        go.transform.SetAsFirstSibling();

        PlayerPrefs.SetString("campaigns", "");
        PlayerPrefs.SetString("missions", "");

        Sprite img = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        MonetizrManager.bundleId = "com.monetizr.sample";

        MonetizrManager.claimForSkippedCampaigns = true;



        MonetizrManager.SetGameCoinAsset(RewardType.Coins, img, "Coins",
            () => { return 0; },
            (ulong reward) => { },
            10000);

        MonetizrManager.Initialize("t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo", null, () =>
        {
            Assert.IsTrue(MonetizrManager.Instance.HasCampaignsAndActive(), "Campaign should have active campaigns");

            if (MonetizrManager.Instance.HasCampaignsAndActive())
            {
                //we can show teaser manually, but better to use TeaserHelper script
                //see DummyMainUI object in SampleScene
                //dummyUI.SetActive(true);

                MonetizrManager.ShowTinyMenuTeaser();
                //Do something

                loadSuccess = 1;
            }
        }, null, null,
        (string campaignId, string placement, MonetizrManager.EventType eventType) =>
        {
            if (eventType == MonetizrManager.EventType.Impression)
            {
                if (eventsAmount.ContainsKey(placement))
                    eventsAmount[placement]++;
                else
                    eventsAmount[placement] = 1;
            }
        });
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

    private void UserDefinedEvent(string campaignId, string placement, MonetizrManager.EventType eventType)
    {
        throw new NotImplementedException();
    }

    [UnityTest]
    public IEnumerator SDKTest()
    {
       
        while(loadSuccess == 0)
            yield return null;

        
        yield return new WaitForSeconds(1);

        FindButtonAndClickIt("MonetizrMenuTeaser");

        yield return new WaitForSeconds(1);
        
        FindButtonAndClickIt("RewardCenterCloseButton");

        yield return new WaitForSeconds(1);

        FindButtonAndClickIt("MonetizrMenuTeaser");

        yield return new WaitForSeconds(1);

        FindButtonAndClickIt("RewardCenterCloseButton");

        yield return new WaitForSeconds(1);

        MonetizrManager.HideTinyMenuTeaser();

        yield return new WaitForSeconds(1);

        MonetizrManager.ShowTinyMenuTeaser();

        yield return new WaitForSeconds(1);

        //foreach (var v in eventsAmount)
        //   Debug.Log($"-----------impr {v.Key} {v.Value}");
        //while (!isTestDone)
        //    yield return null;
        //Assert.IsNotNull(o);

        Assert.AreEqual(eventsAmount["TinyTeaser"], 2);
        Assert.AreEqual(eventsAmount["RewardsCenterScreen"], 2);

        //------

        



        Debug.Log("done");

        //yield return null;
    }

    internal class ClickTask
    {
        public string button;
        public int delay;

        public ClickTask(string button, int delay)
        {
            this.button = button;
            this.delay = delay;
        }
    }


    [UnityTest]
    public IEnumerator RewardCenterItemsTest()
    {
        while (loadSuccess == 0)
            yield return null;

        yield return new WaitForSeconds(1);


        var tasks = new List<ClickTask>() {
            new ClickTask("MonetizrMenuTeaser",2),

            new ClickTask("RewardCenterButtonClaim0",3),
            new ClickTask("PanelCloseButton",2),
            new ClickTask("MessageCloseButton",2),
            new ClickTask("NotifyCloseButton",2),

            new ClickTask("RewardCenterButtonClaim1",3),
            new ClickTask("PanelCloseButton",2),
            new ClickTask("NotifyCloseButton",2),

            new ClickTask("RewardCenterButtonClaim2",3),
            new ClickTask("PanelCloseButton",2),
            new ClickTask("NotifyCloseButton",2),

            new ClickTask("RewardCenterButtonClaim3",3),
            new ClickTask("PanelCloseButton",2),
            new ClickTask("MessageCloseButton",2),
            new ClickTask("NotifyCloseButton",2),
        };


        foreach(var t in tasks)
        {
            FindButtonAndClickIt(t.button);

            yield return new WaitForSeconds(t.delay);
        };

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
