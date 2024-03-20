using Monetizr.SDK;
using MonetizrCampaigns.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Monetizr.SDK.ServerCampaign;

namespace MonetizrCampaigns.Tests
{
    internal class MonetizrTestClient : MonetizrClient
    {
        internal override void Close()
        {

        }

        internal override async Task GetGlobalSettings()
        {
            await Task.FromResult(0);
        }

        internal string GetSimpleCampaignAsString()
        {
            return @"
                ""bg_color"": ""#478EEB"",
                ""bg_color2"": ""#478EEB"",
                ""link_color"": ""#AAAAFF"",
                ""text_color"": ""#FFFFFF"",
                ""design_version"": ""2"",
                ""bg_border_color"": ""#8DCBF0"",
                ""settings_global"": ""true"",
                ""amount_of_teasers"": ""100"",
                ""amount_of_notifications"": ""100"",
                ""StartNotification.header_text"": ""Rewards by Monetizr!"",
                ""openrtb.endpoint"": ""https://openbid.pubmatic.com/translator?pubId=163063"",
                ""base_api_endpoint"": ""https://api-test.themonetizr.com"",
                ""mixpanel.testmode"": ""false"",
                ""openrtb.endpoint--"": ""http://localhost:8080/openrtb_json"",
                ""crash_reports.endpoint"": ""https://us-central1-gcp-monetizr-project.cloudfunctions.net/programmatic-serve/?log"",
                ""openrtb.send_by_client"": ""true"",
                ""openrtb.generator_url--"": ""https://programmatic-serve-stineosy7q-uc.a.run.app/?pmp&show_request=1&deal_id=PM-KSQA-9289&tag_id=5231582&video"",
                ""crash_reports.endpoint--"": ""https://api.raygun.com/entries"",
                ""openrtb.sent_report_to_slack"": ""true"",
                ""openrtb.sent_report_to_mixpanel"": ""true"",
                ""app.sent_error_reports_to_mixpanel"": ""true"",
                ""app.bundleid"": ""com.nextsol.workout.master"",
                ""programmatic"": ""true"",
                ""openrtb.delay"": ""10"",
                ""custom_missions"": ""{'missions': [{'type':'VideoReward','percent_amount':'100','id':'0'}]}"",
                ""min_sdk_version"": ""1.0.1"",
                ""StartNotification.button_text"": ""Learn more!"", 
                ""StartNotification.content_text"": ""Join Monetizr challenges < br /> to get game rewards"", 
                ""CongratsNotification.button_text"": ""Awesome!"", 
                ""CongratsNotification.header_text"": ""Get your reward!"", 
                ""CongratsNotification.content_text"": ""You have earned % ingame_reward % from Monetizr"",
                ""RewardCenter.show_for_one_mission"": ""false"", ""RewardCenter.VideoReward.content_text"":
                ""Watch video and get reward % ingame_reward % "", 
                ""RewardCenter.do_not_claim_and_hide_missions"": ""true""
               ";
        }

        private List<ServerCampaign> CreateSimpleVideoCampaigh()
        {
            Asset asset = new Asset()
            {
                id = "0000000",
                title = "logo",
                type = "logo",
                url = "https://image.themonetizr.com/challenge_asset/46b17dae-b015-4f5b-a42e-0f50719d2037.png"
            };

            ServerCampaign testCampaign = new ServerCampaign()
            {
                id = "",
                content = GetSimpleCampaignAsString(),
                assets = new List<Asset>() { asset }
            };

            return new List<ServerCampaign>() { testCampaign };
        }

        internal override async Task<List<ServerCampaign>> GetList()
        {
            return CreateSimpleVideoCampaigh();
        }

        internal override Task Claim(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            throw new NotImplementedException();
        }

        internal override Task Reset(string campaignId, CancellationToken ct, Action onSuccess = null, Action onFailure = null)
        {
            throw new NotImplementedException();
        }

        internal override void Initialize()
        {
            Analytics = new MonetizrTestAnalytics();
        }

        internal override void SetTestMode(bool testmode)
        {

        }

        internal override Task<string> GetStringFromUrl(string generatorUri)
        {
            throw new NotImplementedException();
        }
    }

}