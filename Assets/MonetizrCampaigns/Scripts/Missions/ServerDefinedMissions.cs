using Monetizr.SDK.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK.Missions
{
    [Serializable]
    public class ServerDefinedMissions
    {
        public string type;
        public string percent_amount;
        public string currency;
        public string activate_after;
        public string survey;
        public string surveyUnity;
        public string id;
        public string hidden_in_rc;
        public string auto_start_after;
        public string reward_image;
        public string activate_conditions;
        public string ortb_request;
        public string has_congrats;

        public int GetAutoStartId()
        {
            int res = -1;
            if (auto_start_after == null) return res;
            if (int.TryParse(auto_start_after, out res)) return res;
            return res;
        }

        public bool IsAlwaysHiddenInRC()
        {
            bool res = false;
            if (bool.TryParse(hidden_in_rc, out res)) return res;
            return res;
        }

        public bool HasCongrats()
        {
            bool res = true;
            if (bool.TryParse(has_congrats, out res)) return res;
            return res;
        }

        public int getId()
        {
            int res = -1;
            if (int.TryParse(id, out res)) return res;
            return -1;
        }

        public List<int> GetActivateRange()
        {
            List<int> result = new List<int>();
            if (activate_after == null) return result;

            string[] p = activate_after.Split(';');

            Array.ForEach(p, (string s) =>
            {
                int res = 0;
                if (int.TryParse(s, out res))
                {
                    result.Add(res);
                }
            });

            return result;
        }

        public RewardType GetRewardType()
        {
            RewardType rt;
            if (System.Enum.TryParse<RewardType>(currency, out rt)) return rt;
            return RewardType.Coins;
        }

        public float GetRewardAmount()
        {
            float reward = 0;
            if (float.TryParse(percent_amount, out reward)) return Mathf.Clamp(reward, 0, 100);
            return 0;
        }

        public MissionType GetMissionType()
        {
            MissionType mt;
            if (System.Enum.TryParse<MissionType>(type, out mt)) return mt;
            return MissionType.Undefined;
        }

    }

}