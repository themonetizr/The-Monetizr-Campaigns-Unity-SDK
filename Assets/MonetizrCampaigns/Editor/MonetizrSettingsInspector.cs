using Monetizr.SDK.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonetizrSettings))]
public class MonetizrSettingsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUI.DrawRect(EditorGUILayout.BeginVertical(), new Color(0.4f, 0.4f, 0.4f));
        EditorGUILayout.LabelField(new GUIContent("SDK Version", "The current version of the SDK."), new GUIContent(MonetizrConfiguration.SDKVersion));
        EditorGUILayout.EndVertical();
    }
}
