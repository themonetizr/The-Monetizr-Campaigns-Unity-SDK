//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using UnityEngine;
using System;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Missions;


namespace Monetizr.SDK
{
    [Serializable]
    internal class LocalCampaignSettings
    {
        [SerializeField] internal string campId;
        [SerializeField] internal string apiKey;
        [SerializeField] internal string sdkVersion;
        [SerializeField] internal UDateTime lastTimeShowNotification;
        [SerializeField] internal int amountNotificationsShown;
        [SerializeField] internal int amountTeasersShown;

        [SerializeField] internal SerializableDictionary<string, string> settings = new SerializableDictionary<string, string>();
    }

}