using Monetizr.SDK.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.SDK.Debug
{
    public class GCPManager: MonoBehaviour
    {
        public static GCPManager Instance;
        private static string postURL = "https://http-intake.logs.us5.datadoghq.com/api/v2/logs";

        private void Awake ()
        {
            Instance = this;
        }

        /*
        public void Log (string logType, int code)
        {
            //string logMessage = ErrorDictionary.GetErrorMessage(code);
            StartCoroutine(GCPLog());
        }
        */

        private IEnumerator GCPLog ()
        {
            string url = "https://us-central1-gcp-monetizr-project.cloudfunctions.net/unity_notification_channel_to_slack";
            string jsonContent = "{\"msg\": \"TEST - Unity logging message.\", \"level\": \"error\"}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            MonetizrLogger.Print("GCP - Result: " + request.result);
            MonetizrLogger.Print("GCP - Text: " + request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                MonetizrLogger.Print("GCP - Error sending log: " + request.error);
            }
        }
    }

}