using Monetizr.Campaigns;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Monetizr.Campaigns
{
    internal abstract class LocalSerializer<T> where T: BaseCollection
    {
        internal T data = default(T);
        internal abstract string GetDataKey();
        
        internal void LoadData()
        {
            data?.Clear();

            var jsonString = PlayerPrefs.GetString(GetDataKey(), "");

            if (jsonString.Length == 0)
                return;

            Log.Print($"Loading {GetDataKey()}: {jsonString}");

            data = JsonUtility.FromJson<T>(jsonString);
        }

        internal void SaveData()
        {
            string jsonString = JsonUtility.ToJson(data);

            Log.Print($"Saving {GetDataKey()}: {jsonString}");

            PlayerPrefs.SetString(GetDataKey(), jsonString);
        }

        internal void ResetData()
        {
            data?.Clear();

            PlayerPrefs.SetString(GetDataKey(), "");
        }
    }

    [Serializable]
    internal abstract class BaseCollection
    {
        internal abstract void Clear();
    }

    [Serializable]
    internal class MissionsCollection : BaseCollection
    {
        [SerializeField] internal List<Mission> missions = new List<Mission>();

        internal override void Clear()
        {
            missions.Clear();
        }
    };

    internal class MissionsSerializeManager : LocalSerializer<MissionsCollection>
    {
        //private readonly string dateTimeOffsetFormatString = "yyyy-MM-ddTHH:mm:sszzz";

        private MissionsCollection missionsCollection = null;

        internal MissionsSerializeManager()
        {
            missionsCollection = new MissionsCollection();

            data = missionsCollection;
        }

        internal override string GetDataKey()
        {
            return "missions";
        }

        internal List<Mission> GetMissions()
        {
            return data.missions;
        }

        internal void Load()
        {
            LoadData();

            int deleted = data.missions.RemoveAll((Mission m) => { return m.apiKey != MonetizrManager.Instance.GetCurrentAPIkey(); });
          
            deleted += data.missions.RemoveAll((Mission m) => { return m.sdkVersion != MonetizrManager.SDKVersion; });

            if(deleted > 0)
            {
                Log.Print($"Deleted {deleted} incorrect(wrong SDK version or api key) missions");
                SaveAll();
            }
        }

        internal void SaveAll()
        {
            SaveData();
        }

        internal void Add(Mission m)
        {
            data.missions.Add(m);
        }
               
        internal void Reset()
        {
            ResetData();
        }
        

    }

}