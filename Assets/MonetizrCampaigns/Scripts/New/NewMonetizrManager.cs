using Monetizr.SDK.Core;
using System.Collections;
using System.Collections.Generic;
using static Monetizr.SDK.Core.MonetizrManager;

namespace Monetizr.SDK.New
{
    public static class NewMonetizrManager
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
            if (string.IsNullOrEmpty(MonetizrConfiguration.apiKey))
            {
                UnityEngine.Debug.Log("Missing API Key.");
                return false;
            }

            if (string.IsNullOrEmpty(MonetizrConfiguration.bundleID))
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