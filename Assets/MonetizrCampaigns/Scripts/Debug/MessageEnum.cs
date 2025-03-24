using System.ComponentModel;

namespace Monetizr.SDK.Debug
{
    public enum MessageEnum
    {
        // Remote Messages
        [Description("SDK succesfully initialized.")] M100 = 100,
        [Description("Global Settings succesfully downloaded.")] M101 = 101,
        [Description("Campaigns succesfully downloaded.")] M102 = 102,
        [Description("No campaigns available.")] M103 = 103,
        [Description("Campaign succesfully loaded.")] M104 = 104,
        [Description("SDK application ended.")] M105 = 105,

        // Remote Errors
        [Description("Network Error - Global Settings were not obtained.")] M400 = 400,
        [Description("Network Error - Campaigns were not obtained.")] M401 = 401,
        [Description("Campaign Error - A Campaign has failed loading.")] M402 = 402,
        [Description("Network Error - An Asset has failed downloading.")] M403 = 403,
        [Description("SDK succesfully initialized.")] M404 = 404,

        // Local Errors
        [Description("SDK Setup Incomplete - Please, verify and provide the missing parameters.")] M600 = 600,
        [Description("Missing SDK Settings - API Key. Please, provide API Key through ProjectSettings -> Monetizr.")] M601 = 601,
        [Description("Missing SDK Settings - Bundle ID. Please, provide Bundle ID through ProjectSettings -> Monetizr.")] M602 = 602,
        [Description("Missing SDK Settings - GameReward. Please, setup at least one Game Reward before SDK initialization.")] M603 = 603,
        [Description("Missing SDK Settings - Advertising ID. Please, call MonetizrManager.SetAdvertisingIds before SDK initialization.")] M604 = 604,
        [Description("Unable to reach network - Please, make sure that Network access is reacheable.")] M605 = 605,
        [Description("Invalid GameReward Setup - Please, make sure that the Game Rewards setup is valid.")] M606 = 606,

    }
}