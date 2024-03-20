using Monetizr.SDK.Debug;
using UnityEditor;
using UnityEngine;

namespace Monetizr.SDK
{
    internal class MonetizrMenu : UnityEditor.Editor
    {
        [MenuItem("Monetizr/Clean local data", false, -1)]
        internal static void CleanupLocalSaves()
        {
            PlayerPrefs.SetString("campaigns", "");
            PlayerPrefs.SetString("missions", "");
            Log.Print($"PlayerPrefs cleaned {PlayerPrefs.GetString("campaigns")}");
            Log.Print($"PlayerPrefs cleaned {PlayerPrefs.GetString("missions")}");
        }
    }
}