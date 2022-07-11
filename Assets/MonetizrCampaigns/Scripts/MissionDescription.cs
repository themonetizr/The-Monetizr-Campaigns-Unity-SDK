using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{
    [Flags]
    public enum MissionType : uint
    {
        VideoReward = 1,
        MutiplyReward = 2,
        SurveyReward = 4,
        TwitterReward = 8,
        GiveawayWithMail = 16,
        VideoWithEmailGiveaway = 32,
        All = uint.MaxValue,
    }

    public class MissionDescription
    {
        public MissionType missionType;
        public int reward;
        public RewardType rewardCurrency;
    }

    internal enum MissionUIState
    {
        ToBeShown,
        Showing,
        Visible,
        ToBeHidden,
        Hiding,
        Hidden,
    }

    internal enum ClaimState
    {
        NotClaimed,
        CompletedNotClaimed,
        Claimed
    }

    [Serializable]
    public class Mission
    {
        [SerializeField] internal MissionType type;
        [SerializeField] internal int startMoney;
   
        

        [SerializeField] internal bool isSponsored;
        [SerializeField] internal string brandName;
        [SerializeField] internal int reward;

        [NonSerialized] internal int sponsoredId;
        [NonSerialized] internal Sprite brandBanner;
        [NonSerialized] internal string missionTitle;
        [NonSerialized] internal string missionDescription;
        [NonSerialized] internal Sprite missionIcon;
        [NonSerialized] internal float progress;
        [NonSerialized] internal Action onClaimButtonPress;
        [NonSerialized] internal Sprite brandLogo;
        [NonSerialized] internal Sprite brandRewardBanner;
        [NonSerialized] internal string claimButtonText;

        //Campaign id
        [SerializeField] internal string campaignId;

        //Survey url
        [SerializeField] internal string surveyUrl;

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
        [SerializeField] internal ClaimState isClaimed;

        [SerializeField] internal bool isDisabled;

        [NonSerialized] internal bool isShouldBeDisabled;

        [NonSerialized] internal MissionUIState state;

    }

    /*internal class  SurveyMission : Mission
    {

    }*/


    

}