using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidUtils
    {
        public enum PrebidResponseType
        {
            VastXml,
            VastUrl,
            CacheId,
            Empty,
            Unknown,
            Error
        }

        public static string ExtractPrebidResponse (string jsonResponse, out PrebidResponseType responseType)
        {
            responseType = PrebidResponseType.Unknown;

            if (string.IsNullOrEmpty(jsonResponse))
            {
                MonetizrLogger.Print("[PrebidParser] Empty response");
                responseType = PrebidResponseType.Empty;
                return null;
            }

            try
            {
                var root = JObject.Parse(jsonResponse);

                var seatbid = root["seatbid"] as JArray;
                if (seatbid == null || seatbid.Count == 0)
                {
                    MonetizrLogger.Print("[PrebidParser] No seatbid found.");
                    responseType = PrebidResponseType.Empty;
                    return null;
                }

                var bids = seatbid[0]["bid"] as JArray;
                if (bids == null || bids.Count == 0)
                {
                    MonetizrLogger.Print("[PrebidParser] No bid found.");
                    responseType = PrebidResponseType.Empty;
                    return null;
                }

                var bid = bids[0];

                // 1. Try VAST (adm)
                if (bid["adm"] != null && bid["adm"].Type == JTokenType.String)
                {
                    string vast = bid["adm"].ToString();
                    MonetizrLogger.Print("[PrebidParser] Extracted VAST XML");
                    responseType = PrebidResponseType.VastXml;
                    return vast;
                }

                // 2. Try VAST URL (nurl)
                if (bid["nurl"] != null && bid["nurl"].Type == JTokenType.String)
                {
                    string url = bid["nurl"].ToString();
                    MonetizrLogger.Print("[PrebidParser] Extracted VAST URL");
                    responseType = PrebidResponseType.VastUrl;
                    return url;
                }

                // 3. Cache ID
                if (bid["cacheId"] != null)
                {
                    MonetizrLogger.Print("[PrebidParser] Found CacheID: " + bid["cacheId"]);
                    responseType = PrebidResponseType.CacheId;
                    return null;
                }

                // 4. Empty object
                if (bid.ToString().Trim() == "{}")
                {
                    MonetizrLogger.Print("[PrebidParser] Empty bid response.");
                    responseType = PrebidResponseType.Empty;
                    return null;
                }

                // 5. Unknown
                MonetizrLogger.Print("[PrebidParser] Unknown bid response structure.");
                responseType = PrebidResponseType.Unknown;
                return null;
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("[PrebidParser] Failed to parse response: " + ex.Message);
                responseType = PrebidResponseType.Error;
                return null;
            }
        }

        public enum EndpointResponseType
        {
            VastXml,       // Proper <VAST> document
            Playlist,      // <Playlist> wrapper (AuthX style)
            Empty,         // Empty or no ads
            Error,         // Malformed XML or HTTP error
            Unknown        // Something else
        }

        public static string ExtractEndpointResponse (string xmlResponse, out EndpointResponseType responseType)
        {
            responseType = EndpointResponseType.Unknown;

            if (string.IsNullOrEmpty(xmlResponse))
            {
                MonetizrLogger.Print("[EndpointParser] Empty response.");
                responseType = EndpointResponseType.Empty;
                return null;
            }

            try
            {
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(xmlResponse);

                var rootName = xml.DocumentElement?.Name?.ToLowerInvariant();

                // 1. Standard VAST
                if (rootName == "vast")
                {
                    MonetizrLogger.Print("[EndpointParser] Found standard VAST XML.");
                    responseType = EndpointResponseType.VastXml;
                    return xmlResponse; // return full VAST XML string
                }

                // 2. Playlist (AuthX)
                if (rootName == "playlist")
                {
                    var adNode = xml.SelectSingleNode("//Playlist/Preroll/Ad");
                    if (adNode != null && !string.IsNullOrEmpty(adNode.InnerText))
                    {
                        string vastUrl = adNode.InnerText.Trim();
                        MonetizrLogger.Print("[EndpointParser] Found Playlist. Extracted first VAST URL: " + vastUrl);
                        responseType = EndpointResponseType.Playlist;
                        return vastUrl; // return the first VAST URL to fetch
                    }

                    MonetizrLogger.Print("[EndpointParser] Playlist found but no <Ad> entry.");
                    responseType = EndpointResponseType.Empty;
                    return null;
                }

                // 3. Empty <VAST> with <Error> or no Ads
                var errorNode = xml.SelectSingleNode("//VAST//Error");
                if (errorNode != null)
                {
                    MonetizrLogger.Print("[EndpointParser] VAST with <Error> node.");
                    responseType = EndpointResponseType.Empty;
                    return null;
                }

                // 4. Unknown root
                MonetizrLogger.Print("[EndpointParser] Unknown XML root: " + rootName);
                responseType = EndpointResponseType.Unknown;
                return null;
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("[EndpointParser] Failed to parse XML: " + ex.Message);
                responseType = EndpointResponseType.Error;
                return null;
            }
        }

        public static List<string> ParseEndpoints (string raw)
        {
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(raw)) return results;

            raw = raw.Trim();

            if (raw.Length >= 2 && ((raw[0] == '"' && raw[^1] == '"') || (raw[0] == '\'' && raw[^1] == '\'')))
            {
                raw = raw.Substring(1, raw.Length - 2).Trim();
            }

            try
            {
                if (raw.StartsWith("["))
                {
                    JSONNode node = JSON.Parse(raw);
                    JSONArray arr = node?.AsArray;
                    if (arr != null)
                    {
                        foreach (JSONNode n in arr)
                        {
                            string v = (n?.Value ?? "").Trim();
                            if (IsHttpUrl(v)) results.Add(v);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintWarning("ParseEndpoints: JSON array parse failed: " + e.Message);
            }

            if (results.Count == 0)
            {
                string[] parts = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    foreach (string p in parts)
                    {
                        string v = p.Trim();
                        if (IsHttpUrl(v)) results.Add(v);
                    }
                }
                else if (IsHttpUrl(raw))
                {
                    results.Add(raw);
                }
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> deduped = new List<string>();
            foreach (var e in results)
            {
                if (seen.Add(e)) deduped.Add(e);
            }

            return deduped;
        }

        private static bool IsHttpUrl (string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return Uri.TryCreate(s, UriKind.Absolute, out Uri u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
        }

    }
}