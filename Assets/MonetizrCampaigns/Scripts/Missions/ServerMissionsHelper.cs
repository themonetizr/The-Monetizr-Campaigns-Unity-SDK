using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK.Missions
{
    internal partial class MissionsManager
    {
        [Serializable]
        internal class ServerMissionsHelper
        {
            public List<ServerDefinedMissions> missions = new List<ServerDefinedMissions>();
            
            internal List<MissionDescription> CreateMissionDescriptions(List<MissionDescription> originalList, SettingsDictionary<string, string> serverSettings)
            {
                List<MissionDescription> m = new List<MissionDescription>();

                foreach(var _m in missions)
                {
                    MissionType serverMissionType = _m.GetMissionType();

                    if (serverMissionType == MissionType.Undefined) continue;

                    float rewardAmount = _m.GetRewardAmount() / 100.0f;
                    RewardType currency = _m.GetRewardType();

                    GameReward gameReward = MonetizrManager.GetGameReward(currency);

                    if (gameReward == null)
                    {
                        if (serverSettings.GetBoolParam("use_default_reward", true))
                        {
                            currency = RewardType.Coins;
                            gameReward = MonetizrManager.GetGameReward(currency);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (rewardAmount > 100.0f) continue;

                    ulong rewardAmount2 = (ulong)Math.Ceiling(gameReward.maximumAmount * rewardAmount);

                    MonetizrLog.Print($"CreateMissionDescriptions:max:{gameReward.maximumAmount}:real:{rewardAmount2}:percent:{rewardAmount}");

                    m.Add(new MissionDescription
                    {
                        missionType = _m.GetMissionType(),
                        reward = rewardAmount2,
                        rewardCurrency = currency,
                        activateAfter = _m.GetActivateRange(),
                        surveyUrl = serverSettings.GetParam(_m.survey),
                        surveyId = string.IsNullOrEmpty(_m.surveyUnity) ? _m.survey : _m.surveyUnity,
                        hasUnitySurvey = !string.IsNullOrEmpty(_m.surveyUnity),
                        rewardPercent = rewardAmount,
                        id = _m.getId(),
                        alwaysHiddenInRC = _m.IsAlwaysHiddenInRC(),
                        autoStartAfter = _m.GetAutoStartId(),
                        rewardImage = _m.reward_image,
                        activateConditions = _m.activate_conditions,
                        openRtbRequestForProgrammatic = _m.ortb_request
                    }); ;

                }

                return m;
            }

            public static ServerMissionsHelper CreateFromJson(string json)
            {
                var result = new ServerMissionsHelper();

                if (!MonetizrUtils.ValidateJson(json))
                    return result;

                json = MonetizrUtils.UnescapeJson(json);

                if(json.Contains("\'"))
                    json = json.Replace('\'', '\"');

                try
                {
                    result = JsonUtility.FromJson<ServerMissionsHelper>(json);
                }
                catch (Exception)
                {
                    throw;
                }

                return result;
            }

        }

    }

}