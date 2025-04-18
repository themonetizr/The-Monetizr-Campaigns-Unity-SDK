﻿using Monetizr.SDK.Debug;
using UnityEngine;

namespace Monetizr.SDK.Campaigns
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

            MonetizrLogger.Print($"Loading {GetDataKey()}: {jsonString}");

            data = JsonUtility.FromJson<T>(jsonString);
        }

        internal void SaveData()
        {
            string jsonString = JsonUtility.ToJson(data);

            MonetizrLogger.Print($"Saving {GetDataKey()}: {jsonString}");

            PlayerPrefs.SetString(GetDataKey(), jsonString);
        }

        internal void ResetData()
        {
            data?.Clear();

            PlayerPrefs.SetString(GetDataKey(), "");
        }

    }

}