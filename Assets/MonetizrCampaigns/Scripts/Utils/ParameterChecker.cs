using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using System.Collections.Generic;
using System.Text;

namespace Monetizr.SDK.Utils
{
    public static class ParameterChecker
    {
        private static List<string> oldParametersList = new List<string>()
        {
            "base_api_endpoint",
            "videoplayer",
            "min_sdk_version",
            "is_campaign_html",
            "custom_missions",
            "amount_of_rv_offers",
            "use_default_reward",
            "mixpanel_proxy_endpoint",
            "unity_logging",
            "send_old_events",
            "close_button_html",
            "claim_button_html",
            "button_text_color",
            "ActionReward.reward_time",
            "ActionReward.reward_pages",
            "ActionReward.reward_url",
            "CodeReward.code",
            "VideoReward.reward_time",
            "VideoReward.close_button_delay",
            "VideoReward.reward_url",
            "RewardCenter.transform",
            "RewardCenter.close_after_mission_completion",
            "RewardCenter.show_for_one_mission",
            "RewardCenter.show_disabled_missions",
            "RewardCenter.do_not_claim_and_hide_missions",
            "RewardCenter.missions_num_text",
            "RewardCenter.money_num_text",
            "teaser.single_picture",
            "teaser.no_video_sign",
            "teaser.show_button",
            "teaser.show_number",
            "teaser.num_text",
            "teaser_transform",
            "hide_teaser_button",
            "amount_of_teasers",
            "allow_fallback_prebid",
            "allow_fallback_endpoint",
            "prebid_host",
            "prebid_data",
            "endpoint_base",
            "endpoints",
            "should_restart",
            "restart_timer",
            "campaign.timeout_duration",
            "endpoint_params",
            "notifications_delay_time_sec",
            "no_start_level_notifications",
            "no_main_menu_notifications",
            "no_campaigns_notifications",
            "amount_of_notifications",
            "startup_skipped_notifications",
            "CongratisNotification.content_text2",
            "email_enter_close_button_delay",
            "GiveawayEmailEnterNotification.terms_url_text",
            "email_giveaway_mission_without_video",
            "email_giveaway_type",
            "watch_video_only_once",
            "SurveyUnityView.next_text",
            "SurveyUnityView.submit_text",
            "SurveyUnityView.close_button_delay",
            "omid_destroy_delay",
            "omsdk.verify_videos",
            "more_memory_stats",
            "programmatic",
            "openrtb.give_reward_on_programmatic_fail",
            "app.storeid",
            "app.storeurl",
            "app.name",
            "app.clientua",
            "app.deviceua",
            "app.id",
            "app.serverua",
            "app.deviceip",
            "app.adtype",
            "app.contenturi",
            "app.contentid",
            "app.serverside",
            "app.playerstate",
            "app.widthheight",
            "app.creatizesize",
            "app.EPSILON_CREATIVE_ID",
            "app.DMC_PLACEMENT_ID",
            "app.EPSILON_TRANSACTION_ID",
            "app.EPSILON_CORRELATION_USER_DATA",
            "app.omidpartner",
            "imp.tagid",
            "us_privacy",
            "site.page",
            "site.domain"
        };

