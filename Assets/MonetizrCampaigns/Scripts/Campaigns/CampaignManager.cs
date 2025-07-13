using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Utils;
using Monetizr.SDK.VAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using EventType = Monetizr.SDK.Core.EventType;

namespace Monetizr.SDK.Campaigns
{
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance;

        private void Awake ()
        {
            Instance = this;
        }

        public async Task<List<ServerCampaign>> ProcessCampaigns (List<ServerCampaign> campaigns)
        {
            campaigns = await ProcessCampaignByType(campaigns);
            campaigns = campaigns.Where(c => c != null).ToList();
            campaigns = CampaignUtils.FilterInvalidCampaigns(campaigns);
            campaigns = await ProcessAssets(campaigns);
            return campaigns;
        }

        private async Task<List<ServerCampaign>> ProcessCampaignByType(List<ServerCampaign> campaigns)
        {
            for (int i = 0; i < campaigns.Count; i++)
            {
                ServerCampaign campaign = campaigns[i];

                try
                {
                    CampaignUtils.SetupCampaignType(campaign);
                    MonetizrLogger.Print($"CampaignID: {campaign.id} / Type: {campaign.campaignType}\n" + CampaignUtils.PrintAssetsTypeList(campaign));

                    switch (campaign.campaignType)
                    {
                        case CampaignType.MonetizrBackend:
                            campaign = ProcessBackendCampaign(campaign);
                            break;

                        case CampaignType.ADM:
                            campaign = await ProcessADMCampaign(campaign);
                            break;

                        case CampaignType.Programmatic:
                            campaign = await ProcessProgrammaticCampaign(campaign);
                            break;

                        default:
                            MonetizrLogger.PrintError($"CampaignID: {campaign.id} - No CampaignType was assigned.");
                            break;
                    }

                    campaigns[i] = campaign;
                }
                catch (Exception ex)
                {
                    MonetizrLogger.PrintError($"CampaignID: {(campaign?.id ?? "unknown")} processing failed: {ex.Message}");
                    campaigns[i] = null;
                }
            }

            return campaigns.Where(c => c != null).ToList();
        }

        private ServerCampaign ProcessBackendCampaign (ServerCampaign campaign)
        {
            campaign.ParseContentStringIntoSettingsDictionary();
            ParameterChecker.CheckForMissingParameters(false, campaign.serverSettings);
            return campaign;
        }

        private async Task<ServerCampaign> ProcessADMCampaign (ServerCampaign campaign)
        {
            campaign = await RecreateCampaignFromADM(campaign);
            if (campaign == null) return null;

            campaign.ParseContentStringIntoSettingsDictionary();
            campaign.campaignTimeoutStart = Time.time;
            campaign.hasMadeEarlyBidRequest = true;
            ParameterChecker.CheckForMissingParameters(false, campaign.serverSettings);

            return campaign;
        }

        private async Task<ServerCampaign> ProcessProgrammaticCampaign (ServerCampaign campaign)
        {
            campaign.ParseContentStringIntoSettingsDictionary();
            campaign = await MakeEarlyProgrammaticBidRequest(campaign);
            if (campaign == null || !campaign.hasMadeEarlyBidRequest) return null;

            campaign.campaignTimeoutStart = Time.time;
            campaign = await RecreateCampaignFromADM(campaign);
            ParameterChecker.CheckForMissingParameters(false, campaign.serverSettings);

            return campaign;
        }

        private async Task<ServerCampaign> RecreateCampaignFromADM (ServerCampaign campaign)
        {
            PubmaticHelper pubmaticHelper = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");
            campaign = await pubmaticHelper.PrepareServerCampaign(campaign.id, campaign.adm, false);
            return campaign;
        }

        private async Task<ServerCampaign> MakeEarlyProgrammaticBidRequest (ServerCampaign campaign)
        {
            PubmaticHelper pubmaticHelper = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");
            bool isProgrammaticOK = false;

            try
            {
                isProgrammaticOK = await pubmaticHelper.GetOpenRTBResponseForCampaign(campaign);
            }
            catch (DownloadUrlAsStringException e)
            {
                MonetizrLogger.PrintError($"EarlyBidRequest - Exception DownloadUrlAsStringException in campaign {campaign.id}\n{e}");
                isProgrammaticOK = false;
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintError($"EarlyBidRequest - Exception in GetOpenRtbResponseForCampaign in campaign {campaign.id}\n{e}");
                isProgrammaticOK = false;
            }

            MonetizrLogger.Print(isProgrammaticOK ? "EarlyBidRequest - COMPLETED" : "EarlyBidRequest - FAILED");
            campaign.hasMadeEarlyBidRequest = isProgrammaticOK;
            return campaign;
        }

        private async Task<List<ServerCampaign>> ProcessAssets (List<ServerCampaign> campaigns)
        {
            foreach (ServerCampaign campaign in campaigns)
            {
                try
                {
                    MonetizrManager.Instance.ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingStarts, EventType.Notification);
                    await campaign.LoadCampaignAssets();

                    if (campaign.isLoaded)
                    {
                        MonetizrLogger.PrintRemoteMessage(MessageEnum.M104);
                        MonetizrLogger.Print($"Campaign {campaign.id} successfully loaded");
                        MonetizrManager.Instance.ConnectionsClient.Analytics.TrackEvent(campaigns[0], null, AdPlacement.AssetsLoadingEnds, EventType.Notification);
                    }
                    else
                    {
                        throw new Exception($"Campaign {campaign.id} asset loading failed with error: {campaign.loadingError}");
                    }
                }
                catch
                {
                    campaign.isLoaded = false;
                    MonetizrLogger.PrintRemoteMessage(MessageEnum.M402);
                    MonetizrManager.Instance.ConnectionsClient.Analytics.TrackEvent(campaign, null, AdPlacement.AssetsLoading, EventType.Error, new Dictionary<string, string> { { "loading_error", campaign.loadingError } });
                }
            }

            campaigns.RemoveAll(c => c.isLoaded == false);
            return campaigns;
        }
    }
}