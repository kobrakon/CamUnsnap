using EFT;
using EFT.UI;
using System;
using System.Linq;
using UnityEngine;
using Comfort.Common;
using System.Reflection;
using EFT.Communications;
using MonoMod.RuntimeDetour;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CamUnsnap
{
    public class CUSController : MonoBehaviour
    {
        bool CamViewInControl { get; set; } = false;
        bool mCamUnsnapped = false;
        GameObject gameCamera;
        Vector3 MemoryPos;
        List<Detour> Detours = new List<Detour>();
        List<Vector3> MemoryPosList = new List<Vector3>();
        int currentListIndex = 0;
        float cacheFOV = 0;

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
                        if (Input.GetKeyDown(Plugin.GoToPos.Value.MainKey))
                        {
                            if (MemoryPos == null)
                                SendWarn("No memory pos to move camera to.");
                            else
                                gameCamera.transform.position = MemoryPos;
                        }

                        if (Input.GetKeyDown(Plugin.HideUI.Value.MainKey))
                            UIEnabled = !UIEnabled;

                        if (Input.GetKeyDown(Plugin.MovePlayerToCam.Value.MainKey)) 
                            MovePlayer();

                        typeof(ActiveHealthControllerClass)
                            .GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public)
                            .SetValue(player.ActiveHealthController, Plugin.ImmuneInCamera.Value ? 0f : player.ActiveHealthController.DamageCoeff != 1f && !playerAirborne ? 1f : 0f);

                        if (Input.GetKeyDown(Plugin.RememberPos.Value.MainKey)) 
                            MemoryPos = gameCamera.transform.position;

                        if (Input.GetKeyDown(Plugin.LockPlayerMovement.Value.MainKey))
                        {
                            if (!Detours.Any()) 
                                Detours = new List<Detour>() 
                                { 
                                    new Detour((Action<Vector2>)player.Move, (Action)BlankOverride), 
                                    new Detour((Action<Vector2, bool>)player.Rotate, (Action)BlankOverride), 
                                    new Detour((Action<Vector2, bool>)player.Rotate, (Action)BlankOverride),
                                    new Detour((Action<float>)player.SlowLean, (Action)BlankOverride),
                                    new Detour((Action<float>)player.ChangePose, (Action)BlankOverride),
                                    new Detour((Action)player.Jump, (Action)BlankOverride),
                                    new Detour((Action)player.ToggleProne, (Action)BlankOverride)
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
                                SendWarn("No valid Vector3 in Memory Position List to move to.");
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
                        float newRotationX = gameCamera.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * CameraSensitivity;
                        float newRotationY = gameCamera.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * CameraSensitivity;

                        gameCamera.transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
                    }

                    if (Input.GetKey(Plugin.RotateLeft.Value.MainKey))
                        gameCamera.transform.localEulerAngles += new Vector3(0, 0, Plugin.RotateUsesSens.Value ? 1f * CameraSensitivity : 1f);

                    if (Input.GetKey(Plugin.RotateRight.Value.MainKey))
                        gameCamera.transform.localEulerAngles += new Vector3(0, 0, Plugin.RotateUsesSens.Value ? -1f * CameraSensitivity : -1f);

                } catch (Exception e)
                {
                    SendWarn($"Camera machine broke =>\n{e.Message}");
                    Plugin.logger.LogError(e);
                    CamUnsnapped = false;
                }
            }
        }

        async void MovePlayer()
        {
            player.Transform.position = gameCamera.transform.position;
            typeof(ActiveHealthControllerClass).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 0f);
            while (playerAirborne)
            {
                await Task.Yield();
            }
            typeof(ActiveHealthControllerClass).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 1f);
        }

        void SendWarn(string message) => NotificationManagerClass.DisplayMessageNotification(message, ENotificationDurationType.Long, ENotificationIconType.Alert);

        public static void BlankOverride() {} // override so player doesn't move

        Player player
        { get => gameWorld.AllPlayers[0]; }

        GameWorld gameWorld
        { get => Singleton<GameWorld>.Instance; }

        float MovementSpeed
        { get => Plugin.MovementSpeed.Value; }

        float CameraSensitivity
        { get => Plugin.CameraSensitivity.Value; }

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
                Time.timeScale = value ? 0f : 1f;
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
                        player.PointOfView = EPointOfView.FirstPerson;
                        if (Detours.Any()) Detours.ForEach((Detour det) => det.Dispose()); Detours.Clear();
                        if (Plugin.ImmuneInCamera.Value) typeof(ActiveHealthControllerClass).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 1f);
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
                    player.PointOfView = EPointOfView.FreeCamera;
                    player.PointOfView = EPointOfView.ThirdPerson;
                    cacheFOV = Camera.current.fieldOfView;
                    if (Plugin.OverrideGameRestriction.Value) SendWarn("Session Override is enabled, player and positioning options are ignored, and controlling the camera outside of a raid may cause issues.\nYou've been warned...");
                }
                mCamUnsnapped = value;
            }
        }

        bool Ready() => gameWorld != null && gameWorld.AllPlayers != null && gameWorld.AllPlayers.Count > 0;
    }
}
