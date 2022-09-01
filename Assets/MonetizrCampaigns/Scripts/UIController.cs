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
            //Debug.LogWarning($"ShowPanel: {id} Mission: {m==null}");

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
                int uiVersion = 0;

                if (m != null)
                {
                    //var campaign = MonetizrManager.Instance.GetCampaign(m.campaignId);

                    uiVersion = m.additionalParams.GetIntParam("design_version");

                    //Debug.Log($"-------------{uiVersion}");

                    if (uiVersion == 2 && id != PanelId.DebugPanel)
                    {
                        prefab += "2";
                    }
                }

                GameObject asset = Resources.Load(prefab) as GameObject;

                panel = GameObject.Instantiate<GameObject>(asset, mainCanvas.transform);
                ctrlPanel = panel.GetComponent<PanelController>();


                if(id != PanelId.DebugPanel)
                PrepareCustomColors(ctrlPanel.backgroundImage,
                    ctrlPanel.backgroundBorderImage,
                    m.additionalParams.dictionary,
                    id);



                ctrlPanel.uiController = this;
                ctrlPanel.uiVersion = uiVersion;

                foreach (var t in ctrlPanel.gameObject.GetComponents<PanelTextItem>())
                    t.InitializeByParent(id, m);

                ctrlPanel.PreparePanel(id, complete, m);
                               

                panels.Add(id, ctrlPanel);
            }

            ctrlPanel.SetActive(true);

            if (rememberPrevious)
               previousPanel = id;

            
        }

        internal static void SetColorForElement(Graphic i, Dictionary<string, string> additionalParams, string param)
        {
            Color c;
            if (additionalParams.ContainsKey(param) && ColorUtility.TryParseHtmlString(additionalParams[param], out c))
            {
                if (i != null)
                    i.color = c;
            }
        }

        internal static void PrepareCustomColors(
            Image background,
            Image border,
            Dictionary<string,string> additionalParams,
            PanelId id)
        {
            //Debug.Log($"--------{id.ToString()}");

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

        public void ShowTinyMenuTeaser(Vector2 screenPos, Action UpdateGameUI, int designVersion, ServerCampaign campaign)
        {
             MonetizrMenuTeaser teaser;     

            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                string teaserPrefab = "MonetizrMenuTeaser";

                if (designVersion == 2)
                {
                    teaserPrefab = "MonetizrMenuTeaser2";
                    //screenPos = Vector2.zero;
                }


                var obj = GameObject.Instantiate<GameObject>(Resources.Load(teaserPrefab) as GameObject, mainCanvas.transform);
                teaser = obj.GetComponent<MonetizrMenuTeaser>();

                PrepareCustomColors(teaser.backgroundImage, teaser.backgroundBorderImage, campaign.additional_params, PanelId.TinyMenuTeaser);

                teaser.uiVersion = designVersion;
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

            var challengeId = MonetizrManager.Instance.GetActiveCampaign();

            Mission m = MonetizrManager.Instance.missionsManager.GetMission(challengeId);

            foreach (var t in teaser.gameObject.GetComponents<PanelTextItem>())
                t.InitializeByParent(PanelId.TinyMenuTeaser, m);

            teaser.SetActive(true);

            teaser.PreparePanel(PanelId.TinyMenuTeaser, null, m);


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