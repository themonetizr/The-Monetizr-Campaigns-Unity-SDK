using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.SDK.Prebid
{
    public static class TrackingUtils
    {
        public static List<string> ExtractAllTrackingStrings(string jsonResponse)
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

        public static void FireTrackers(IEnumerable<string> urls)
        {
            if (urls == null) return;

            foreach (string url in urls)
            {
                if (string.IsNullOrEmpty(url)) continue;

                MonetizrLogger.Print("Firing tracker: " + url);
                MonetizrManager.Instance.StartCoroutine(Fire(url));
            }
        }

        private static IEnumerator Fire(string url)
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
                JSONNode root = JSON.Parse(jsonResponse);
                if (root == null) return urls;

                JSONArray seatbid = root["seatbid"].AsArray;
                if (seatbid == null || seatbid.Count == 0) return urls;

                JSONArray bids = seatbid[0]?["bid"]?.AsArray;
                if (bids == null || bids.Count == 0) return urls;

                JSONNode bid = bids[0];

                if (bid["nurl"] != null && bid["nurl"].IsString)
                {
                    string nurl = bid["nurl"].Value;
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
                JSONNode root = JSON.Parse(jsonResponse);
                if (root == null) return urls;

                JSONArray seatbid = root["seatbid"].AsArray;
                if (seatbid == null || seatbid.Count == 0) return urls;

                JSONArray bids = seatbid[0]?["bid"]?.AsArray;
                if (bids == null || bids.Count == 0) return urls;

                JSONNode bid = bids[0];

                // adm can either be inline VAST (XML string) or JSON containing imptrackers
                if (bid["adm"] != null && bid["adm"].IsString)
                {
                    string adm = bid["adm"].Value;

                    try
                    {
                        JSONNode admJson = JSON.Parse(adm);
                        if (admJson != null && admJson["imptrackers"] != null)
                        {
                            JSONArray imptrackers = admJson["imptrackers"].AsArray;
                            foreach (JSONNode tracker in imptrackers)
                            {
                                if (tracker != null && tracker.IsString)
                                {
                                    string url = tracker.Value;
                                    urls.Add(url);
                                    MonetizrLogger.Print("[PrebidParser] Extracted imptracker: " + url);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // adm was not JSON → ignore (it's probably inline VAST XML)
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
                JSONNode root = JSON.Parse(jsonResponse);
                if (root == null) return urls;

                JSONArray seatbid = root["seatbid"].AsArray;
                if (seatbid == null || seatbid.Count == 0) return urls;

                JSONArray bids = seatbid[0]?["bid"]?.AsArray;
                if (bids == null || bids.Count == 0) return urls;

                JSONNode bid = bids[0];

                if (bid["burl"] != null && bid["burl"].IsString)
                {
                    string burl = bid["burl"].Value;
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
