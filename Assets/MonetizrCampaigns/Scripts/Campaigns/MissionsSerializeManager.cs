using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using System.Collections.Generic;

namespace Monetizr.SDK.Campaigns
{
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