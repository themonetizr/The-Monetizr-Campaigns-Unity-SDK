using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal abstract class PanelController : MonoBehaviour
    {
        enum State
        {
            Unknown,
            Hidden,
            Animating,
            Visible
        };

        private Animator animator;
        private CanvasGroup canvasGroup;
        public Action<bool> _onComplete = null;
        protected PanelId panelId;
        private State state;
        public UIController uiController;
        protected bool isSkipped = false;

        public Image backgroundImage;
        public Image backgroundBorderImage;

        [HideInInspector]
        internal Mission currentMission;

        [HideInInspector]
        public int uiVersion = 0;

        internal bool triggersButtonEventsOnDeactivate = true;
        internal Dictionary<string, string> additionalEventValues = new Dictionary<string, string>();

        internal abstract void PreparePanel(PanelId id, Action<bool> onComplete, Mission m);
        internal abstract void FinalizePanel(PanelId id);

        internal virtual bool SendImpressionEventManually()
        {
            return false;
        }

        internal virtual AdPlacement? GetAdPlacement()
        {
            return null;
        }

        protected void Awake()
        {
            animator = GetComponent<Animator>();

            if (canvasGroup == null)
                canvasGroup = gameObject.GetComponent<CanvasGroup>();

            Assert.IsNotNull(animator);
            Assert.IsNotNull(canvasGroup);


            Intractable(false);

            state = State.Unknown;
        }

        internal void BlockRaycasts(bool enable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = enable;
            }
        }

        internal void Intractable(bool enable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = enable;


            }
        }

        internal void SetActive(bool active, bool immediately = false)
        {
            if (!active && !immediately && triggersButtonEventsOnDeactivate)
            {
                MonetizrManager.Analytics.TrackEvent(currentMission,
                    this,
                    isSkipped ? MonetizrManager.EventType.ButtonPressSkip : MonetizrManager.EventType.ButtonPressOk,
                    additionalEventValues);
            }

            if (active) //showing
            {
                if (state != State.Animating && state != State.Visible)
                {
                    BlockRaycasts(true);

                    gameObject.SetActive(true);
                    animator.Play("PanelAnimator_Show");

                    state = State.Animating;
                }
            }
            else if (state != State.Hidden) //hiding
            {
                BlockRaycasts(false);

                Intractable(false);

                if (!immediately && state != State.Animating)
                {
                    animator.Play("PanelAnimator_Hide");

                    state = State.Animating;
                }
                else
                {
                    OnAnimationHide();
                }

            }
        }

        internal GameObject GetActiveElement(string name)
        {
            return transform.Find(name).gameObject;
        }

        internal GameObject GetElement(string name)
        {
            foreach (var childTransform in transform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform.name == name)
                    return childTransform.gameObject;
            }

            return null;
        }

        internal bool IsVisible()
        {
            return state == State.Visible;
        }

        internal bool IsHidden()
        {
            return state == State.Hidden;
        }

        internal void OnAnimationShow()
        {
            Intractable(true);

            state = State.Visible;
        }

        private void OnAnimationHide()
        {
            state = State.Hidden;

            FinalizePanel(panelId);

            if (!SendImpressionEventManually())
                MonetizrManager.Analytics?.TrackEvent(currentMission, this, MonetizrManager.EventType.ImpressionEnds);

            gameObject.SetActive(false);

            _onComplete?.Invoke(isSkipped);
        }


    }

}