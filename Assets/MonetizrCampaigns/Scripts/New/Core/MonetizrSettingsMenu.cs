using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Monetizr.SDK.Core;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MonetizrSettingsMenu : ScriptableObject
{
    [Header("Android Settings")]
    public string androidBundleID;
    public string androidAPIKey;

    [Header("iOS Settings")]
    public string iOSBundleID;
    public string iOSAPIKey;

    [Header("Game Reward Settings")]
    public List<TestGameReward> gameRewards;

    private static MonetizrSettingsMenu _instance;

    public static void LoadSettings()
    {
        if (!_instance)
        {
            _instance = FindOrCreateInstance();
            MonetizrSettings.bundleID = _instance.androidBundleID;
            MonetizrSettings.apiKey = _instance.androidAPIKey;
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

[Serializable]
public class TestGameReward
{
    public RewardType rewardType;
    public Texture2D icon;
    public string name;
    public int maxValue;
}
