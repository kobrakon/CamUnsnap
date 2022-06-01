using BepInEx;
using UnityEngine;
using BepInEx.Configuration;

namespace CamUnSnap
{
    [BepInPlugin("com.kobrakon.camunsnap", "CamUnsnap", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static GameObject Hook;
        private const string KeybindSectionName = "Keybinds";
        internal static ConfigEntry<KeyboardShortcut> ToggleCameraSnap;
        
        private void Awake()
        {
            ToggleCameraSnap = Config.Bind(KeybindSectionName, "Toggle Camera Snap", new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl));
            Hook = new GameObject("CUS");
            Hook.AddComponent<CUSController>();
            DontDestroyOnLoad(Hook);
            Logger.LogInfo($"Camera Unsnap Loaded");
        }
    }
}
