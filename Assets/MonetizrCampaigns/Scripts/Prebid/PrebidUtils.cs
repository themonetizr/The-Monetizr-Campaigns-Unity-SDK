using Monetizr.SDK.Debug;
using Newtonsoft.Json.Linq;
using System;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidUtils
    {
        public enum AdResponseType
        {
            VastXml,
            VastUrl,
            CacheId,
            Empty,
            Unknown,
            Error
        }

        public static string ExtractAd (string jsonResponse, out AdResponseType responseType)
        {
            responseType = AdResponseType.Unknown;

            if (string.IsNullOrEmpty(jsonResponse))
            {
                MonetizrLogger.Print("[PrebidParser] Empty response");
                responseType = AdResponseType.Empty;
                return null;
            }

            try
            {
                var root = JObject.Parse(jsonResponse);

                var seatbid = root["seatbid"] as JArray;
                if (seatbid == null || seatbid.Count == 0)
                {
                    MonetizrLogger.Print("[PrebidParser] No seatbid found.");
                    responseType = AdResponseType.Empty;
                    return null;
                }

                var bids = seatbid[0]["bid"] as JArray;
                if (bids == null || bids.Count == 0)
                {
                    MonetizrLogger.Print("[PrebidParser] No bid found.");
                    responseType = AdResponseType.Empty;
                    return null;
                }

                var bid = bids[0];

                // 1. Try VAST (adm)
                if (bid["adm"] != null && bid["adm"].Type == JTokenType.String)
                {
                    string vast = bid["adm"].ToString();
                    MonetizrLogger.Print("[PrebidParser] Extracted VAST XML");
                    responseType = AdResponseType.VastXml;
                    return vast;
                }

                // 2. Try VAST URL (nurl)
                if (bid["nurl"] != null && bid["nurl"].Type == JTokenType.String)
                {
                    string url = bid["nurl"].ToString();
                    MonetizrLogger.Print("[PrebidParser] Extracted VAST URL");
                    responseType = AdResponseType.VastUrl;
                    return url;
                }

                // 3. Cache ID
                if (bid["cacheId"] != null)
                {
                    MonetizrLogger.Print("[PrebidParser] Found CacheID: " + bid["cacheId"]);
                    responseType = AdResponseType.CacheId;
                    return null;
                }

                // 4. Empty object
                if (bid.ToString().Trim() == "{}")
                {
                    MonetizrLogger.Print("[PrebidParser] Empty bid response.");
                    responseType = AdResponseType.Empty;
                    return null;
                }

                // 5. Unknown
                MonetizrLogger.Print("[PrebidParser] Unknown bid response structure.");
                responseType = AdResponseType.Unknown;
                return null;
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print("[PrebidParser] Failed to parse response: " + ex.Message);
                responseType = AdResponseType.Error;
                return null;
            }
        }


    }
}