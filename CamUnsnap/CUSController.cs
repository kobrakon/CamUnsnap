using EFT;
using System;
using System.Linq;
using UnityEngine;
using Comfort.Common;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            if (Plugin.CameraMouse.Value.IsDown()) CamViewInControl = !CamViewInControl;

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

                if (Plugin.ImmuneInCamera.Value) typeof(GClass1911).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 0f);
                else if (player.ActiveHealthController.DamageCoeff != 1f)
                    typeof(GClass1911).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 1f);

                if (Input.GetKey(Plugin.RememberPos.Value.MainKey)) MemoryPos = gameCamera.transform.position;
                if (Input.GetKey(Plugin.MovePlayerToCam.Value.MainKey)) MovePlayer();
                if (Input.GetKey(Plugin.LockPlayerMovement.Value.MainKey))
                {
                    if (!Detours.Any()) Detours = new List<Detour>() { new Detour((Action<Vector2>)player.Move, new Action(BlankOverride)), new Detour((Action<Vector2, bool>)player.Rotate, new Action(BlankOverride)) };
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
            } else if (CamUnsnapped) SnapCam();
        }

        async void MovePlayer()
        {
            player.Transform.position = gameCamera.transform.position;
            player.ActiveHealthController.FallSafeHeight = 99999f;
            while (!player.CharacterControllerCommon.isGrounded) await Task.Yield();
            player.ActiveHealthController.FallSafeHeight = Singleton<GClass1173>.Instance.Health.Falling.SafeHeight;
        }

        void SnapCam()
        {
            if (!CamUnsnapped && Ready())
            {
                player.PointOfView = EPointOfView.FreeCamera;
                player.PointOfView = EPointOfView.ThirdPerson;
            }
            else
            {
                player.PointOfView = EPointOfView.FirstPerson;
                if (Detours.Any()) Detours.ForEach((Detour det) => det.Dispose()); Detours.Clear();
                if (Plugin.ImmuneInCamera.Value) typeof(GClass1911).GetProperty("DamageCoeff", BindingFlags.Instance | BindingFlags.Public).SetValue(player.ActiveHealthController, 1f);
            }
            CamUnsnapped = !CamUnsnapped;
        }

        public static void BlankOverride() {} // override so player doesn't move

        Player player
        { get => gameWorld.AllPlayers[0]; }

        GameWorld gameWorld
        { get => Singleton<GameWorld>.Instance; }

        float MovementSpeed
        { get => Plugin.MovementSpeed.Value; }

        float CameraSensitivity
        { get => Plugin.CameraSensitivity.Value; }

        bool Ready() => gameWorld != null || gameWorld.AllPlayers != null || gameWorld.AllPlayers.Count > 0;
    }
}