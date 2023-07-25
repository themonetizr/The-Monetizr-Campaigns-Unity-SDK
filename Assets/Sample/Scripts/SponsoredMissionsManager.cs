using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;
using UnityEngine.UI;


public class SponsoredMissionsManager : MonoBehaviour
{
    public Sprite defaultRewardIcon;
    public GameObject dummyUI;

    private void GetAdvertisingId(out string advertisingID, out bool limitAdvertising)
    {
#if !UNITY_EDITOR
#if UNITY_ANDROID
               //AndroidJavaClass versionInfo = new AndroidJavaClass("android/os/Build$VERSION");

               //osVersion = versionInfo.GetStatic<string>("RELEASE");

               AndroidJavaClass up = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
               AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
               AndroidJavaClass client = new AndroidJavaClass ("com.google.android.gms.ads.identifier.AdvertisingIdClient");
               AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject> ("getAdvertisingIdInfo",currentActivity);

               advertisingID = adInfo.Call<string> ("getId").ToString();   
               limitAdvertising = (adInfo.Call<bool> ("isLimitAdTrackingEnabled"));

#elif UNITY_IOS
              // osVersion = UnityEngine.iOS.Device.systemVersion;
               limitAdvertising = !(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
               advertisingID = Device.advertisingIdentifier;

#endif
#else
        advertisingID = "";
        limitAdvertising = false;

#endif
    }

    private void Start()
    {
        //temporary API key for testing
        var key = "t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo";

        string advertisingID = "";
        bool limitAdvertising = false;

        GetAdvertisingId(out advertisingID, out limitAdvertising);

        MonetizrManager.SetAdvertisingIds(advertisingID, limitAdvertising);


        //good default placement for teaser
        MonetizrManager.SetTeaserPosition(new Vector2(-230, -765));

        //define reward type, name, getter and adder for reward
        MonetizrManager.SetGameCoinAsset(RewardType.Coins, defaultRewardIcon, "Coins", () =>
                {
                    //return current amount of coins
                    return 0;// GameController.I.GetCoinsTotal();
                },
                (ulong reward) =>
                {
                    //add coins
                    //GameController.I.AddCoinsTotal(reward);
                }, 10000);


        //initialize SDK
        MonetizrManager.Initialize(key, null, () =>
            {
                if(MonetizrManager.IsActiveAndEnabled())
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


    }


   
}
