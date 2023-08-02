using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

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
        HtmlWebPageView,
        TinyMenuTeaser,
        SurveyNotification,
        DebugPanel,
        TwitterNotification,
        GiveawayEmailEnterNotification,
        BadEmailMessageNotification,
        EmailEnterCloseConfirmation,
        CarMemoryGame,
        MemoryGame,
        SurveyCloseConfirmation,
        SurveyUnityView,
        ActionHtmlPanelView,
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
                

        /*public void PlayVideo(String path, Action<bool> _onComplete)
        {
            isVideoPlaying = true;

            MonetizrManager.HideRewardCenter();

            var prefab = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrVideoPlayer") as GameObject, mainCanvas.transform);

            var player = prefab.GetComponent<MonetizrVideoPlayer>();

            player.Play(path, (bool isSkip) => {
                
                    _onComplete?.Invoke(isSkip);

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
            PanelController ctrlPanel = null;
            GameObject panel = null;

            Action<bool> complete = (bool isSkipped) =>
            {                                
                panels.Remove(id);

                onComplete?.Invoke(isSkipped);

                //TODO: track end of placement!!!!

                GameObject.Destroy(panel.gameObject);

                //nothing, but teaser
                if (panels.Count == 1 && panels.ContainsKey(PanelId.TinyMenuTeaser))
                {
                    MonetizrManager.Instance?.onUIVisible?.Invoke(false);
                }
            };

            //nothing or only teaser
            if ((panels.Count == 1 && panels.ContainsKey(PanelId.TinyMenuTeaser)) || panels.Count == 0)
                MonetizrManager.Instance?.onUIVisible?.Invoke(true);

            if (panels.ContainsKey(id))
            {
                ctrlPanel = panels[id];
                panel = ctrlPanel.gameObject;
            }
            else
            {
                string prefabLandscape = prefab + "_landscape";
                GameObject asset = null;
                             
                if (Utils.isInLandscapeMode())
                {       
                    //Log.Print("Loading landscape");
                    asset = Resources.Load(prefabLandscape) as GameObject;
                }

                if (asset == null)
                {
                    //Log.Print("Loading regular prefab");
                    asset = Resources.Load(prefab) as GameObject;
                }

                panel = GameObject.Instantiate<GameObject>(asset, mainCanvas.transform);
                ctrlPanel = panel.GetComponent<PanelController>();


                if(id != PanelId.DebugPanel && m != null)
                    PrepareCustomColors(ctrlPanel.backgroundImage,
                    ctrlPanel.backgroundBorderImage,
                    m.campaignServerSettings.dictionary,
                    id);
                
                ctrlPanel.uiController = this;
                ctrlPanel.uiVersion = 0;

                foreach (var t in ctrlPanel.gameObject.GetComponents<PanelTextItem>())
                    t.InitializeByParent(id, m);

                ctrlPanel.PreparePanel(id, complete, m);

                MonetizrManager.Analytics.TrackEvent(m, ctrlPanel, MonetizrManager.EventType.Impression);

                panels.Add(id, ctrlPanel);
            }

            ctrlPanel.transform.SetAsLastSibling();

            ctrlPanel.SetActive(true);

            if (rememberPrevious)
               previousPanel = id;

            
        }

        internal static void SetColorForElement(Graphic i, Dictionary<string, string> additionalParams, string param)
        {
            if (i == null || additionalParams.Count == 0)
                return;

            if (additionalParams.ContainsKey(param) && ColorUtility.TryParseHtmlString(additionalParams[param], out var c))
            {
                i.color = c;
            }
        }

        internal static void PrepareCustomColors(
            Image background,
            Image border,
            Dictionary<string,string> additionalParams,
            PanelId id)
        {
            //Log.Print($"--------{id.ToString()}");

            SetColorForElement(background, additionalParams, "bg_color");
            SetColorForElement(border, additionalParams, "bg_border_color");

            SetColorForElement(background, additionalParams, $"{id.ToString()}.bg_color");
            SetColorForElement(border, additionalParams, $"{id.ToString()}.bg_border_color");
        }

        public void DestroyTinyMenuTeaser()
        {
            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
                return;

            MonetizrMenuTeaser teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;

            GameObject.Destroy(teaser.gameObject);

            panels.Remove(PanelId.TinyMenuTeaser);
        }

        public void ShowTinyMenuTeaser(Transform root, Vector2? screenPos, Action UpdateGameUI, int designVersion, ServerCampaign campaign)
        {
             MonetizrMenuTeaser teaser;     

            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                string teaserPrefab = "MonetizrMenuTeaser2";

                /*if (designVersion == 2)
                {
                    teaserPrefab = "MonetizrMenuTeaser2";
                    //screenPos = Vector2.zero;
                }*/

                if (designVersion >= 3)
                {
                    teaserPrefab = $"MonetizrMenuTeaser{designVersion}";
                    //screenPos = Vector2.zero;
                }

                               
                var obj = GameObject.Instantiate<GameObject>(Resources.Load(teaserPrefab) as GameObject,
                    root != null ? root : mainCanvas.transform);

                teaser = obj.GetComponent<MonetizrMenuTeaser>();

                PrepareCustomColors(teaser.backgroundImage, teaser.backgroundBorderImage, campaign.serverSettings.dictionary, PanelId.TinyMenuTeaser);

                teaser.uiVersion = designVersion;
                teaser.triggersButtonEventsOnDeactivate = false;

                panels.Add(PanelId.TinyMenuTeaser, teaser);
                  
            }
            else
            {
                teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;
            }

            if (teaser.IsVisible())
                return;

            
            //var campaign = MonetizrManager.Instance.GetActiveCampaign();

            //Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);
            var missionsList = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(campaign,false);

            var m = missionsList[0];

            foreach (var t in teaser.gameObject.GetComponents<PanelTextItem>())
                t.InitializeByParent(PanelId.TinyMenuTeaser, m);

            teaser.SetActive(true);

            teaser.PreparePanel(PanelId.TinyMenuTeaser, null, m);

            MonetizrManager.Analytics.TrackEvent(m, teaser, MonetizrManager.EventType.Impression);

            //if teaser attached is not attached to user defined root
            if(root == null)
                teaser.rectTransform.SetAsFirstSibling();

            if (screenPos != null)
            {
                teaser.rectTransform.anchoredPosition = screenPos.Value;
            }

            //previousPanel = PanelId.TinyMenuTeaser;


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