using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetizr.SDK.Campaigns
{
    public static class CampaignUtils
    {
        public static bool IsCampaignValid (ServerCampaign campaign)
        {
            if (DoesCampaignHaveAssets(campaign))
            {
                MonetizrLogger.PrintError("Removing CampaignID: " + campaign.id + " for it has no assets.");
                return false;
            }

            if (IsCampaignCompatibleWithSDKVersion(campaign))
            {
                MonetizrLogger.PrintError("Removing CampaignID: " + campaign.id + " for it does not have correct SDK version.");
                return false;
            }

            if (IsCampaignCompatibleWithDevice(campaign))
            {
                MonetizrLogger.PrintError("Removing CampaignID: " + campaign.id + " for it is not compatible with device.");
                return false;
            }

            return true;
        }

        private static bool DoesCampaignHaveAssets (ServerCampaign campaign)
        {
            bool hasAssets = campaign.assets.Count != 0;
            return hasAssets;
        }

        private static bool IsCampaignCompatibleWithSDKVersion (ServerCampaign campaign)
        {
            string minSdkVersion = campaign.serverSettings.GetParam("min_sdk_version");

            if (minSdkVersion != null)
            {
                bool sdkVersionCheck = MonetizrUtils.CompareVersions(MonetizrSettings.SDKVersion, minSdkVersion) < 0;
                return !sdkVersionCheck;
            }

            return false;
        }

        private static bool IsCampaignCompatibleWithDevice (ServerCampaign campaign)
        {
#if !UNITY_EDITOR
            bool hasAdId = !string.IsNullOrEmpty(MonetizrMobileAnalytics.advertisingID);

            if (hasAdId)
            {
                string allowed_device_id = campaign.serverSettings.GetParam("allowed_ad_id", "");

                if (allowed_device_id.Length == 0)
                {
                    MonetizrLogger.Print($"Campaign {campaign.id} has no allowed list");
                    return true;
                }
                else
                {
                    MonetizrLogger.Print($"Campaign {campaign.id} has allowed list: {allowed_device_id}");

                    bool isKeyFound = false;

                    Array.ForEach(allowed_device_id.Split(';'), id =>
                    {
                        if (id == MonetizrMobileAnalytics.advertisingID) isKeyFound = true;
                    });

                    if (!isKeyFound)
                    {
                        MonetizrLogger.Print($"Device {MonetizrMobileAnalytics.advertisingID} isn't allowed for campaign {campaign.id}");
                        return false;
                    }
                    else
                    {
                        MonetizrLogger.Print($"Device {MonetizrMobileAnalytics.advertisingID} is OK for campaign {campaign.id}");
                        return true;
                    }
                }
            }
            else
            {
                MonetizrLogger.Print($"No ad id defined to filter campaigns. Please allow ad tracking!");
                return false;
            }
#endif

            return true;
        }

        public static void FilterInvalidCampaigns (List<ServerCampaign> result)
        {
            RemoveCampaignsWithNoAssets(result);
            RemoveCampaignsWithWrongSDKVersion(result);
            CheckAllowedDevices(result);
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
            if (IsProgrammatic(campaign))
            {
                campaign.campaignType = CampaignType.Programmatic;
                return;
            }

            if (IsADM(campaign))
            {
                campaign.campaignType = CampaignType.ADM;
                return;
            }

            campaign.campaignType = CampaignType.MonetizrBackend;
        }

        public static void SetupCampaignsType (List<ServerCampaign> serverCampaigns)
        {
            foreach (ServerCampaign campaign in serverCampaigns)
            {
                if (IsADM(campaign))
                {
                    campaign.campaignType = CampaignType.ADM;
                    continue;
                }

                if (IsProgrammatic(campaign))
                {
                    campaign.campaignType = CampaignType.Programmatic;
                    continue;
                }

                campaign.campaignType = CampaignType.MonetizrBackend;
            }
        }

        private static bool IsADM (ServerCampaign campaign)
        {
            string extractedValue = MonetizrUtils.ExtractValueFromJSON(campaign.content, "campaign.use_adm");
            bool useADM = bool.TryParse(extractedValue, out bool result) && result;
            return useADM;
        }

        private static bool IsProgrammatic (ServerCampaign campaign)
        {
            string extractedValue = MonetizrUtils.ExtractValueFromJSON(campaign.content, "programmatic");
            bool isProgrammatic = bool.TryParse(extractedValue, out bool result) && result;
            return isProgrammatic;
        }

        public static string PrintAssetsTypeList(ServerCampaign serverCampaign)
        {
            StringBuilder result = new StringBuilder();

            foreach (var asset in serverCampaign.assets)
            {
                result.AppendLine($"{serverCampaign.id}: {asset.type}");
            }

            return result.ToString();
        }

        public static SettingsDictionary<string, string> GetDefaultSettingsDictionaryForProgrammatic ()
        {
            Dictionary<string, string> settingsDictionary = new Dictionary<string, string>()
            {
                { "design_version", "2" },
                { "amount_of_teasers", "100" },
                { "teaser_design_version", "3" },
                { "amount_of_notifications", "100" },
                { "RewardCenter.show_for_one_mission", "true" },

                { "bg_color", "#124674" },
                { "bg_color2", "#124674" },
                { "link_color", "#AAAAFF" },
                { "text_color", "#FFFFFF" },
                { "bg_border_color", "#FFFFFF" },
                { "RewardCenter.reward_text_color", "#2196F3" },

                { "CongratsNotification.button_text", "Awesome!" },
                { "CongratsNotification.content_text", "You have earned <b>%ingame_reward%</b> from Monetizr" },
                { "CongratsNotification.header_text", "Get your awesome reward!" },

                { "StartNotification.SurveyReward.header_text", "<b>Survey by Monetizr</b>" },
                { "StartNotification.button_text", "Learn more!" },
                { "StartNotification.content_text", "Join Monetizr<br/>to get game rewards" },
                { "StartNotification.header_text", "<b>Rewards by Monetizr</b>" },

                { "RewardCenter.VideoReward.content_text", "Watch video and get reward %ingame_reward%" }
            };

            return new SettingsDictionary<string, string>(settingsDictionary);
        }
    }
}