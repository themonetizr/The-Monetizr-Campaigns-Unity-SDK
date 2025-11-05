using Monetizr.SDK.Utils;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Monetizr.SDK.Debug
{
    public static class MonetizrLogger
    {
        public static bool isEnabled { set; get; } = false;

        public static void Print (object message, bool forwardToRemote = false, [CallerFilePath] string filePath = "")
        {
            LogInternal(LogType.Log, message, forwardToRemote, filePath);
        }

        public static void PrintWarning (object message, bool forwardToRemote = false, [CallerFilePath] string filePath = "")
        {
            LogInternal(LogType.Warning, message, forwardToRemote, filePath);
        }

        public static void PrintError (object message, bool forwardToRemote = false, [CallerFilePath] string filePath = "")
        {
            LogInternal(LogType.Error, message, forwardToRemote, filePath);
        }

        private static void LogInternal (LogType type, object message, bool forwardToRemote, string filePath)
        {
            if (!isEnabled) return;

            string className = MonetizrUtils.ExtractClassName(filePath);
            string formatted = $"Monetizr SDK: [{className}] {message}";

            switch (type)
            {
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(formatted);
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(formatted);
                    break;
                default:
                    UnityEngine.Debug.Log(formatted);
                    break;
            }

            if (forwardToRemote && GCPManager.Instance)
            {
                bool isError = type == LogType.Error;
                GCPManager.Instance.Log(formatted, isError);
            }
        }

    }
}
