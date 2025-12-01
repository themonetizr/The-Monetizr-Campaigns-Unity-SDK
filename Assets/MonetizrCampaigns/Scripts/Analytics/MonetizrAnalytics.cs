using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Monetizr.SDK.Analytics
{
    public static class MonetizrAnalytics
    {
        private static string endpointUrl = "https://mixpanel-retirement-func-1074977689117.us-central1.run.app/";

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
            payload["user_id"] = MonetizrMobileAnalytics.deviceIdentifier;
            payload["timestamp"] = DateTime.UtcNow.ToString("o");

            JSONObject propsJson = MonetizrUtils.DictionaryToJson(props);
            payload["properties"] = propsJson;

            string json = payload.ToString();
            MonetizrInstance.Instance.StartCoroutine(Send(json));
        }

        private static void AddDefaultValues (Dictionary<string, object> props, ServerCampaign campaign)
        {
            props["application_id"] = campaign.application_id;
            props["camp_id"] = campaign.id;
            props["brand_id"] = campaign.brand_id;
            props["camp_title"] = campaign.title ?? "none";
            props["bundle_id"] = MonetizrManager.bundleId ?? Application.identifier;
            props["player_id"] = MonetizrMobileAnalytics.deviceIdentifier;
            props["application_name"] = Application.productName;
            props["application_version"] = Application.version;
            props["sdk_version"] = MonetizrSettings.SDKVersion;
            props["screen_width"] = Screen.width;
            props["screen_height"] = Screen.height;
            props["device_model"] = SystemInfo.deviceModel;
            props["device_name"] = SystemInfo.deviceName;
            props["os"] = MonetizrMobileAnalytics.GetOsGroup();
        }

        private static IEnumerator Send (string json)
        {
            using (UnityWebRequest request = new UnityWebRequest(endpointUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success) MonetizrLogger.PrintWarning($"BackendAnalytics failed: {request.error}");
            }
        }

        public static void SendOpenRtbReport(string openRtbRequest, string status, string openRtbResponse, ServerCampaign campaign)
        {
            if (campaign == null)
            {
                MonetizrLogger.PrintWarning("ServerCampaign is null in SendOpenRtbReport");
                return;
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            AddDefaultValues(props, campaign);

            props["request"] = openRtbRequest;
            props["response"] = openRtbResponse;
            props["status"] = status;
            props["request_pieces"] = MonetizrUtils.SplitStringIntoPieces(openRtbRequest, 255);
            props["response_pieces"] = MonetizrUtils.SplitStringIntoPieces(openRtbResponse, 255);

        #if UNITY_EDITOR
            props["editor_test"] = true;
        #endif

            string eventName = "Programmatic-request-client";
            MonetizrLogger.Print($"SendOpenRtbReport: {status}, campaign {campaign.id}");

            JSONObject payload = new JSONObject();
            payload["event"] = eventName;
            payload["user_id"] = MonetizrMobileAnalytics.deviceIdentifier;
            payload["timestamp"] = DateTime.UtcNow.ToString("o");
            payload["properties"] = MonetizrUtils.DictionaryToJson(props);

            string json = payload.ToString();
            MonetizrInstance.Instance.StartCoroutine(Send(json));
        }

    }
}