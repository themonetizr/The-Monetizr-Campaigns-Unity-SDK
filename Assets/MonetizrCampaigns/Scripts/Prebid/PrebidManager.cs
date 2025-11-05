// Assets/Monetizr/Prebid/PrebidManager.cs
// Unity-side bridge for Monetizr + Prebid (Android + iOS)
// - Android: unchanged JNI flow (AAR).
// - iOS: safe reverse P/Invoke with GCHandle + explicit prebid_free_string.

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Prebid
{
    public static class PrebidManager
    {
        // ---------- iOS native bindings ----------
#if UNITY_IOS && !UNITY_EDITOR
        private const string Dll = "__Internal";

        // Native signatures (MUST match the native header/impl you'll paste next turn)
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void prebid_init_default_host();

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void prebid_init_with_host(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string hostUrlUtf8);

        // Callback signature: cb(const char* resultUtf8, void* userData)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PBUnityStaticCallback(IntPtr resultUtf8, IntPtr userData);

        // Async demand: prebid_fetch_demand(prebidDataUtf8, hostUrlUtf8, cb, userData)
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void prebid_fetch_demand(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string prebidDataUtf8,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string hostUrlUtf8,
            PBUnityStaticCallback cb,
            IntPtr userData);

        // Privacy helpers return malloc'ed UTF-8 C strings (must be freed via prebid_free_string)
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prebid_get_iab_tcf_consent();

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prebid_get_iab_us_privacy();

        // Free a C string allocated by the native bridge (strdup/malloc)
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void prebid_free_string(IntPtr p);

        // Keep managed callbacks alive across native async calls
        private static class CallbackKeeper
        {
            public static IntPtr MakeHandle(Action<string> cb)
            {
                var handle = GCHandle.Alloc(cb ?? (_ => { }));
                return GCHandle.ToIntPtr(handle);
            }

            public static Action<string> Get(IntPtr h)
            {
                if (h == IntPtr.Zero) return null;
                var gh = GCHandle.FromIntPtr(h);
                return gh.Target as Action<string>;
            }

            public static void ReleaseHandle(IntPtr h)
            {
                if (h == IntPtr.Zero) return;
                var gh = GCHandle.FromIntPtr(h);
                if (gh.IsAllocated) gh.Free();
            }
        }

        // Safe UTF-8 copy + free
        private static string TakeUtf8StringAndFree(IntPtr p)
        {
            try
            {
                if (p == IntPtr.Zero) return string.Empty;
#if UNITY_2021_2_OR_NEWER
                return Marshal.PtrToStringUTF8(p) ?? string.Empty;
#else
                int len = 0;
                while (Marshal.ReadByte(p, len) != 0) len++;
                var buf = new byte[len];
                Marshal.Copy(p, buf, 0, len);
                return System.Text.Encoding.UTF8.GetString(buf);
#endif
            }
            finally
            {
                try { if (p != IntPtr.Zero) prebid_free_string(p); } catch { /* ignore */ }
            }
        }

        // Reverse P/Invoke trampoline (must be static + AOT attribute)
        [AOT.MonoPInvokeCallback(typeof(PBUnityStaticCallback))]
        private static void OnNativeDemandFinished(IntPtr jsonUtf8, IntPtr userData)
        {
            try
            {
                var cb = CallbackKeeper.Get(userData);
                var s = TakeUtf8StringAndFree(jsonUtf8);
                MonetizrLogger.Print($"[Prebid][iOS] result: {Truncate(s, 200)}");
                cb?.Invoke(s ?? string.Empty);
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid][iOS] callback exception: {ex.Message}");
            }
            finally
            {
                CallbackKeeper.ReleaseHandle(userData);
            }
        }

        private static string Truncate(string s, int max)
            => string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s.Substring(0, max));
#endif // UNITY_IOS && !UNITY_EDITOR

        // ---------- Android callback proxy ----------
#if UNITY_ANDROID && !UNITY_EDITOR
        private class CallbackProxy : AndroidJavaProxy
        {
            private readonly Action<string> _onResult;
            public CallbackProxy(Action<string> onResult)
                : base("com.monetizr.prebidbridge.PrebidBridge$UnityCallback")
            {
                _onResult = onResult;
            }
            public void onResult(string vastUrl)
            {
                MonetizrLogger.Print($"[Prebid][Android] onResult: {vastUrl}");
                _onResult?.Invoke(vastUrl ?? "");
            }
        }
#endif

        // ---------- Public API ----------
        public static void InitializePrebid(string host = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                if (!string.IsNullOrEmpty(host))
                    bridge.CallStatic("initPrebid", host);
                else
                    MonetizrLogger.PrintWarning("[Prebid] No host provided. Skipping Prebid init (Android).");
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid][Android] init exception: {ex.Message}");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            try
            {
                if (!string.IsNullOrEmpty(host))
                    prebid_init_with_host(host);
                else
                    prebid_init_default_host();
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid][iOS] init exception: {ex.Message}");
            }
#else
            MonetizrLogger.Print("[Prebid] Init skipped (not on device).");
#endif
        }

        /// <summary>
        /// prebidData:
        ///   - Mode A (raw OpenRTB JSON): a JSON string starting with '{' + prebidHost set to Prebid Server URL.
        ///   - Mode B (configId): a non-JSON string (e.g., "abc123"); prebidHost may be empty.
        /// </summary>
        public static void FetchDemand(string prebidData, string prebidHost, Action<string> onResult)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                var proxy = new CallbackProxy(onResult);
                bridge.CallStatic("fetchDemand", activity, prebidData ?? "", prebidHost ?? "", proxy);
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid][Android] FetchDemand exception: {ex.Message}");
                onResult?.Invoke("");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            try
            {
                IntPtr handle = CallbackKeeper.MakeHandle(onResult);
                prebid_fetch_demand(prebidData ?? "", prebidHost ?? "", OnNativeDemandFinished, handle);
            }
            catch (Exception ex)
            {
                MonetizrLogger.Print($"[Prebid][iOS] FetchDemand exception: {ex.Message}");
                onResult?.Invoke("");
            }
#else
            onResult?.Invoke("");
#endif
        }

        public static string GetIabConsentString()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var bridge = new AndroidJavaClass("com.monetizr.prebidbridge.PrebidBridge");
                return bridge.CallStatic<string>("getIabTcfConsent") ?? "";
            }
            catch { return ""; }
#elif UNITY_IOS && !UNITY_EDITOR
            try
            {
                var p = prebid_get_iab_tcf_consent();
                return TakeUtf8StringAndFree(p);
            }
            catch { return ""; }
#else
            return "";
#endif
        }

        public static string GetIabUsPrivacyString()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Add Android path if you wire it later; currently iOS-only.
            return "";
#elif UNITY_IOS && !UNITY_EDITOR
            try
            {
                var p = prebid_get_iab_us_privacy();
                return TakeUtf8StringAndFree(p);
            }
            catch { return ""; }
#else
            return "";
#endif
        }
    }
}
