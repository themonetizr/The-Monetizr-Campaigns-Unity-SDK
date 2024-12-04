using Monetizr.SDK.Utils;

namespace Monetizr.SDK.Debug
{
    public static class MonetizrLogger
    {
        public static bool isEnabled { set; get; } = false;
        
        public static void Print (object message)
        {
            if (!isEnabled) return;
            PrintToConsole(message);
        }
        
        private static void PrintToConsole (object message)
        {
            UnityEngine.Debug.Log($"Monetizr SDK: {message}");
        }

        public static void PrintError (object message)
        {
            UnityEngine.Debug.LogError ($"Monetizr SDK: {message}");
        }

        public static void PrintWarning (object message)
        {
            UnityEngine.Debug.LogWarning($"Monetizr SDK: {message}");
        }

        public static void PrintLocalMessage (MessageEnum messageEnum)
        {
            string messageString = EnumUtils.GetEnumDescription(messageEnum);
            if (EnumUtils.IsEnumError(messageEnum))
            {
                PrintError(messageString);
            }
            else
            {
                PrintToConsole(messageString);
            }
        }

        public static void PrintRemoteMessage (MessageEnum messageEnum)
        {
            PrintLocalMessage(messageEnum);
            GCPManager.Instance.Log(messageEnum);
        }
    }
}
