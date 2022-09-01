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
        //GiveawayWithMail = 16,
        VideoWithEmailGiveaway = 32,
        All = uint.MaxValue,
    }

    public class MissionDescription
    {
        internal MissionType missionType;
        internal int reward;
        internal RewardType rewardCurrency;

        public MissionDescription(int reward, RewardType rewardCurrency)
        {
            this.missionType = MissionType.VideoWithEmailGiveaway;
            this.reward = reward;
            this.rewardCurrency = rewardCurrency;
        }
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
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        public SerializableDictionary()
        {

        }

        public SerializableDictionary(Dictionary<TKey,TValue> d)
        {
            dictionary = d;
        }

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                dictionary.Add(keys[i], values[i]);
        }

        public TValue GetParam(TKey p)
        {
            if (!dictionary.ContainsKey(p))
                return default(TValue);

            return dictionary[p];
        }

        public int GetIntParam(TKey p, int defaultParam = 0)
        {
            if (!dictionary.ContainsKey(p))
                return defaultParam;

            int result = 0;
            string val = dictionary[p].ToString();

            if (!Int32.TryParse(val, out result))
            {
                return defaultParam;
            }

            return result;
        }
    }

    [Serializable]
    public class Mission
    {
        [SerializeField] internal string apiKey;

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


        [NonSerialized] internal int amountOfNotificationsShown;

        [NonSerialized] internal int amountOfNotificationsSkipped;

        [SerializeField] internal bool isDisabled;

        [NonSerialized] internal bool isShouldBeDisabled;

        [NonSerialized] internal MissionUIState state;

        //Do we have this campaign active on the server now or it's just in a local cache
        [NonSerialized] internal bool isServerCampaignActive;

        //Field for campaign 
        [SerializeField] internal SerializableDictionary<string, string> additionalParams;

        
    }

    /*internal class  SurveyMission : Mission
    {

    }*/


    

}