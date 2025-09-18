using System.Collections.Generic;
using UnityEngine;
using mixpanel;
using System;
using System.Globalization;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.UI;
using EventType = Monetizr.SDK.Core.EventType;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Monetizr.SDK.Analytics
{
    internal class MonetizrMobileAnalytics
    {
        public static string osVersion;
        public static string advertisingID = "";
        public static bool limitAdvertising = false;
        public static bool isMixpanelInitialized = false;

        internal static DeviceSizeGroup deviceSizeGroup = DeviceSizeGroup.Unknown;
        internal static bool isAdvertisingIDDefined = false;
        internal static string deviceIdentifier = "";

        private HashSet<AdElement> adNewElements = new HashSet<AdElement>();
        private HashSet<AdElement> visibleAdAsset = new HashSet<AdElement>();

        public static bool useBackendAnalytics = true;

        internal static string GetPlacementName(AdPlacement t)
        {
            switch (t)
            {
                case AdPlacement.TinyTeaser: return "TinyTeaser";
                case AdPlacement.Video:
                case AdPlacement.Html5: return "Html5VideoScreen";
                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                    return "NotificationScreen";
                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                    return "EmailEnterScreen";
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.EmailCongratsNotificationScreen: return "CongratsScreen";
                case AdPlacement.Minigame: return "MiniGameScreen";
                case AdPlacement.Survey: return "SurveyScreen";
                case AdPlacement.HtmlPage: return "HtmlPageScreen";
                case AdPlacement.RewardsCenterScreen: return "RewardsCenterScreen";

                default:
                    return t.ToString();

            }
        }

        public static readonly Dictionary<DeviceSizeGroup, string> deviceSizeGroupNames = new Dictionary<DeviceSizeGroup, string>()
        {
            { DeviceSizeGroup.Phone, "phone" },
            { DeviceSizeGroup.Tablet, "tablet" },
            { DeviceSizeGroup.Unknown, "unknown" },
        };

        internal static DeviceSizeGroup GetDeviceGroup()
        {
#if UNITY_IOS
    bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
            {
                return DeviceSizeGroup.Tablet;
            }
 
            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");
            if (deviceIsIphone)
            {
                return DeviceSizeGroup.Phone;
            }
#elif UNITY_ANDROID

            float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            bool isTablet = (MobileUtils.GetDeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);

            if (isTablet)
            {
                return DeviceSizeGroup.Tablet;
            }
            else
            {
                return DeviceSizeGroup.Phone;
            }

#endif
            return DeviceSizeGroup.Unknown;

        }

        internal static string GetOsGroup()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#else
            return "";
#endif
        }

        internal MonetizrMobileAnalytics ()
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
            deviceSizeGroup = GetDeviceGroup();
            MonetizrLogger.Print($"OS Version {osVersion} Ad id: {advertisingID} Limit ads: {limitAdvertising} Device group: {deviceSizeGroup}");
            isMixpanelInitialized = false;
        }

        internal void Initialize (bool testEnvironment, string mixPanelApiKey, bool logConnectionErrors)
        {
            if (useBackendAnalytics)
            {
                MonetizrLogger.Print("BackendAnalytics: Skipping Mixpanel init.");
                return;
            }

            string key = "cda45517ed8266e804d4966a0e693d0d";

            if (testEnvironment)
            {
                key = "d4de97058730720b3b8080881c6ba2e0";
            }

            if (!string.IsNullOrEmpty(mixPanelApiKey))
            {
                if (mixPanelApiKey.IndexOf("\n", StringComparison.Ordinal) >= 0) mixPanelApiKey = null;
                key = mixPanelApiKey;
            }

            if (isMixpanelInitialized) return;
            isMixpanelInitialized = true;

            Mixpanel.Init();
            Mixpanel.SetToken(key);
            Mixpanel.Identify(deviceIdentifier);
            Mixpanel.SetLogConnectionErrors(logConnectionErrors);
            MonetizrLogger.Print($"Mixpanel init called {key}");
        }

        private void BeginShowAdAsset (string eventName, AdPlacement placement, ServerCampaign campaign)
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
            StartTimedEvent(eventName);
        }

        private void StartTimedEvent (string eventName)
        {
            if (useBackendAnalytics)
            {
                MonetizrAnalytics.TrackEvent(eventName, null, null, timed: true);
            }
            else
            {
                Mixpanel.StartTimedEvent($"[UNITY_SDK] [TIMED] {eventName}");
            }
        }

        private void EndShowAdAsset (AdPlacement placement, ServerCampaign campaign, bool removeElement = true)
        {
            visibleAdAsset.RemoveWhere((AdElement a) =>
            {
                ServerCampaign c = campaign == null ? null : a.campaign;
                bool remove = a.placement == placement && c == campaign;
                if (remove) _EndShowAdAsset(a);
                return remove;
            });
        }

        private void AddDefaultMixpanelValues(Value props, ServerCampaign campaign)
        {
            if (campaign != null)
            {
                props["application_id"] = campaign.application_id;
                props["camp_id"] = campaign.id;
                props["brand_id"] = campaign.brand_id;
                props["camp_title"] = campaign.title;

                props["brand_name"] = campaign.TryGetAsset(AssetsType.BrandTitleString, out string brandName)
                    ? brandName
                    : "none";
            }

            props["bundle_id"] = MonetizrManager.bundleId;
            props["player_id"] = GetUserId();
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["impressions"] = "1";
            props["ab_segment"] = MonetizrManager.abTestSegment;
            props["device_size"] = deviceSizeGroupNames[deviceSizeGroup];
            props["api_key"] = MonetizrManager.Instance.GetCurrentAPIkey();
            props["sdk_version"] = MonetizrSettings.SDKVersion;
            props["ad_id"] = MonetizrMobileAnalytics.advertisingID;
            props["screen_width"] = Screen.width.ToString();
            props["screen_height"] = Screen.height.ToString();
            props["screen_dpi"] = Screen.dpi.ToString(CultureInfo.InvariantCulture);
            props["device_group"] = GetDeviceGroup().ToString().ToLower();
            props["device_memory"] = SystemInfo.systemMemorySize.ToString();
            props["device_model"] = SystemInfo.deviceModel;
            props["device_name"] = SystemInfo.deviceName;
            props["internet_connection"] = NetworkingUtils.GetInternetConnectionType();

            if (campaign == null) return;

            foreach (KeyValuePair<string, string> s in campaign.serverSettings)
            {
                string key = s.Key;
                if (!key.EndsWith("_text") && key != "custom_missions") props[$"cs_{s.Key}"] = s.Value;
            }
        }

        private void MixpanelTrack(ServerCampaign camp, string eventName, Value props, bool darTag = false)
        {
            if (useBackendAnalytics)
            {
                Dictionary<string, string> dictProps = new Dictionary<string, string>();
                foreach (var v in props.GetFieldValue<Dictionary<string, Value>>("_container"))
                {
                    string value = v.Value.GetFieldValue<string>("_string");
                    dictProps[v.Key] = value;
                }
                MonetizrAnalytics.TrackEvent(eventName, camp, dictProps);
                return;
            }

            props["dar_tag_sent"] = darTag.ToString();
            Mixpanel.Identify(deviceIdentifier);
            Mixpanel.Track(eventName, props);

            if (camp.serverSettings.GetBoolParam("mixpanel_fast_flush", false)) Mixpanel.Flush();

            if (MonetizrManager.ExternalAnalytics != null)
            {
                Dictionary<string, string> eventProps = new Dictionary<string, string>();
                foreach (KeyValuePair<string, Value> v in props.GetFieldValue<Dictionary<string, Value>>("_container"))
                {
                    string value = v.Value.GetFieldValue<string>("_string");
                    eventProps.Add(v.Key, value);
                }
                MonetizrManager.ExternalAnalytics(eventName, eventProps);
            }
        }


        private void _EndShowAdAsset(AdElement adAsset)
        {
            string placementName = adAsset.placement.ToString();
            _TrackEvent(adAsset.eventName, adAsset.campaign, true, new Dictionary<string, string>() { { "type", placementName } });
        }

        internal void LoadUserId()
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

        internal void RandomizeUserId()
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

        internal string GetUserId()
        {
            return deviceIdentifier;
        }

        internal void TrackEvent(Mission currentMission, PanelController panel, EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            if (panel.GetAdPlacement() == null) return;
            AdPlacement adPlacement = panel.GetAdPlacement().Value;
            TrackEvent(currentMission.campaign, currentMission, adPlacement, eventType, additionalValues);
        }

        internal void TrackEvent(Mission currentMission, AdPlacement adPlacement, EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            TrackEvent(currentMission.campaign, currentMission, adPlacement, eventType, additionalValues);
        }

        internal void TrackEvent(ServerCampaign currentCampaign, Mission currentMission, AdPlacement adPlacement, EventType eventType, Dictionary<string, string> additionalValues = null)
        {
            UnityEngine.Debug.Assert(currentCampaign != null);
            string placementName = GetPlacementName(adPlacement);
            MonetizrManager._CallUserDefinedEvent(currentCampaign.id, placementName, eventType);

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

        internal void TrackNewEvents(ServerCampaign campaign, Mission currentMission, AdPlacement adPlacement, string placementName, EventType eventType, Dictionary<string, string> additionalValues)
        {
            additionalValues.Add("placement", placementName);
            additionalValues.Add("placement_group", GetPlacementGroup(adPlacement));

            TrackOMSDKEvents(eventType, adPlacement, GetPlacementGroup(adPlacement));

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
                    additionalValues.Add("reward_type", MonetizrManager.temporaryRewardTypeSelection == RewardSelectionType.Ingame ? "ingame" : "product");
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

        private bool CanTrackInOMSDK(AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.TinyTeaser:
                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.EmailCongratsNotificationScreen:
                case AdPlacement.Minigame:
                case AdPlacement.Video:
                case AdPlacement.Survey:
                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                    return true;
            }

            return false;
        }

        private void TrackOMSDKEvents(EventType eventType, AdPlacement adPlacement, string placementGroup)
        {
            if (!CanTrackInOMSDK(adPlacement)) return;

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

        internal string GetPlacementGroup(AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.TinyTeaser:
                    return "MiniBanners";

                case AdPlacement.NotificationScreen:
                case AdPlacement.SurveyNotificationScreen:
                case AdPlacement.CongratsNotificationScreen:
                case AdPlacement.RewardsCenterScreen:
                case AdPlacement.EmailCongratsNotificationScreen:
                    return "StaticScreens";

                case AdPlacement.Minigame:
                case AdPlacement.Video:
                case AdPlacement.Html5:
                case AdPlacement.Survey:
                    return "EngagementScreens";

                case AdPlacement.EmailEnterInGameRewardScreen:
                case AdPlacement.EmailEnterCouponRewardScreen:
                case AdPlacement.EmailEnterSelectionRewardScreen:
                case AdPlacement.ActionScreen:
                    return "ActionScreens";

                default:
                    return "Other";

            }
        }

        internal void _TrackEvent (string name, ServerCampaign campaign, bool timed = false, Dictionary<string, string> additionalValues = null, double duration = -1.0)
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

            if (useBackendAnalytics)
            {
                MonetizrAnalytics.TrackEvent(eventName, campaign, additionalValues, timed, duration);
                return;
            }

            UnityEngine.Debug.Assert(isMixpanelInitialized);
            Value props = new Value();
            AddDefaultMixpanelValues(props, campaign);

            if (additionalValues != null)
            {
                foreach (KeyValuePair<string, string> s in additionalValues)
                {
                    props[s.Key] = s.Value;
                }
            }

            if (duration > 0.0) props["$duration"] = duration;
            MixpanelTrack(campaign, eventName, props);
        }

        internal void OnApplicationQuit()
        {
            foreach (AdElement ad in visibleAdAsset)
            {
                _EndShowAdAsset(ad);
            }

            if (!useBackendAnalytics) Mixpanel.Flush();
        }

        private void NestedDictIteration(string rootName, SimpleJSON.JSONNode p, Value props)
        {
            foreach (KeyValuePair<string, SimpleJSON.JSONNode> key in p)
            {
                SimpleJSON.JSONNode value = key.Value;
                string k = key.Key;
                string name = string.IsNullOrEmpty(k) ? rootName : $"{rootName}/{key.Key}";

                if (value.IsString || value.IsNumber)
                {
                    string v = key.Value.ToString();
                    if (value.IsString) v = v.Trim('"');
                    props[name] = v;
                }

                NestedDictIteration(name, key.Value, props);
            }
        }

        internal void SendOpenRtbReportToMixpanel(string openRtbRequest, string status, string openRtbResponse, ServerCampaign campaign)
        {
            if (useBackendAnalytics)
            {
                MonetizrAnalytics.SendOpenRtbReport(openRtbRequest, status, openRtbResponse, campaign);
                return;
            }

            Value props = new Value();
            AddDefaultMixpanelValues(props, campaign);
            SimpleJSON.JSONNode parameters = SimpleJSON.JSON.Parse(openRtbRequest);
            NestedDictIteration("", parameters, props);

#if UNITY_EDITOR
            props["editor_test"] = 1;
#endif

            props["request"] = openRtbRequest;
            props["response"] = openRtbResponse;
            props["status"] = status;
            props["response_pieces"] = MonetizrUtils.SplitStringIntoPieces(openRtbResponse, 255);
            props["request_pieces"] = MonetizrUtils.SplitStringIntoPieces(openRtbRequest, 255);

            MonetizrLogger.Print($"SendReport: {props}");
            Mixpanel.Identify(deviceIdentifier);
            Mixpanel.Track("Programmatic-request-client", props);
        }


    }

}
