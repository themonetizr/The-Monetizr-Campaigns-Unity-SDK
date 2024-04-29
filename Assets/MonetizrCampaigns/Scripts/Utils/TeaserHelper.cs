using Monetizr.SDK.Core;
using UnityEngine;

namespace Monetizr.SDK.Utils
{
    public class TeaserHelper : MonoBehaviour
    {
        void OnEnable()
        {
            MonetizrManager.OnMainMenuShow();
        }

        void OnDisable()
        {
            MonetizrManager.OnMainMenuHide();
        }
    }
}
