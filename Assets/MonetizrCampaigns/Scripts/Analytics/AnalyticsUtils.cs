using Monetizr.SDK.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK.Analytics
{
    public static class AnalyticsUtils
    {
        public static bool CanTrackInOMSDK (AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.TinyTeaser:
                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.EmailCongratsNotificationScreen:
                case AdPlacement.Minigame:
                case AdPlacement.Video:
                case AdPlacement.Survey:
                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                    return true;
            }

            return false;
        }

        public static string GetPlacementName (AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.TinyTeaser: return "TinyTeaser";
                case AdPlacement.Video:
                case AdPlacement.Html5: return "Html5VideoScreen";
                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                    return "NotificationScreen";
                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                    return "EmailEnterScreen";
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.EmailCongratsNotificationScreen: return "CongratsScreen";
                case AdPlacement.Minigame: return "MiniGameScreen";
                case AdPlacement.Survey: return "SurveyScreen";
                case AdPlacement.HtmlPage: return "HtmlPageScreen";
                case AdPlacement.RewardsCenterScreen: return "RewardsCenterScreen";

                default:
                    return adPlacement.ToString();

            }
        }

        public static readonly Dictionary<DeviceSizeGroup, string> deviceSizeGroupNames = new Dictionary<DeviceSizeGroup, string>()
        {
            { DeviceSizeGroup.Phone, "phone" },
            { DeviceSizeGroup.Tablet, "tablet" },
            { DeviceSizeGroup.Unknown, "unknown" },
        };

        public static DeviceSizeGroup GetDeviceGroup()
        {
#if UNITY_IOS
    bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
            {
                return DeviceSizeGroup.Tablet;
            }
 
            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");
            if (deviceIsIphone)
            {
                return DeviceSizeGroup.Phone;
            }
#elif UNITY_ANDROID

            float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            bool isTablet = (MobileUtils.GetDeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);

            if (isTablet)
            {
                return DeviceSizeGroup.Tablet;
            }
            else
            {
                return DeviceSizeGroup.Phone;
            }

#endif
            return DeviceSizeGroup.Unknown;
        }

        public static string GetOsGroup ()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#else
            return "";
#endif
        }

        public static string GetPlacementGroup (AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.TinyTeaser:
                    return "MiniBanners";

                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.RewardsCenterScreen:
                case AdPlacement.EmailCongratsNotificationScreen:
                    return "StaticScreens";

                case AdPlacement.Minigame:
                case AdPlacement.Video:
                case AdPlacement.Html5:
                case AdPlacement.Survey:
                    return "EngagementScreens";

                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                case AdPlacement.ActionScreen:
                    return "ActionScreens";

                default:
                    return "Other";

            }
        }

    }
}