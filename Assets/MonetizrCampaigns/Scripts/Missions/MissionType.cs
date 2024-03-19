using System;

namespace Monetizr.Campaigns
{
    [Flags]
    public enum MissionType : uint
    {
        Undefined = 0,
        VideoReward = 1,
        MutiplyReward = 2,
        SurveyReward = 4,
        //TwitterReward = 8,
        //GiveawayWithMail = 16,
        VideoWithEmailGiveaway = 32,
        MinigameReward = 64,
        MemoryMinigameReward = 128,
        ActionReward = 256,
        CodeReward = 512,
        All = uint.MaxValue,
    }


}