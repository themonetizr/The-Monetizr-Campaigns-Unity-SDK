#define MONETIZR_VERBOSE

using UnityEngine;

namespace Monetizr.SDK.Debug
{
    public static class Log
    {
        public static bool isVerbose { set; get; } = false;
        
        public static void PrintV(object message)
        {
            if(!isVerbose)
                return;
            
            Print(message);
        }

        public static void Print(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Monetizr SDK: {message}");
#else
            PrintLine($"Monetizr SDK: {message}");
#endif
        }

        public static void PrintLine(object message)
        {
            UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", message);
        }

        public static void PrintError(object message)
        {
            UnityEngine.Debug.LogError($"Monetizr SDK: {message}");
        }

        public static void PrintWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"Monetizr SDK: {message}");
        }
    }
}
