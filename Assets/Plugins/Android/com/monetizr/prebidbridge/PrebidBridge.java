package com.monetizr.prebidbridge;

import android.app.Activity;
import android.util.Log;

import org.prebid.mobile.PrebidMobile;
import org.prebid.mobile.Host;

public class PrebidBridge {

    private static final String TAG = "PrebidBridge";

    public static void init(Activity activity, String accountId) {
        Log.d(TAG, "[Prebid] Initializing SDK v2.0.3");

        // Set your account ID and server host
        PrebidMobile.setPrebidServerAccountId(accountId);
        PrebidMobile.setPrebidServerHost(Host.APPNEXUS);

        // Optional debug logging
        PrebidMobile.setLogLevel(PrebidMobile.LogLevel.DEBUG);

        // No need to call initializeSdk in this version
        Log.d(TAG, "[Prebid] Initialization complete (no async required in v2.0.3)");
    }
}
