using Monetizr.SDK.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
{
    [RequireComponent(typeof(Image))]
    public class MonetizrCoin : MonoBehaviour
    {
        private Image _uiCoin;
        private Sprite _defaultSprite;

        private bool UpdateCoinToCustom()
        {
            var campaign = MonetizrManager.Instance?.GetActiveCampaign();

            if (campaign == null)
                return false;

            if (campaign.TryGetAsset<Sprite>(AssetsType.CustomCoinSprite, out var coinSprite))
            {
                _uiCoin.sprite = coinSprite;
            }

            return false;
        }

        private void OnEnable()
        {
            if (!UpdateCoinToCustom())
            {
                _uiCoin.sprite = _defaultSprite;
            }
        }

        private void Awake()
        {
            _uiCoin = GetComponent<Image>();
            _defaultSprite = _uiCoin.sprite;
        }

    }

}