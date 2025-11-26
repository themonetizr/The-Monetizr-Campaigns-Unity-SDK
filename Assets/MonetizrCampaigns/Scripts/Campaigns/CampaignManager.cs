using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Prebid;
using Monetizr.SDK.Utils;
using Monetizr.SDK.VAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        private async Task<List<ServerCampaign>> ProcessCampaignByType (List<ServerCampaign> campaigns)
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

                        case CampaignType.Fallback:
                            campaign = await ProcessFallbackCampaign(campaign);
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

        private async Task<ServerCampaign> ProcessFallbackCampaign (ServerCampaign campaign)
        {
            campaign.ParseContentStringIntoSettingsDictionary();
            ParameterChecker.CheckForMissingParameters(false, campaign.serverSettings);

            bool allowPrebid = campaign.serverSettings.GetBoolParam("allow_fallback_prebid", false);
            bool allowEndpoint = campaign.serverSettings.GetBoolParam("allow_fallback_endpoint", false);

            if (allowPrebid && string.IsNullOrEmpty(campaign.serverSettings.GetParam("prebid_host", "")))
            {
                MonetizrLogger.PrintWarning("Prebid flow skipped: no prebid_host set.");
                allowPrebid = false;
            }

            if (allowPrebid)
            {
                ServerCampaign prebidCampaign = await HandlePrebidFallback(campaign);
                if (prebidCampaign == null && allowEndpoint)
                {
                    MonetizrLogger.Print("CampaignID " + campaign.id + " - Prebid flow failed, will attempt Endpoint flow.");
                    campaign = await HandleEndpointFallback(campaign);
                }
                else
                {
                    campaign = prebidCampaign;
                }
            }
            else if (allowEndpoint)
            {
                campaign = await HandleEndpointFallback(campaign);
            }
            else
            {
                MonetizrLogger.PrintError("CampaignID " + campaign.id + " - No fallback allowed.");
                return null;
            }

            if (campaign == null) return null;

            campaign.isDirectVASTinjection = true;
            await campaign.PreloadVideoPlayerForFallback();
            MonetizrLogger.Print("CampaignID " + campaign.id + " - Marked for direct VAST injection.");
            campaign.campaignType = CampaignType.Fallback;
            return campaign;
        }

        private async Task<ServerCampaign> HandlePrebidFallback (ServerCampaign campaign)
        {
            string prebidJSON = campaign.serverSettings.GetParam("prebid_data", "");
            if (string.IsNullOrEmpty(prebidJSON))
            {
                MonetizrLogger.PrintError("Prebid - Data not found in campaign.");
                return null;
            }

            prebidJSON = MacroUtils.ExpandMacrosInText(prebidJSON, campaign);

            string prebidHost = campaign.serverSettings.GetParam("prebid_host", "");
            PrebidManager.InitializePrebid(prebidHost);

            string prebidResponse = await FetchPrebid(prebidJSON, prebidHost);
            if (string.IsNullOrEmpty(prebidResponse))
            {
                MonetizrLogger.PrintError("Prebid - Fetch returned null.");
                return null;
            }

            campaign.trackingURLs = TrackingUtils.ExtractAllTrackingStrings(prebidResponse);

            string extractedResponse = PrebidUtils.ExtractPrebidResponse(prebidResponse, out PrebidUtils.PrebidResponseType responseType);
            MonetizrLogger.Print("PrebidResponseType: " + responseType + " / ExtractedResponse: " + extractedResponse);

            switch (responseType)
            {
                case PrebidUtils.PrebidResponseType.VastUrl:
                    MonetizrLogger.Print($"Prebid - VAST URL");
                    string receivedVAST = await MonetizrHttpClient.DownloadVastXmlAsync(extractedResponse);
                    if (string.IsNullOrEmpty(receivedVAST))
                    {
                        MonetizrLogger.PrintError("Prebid - VAST not downloaded.");
                        return null;
                    }
                    campaign.adm = receivedVAST;
                    break;

                case PrebidUtils.PrebidResponseType.VastXml:
                    MonetizrLogger.Print($"Prebid - VAST XML");
                    campaign.adm = extractedResponse;
                    break;

                case PrebidUtils.PrebidResponseType.HtmlCreative:
                    MonetizrLogger.PrintWarning($"Prebid - HTML/MRAID Creative (Not Implemented)");
                    return null;

                case PrebidUtils.PrebidResponseType.CacheId:
                    MonetizrLogger.PrintWarning($"Prebid - Cache ID (Not Implemented)");
                    return null;

                default:
                    MonetizrLogger.PrintError("Prebid - Response not usable (empty/unknown).");
                    return null;
            }

            return campaign;
        }

        private async Task<ServerCampaign> HandleEndpointFallback (ServerCampaign campaign)
        {
            string rawEndpoints = campaign.serverSettings.GetParam("endpoints", "");
            List<string> endpoints = PrebidUtils.ParseEndpoints(rawEndpoints);

            if (endpoints.Count <= 0)
            {
                MonetizrLogger.PrintError("Endpoint - No base URLs provided or correctly parsed.");
                return null;
            }

            foreach (string baseUrl in endpoints)
            {
                string endpointURL = NetworkingUtils.BuildEndpointURL(campaign, baseUrl);
                if (string.IsNullOrEmpty(endpointURL)) continue;

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, endpointURL);
                (bool isSuccess, string content) = await MonetizrHttpClient.DownloadUrlAsString(requestMessage);

                if (!isSuccess || string.IsNullOrEmpty(content))
                {
                    MonetizrLogger.PrintWarning("Endpoint failed: " + baseUrl);
                    continue;
                }

                string extracted = PrebidUtils.ExtractEndpointResponse(content, out PrebidUtils.EndpointResponseType responseType);
                MonetizrLogger.Print("EndpointResponseType: " + responseType + " / ExtractedResponse: " + extracted);

                switch (responseType)
                {
                    case PrebidUtils.EndpointResponseType.VastXml:
                        campaign.adm = extracted;
                        return campaign;

                    case PrebidUtils.EndpointResponseType.Playlist:
                        string receivedVAST = await MonetizrHttpClient.DownloadVastXmlAsync(extracted);
                        if (string.IsNullOrEmpty(receivedVAST))
                        {
                            MonetizrLogger.PrintError("Endpoint - Failed to download VAST from Playlist URL.");
                            break;
                        }

                        campaign.adm = receivedVAST;
                        return campaign;

                    default:
                        MonetizrLogger.Print("Endpoint - Response not usable (empty/error/unknown) at: " + baseUrl);
                        break;
                }
            }

            MonetizrLogger.PrintError("All endpoint fallbacks failed.");
            return null;
        }

        private async Task<string> FetchPrebid (string prebidData, string prebidHost, int timeoutMs = 3000)
        {
            MonetizrLogger.PrintWarning("Prebid - Fetching with: " + prebidData);

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            PrebidManager.FetchDemand(prebidData, prebidHost, keywordsJson =>
            {
                MonetizrLogger.Print($"Prebid - Result length = {keywordsJson?.Length ?? 0}");
                tcs.TrySetResult(keywordsJson);
            });

            Task delayTask = Task.Delay(timeoutMs);
            Task completedTask = await Task.WhenAny(tcs.Task, delayTask);

            if (completedTask == delayTask)
            {
                MonetizrLogger.PrintError("Prebid - Timeout waiting for response.");
                return null;
            }

            return await tcs.Task;
        }

        private async Task<List<ServerCampaign>> ProcessAssets (List<ServerCampaign> campaigns)
        {
            foreach (ServerCampaign campaign in campaigns)
            {
                try
                {
                    MonetizrManager.Instance.ConnectionsClient.Analytics.TrackEvent(campaign, null, AdPlacement.AssetsLoadingStarts, EventType.Notification);
                    await campaign.LoadCampaignAssets();

                    if (campaign.isLoaded)
                    {
                        MonetizrLogger.Print($"CampaignID: {campaign.id} successfully loaded", true);
                        MonetizrManager.Instance.ConnectionsClient.Analytics.TrackEvent(campaign, null, AdPlacement.AssetsLoadingEnds, EventType.Notification);
                    }
                    else
                    {
                        throw new Exception($"CampaignID: {campaign.id} asset loading failed with error: {campaign.loadingError}");
                    }
                }
                catch
                {
                    campaign.isLoaded = false;
                    MonetizrLogger.PrintError("CampaignID: " + campaign.id + " failed loading assets.", true);
                    MonetizrManager.Instance.ConnectionsClient.Analytics.TrackEvent(campaign, null, AdPlacement.AssetsLoading, EventType.Error, new Dictionary<string, string> { { "loading_error", campaign.loadingError } });
                }
            }

            campaigns.RemoveAll(c => c.isLoaded == false);
            return campaigns;
        }
    }
}