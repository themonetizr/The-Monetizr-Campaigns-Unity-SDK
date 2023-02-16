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

public class Tests
{
    static int loadSuccess = 0;
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

        eventsAmount.Clear();

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

    static GameObject FindObjectByName(string name)
    {
        GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));

        for (int i = 0; i < gos.Length; i++)
            if (gos[i].name.Contains(name))
                return gos[i];

        return null;
    }
     
    [UnityTest]
    public IEnumerator SDKTest()
    {
        
        
        var tasks = new List<TestUITask>() {
            new ClickTask("MonetizrMenuTeaser",2),

            new ClickTask("RewardCenterCloseButton",3),
            new ClickTask("MonetizrMenuTeaser",2),
            new ClickTask("RewardCenterCloseButton",2),
            new TeaserShowTask(false,1),
            new TeaserShowTask(true,1),
        };


        yield return new TasksManager().Run(tasks);

        //---

        Assert.AreEqual(eventsAmount["TinyTeaser"], 4);
        Assert.AreEqual(eventsAmount["RewardsCenterScreen"], 2);

        //------

        //Log.Print("done");

        yield return null;
    }

   


    [UnityTest]
    public IEnumerator RewardCenterItemsTest()
    {
        var tasks = new List<TestUITask>() {
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

            new ClickTask("RewardCenterButtonClaim4",4),
            new ClickTask("PanelCloseButton",2),
            new ClickTask("MessageCloseButton",2),
            new ClickTask("NotifyCloseButton",2),
        };

        yield return new TasksManager().Run(tasks);
    }

    [UnityTest]
    public IEnumerator TestEmailScreen()
    {
        var tasks = new List<TestUITask>() {

            new ClickTask("MonetizrMenuTeaser",2),

            new ClickTask("RewardCenterButtonClaim4",2),
            new InputFieldTask("PanelInputField","aa@a.a",2),
            new TestInteractivity("PanelOkButton",false,1),

            new ClickTask("PanelCloseButton",2),
            new ClickTask("MessageCloseButton",2),
            //new ClickTask("NotifyCloseButton",2),
        };

        yield return new TasksManager().Run(tasks);

        //yield return null;
    }

    class TasksManager
    {
        List<TestUITask> tasks = new List<TestUITask>();

        public TasksManager Add(TestUITask task)
        {
            tasks.Add(task);
            return this;
        }

        public TasksManager Add(List<TestUITask> tasks)
        {
            this.tasks.AddRange(tasks);
            return this;
        }

        public IEnumerator Run(List<TestUITask> tasks)
        {
            while (Tests.loadSuccess == 0)
                yield return null;


            yield return new WaitForSeconds(1);

            this.tasks.AddRange(tasks);

            foreach (var t in tasks)
            {
                t.Do();

                yield return new WaitForSeconds(t.delay);
            };
        }

    }

    abstract class TestUITask
    {
        public string button;
        public int delay;

        public TestUITask(string button, int delay)
        {
            this.button = button;
            this.delay = delay;
        }

        abstract public void Do();
    }

    class ClickTask : TestUITask
    {        
        public ClickTask(string button, int delay) : base(button, delay)
        {
 
        }

        public override void Do()
        {        
            var buttonObject = Tests.FindObjectByName(button);

            Assert.IsNotNull(buttonObject, $"Game object {button} is null!");

            Button b = buttonObject.GetComponent<Button>();

            Assert.IsTrue(b.interactable, $"Button {button} is not active!");

            Assert.IsNotNull(b,$"Button {button} is null!");

            b.onClick.Invoke();
        }
    }

    class TestInteractivity : TestUITask
    {
        bool interactive;

        public TestInteractivity(string button, bool interactive, int delay) : base(button, delay)
        {
            this.interactive = interactive;
        }

        public override void Do()
        {
            var buttonObject = Tests.FindObjectByName(button);

            Assert.IsNotNull(buttonObject, $"Game object {button} is null!");

            Button b = buttonObject.GetComponent<Button>();

            Assert.IsTrue(b.interactable == interactive, $"Button {button} is wrongly interactive. Should be {interactive}!");

            Assert.IsNotNull(b, $"Button {button} is null!");
        }
    }

    class InputFieldTask : TestUITask
    {
        string text;

        public InputFieldTask(string button, string text, int delay) : base(button, delay)
        {
            this.text = text;
        }

        public override void Do()
        {
            var buttonObject = FindObjectByName(button);

            Assert.IsNotNull(buttonObject, $"Game object {button} is null!");

            InputField inf = buttonObject.GetComponent<InputField>();

            Assert.IsNotNull(inf, $"Button {button} is null!");

            inf.text = text;
        }
    }

    class TeaserShowTask : TestUITask
    {
        bool show;

        public TeaserShowTask(bool show, int delay) : base("",delay)
        {
            this.show = show;
        }

        public override void Do()
        {
            if(show)
            {                
                MonetizrManager.ShowTinyMenuTeaser();
            }
            else
            {
                MonetizrManager.HideTinyMenuTeaser();
            }
        }
    }
}
