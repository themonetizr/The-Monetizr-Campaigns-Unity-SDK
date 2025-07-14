using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Missions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Monetizr.SDK.Core
{
    internal abstract class MonetizrClient
    {
        internal string currentApiKey;
        public string userAgent;

        internal MonetizrMobileAnalytics Analytics { get; set; } = null;
        internal SettingsDictionary<string, string> GlobalSettings { get; set; } = new SettingsDictionary<string, string>();

        internal void SetUserAgent(string _userAgent) { this.userAgent = _userAgent; }

        internal virtual void Close() { }

        internal abstract Task GetGlobalSettings();

        internal abstract Task<List<ServerCampaign>> GetList();

        internal abstract Task ClaimReward(ServerCampaign challenge, CancellationToken ct, Action onSuccess = null, Action onFailure = null);

        internal abstract Task ResetCampaign(string campaignId, CancellationToken ct, Action onSuccess = null, Action onFailure = null);

        internal abstract void Initialize();

        internal virtual void SetTestMode(bool testMode) {}

        internal abstract Task<string> GetResponseStringFromUrl(string generatorUri);
    }

}