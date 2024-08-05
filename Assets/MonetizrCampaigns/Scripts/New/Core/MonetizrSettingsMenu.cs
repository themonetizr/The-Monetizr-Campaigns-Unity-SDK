using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Monetizr.SDK.Debug;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Monetizr.SDK.Core
{
    public class MonetizrSettingsMenu : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("Should Unity Editor use the Android Settings.")]
        public bool shouldUnityUseAndroidSettings = true;
        [Tooltip("Should Bundle ID be automatically set as the Application Identifier.")]
        public bool shouldBundleIDBeApplicationIdentifier = false;

        [Header("Android Settings")]
        public string androidBundleID;
        public string androidAPIKey;

        [Header("iOS Settings")]
        public string iOSBundleID;
        public string iOSAPIKey;

        private static MonetizrSettingsMenu _instance;

        public static void LoadSettings()
        {
            if (!_instance)
            {
                _instance = FindOrCreateInstance();

#if UNITY_ANDROID
                MonetizrSettings.bundleID = _instance.androidBundleID;
                MonetizrSettings.apiKey = _instance.androidAPIKey;
#elif UNITY_IOS
                MonetizrSettings.bundleID = _instance.iOSBundleID;
                MonetizrSettings.apiKey = _instance.iOSAPIKey;
#else
                if (_instance.shouldUnityUseAndroidSettings)
                {
                    MonetizrSettings.bundleID = _instance.androidBundleID;
                    MonetizrSettings.apiKey = _instance.androidAPIKey;
                }
                else
                {
                    MonetizrSettings.bundleID = _instance.iOSBundleID;
                    MonetizrSettings.apiKey = _instance.iOSAPIKey;
                }
#endif

                if (_instance.shouldBundleIDBeApplicationIdentifier) MonetizrSettings.bundleID = Application.identifier;
            }
        }

        public static MonetizrSettingsMenu Instance
        {
            get
            {
                LoadSettings();
                return _instance;
            }
        }

        private static MonetizrSettingsMenu FindOrCreateInstance()
        {
            MonetizrSettingsMenu instance = null;
            instance = instance ? null : Resources.Load<MonetizrSettingsMenu>("MonetizrSettings");
            instance = instance ? instance : Resources.LoadAll<MonetizrSettingsMenu>(string.Empty).FirstOrDefault();
            instance = instance ? instance : CreateAndSave<MonetizrSettingsMenu>();
            if (instance == null) throw new Exception("Could not find or create settings for Monetizr.");
            return instance;
        }

        private static T CreateAndSave<T>() where T : ScriptableObject
        {
            T instance = CreateInstance<T>();
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += () => SaveAsset(instance);
            }
            else
            {
                SaveAsset(instance);
            }
#endif
            return instance;
        }

#if UNITY_EDITOR
        private static void SaveAsset<T>(T obj) where T : ScriptableObject
        {

            string dirName = "Assets/Resources";
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            AssetDatabase.CreateAsset(obj, "Assets/Resources/MonetizrSettings.asset");
            AssetDatabase.SaveAssets();
        }
#endif

    }

}