using System.Collections.Generic;
using UnityEngine;
using System;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.UI;
using EventType = Monetizr.SDK.Core.EventType;
using System.Collections;
using Monetizr.SDK.Utils;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using Monetizr.SDK.Rewards;


#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Monetizr.SDK.Analytics
{
    public static class MonetizrMobileAnalytics
    {
        public static string osVersion;
        public static string advertisingID = "";
        public static bool limitAdvertising = false;

        internal static DeviceSizeGroup deviceSizeGroup = DeviceSizeGroup.Unknown;
        internal static bool isAdvertisingIDDefined = false;
        internal static string deviceIdentifier = "";

        private static string defaultEndpointURL = "https://mixpanel-retirement-func-1074977689117.us-central1.run.app/";
        private static string currentEndpointURL = "";

        private static HashSet<AdElement> adNewElements = new HashSet<AdElement>();
        private static HashSet<AdElement> visibleAdAsset = new HashSet<AdElement>();

        public static void SetupAnalytics ()
        {
            LoadUserId();
            MonetizrLogger.Print($"MonetizrMobileAnalytics initialized with user id: {GetUserId()}");
            osVersion = "0.0";

#if !UNITY_EDITOR
#if UNITY_ANDROID
                   AndroidJavaClass versionInfo = new AndroidJavaClass("android/os/Build$VERSION");

                   osVersion = versionInfo.GetStatic<string>("RELEASE");
#elif UNITY_IOS
                   osVersion = UnityEngine.iOS.Device.systemVersion;
#endif
#endif
            deviceSizeGroup = AnalyticsUtils.GetDeviceGroup();
            MonetizrLogger.Print($"OS Version {osVersion} Ad id: {advertisingID} Limit ads: {limitAdvertising} Device group: {deviceSizeGroup}");
            currentEndpointURL = defaultEndpointURL;
        }

        public static void SetProxyEndpoint (string proxyEndpoint)
        {
            currentEndpointURL = proxyEndpoint;
        }

        internal static void OnApplicationQuit ()
        {
            foreach (AdElement ad in visibleAdAsset)
            {
                _EndShowAdAsset(ad);
            }
        }

        private static void BeginShowAdAsset (string eventName, AdPlacement placement, ServerCampaign campaign)
        {
            if (campaign == null)
            {
                MonetizrLogger.PrintWarning($"BeginShowAdAsset: MissionUIDescription shouldn't be null");
                return;
            }

            AdElement adElement = new AdElement(eventName, placement, campaign);

            if (visibleAdAsset.Contains(adElement))
            {
                MonetizrLogger.Print("Some ad asset are still showing.");
            }

            visibleAdAsset.Add(adElement);
            TrackEvent(eventName, null, null, timed: true);
        }

        private static void EndShowAdAsset (AdPlacement placement, ServerCampaign campaign, bool removeElement = true)
        {
            visibleAdAsset.RemoveWhere((AdElement a) =>
            {
                ServerCampaign c = campaign == null ? null : a.campaign;
                bool remove = a.placement == placement && c == campaign;
                if (remove) _EndShowAdAsset(a);
                return remove;
            });
        }

        private static void _EndShowAdAsset (AdElement adAsset)
        {
            string placementName = adAsset.placement.ToString();
            _TrackEvent(adAsset.eventName, adAsset.campaign, true, new Dictionary<string, string>() { { "type", placementName } });
        }

        internal static void LoadUserId ()
        {
            if (PlayerPrefs.HasKey("Monetizr.user_id"))
            {
                deviceIdentifier = PlayerPrefs.GetString("Monetizr.user_id");
            }
            else
            {
                deviceIdentifier = SystemInfo.deviceUniqueIdentifier;
            }

            PlayerPrefs.SetString("Monetizr.user_id", deviceIdentifier);
            PlayerPrefs.Save();
        }

        internal static void RandomizeUserId ()
        {
            char[] _deviceIdentifier = deviceIdentifier.ToCharArray();

            for (int i = 0; i < _deviceIdentifier.Length; i++)
            {
                char temp = _deviceIdentifier[i];
                int randomIndex = UnityEngine.Random.Range(i, deviceIdentifier.Length);
                if (temp == randomIndex && randomIndex == '-') continue;
                _deviceIdentifier[i] = _deviceIdentifier[randomIndex];
                _deviceIdentifier[randomIndex] = temp;
            }

            deviceIdentifier = new string(_deviceIdentifier);
            PlayerPrefs.SetString("Monetizr.user_id", deviceIdentifier);
            PlayerPrefs.Save();
        }

        internal static string GetUserId()
        {
            return deviceIdentifier;
        }

        internal static void TrackEvent(Mission currentMission, PanelController panel, EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            if (panel.GetAdPlacement() == null) return;
            AdPlacement adPlacement = panel.GetAdPlacement().Value;
            TrackEvent(currentMission.campaign, currentMission, adPlacement, eventType, additionalValues);
        }

        internal static void TrackEvent(Mission currentMission, AdPlacement adPlacement, EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            TrackEvent(currentMission.campaign, currentMission, adPlacement, eventType, additionalValues);
        }

        internal static void TrackEvent(ServerCampaign currentCampaign, Mission currentMission, AdPlacement adPlacement, EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            UnityEngine.Debug.Assert(currentCampaign != null);
            string placementName = AnalyticsUtils.GetPlacementName(adPlacement);
            MonetizrInstance.Instance.CallUserDefinedEvent(currentCampaign.id, placementName, eventType);

            if (additionalValues == null) additionalValues = new Dictionary<string, string>();
            if (currentMission != null) additionalValues["mission_id"] = currentMission.serverId.ToString();

            Dictionary<AdPlacement, string> eventNames = new Dictionary<AdPlacement, string>()
            {
                { AdPlacement.TinyTeaser,"Tiny teaser" },
                { AdPlacement.Minigame, "Minigame" },
                { AdPlacement.Survey, "Survey" },
                { AdPlacement.Html5, "Html5" },
                { AdPlacement.HtmlPage, "HtmlPage" },
                { AdPlacement.RewardsCenterScreen, "Reward center" },
                { AdPlacement.NotificationScreen, "Notification" },
                { AdPlacement.CongratsNotificationScreen, "Congrats screen" },
                { AdPlacement.SurveyNotificationScreen, "Survey notification" },
                { AdPlacement.EmailCongratsNotificationScreen, "Email congrats" },
                { AdPlacement.EmailEnterCouponRewardScreen, "Enter email" },
                { AdPlacement.EmailEnterInGameRewardScreen, "Enter email" },
                { AdPlacement.EmailEnterSelectionRewardScreen, "Enter email" },
                { AdPlacement.AssetsLoading, "Assets loading" },
                { AdPlacement.ActionScreen, "Action screen" },
                { AdPlacement.AssetsLoadingStarts, "Assets loading starts" },
                { AdPlacement.AssetsLoadingEnds, "Assets loading ends" },
            };

            string completedOrPressed(AdPlacement p)
            {
                switch (p)
                {
                    case AdPlacement.Minigame:
                    case AdPlacement.Survey:
                    case AdPlacement.Html5:
                    case AdPlacement.HtmlPage:
                    case AdPlacement.ActionScreen:
                        return "completed";

                    case AdPlacement.EmailEnterCouponRewardScreen:
                    case AdPlacement.EmailEnterInGameRewardScreen:
                    case AdPlacement.EmailEnterSelectionRewardScreen:
                        return "submitted";

                    default: break;
                }

                return "pressed";
            };

            Dictionary<EventType, string> eventTypes = new Dictionary<EventType, string>()
            {
                { EventType.ButtonPressOk, completedOrPressed(adPlacement) },
                { EventType.ButtonPressSkip, "skipped" },
                { EventType.Impression, "shown" },
                { EventType.Error, "failed" },
                { EventType.Notification, "notified" }
            };

            TrackNewEvents(currentCampaign, currentMission, adPlacement, placementName, eventType, additionalValues);

            if (!currentCampaign.serverSettings.GetBoolParam("send_old_events", false)) return;

            if (eventType == EventType.Impression)
            {
                NielsenDar.Track(currentCampaign, adPlacement);
                BeginShowAdAsset($"{adPlacement}", adPlacement, currentCampaign);
            }

            if (eventType == EventType.ImpressionEnds)
            {
                EndShowAdAsset(adPlacement, currentCampaign);
                return;
            }

            string eventName = $"{eventNames[adPlacement]} {eventTypes[eventType]}";
            _TrackEvent(eventName, currentCampaign, false, additionalValues);
        }

        internal static void TrackNewEvents (ServerCampaign campaign, Mission currentMission, AdPlacement adPlacement, string placementName, EventType eventType, Dictionary<string, string> additionalValues)
        {
            additionalValues.Add("placement", placementName);
            additionalValues.Add("placement_group", AnalyticsUtils.GetPlacementGroup(adPlacement));

            TrackOMSDKEvents(eventType, adPlacement, AnalyticsUtils.GetPlacementGroup(adPlacement));

            string eventName = "";
            bool timed = false;

            switch (adPlacement)
            {
                case AdPlacement.EmailEnterCouponRewardScreen:
                    additionalValues.Add("reward_type", "product");
                    break;

                case AdPlacement.EmailEnterInGameRewardScreen:
                    additionalValues.Add("reward_type", "ingame");
                    break;

                case AdPlacement.EmailEnterSelectionRewardScreen:
                    additionalValues.Add("reward_type", MonetizrInstance.Instance.temporaryRewardTypeSelection == RewardSelectionType.Ingame ? "ingame" : "product");
                    break;

                default: break;
            }

            double duration = 0.0;

            switch (eventType)
            {
                case EventType.ButtonPressOk:
                    eventName = "Action";
                    additionalValues.Add("action", "ok");
                    break;

                case EventType.ButtonPressSkip:
                    additionalValues.Add("action", "skip");
                    eventName = "Action";
                    break;

                case EventType.Impression:
                    eventName = "ImpressionStarts";
                    NielsenDar.Track(campaign, adPlacement);
                    adNewElements.Add(new AdElement("ImpressionEnds", adPlacement, campaign));
                    break;

                case EventType.ImpressionEnds:
                    timed = true;
                    eventName = "ImpressionEnds";

                    adNewElements.RemoveWhere((AdElement a) =>
                    {
                        if (adPlacement == a.placement && campaign == a.campaign)
                        {
                            duration = (DateTime.Now - a.activateTime).TotalSeconds;
                            return true;
                        }

                        return false;
                    });

                    break;

                case EventType.Error:
                case EventType.Notification:
                    eventName = eventType.ToString();
                    break;

            };

            _TrackEvent(eventName, campaign, timed, additionalValues, duration);
        }

        private static void TrackOMSDKEvents (EventType eventType, AdPlacement adPlacement, string placementGroup)
        {
            if (!AnalyticsUtils.CanTrackInOMSDK(adPlacement)) return;

            string resourceUrl = $"https://image.themonetizr.com/{placementGroup.ToLower()}.png";

            if (eventType == EventType.Impression)
            {
                UniWebViewInterface.InitOMSDKSession(resourceUrl);
                UniWebViewInterface.StartImpression(resourceUrl);
            }

            if (eventType == EventType.ImpressionEnds)
            {
                UniWebViewInterface.StopImpression(resourceUrl);
            }
        }

        internal static void _TrackEvent (string name, ServerCampaign campaign, bool timed = false, Dictionary<string, string> additionalValues = null, double duration = -1.0)
        {
            if (campaign == null)
            {
                MonetizrLogger.PrintWarning($"TrackEvent: ServerCampaign shouldn't be null");
                return;
            }

            string logString = $"SendEvent: {name}";
            if (additionalValues != null)
            {
                if (additionalValues.ContainsKey("placement")) logString += " placement:" + additionalValues["placement"];
                if (additionalValues.ContainsKey("placement_group")) logString += " group:" + additionalValues["placement_group"];
                if (additionalValues.ContainsKey("action")) logString += " action:" + additionalValues["action"];
                if (additionalValues.ContainsKey("$duration")) logString += " duration:" + additionalValues["$duration"];
            }

            logString += $" id:{campaign.id}";
            MonetizrLogger.Print(logString);
            string eventName = timed ? $"[UNITY_SDK] [TIMED] {name}" : $"[UNITY_SDK] {name}";
            TrackEvent(eventName, campaign, additionalValues, timed, duration);
        }

        public static void TrackEvent (string eventName, ServerCampaign campaign, Dictionary<string, string> additionalValues = null, bool timed = false, double duration = -1.0)
        {
            if (campaign == null)
            {
                MonetizrLogger.PrintWarning($"BackendAnalytics: ServerCampaign is null for {eventName}");
                return;
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            AddDefaultValues(props, campaign);

            if (additionalValues != null)
            {
                foreach (KeyValuePair<string, string> kv in additionalValues)
                {
                    props[kv.Key] = kv.Value;
                }
            }

            if (duration > 0.0) props["duration"] = duration;
            string finalEventName = timed ? $"[UNITY_SDK] [TIMED] {eventName}" : $"[UNITY_SDK] {eventName}";
            MonetizrLogger.Print($"SendEvent: {finalEventName}");

            JSONObject payload = new JSONObject();
            payload["event"] = finalEventName;
            payload["user_id"] = deviceIdentifier;
            payload["timestamp"] = DateTime.UtcNow.ToString("o");

            JSONObject propsJson = MonetizrUtils.DictionaryToJson(props);
            payload["properties"] = propsJson;

            string json = payload.ToString();
            MonetizrInstance.Instance.StartCoroutine(Send(json));
        }

        private static void AddDefaultValues(Dictionary<string, object> props, ServerCampaign campaign)
        {
            props["application_id"] = campaign.application_id;
            props["camp_id"] = campaign.id;
            props["brand_id"] = campaign.brand_id;
            props["camp_title"] = campaign.title ?? "none";
            props["bundle_id"] = MonetizrManager.bundleId ?? Application.identifier;
            props["player_id"] = deviceIdentifier;
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["sdk_version"] = MonetizrSettings.SDKVersion;
            props["screen_width"] = Screen.width;
            props["screen_height"] = Screen.height;
            props["device_model"] = SystemInfo.deviceModel;
            props["device_name"] = SystemInfo.deviceName;
            props["os"] = AnalyticsUtils.GetOsGroup();
        }

        private static IEnumerator Send (string json)
        {
            using (UnityWebRequest request = new UnityWebRequest(currentEndpointURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success) MonetizrLogger.PrintWarning($"BackendAnalytics failed: {request.error}");
            }
        }

    }

}
