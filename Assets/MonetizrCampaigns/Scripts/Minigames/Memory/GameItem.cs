using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.Minigames
{
    internal class GameItem
    {
        public GameObject go;
        public Button b;
        public Animator a;
        public MemoryGameItem gi;
        public int value;

        internal bool isOpened;
        internal bool isFullyOpened;
    }

}