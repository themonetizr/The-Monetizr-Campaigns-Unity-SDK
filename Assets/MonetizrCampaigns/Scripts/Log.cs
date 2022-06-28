#define MONETIZR_VERBOSE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{
    internal static class Log
    {
        internal static void Print(object message)
        {
#if MONETIZR_VERBOSE
            Debug.Log(message);
#endif
        }

        internal static void PrintError(object message)
        {
            Debug.LogError(message);
        }

        internal static void PrintWarning(object message)
        {
            Debug.LogWarning(message);
        }
    }
}
