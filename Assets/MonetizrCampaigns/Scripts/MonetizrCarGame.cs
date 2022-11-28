using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal abstract class MonetizrGameParentBase : PanelController
    {
        internal abstract void OnOpenDone(int id);
        internal abstract void OnCloseDone(int id);
    }

    internal class MonetizrCarGame : MonetizrGameParentBase
    {
        internal class Item
        {
            public GameObject go;
            public Button b;
            public Animator a;
            public MemoryGameItem gi;

            //0 - undefined, 1 - empty, 2 - item
            public int value;
            internal bool isOpened;
        }

        private List<Item> gameItems;
        private Mission currentMission;

        public Sprite backSprite;
        public Sprite backSpriteDisabled;
        public Sprite[] mapSprites;
        public GameObject[] items;
        public Text movesLeftText;
        public MonetizrCar car;
        public Button closeButton;
        public Image logo;

        private int bonusTaken = 0;

        void Update()
        {
            

        }

        void SetProgress(float a)
        {
            //uvRect.y = 0.5f * (1.0f - Tween(a));
            //teaserImage.uvRect = uvRect;
        }

        float Tween(float k)
        {
            return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
        }

        public void OnButtonClick()
        {
            //MonetizrManager.Analytics.TrackEvent("Minigame pressed", currentMission);
            //MonetizrManager.ShowRewardCenter(null);
            MonetizrManager.Analytics.TrackEvent("Minigame skipped", currentMission);


            isSkipped = true;

            SetActive(false);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            Debug.Log("Prepare panel - car game");

            this.onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            bonusTaken = 0;

            Debug.Log($"1 {car}");

            car.parent = this;

            Debug.Log($"2 {logo} {logo.sprite}");

            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;

            Debug.Log($"3 {logo.gameObject}");

            logo.gameObject.SetActive(logo.sprite != null);

            

            gameItems = new List<Item>(9);

            Debug.Log($"4 {gameItems}");

            for (int i = 0; i < items.Length; i++)
            {
                Debug.Log($"5 item {i}");

                int i_copy = i;
                Button _b = items[i].GetComponent<Button>();

                Debug.Log($"6 item {_b}");

                _b.onClick.RemoveAllListeners();
                _b.onClick.AddListener( ()=>{ OnItemClick(i_copy); });
                Animator _a = items[i].GetComponent<Animator>();

                Debug.Log($"6 item {_a}");

                if (i != 0)
                    _b.interactable = false;

                MemoryGameItem _gi = items[i].GetComponent<MemoryGameItem>();

                Debug.Log($"7 item {_gi} {_gi.image}");

                _gi.parent = this;
                _gi.id = i;

                if(!_b.interactable)
                    _gi.image.sprite = backSpriteDisabled;
                else
                    _gi.image.sprite = backSprite;

                gameItems.Add(new Item { b = _b, go = items[i], value = 0, a = _a, gi = _gi, isOpened = false  });
            }

            Debug.Log("5");

            movesLeftText.text = "MOVES LEFT: 4";

            //Log.PrintWarning($"{m.campaignId} {m}");
            var adType = AdType.Minigame;

            MonetizrManager.CallUserDefinedEvent(m.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);

            MonetizrManager.Analytics.BeginShowAdAsset(adType, m);

            MonetizrManager.Analytics.TrackEvent("Minigame started", currentMission);

            //MonetizrManager.Analytics.TrackEvent("Minigame shown", m);
        }

        int amountOpened = 0;
        int phase = 0;
        bool disabledClick = false;
        int totalUnknownOpened = 0;

        //int[] map = { 0, 0, 1, 2, 3, 4, 5, 6, 5 };

        int[] map = { 0, 0, 11, 7, 8, 1, 5, 9, 10 };

        /*List<int> []openFields = {
            new List<int>{ 1, 3 },
            new List<int>{ 2, 4 },
            new List<int>{ 5 },
            new List<int>{ 6, 4 },
            new List<int>{ 1, 3, 5, 7 },
            new List<int>{ 2, 4, 8 },
        };*/

        List<int>[] openFields = {
            new List<int>{ 1, 3 },
            new List<int>{ 2, 4 },
            new List<int>{ -1 },
            new List<int>{ 6 },
            new List<int>{ 5 },
            new List<int>{ 4, 8 },
            new List<int>{ -1 },
            new List<int>{ -1 },
            new List<int>{ 7 },
        };


       
        int[] bonusIds = { 0, 4, 7 };

        internal void OnItemClick(int item)
        {
            if (disabledClick)
                return;

            if (gameItems[item].isOpened)
                return;


            gameItems[item].a.Play("MonetizrMemoryGameTap2");
            gameItems[item].gi.middleAnimSprite = mapSprites[map[item]];
            gameItems[item].gi.hasEvents = true;


            gameItems[item].isOpened = true;

            gameItems[item].gi.hasBonus = Array.FindIndex(bonusIds, i => i == item) != -1;
            gameItems[item].gi.isOpening = true;
        }

        internal void OnBonusTaken()
        {
            gameItems[bonusIds[bonusTaken]].gi.PlayOnBonus("BonusDisappear");

            bonusTaken++;
        }

        internal override void OnOpenDone(int item)
        {
            amountOpened++;

            movesLeftText.text = $"MOVES LEFT: {4 - amountOpened}";

            //found finish
            //if (map[item] == 6)
            if(item == 7)
            {
                car.gameObject.GetComponent<Animator>().Play("CarDrive2");

                StartCoroutine(OnGameVictory());
                return;
            }

            //failed
            //if(false)
            //*
            if (map[item] == 5 || map[item] == 11)
            //if(amountOpened >= 4)
            {
                amountOpened = 0;
                disabledClick = true;

                StartCoroutine(RestartGame());

                return;
            }
            //*/

            //open something
            if (item < openFields.Length)
            {
                openFields[item].ForEach((int i) => {
                    if (i > 0 && !gameItems[i].isOpened)
                    {
                        gameItems[i].b.interactable = true;
                        gameItems[i].gi.image.sprite = backSprite;
                    }
                });
            }

          

            Debug.Log("OnOpenDone" + item);
        }

        internal IEnumerator RestartGame()
        {
            Debug.Log("RestartGame");


            yield return new WaitForSeconds(0.5f);

            gameItems.ForEach((Item i) => {
                if (i.b.interactable)
                {

                    i.a.Play("MonetizrMemoryGameTap2");
                    i.gi.middleAnimSprite = backSpriteDisabled;

                    i.gi.isOpening = false;

                    //i.gi.image.sprite = backSpriteDisabled;
                    i.gi.hasEvents = false;


                }

                i.isOpened = false;

                if (i.gi.id != 0)
                    i.b.interactable = false;

                if (i.gi.id == 0)
                {
                    i.gi.middleAnimSprite = backSprite;

                    i.gi.onCloseDone = () =>
                    {
                        disabledClick = false;
                        amountOpened = 0;
                    };
                }
            });
        }

        internal IEnumerator OnGameVictory()
        {
            yield return new WaitForSeconds(3);

            //var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            //Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);

            isSkipped = false;

            SetActive(false);

            MonetizrManager.Analytics.TrackEvent("Minigame completed", currentMission);

            //MonetizrManager.ShowCongratsNotification(null, m);
        }

        internal override void OnCloseDone(int item)
        {
            //disabledClick = false;

            gameItems[item].isOpened = false;

            

            Debug.Log("OnCloseDone" + item);
        }




        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(AdType.Minigame);
        }
    }

}