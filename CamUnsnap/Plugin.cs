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
        internal static ConfigEntry<KeyboardShortcut> ChangeGamespeed;
        internal static ConfigEntry<KeyboardShortcut> CamForward;
        internal static ConfigEntry<KeyboardShortcut> CamBack;
        internal static ConfigEntry<KeyboardShortcut> CamLeft;
        internal static ConfigEntry<KeyboardShortcut> CamRight;
        internal static ConfigEntry<KeyboardShortcut> CamUp;
        internal static ConfigEntry<KeyboardShortcut> CamDown;
        internal static ConfigEntry<float> Gamespeed;
        internal static ConfigEntry<float> CameraMoveSpeed;
        internal static ConfigEntry<float> CameraSensitivity;

        private void Awake()
        {
            ToggleCameraSnap = Config.Bind(KeybindSectionName, "Toggle Camera Snap", new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl),"Allows you to unsnap the camera at will");
            CameraMoveSpeed = Config.Bind(CameraSettingsSectionName, "CameraMoveSpeed", 10f, new ConfigDescription("How fast you want the camera to move", new AcceptableValueRange<float>(0.01f, 100f)));
            CameraSensitivity = Config.Bind(CameraSettingsSectionName, "Camera Sensitivity", 10f, new ConfigDescription("How fast you want the camera viewport to move while slaved", new AcceptableValueRange<float>(0.0f, 100f)));
            CameraMouse = Config.Bind(KeybindSectionName, "Switch camera control to mouse", new KeyboardShortcut(KeyCode.Equals), "Lets you contol the camera viewport with the mouse, switch between enabling to pose your character");
            ChangeGamespeed = Config.Bind(CameraSettingsSectionName, "Change Gamespeed", new KeyboardShortcut(KeyCode.Tilde), "Toggle that sets the gamespeed to what you set using the gamespeed slider");
            Gamespeed = Config.Bind(CameraSettingsSectionName, "Set Gamespeed", 1f, new ConfigDescription("What gamespeed you want to set the gameworld to when pressing the Change Gamespeed bind !WARNING! Changing the gamespeed for too long can cause weird (but temporary) side effects", new AcceptableValueRange<float>(0f, 1f)));
            CamForward = Config.Bind(CameraSettingsSectionName, "Move Forward", new KeyboardShortcut(KeyCode.UpArrow), "Moves the camera forwards");
            CamBack = Config.Bind(CameraSettingsSectionName, "Move Back", new KeyboardShortcut(KeyCode.DownArrow), "Moves the camera backwards");
            CamLeft = Config.Bind(CameraSettingsSectionName, "Move Left", new KeyboardShortcut(KeyCode.LeftArrow), "Moves the camera left");
            CamRight = Config.Bind(CameraSettingsSectionName, "Move Right", new KeyboardShortcut(KeyCode.RightArrow), "Moves the camera right");
            CamUp = Config.Bind(CameraSettingsSectionName, "Move Up", new KeyboardShortcut(KeyCode.Space), "Moves the camera up");
            CamDown = Config.Bind(CameraSettingsSectionName, "Move Down", new KeyboardShortcut(KeyCode.LeftControl), "Moves the camera down");

            Hook = new GameObject("CUS");
            Hook.AddComponent<CUSController>();
            DontDestroyOnLoad(Hook);
            Logger.LogInfo($"Camera Unsnap Loaded");
        }
    }
}
