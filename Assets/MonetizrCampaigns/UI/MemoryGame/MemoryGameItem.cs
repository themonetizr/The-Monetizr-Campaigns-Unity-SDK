using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{

    internal class MemoryGameItem : MonoBehaviour
    {
        internal int id;
        internal MonetizrMemoryGame parent;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnOpenDone()
        {
            parent.OnOpenDone(id);
        }

        public void OnCloseDone()
        {
            parent.OnCloseDone(id);
        }
    }

}
