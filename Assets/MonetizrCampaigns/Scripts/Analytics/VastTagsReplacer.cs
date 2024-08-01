using System.Collections.Generic;
using System;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;

namespace Monetizr.SDK.Analytics
{
    internal class VastTagsReplacer : TagsReplacer
    {
        private readonly ServerCampaign _serverCampaign;

        internal VastTagsReplacer(ServerCampaign serverCampaign, Asset asset, string clientUA)
        {
            _serverCampaign = serverCampaign;
            var clientUa = clientUA;

            var urlModifiers = new Dictionary<string, Func<string>>()
            {
                {"APPBUNDLE", () => _serverCampaign.serverSettings.GetParam("app.bundleid",MonetizrManager.bundleId) },
                {"STOREID", () => _serverCampaign.serverSettings.GetParam("app.storeid","-1") },
                {"STOREURL", () => _serverCampaign.serverSettings.GetParam("app.storeurl","-1") },
                {"ADTYPE", () =>  _serverCampaign.serverSettings.GetParam("app.adtype","Video") },
                {"DEVICEIP", () => _serverCampaign.serverSettings.GetParam("app.deviceip",_serverCampaign.device_ip) },
                {"DEVICEUA", () => _serverCampaign.serverSettings.GetParam("app.deviceua",clientUa) },
                {"CONTENTURI", () =>  _serverCampaign.serverSettings.GetParam("app.contenturi",asset.url) },
                {"CONTENTID", () => _serverCampaign.serverSettings.GetParam("app.contentid","-1") },
                {"SERVERUA", () => _serverCampaign.serverSettings.GetParam("app.serverua","-1") },
                {"SERVERSIDE", () => _serverCampaign.serverSettings.GetParam("app.serverside","0") },
                {"CLIENTUA", () => _serverCampaign.serverSettings.GetParam("app.clientua",clientUa) },
                {"PLAYBACKMETHODS", () => _serverCampaign.serverSettings.GetParam("app.playbackmethods","1") },
                {"PLAYERSTATE", () =>  _serverCampaign.serverSettings.GetParam("app.playerstate","fullscreen") },
                {"IFA", () => MonetizrMobileAnalytics.advertisingID },
                {"ANDROID_DEVICE_ID", () => MonetizrMobileAnalytics.advertisingID },
                {"iOS_DEVICE_ID", () => MonetizrMobileAnalytics.advertisingID },
                {"IN_APP/MOBILE_WEB", () => "IN_APP" },
                {"ENTER_MOBILE_WEB_OR_IN_APP", () => "IN_APP" },
                {"WIDTHxHEIGHT", () => _serverCampaign.serverSettings.GetParam("app.widthheight","0x0") },
                {"ENTER_CREATIVE_SIZE", () => _serverCampaign.serverSettings.GetParam("app.creativesize","-1") },
                {"EPSILON_CREATIVE_ID", () => _serverCampaign.serverSettings.GetParam("app.EPSILON_CREATIVE_ID","-1") },
                {"DMC_PLACEMENT_ID", () => _serverCampaign.serverSettings.GetParam("app.DMC_PLACEMENT_ID","-1") },
                {"EPSILON_TRANSACTION_ID", () => _serverCampaign.serverSettings.GetParam("app.EPSILON_TRANSACTION_ID","-1") },
                {"EPSILON_CORRELATION_USER_DATA", () => _serverCampaign.serverSettings.GetParam("app.EPSILON_CORRELATION_USER_DATA","-1") },
                {"OMIDPARTNER", () => _serverCampaign.serverSettings.GetParam("app.omidpartner", _serverCampaign.vastSettings.vendorName) },
            };

            SetModifiers(urlModifiers);
        }

        protected override string UnknownModifier(string tag)
        {
            var value = _serverCampaign.serverSettings.GetParam($"app.{tag}");

            if (!string.IsNullOrEmpty(value))
                return value;

            MonetizrLog.PrintError($"Unknown VAST tag {tag}");
            return "-1";
        }

    }

}
