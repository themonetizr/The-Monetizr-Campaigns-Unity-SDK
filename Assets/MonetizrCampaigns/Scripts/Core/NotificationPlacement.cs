//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

namespace Monetizr.SDK
{
    public partial class MonetizrManager
    {
        public enum NotificationPlacement
        {
            LevelStartNotification = 0,
            MainMenuShowNotification = 1,
            ManualNotification = 2
        }

    }

}