using Monetizr.SDK.Core;
using System.Collections.Generic;

namespace Monetizr.SDK.Missions
{
    public class MissionDescription
    {
        internal MissionType missionType;
        internal ulong reward;
        internal float rewardPercent;
        internal RewardType rewardCurrency;
        internal List<int> activateAfter = new List<int>();
        internal string surveyUrl;
        internal string surveyId;
        internal int id;
        internal int autoStartAfter;
        internal bool alwaysHiddenInRC;
        internal bool hasUnitySurvey;
        internal string rewardImage;
        internal string activateConditions;
        internal string openRtbRequestForProgrammatic;
    }

}