package com.monetizr.prebidbridge;

import android.app.Activity;
import android.util.Log;

import org.prebid.mobile.Host;
import org.prebid.mobile.PrebidMobile;
import org.prebid.mobile.VideoAdUnit;
import org.prebid.mobile.VideoParameters;
import org.prebid.mobile.ResultCode;

import java.util.HashMap;
import java.util.Arrays;

public class PrebidBridge {

    private static final String TAG = "PrebidBridge";

    public interface UnityCallback {
        void onResult(String vastUrl);
    }

    public static void init(Activity activity, String hostUrl) {
        try {
            if (hostUrl != null && !hostUrl.isEmpty()) {
                PrebidMobile.setPrebidServerHost(Host.createCustomHost(hostUrl));
            } else {
                PrebidMobile.setPrebidServerHost(Host.APPNEXUS); // fallback
            }

            PrebidMobile.setLogLevel(PrebidMobile.LogLevel.DEBUG);

            PrebidMobile.initializeSdk(activity, status ->
                    Log.d(TAG, "[Prebid] SDK Initialized: " + status));
        } catch (Throwable t) {
            Log.e(TAG, "[Prebid] Init error: " + t.getMessage());
        }
    }

    public static void fetchDemand(final Activity activity,
                               final String storedRequestId,
                               final UnityCallback cb) {
        try {
            if (storedRequestId == null || storedRequestId.isEmpty()) {
                Log.e(TAG, "[Prebid] Stored request ID is missing");
                cb.onResult("");
                return;
            }

            // Create video ad unit
            VideoAdUnit adUnit = new VideoAdUnit(storedRequestId, 640, 480);

            // Video parameters
            VideoParameters parameters = new VideoParameters(
                    Arrays.asList("video/mp4")
            );
            adUnit.setVideoParameters(parameters);

            // Map to collect Prebid targeting keywords
            HashMap<String, String> targetingMap = new HashMap<>();

            // Fetch demand
            adUnit.fetchDemand(targetingMap, resultCode -> {
                if (resultCode != ResultCode.SUCCESS) {
                    Log.e(TAG, "[Prebid] No demand received: " + resultCode);
                    cb.onResult("");
                } else {
                    String cacheId = targetingMap.get("hb_cache_id");
                    if (cacheId == null) cacheId = "";

                    Log.d(TAG, "[Prebid] Demand result hb_cache_id: " + cacheId);
                    cb.onResult(cacheId);
                }
            });

        } catch (Throwable t) {
            Log.e(TAG, "[Prebid] fetchDemand error: " + t.getMessage());
            cb.onResult("");
        }
    }


}
