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
        private const string CameraSettingsSectionName = "Camera Control Settings";
        internal static ConfigEntry<KeyboardShortcut> ToggleCameraSnap;
        internal static ConfigEntry<KeyboardShortcut> CameraMouse;
        internal static ConfigEntry<float> CameraMoveSpeed;

        private void Awake()
        {
            ToggleCameraSnap = Config.Bind(KeybindSectionName, "Toggle Camera Snap", new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl), "Allows you to unsnap the camera at will");
            CameraMoveSpeed = Config.Bind(CameraSettingsSectionName, "CameraMoveSpeed", 10f, new ConfigDescription("How fast you want the camera to move", new AcceptableValueRange<float>(0.01f, 100f)));
            CameraMouse = Config.Bind(KeybindSectionName, "Switch camera control to mouse", new KeyboardShortcut(KeyCode.Equals), "Lets you contol the camera viewport with the mouse, switch between enabling to pose your character");

            Hook = new GameObject("CUS");
            Hook.AddComponent<CUSController>();
            DontDestroyOnLoad(Hook);
            Logger.LogInfo($"Camera Unsnap Loaded");
        }
    }
}
