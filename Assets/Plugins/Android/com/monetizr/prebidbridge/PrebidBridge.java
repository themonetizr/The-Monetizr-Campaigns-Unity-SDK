package com.monetizr.prebidbridge;

import android.app.Activity;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONObject;
import org.json.JSONException;

import org.prebid.mobile.Host;
import org.prebid.mobile.PrebidMobile;
import org.prebid.mobile.VideoAdUnit;
import org.prebid.mobile.AdSize;

import java.util.Iterator;

public class PrebidBridge {
    private static final String TAG = "PrebidBridge";

    public static void init(Activity activity, String jsonString) {
        try {
            Log.d(TAG, "[Prebid] Initializing with JSON: " + jsonString);

            JSONObject json = new JSONObject(jsonString);

            // Set account ID
            String accountId = json.optString("accountId", null);
            if (accountId != null) {
                PrebidMobile.setPrebidServerAccountId(accountId);
                Log.d(TAG, "[Prebid] Account ID set: " + accountId);
            }

            // Set custom host if provided
            String hostUrl = json.optString("host", null);
            if (hostUrl != null) {
                PrebidMobile.setPrebidServerHost(Host.createCustomHost(hostUrl));
                Log.d(TAG, "[Prebid] Custom Host set: " + hostUrl);
            }

            // Enable debug logging
            PrebidMobile.setLogLevel(PrebidMobile.LogLevel.DEBUG);

            // Create video ad unit
            String configId = json.optString("configId", "test-config-id");
            int width = json.optInt("width", 640);
            int height = json.optInt("height", 480);

            VideoAdUnit adUnit = new VideoAdUnit(configId, width, height);
            Log.d(TAG, "[Prebid] Created VideoAdUnit with configId: " + configId);

            // Add context data (keywords)
            JSONObject keywords = json.optJSONObject("keywords");
            if (keywords != null) {
                Iterator<String> keys = keywords.keys();
                while (keys.hasNext()) {
                    String name = keys.next();
                    String value = keywords.optString(name);
                    adUnit.addContextData(name, value);
                    Log.d(TAG, "[Prebid] Context Data added: " + name + " = " + value);
                }
            }

            Log.d(TAG, "[Prebid] Initialization complete.");
        } catch (JSONException e) {
            Log.e(TAG, "[Prebid] Failed to parse JSON: " + e.getMessage());
        } catch (Exception e) {
            Log.e(TAG, "[Prebid] Unexpected error during init: " + e.getMessage());
        }
    }
}
