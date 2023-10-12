using Mindscape.Raygun4Unity;
using UnityEngine;

namespace Monetizr.Campaigns
{

    public class MonetizrErrorLogger : MonoBehaviour
    {
        private RaygunClient _raygunClient;

        void Start()
        {
            _raygunClient = new RaygunClient("jOWSRRIMNNUhlzoxd7EBdA");
            Application.logMessageReceived += Application_logMessageReceived;
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!condition.StartsWith("Monetizr SDK"))
                return;

            if (type == LogType.Exception || type == LogType.Error)
            {
                Debug.LogError(condition + " " + stackTrace);
                
                _raygunClient.ApplicationVersion = MonetizrManager.SDKVersion;
                _raygunClient.Send(condition, stackTrace);
            }
        }
    }

}