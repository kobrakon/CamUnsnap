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
        internal static ConfigEntry<KeyboardShortcut> HideUI;
        internal static ConfigEntry<KeyboardShortcut> FastMove;
        internal static ConfigEntry<KeyboardShortcut> RotateLeft;
        internal static ConfigEntry<KeyboardShortcut> RotateRight;
        internal static ConfigEntry<KeyboardShortcut> ResetRotation;
        internal static ConfigEntry<KeyboardShortcut> AddToMemPosList;
        internal static ConfigEntry<KeyboardShortcut> AdvanceList;
        internal static ConfigEntry<KeyboardShortcut> ClearList;
        internal static ConfigEntry<KeyboardShortcut> BeginRecord;
        internal static ConfigEntry<KeyboardShortcut> ResumeRecord;
        internal static ConfigEntry<KeyboardShortcut> StopRecord;
        internal static ConfigEntry<KeyboardShortcut> PlayRecord;
        internal static ConfigEntry<float> Gamespeed;
        internal static ConfigEntry<float> MovementSpeed;
        internal static ConfigEntry<float> CameraSensitivity;
        internal static ConfigEntry<float> CameraSmoothing;
        internal static ConfigEntry<float> FastMoveMult;
        internal static ConfigEntry<int> CameraFOV;
        internal static ConfigEntry<bool> ImmuneInCamera;
        internal static ConfigEntry<bool> RotateUsesSens;
        internal static ConfigEntry<bool> OverrideGameRestriction;

        private void Awake()
        {
            logger = Logger;
            ToggleCameraSnap = Config.Bind(KeybindSectionName, "Toggle Camera Snap", new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl),"Allows you to unsnap the camera at will");
            CameraMouse = Config.Bind(KeybindSectionName, "Switch camera control to mouse", new KeyboardShortcut(KeyCode.Equals), "Lets you contol the camera viewport with the mouse, switch between enabling to pose your character");
            CamForward = Config.Bind(KeybindSectionName, "Move Forward", new KeyboardShortcut(KeyCode.UpArrow), "Moves the camera forwards");
            CamBack = Config.Bind(KeybindSectionName, "Move Back", new KeyboardShortcut(KeyCode.DownArrow), "Moves the camera backwards");
            CamLeft = Config.Bind(KeybindSectionName, "Move Left", new KeyboardShortcut(KeyCode.LeftArrow), "Moves the camera left");
            CamRight = Config.Bind(KeybindSectionName, "Move Right", new KeyboardShortcut(KeyCode.RightArrow), "Moves the camera right");
            CamUp = Config.Bind(KeybindSectionName, "Move Up", new KeyboardShortcut(KeyCode.Space), "Moves the camera up");
            CamDown = Config.Bind(KeybindSectionName, "Move Down", new KeyboardShortcut(KeyCode.LeftControl), "Moves the camera down");
            RotateLeft = Config.Bind(KeybindSectionName, "Rotate Left", new KeyboardShortcut(KeyCode.Q), "Rotates/tilts the camera to the left");
            RotateRight = Config.Bind(KeybindSectionName, "Rotate Right", new KeyboardShortcut(KeyCode.E), "Rotates/tilts the camera to the right");
            ResetRotation = Config.Bind(KeybindSectionName, "Reset Rotation", new KeyboardShortcut(KeyCode.Minus), "Resets camera rotation back to 0");
            FastMove = Config.Bind(KeybindSectionName, "Move Fast", new KeyboardShortcut(KeyCode.LeftShift), "Makes the camera move faster when held\nBasically like sprinting");
            RememberPos = Config.Bind(KeybindSectionName, "Remember Camera Position", new KeyboardShortcut(KeyCode.O), "Save the camera's current Vector3 position.");
            ChangeGamespeed = Config.Bind(KeybindSectionName, "Change Gamespeed", new KeyboardShortcut(KeyCode.Tilde), "Toggle that sets the gamespeed to what you set using the gamespeed slider");
            GoToPos = Config.Bind(KeybindSectionName, "Go to Memory Position", new KeyboardShortcut(KeyCode.P), "Moves the camera to the last remembered Vector3 position.");
            MovePlayerToCam = Config.Bind(KeybindSectionName, "Move Player to Camera Position", new KeyboardShortcut(KeyCode.RightAlt), "Moves the player to the camera's position.");
            LockPlayerMovement = Config.Bind(KeybindSectionName, "Lock Player Movement", new KeyboardShortcut(KeyCode.RightControl), "Locks player body movement when pressed");
            HideUI = Config.Bind(KeybindSectionName, "Hide UI", new KeyboardShortcut(KeyCode.Keypad7), "Hides the game UI");
            AddToMemPosList = Config.Bind(KeybindSectionName, "Add Position To Camera Memory Position List", new KeyboardShortcut(KeyCode.Plus), "Adds the current camera position into the Memory Position List");
            AdvanceList = Config.Bind(KeybindSectionName, "Advance Memory List Position", new KeyboardShortcut(KeyCode.Greater), "Changes the camera's position to the next position contained in the list");
            ClearList = Config.Bind(KeybindSectionName, "Clear Camera Memory Position List", new KeyboardShortcut(KeyCode.Less), "Empties the current Camera Memory Position List");
            BeginRecord = Config.Bind(KeybindSectionName, "Begin Path Recording", new KeyboardShortcut(KeyCode.LeftBracket), "Begins recording camera movement");
            ResumeRecord = Config.Bind(KeybindSectionName, "Continue Path Recording", new KeyboardShortcut(KeyCode.Backslash), "Resumes recording and appends it to the previous recording");
            StopRecord = Config.Bind(KeybindSectionName, "End Path Recording", new KeyboardShortcut(KeyCode.RightBracket), "Ends and saves the currently recording camera path");
            PlayRecord = Config.Bind(KeybindSectionName, "Play Path Recording", new KeyboardShortcut(KeyCode.Slash), "Plays the currently saved camera path recording");
            CameraFOV = Config.Bind(CameraSettingsSectionName, "Camera FOV", 75, new ConfigDescription("The FOV value of the camera while unsnaped", new AcceptableValueRange<int>(1, 200)));
            MovementSpeed = Config.Bind(CameraSettingsSectionName, "CameraMoveSpeed", 10f, new ConfigDescription("How fast you want the camera to move", new AcceptableValueRange<float>(0.01f, 100f)));
            CameraSensitivity = Config.Bind(CameraSettingsSectionName, "Camera Sensitivity", 10f, new ConfigDescription("How fast you want the camera viewport to move while slaved", new AcceptableValueRange<float>(0.0f, 100f)));
            CameraSmoothing = Config.Bind(CameraSettingsSectionName, "Mouse Smoothing", 10f, new ConfigDescription("The amount of smoothing you want applied to the mouse in camera mode. (Lower is smoother)", new AcceptableValueRange<float>(0.0001f, 1f)));
            Gamespeed = Config.Bind(CameraSettingsSectionName, "Set Gamespeed", 1f, new ConfigDescription("What gamespeed you want to set the gameworld to when pressing the Change Gamespeed bind !WARNING! Changing the gamespeed for too long can cause weird (but temporary) side effects", new AcceptableValueRange<float>(0f, 1f)));
            FastMoveMult = Config.Bind(CameraSettingsSectionName, "Set Fast Movement Multiplier", 2f, new ConfigDescription("The value that the camera movement speed is multiplied by while the move fast key is held", new AcceptableValueRange<float>(0f, 100f)));
            RotateUsesSens = Config.Bind(CameraSettingsSectionName, "Rotation speed inherits Camera Sensitivity", false, "If true, the camera rotation speed is multiplied by the Camera Sensitivity value, otherwise, the camera is rotated by only 1 degree per frame");
            ImmuneInCamera = Config.Bind(CameraSettingsSectionName, "Immune In Camera Mode", true, "Makes the player unkillable while camera mode is active");
            OverrideGameRestriction = Config.Bind("Unsafe Options", "Override Session Restriction", false, "When enabled, the requirement for the player to be in a session is overridden, allowing access to the main camera whenever it's in use. However, options regarding player values and the such (immune in camera, move player, memory pos etc) are ignored until this option is disabled.\nThis option can result in bugs and artifacts, and while exceptions thrown by the CUS script are automatically handled, EFT is not so well programmed, and may react unpredictably.");

            Hook = new GameObject("CUS");
            Hook.AddComponent<CUSController>();
            DontDestroyOnLoad(Hook);
            Logger.LogInfo($"Camera Unsnap Loaded");
        }
    }
}
