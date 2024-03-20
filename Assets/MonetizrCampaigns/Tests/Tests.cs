using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Monetizr.SDK;
using Monetizr.SDK.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Monetizr.Campaigns;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using Monetizr.SDK.Core;

namespace Monetizr.Tests
{
    public class Tests
    {
        static int loadSuccess = 0;
        Dictionary<string, int> eventsAmount = new Dictionary<string, int>();
    
    
        [SetUp]
        public void Setup()
        {
            var go = new GameObject("Default Camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.transform.SetAsFirstSibling();

            PlayerPrefs.SetString("campaigns", "");
            PlayerPrefs.SetString("missions", "");

            Sprite img = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            Time.timeScale = 5;

            MonetizrManager.bundleId = "com.monetizr.sample";
            
            eventsAmount.Clear();

            MonetizrManager.SetAdvertisingIds("", false);

            MonetizrManager.SetGameCoinAsset(RewardType.Coins, img, "Coins",
                () => { return 0; },
                (ulong reward) => { },
                10000);

            MonetizrManager.InitializeForTests("t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo", null, () =>
                {
                    Assert.IsTrue(MonetizrManager.IsActiveAndEnabled(), "Campaign should have active campaigns");

                    if (!MonetizrManager.IsActiveAndEnabled()) return;

                    MonetizrManager.claimForSkippedCampaigns = false;

                    MonetizrManager.ShowTinyMenuTeaser();
                    //Do something

                    loadSuccess = 1;
                }, null, null,
                (string campaignId, string placement, MonetizrManager.EventType eventType) =>
                {
                    if (eventType != MonetizrManager.EventType.Impression) return;

                    if (eventsAmount.ContainsKey(placement))
                        eventsAmount[placement]++;
                    else
                        eventsAmount[placement] = 1;
                }, new MonetizrTestClient());
        }

        [TearDown]
        public void Teardown()
        {

        }

        static GameObject FindObjectByName(string name)
        {
            GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject),true);

            foreach (var t in gos)
                if (t.name.Contains(name))
                    return t;

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
                //new ClickTask("NotifyCloseButton",2),
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

                new ClickTask("RewardCenterButtonClaim4",2),
                new InputFieldTask("PanelInputField","artem+100@themonetizr.com",2),
                new ClickTask("PanelOkButton",2),
                new ClickTask("NotifyCloseButton",2),

                new TestVisibility("MonetizrRewardedItem4",false,2)
                //new ClickTask("NotifyCloseButton",2),
            };

            yield return new TasksManager().Run(tasks);

            //yield return null;
        }

        [UnityTest]
        public IEnumerator TestMemoryGame()
        {
            List<int> b = new List<int>{ 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            MonetizrUtils.ShuffleList(b);

            var tasks = new List<TestUITask>() {

                new ClickTask("MonetizrMenuTeaser",2),

                new ClickTask("RewardCenterButtonClaim1",2),

                new ClickTask("PanelCloseButton",2),
                //new ClickTask("MessageCloseButton",2),

                new TestVisibility("MonetizrRewardedItem1",true,2),

                new ClickTask("RewardCenterButtonClaim1",2),

                new ClickTask($"GameItem{b[0]}",2),
                new ClickTask($"GameItem{b[1]}",2),
                new ClickTask($"GameItem{b[2]}",2),
                new ClickTask($"GameItem{b[3]}",2),
                new ClickTask($"GameItem{b[3]}",2),
                new ClickTask($"GameItem{b[4]}",5),

                new ClickTask("NotifyCloseButton",2),

                new TestVisibility("MonetizrRewardedItem1",false,2)
                //new ClickTask("NotifyCloseButton",2),
            };

            yield return new TasksManager().Run(tasks);

            //yield return null;
        }

        [UnityTest]
        public IEnumerator TestSurvey()
        {
            var tasks = new List<TestUITask>() {

                new ClickTask("MonetizrMenuTeaser",2),

                new ClickTask("RewardCenterButtonClaim0",2),

                /*new ClickTask("PanelCloseButton",2),
            new ClickTask("MessageCloseButton",2),

            new TestVisibility("MonetizrRewardedItem0",true,2),

            new ClickTask("RewardCenterButtonClaim0",2),*/

                //page1
                new ClickTask($"NextButton",2),

                //page2
                new ToggleTask($"Q:1:A:1",2),

                //page3
                new ToggleTask($"Q:2:A:1",2),
                new ToggleTask($"Q:2:A:2",2),
                new ClickTask($"NextButton",2),

                //page4
                new ToggleTask($"Q:3:A:2",2),

                //page5
                new InputFieldTask("SurveyInputField","test!!!",4),
                new ClickTask($"NextButton",4),

                //page6
                new ClickTask($"NextButton",2),

                new ClickTask("NotifyCloseButton",2),

                new TestVisibility("MonetizrRewardedItem0",false,2)
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

        class ToggleTask : TestUITask
        {
            public ToggleTask(string button, int delay) : base(button, delay)
            {

            }

            public override void Do()
            {
                var buttonObject = Tests.FindObjectByName(button);

                Assert.IsNotNull(buttonObject, $"Game object {button} is null!");

                Toggle b = buttonObject.GetComponent<Toggle>();

                Assert.IsTrue(b.interactable, $"Button {button} is not active!");

                Assert.IsNotNull(b, $"Button {button} is null!");

                //b.onClick.Invoke();
                b.isOn = true;
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

        class TestVisibility : TestUITask
        {
            bool visibility;

            public TestVisibility(string button, bool visible, int delay) : base(button, delay)
            {
                this.visibility = visible;
            }

            public override void Do()
            {
                var buttonObject = Tests.FindObjectByName(button);

                Assert.IsNotNull(buttonObject, $"Game object {button} is null!");
                        
                Assert.IsTrue(buttonObject.activeSelf == visibility, $"Object {button} is wrongly active. Should be {visibility}!");
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
}