using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.UI;

namespace Monetizr.SDK.Minigames
{
    internal class MonetizrCarGame : MonetizrGameParentBase
    {
        internal class Item
        {
            public GameObject go;
            public Button b;
            public Animator a;
            public MemoryGameItem gi;
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
        public Sprite bonusSprite;
        public Sprite finishSprite;

        public Sprite[] mapSprites;
        public GameObject[] items;
        public MonetizrCar car;
        public Button closeButton;
        public Image logo;
        public Image finishImage;
        
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
            MonetizrLogger.Print("Prepare panel - car game");

            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            _bonusTaken = 0;
            car.parent = this;

            bool hasLogo = m.campaign.TryGetAsset(AssetsType.BrandRewardLogoSprite, out Sprite res);

            Sprite s;

            for (int i = 0; i < mapSprites.Length; i++)
            {
                if (m.campaign.TryGetSpriteAsset($"cargame{i}", out s))
                {
                    mapSprites[i] = s;
                }
            }

            if (m.campaign.TryGetSpriteAsset($"cargame_back", out s))
            {
                backSprite = s;
            }

            if (m.campaign.TryGetSpriteAsset($"cargame_back_disabled", out s))
            {
                backSpriteDisabled = s;
            }

            if (m.campaign.TryGetSpriteAsset($"cargame_bonus", out s))
            {
                bonusSprite = s;
            }

            if (m.campaign.TryGetSpriteAsset($"cargame_finish", out s))
            {
                finishSprite = s;
                finishImage.sprite = finishSprite;
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

                if(bonusSprite != null)
                    gi.bonusImage.sprite = bonusSprite;

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
            
            MonetizrLogger.Print("OnOpenDone" + item);
        }

        internal IEnumerator RestartGame()
        {
            MonetizrLogger.Print("RestartGame");

            yield return new WaitForSeconds(0.5f);

            _gameItems.ForEach((Item i) => {
                if (i.b.interactable)
                {
                    i.a.Play("MonetizrMemoryGameTap2");
                    i.gi.middleAnimSprite = backSpriteDisabled;
                    i.gi.isOpening = false;
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

            MonetizrLogger.Print("OnCloseDone" + item);
        }

        internal override void FinalizePanel(PanelId id)
        {

        }

    }

}