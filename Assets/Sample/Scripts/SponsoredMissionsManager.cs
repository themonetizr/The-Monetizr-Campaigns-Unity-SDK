using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;
using UnityEngine.UI;

public class SponsoredMissionsManager : MonoBehaviour
{
    public Sprite defaultRewardIcon;
    public GameObject dummyUI;

    private void Start()
    {
        //temporary API key for testing
        var key = "t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo";

        //define sponsored mission (you can change amount of reward here)
        var sponsoredMissions = new List<MissionDescription>()
        {
           new MissionDescription(20, RewardType.Coins),
        };

        //initialize SDK
        MonetizrManager.Initialize(key, sponsoredMissions, () =>
            {
                if(MonetizrManager.Instance.HasCampaignsAndActive())
                {
                    //we can show teaser manually, but better to use TeaserHelper script
                    //see DummyMainUI object in SampleScene
                    dummyUI.SetActive(true);

                    //MonetizrManager.ShowTinyMenuTeaser();
                    //Do something
                }
            },
            (bool soundOn) =>
            {
                //SoundManager.I.SetSoundAllowed(soundOn);
            }, null);

        //good default placement for teaser
        MonetizrManager.SetTeaserPosition(new Vector2(-230, -765));

        //define reward type, name, getter and adder for reward
        MonetizrManager.SetGameCoinAsset(RewardType.Coins,defaultRewardIcon, "Coins", () =>
                {
                    //return current amount of coins
                    return 0;// GameController.I.GetCoinsTotal();
                },
                (ulong reward) =>
                {
                    //add coins
                    //GameController.I.AddCoinsTotal(reward);
                }, 10000);



    }


   
}
