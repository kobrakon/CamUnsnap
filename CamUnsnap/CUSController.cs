using UnityEngine;
using System.Collections;
using Comfort.Common;
using EFT;
using EFT.CameraControl;

namespace CamUnSnap 
{ 
    public class CUSController : MonoBehaviour
    {
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
            
            if (isSnapped)
            {
                GameObject.Find("PlayerSuperior(Clone)").GetComponent<EFT.CameraControl.PlayerCameraController>().enabled = false;

                gameWorld.AllPlayers[0].PointOfView = EPointOfView.ThirdPerson;

                isSnapped = false;
                return;
            }

            GameObject.Find("PlayerSuperior(Clone)").GetComponent<PlayerCameraController>().enabled = true;

            gameWorld.AllPlayers[0].PointOfView = EPointOfView.ThirdPerson;

            isSnapped = true;
            return;            
        }
    }
}