        private static List<string> newParametersList = new List<string>()
        {
            "global.baseApiEndpoint",
            "campaign.videoplayer",
            "campaign.minSdkVersion",
            "campaign.isHtml",
            "campaign.missions",
            "campaign.amountOfRvOffers",
            "campaign.useDefaultReward",
            "log.proxyEndpoint",
            "log.isEnabled",
            "log.sendOldEvents",
            "ui.closeButtonHtml",
            "ui.claimButtonHtml",
            "ui.buttonTextColor",
            "actionReward.time",
            "actionReward.pages",
            "actionReward.url",
            "codeReward.code",
            "videoReward.time",
            "videoReward.closeButtonDelay",
            "videoReward.url",
            "rewardCenter.transform",
            "rewardCenter.closeAfterMissionCompletion",
            "rewardCenter.showForOneMission",
            "rewardCenter.showDisabledMissions",
            "rewardCenter.hideMissionsOnClaim",
            "rewardCenter.missionsNumText",
            "rewardCenter.moneyNumText",
            "teaser.singlePicture",
            "teaser.noVideoSign",
            "teaser.showButton",
            "teaser.showNumber",
            "teaser.numText",
            "teaser.transform",
            "teaser.hideButton",
            "teaser.amount",
            "fallback.allowFallback",
            "fallback.allowEndpoint",
            "fallback.prebid.host",
            "fallback.prebid.data",
            "fallback.endpoints.base",
            "fallback.endpoints.list",
            "fallback.shouldRestart",
            "fallback.restartTimer",
            "fallback.timeoutDuration",
            "fallback.endpoints.params",
            "notifications.delay",
            "notifications.disableOnStartLevel",
            "notifications.disableOnMainMenu",
            "notifications.disableForCampaigns",
            "notifications.maxAmount",
            "notifications.startupSkipCount",
            "notifications.congratsContentText",
            "emailGiveaway.closeButtonDelay",
            "emailGiveaway.termsUrlText",
            "emailGiveaway.missionWithoutVideo",
            "emailGiveaway.type",
            "emailGiveaway.watchVideoOnlyOnce",
            "survey.nextText",
            "survey.submitText",
            "survey.closeButtonDelay",
            "omsdk.destroyDelay",
            "omsdk.verifyVideos",
            "more_memory_stats",
            "programmatic",
            "openrtb.give_reward_on_programmatic_fail",
            "app.storeid",
            "app.storeurl",
            "app.name",
            "app.clientua",
            "app.deviceua",
            "app.id",
            "app.serverua",
            "app.deviceip",
            "app.adtype",
            "app.contenturi",
            "app.contentid",
            "app.serverside",
            "app.playerstate",
            "app.widthheight",
            "app.creatizesize",
            "app.EPSILON_CREATIVE_ID",
            "app.DMC_PLACEMENT_ID",
            "app.EPSILON_TRANSACTION_ID",
            "app.EPSILON_CORRELATION_USER_DATA",
            "app.omidpartner",
            "imp.tagid",
            "us_privacy",
            "site.page",
            "site.domain"
        };

        public static void CheckForMissingParameters (SettingsDictionary<string, string> settingsDictionary)
        {
            if (settingsDictionary == null) return;

            List<string> parameterList = new List<string>(oldParametersList);
            StringBuilder missingKeys = new StringBuilder();

            foreach (string key in parameterList)
            {
                if (!settingsDictionary.ContainsKey(key))
                {
                    missingKeys.AppendLine("Parameter: " + key);
                }
            }

            if (missingKeys.Length != 0)
            {
                MonetizrLogger.Print("Missing Settings: " + "\n\n" + missingKeys);
            }
        }

        public static SettingsDictionary<string, string> TEMPORARY_ConvertNewParameters (SettingsDictionary<string, string> settingsDictionary)
        {
            if (settingsDictionary == null || settingsDictionary.Count == 0) return settingsDictionary;

            SettingsDictionary<string, string> convertedList = new SettingsDictionary<string, string>(settingsDictionary);
            List<string> keysToRemove = new List<string>();

            for (int i = 0; i < newParametersList.Count; i++)
            {
                string newKey = newParametersList[i];
                string oldKey = oldParametersList[i];

                if (newKey == oldKey) continue;

                if (convertedList.TryGetValue(newKey, out string value))
                {
                    convertedList[oldKey] = value;
                    keysToRemove.Add(newKey);
                }
            }

            if (keysToRemove.Count > 0) for (int i = 0; i < keysToRemove.Count; i++) convertedList.Remove(keysToRemove[i]);

            return convertedList;
        }
    }
}