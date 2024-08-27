#define MONETIZR_VERBOSE

using System;
using UnityEngine;

namespace Monetizr.SDK.Debug
{
    public static class MonetizrLog
    {
        public static bool isEnabled { set; get; } = false;
        
        public static void Print (object message)
        {
            if (!isEnabled) return;
            PrintToConsole(message);
        }
        
        private static void PrintToConsole (object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Monetizr SDK: {message}");
#else
            Console.WriteLine($"Monetizr SDK: {message}");
#endif
        }

        public static void PrintError (object message)
        {
            UnityEngine.Debug.LogError ($"Monetizr SDK: {message}");
        }

        public static void PrintWarning (object message)
        {
            UnityEngine.Debug.LogWarning($"Monetizr SDK: {message}");
        }

    }

}
