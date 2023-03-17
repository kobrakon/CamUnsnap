using EFT;
using EFT.UI;
using System;
using System.Linq;
using UnityEngine;
using Comfort.Common;
using System.Numerics;
using System.Reflection;
using EFT.Communications;
using MonoMod.RuntimeDetour;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CamUnsnap
{
    // I love documentation
    /// <summary>
    /// Represents a stream of positions and angles captured from a parent GameObject
    /// </summary>
    public struct TransformRecording
    {
        /// <summary>
        /// Creates a new transform recording stream
        /// </summary>
        /// <param name="Target">The GameObject to capture transform data from</param>
        public TransformRecording(GameObject Target)
        {
            this.Target = Target;
            Positions = new List<Vector3>();
            Angles = new List<Vector3>();
        }

        /// <summary>
        /// Captures and saves the position and angles of the target GameObject on the current frame
        /// </summary>
        public void Capture()
        {
            Positions.Add(Target.transform.position);
            Angles.Add(Target.transform.localEulerAngles);
        }

        /// <summary>
        /// Clears all recorded position and angle streams
        /// </summary>
        public void Clear()
        {
            Positions = new List<Vector3>();
            Angles = new List<Vector3>();
        }

        /// <summary>
        /// Checks if any values are present in the current streams
        /// </summary>
        /// <returns>true if there are any non-null values, false otherwise</returns>
        public bool Any() => Positions.Any();

        public Vector3[] this[int index]
        {
            get => new Vector3[] { Positions[index], Angles[index] };
        }

        public int Length
        { get => Positions.Count - 1; }

        public List<Vector3> Positions
        { get; private set; }

        public List<Vector3> Angles
        { get; private set; }

        public readonly GameObject Target;
    }

    public class CUSController : MonoBehaviour
    {
        bool mCamUnsnapped = false;
        bool Recording = false;
        bool playingPath = false;
        int currentRecordingIndex = 0;
        GameObject gameCamera;
        Vector3 MemoryPos;
        List<Detour> Detours = new List<Detour>();
        List<Vector3> MemoryPosList = new List<Vector3>();
        TransformRecording PathRecording;
        int currentListIndex = 0;
        float cacheFOV = 0;

        bool CamViewInControl { get; set; } = false;

        Player player
        { get => gameWorld.AllPlayers[0]; }

        GameWorld gameWorld
        { get => Singleton<GameWorld>.Instance; }

        float MovementSpeed
        { get => Plugin.MovementSpeed.Value; }

        float CameraSensitivity
        { get => Plugin.CameraSensitivity.Value; }
        
        float CameraSmoothing
        { get => Plugin.CameraSmoothing.Value; }

        GameObject commonUI
        { get => MonoBehaviourSingleton<CommonUI>.Instance.gameObject; }

        GameObject preloaderUI
        { get => MonoBehaviourSingleton<PreloaderUI>.Instance.gameObject; }

        GameObject gameScene
        { get => MonoBehaviourSingleton<GameUI>.Instance.gameObject; }

        bool GamespeedChanged
        {
            get => Time.timeScale != 1f;
            set
            {
                Time.timeScale = value ? Plugin.Gamespeed.Value : 1f;
            }
        }

        bool UIEnabled
        {
            get => commonUI.activeSelf && preloaderUI.activeSelf && gameScene.activeSelf;
            set
            {
                commonUI.SetActive(value);
                preloaderUI.SetActive(value);
                gameScene.SetActive(value);
            }
        }

        bool playerAirborne
        {
            get => !player.CharacterControllerCommon.isGrounded;
        }

        bool CamUnsnapped
        {
            get => mCamUnsnapped;
            set
            {
                if (!value)
                {
                    if (!Plugin.OverrideGameRestriction.Value)
                    {
                        if (Ready())
                        {
                            player.PointOfView = EPointOfView.FirstPerson;
                            if (Plugin.ImmuneInCamera.Value) player.ActiveHealthController.SetDamageCoeff(1);
                        }
                        if (Detours.Any()) Detours.ForEach((Detour det) => det.Dispose()); Detours.Clear();
                        if (!UIEnabled)
                        {
                            try
                            {
                                commonUI.SetActive(true);
                                preloaderUI.SetActive(true);
                                gameScene.SetActive(true);
                            }
                            catch (Exception e) { Plugin.logger.LogError($"bruh\n{e}"); }
                            UIEnabled = true;
                        }
                        Camera.current.fieldOfView = cacheFOV;
                    }
                } else
                {
                    if (player != null)
                    {
                        player.PointOfView = EPointOfView.FreeCamera;
                        player.PointOfView = EPointOfView.ThirdPerson;
                    }
                    
                    cacheFOV = Camera.current.fieldOfView;
                    if (Plugin.OverrideGameRestriction.Value) SendNotificaiton("Session Override is enabled, player and positioning options are ignored, and controlling the camera outside of a raid may cause issues.\nYou've been warned...");
                }
                mCamUnsnapped = value;
            }
        }
        private Vector2 smoothedMouseDelta;
        private Vector2 currentMouseDelta;
        void Update()
        {
            if (Input.GetKeyDown(Plugin.ToggleCameraSnap.Value.MainKey)) 
                CamUnsnapped = !CamUnsnapped;

            if (Input.GetKeyDown(Plugin.CameraMouse.Value.MainKey)) 
                CamViewInControl = !CamViewInControl;

            if (Input.GetKeyDown(Plugin.ChangeGamespeed.Value.MainKey) && CamUnsnapped)
                GamespeedChanged = !GamespeedChanged;

            if (CamUnsnapped)
            {
                try
                {
                    gameCamera = Camera.current.gameObject;

                    if (!Plugin.OverrideGameRestriction.Value && Ready())
                    {
                        if (PathRecording.Target == null)
                        {
                            PathRecording = new TransformRecording(gameCamera);
                        }

                        if (Input.GetKeyDown(Plugin.GoToPos.Value.MainKey))
                        {
                            if (MemoryPos == null)
                                SendNotificaiton("No memory pos to move camera to.");
                            else
                                gameCamera.transform.position = MemoryPos;
                        }

                        if (Input.GetKeyDown(Plugin.HideUI.Value.MainKey))
                            UIEnabled = !UIEnabled;

                        if (Input.GetKeyDown(Plugin.PlayRecord.Value.MainKey))
                            playingPath = true;

                        if (Recording)
                            PathRecording.Capture();

                        if (playingPath)
                        {
                            Vector3[] transformFrame = PathRecording[currentRecordingIndex];
                            gameCamera.transform.position = transformFrame[0];
                            gameCamera.transform.localEulerAngles = transformFrame[1];

                            currentRecordingIndex++;

                            if (currentRecordingIndex > PathRecording.Length) // fuckers took my .Length cant have shit with VS
                            {
                                currentRecordingIndex = 0;
                                playingPath = false;
                                return;
                            }

                            return;
                        }

                        if (Input.GetKeyDown(Plugin.MovePlayerToCam.Value.MainKey))
                            MovePlayer();

                        if (Input.GetKeyDown(Plugin.BeginRecord.Value.MainKey))
                        {
                            Recording = true;
                            PathRecording.Clear();
                            SendNotificaiton("Recording Started", false);
                        }

                        if (Input.GetKeyDown(Plugin.ResumeRecord.Value.MainKey))
                        {
                            if (PathRecording.Any())
                            {
                                Recording = true;
                                SendNotificaiton("Recording Resumed", false);
                            } else
                            {
                                SendNotificaiton($"Cannot resume recording\nNo previous recording exists, press '{Plugin.BeginRecord.Value}' to start a new one");
                            }
                        }

                        if (Input.GetKeyDown(Plugin.StopRecord.Value.MainKey))
                        {
                            Recording = false;
                            SendNotificaiton("Recording Stopped", false);
                        }

                        player.ActiveHealthController.SetDamageCoeff(Plugin.ImmuneInCamera.Value ? 0f : player.ActiveHealthController.DamageCoeff != 1f && !playerAirborne ? 1f : 0f);

                        if (Input.GetKeyDown(Plugin.RememberPos.Value.MainKey)) 
                            MemoryPos = gameCamera.transform.position;

                        if (Input.GetKeyDown(Plugin.LockPlayerMovement.Value.MainKey))
                        {
                            if (!Detours.Any()) 
                                Detours = new List<Detour>() 
                                { 
                                    new Detour(typeof(Player).GetMethod(nameof(Player.Move)).CreateDelegate(player), (Action)BlankOverride), 
                                    new Detour(typeof(Player).GetMethod(nameof(Player.Rotate)).CreateDelegate(player), (Action)BlankOverride), 
                                    new Detour(typeof(Player).GetMethod(nameof(Player.SlowLean)).CreateDelegate(player), (Action)BlankOverride),
                                    new Detour(typeof(Player).GetMethod(nameof(Player.ChangePose)).CreateDelegate(player), (Action)BlankOverride),
                                    new Detour(typeof(Player).GetMethod(nameof(Player.Jump)).CreateDelegate(player), (Action)BlankOverride),
                                    new Detour(typeof(Player).GetMethod(nameof(Player.ToggleProne)).CreateDelegate(player), (Action)BlankOverride)
                                };
                            else 
                            { 
                                Detours.ForEach((Detour det) => det.Dispose()); 
                                Detours.Clear(); 
                            };
                        }

                        if (Input.GetKeyDown(Plugin.AddToMemPosList.Value.MainKey))
                            MemoryPosList.Add(gameCamera.transform.position);
                    
                        if (Input.GetKeyDown(Plugin.AdvanceList.Value.MainKey))
                        {
                            if (MemoryPosList[currentListIndex + 1] != null)
                            {
                                currentListIndex++;
                                gameCamera.transform.position = MemoryPosList[currentListIndex];
                            } else if (MemoryPosList.First() != null)
                            {
                                currentListIndex = 0;
                                gameCamera.transform.position = MemoryPosList.First();
                            } else
                            {
                                currentListIndex = 0;
                                SendNotificaiton("No valid Vector3 in Memory Position List to move to.");
                            }
                        }

                        if (Input.GetKeyDown(Plugin.ClearList.Value.MainKey))
                            MemoryPosList.Clear();

                    } else if (!Ready() && MemoryPosList.Any())
                    {
                        MemoryPosList.Clear();
                        CamUnsnapped = false;
                        return;
                    }

                    float delta = !GamespeedChanged ? Time.deltaTime : Time.fixedDeltaTime;
                    float fastMove = Input.GetKey(Plugin.FastMove.Value.MainKey) ? Plugin.FastMoveMult.Value : 1f;
                    Camera.current.fieldOfView = Plugin.CameraFOV.Value;

                    if (Input.GetKey(Plugin.CamLeft.Value.MainKey))
                        gameCamera.transform.position += (-gameCamera.transform.right * MovementSpeed * fastMove * delta);

                    if (Input.GetKey(Plugin.CamRight.Value.MainKey))
                        gameCamera.transform.position += (gameCamera.transform.right * MovementSpeed * fastMove * delta);

                    if (Input.GetKey(Plugin.CamForward.Value.MainKey))
                        gameCamera.transform.position += (gameCamera.transform.forward * MovementSpeed * fastMove * delta);

                    if (Input.GetKey(Plugin.CamBack.Value.MainKey))
                        gameCamera.transform.position += (-gameCamera.transform.forward * MovementSpeed * fastMove * delta);

                    if (Input.GetKey(Plugin.CamUp.Value.MainKey))
                        gameCamera.transform.position += (gameCamera.transform.up * MovementSpeed * fastMove * delta);

                    if (Input.GetKey(Plugin.CamDown.Value.MainKey))
                        gameCamera.transform.position += (-gameCamera.transform.up * MovementSpeed * fastMove * delta);

                    if (CamViewInControl)
                    {

                        currentMouseDelta.x = Input.GetAxis("Mouse X") * CameraSensitivity;
                        currentMouseDelta.y = Input.GetAxis("Mouse Y") * CameraSensitivity;

                        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, currentMouseDelta, CameraSmoothing);

                        float newRotationX = gameCamera.transform.localEulerAngles.y + smoothedMouseDelta.x;
                        float newRotationY = gameCamera.transform.localEulerAngles.x - smoothedMouseDelta.y;

                        gameCamera.transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);

                    }

                    if (Input.GetKey(Plugin.RotateLeft.Value.MainKey))
                        gameCamera.transform.localEulerAngles += new Vector3(0, 0, Plugin.RotateUsesSens.Value ? 1f * CameraSensitivity : 1f);

                    if (Input.GetKey(Plugin.RotateRight.Value.MainKey))
                        gameCamera.transform.localEulerAngles += new Vector3(0, 0, Plugin.RotateUsesSens.Value ? -1f * CameraSensitivity : -1f);

                } catch (Exception e)
                {
                    SendNotificaiton($"Camera machine broke =>\n{e.Message}");
                    Plugin.logger.LogError(e);
                    CamUnsnapped = false;
                }
            }
        }

        async void MovePlayer()
        {
            player.Transform.position = gameCamera.transform.position;
            player.ActiveHealthController.SetDamageCoeff(1);
            while (playerAirborne)
            {
                await Task.Yield();
            }
            player.ActiveHealthController.SetDamageCoeff(1);
        }

        void SendNotificaiton(string message, bool warn = true) => NotificationManagerClass.DisplayMessageNotification(message, ENotificationDurationType.Long, warn ? ENotificationIconType.Alert : ENotificationIconType.Default);

        public static void BlankOverride() {} // override so player doesn't move

        bool Ready() => gameWorld != null && gameWorld.AllPlayers != null && gameWorld.AllPlayers.Count > 0;
    }
}
