using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace Monetizr.SDK.Campaigns
{
    public static class CampaignUtils
    {
        public static List<ServerCampaign> FilterInvalidCampaigns (List<ServerCampaign> result)
        {
            RemoveCampaignsWithNoAssets(result);
            RemoveCampaignsWithWrongSDKVersion(result);
            CheckAllowedDevices(result);
            return result;
        }

        private static void RemoveCampaignsWithNoAssets (List<ServerCampaign> result)
        {
            result.RemoveAll((System.Predicate<ServerCampaign>)(e =>
            {
                bool noAssets = e.assets.Count == 0;
                if (noAssets) MonetizrLogger.Print($"Removing campaign {e.id} with no assets");
                return noAssets;
            }));
        }

        private static void RemoveCampaignsWithWrongSDKVersion (List<ServerCampaign> result)
        {
            result.RemoveAll((System.Predicate<ServerCampaign>)(e =>
            {
                string minSdkVersion = e.serverSettings.GetParam("min_sdk_version");

                if (minSdkVersion != null)
                {
                    bool sdkVersionCheck = MonetizrUtils.CompareVersions(MonetizrSettings.SDKVersion, minSdkVersion) < 0;
                    if (sdkVersionCheck) MonetizrLogger.Print($"Removing campaign {e.id} because SDK version {MonetizrSettings.SDKVersion} less then required SDK version {minSdkVersion}");
                    return sdkVersionCheck;
                }

                return false;
            }));
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
                        MonetizrLogger.Print($"Campaign {e.id} has no allowed list");
                        return false;
                    }
                    else
                    {
                        MonetizrLogger.Print($"Campaign {e.id} has allowed list: {allowed_device_id}");

                        bool isKeyFound = false;

                        Array.ForEach(allowed_device_id.Split(';'), id =>
                        {
                            if (id == MonetizrMobileAnalytics.advertisingID)
                                isKeyFound = true;
                        });

                        if (!isKeyFound)
                        {
                            MonetizrLogger.Print($"Device {MonetizrMobileAnalytics.advertisingID} isn't allowed for campaign {e.id}");
                            return true;
                        }
                        else
                        {
                            MonetizrLogger.Print($"Device {MonetizrMobileAnalytics.advertisingID} is OK for campaign {e.id}");
                            return false;
                        }
                    }
                });
            }
            else
            {
                MonetizrLogger.Print($"No ad id defined to filter campaigns. Please allow ad tracking!");
            }
#endif
        }

        public static void SetupCampaignType (ServerCampaign campaign)
        {
            if (GetBoolParameter(campaign, "is_fallback_campaign"))
            {
                campaign.campaignType = CampaignType.Fallback;
                return;
            }

            if (GetBoolParameter(campaign, "programmatic"))
            {
                campaign.campaignType = CampaignType.Programmatic;
                return;
            }

            if (GetBoolParameter(campaign, "campaign.use_adm"))
            {
                campaign.campaignType = CampaignType.ADM;
                return;
            }

            campaign.campaignType = CampaignType.MonetizrBackend;
        }

        public static bool GetBoolParameter (ServerCampaign campaign, string parameter)
        {
            if (String.IsNullOrEmpty(campaign.content))
            {
                MonetizrLogger.Print("CampaignID " + campaign.id + " - Content is empty.");
                return false;
            }

            string extractedValue = MonetizrUtils.ExtractValueFromJSON(campaign.content, parameter);
            if (String.IsNullOrEmpty(extractedValue))
            {
                MonetizrLogger.Print("CampaignID " + campaign.id + " - Parameter " + parameter + " was not parsed correctly.");
                return false;
            }

            bool parameterValue = bool.TryParse(extractedValue, out bool result);
            MonetizrLogger.Print("CampaignID " + campaign.id + " - Parameter " + parameter + " value is: " + parameterValue);
            return parameterValue;
        }

        public static string PrintAssetsTypeList (ServerCampaign serverCampaign)
        {
            StringBuilder result = new StringBuilder();

            foreach (var asset in serverCampaign.assets)
            {
                result.AppendLine($"{serverCampaign.id}: {asset.type}");
            }

            return result.ToString();
        }
    }
}