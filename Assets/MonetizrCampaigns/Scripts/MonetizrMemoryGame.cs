using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class MonetizrMemoryGame : MonetizrGameParentBase
    {
        internal class Stats
        {
            public DateTime gameStartTime;
            public DateTime lastTapTime;
            public int amountOfTotalTaps;
            public int amountOfTapsOnKnownCells;
            public int amountOfTapsOnDisabledCells;
            public double totalTime;
            public double averageTimeBetweenTaps;
            public double timeBeforeFirstTap;
            public int firstTapPiece;
            public bool isSkipped;

            public Dictionary<string, string> GetDictionary()
            {
                var result = new Dictionary<string, string>();

                result.Add("amount_of_total_taps", amountOfTotalTaps.ToString());
                result.Add("amount_of_taps_on_known_cells", amountOfTapsOnKnownCells.ToString());
                result.Add("amount_of_taps_on_disabled_cells", amountOfTapsOnDisabledCells.ToString());
                result.Add("total_time", totalTime.ToString());
                result.Add("avarage_time_between_taps", averageTimeBetweenTaps.ToString());
                result.Add("time_beforefirsttap", timeBeforeFirstTap.ToString());
                result.Add("first_tap_piece", firstTapPiece.ToString());
                result.Add("is_skipped", isSkipped ? "true" : "false");

                return result;
            }

            public override string ToString()
            {
                return $"GameStartTime: {gameStartTime}, LastTapTime: {lastTapTime}, AmountOfTotalTaps: {amountOfTotalTaps}," +
                       $"AmountOfTapsOnOpenedCells: {amountOfTapsOnKnownCells}, AmountOfTapsOnDisabledCells: {amountOfTapsOnDisabledCells}, TotalTime: {totalTime}, Avarage TimeBetween Taps:" +
                        $"{averageTimeBetweenTaps}, TimeBeforeFirstTap:{timeBeforeFirstTap}, FirstTapPiece:{firstTapPiece}," +
                        $"IsSkipped:{isSkipped}";
            }
        }

        internal class Item
        {
            public GameObject go;
            public Button b;
            public Animator a;
            public MemoryGameItem gi;

            //0 - undefined, 1 - empty, 2 - item
            public int value;
            internal bool isOpened;
            internal bool isFullyOpened;
        }

        public Stats stats = new Stats();

        public Sprite[] mapSprites;
        private List<Item> _gameItems;
        public GameObject[] items;
        public Image minigameBackground;
        public Image logo;

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
                MonetizrManager.Analytics._TrackEvent("Minigame stats", currentMission.campaign, false, stats.GetDictionary());
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

            //---------------

            _gameItems = new List<Item>(9);

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

                _gameItems.Add(new Item { b = b, go = items[i], value = 0, a = a, gi = gi, isOpened = false });
            }
        }

        private int _amountOpened = 0;
        private int _phase = 0;
        private bool _disabledClick = false;
        private int _totalUnknownOpened = 0;
        private int _correctCreated = 0;
        private int _numTapped = 0;
        
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

            Log.PrintV(stats.ToString());

            if (_disabledClick)
                return;

            if (_gameItems[item].isOpened)
                return;

            Log.PrintV("click" + item);

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

            //if (gameItems[item].value == 1)
            _gameItems[item].a.Play("MonetizrMemoryGameTap");
            _gameItems[item].gi.middleAnimSprite = mapSprites[_gameItems[item].value];
            _gameItems[item].gi.hasEvents = true;
            _gameItems[item].isOpened = true;
            _gameItems[item].isFullyOpened = false;

            _numTapped++;

            if (_numTapped > 1)
                _disabledClick = true;
        }

        internal override void OnOpenDone(int item)
        {
            _amountOpened++;

            _gameItems[item].isFullyOpened = true;
            
            Log.PrintV($"OnOpenDone {item} {_gameItems[item].value}");

            if (_amountOpened < 2)
                return;

            _phase++;
            _amountOpened = 0;
            
            int correct = 0;
            _gameItems.ForEach((Item i) => { if (i.value == 2 && i.isFullyOpened) correct++; });

            //end game
            if (correct == 2)
            {
                _disabledClick = true;

                _gameItems.ForEach((Item i) => { if (i.value == 2 && i.isOpened) i.a.Play("MonetizrMemoryGameVictory"); });

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
            
            Log.Print("OnCloseDone" + item);
        }
        
        internal override void FinalizePanel(PanelId id)
        {
     
        }
    }

}