using Monetizr.SDK.Debug;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class DatadogManager
{
    private static string postURL = "https://http-intake.logs.us5.datadoghq.com/api/v2/logs";
    private static string apiKey = "558f6d31be858ca8c13cff336d4ecd98";
    private static string applicationKey = "2ec3f5a8d60631134b6a544ec8d8a8a051292059";

    public static IEnumerator PostLogData ()
    {
        MonetizrLogger.Print("DATADOG - Sending log.");

        string ddsource = "MonetizrSDK";
        string ddtags = "env:dev,version:1.1.3";
        string hostname = "Monetizr";
        string message = System.DateTime.Now.ToLongDateString() + " " + System.DateTime.Now.ToLongTimeString() + " - Test Message.";
        string service = Application.identifier;
        var jsonData = $"{{\"ddsource\":\"{ddsource}\",\"ddtags\":\"{ddtags}\",\"hostname\":\"{hostname}\",\"message\":\"{message}\",\"service\":\"{service}\"}}";

        UnityWebRequest request = new UnityWebRequest(postURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        //request.SetRequestHeader("DD-SITE", "us5.datadoghq.com");
        //request.SetRequestHeader("Content-Encoding", "deflate");
        request.SetRequestHeader("DD-API-KEY", apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        MonetizrLogger.Print("DATADOG - Result: " + request.result.ToString());

        if (request.result != UnityWebRequest.Result.Success)
        {
            MonetizrLogger.Print("DATADOG - Error sending log: " + request.error);
            MonetizrLogger.Print($"DATADOG - Status Code: {request.responseCode}");
            MonetizrLogger.Print($"DATADOG - Response: {request.downloadHandler.text}");
        }
        else
        {
            MonetizrLogger.Print("DATADOG - Log sent successfully!");
        }
    }
}
