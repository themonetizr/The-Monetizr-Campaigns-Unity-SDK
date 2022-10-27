using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class MonetizrMemoryGame : MonetizrGameParentBase
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

       

        public GameObject[] items;


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

                MemoryGameItem _gi = items[i].GetComponent<MemoryGameItem>();

                _gi.parent = this;
                _gi.id = i;

                gameItems.Add(new Item { b = _b, go = items[i], value = 0, a = _a, gi = _gi, isOpened = false  });
            }
            

            //Log.PrintWarning($"{m.campaignId} {m}");
            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.MinigameScreen, m);

            //MonetizrManager.Analytics.TrackEvent("Minigame shown", m);
        }

        int amountOpened = 0;
        int phase = 0;
        bool disabledClick = false;
        int totalUnknownOpened = 0;

        internal void OnItemClick(int item)
        {
            if (disabledClick)
                return;

            if (gameItems[item].isOpened)
                return;

            Debug.Log("click" + item);

            if (gameItems[item].value == 0)
            {
                totalUnknownOpened++;

                gameItems[item].value = 1;

                if (totalUnknownOpened > 3)
                    gameItems[item].value = 2;
                                    
            }
           
            if (gameItems[item].value == 1)
                gameItems[item].a.Play("MonetizrMemoryGameTap");
            else
                gameItems[item].a.Play("MonetizrMemoryGameTapCorrect");

            gameItems[item].isOpened = true;
        }

        internal override void OnOpenDone(int item)
        {
            //disabledClick = false;

            

            amountOpened++;

            if (amountOpened == 2)
            {
                
                phase++;
                amountOpened = 0;

                int correct = 0;

                gameItems.ForEach((Item i) => { if (i.value == 2) correct++; });

                //end game
                if (correct == 2)
                {
                    disabledClick = true;

                    gameItems.ForEach((Item i) => { if (i.value == 2) i.a.Play("MonetizrMemoryGameVictory"); });

                    StartCoroutine(OnGameVictory());

                    return;
                }



                foreach (var i in gameItems)
                {
                    if (i.isOpened == true)
                    {
                        if (i.value == 1)
                            i.a.Play("MonetizrMemoryGameTapBack");
                        else
                            i.a.Play("MonetizrMemoryGameTapBackCorrect");
                    }
                }
                //close opened
            }

            Debug.Log("OnOpenDone" + item);
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
            MonetizrManager.Analytics.EndShowAdAsset(AdType.Minigame);
        }
    }

}