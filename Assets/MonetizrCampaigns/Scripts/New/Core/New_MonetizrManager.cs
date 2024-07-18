using Monetizr.SDK.Core;
using System.Collections.Generic;

namespace Monetizr.SDK.New
{
    public static class New_MonetizrManager
    {
        private static Dictionary<RewardType, GameReward> gameRewards = new Dictionary<RewardType, GameReward>();

        public static void Initialize ()
        {
            if (IsInitializationSetupComplete())
            {
                UnityEngine.Debug.LogError("Initialization Setup Failed.");
                return;
            }
        }

        private static bool IsInitializationSetupComplete ()
        {
            if (string.IsNullOrEmpty(MonetizrSettings.apiKey))
            {
                UnityEngine.Debug.Log("Missing API Key.");
                return false;
            }

            if (string.IsNullOrEmpty(MonetizrSettings.bundleID))
            {
                UnityEngine.Debug.Log("Missing Bundle ID.");
                return false;
            }

            if (gameRewards == null || gameRewards.Count <= 0)
            {
                UnityEngine.Debug.Log("Missing Game Rewards.");
                return false;
            }

            return true;
        }
    }
}