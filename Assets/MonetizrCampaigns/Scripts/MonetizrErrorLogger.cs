using System.Collections.Generic;
using Monetizr.Raygun4Unity;
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
            if (type != LogType.Exception && type != LogType.Error) return;

            if (!MonetizrManager.IsInitialized())
                return;

            if (MonetizrManager.Instance.ConnectionsClient == null || MonetizrManager.Instance.ConnectionsClient.GlobalSettings == null)
                return;

            if (!condition.StartsWith("Monetizr SDK"))
                return;
            
            //Debug.LogError(condition + " " + stackTrace);

            _raygunClient.ApplicationVersion = MonetizrManager.SDKVersion;

            var tags = new List<string>();

            var campaign = MonetizrManager.Instance.GetActiveCampaign();

            var customData = new Dictionary<string, string>() {
                {"app_name", Application.productName } ,
                {"app_version", Application.version},
                {"unity_version", Application.unityVersion},
                {"bundle_id",MonetizrManager.bundleId},
                {"os_group",MonetizrMobileAnalytics.GetOsGroup()},
                {"camp_id", campaign != null ? campaign.id : "none"}
            };

#if !UNITY_EDITOR
            if (!string.IsNullOrEmpty(RaygunCrashReportingPostService.defaultApiEndPointForCr))
                _raygunClient.Send(condition, stackTrace, tags, customData);
#endif

            var sendReportToMixpanel =
                MonetizrManager.Instance.ConnectionsClient.GlobalSettings.GetBoolParam("app.sent_error_reports_to_mixpanel",
                    false);

            if (sendReportToMixpanel)
            {
                MonetizrManager.Instance.ConnectionsClient.Analytics.SendErrorToMixpanel(condition,
                    stackTrace,
                    MonetizrManager.Instance.GetActiveCampaign());
            }
        }
    }

}