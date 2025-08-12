package com.monetizr.prebidbridge;

import android.app.Activity;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedOutputStream;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.UUID;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class PrebidBridge {

    private static final String TAG = "PrebidBridge";
    private static final int CONNECT_TIMEOUT_MS = 8000;
    private static final int READ_TIMEOUT_MS = 15000;

    private static final Pattern VAST_TAG_CDATA =
            Pattern.compile("<VASTAdTagURI>\\s*<!\\[CDATA\\[(.*?)]\\]>\\s*</VASTAdTagURI>", Pattern.DOTALL);
    private static final Pattern VAST_TAG_PLAIN =
            Pattern.compile("<VASTAdTagURI>(.*?)</VASTAdTagURI>", Pattern.DOTALL);

    public interface UnityCallback {
        void onResult(String vastUrl);
    }

    public static void fetchStoredVastAsync(final Activity activity,
                                            final String configJson,
                                            final UnityCallback cb) {
        new Thread(() -> {
            String url = fetchStoredVast(configJson);
            if (activity != null && cb != null) {
                activity.runOnUiThread(() -> {
                    try { cb.onResult(url); }
                    catch (Throwable t) { Log.e(TAG, "Callback error: " + t.getMessage()); }
                });
            }
        }).start();
    }

    public static String fetchStoredVast(String configJson) {
        try {
            JSONObject cfg = new JSONObject(configJson);

            String host = optStringOr(cfg, "pbsHost", "https://rtb.monetizr.com/openrtb2/auction");
            JSONObject inlineSeatbid = cfg.optJSONObject("inlineSeatbidObj");
            String storedId = cfg.optString("storedAuctionResponseId", "");
            int width = cfg.optInt("width", 640);
            int height = cfg.optInt("height", 480);

            JSONArray mimes = cfg.optJSONArray("mimes");
            if (mimes == null || mimes.length() == 0) {
                mimes = new JSONArray().put("video/mp4");
            }

            if (inlineSeatbid == null && (storedId == null || storedId.isEmpty())) {
                Log.e(TAG, "[PBS] Need inlineSeatbidObj or storedAuctionResponseId");
                return "";
            }

            JSONObject payload = buildOpenRtbPayload(inlineSeatbid, storedId, width, height, mimes, userAgent());
            String body = payload.toString();

            Log.d(TAG, "[PBS] POST " + host + " body: " + truncate(body, 500));
            String response = httpPostJson(host, body);
            if (response == null || response.isEmpty()) {
                Log.e(TAG, "[PBS] Empty HTTP response");
                return "";
            }
            Log.d(TAG, "[PBS] Response: " + truncate(response, 800));

            String adm = extractAdmFromResponse(response);
            if (adm.isEmpty()) {
                Log.e(TAG, "[PBS] Missing adm in bid");
                return "";
            }

            String vastTag = extractVastAdTagUri(adm);
            if (vastTag == null || vastTag.isEmpty()) {
                Log.e(TAG, "[PBS] VASTAdTagURI not found in adm");
                return "";
            }

            Log.d(TAG, "[PBS] VAST tag: " + vastTag);
            return vastTag;

        } catch (JSONException e) {
            Log.e(TAG, "[PBS] JSON error: " + e.getMessage());
            return "";
        } catch (Exception e) {
            Log.e(TAG, "[PBS] Error: " + e.getMessage());
            return "";
        }
    }

    private static JSONObject buildOpenRtbPayload(JSONObject inlineSeatbidObj,
                                                  String storedId,
                                                  int w, int h,
                                                  JSONArray mimes,
                                                  String ua) throws JSONException {
        JSONObject root = new JSONObject();
        root.put("id", UUID.randomUUID().toString());
        root.put("test", 1);
        root.put("tmax", 800);

        JSONObject app = new JSONObject();
        app.put("id", "monetizr-unity-mvp");
        root.put("app", app);

        JSONObject device = new JSONObject();
        device.put("ua", ua);
        device.put("ip", "127.0.0.1");
        root.put("device", device);

        JSONObject video = new JSONObject();
        video.put("mimes", mimes);
        video.put("w", w);
        video.put("h", h);
        video.put("minduration", 6);
        video.put("maxduration", 60);

        JSONObject imp = new JSONObject();
        imp.put("id", "1");
        imp.put("secure", 1);
        imp.put("video", video);

        JSONObject stored = new JSONObject();
        if (inlineSeatbidObj != null) {
            stored.put("seatbidobj", inlineSeatbidObj);
        } else {
            stored.put("id", storedId);
        }

        JSONObject prebid = new JSONObject();
        prebid.put("storedauctionresponse", stored);

        JSONObject ext = new JSONObject();
        ext.put("prebid", prebid);

        imp.put("ext", ext);

        JSONArray imps = new JSONArray();
        imps.put(imp);
        root.put("imp", imps);

        return root;
    }

    private static String httpPostJson(String urlStr, String jsonBody) throws Exception {
        HttpURLConnection conn = null;
        try {
            URL url = new URL(urlStr);
            conn = (HttpURLConnection) url.openConnection();
            conn.setConnectTimeout(CONNECT_TIMEOUT_MS);
            conn.setReadTimeout(READ_TIMEOUT_MS);
            conn.setRequestMethod("POST");
            conn.setDoOutput(true);
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setRequestProperty("Accept", "application/json");

            byte[] bytes = jsonBody.getBytes(StandardCharsets.UTF_8);
            OutputStream out = new BufferedOutputStream(conn.getOutputStream());
            out.write(bytes);
            out.flush();
            out.close();

            int code = conn.getResponseCode();
            BufferedReader reader = new BufferedReader(new InputStreamReader(
                    code >= 200 && code < 300 ? conn.getInputStream() : conn.getErrorStream(),
                    StandardCharsets.UTF_8
            ));
            StringBuilder sb = new StringBuilder();
            String line;
            while ((line = reader.readLine()) != null) sb.append(line);
            reader.close();

            if (code < 200 || code >= 300) {
                throw new RuntimeException("HTTP " + code + " : " + sb);
            }
            return sb.toString();
        } finally {
            if (conn != null) conn.disconnect();
        }
    }

    private static String extractAdmFromResponse(String responseJson) {
        try {
            JSONObject res = new JSONObject(responseJson);
            JSONArray seatbid = res.optJSONArray("seatbid");
            if (seatbid == null || seatbid.length() == 0) return "";
            JSONObject sb0 = seatbid.optJSONObject(0);
            if (sb0 == null) return "";
            JSONArray bids = sb0.optJSONArray("bid");
            if (bids == null || bids.length() == 0) return "";
            JSONObject b0 = bids.optJSONObject(0);
            if (b0 == null) return "";
            return b0.optString("adm", "");
        } catch (Exception e) {
            Log.e(TAG, "[PBS] extractAdm error: " + e.getMessage());
            return "";
        }
    }

    private static String extractVastAdTagUri(String adm) {
        Matcher m1 = VAST_TAG_CDATA.matcher(adm);
        if (m1.find()) return m1.group(1).trim();
        Matcher m2 = VAST_TAG_PLAIN.matcher(adm);
        if (m2.find()) return m2.group(1).trim();
        return null;
    }

    private static String optStringOr(JSONObject obj, String key, String def) {
        String v = obj.optString(key, null);
        return v == null || v.isEmpty() ? def : v;
    }

    private static String userAgent() {
        String ua = System.getProperty("http.agent");
        return ua == null ? "Mozilla/5.0 (Linux; Android) Monetizr/1.0" : ua;
    }

    private static String truncate(String s, int max) {
        if (s == null) return "";
        return s.length() <= max ? s : s.substring(0, max) + "...";
    }
}
