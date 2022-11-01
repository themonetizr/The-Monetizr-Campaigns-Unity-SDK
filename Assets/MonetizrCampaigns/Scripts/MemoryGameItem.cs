using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class MemoryGameItem : MonoBehaviour
    {
        internal int id;
        internal MonetizrGameParentBase parent;
        internal Sprite middleAnimSprite;
        internal bool hasEvents;

        public Image image;
        public GameObject bonus;

        internal Action onCloseDone;
        internal bool hasBonus;
        internal bool isOpening;

        internal Animator bonusAnimator;

        // Start is called before the first frame update
        void Start()
        {
            hasEvents = true;
            bonus?.SetActive(false);

            bonusAnimator = bonus?.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlayOnBonus(string anim)
        {
            bonusAnimator?.Play(anim);
        }

        public void OnOpenDone()
        {
            onCloseDone?.Invoke();

            if (hasEvents)
                parent.OnOpenDone(id);
        }

        public void OnCloseDone()
        {

            if (hasEvents)
                parent.OnCloseDone(id);
        }

        public void OnMiddle()
        {
            if (hasBonus)
            {
                bonus?.SetActive(isOpening);
            }

            if (middleAnimSprite != null)
            {
                image.sprite = middleAnimSprite;
            }
        }
    }

}
