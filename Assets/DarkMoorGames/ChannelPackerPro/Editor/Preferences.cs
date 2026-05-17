using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class Preferences : ScriptableObject
    {
        public bool autoUpdate = true;
        public int jpgQuality = 100;
        public string prefixTextColorHtml = "#00BFFF";
        public string suffixTextColorHtml = "#FF9900";
        public string wireframeColorHtml = "#FF4C00";
        public string lastLoadSaveDirectory = "Assets";

        public static Preferences Create()
        {
            Preferences preferences = CreateInstance<Preferences>();
            preferences.hideFlags = HideFlags.HideAndDontSave;
            return preferences;
        }
        void Awake()
        {
            autoUpdate = EditorPrefs.GetBool("ChannelPackerProAutoUpdate", true);
            jpgQuality = EditorPrefs.GetInt("ChannelPackerProJpgQuality", 100);
            prefixTextColorHtml = EditorPrefs.GetString("ChannelPackerProPrefixTextColor", "#00BFFF");
            suffixTextColorHtml = EditorPrefs.GetString("ChannelPackerProSuffixTextColor", "#FF9900");
            wireframeColorHtml = EditorPrefs.GetString("ChannelPackerProWireframeColor", "#FF4C00");
            lastLoadSaveDirectory = EditorPrefs.GetString("ChannelPackerPreviousDirectory", "Assets");
        }
        void OnDestroy()
        {
            EditorPrefs.SetBool("ChannelPackerProAutoUpdate", autoUpdate);
            EditorPrefs.SetInt("ChannelPackerProJpgQuality", jpgQuality);
            EditorPrefs.SetString("ChannelPackerProPrefixTextColor", prefixTextColorHtml);
            EditorPrefs.SetString("ChannelPackerProSuffixTextColor", suffixTextColorHtml);
            EditorPrefs.SetString("ChannelPackerProWireframeColor", wireframeColorHtml);
            EditorPrefs.SetString("ChannelPackerPreviousDirectory", lastLoadSaveDirectory);
        }
        public void ResetToDefault()
        {
            autoUpdate = true;
            jpgQuality = 100;
            prefixTextColorHtml = "#00BFFF";
            suffixTextColorHtml = "#FF9900";
            wireframeColorHtml = "#FF4C00";
            lastLoadSaveDirectory = "Assets";
        }
    }
}
