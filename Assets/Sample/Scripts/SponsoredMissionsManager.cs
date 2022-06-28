using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;
using UnityEngine.UI;

public class SponsoredMissionsManager : MonoBehaviour
{
    public Sprite defaultRewardIcon;

    private void Start()
    {
        var key = "e_ESSXx8PK_aVFr8wwW2Sur31yjQKLtaNIUDS5X9rKo";
        

        MonetizrManager.Initialize(key, null, () =>
            {
                if(MonetizrManager.Instance.HasChallengesAndActive())
                {
                    MonetizrManager.ShowTinyMenuTeaser();
                    //Do something
                }
            },
            (bool soundOn) =>
            {
                //SoundManager.I.SetSoundAllowed(soundOn);
            });

        MonetizrManager.SetTeaserPosition(new Vector2(-420, 270));

        MonetizrManager.SetGameCoinAsset(RewardType.Coins,defaultRewardIcon, "Coins", () =>
                {
                    return 0;// GameController.I.GetCoinsTotal();
                },
                (int reward) =>
                {
                    //GameController.I.AddCoinsTotal(reward);
                });



    }


   
}
