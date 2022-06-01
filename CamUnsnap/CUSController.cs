using UnityEngine;
using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.CameraControl;

namespace CamUnSnap 
{ 
    public class CUSController : MonoBehaviour
    {
        private static ILogger logger = Debug.unityLogger;
        public static bool isSnapped = true;
        public void Update()
        {
            if (Plugin.ToggleCameraSnap.Value.IsDown())
            {
                SnapCam();
            }
        }

        private static void SnapCam()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            
            if (gameWorld == null || gameWorld.AllPlayers == null)
            {
                if (!isSnapped)
                {
                    isSnapped = true;

                    return;
                }
                return;
            }

            var player = GameObject.Find("PlayerSuperior(Clone)");
            
            if (player == null)
            {
                PreloaderUI.Instance.Console.AddLog("Couldn't get PlayerSuperior object", "DEBUG");
                return;
            }

            var cam = player.GetComponent<PlayerCameraController>();
            
            if (cam == null)
            {
                PreloaderUI.Instance.Console.AddLog("Couldn't get PlayerCameraController component", "DEBUG");
                return;
            }

            if (isSnapped)
            {
                cam.enabled = false;

                gameWorld.AllPlayers[0].PointOfView = EPointOfView.ThirdPerson;

                isSnapped = false;
                return;
            }

            cam.enabled = true;

            gameWorld.AllPlayers[0].PointOfView = EPointOfView.ThirdPerson;

            isSnapped = true;
            return;            
        }
    }
}
