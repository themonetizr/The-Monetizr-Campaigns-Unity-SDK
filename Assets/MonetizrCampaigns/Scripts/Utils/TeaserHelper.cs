using Monetizr.SDK.Core;
using UnityEngine;

namespace Monetizr.SDK.Utils
{
    public class TeaserHelper : MonoBehaviour
    {
        private void Start()
        {
            //MonetizrManager.SetTeaserRoot(GetComponentInParent<RectTransform>());
        }

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
