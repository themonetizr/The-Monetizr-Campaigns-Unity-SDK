#define MONETIZR_VERBOSE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{
    public static class Log
    {
        public static void Print(object message)
        {
#if UNITY_EDITOR
            Debug.Log($"Monetizr SDK: {message}");
#else
            PrintLine($"Monetizr SDK: {message}");
#endif
        }

        public static void PrintLine(object message)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", message);
        }


        // ReSharper disable Unity.PerformanceAnalysis
        public static void PrintError(object message)
        {
            Debug.LogError($"Monetizr SDK: {message}");
        }

        public static void PrintWarning(object message)
        {
            Debug.LogWarning($"Monetizr SDK: {message}");
        }
    }
}
