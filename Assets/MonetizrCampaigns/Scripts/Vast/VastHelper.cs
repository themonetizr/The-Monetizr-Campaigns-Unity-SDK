using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Monetizr.SDK.Utils;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Core;

namespace Monetizr.SDK.VAST
{
    internal class VastHelper
    {
        internal static MonetizrHttpClient httpClient;
        internal static string userAgent;
        private string _omidJsServiceContent;

        internal VastHelper(MonetizrHttpClient httpClient, string _userAgent)
        {
            VastHelper.httpClient = httpClient;
            userAgent = _userAgent;
        }

        [System.Serializable]
        internal class VideoSettings
        {
            public bool isSkippable = true;
            public string skipOffset = "";
            public bool isAutoPlay = true;
            public string position = "preroll";
            public string videoUrl = "";
            public string videoClickThroughUrl = "";

            public VideoSettings() { }

            public VideoSettings(VideoSettings vidSettings)
            {
                isSkippable = vidSettings.isSkippable;
                skipOffset = vidSettings.skipOffset;
                isAutoPlay = vidSettings.isAutoPlay;
                position = vidSettings.position;
                videoUrl = vidSettings.videoUrl;
            }
        }

        [System.Serializable]
        internal class VastSettings
        {
            public string vendorName = "Themonetizr";
            public string sdkVersion = MonetizrSettings.SDKVersion;
            public VideoSettings videoSettings = new VideoSettings();
            public List<AdVerification> adVerifications = new List<AdVerification>();
            public List<TrackingEvent> videoTrackingEvents = new List<TrackingEvent>();

            internal VastSettings() { }

            internal VastSettings(VastSettings settingsToCopy)
            {
                vendorName = settingsToCopy.vendorName;
                sdkVersion = settingsToCopy.sdkVersion;

                videoSettings = new VideoSettings(settingsToCopy.videoSettings);

                adVerifications = new List<AdVerification>();
                settingsToCopy.adVerifications.ForEach((v) => adVerifications.Add(new AdVerification(v)));

                videoTrackingEvents = new List<TrackingEvent>();
                settingsToCopy.videoTrackingEvents.ForEach((e) => videoTrackingEvents.Add(new TrackingEvent(e)));
            }

            internal bool IsEmpty()
            {
                return string.IsNullOrEmpty(videoSettings.videoUrl);
            }

            internal void ReplaceVastTags(TagsReplacer replacer)
            {
                foreach (var a in adVerifications)
                {
                    foreach (var er in a.executableResource)
                    {
                        er.value = replacer.Replace(er.value);
                    }

                    foreach (var jsr in a.javaScriptResource)
                    {
                        jsr.value = replacer.Replace(jsr.value);
                    }

                    foreach (var te in a.tracking)
                    {
                        te.value = replacer.Replace(te.value);
                    }
                }

                foreach (var vte in videoTrackingEvents)
                {
                    vte.value = replacer.Replace(vte.value);
                }

                videoSettings.videoClickThroughUrl = replacer.Replace(videoSettings.videoClickThroughUrl);
            }

        }

        [System.Serializable]
        internal class AdVerification
        {
            public string verificationParameters = "";
            public string vendorField = "";
            public List<VerificationExecutableResource> executableResource = new List<VerificationExecutableResource>();
            public List<VerificationJavaScriptResource> javaScriptResource = new List<VerificationJavaScriptResource>();
            public List<TrackingEvent> tracking = new List<TrackingEvent>();

            public AdVerification() { }

            public AdVerification(AdVerification adVerificationToCopy)
            {
                verificationParameters = adVerificationToCopy.verificationParameters;
                vendorField = adVerificationToCopy.vendorField;
                executableResource = new List<VerificationExecutableResource>();
                adVerificationToCopy.executableResource.ForEach(item => executableResource.Add(new VerificationExecutableResource(item)));
                javaScriptResource = new List<VerificationJavaScriptResource>();
                adVerificationToCopy.javaScriptResource.ForEach(item => javaScriptResource.Add(new VerificationJavaScriptResource(item)));
                tracking = new List<TrackingEvent>();
                adVerificationToCopy.tracking.ForEach(item => tracking.Add(new TrackingEvent(item)));
            }
        }

        [System.Serializable]
        internal class VerificationExecutableResource
        {
            public string apiFramework = "";
            public string type = "";
            public string value = "";

            public VerificationExecutableResource() { }

            public VerificationExecutableResource(VerificationExecutableResource original)
            {
                apiFramework = original.apiFramework;
                type = original.type;
                value = original.value;
            }

            public VerificationExecutableResource(Verification_typeExecutableResource er)
            {
                apiFramework = er.apiFramework;
                type = er.type;
                value = er.Value;
            }
        }

        [System.Serializable]
        internal class VerificationJavaScriptResource
        {
            public string apiFramework = "";
            public bool browserOptional = false;
            public bool browserOptionalSpecified = false;
            public string value = "";

            public VerificationJavaScriptResource() { }

            public VerificationJavaScriptResource(VerificationJavaScriptResource original)
            {
                apiFramework = original.apiFramework;
                browserOptional = original.browserOptional;
                browserOptionalSpecified = original.browserOptionalSpecified;
                value = original.value;
            }

            public VerificationJavaScriptResource(string apiFramework, bool browserOptional, bool browserOptionalSpecified, string value)
            {
                this.apiFramework = apiFramework;
                this.browserOptional = browserOptional;
                this.browserOptionalSpecified = browserOptionalSpecified;
                this.value = value;
            }

            public VerificationJavaScriptResource(Verification_typeJavaScriptResource jsr)
            {
                apiFramework = jsr.apiFramework;
                browserOptional = jsr.browserOptional;
                browserOptionalSpecified = jsr.browserOptionalSpecified;
                value = jsr.Value;
            }
        }

        [System.Serializable]
        internal class TrackingEvent
        {
            public string @event = "";
            public string value = "";

            public TrackingEvent() { }

            public TrackingEvent(TrackingEvent trackingEventToCopy)
            {
                @event = trackingEventToCopy.@event;
                value = trackingEventToCopy.value;
            }

            public TrackingEvent(TrackingEvents_Verification_typeTracking te)
            {
                @event = te.@event;
                value = te.Value;
            }
        }

    }
}