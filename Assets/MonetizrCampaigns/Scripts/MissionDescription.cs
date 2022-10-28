using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monetizr.Campaigns
{
    [Flags]
    public enum MissionType : uint
    {
        Undefined = 0,
        VideoReward = 1,
        MutiplyReward = 2,
        SurveyReward = 4,
        TwitterReward = 8,
        //GiveawayWithMail = 16,
        VideoWithEmailGiveaway = 32,
        MinigameReward = 64,
        All = uint.MaxValue,
    }

    public class MissionDescription
    {
        internal MissionType missionType;
        internal int reward;
        internal RewardType rewardCurrency;
        internal RangeInt activateAfter = new RangeInt(-1,0);
        internal string surveyUrl;

        public MissionDescription(int reward, RewardType rewardCurrency)
        {
            this.missionType = MissionType.VideoWithEmailGiveaway;
            this.reward = reward;
            this.rewardCurrency = rewardCurrency;
        }

        public MissionDescription(MissionType missionType, int reward, RewardType rewardCurrency)
        {
            this.missionType = missionType;
            this.reward = reward;
            this.rewardCurrency = rewardCurrency;
        }

        public MissionDescription(MissionType missionType, int reward, RewardType rewardCurrency, RangeInt activateAfter, string surveyUrl)
        {
            this.missionType = missionType;
            this.reward = reward;
            this.rewardCurrency = rewardCurrency;
            this.activateAfter = activateAfter;
            this.surveyUrl = surveyUrl;
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

        internal void Clear()
        {
            dictionary.Clear();
        }

        internal int RemoveAllByValue(Func<TValue, bool> predicate)
        {
            int count = 0;
            foreach (var item in dictionary.Where(kvp => predicate(kvp.Value)).ToList())
            {
                count++;
                dictionary.Remove(item.Key);
            }

            return count;
        }

        public TValue this[TKey k]
        {
            get => dictionary[k];
            set => dictionary[k] = value;
        }

        public bool ContainsKey(TKey k)
        {
            return dictionary.ContainsKey(k);
        }
    }

    public class SettingsDictionary<TKey, TValue>
    {
        public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public SettingsDictionary()
        {

        }

        public SettingsDictionary(Dictionary<TKey, TValue> d)
        {
            dictionary = d;
        }

        public TValue GetParam(TKey p)
        {
            if(p == null)
                return default(TValue);

            if (!dictionary.ContainsKey(p))
                return default(TValue);

            return dictionary[p];
        }

        public bool GetBoolParam(TKey p, bool defaultParam)
        {
            if (p == null)
                return defaultParam;

            if (!dictionary.ContainsKey(p))
                return defaultParam;

            Boolean result = defaultParam;
            string val = dictionary[p].ToString();

            if (!Boolean.TryParse(val, out result))
            {
                return defaultParam;
            }

            return result;
        }

        public int GetIntParam(TKey p, int defaultParam = 0)
        {
            if (p == null)
                return defaultParam;

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


        //how much times RC is offered to show instead of rewarded video
        [NonSerialized] internal int amountOfRVOffersShown;

        //[NonSerialized] internal int amountOfNotificationsShown;

        [NonSerialized] internal int amountOfNotificationsSkipped;

        [SerializeField] internal bool isDisabled;

        //is video shown already and don't need to be shown again
        [NonSerialized] internal bool isVideoShown;

        [SerializeField] internal bool hasVideo;

        [NonSerialized] internal bool isShouldBeDisabled;

        [NonSerialized] internal MissionUIState state;

        //Do we have this campaign active on the server now or it's just in a local cache
        [NonSerialized] internal bool isServerCampaignActive;

        //Field for campaign 
        //[SerializeField] internal SerializableDictionary<string, string> additionalParams;

        [NonSerialized] internal SettingsDictionary<string, string> campaignServerSettings;

        [SerializeField] internal string sdkVersion;

        //Integer ids shows when this missions should be activated (maybe it's better to convert into list)
        [NonSerialized] internal RangeInt activateAfter;

        [NonSerialized] internal bool isToBeRemoved;
    }

    /*internal class  SurveyMission : Mission
    {

    }*/


    

}