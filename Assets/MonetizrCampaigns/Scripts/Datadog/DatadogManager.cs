using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.SDK.Datadog
{
    public class DatadogManager: MonoBehaviour
    {
        public static DatadogManager Instance;
        private static string postURL = "https://http-intake.logs.us5.datadoghq.com/api/v2/logs";
        private static string apiKey = "";

        private void Awake ()
        {
            Instance = this;
        }

        public void Log (string logType, int code)
        {
            string logMessage = ErrorDictionary.GetErrorMessage(code);
            StartCoroutine(PostLogData(logType, code.ToString(), logMessage));
        }

        private IEnumerator PostLogData (string logType, string code, string logMessage)
        {
            string ddsource = "UnitySDK";
            string ddtags = "env:prod,code:" + code + ",sdk-version:" + MonetizrSettings.SDKVersion + ",app-version:" + Application.version;
            string hostname = "Monetizr";
            string message = logMessage;
            string service = Application.identifier;
            string status = "info";
            if (logType == "error") status = "error";

            var jsonData = $"{{\"ddsource\":\"{ddsource}\",\"ddtags\":\"{ddtags}\",\"hostname\":\"{hostname}\",\"message\":\"{message}\",\"status\":\"{status}\",\"service\":\"{service}\"}}";

            UnityWebRequest request = new UnityWebRequest(postURL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("DD-API-KEY", apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                MonetizrLogger.Print("DATADOG - Error sending log: " + request.error);
            }
        }
    }

}