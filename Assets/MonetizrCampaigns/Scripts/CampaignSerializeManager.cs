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
        internal abstract string GetDataKey();

        internal T LoadData(T previousData)
        {
            Assert.IsNotNull(previousData);

            previousData.Clear();

            var jsonString = PlayerPrefs.GetString(GetDataKey(), "");

            if (jsonString.Length == 0)
                return previousData;

            Log.Print($"Loading {GetDataKey()}: {jsonString}");

            return JsonUtility.FromJson<T>(jsonString);
        }

        internal void SaveData(T data)
        {
            Assert.IsNotNull(data);

            string jsonString = JsonUtility.ToJson(data);

            Log.Print($"Saving {GetDataKey()}: {jsonString}");

            PlayerPrefs.SetString(GetDataKey(), jsonString);
        }

        internal void ResetData(T data)
        {
            Assert.IsNotNull(data);

            data.Clear();

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

        MissionsCollection missionsCollection = new MissionsCollection();

        internal override string GetDataKey()
        {
            return "missions";
        }

        internal List<Mission> GetMissions()
        {
            return missionsCollection.missions;
        }

        internal void Load()
        {
            missionsCollection = LoadData(missionsCollection);

            int deleted = missionsCollection.missions.RemoveAll((Mission m) => { return m.apiKey != MonetizrManager.Instance.GetCurrentAPIkey(); });
          
            deleted += missionsCollection.missions.RemoveAll((Mission m) => { return m.sdkVersion != MonetizrManager.SDKVersion; });

            if(deleted > 0)
            {
                Log.Print($"Deleted {deleted} incorrect(wrong SDK version or api key) missions");
                SaveAll();
            }
        }

        internal void SaveAll()
        {
            SaveData(missionsCollection);
        }

        internal void Add(Mission m)
        {
            missionsCollection.missions.Add(m);
        }
               
        internal void Reset()
        {
            ResetData(missionsCollection);
        }
        

    }

}