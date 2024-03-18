using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class MemoryGameItem : MonoBehaviour
    {
        internal int id = 0;
        internal MonetizrGameParentBase parent = null;
        internal Sprite middleAnimSprite = null;
        internal bool hasEvents = false;

        public Image image = null;
        public GameObject bonus = null;
        public Image bonusImage = null;

        internal Action onCloseDone = null;
        internal bool hasBonus = false;
        internal bool isOpening = false;

        internal Animator bonusAnimator = null;

        // Start is called before the first frame update
        void Start()
        {
            hasEvents = true;

            if (bonus == null) return;

            bonus.SetActive(false);
            bonusAnimator = bonus.GetComponent<Animator>();
        }

        public void PlayOnBonus(string anim)
        {
            if (bonusAnimator != null)
            {
                bonusAnimator.Play(anim);
            }
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
            if (hasBonus && bonus != null)
            {
                bonus.SetActive(isOpening);
            }

            if (middleAnimSprite != null)
            {
                image.sprite = middleAnimSprite;
            }
        }
    }

}
