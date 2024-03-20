//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

namespace Monetizr.SDK.Core
{
    public partial class MonetizrManager
    {
        public enum OnCompleteStatus
        {
            //if player rejected the offer or haven't seen anything
            Skipped,

            //if player completed the offer
            Completed
        }

    }

}