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
    internal class MonetizrMemoryGame : MonetizrGameParentBase
    {
        public GameStats stats = new GameStats();
        public Sprite[] mapSprites;
        private List<GameItem> _gameItems;
        public GameObject[] items;
        public Image minigameBackground;
        public Image logo;

        private int _amountOpened = 0;
        private int _phase = 0;
        private bool _disabledClick = false;
        private int _totalUnknownOpened = 0;
        private int _correctCreated = 0;
        private int _numTapped = 0;

        internal override AdPlacement? GetAdPlacement()
        {
            return AdPlacement.Minigame;
        }

        public void SendStatsEvent()
        {
            stats.isSkipped = isSkipped;
            stats.totalTime = (DateTime.Now - stats.gameStartTime).TotalSeconds;

            if (currentMission.campaignServerSettings.GetBoolParam("more_memory_stats", false))
            {
                MonetizrInstance.Instance.Analytics._TrackEvent("Minigame stats", currentMission.campaign, false, stats.GetDictionary());
            }
        }

        public void OnButtonClick()
        {
            isSkipped = true;

            SendStatsEvent();

            SetActive(false);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            bool hasLogo = m.campaign.TryGetAsset(AssetsType.BrandRewardLogoSprite, out Sprite res);

            logo.sprite = res;
            logo.gameObject.SetActive(hasLogo);
            
            stats.gameStartTime = DateTime.Now;
            stats.lastTapTime = DateTime.Now;

            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            AssetsType[] minigameSprites =
            {
                AssetsType.MinigameSprite1,
                AssetsType.MinigameSprite2,
                AssetsType.MinigameSprite3
            };

            for (int i = 0; i < minigameSprites.Length; i++)
            {
                if (m.campaign.TryGetAsset<Sprite>(minigameSprites[i], out Sprite res2))
                    mapSprites[i] = res2;
            }

            UIController.SetColorForElement(minigameBackground, m.campaignServerSettings, "MemoryGame.bg_color2");

            _gameItems = new List<GameItem>(9);

            for (int i = 0; i < items.Length; i++)
            {
                int iCopy = i;
                items[i].name = $"GameItem{i}";
                Button b = items[i].GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => { OnItemClick(iCopy); });
                Animator a = items[i].GetComponent<Animator>();

                MemoryGameItem gi = items[i].GetComponent<MemoryGameItem>();

                gi.parent = this;
                gi.id = i;

                gi.image.sprite = mapSprites[0];

                _gameItems.Add(new GameItem { b = b, go = items[i], value = 0, a = a, gi = gi, isOpened = false });
            }
        }
        
        internal void OnItemClick(int item)
        {
            stats.amountOfTotalTaps++;
            double tapTime = (DateTime.Now - stats.lastTapTime).TotalSeconds;
            stats.lastTapTime = DateTime.Now;

            if (stats.amountOfTotalTaps == 1)
            {
                stats.timeBeforeFirstTap = (stats.lastTapTime - stats.gameStartTime).TotalSeconds;
                stats.firstTapPiece = item;
                stats.averageTimeBetweenTaps = tapTime;
            }
            else
            {
                stats.averageTimeBetweenTaps = (stats.averageTimeBetweenTaps + tapTime) / 2;
            }

            if (_disabledClick || _gameItems[item].isOpened)
                stats.amountOfTapsOnDisabledCells++;

            MonetizrLogger.Print(stats.ToString());

            if (_disabledClick)
                return;

            if (_gameItems[item].isOpened)
                return;

            MonetizrLogger.Print("click" + item);

            if (_gameItems[item].value != 0)
                stats.amountOfTapsOnKnownCells++;

            if (_gameItems[item].value == 0)
            {
                _totalUnknownOpened++;

                _gameItems[item].value = 1;

                if (_totalUnknownOpened > 3 && _correctCreated < 2)
                {
                    _correctCreated++;
                    _gameItems[item].value = 2;
                }

            }

            _gameItems[item].a.Play("MonetizrMemoryGameTap");
            _gameItems[item].gi.middleAnimSprite = mapSprites[_gameItems[item].value];
            _gameItems[item].gi.hasEvents = true;
            _gameItems[item].isOpened = true;
            _gameItems[item].isFullyOpened = false;

            _numTapped++;

            if (_numTapped > 1) _disabledClick = true;
        }

        internal override void OnOpenDone(int item)
        {
            _amountOpened++;

            _gameItems[item].isFullyOpened = true;
            
            MonetizrLogger.Print($"OnOpenDone {item} {_gameItems[item].value}");

            if (_amountOpened < 2)
                return;

            _phase++;
            _amountOpened = 0;
            
            int correct = 0;
            _gameItems.ForEach((GameItem i) => { if (i.value == 2 && i.isFullyOpened) correct++; });

            if (correct == 2)
            {
                _disabledClick = true;
                _gameItems.ForEach((GameItem i) => { if (i.value == 2 && i.isOpened) i.a.Play("MonetizrMemoryGameVictory"); });
                StartCoroutine(OnGameVictory());
                return;
            }

            _numTapped = 0;
            _disabledClick = true;
            bool hasEvents = false;
            
            foreach (var i in _gameItems)
            {
                if (i.isOpened != true) continue;

                i.a.Play("MonetizrMemoryGameTap2");
                i.gi.middleAnimSprite = mapSprites[0];
                i.gi.hasEvents = false;
                i.gi.isOpening = false;
                i.isOpened = false;
                i.isFullyOpened = false;

                if (hasEvents) continue;

                hasEvents = true;

                i.gi.onCloseDone = () =>
                {
                    _disabledClick = false;
                };
            }
        }

        internal IEnumerator OnGameVictory()
        {
            yield return new WaitForSeconds(2);
            isSkipped = false;
            SetActive(false);
            SendStatsEvent();
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