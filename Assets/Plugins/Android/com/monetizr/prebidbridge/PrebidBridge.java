package com.monetizr.prebidbridge;

import android.app.Activity;
import android.content.Context;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.prebid.mobile.Host;
import org.prebid.mobile.PrebidMobile;
import org.prebid.mobile.ResultCode;
import org.prebid.mobile.VideoAdUnit;
import org.prebid.mobile.VideoParameters;

import java.lang.reflect.Method;
import java.lang.reflect.Proxy;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONObject;

import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;

public class PrebidBridge {

    private static final String TAG = "PrebidBridge";
    private static final String DEFAULT_OPENRTB_URL = "https://rtb.monetizr.com/openrtb2/auction";
    private static final String DEFAULT_SITE_PAGE   = "https://example.com";

    private static final OkHttpClient HTTP = new OkHttpClient();
    private static final MediaType MEDIA_TYPE_JSON = MediaType.get("application/json; charset=utf-8");

    public interface UnityCallback { void onResult(String s); }

    // ---------- Init (robust across 2.2.1 shapes) ----------
    public static void initPrebid() {
        Context ctx = UnityPlayer.currentActivity.getApplicationContext();
        setDefaultHost();
        initPrebidInternal(ctx);
    }

    public static void initPrebid(String hostUrl) {
        Context ctx = UnityPlayer.currentActivity.getApplicationContext();
        try {
            if (hostUrl != null && !hostUrl.isEmpty()) {
                PrebidMobile.setPrebidServerHost(Host.createCustomHost(hostUrl));
            } else { setDefaultHost(); }
        } catch (Throwable t) {
            Log.e(TAG, "[Prebid] set host error: " + t.getMessage());
            setDefaultHost();
        }
        initPrebidInternal(ctx);
    }

    private static void setDefaultHost() {
        try { PrebidMobile.setPrebidServerHost(Host.APPNEXUS); }
        catch (Throwable t) { Log.e(TAG, "[Prebid] setDefaultHost error: " + t.getMessage()); }
    }

    private static void initPrebidInternal(Context ctx) {
        try {
            Class<?> pm = Class.forName("org.prebid.mobile.PrebidMobile");

            try {
                Method m = pm.getMethod("initializeSdk", Context.class, Object.class, Object.class, Runnable.class);
                m.invoke(null, ctx, null, null, (Runnable) () -> Log.d(TAG, "[Prebid] SDK initialized (4-arg)."));
                Log.d(TAG, "[Prebid] init via 4-arg OK"); return;
            } catch (Throwable ignored) {}

            try {
                Method m = pm.getMethod("initializeSdk", Context.class, Runnable.class);
                m.invoke(null, ctx, (Runnable) () -> Log.d(TAG, "[Prebid] SDK initialized (2-arg Runnable)."));
                Log.d(TAG, "[Prebid] init via 2-arg Runnable OK"); return;
            } catch (Throwable ignored) {}

            try {
                Class<?> inner = Class.forName("org.prebid.mobile.PrebidMobile$SdkInitializationListener");
                Method m = pm.getMethod("initializeSdk", Context.class, inner);
                Object proxy = Proxy.newProxyInstance(inner.getClassLoader(), new Class<?>[]{ inner },
                        (p, method, args) -> { if ("onSdkInitialized".equals(method.getName())) Log.d(TAG, "[Prebid] SDK initialized (inner listener)."); return null; });
                m.invoke(null, ctx, proxy);
                Log.d(TAG, "[Prebid] init via inner listener OK"); return;
            } catch (Throwable ignored) {}

            try {
                Class<?> top = Class.forName("org.prebid.mobile.SdkInitializationListener");
                Method m = pm.getMethod("initializeSdk", Context.class, top);
                Object proxy = Proxy.newProxyInstance(top.getClassLoader(), new Class<?>[]{ top },
                        (p, method, args) -> { if ("onSdkInitialized".equals(method.getName())) Log.d(TAG, "[Prebid] SDK initialized (top-level listener)."); return null; });
                m.invoke(null, ctx, proxy);
                Log.d(TAG, "[Prebid] init via top-level listener OK"); return;
            } catch (Throwable ignored) {}

            try {
                Method m = pm.getMethod("initializeSdk", Context.class);
                m.invoke(null, ctx);
                Log.d(TAG, "[Prebid] init via 1-arg OK"); return;
            } catch (Throwable e) {
                Log.w(TAG, "[Prebid] init via 1-arg failed: " + e.getMessage());
            }

            Log.w(TAG, "[Prebid] No compatible initializeSdk signature found (non-fatal).");
        } catch (Throwable t) {
            Log.w(TAG, "[Prebid] init error (non-fatal): " + t.getMessage());
        }
    }

