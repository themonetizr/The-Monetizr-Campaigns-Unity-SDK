using UnityEngine;

namespace Monetizr.SDK.Minigames
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
