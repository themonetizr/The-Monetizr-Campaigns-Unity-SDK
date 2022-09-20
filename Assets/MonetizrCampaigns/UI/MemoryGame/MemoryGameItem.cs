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

        internal Action onCloseDone;

        // Start is called before the first frame update
        void Start()
        {
            hasEvents = true;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnOpenDone()
        {
            onCloseDone?.Invoke();

            if (hasEvents)
                parent.OnOpenDone(id);
        }

        public void OnCloseDone()
        {

            if(hasEvents)
                parent.OnCloseDone(id);
        }

        public void OnMiddle()
        {
            if (middleAnimSprite != null) image.sprite = middleAnimSprite;
        }
    }

}
