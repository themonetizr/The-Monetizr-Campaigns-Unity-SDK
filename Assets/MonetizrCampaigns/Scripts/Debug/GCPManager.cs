using Monetizr.SDK.Core;
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

        public void Log (string messageString, bool isError = false)
        {
            if (!isEnabled) return;
            string jsonContent = BuildJSONContent(messageString, isError);
            StartCoroutine(GCPLog(jsonContent));
        }

        private string BuildJSONContent (string messageString, bool isError)
        {
            string message = messageString;
            string code = "";
            string sdkVersion = MonetizrSettings.SDKVersion;
            string appVersion = Application.version;
            string bundleID = MonetizrSettings.bundleID;
            string campaignID = "";
            string missionID = "";
            string level = isError ? "error" : "info";
            string jsonContent = $"{{\"msg\": \"{message}\", \"code\": \"{code}\", \"sdk_version\": \"{sdkVersion}\", \"app_version\": \"{appVersion}\", \"bundle\": \"{bundleID}\", \"camp_id\": \"{campaignID}\", \"mission_id\": \"{missionID}\", \"level\": \"{level}\"}}";
            return jsonContent;
        }

        private IEnumerator GCPLog (string jsonContent)
        {
            UnityWebRequest request = new UnityWebRequest(postURL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                MonetizrLogger.Print("Error sending log: " + request.error);
            }
        }

    }

}