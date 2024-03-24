using System;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.Minigames
{
    internal class MemoryGameItem : MonoBehaviour
    {
        public GameObject bonus = null;
        public Image image = null;
        public Image bonusImage = null;

        internal MonetizrGameParentBase parent = null;
        internal Sprite middleAnimSprite = null;
        internal Animator bonusAnimator = null;
        internal Action onCloseDone = null;

        internal int id = 0;

        internal bool hasEvents = false;
        internal bool hasBonus = false;
        internal bool isOpening = false;

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
