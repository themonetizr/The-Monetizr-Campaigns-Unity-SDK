using Monetizr.SDK.Core;
using Monetizr.SDK.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.SDK.Debug
{
    public class GCPManager: MonoBehaviour
    {
        public static GCPManager Instance;
        private static string postURL = "https://us-central1-gcp-monetizr-project.cloudfunctions.net/unity_notification_channel_to_slack";
        private static bool isEnabled = false;
        private static bool localOverride = false;

        private void Awake ()
        {
            Instance = this;
        }

        public void EnableLogging ()
        {
            if (localOverride) return;
            MonetizrLogger.Print("GCP - Enabled.");
            isEnabled = true;
        }

        public void Log (MessageEnum messageEnum)
        {
            if (!isEnabled) return;
            string jsonContent = BuildJSONContent(messageEnum);
            StartCoroutine(GCPLog(jsonContent));
        }

        private IEnumerator GCPLog (string jsonContent)
        {
            UnityWebRequest request = new UnityWebRequest(postURL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            MonetizrLogger.Print("GCP - Result: " + request.result + " / Text: " + request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                MonetizrLogger.Print("GCP - Error sending log: " + request.error);
            }
        }

        private string BuildJSONContent (MessageEnum messageEnum)
        {
            string message = EnumUtils.GetEnumDescription(messageEnum);
            string code = ((int) messageEnum).ToString();
            string sdkVersion = MonetizrSettings.SDKVersion;
            string appVersion = Application.version;
            string bundleID = MonetizrSettings.bundleID;
            string campaignID = "";
            string missionID = "";
            string level = EnumUtils.IsEnumError(messageEnum) ? "error" : "info";
            string jsonContent = $"{{\"msg\": \"{message}\", \"code\": \"{code}\", \"sdk_version\": \"{sdkVersion}\", \"app_version\": \"{appVersion}\", \"bundle\": \"{bundleID}\", \"camp_id\": \"{campaignID}\", \"mission_id\": \"{missionID}\", \"level\": \"{level}\"}}";
            return jsonContent;
        }
    }

}