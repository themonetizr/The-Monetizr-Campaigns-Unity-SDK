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
        public Sprite[] mapSprites;
        public GameObject[] items;
        public Text movesLeftText;
        public GameObject car;

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
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            currentMission = m;

            gameItems = new List<Item>(9);

            for (int i = 0; i < items.Length; i++)
            {
                int i_copy = i;
                Button _b = items[i].GetComponent<Button>();
                _b.onClick.RemoveAllListeners();
                _b.onClick.AddListener( ()=>{ OnItemClick(i_copy); });
                Animator _a = items[i].GetComponent<Animator>();

                if (i != 0)
                    _b.interactable = false;

                MemoryGameItem _gi = items[i].GetComponent<MemoryGameItem>();

                _gi.parent = this;
                _gi.id = i;
                _gi.image.sprite = backSprite;

                gameItems.Add(new Item { b = _b, go = items[i], value = 0, a = _a, gi = _gi, isOpened = false  });
            }

            movesLeftText.text = "MOVES LEFT: 4";

            //Log.PrintWarning($"{m.campaignId} {m}");
            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.MinigameScreen, m);

            //MonetizrManager.Analytics.TrackEvent("Minigame shown", m);
        }

        int amountOpened = 0;
        int phase = 0;
        bool disabledClick = false;
        int totalUnknownOpened = 0;

        int[] map = { 0, 0, 1, 2, 3, 4, 5, 6, 5 };

        List<int> []openFields = {
            new List<int>{ 1, 3 },
            new List<int>{ 2, 4 },
            new List<int>{ 5 },
            new List<int>{ 6, 4 },
            new List<int>{ 1, 3, 5, 7 },
            new List<int>{ 2, 4, 8 },
        };

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
        }

        internal override void OnOpenDone(int item)
        {
             amountOpened++;

            movesLeftText.text = $"MOVES LEFT: {4 - amountOpened}";

            //found finish
            if (map[item] == 6)
            {
                car.GetComponent<Animator>().Play("CarDrive");

                StartCoroutine(OnGameVictory());
                return;
            }

            //failed
            if(amountOpened >= 4)
            {
                amountOpened = 0;
                disabledClick = true;

                StartCoroutine(RestartGame());

                return;
            }

            //open something
            if (item < openFields.Length)
                openFields[item].ForEach((int i) => { gameItems[i].b.interactable = true; });

          

            Debug.Log("OnOpenDone" + item);
        }

        internal IEnumerator RestartGame()
        {
            yield return new WaitForSeconds(2);

            gameItems.ForEach((Item i) => {
                if (i.isOpened)
                {

                    i.a.Play("MonetizrMemoryGameTap2");
                    i.gi.middleAnimSprite = backSprite;



                    //i.gi.image.sprite = backSprite;
                    i.gi.hasEvents = false;


                }

                i.isOpened = false;

                if (i.gi.id != 0)
                    i.b.interactable = false;

                if (i.gi.id == 0)
                {
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
            yield return new WaitForSeconds(2);

            var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);

            MonetizrManager.ShowCongratsNotification(null, m);
        }

        internal override void OnCloseDone(int item)
        {
            //disabledClick = false;

            gameItems[item].isOpened = false;

            

            Debug.Log("OnCloseDone" + item);
        }




        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(AdType.MinigameScreen);
        }
    }

}