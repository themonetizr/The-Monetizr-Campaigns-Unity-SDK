using System;
using UnityEngine;

namespace Monetizr.SDK
{
    internal partial class MonetizrHttpClient
    {
        [Serializable]
        internal class IpApiData
        {
            public string country_name;
            public string country_code;
            public string region_code;

            public static IpApiData CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<IpApiData>(jsonString);
            }
        }
    }
}