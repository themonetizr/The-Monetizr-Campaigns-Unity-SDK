using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK
{
    internal partial class MonetizrMemoryGame
    {
        internal class Item
        {
            public GameObject go;
            public Button b;
            public Animator a;
            public MemoryGameItem gi;

            //0 - undefined, 1 - empty, 2 - item
            public int value;
            internal bool isOpened;
            internal bool isFullyOpened;
        }
    }

}