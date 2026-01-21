using Monetizr.SDK.Core;
using UnityEngine;

namespace Monetizr.SDK.Utils
{
    public class TeaserHelper : MonoBehaviour
    {
        void OnEnable()
        {
            MonetizrInstance.Instance.OnMainMenuShow();
        }

        void OnDisable()
        {
            MonetizrInstance.Instance.OnMainMenuHide();
        }
    }
}
