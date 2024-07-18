using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using Monetizr.SDK.New;
using Monetizr.SDK.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
{
    internal class UIController
    {
        private GameObject rootUIObject;
        private Canvas mainCanvas;
        private PanelId previousPanel;
        private Dictionary<PanelId, PanelController> panels = null;

        internal UIController()
        {
            var resCanvas = Resources.Load("MonetizrCanvas");

            Assert.IsNotNull(resCanvas);

            rootUIObject = GameObject.Instantiate<GameObject>(resCanvas as GameObject);

            mainCanvas = rootUIObject.GetComponent<Canvas>();

            GameObject.DontDestroyOnLoad(rootUIObject);

            Assert.IsNotNull(rootUIObject);

            previousPanel = PanelId.Unknown;

            panels = new Dictionary<PanelId, PanelController>();
        }

        internal Canvas GetMainCanvas()
        {
            return mainCanvas;
        }

        internal bool HasActivePanel(PanelId panel)
        {
            return panels.ContainsKey(panel);
        }

        internal PanelController ShowLoadingScreen()
        {
            var go = GameObject.Instantiate<GameObject>(Resources.Load("MonetizrLoadingScreen") as GameObject, rootUIObject.transform);

            var ctrlPanel = go.GetComponent<PanelController>();

            ctrlPanel.SetActive(true);

            return ctrlPanel;      
        }

        internal void ShowPanelFromPrefab(String prefab, PanelId id = PanelId.Unknown, Action<bool> onComplete = null, bool rememberPrevious = false, Mission m = null)
        {
            PanelController ctrlPanel = null;
            GameObject panel = null;

            Action<bool> complete = (bool isSkipped) =>
            {                                
                panels.Remove(id);
                onComplete?.Invoke(isSkipped);
                GameObject.Destroy(panel.gameObject);

                if (panels.Count == 1 && panels.ContainsKey(PanelId.TinyMenuTeaser))
                {
                    MonetizrManager.Instance?.onUIVisible?.Invoke(false);
                }
            };

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
                             
                if (New_MobileUtils.IsInLandscapeMode())
                {       
                    asset = Resources.Load(prefabLandscape) as GameObject;
                }

                if (asset == null)
                {
                    asset = Resources.Load(prefab) as GameObject;
                }

                panel = GameObject.Instantiate<GameObject>(asset, rootUIObject.transform);
                ctrlPanel = panel.GetComponent<PanelController>();


                if(id != PanelId.DebugPanel && m != null)
                    PrepareCustomColors(ctrlPanel.backgroundImage,
                    ctrlPanel.backgroundBorderImage,
                    m.campaignServerSettings,
                    id);
                
                ctrlPanel.uiController = this;
                ctrlPanel.uiVersion = 0;

                foreach (var t in ctrlPanel.gameObject.GetComponents<PanelTextItem>())
                    t.InitializeByParent(id, m);

                ctrlPanel.PreparePanel(id, complete, m);

                if(!ctrlPanel.SendImpressionEventManually())
                    MonetizrManager.Analytics.TrackEvent(m, ctrlPanel, MonetizrManager.EventType.Impression);

                panels.Add(id, ctrlPanel);
            }

            ctrlPanel.transform.SetAsLastSibling();
            ctrlPanel.SetActive(true);

            if (rememberPrevious) previousPanel = id;
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
            SetColorForElement(background, additionalParams, "bg_color");
            SetColorForElement(border, additionalParams, "bg_border_color");
            SetColorForElement(background, additionalParams, $"{id.ToString()}.bg_color");
            SetColorForElement(border, additionalParams, $"{id.ToString()}.bg_border_color");
        }

        internal void DestroyTinyMenuTeaser()
        {
            if (!panels.ContainsKey(PanelId.TinyMenuTeaser)) return;
            MonetizrMenuTeaser teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;
            GameObject.Destroy(teaser.gameObject);
            panels.Remove(PanelId.TinyMenuTeaser);
        }

        internal void ShowTinyMenuTeaser(Transform root, Vector2? screenPos, Action UpdateGameUI, int designVersion, ServerCampaign campaign)
        {
             MonetizrMenuTeaser teaser;     

            if (!panels.ContainsKey(PanelId.TinyMenuTeaser))
            {
                string teaserPrefab = "MonetizrMenuTeaser2";
                if (designVersion >= 3)
                {
                    teaserPrefab = $"MonetizrMenuTeaser{designVersion}";
                }
                
                var obj = GameObject.Instantiate<GameObject>(Resources.Load(teaserPrefab) as GameObject,
                    root != null ? root : rootUIObject.transform);

                teaser = obj.GetComponent<MonetizrMenuTeaser>();

                PrepareCustomColors(teaser.backgroundImage, teaser.backgroundBorderImage, campaign.serverSettings, PanelId.TinyMenuTeaser);

                teaser.uiVersion = designVersion;
                teaser.triggersButtonEventsOnDeactivate = false;

                panels.Add(PanelId.TinyMenuTeaser, teaser);
                  
            }
            else
            {
                teaser = panels[PanelId.TinyMenuTeaser] as MonetizrMenuTeaser;
            }

            if (teaser.IsVisible()) return;
            
            var missionsList = MonetizrManager.Instance.missionsManager.GetMissionsForRewardCenter(campaign,false);

            var m = missionsList[0];

            foreach (var t in teaser.gameObject.GetComponents<PanelTextItem>())
                t.InitializeByParent(PanelId.TinyMenuTeaser, m);

            teaser.SetActive(true);

            teaser.PreparePanel(PanelId.TinyMenuTeaser, null, m);

            MonetizrManager.Analytics.TrackEvent(m, teaser, MonetizrManager.EventType.Impression);

            if(root == null) teaser.rectTransform.SetAsFirstSibling();

            if (screenPos != null)
            {
                teaser.rectTransform.anchoredPosition = screenPos.Value;
            }
        }

        internal void HidePanel(PanelId id = PanelId.Unknown)
        {
            if (id == PanelId.Unknown && previousPanel != PanelId.Unknown) id = previousPanel;
            if(panels.ContainsKey(id)) panels[id].SetActive(false);
        }
      
    }

}