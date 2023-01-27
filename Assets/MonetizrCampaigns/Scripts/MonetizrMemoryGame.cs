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
            public double avarageTimeBetweenTaps;
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
                result.Add("avarage_time_between_taps", avarageTimeBetweenTaps.ToString());
                result.Add("time_beforefirsttap", timeBeforeFirstTap.ToString());
                result.Add("first_tap_piece", firstTapPiece.ToString());
                result.Add("is_skipped", isSkipped ? "true" : "false");

                return result;
            }

            public override string ToString()
            {
                return $"GameStartTime: {gameStartTime}, LastTapTime: {lastTapTime}, AmountOfTotalTaps: {amountOfTotalTaps}," +
                       $"AmountOfTapsOnOpenedCells: {amountOfTapsOnKnownCells}, AmountOfTapsOnDisabledCells: {amountOfTapsOnDisabledCells}, TotalTime: {totalTime}, Avarage TimeBetween Taps:" +
                        $"{avarageTimeBetweenTaps}, TimeBeforeFirstTap:{timeBeforeFirstTap}, FirstTapPiece:{firstTapPiece}," +
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
        private List<Item> gameItems;
        private Mission currentMission;
        public GameObject[] items;
        public Image minigameBackground;
        public Image logo;

        public void SendStatsEvent()
        {
            stats.isSkipped = isSkipped;
            stats.totalTime = (DateTime.Now - stats.gameStartTime).TotalSeconds;

            if (currentMission.campaignServerSettings.GetBoolParam("more_memory_stats",false))
            {
                var campaign = MonetizrManager.Instance.GetCampaign(currentMission.campaignId);

                MonetizrManager.Analytics.TrackEvent("Minigame stats", campaign, false, stats.GetDictionary());
            }
        }

        public void OnButtonClick()
        {
            //MonetizrManager.Analytics.TrackEvent("Minigame pressed", currentMission);
            //MonetizrManager.ShowRewardCenter(null);
            MonetizrManager.Analytics.TrackEvent("Minigame skipped", currentMission);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.Minigame), MonetizrManager.EventType.ButtonPressSkip);

            isSkipped = true;

            SendStatsEvent();

            SetActive(false);
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

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

            stats.gameStartTime = DateTime.Now;
            stats.lastTapTime = DateTime.Now;

            this.onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;


            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.MinigameSprite1))
            {
                mapSprites[0] = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.MinigameSprite1);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.MinigameSprite2))
            {
                mapSprites[1] = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.MinigameSprite2);
            }

            if (MonetizrManager.Instance.HasAsset(m.campaignId, AssetsType.MinigameSprite3))
            {
                mapSprites[2] = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.MinigameSprite3);
            }

            UIController.SetColorForElement(minigameBackground, m.campaignServerSettings.dictionary, "MemoryGame.bg_color2");

            //---------------

            gameItems = new List<Item>(9);

            for (int i = 0; i < items.Length; i++)
            {
                int i_copy = i;
                Button _b = items[i].GetComponent<Button>();
                _b.onClick.RemoveAllListeners();
                _b.onClick.AddListener(() => { OnItemClick(i_copy); });
                Animator _a = items[i].GetComponent<Animator>();

                MemoryGameItem _gi = items[i].GetComponent<MemoryGameItem>();

                _gi.parent = this;
                _gi.id = i;

                _gi.image.sprite = mapSprites[0];

                gameItems.Add(new Item { b = _b, go = items[i], value = 0, a = _a, gi = _gi, isOpened = false });
            }


            //Log.PrintWarning($"{m.campaignId} {m}");
            //MonetizrManager.Analytics.BeginShowAdAsset(AdType.MinigameScreen, m);

            //MonetizrManager.Analytics.TrackEvent("Minigame shown", m);

            var adType = AdType.Minigame;

            MonetizrManager.CallUserDefinedEvent(m.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);

            MonetizrManager.Analytics.BeginShowAdAsset(adType, m);

            MonetizrManager.Analytics.TrackEvent("Minigame started", currentMission);
        }

        int amountOpened = 0;
        int phase = 0;
        bool disabledClick = false;
        int totalUnknownOpened = 0;
        int correctCreated = 0;
        int numTapped = 0;
        

        internal void OnItemClick(int item)
        {
            

            stats.amountOfTotalTaps++;

            double tapTime = (DateTime.Now - stats.lastTapTime).TotalSeconds;

            stats.lastTapTime = DateTime.Now;

            if (stats.amountOfTotalTaps == 1)
            {
                stats.timeBeforeFirstTap = (stats.lastTapTime - stats.gameStartTime).TotalSeconds;
                stats.firstTapPiece = item;
                stats.avarageTimeBetweenTaps = tapTime;
            }
            else
            {
                stats.avarageTimeBetweenTaps = (stats.avarageTimeBetweenTaps + tapTime) / 2;
            }

            if (disabledClick || gameItems[item].isOpened)
                stats.amountOfTapsOnDisabledCells++;

            Debug.Log(stats.ToString());

            if (disabledClick)
                return;

            if (gameItems[item].isOpened)
                return;

            Debug.Log("click" + item);

            if (gameItems[item].value != 0)
                stats.amountOfTapsOnKnownCells++;

            if (gameItems[item].value == 0)
            {
                totalUnknownOpened++;

                gameItems[item].value = 1;

                if (totalUnknownOpened > 3 && correctCreated < 2)
                {
                    correctCreated++;
                    gameItems[item].value = 2;
                }

            }
            


            //if (gameItems[item].value == 1)
            gameItems[item].a.Play("MonetizrMemoryGameTap");
            gameItems[item].gi.middleAnimSprite = mapSprites[gameItems[item].value];
            gameItems[item].gi.hasEvents = true;
            gameItems[item].isOpened = true;
            gameItems[item].isFullyOpened = false;

            numTapped++;

            if (numTapped > 1)
                disabledClick = true;
        }

        internal override void OnOpenDone(int item)
        {
            //disabledClick = false;

            amountOpened++;

            gameItems[item].isFullyOpened = true;


            Debug.Log($"OnOpenDone {item} {gameItems[item].value}");

            if (amountOpened < 2)
                return;

            phase++;
            amountOpened = 0;

            
            int correct = 0;
            gameItems.ForEach((Item i) => { if (i.value == 2 && i.isFullyOpened) correct++; });

            //end game
            if (correct == 2)
            {
                disabledClick = true;

                gameItems.ForEach((Item i) => { if (i.value == 2 && i.isOpened) i.a.Play("MonetizrMemoryGameVictory"); });

                StartCoroutine(OnGameVictory());

                return;

            }

            numTapped = 0;
        

            disabledClick = true;
            bool hasEvents = false;
            

            foreach (var i in gameItems)
            {
                if (i.isOpened == true)
                {

                    i.a.Play("MonetizrMemoryGameTap2");
                    i.gi.middleAnimSprite = mapSprites[0];
                    i.gi.hasEvents = false;
                    i.gi.isOpening = false;
                    i.isOpened = false;
                    i.isFullyOpened = false;

                    if (!hasEvents)
                    {
                        hasEvents = true;

                        i.gi.onCloseDone = () =>
                        {
                            disabledClick = false;
                            

                        };
                    }

                }
            }
            //close opened



        }

        internal IEnumerator OnGameVictory()
        {
            yield return new WaitForSeconds(2);

            //var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            //Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);

            //isSkipped = false;
            //MonetizrManager.ShowCongratsNotification(null, m);
            isSkipped = false;

            SetActive(false);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.Minigame), MonetizrManager.EventType.ButtonPressOk);

            MonetizrManager.Analytics.TrackEvent("Minigame completed", currentMission);

            SendStatsEvent();
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