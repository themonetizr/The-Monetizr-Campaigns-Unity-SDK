//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using System.Collections.Generic;


namespace Monetizr.Campaigns
{
    internal static class MonetizrErrors
    {
        public static readonly Dictionary<ErrorType, string> msg = new Dictionary<ErrorType, string>()
        {
            { ErrorType.NotinitializedSDK, "You're trying to use Monetizr SDK before it's been initialized. Call MonetizerManager.Initalize first." },
            { ErrorType.SimultaneusAdAssets, "Simultaneous display of multiple ads is not supported!" },
            { ErrorType.AdAssetStillShowing, "Some ad asset are still showing." },
            { ErrorType.ConnectionError, "Connection error while getting list of campaigns!" }
        };
    }

}