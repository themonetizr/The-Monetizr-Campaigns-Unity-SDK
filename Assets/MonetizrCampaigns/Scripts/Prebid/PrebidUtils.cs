using Monetizr.SDK.Debug;
using Newtonsoft.Json.Linq;
using System;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidUtils
    {
        public enum PrebidHandleKind { Empty, Url, VastXml, CacheId, Unknown }

        public static bool TryExtractHandle(string input, out PrebidHandleKind kind, out string handle)
        {
            handle = string.Empty;
            kind = PrebidHandleKind.Empty;

            if (string.IsNullOrWhiteSpace(input)) return false;

            // URL?
            if (Uri.TryCreate(input, UriKind.Absolute, out var u) &&
                (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
            {
                handle = input; kind = PrebidHandleKind.Url; return true;
            }

            // Inline XML?
            var t = input.TrimStart();
            if (t.StartsWith("<") || t.IndexOf("<VAST", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                handle = input; kind = PrebidHandleKind.VastXml; return true;
            }

            // JSON?
            if (t.StartsWith("{") || t.StartsWith("["))
            {
                // Try PBS auction JSON first
                var fromPbs = ExtractFromPbsJson(input);
                if (!string.IsNullOrEmpty(fromPbs))
                {
                    ClassifyLeaf(fromPbs, out kind);
                    handle = fromPbs;
                    return true;
                }

                // Try SDK targeting JSON ({"resultCode":"...","targeting":{...}})
                var fromTargeting = ExtractFromTargetingJson(input);
                if (!string.IsNullOrEmpty(fromTargeting))
                {
                    ClassifyLeaf(fromTargeting, out kind);
                    handle = fromTargeting;
                    return true;
                }

                kind = PrebidHandleKind.Unknown; // JSON but no handle found
                return false;
            }

            // Fallback: treat as opaque text (often a cache id)
            handle = input.Trim();
            kind = PrebidHandleKind.CacheId;
            return true;
        }

        static void ClassifyLeaf(string s, out PrebidHandleKind kind)
        {
            kind = PrebidHandleKind.CacheId;
            if (Uri.TryCreate(s, UriKind.Absolute, out var u) &&
                (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
            { kind = PrebidHandleKind.Url; return; }

            var t = s?.TrimStart();
            if (!string.IsNullOrEmpty(t) && (t.StartsWith("<") || t.IndexOf("<VAST", StringComparison.OrdinalIgnoreCase) >= 0))
            { kind = PrebidHandleKind.VastXml; return; }
        }

        // Prefer URL -> cacheId -> hb_cache_id -> adm
        static string ExtractFromPbsJson(string json)
        {
            try
            {
                var root = JObject.Parse(json);
                var seatbid = root["seatbid"] as JArray;
                if (seatbid == null) return string.Empty;

                foreach (var sb in seatbid)
                {
                    var bids = sb?["bid"] as JArray;
                    if (bids == null) continue;

                    foreach (var bid in bids)
                    {
                        var prebid = bid?["ext"]?["prebid"] as JObject;

                        var vastXml = prebid?["cache"]?["vastXml"] as JObject;
                        var url = vastXml?["url"]?.ToString();
                        if (!string.IsNullOrEmpty(url)) return url;

                        var cacheId = vastXml?["cacheId"]?.ToString() ?? vastXml?["key"]?.ToString();
                        if (!string.IsNullOrEmpty(cacheId)) return cacheId;

                        var hbCacheId = prebid?["targeting"]?["hb_cache_id"]?.ToString();
                        if (!string.IsNullOrEmpty(hbCacheId)) return hbCacheId;

                        var adm = bid?["adm"]?.ToString();
                        if (!string.IsNullOrEmpty(adm)) return adm;
                    }
                }
            }
            catch (Exception e) { MonetizrLogger.Print($"Prebid - PBS parse: {e.Message}"); }
            return string.Empty;
        }

        // Prefer adm -> hb_cache_id
        static string ExtractFromTargetingJson(string json)
        {
            try
            {
                var root = JObject.Parse(json);
                var targeting = (root["targeting"] as JObject) ?? root;

                var adm = targeting["adm"]?.ToString();
                if (!string.IsNullOrEmpty(adm)) return adm;

                var cacheId = targeting["hb_cache_id"]?.ToString();
                if (!string.IsNullOrEmpty(cacheId)) return cacheId;
            }
            catch (Exception e) { MonetizrLogger.Print($"Prebid - Targeting parse: {e.Message}"); }
            return string.Empty;
        }


    }
}