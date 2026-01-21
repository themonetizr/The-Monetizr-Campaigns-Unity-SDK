using Monetizr.SDK.Debug;
using Monetizr.SDK.Networking;
using System.Text;
using System.Threading.Tasks;

namespace Monetizr.SDK.Analytics
{
    public static class OMSDKHelper
    {
        private static string omidServiceContent;

        public static async Task<bool> DownloadOMSDKServiceContent()
        {
            string url = "https://image.themonetizr.com/omsdk/omsdk-v1.js";
            byte[] data = await MonetizrHttpClient.DownloadAssetData(url);

            if (data == null)
            {
                MonetizrLogger.PrintWarning($"InitializeOMSDK failed! Download of {url} failed!");
                return false;
            }

            omidServiceContent = Encoding.UTF8.GetString(data);
            return true;
        }

        public static void InitializeOMSDK (string vastAdVerificationParams)
        {
            MonetizrLogger.Print("InitializeOMSDK Params: {" + vastAdVerificationParams + "} / Service Content: " + omidServiceContent);
            UniWebViewInterface.InitOMSDK(vastAdVerificationParams, omidServiceContent);
        }
    }

}

