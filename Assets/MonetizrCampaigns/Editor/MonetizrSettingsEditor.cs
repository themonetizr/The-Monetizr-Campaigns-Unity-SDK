using UnityEditor;

namespace Monetizr
{
    public class MonetizrSettingsEditor
    {
        [SettingsProvider]
        internal static SettingsProvider CreateCustomSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Project/Monetizr", SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    Editor.CreateEditor(MonetizrSettings.Instance).OnInspectorGUI();
                },

                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(MonetizrSettings.Instance))
            };

            return provider;
        }
    }
}