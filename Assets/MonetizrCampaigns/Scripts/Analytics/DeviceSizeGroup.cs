#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if USING_FACEBOOK
using Facebook.Unity;   
#endif

namespace Monetizr.Campaigns
{
    internal enum DeviceSizeGroup
    {
        Phone,
        Tablet,
        Unknown
    }
}
