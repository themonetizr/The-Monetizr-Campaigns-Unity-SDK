using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Utils;

namespace Monetizr.SDK.Video
{
    public static class VideoPlayerUtils
    {
        public static string GetVideoPlayerURL (ServerCampaign serverCampaign)
        {
            string fallbackVideoPlayerURL = "https://image.themonetizr.com/videoplayer/html.zip";
            string globalSettingsVideoPlayerURL = serverCampaign.serverSettings.GetParam("videoplayer", "");
            if (string.IsNullOrEmpty(globalSettingsVideoPlayerURL))
            {
                MonetizrLogger.Print("VideoPlayer URL is from GlobalSettings.");
                return fallbackVideoPlayerURL;
            }
            else
            {
                MonetizrLogger.Print("VideoPlayer URL is from FallbackURL.");
                return globalSettingsVideoPlayerURL;
            }
        }

        public static async Task GetVideoPlayer (ServerCampaign campaign, Asset asset, bool isProgrammatic)
        {
            string videoPlayerURL = GetVideoPlayerURL(campaign);
            string campPath = Application.persistentDataPath + "/" + campaign.id;
            string zipFolder = isProgrammatic ? campaign.GetCampaignPath(asset.fpath) : campPath + "/" + asset.fpath;
            string indexPath = $"{zipFolder}/index.html";
            MonetizrLogger.Print($"{campPath} {zipFolder}");

            if (!Directory.Exists(zipFolder))
            {
                Directory.CreateDirectory(zipFolder);
            }

            string playerUrl = isProgrammatic ? campaign.serverSettings.GetParam("openrtb.player_url", videoPlayerURL) : videoPlayerURL;
            byte[] data = await MonetizrHttpClient.DownloadAssetData(playerUrl);

            if (data == null)
            {
                MonetizrLogger.PrintError("Can't download video player");
                return;
            }

            File.WriteAllBytes(zipFolder + "/html.zip", data);
            MonetizrUtils.ExtractAllToDirectory(zipFolder + "/html.zip", zipFolder);
            File.Delete(zipFolder + "/html.zip");

            if (!File.Exists(indexPath))
            {
                MonetizrLogger.PrintError($"Main html for video player {indexPath} doesn't exist");
                return;
            }

            if (!isProgrammatic)
            {
                string str = File.ReadAllText(indexPath);
                str = str.Replace("\"${MON_VAST_COMPONENT}\"", $"{campaign.vastAdParameters}");
                File.WriteAllText(indexPath, str);
            }
        }

    }
}