using Monetizr.Campaigns;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{

    internal class CampaignsSerializeManager
    {
        //private readonly string dateTimeOffsetFormatString = "yyyy-MM-ddTHH:mm:sszzz";

        [Serializable]
        internal class CampaignsCollection
        {
            [SerializeField] internal List<Mission> missions = new List<Mission>();
        };

        CampaignsCollection campaignsCollection = new CampaignsCollection();

        internal List<Mission> GetMissions()
        {
            return campaignsCollection.missions;
        }

        internal void Load()
        {
            campaignsCollection.missions.Clear();

            var jsonString = PlayerPrefs.GetString("campaigns", "");

            if (jsonString.Length == 0)
                return;

            Log.Print($"Loading campaigns: {jsonString}");

            campaignsCollection = JsonUtility.FromJson<CampaignsCollection>(jsonString);
        }

        internal void SaveAll()
        {
            string jsonString = JsonUtility.ToJson(campaignsCollection);

            Log.Print($"Saving campaigns: {jsonString}");

            PlayerPrefs.SetString("campaigns", jsonString);
        }

        internal void Add(Mission m)
        {
            campaignsCollection.missions.Add(m);
        }

       
        internal void Reset()
        {
            campaignsCollection.missions.Clear();

            ResetPlayerPrefs();
        }

        internal static void ResetPlayerPrefs()
        {
            PlayerPrefs.SetString("campaigns", "");
        }

    }

}