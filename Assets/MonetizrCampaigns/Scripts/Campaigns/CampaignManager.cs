using System.Collections.Generic;
using UnityEngine;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Core;
using Monetizr.SDK.VAST;
using System.Threading.Tasks;
using System;

namespace Monetizr.SDK.Campaigns
{
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance;

        private void Awake ()
        {
            Instance = this;
        }

        public void ProcessCampaigns (List<ServerCampaign> campaigns)
        {
            for (int i = campaigns.Count - 1; i >= 0; i--)
            {
                CampaignUtils.SetupCampaignType(campaigns[i]);
                ProcessCampaignType(campaigns[i]);

                if (!CampaignUtils.IsCampaignValid(campaigns[i]))
                {
                    campaigns.RemoveAt(i);
                    continue;
                }

                ProcessAssets(campaigns[i]);
                if (!campaigns[i].isLoaded)
                {
                    campaigns.RemoveAt(i);
                    continue;
                }
            }
        }

        private void ProcessCampaignType (ServerCampaign campaign)
        {
            switch (campaign.campaignType)
            {
                case CampaignType.MonetizrBackend:
                    ProcessBackendCampaign(campaign);
                    break;
                case CampaignType.ADM:
                    ProcessADMCampaign(campaign);
                    break;
                case CampaignType.Programmatic:
                    ProcessProgrammaticCampaign(campaign);
                    break;
                default:
                    MonetizrLogger.PrintError("CampaignID: " + campaign.id + " - No CampaignType was assigned.");
                    break;
            }
        }

        private async void ProcessADMCampaign (ServerCampaign campaign)
        {
            //await RecreateCampaignsFromADM(campaign);
            campaign.ParseContentToSettingsDictionary();
        }

        private void ProcessBackendCampaign (ServerCampaign campaign)
        {
            campaign.ParseContentToSettingsDictionary();
        }

        private async void ProcessProgrammaticCampaign (ServerCampaign campaign)
        {
            campaign.ParseContentToSettingsDictionary();
            //await MakeProgrammaticBidRequest(campaign);
            //await RecreateCampaignsFromADM(campaign);
        }

        private async void ProcessAssets (ServerCampaign campaign)
        {
            await campaign.LoadCampaignAssets();

            if (campaign.isLoaded)
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M104);
                MonetizrLogger.Print($"Campaign {campaign.id} successfully loaded assets.");
            }
            else
            {
                MonetizrLogger.PrintRemoteMessage(MessageEnum.M402);
                MonetizrLogger.PrintError($"Campaign {campaign.id} loading assets failed with error {campaign.loadingError}.");
            }
        }

        internal async void RecreateCampaignsFromADM (ServerCampaign campaign)
        {
            PubmaticHelper pubmaticHelper = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");
            ServerCampaign admCampaign = await pubmaticHelper.PrepareServerCampaign(campaign.id, campaign.adm, false);
        }

        internal async void MakeProgrammaticBidRequest (ServerCampaign campaign)
        {
            MonetizrLogger.Print("PBR - Started");
            PubmaticHelper pubmaticHelper = new PubmaticHelper(MonetizrManager.Instance.ConnectionsClient, "");

            if (!String.IsNullOrEmpty(campaign.adm))
            {
                bool initializeResult = await pubmaticHelper.InitializeServerCampaignForProgrammatic(campaign, campaign.adm);
                campaign.hasMadeEarlyBidRequest = initializeResult;

                if (initializeResult)
                {
                    MonetizrLogger.Print("Programmatic with ADM initialization successful.");
                }
                else
                {
                    MonetizrLogger.Print("Programmatic with ADM initialization failed.");
                }

                return;
            }

            bool isProgrammaticOK = false;
            try
            {
                isProgrammaticOK = await pubmaticHelper.TEST_GetOpenRtbResponseForCampaign(campaign);
            }
            catch (DownloadUrlAsStringException e)
            {
                MonetizrLogger.PrintError($"PBR - Exception DownloadUrlAsStringException in campaign {campaign.id}\n{e}");
                isProgrammaticOK = false;
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintError($"PBR - Exception in GetOpenRtbResponseForCampaign in campaign {campaign.id}\n{e}");
                isProgrammaticOK = false;
            }

            MonetizrLogger.Print(isProgrammaticOK ? "PBR - COMPLETED" : "PBR - FAILED");
            campaign.hasMadeEarlyBidRequest = isProgrammaticOK;
        }
    }
}