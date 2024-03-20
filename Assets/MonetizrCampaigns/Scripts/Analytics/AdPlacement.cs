#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.SDK.Analytics
{
    internal enum AdPlacement
    {
        TinyTeaser,
        Html5,
        HtmlPage,
        NotificationScreen,
        EmailEnterInGameRewardScreen,
        EmailEnterCouponRewardScreen,
        EmailEnterSelectionRewardScreen,
        EmailErrorScreen,
        CongratsNotificationScreen,
        EmailCongratsNotificationScreen,
        Video,
        Survey,
        SurveyNotificationScreen,
        Minigame,
        RewardsCenterScreen,
        AssetsLoading,
        ActionScreen,
        AssetsLoadingEnds,
        AssetsLoadingStarts,
        CodeEnterRewardScreen,
    }
}