using Monetizr.SDK.Debug;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidUtils
    {
        public enum PrebidResponseType
        {
            VastXml,
            VastUrl,
            HtmlCreative,
            CacheId,
            Empty,
            Unknown,
            Error
        }

        public static string ExtractPrebidResponse(string jsonResponse, out PrebidResponseType responseType)
        {
            responseType = PrebidResponseType.Unknown;

            if (string.IsNullOrEmpty(jsonResponse))
            {
                MonetizrLogger.Print("Empty response");
                responseType = PrebidResponseType.Empty;
                return null;
            }

            try
            {
                JSONNode root = JSON.Parse(jsonResponse);
                if (root == null)
                {
                    MonetizrLogger.Print("Failed to parse JSON root.");
                    responseType = PrebidResponseType.Error;
                    return null;
                }

                JSONArray seatbid = root["seatbid"].AsArray;
                if (seatbid == null || seatbid.Count == 0)
                {
                    MonetizrLogger.Print("No seatbid found.");
                    responseType = PrebidResponseType.Empty;
                    return null;
                }

                JSONArray bids = seatbid[0]?["bid"]?.AsArray;
                if (bids == null || bids.Count == 0)
                {
                    MonetizrLogger.Print("No bid found.");
                    responseType = PrebidResponseType.Empty;
                    return null;
                }

                JSONNode bid = bids[0];

                // 1. Inline creative (adm)
                if (bid["adm"] != null && bid["adm"].IsString)
                {
                    string adm = bid["adm"].Value.Trim();

                    if (Regex.IsMatch(adm, @"<\s*VAST", RegexOptions.IgnoreCase))
                    {
                        MonetizrLogger.Print("Extracted inline VAST XML");
                        responseType = PrebidResponseType.VastXml;
                        return adm;
                    }
                    else if (adm.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        MonetizrLogger.Print("Extracted HTML creative");
                        responseType = PrebidResponseType.HtmlCreative;
                        return adm;
                    }
                }

                // 2. Cached VAST URL in ext.prebid.cache.vastXml.url
                JSONNode cacheUrl = root["seatbid"]?[0]?["bid"]?[0]?["ext"]?["prebid"]?["cache"]?["vastXml"]?["url"];
                if (cacheUrl != null && Uri.IsWellFormedUriString(cacheUrl.Value, UriKind.Absolute))
                {
                    MonetizrLogger.Print("Extracted cached VAST URL");
                    responseType = PrebidResponseType.VastUrl;
                    return cacheUrl.Value;
                }

                // 3. Cache ID
                if (bid["cacheId"] != null)
                {
                    MonetizrLogger.Print("Found CacheID: " + bid["cacheId"].Value);
                    responseType = PrebidResponseType.CacheId;
                    return bid["cacheId"].Value;
                }

                // 4. Empty object
                if (bid.ToString().Trim() == "{}")
                {
                    MonetizrLogger.Print("Empty bid response.");
                    responseType = PrebidResponseType.Empty;
                    return null;
                }

                // 5. Unknown
                MonetizrLogger.Print("Unknown bid response structure.");
                responseType = PrebidResponseType.Unknown;
                return null;
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("Failed to parse response: " + ex.Message);
                responseType = PrebidResponseType.Error;
                return null;
            }
        }

        public enum EndpointResponseType
        {
            VastXml,
            Playlist,
            Empty,
            Error,
            Unknown
        }

        public static string ExtractEndpointResponse(string xmlResponse, out EndpointResponseType responseType)
        {
            responseType = EndpointResponseType.Unknown;

            if (string.IsNullOrEmpty(xmlResponse))
            {
                MonetizrLogger.Print("Empty response.");
                responseType = EndpointResponseType.Empty;
                return null;
            }

            try
            {
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(xmlResponse);

                var rootName = xml.DocumentElement?.Name?.ToLowerInvariant();

                if (rootName == "vast")
                {
                    MonetizrLogger.Print("Found standard VAST XML.");
                    responseType = EndpointResponseType.VastXml;
                    return xmlResponse;
                }

                if (rootName == "playlist")
                {
                    var adNode = xml.SelectSingleNode("//Playlist/Preroll/Ad");
                    if (adNode != null && !string.IsNullOrEmpty(adNode.InnerText))
                    {
                        string vastUrl = adNode.InnerText.Trim();
                        MonetizrLogger.Print("Found Playlist. Extracted first VAST URL: " + vastUrl);
                        responseType = EndpointResponseType.Playlist;
                        return vastUrl;
                    }

                    MonetizrLogger.Print("Playlist found but no <Ad> entry.");
                    responseType = EndpointResponseType.Empty;
                    return null;
                }

                var errorNode = xml.SelectSingleNode("//VAST//Error");
                if (errorNode != null)
                {
                    MonetizrLogger.Print("VAST with <Error> node.");
                    responseType = EndpointResponseType.Empty;
                    return null;
                }

                MonetizrLogger.Print("Unknown XML root: " + rootName);
                responseType = EndpointResponseType.Unknown;
                return null;
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("Failed to parse XML: " + ex.Message);
                responseType = EndpointResponseType.Error;
                return null;
            }
        }

        public static List<string> ParseEndpoints(string raw)
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

        private static bool IsHttpUrl(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return Uri.TryCreate(s, UriKind.Absolute, out Uri u) && 
                   (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
        }
    }
}
