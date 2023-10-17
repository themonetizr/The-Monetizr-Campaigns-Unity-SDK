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
        //TwitterReward = 8,
        //GiveawayWithMail = 16,
        VideoWithEmailGiveaway = 32,
        MinigameReward = 64,
        MemoryMinigameReward = 128,
        ActionReward = 256,
        All = uint.MaxValue,
    }

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
                throw new System.Exception(string.Format($"there are {keys.Count} keys and {values.Count} values after deserialization. Make sure that both key and value types are serializable."));

            for (var i = 0; i < keys.Count; i++)
                dictionary.Add(keys[i], values[i]);
        }

        public TValue GetParam(TKey p)
        {
            return dictionary.TryGetValue(p, out var value) ? value : default(TValue);
        }

        public int GetIntParam(TKey p, int defaultParam = 0)
        {
            if (!dictionary.ContainsKey(p))
                return defaultParam;

            var val = dictionary[p].ToString();

            if (!int.TryParse(val, out var result))
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
            get => GetParam(k);
            set => dictionary[k] = value;
        }

        public bool ContainsKey(TKey k)
        {
            return dictionary.ContainsKey(k);
        }
    }

    public class SettingsDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        //private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public SettingsDictionary()
        {

        }

        public SettingsDictionary(Dictionary<TKey, TValue> d) : base(d)
        {

        }

        public void MergeSettingsFrom(SettingsDictionary<TKey, TValue> addDictionary)
        {
            if (Count == 0)
                return;
            
            addDictionary?.ToList().ForEach(
                x => this[x.Key] = x.Value);
        }
        
        public TValue GetParam(TKey p, TValue def = default(TValue))
        {
            if(p == null)
                return def;

            return TryGetValue(p, out var value) ? value : def;
        }

        public bool GetBoolParam(TKey p, bool defaultParam)
        {
            if (p == null)
                return defaultParam;
            
            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }
            
            return bool.TryParse(value.ToString(), out var result) ? result : defaultParam;
        }

        public List<float> GetRectParam(TKey p, List<float> defaultParam)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }
            
            string val = value.ToString();

            var svals = val.Split(new char[] {';',','});

            var v = new List<float>(0);

            Array.ForEach(svals, s =>
            {
                if (!float.TryParse(s, out var f))
                    return;

                v.Add(f);
            });

            return v.Count == 4 ? v : defaultParam;
        }

        public int GetIntParam(TKey p, int defaultParam = 0)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }

            return int.TryParse(value.ToString(), out var result) ? result : defaultParam;
        }

        public float GetFloatParam(TKey p, float defaultParam = 0.0f)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }

            return float.TryParse(value.ToString(), out var result) ? result : defaultParam;
        }

    }

    [Serializable]
    public class Mission
    {
        [SerializeField] internal string apiKey;

        [SerializeField] internal MissionType type;
        [SerializeField] internal ulong startMoney;
   
        [SerializeField] internal bool isSponsored;
        [SerializeField] internal string brandName;
        [SerializeField] internal ulong reward;
        [SerializeField] internal float rewardPercent;

        [NonSerialized] internal int sponsoredId;
       // [NonSerialized] internal Sprite brandBanner;
        [NonSerialized] internal string missionTitle;
        [NonSerialized] internal string missionDescription;
        //[NonSerialized] internal Sprite missionIcon;
        [NonSerialized] internal float progress;
        [NonSerialized] internal Action onClaimButtonPress;
        //[NonSerialized] internal Sprite brandLogo;
        //[NonSerialized] internal Sprite brandRewardBanner;
        [NonSerialized] internal string claimButtonText;

        //Campaign id
        [SerializeField] internal string campaignId;

        //Survey url
        [SerializeField] internal string surveyUrl;
        [SerializeField] internal string surveyId;

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
        [SerializeField] internal int serverId;
        [SerializeField] internal ClaimState isClaimed;


        //how much times RC is offered to show instead of rewarded video
        [NonSerialized] internal int amountOfRVOffersShown;

        //[NonSerialized] internal int amountOfNotificationsShown;

        [NonSerialized] internal int amountOfNotificationsSkipped;

        [SerializeField] internal bool isDisabled;

        [NonSerialized] internal bool isDeactivatedByCondition;
        [NonSerialized] internal Dictionary<string, string> conditions;

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
        [SerializeField] internal List<int> activateAfter;

        [NonSerialized] internal bool isToBeRemoved;

        [SerializeField] internal bool isRewardIngame;

        [NonSerialized] internal bool showHidden;

        [NonSerialized] internal MonetizrRewardedItem rewardCenterItem;

        [SerializeField] internal int autoStartAfter;
        [SerializeField] internal bool alwaysHiddenInRC;
        [SerializeField] internal bool hasUnitySurvey;

        [NonSerialized] internal AdPlacement adPlacement;

        [NonSerialized] internal ServerCampaign campaign;
        [NonSerialized] public string rewardAssetName;
    }

    /*internal class  SurveyMission : Mission
    {

    }*/


    

}