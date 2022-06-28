using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Monetizr.Campaigns
{
    internal class MonetizrMenu : UnityEditor.Editor
    {
        [MenuItem("Monetizr/Clean local data", false, -1)]
        internal static void CleanupLocalSaves()
        {
            PlayerPrefs.SetString("campaigns", "");
            Debug.Log($"PlayerPrefs cleaned {PlayerPrefs.GetString("campaigns")}");
        }
    }
}