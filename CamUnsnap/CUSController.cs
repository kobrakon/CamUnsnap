using EFT;
using System;
using UnityEngine;
using System.Linq;
using Comfort.Common;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CamUnsnap
{
    public class CUSController : MonoBehaviour
    {
        bool CamUnsnapped { get; set; } = false;
        bool CamViewInControl { get; set; } = false;
        bool GamespeedChanged { get; set; } = false;
        GameObject gameCamera;
        Vector3 MemoryPos;
        List<Detour> Detours = new List<Detour>();

        void Update()
        {
            if (Plugin.ToggleCameraSnap.Value.IsDown())
                SnapCam();

            if (Plugin.CameraMouse.Value.IsDown())
            {
                switch (CamViewInControl)
                {
                    case true:
                        CamViewInControl = false;
                    break;
                    case false:
                        CamViewInControl = true;
                    break;
                }
            }

            if (Plugin.ChangeGamespeed.Value.IsDown() && CamUnsnapped)
            {
                switch (GamespeedChanged)
                {
                    case false:
                        Time.timeScale = Plugin.Gamespeed.Value;
                        GamespeedChanged = true;
                    break;
                    case true:
                        Time.timeScale = 1f;
                        GamespeedChanged = false;
                    break;
                }
            }
            else if (!CamUnsnapped) Time.timeScale = 1f;

            if (CamUnsnapped && Ready())
            {
                gameCamera = GameObject.Find("FPS Camera");

                if (Input.GetKey(Plugin.GoToPos.Value.MainKey))
                {
                    if (MemoryPos == null)
                    {
                        Plugin.logger.LogError("No memory pos to move camera to.");
                        return;
                    }
                    gameCamera.transform.position = MemoryPos;
                }

                if (Plugin.ImmuneInCamera.Value) typeof(GClass1905).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 0f);
                else if (player.ActiveHealthController.DamageCoeff != 1f)
                    typeof(GClass1905).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 1f);

                if (Input.GetKey(Plugin.RememberPos.Value.MainKey)) MemoryPos = gameCamera.transform.position;
                if (Input.GetKey(Plugin.MovePlayerToCam.Value.MainKey)) MovePlayer();
                if (Input.GetKey(Plugin.LockPlayerMovement.Value.MainKey))
                {
                    if (!Detours.Any()) Detours = new List<Detour>() { new Detour(typeof(Player).GetMethod("Move"), typeof(CUSController).GetMethod(nameof(BlankOverride))), new Detour(typeof(Player).GetMethod("Rotate"), typeof(CUSController).GetMethod(nameof(BlankOverride))) };
                    else { Detours.ForEach((Detour det) => det.Dispose()); Detours.Clear(); };
                }

                float delta = !GamespeedChanged ? Time.deltaTime : Time.fixedDeltaTime;

                if (Input.GetKey(Plugin.CamLeft.Value.MainKey))
                    gameCamera.transform.position += (-gameCamera.transform.right * MovementSpeed * delta);

                if (Input.GetKey(Plugin.CamRight.Value.MainKey))
                    gameCamera.transform.position += (gameCamera.transform.right * MovementSpeed * delta);

                if (Input.GetKey(Plugin.CamForward.Value.MainKey))
                    gameCamera.transform.position += (gameCamera.transform.forward * MovementSpeed * delta);

                if (Input.GetKey(Plugin.CamBack.Value.MainKey))
                    gameCamera.transform.position += (-gameCamera.transform.forward * MovementSpeed * delta);

                if (Input.GetKey(Plugin.CamUp.Value.MainKey))
                    gameCamera.transform.position += (gameCamera.transform.up * MovementSpeed * delta);

                if (Input.GetKey(Plugin.CamDown.Value.MainKey))
                    gameCamera.transform.position += (-gameCamera.transform.up * MovementSpeed * delta);

                if (CamViewInControl)
                {
                    float newRotationX = gameCamera.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * CameraSensitivity;
                    float newRotationY = gameCamera.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * CameraSensitivity;
                    gameCamera.transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
                }
            } else if (CamUnsnapped && !Ready() && Detours.Any()) { Detours.ForEach((Detour det) => det.Dispose()); Detours.Clear(); }
        }

        async void MovePlayer()
        {
            player.Transform.position = gameCamera.transform.position;
            player.ActiveHealthController.FallSafeHeight = 99999f;
            while (player.CharacterControllerCommon.isGrounded == false)
            {
                await Task.Yield();
            }
            player.ActiveHealthController.FallSafeHeight = Singleton<GClass1168>.Instance.Health.Falling.SafeHeight;
        }

        void SnapCam()
        {
            if (!Ready()) return;

            if (!CamUnsnapped)
            {
                player.PointOfView = EPointOfView.FreeCamera;
                player.PointOfView = EPointOfView.ThirdPerson;
            }
            else
                player.PointOfView = EPointOfView.FirstPerson;

            CamUnsnapped = !CamUnsnapped;
            return;
        }

        public static void BlankOverride() {} // override so player doesn't move

        Player player
        { get { return gameWorld.AllPlayers[0]; } }

        GameWorld gameWorld
        { get { return Singleton<GameWorld>.Instance; } }

        float MovementSpeed
        { get { return Plugin.MovementSpeed.Value; } }

        float CameraSensitivity
        { get { return Plugin.CameraSensitivity.Value; } }

        bool Ready() => gameWorld is null || gameWorld.AllPlayers is null || gameWorld.AllPlayers.Count == 0 ? false : true;
    }
}
