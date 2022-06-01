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

            if (isSnapped)
            {
                gameWorld.AllPlayers[0].PointOfView = EPointOfView.FreeCamera;
                gameWorld.AllPlayers[0].PointOfView = EPointOfView.ThirdPerson;

                isSnapped = false;
                return;
            }

            gameWorld.AllPlayers[0].PointOfView = EPointOfView.FirstPerson;

            isSnapped = true;
            return;            
        }
    }
}
