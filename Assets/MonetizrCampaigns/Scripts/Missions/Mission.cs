using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
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
        // [NonSerialized] internal Sprite brandBanner;
        [NonSerialized] internal string missionTitle;
        [NonSerialized] internal string missionDescription;
        //[NonSerialized] internal Sprite missionIcon;
        [NonSerialized] internal float progress;
        [NonSerialized] internal Action onClaimButtonPress;
        //[NonSerialized] internal Sprite brandLogo;
        //[NonSerialized] internal Sprite brandRewardBanner;
        [NonSerialized] internal string claimButtonText;

        //Campaign id
        [SerializeField] internal string campaignId;

        //Survey url
        [SerializeField] internal string surveyUrl;
        [SerializeField] internal string surveyId;

        //Time, when campaign was claimed and this mission could be active
        [SerializeField] internal string claimedTime;

        [SerializeField] internal UDateTime deactivateTime;

        [SerializeField] internal UDateTime activateTime;


        //Delay, when campaign must be shown
        [SerializeField] internal int delaySurveyTimeSec;

        [SerializeField] internal bool surveyAlreadyShown;

        [SerializeField] internal string brandId;
        [SerializeField] internal string appId;

        //reward type - coin, premium currency, etc
        [SerializeField] internal RewardType rewardType;
        [SerializeField] internal string brandBannerUrl;
        [SerializeField] internal string brandLogoUrl;
        [SerializeField] internal string brandRewardBannerUrl;


        [SerializeField] internal int id;
        [SerializeField] internal int serverId;
        [SerializeField] internal ClaimState isClaimed;


        //how much times RC is offered to show instead of rewarded video
        [NonSerialized] internal int amountOfRVOffersShown;

        //[NonSerialized] internal int amountOfNotificationsShown;

        [NonSerialized] internal int amountOfNotificationsSkipped;

        [SerializeField] internal bool isDisabled;

        [NonSerialized] internal bool isDeactivatedByCondition;
        [NonSerialized] internal Dictionary<string, string> conditions;

        //is video shown already and don't need to be shown again
        [NonSerialized] internal bool isVideoShown;

        [SerializeField] internal bool hasVideo;

        [NonSerialized] internal bool isShouldBeDisabled;

        [NonSerialized] internal MissionUIState state;

        //Do we have this campaign active on the server now or it's just in a local cache
        [NonSerialized] internal bool isServerCampaignActive;

        [NonSerialized] internal SettingsDictionary<string, string> campaignServerSettings;

        [SerializeField] internal string sdkVersion;

        //Integer ids shows when this missions should be activated (maybe it's better to convert into list)
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