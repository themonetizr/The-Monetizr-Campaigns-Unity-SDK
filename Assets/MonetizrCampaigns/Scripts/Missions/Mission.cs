using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.UI;
using Monetizr.SDK.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK.Missions
{
    [Serializable]
    public class Mission
    {
        [SerializeField] internal string apiKey;
        [SerializeField] internal MissionType type;
        [SerializeField] internal ulong startMoney;
        [SerializeField] internal bool isSponsored;
        [SerializeField] internal string brandName;
        [SerializeField] internal ulong reward;
        [SerializeField] internal float rewardPercent;
        [NonSerialized] internal int sponsoredId;
        [NonSerialized] internal string missionTitle;
        [NonSerialized] internal string missionDescription;
        [NonSerialized] internal float progress;
        [NonSerialized] internal Action onClaimButtonPress;
        [NonSerialized] internal string claimButtonText;
        [SerializeField] internal string campaignId;
        [SerializeField] internal string surveyUrl;
        [SerializeField] internal string surveyId;
        [SerializeField] internal string claimedTime;
        [SerializeField] internal UDateTime deactivateTime;
        [SerializeField] internal UDateTime activateTime;
        [SerializeField] internal int delaySurveyTimeSec;
        [SerializeField] internal bool surveyAlreadyShown;
        [SerializeField] internal string brandId;
        [SerializeField] internal string appId;
        [SerializeField] internal RewardType rewardType;
        [SerializeField] internal string brandBannerUrl;
        [SerializeField] internal string brandLogoUrl;
        [SerializeField] internal string brandRewardBannerUrl;
        [SerializeField] internal int id;
        [SerializeField] internal int serverId;
        [SerializeField] internal ClaimState isClaimed;
        [SerializeField] internal bool isDisabled;
        [SerializeField] internal bool hasVideo;
        [NonSerialized] internal int amountOfRVOffersShown;
        [NonSerialized] internal int amountOfNotificationsSkipped;
        [NonSerialized] internal bool isDeactivatedByCondition;
        [NonSerialized] internal Dictionary<string, string> conditions;
        [NonSerialized] internal bool isVideoShown;
        [NonSerialized] internal bool isShouldBeDisabled;
        [NonSerialized] internal MissionUIState state;
        [NonSerialized] internal bool isServerCampaignActive;
        [NonSerialized] internal SettingsDictionary<string, string> campaignServerSettings;
        [SerializeField] internal string sdkVersion;
        [SerializeField] internal List<int> activateAfter;
        [NonSerialized] internal bool isToBeRemoved;
        [SerializeField] internal bool isRewardIngame;
        [NonSerialized] internal bool showHidden;
        [NonSerialized] internal MonetizrRewardedItem rewardCenterItem;
        [SerializeField] internal int autoStartAfter;
        [SerializeField] internal bool alwaysHiddenInRC;
        [SerializeField] internal bool hasUnitySurvey;
        [NonSerialized] internal AdPlacement adPlacement;
        [NonSerialized] internal ServerCampaign campaign;
        [NonSerialized] public string rewardAssetName;
        [NonSerialized] public string openRtbRequestForProgrammatic;
    }

}