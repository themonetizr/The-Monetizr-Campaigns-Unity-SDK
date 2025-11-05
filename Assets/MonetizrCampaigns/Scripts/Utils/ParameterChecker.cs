using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monetizr.SDK.Utils
{
    public static class ParameterChecker
    {
        private static List<string> globalSettingsParameters = new List<string>()
        {
            "bg_color",
            "bg_color2",
            "link_color",
            "text_color",
            "videoplayer",
            "unity_logging",
            "design_version",
            "bg_border_color",
            "settings_global",
            "campaign.use_adm",
            "amount_of_teasers",
            "amount_of_notifications",
            "mixpanel_proxy_endpoint",
            "StartNotification.header_text",
            "assets"
        };

        private static List<string> campaignSettingsParameters = new List<string>()
        {
            "bg_color",
            "bg_color2",
            "link_color",
            "text_color",
            "videoplayer",
            "unity_logging",
            "design_version",
            "bg_border_color",
            "settings_global",
            "campaign.use_adm",
            "amount_of_teasers",
            "amount_of_notifications",
            "mixpanel_proxy_endpoint",
            "StartNotification.header_text",
            "test_name",
            "programmatic",
            "openrtb.delay",
            "brand_name_text",
            "custom_missions",
            "openrtb.request",
            "ActionReward.url",
            "openrtb.endpoint",
            "product_name_text",
            "show_teaser_button",
            "email_giveaway_type",
            "teaser_no_animation",
            "RewardCenter.bg_color",
            "show_reward_on_teaser",
            "teaser_design_version",
            "watch_video_only_once",
            "TinyMenuTeaser.bg_color",
            "no_main_menu_notifications",
            "show_campaigns_notification",
            "teaser_no_texture_animation",
            "no_start_level_notifications",
            "notifications_delay_time_sec",
            "StartNotification.button_text",
            "startup_skipped_notifications",
            "RewardCenter.reward_text_color",
            "StartNotification.content_text",
            "TinyMenuTeaser.bg_border_color",
            "email_enter_close_button_delay",
            "amount_of_skipped_notifications",
            "CongratsNotification.button_text",
            "CongratsNotification.header_text",
            "CongratsNotification.content_text",
            "RewardCenter.show_for_one_mission",
            "email_giveaway_mission_without_video",
            "EmailEnterCloseConfirmation.button_text",
            "EmailEnterCloseConfirmation.header_text",
            "EmailEnterCloseConfirmation.content_text",
            "GiveawayEmailEnterNotification.terms_text",
            "RewardCenter.do_not_claim_and_hide_missions",
            "is_campaign_html",
            "claim_button_html",
            "close_button_html",
        };

        public static void CheckForMissingParameters (bool isGlobal, SettingsDictionary<string, string> settingsDictionary)
        {
            List<string> parameterList = new List<string>(isGlobal ? globalSettingsParameters : campaignSettingsParameters);
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
                MonetizrLogger.Print((isGlobal ? "Missing Global Settings: " : "Missing Campaign Settings: ") + "\n\n" + missingKeys);
            }
        }
    }
}