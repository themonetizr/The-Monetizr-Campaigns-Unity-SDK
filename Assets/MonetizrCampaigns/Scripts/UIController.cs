using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.Campaigns
{

    internal enum PanelId
    {
        Unknown = -1,
        StartNotification,
        RewardCenter,
        CongratsNotification,
        SurveyWebView,
        VideoWebView,
        Html5WebView,
        TinyMenuTeaser,
        SurveyNotification,
        DebugPanel,
        TwitterNotification,
        GiveawayEmailEnterNotification,
    }
    
    internal class UIController
    {       
        private GameObject mainCanvas;
        private PanelId previousPanel;


        public Dictionary<PanelId, PanelController> panels = null;
        public bool isVideoPlaying;

        public UIController()
        {
            var resCanvas = Resources.Load("MonetizrCanvas");

            Assert.IsNotNull(resCanvas);

            mainCanvas = GameObject.Instantiate<GameObject>(resCanvas as GameObject);

            GameObject.DontDestroyOnLoad(mainCanvas);

            Assert.IsNotNull(mainCanvas);

            previousPanel = PanelId.Unknown;

            panels = new Dictionary<PanelId, PanelController>();
        }
                

        /*public void PlayVideo(String path, Action<bool> onComplete)
        {
            isVideoPlaying = true;

            MonetizrManager.HideRewardCenter();

            var prefab = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrVideoPlayer") as GameObject, mainCanvas.transform);

            var player = prefab.GetComponent<MonetizrVideoPlayer>();

            player.Play(path, (bool isSkip) => {
                
                    onComplete?.Invoke(isSkip);

                    GameObject.Destroy(prefab);
                    isVideoPlaying = false;
            } );
        }*/

        internal PanelController ShowLoadingScreen()
        {
            var go = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrLoadingScreen") as GameObject, mainCanvas.transform);

            var ctrlPanel = go.GetComponent<PanelController>();

            ctrlPanel.SetActive(true);

            return ctrlPanel;      
        }

        public void ShowPanelFromPrefab(String prefab, PanelId id = PanelId.Unknown, Action<bool> onComplete = null, bool rememberPrevious = false, Mission m = null)
        {
            Log.Print($"ShowPanel: {id} Mission: {m==null}");

            //if (panels.ContainsKey(previousPanel) && previousPanel != PanelId.Unknown)
            //    panels[previousPanel].SetActive(false);
            PanelController ctrlPanel = null;
            GameObject panel = null;

            Action<bool> complete = (bool isSkipped) =>
            {
                onComplete?.Invoke(isSkipped);
                GameObject.Destroy(panel.gameObject);

                panels.Remove(id);
            };


            if (panels.ContainsKey(id))
            {
                ctrlPanel = panels[id];
                panel = ctrlPanel.gameObject;
            }
            else
            {
                panel = GameObject.Instantiate<GameObject>(Resources.Load(prefab) as GameObject, mainCanvas.transform);
                ctrlPanel = panel.GetComponent<PanelController>();

                ctrlPanel.uiController = this;

                ctrlPanel.PreparePanel(id, complete, m);

                panels.Add(id, ctrlPanel);
            }

            ctrlPanel.SetActive(true);

            if (rememberPrevious)
               previousPanel = id;

            
        }

        public void DestroyTinyMenuTeaser()
        {
            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
                return;

            MonetizrMenuTeaser teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;

            GameObject.Destroy(teaser.gameObject);

            panels.Remove(PanelId.TinyMenuTeaser);
        }

        public void ShowTinyMenuTeaser(Vector2 screenPos, Action UpdateGameUI)
        {
             MonetizrMenuTeaser teaser;

            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                var obj = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrMenuTeaser") as GameObject, mainCanvas.transform);
                teaser = obj.GetComponent<MonetizrMenuTeaser>();
                panels.Add(PanelId.TinyMenuTeaser, teaser);
                teaser.button.onClick.AddListener(() => {
                       MonetizrManager.ShowRewardCenter(UpdateGameUI);
                });

                if (screenPos != null)
                {
                    teaser.rectTransform.anchoredPosition = screenPos;
                }
            }
            else
            {
                teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;
            }

            if (teaser.IsVisible())
                return;

            teaser.PreparePanel(PanelId.TinyMenuTeaser, null, null);

            //previousPanel = PanelId.TinyMenuTeaser;

            teaser.SetActive(true);
        }
        
        public void HidePanel(PanelId id = PanelId.Unknown)
        {
            if (id == PanelId.Unknown && previousPanel != PanelId.Unknown)
                id = previousPanel;

            if(panels.ContainsKey(id))
                panels[id].SetActive(false);

        }

      
    }


}