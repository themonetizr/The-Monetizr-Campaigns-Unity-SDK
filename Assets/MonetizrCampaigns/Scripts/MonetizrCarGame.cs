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

        private List<Item> _gameItems;
        private int _bonusTaken = 0;

        private int _amountOpened = 0;
        private bool _disabledClick = false;
        private readonly int[] _map = { 0, 0, 11, 7, 8, 1, 5, 9, 10 };
        private readonly List<int>[] _openFields = {
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
        private readonly int[] _bonusIds = { 0, 4, 7 };


        public Sprite backSprite;
        public Sprite backSpriteDisabled;
        public Sprite[] mapSprites;
        public GameObject[] items;
        public MonetizrCar car;
        public Button closeButton;
        public Image logo;
        
        internal override AdPlacement? GetAdPlacement()
        {
            return AdPlacement.Minigame;
        }

        public void OnButtonClick()
        {
            isSkipped = true;

            SetActive(false);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            Log.PrintV("Prepare panel - car game");

            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            _bonusTaken = 0;
            car.parent = this;

            bool hasLogo = m.campaign.TryGetAsset(AssetsType.BrandRewardLogoSprite, out Sprite res);

            for (int i = 0; i < mapSprites.Length; i++)
            {
                if (m.campaign.TryGetSpriteAsset($"cargame{i}", out var s))
                 {
                    mapSprites[i] = s;
                }
            }

            logo.sprite = res;
            logo.gameObject.SetActive(hasLogo);
            

            _gameItems = new List<Item>(9);

            for (int i = 0; i < items.Length; i++)
            {
                int iCopy = i;
                Button b = items[i].GetComponent<Button>();
                items[i].name = $"GameItem{i}";
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener( ()=>{ OnItemClick(iCopy); });
                Animator a = items[i].GetComponent<Animator>();
                          
                if (i != 0)
                    b.interactable = false;

                MemoryGameItem gi = items[i].GetComponent<MemoryGameItem>();
                                
                gi.parent = this;
                gi.id = i;

                if(!b.interactable)
                    gi.image.sprite = backSpriteDisabled;
                else
                    gi.image.sprite = backSprite;

                _gameItems.Add(new Item { b = b, go = items[i], value = 0, a = a, gi = gi, isOpened = false  });
            }
           
        }
        
        internal void OnItemClick(int item)
        {
            if (_disabledClick)
                return;

            if (_gameItems[item].isOpened)
                return;


            _gameItems[item].a.Play("MonetizrMemoryGameTap2");
            _gameItems[item].gi.middleAnimSprite = mapSprites[_map[item]];
            _gameItems[item].gi.hasEvents = true;


            _gameItems[item].isOpened = true;

            _gameItems[item].gi.hasBonus = Array.FindIndex(_bonusIds, i => i == item) != -1;
            _gameItems[item].gi.isOpening = true;
        }

        internal void OnBonusTaken()
        {
            _gameItems[_bonusIds[_bonusTaken]].gi.PlayOnBonus("BonusDisappear");

            _bonusTaken++;
        }

        internal override void OnOpenDone(int item)
        {
            _amountOpened++;
            
            if(item == 7)
            {
                car.gameObject.GetComponent<Animator>().Play("CarDrive2");

                StartCoroutine(OnGameVictory());
                return;
            }
                        
            if (_map[item] == 5 || _map[item] == 11)
            {
                _amountOpened = 0;
                _disabledClick = true;

                StartCoroutine(RestartGame());

                return;
            }
            
            //open something
            if (item < _openFields.Length)
            {
                _openFields[item].ForEach((int i) => {
                    if (i > 0 && !_gameItems[i].isOpened)
                    {
                        _gameItems[i].b.interactable = true;
                        _gameItems[i].gi.image.sprite = backSprite;
                    }
                });
            }
            
            Log.PrintV("OnOpenDone" + item);
        }

        internal IEnumerator RestartGame()
        {
            Log.PrintV("RestartGame");


            yield return new WaitForSeconds(0.5f);

            _gameItems.ForEach((Item i) => {
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
                        _disabledClick = false;
                        _amountOpened = 0;
                    };
                }
            });
        }

        internal IEnumerator OnGameVictory()
        {
            yield return new WaitForSeconds(3);

            isSkipped = false;

            SetActive(false);
        }

        internal override void OnCloseDone(int item)
        {
            _gameItems[item].isOpened = false;

            Log.PrintV("OnCloseDone" + item);
        }




        internal override void FinalizePanel(PanelId id)
        {

        }
    }

}