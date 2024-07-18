using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Utils;
using System.Collections.Generic;

namespace Monetizr.SDK.New
{
    public static class New_CampaignUtils
    {
        public static void FilterInvalidCampaigns (List<ServerCampaign> result)
        {
            RemoveCampaignsWithNoAssets(result);
            RemoveCampaignsWithWrongSDKVersion(result);
            CheckAllowedDevices(result);
        }

        private static void RemoveCampaignsWithNoAssets (List<ServerCampaign> result)
        {
            result.RemoveAll(e =>
            {
                bool noAssets = e.assets.Count == 0;
                if (noAssets) Log.Print($"Removing campaign {e.id} with no assets");
                return noAssets;
            });
        }

        private static void RemoveCampaignsWithWrongSDKVersion (List<ServerCampaign> result)
        {
            result.RemoveAll(e =>
            {
                string minSdkVersion = e.serverSettings.GetParam("min_sdk_version");

                if (minSdkVersion != null)
                {
                    bool sdkVersionCheck = MonetizrUtils.CompareVersions(MonetizrSettings.SDKVersion, minSdkVersion) < 0;
                    if (sdkVersionCheck) Log.Print($"Removing campaign {e.id} because SDK version {MonetizrSettings.SDKVersion} less then required SDK version {minSdkVersion}");
                    return sdkVersionCheck;
                }

                return false;
            });
        }

        private static void CheckAllowedDevices (List<ServerCampaign> result)
        {
#if !UNITY_EDITOR

            var hasAdId = !string.IsNullOrEmpty(MonetizrMobileAnalytics.advertisingID);

            if (hasAdId)
            {
                result.RemoveAll(e =>
                {
                    string allowed_device_id = e.serverSettings.GetParam("allowed_ad_id", "");

                    if (allowed_device_id.Length == 0)
                    {
                        Log.Print($"Campaign {e.id} has no allowed list");
                        return false;
                    }
                    else
                    {
                        Log.Print($"Campaign {e.id} has allowed list: {allowed_device_id}");

                        bool isKeyFound = false;

                        Array.ForEach(allowed_device_id.Split(';'), id =>
                        {
                            if (id == MonetizrMobileAnalytics.advertisingID)
                                isKeyFound = true;
                        });

                        if (!isKeyFound)
                        {
                            Log.Print($"Device {MonetizrMobileAnalytics.advertisingID} isn't allowed for campaign {e.id}");
                            return true;
                        }
                        else
                        {
                            Log.Print($"Device {MonetizrMobileAnalytics.advertisingID} is OK for campaign {e.id}");
                            return false;
                        }
                    }
                });
            }
            else
            {
                Log.Print($"No ad id defined to filter campaigns. Please allow ad tracking!");
            }
#endif
        }
    }
}