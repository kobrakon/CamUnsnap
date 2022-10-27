using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace CamUnsnap
{
    [BepInPlugin("com.kobrakon.camunsnap", "CamUnsnap", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject Hook;
        private const string KeybindSectionName = "Keybinds";
        private const string CameraSettingsSectionName = "Camera Control Settings";
        internal static ManualLogSource logger;
        internal static ConfigEntry<KeyboardShortcut> ToggleCameraSnap;
        internal static ConfigEntry<KeyboardShortcut> CameraMouse;
        internal static ConfigEntry<KeyboardShortcut> ChangeGamespeed;
        internal static ConfigEntry<KeyboardShortcut> CamForward;
        internal static ConfigEntry<KeyboardShortcut> CamBack;
        internal static ConfigEntry<KeyboardShortcut> CamLeft;
        internal static ConfigEntry<KeyboardShortcut> CamRight;
        internal static ConfigEntry<KeyboardShortcut> CamUp;
        internal static ConfigEntry<KeyboardShortcut> CamDown;
        internal static ConfigEntry<KeyboardShortcut> RememberPos;
        internal static ConfigEntry<KeyboardShortcut> GoToPos;
        internal static ConfigEntry<KeyboardShortcut> MovePlayerToCam;
        internal static ConfigEntry<KeyboardShortcut> LockPlayerMovement;
        internal static ConfigEntry<float> Gamespeed;
        internal static ConfigEntry<float> MovementSpeed;
        internal static ConfigEntry<float> CameraSensitivity;
        internal static ConfigEntry<bool> ImmuneInCamera;

        private void Awake()
        {
            logger = Logger;
            ToggleCameraSnap = Config.Bind(KeybindSectionName, "Toggle Camera Snap", new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl),"Allows you to unsnap the camera at will");
            MovementSpeed = Config.Bind(CameraSettingsSectionName, "CameraMoveSpeed", 10f, new ConfigDescription("How fast you want the camera to move", new AcceptableValueRange<float>(0.01f, 100f)));
            CameraSensitivity = Config.Bind(CameraSettingsSectionName, "Camera Sensitivity", 10f, new ConfigDescription("How fast you want the camera viewport to move while slaved", new AcceptableValueRange<float>(0.0f, 100f)));
            Gamespeed = Config.Bind(CameraSettingsSectionName, "Set Gamespeed", 1f, new ConfigDescription("What gamespeed you want to set the gameworld to when pressing the Change Gamespeed bind !WARNING! Changing the gamespeed for too long can cause weird (but temporary) side effects", new AcceptableValueRange<float>(0f, 1f)));
            CameraMouse = Config.Bind(KeybindSectionName, "Switch camera control to mouse", new KeyboardShortcut(KeyCode.Equals), "Lets you contol the camera viewport with the mouse, switch between enabling to pose your character");
            ChangeGamespeed = Config.Bind(CameraSettingsSectionName, "Change Gamespeed", new KeyboardShortcut(KeyCode.Tilde), "Toggle that sets the gamespeed to what you set using the gamespeed slider");
            CamForward = Config.Bind(KeybindSectionName, "Move Forward", new KeyboardShortcut(KeyCode.UpArrow), "Moves the camera forwards");
            CamBack = Config.Bind(KeybindSectionName, "Move Back", new KeyboardShortcut(KeyCode.DownArrow), "Moves the camera backwards");
            CamLeft = Config.Bind(KeybindSectionName, "Move Left", new KeyboardShortcut(KeyCode.LeftArrow), "Moves the camera left");
            CamRight = Config.Bind(KeybindSectionName, "Move Right", new KeyboardShortcut(KeyCode.RightArrow), "Moves the camera right");
            CamUp = Config.Bind(KeybindSectionName, "Move Up", new KeyboardShortcut(KeyCode.Space), "Moves the camera up");
            CamDown = Config.Bind(KeybindSectionName, "Move Down", new KeyboardShortcut(KeyCode.LeftControl), "Moves the camera down");
            RememberPos = Config.Bind(KeybindSectionName, "Remember Camera Position", new KeyboardShortcut(KeyCode.O), "Save the camera's current Vector3 position.");
            GoToPos = Config.Bind(KeybindSectionName, "Go to Memory Position", new KeyboardShortcut(KeyCode.P), "Moves the camera to the last remembered Vector3 position.");
            MovePlayerToCam = Config.Bind(KeybindSectionName, "Move Player to Camera Position", new KeyboardShortcut(KeyCode.RightAlt), "Moves the player to the camera's position.");
            LockPlayerMovement = Config.Bind(CameraSettingsSectionName, "Lock Player Movement", new KeyboardShortcut(KeyCode.RightControl), "Locks player body movement when pressed");
            ImmuneInCamera = Config.Bind(CameraSettingsSectionName, "Immune In Camera Mode", true, "Makes the player unkillable while camera mode is active");

            Hook = new GameObject("CUS");
            Hook.AddComponent<CUSController>();
            DontDestroyOnLoad(Hook);
            Logger.LogInfo($"Camera Unsnap Loaded");
        }
    }
}