    // ---------- Demand ----------
    public static void fetchDemand(final Activity activity,
                                   final String prebidData,
                                   final String hostUrl,
                                   final UnityCallback cb) {

        if (prebidData == null || prebidData.isEmpty()) {
            cb.onResult(""); return;
        }

        // Mode A: Raw OpenRTB JSON → return PBS response **as-is** (string)
        if (prebidData.trim().startsWith("{")) {
            new Thread(() -> {
                try {
                    String finalHost = (hostUrl != null && !hostUrl.isEmpty()) ? hostUrl : DEFAULT_OPENRTB_URL;
                    String json = ensureValidJson(prebidData);
                    json = normalizeOpenRtbJson(json);

                    String response = httpPostJson(finalHost, json);
                    // return body as-is (Unity will parse/decide)
                    cb.onResult(response != null ? response : "");
                } catch (Throwable t) {
                    Log.e(TAG, "[Prebid] JSON flow error: " + t.getMessage());
                    cb.onResult("");
                }
            }).start();
            return;
        }

        // Mode B: Stored Config ID via SDK → return raw targeting map as JSON
        activity.runOnUiThread(() -> {
            try {
                VideoAdUnit adUnit = new VideoAdUnit(prebidData, 640, 480);
                VideoParameters params = new VideoParameters(Arrays.asList("video/mp4"));
                adUnit.setVideoParameters(params);

                HashMap<String, String> targeting = new HashMap<>();
                adUnit.fetchDemand(targeting, resultCode -> {
                    // Build a JSON wrapper Unity can inspect
                    JSONObject out = new JSONObject();
                    try {
                        out.put("resultCode", (resultCode != null ? resultCode.name() : "UNKNOWN"));

                        JSONObject tJson = new JSONObject();
                        for (Map.Entry<String, String> e : targeting.entrySet()) {
                            tJson.put(e.getKey(), e.getValue());
                        }
                        out.put("targeting", tJson);
                    } catch (Throwable ignore) {}

                    cb.onResult(out.toString());
                });
            } catch (Throwable e) {
                Log.e(TAG, "[Prebid] SDK flow error: " + e.getMessage());
                cb.onResult("");
            }
        });
    }

    // ---------- Helpers ----------

    public static String getIabTcfConsent() {
        try {
            Context ctx = UnityPlayer.currentActivity.getApplicationContext();
            SharedPreferences prefs = ctx.getSharedPreferences("IABTCF_Preferences", Context.MODE_PRIVATE);
            return prefs.getString("IABTCF_TCString", "");
        } catch (Throwable t) {
            Log.w(TAG, "Failed to get IABTCF_TCString: " + t.getMessage());
            return "";
        }
    }

    private static String ensureValidJson(String s) {
        if (s == null) return "";
        String t = s.trim();
        if (t.startsWith("{")) {
            if (t.indexOf('"') == -1 && t.indexOf('\'') != -1) {
                return t.replace('\'', '"');
            }
        }
        return s;
    }

    private static String normalizeOpenRtbJson(String s) {
        try {
            JSONObject root = new JSONObject(s);

            JSONObject site = root.optJSONObject("site");
            if (site == null) {
                site = new JSONObject();
                site.put("page", DEFAULT_SITE_PAGE);
                root.put("site", site);
            } else if (!site.has("id") && !site.has("page")) {
                site.put("page", DEFAULT_SITE_PAGE);
            }

            JSONObject device = root.optJSONObject("device");
            if (device == null) {
                device = new JSONObject();
                device.put("ua", "UnityPlayer");
                device.put("ip", "127.0.0.1");
                root.put("device", device);
            } else {
                if (!device.has("ua")) device.put("ua", "UnityPlayer");
                if (!device.has("ip")) device.put("ip", "127.0.0.1");
            }

            return root.toString();
        } catch (Exception e) {
            Log.e(TAG, "[Prebid] normalizeOpenRtbJson error: " + e.getMessage());
            return s;
        }
    }

    private static String httpPostJson(String url, String jsonBody) {
        try {
            RequestBody body = RequestBody.create(jsonBody, MEDIA_TYPE_JSON);
            Request request = new Request.Builder()
                    .url(url)
                    .post(body)
                    .header("Accept", "application/json")
                    .header("Content-Type", "application/json; charset=utf-8")
                    .build();

            try (Response resp = HTTP.newCall(request).execute()) {
                String respBody = (resp.body() != null) ? resp.body().string() : "";
                if (!resp.isSuccessful()) {
                    Log.e(TAG, "[Prebid] HTTP " + resp.code() + " body=" + (respBody != null ? respBody : ""));
                }
                return respBody;
            }
        } catch (Exception e) {
            Log.e(TAG, "[Prebid] httpPostJson error: " + e.getMessage());
            return "";
        }
    }
}
