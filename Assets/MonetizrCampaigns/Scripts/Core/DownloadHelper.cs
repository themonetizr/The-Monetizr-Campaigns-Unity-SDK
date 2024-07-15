using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Core
{
    internal class DownloadHelper
    {
        public static async Task<byte[]> DownloadAssetData(string url, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            uwr.timeout = 10; 

            await uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Log.PrintError($"Network error {uwr.error} with {url}");
                onDownloadFailed?.Invoke();
                return null;
            }

            return uwr.downloadHandler.data;
        }

    }

}