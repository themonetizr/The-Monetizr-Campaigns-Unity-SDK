//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using UnityEngine.Networking;
using System;
using System.Threading.Tasks;


namespace Monetizr.Campaigns
{
    internal class DownloadHelper
    {
        /// <summary>
        /// Downloads any type of asset and returns its data as an array of bytes
        /// </summary>
        public static async Task<byte[]> DownloadAssetData(string url, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            await uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Log.PrintError($"Network error {uwr.error} with {url}");
                onDownloadFailed?.Invoke();
                return null;
            }

            return uwr.downloadHandler.data;
        }
    }

}