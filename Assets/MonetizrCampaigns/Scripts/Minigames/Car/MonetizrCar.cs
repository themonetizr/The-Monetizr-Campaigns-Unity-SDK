using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK
{
    internal class MonetizrCar : MonoBehaviour
    {
        internal MonetizrCarGame parent;

        void AnimationEvent()
        {
            if (parent != null)
            {
                parent.OnBonusTaken();
            }
        }
    }
}
