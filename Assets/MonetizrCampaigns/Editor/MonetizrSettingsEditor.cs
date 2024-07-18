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
                    Editor.CreateEditor(MonetizrSettingsMenu.Instance).OnInspectorGUI();
                },

                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(MonetizrSettingsMenu.Instance))
            };

            return provider;
        }
    }
}