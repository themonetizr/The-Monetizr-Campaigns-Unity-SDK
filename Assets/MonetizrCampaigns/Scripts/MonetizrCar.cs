using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
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
