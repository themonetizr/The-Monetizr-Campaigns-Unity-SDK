using System.Collections;
using System.Collections.Generic;

public static class ErrorDictionary
{
    public static Dictionary<int, string> errorDictionary = new Dictionary<int, string>
    {
        // Local Errors
        {100, "SDK Setup Incomplete - Please, verify and provide the missing parameters."},
        {101, "Missing SDK Settings - API Key. Please, provide API Key through ProjectSettings -> Monetizr."},
        {102, "Missing SDK Settings - Bundle ID. Please, provide Bundle ID through ProjectSettings -> Monetizr."},
        {103, "Missing SDK Settings - GameReward. Please, setup at least one Game Reward before SDK initialization."},
        {104, "Missing SDK Settings - Advertising ID. Please, call MonetizrManager.SetAdvertisingIds before SDK initialization."},
        {105, "Unable to reach network - Please, make sure that Network access is reacheable."},

        // Remote Log Errors
        {200, "Test Error."},
        {201, "SDK has not been correctly initialized."},
        {202, "Network Error - Global Settings were not obtained."},
        {203, "Network Error - Campaigns were not obtained."},
        {204, "Campaign Error - A Campaign has failed loading."},
        {205, "Network Error - An Asset has failed downloading."},

        // Remote Log Info
        {300, "SDK succesfully initialized."},
        {301, "Global Settings succesfully downloaded."},
        {302, "Campaigns succesfully downloaded."},
        {303, "No campaigns available."},
        {304, "Campaign succesfully loaded."},
        {305, "SDK application ended."},
    };

    public static string GetErrorMessage (int errorCode)
    {
        return errorDictionary[errorCode];
    }
}
