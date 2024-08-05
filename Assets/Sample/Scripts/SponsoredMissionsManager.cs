using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monetizr.SDK.Core;
using Monetizr.SDK.Utils;
using Monetizr.SDK.New;
using Monetizr.SDK.Debug;

namespace Monetizr.Sample
{
    public class SponsoredMissionsManager : MonoBehaviour
    {
        public Sprite defaultRewardIcon;
        public Sprite gemsRewardIcon;
        public GameObject dummyUI;

        private static void GetAdvertisingId(out string advertisingID, out bool limitAdvertising)
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
                   AndroidJavaClass up = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
                   AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
                   AndroidJavaClass client = new AndroidJavaClass ("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                   AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject> ("getAdvertisingIdInfo",currentActivity);

                   advertisingID = adInfo.Call<string> ("getId").ToString();   
                   limitAdvertising = (adInfo.Call<bool> ("isLimitAdTrackingEnabled"));

#elif UNITY_IOS
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
            MonetizrLog.isEnabled = true;
            GetAdvertisingId(out var advertisingID, out var limitAdvertising);
            MonetizrManager.SetAdvertisingIds(advertisingID, limitAdvertising);

            //define reward type, name, getter and adder for reward
            MonetizrManager.SetGameCoinAsset(RewardType.Reward1, defaultRewardIcon, "Coins", () =>
            {
                //return current amount of coins
                return 0;// GameController.I.GetCoinsTotal();
            },
                    (ulong reward) =>
                    {
                        //add coins
                        //GameController.I.AddCoinsTotal(reward);
                    }, 10000);

            MonetizrManager.SetGameCoinAsset(RewardType.Reward2, gemsRewardIcon, "Gems", () =>
            {
                //return current amount of coins
                return 0;// GameController.I.GetCoinsTotal();
            },
                (ulong reward) =>
                {
                    //add coins
                    //GameController.I.AddCoinsTotal(reward);
                }, 100);


            //good default placement for teaser
            MonetizrManager.SetTeaserPosition(New_MobileUtils.IsInLandscapeMode() ? new Vector2(700, 300) : new Vector2(-230, -765));

            //initialize SDK
            MonetizrManager.Initialize(null, () =>
            {
                if (MonetizrManager.IsActiveAndEnabled())
                {
                    //we can show teaser manually, but better to use TeaserHelper script
                    //see DummyMainUI object in SampleScene
                    dummyUI.SetActive(true);

                    //MonetizrManager.ShowTeaser();
                    //Do something
                }
            }, (bool soundOn) => { }, null);

        }

    }

}