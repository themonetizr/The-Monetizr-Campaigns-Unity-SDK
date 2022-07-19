using System.Collections;
using System.Collections.Generic;
using Monetizr.Campaigns;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MonetizrCoin : MonoBehaviour
{
    Image uiCoin;
    Sprite defaultSprite;

    bool UpdateCoinToCustom()
    {
        //Log.Print($"------UpdateCoinToCustom-------- {uiCoin}");

        if (MonetizrManager.Instance == null || !MonetizrManager.Instance.HasCampaignsAndActive())
            return false;

        var currentCampaign = MonetizrManager.Instance.GetActiveCampaign();

        if (MonetizrManager.Instance.HasAsset(currentCampaign, AssetsType.CustomCoinSprite))
        {
            var coinSprite = MonetizrManager.Instance.GetAsset<Sprite>(currentCampaign, AssetsType.CustomCoinSprite);

            uiCoin.sprite = coinSprite;

            return true;
        }

        return false;
    }


    void OnEnable()
    {
        if(!UpdateCoinToCustom())
        {
            uiCoin.sprite = defaultSprite;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        
        uiCoin = GetComponent<Image>();
        defaultSprite = uiCoin.sprite;

        //Log.Print($"------COIN-------- {uiCoin}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
