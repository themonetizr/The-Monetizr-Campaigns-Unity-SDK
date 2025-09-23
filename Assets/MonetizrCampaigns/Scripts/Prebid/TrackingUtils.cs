using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.SDK.Prebid
{
    public static class TrackingUtils
    {
        public static List<string> ExtractAllTrackingStrings (string jsonResponse)
        {
            List<string> nurls = ExtractNurls(jsonResponse);
            List<string> burls = ExtractBurls(jsonResponse);
            List<string> imptrackers = ExtractImpTrackers(jsonResponse);

            List<string> allTrackers = new List<string>();
            allTrackers.AddRange(nurls);
            allTrackers.AddRange(burls);
            allTrackers.AddRange(imptrackers);

            return allTrackers;
        }

        public static void FireTrackers (IEnumerable<string> urls)
        {
            if (urls == null) return;

            foreach (string url in urls)
            {
                if (string.IsNullOrEmpty(url)) continue;

                MonetizrLogger.Print("Firing tracker: " + url);
                MonetizrManager.Instance.StartCoroutine(Fire(url));
            }
        }

        private static IEnumerator Fire (string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MonetizrLogger.PrintWarning("Tracker failed: " + request.error + " (" + url + ")");
                }
            }
        }

        public static List<string> ExtractNurls(string jsonResponse)
        {
            List<string> urls = new List<string>();
            if (string.IsNullOrEmpty(jsonResponse)) return urls;

            try
            {
                JObject root = JObject.Parse(jsonResponse);

                JArray seatbid = root["seatbid"] as JArray;
                if (seatbid == null || seatbid.Count == 0) return urls;

                JArray bids = seatbid[0]["bid"] as JArray;
                if (bids == null || bids.Count == 0) return urls;

                JToken bid = bids[0];

                if (bid["nurl"] != null && bid["nurl"].Type == JTokenType.String)
                {
                    string nurl = bid["nurl"].ToString();
                    MonetizrLogger.Print("[PrebidParser] Extracted nurl tracker: " + nurl);
                    urls.Add(nurl);
                }
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("[PrebidParser] Failed to extract nurls: " + ex.Message);
            }

            return urls;
        }

        public static List<string> ExtractImpTrackers(string jsonResponse)
        {
            List<string> urls = new List<string>();
            if (string.IsNullOrEmpty(jsonResponse)) return urls;

            try
            {
                JObject root = JObject.Parse(jsonResponse);

                JArray seatbid = root["seatbid"] as JArray;
                if (seatbid == null || seatbid.Count == 0) return urls;

                JArray bids = seatbid[0]["bid"] as JArray;
                if (bids == null || bids.Count == 0) return urls;

                JToken bid = bids[0];

                // imptrackers usually appear inside adm (if it's JSON instead of raw XML)
                if (bid["adm"] != null && bid["adm"].Type == JTokenType.String)
                {
                    string adm = bid["adm"].ToString();

                    try
                    {
                        JObject admJson = JObject.Parse(adm);
                        JArray imptrackers = admJson["imptrackers"] as JArray;
                        if (imptrackers != null)
                        {
                            foreach (var tracker in imptrackers)
                            {
                                if (tracker.Type == JTokenType.String)
                                {
                                    urls.Add(tracker.ToString());
                                    MonetizrLogger.Print("[PrebidParser] Extracted imptracker: " + tracker);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // adm is not JSON (it might be inline VAST XML), so ignore
                    }
                }
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("[PrebidParser] Failed to extract imptrackers: " + ex.Message);
            }

            return urls;
        }

        public static List<string> ExtractBurls(string jsonResponse)
        {
            List<string> urls = new List<string>();
            if (string.IsNullOrEmpty(jsonResponse)) return urls;

            try
            {
                JObject root = JObject.Parse(jsonResponse);

                JArray seatbid = root["seatbid"] as JArray;
                if (seatbid == null || seatbid.Count == 0) return urls;

                JArray bids = seatbid[0]["bid"] as JArray;
                if (bids == null || bids.Count == 0) return urls;

                JToken bid = bids[0];

                if (bid["burl"] != null && bid["burl"].Type == JTokenType.String)
                {
                    string burl = bid["burl"].ToString();
                    MonetizrLogger.Print("[PrebidParser] Extracted burl tracker: " + burl);
                    urls.Add(burl);
                }
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("[PrebidParser] Failed to extract burls: " + ex.Message);
            }

            return urls;
        }
    }
}
