using UnityEngine;
using Comfort.Common;
using EFT;
using EFT.UI;

namespace CamUnSnap
{
    public class CUSController : MonoBehaviour
    {
        private static bool isSnapped { get; set; } = false;

        public void Update()
        {
            if (Plugin.ToggleCameraSnap.Value.IsDown())
                SnapCam();

        }

        private static void SnapCam()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.AllPlayers == null)
            {
                if (isSnapped) isSnapped = !isSnapped;
                PreloaderUI.Instance.Console.AddLog("You must be in-raid before you can unsnap the camera.", "WARNING");

                return;
            }

            if (!isSnapped)
            {
                gameWorld.AllPlayers[0].PointOfView = EPointOfView.FreeCamera;
                gameWorld.AllPlayers[0].PointOfView = EPointOfView.ThirdPerson;
            }
            else
                gameWorld.AllPlayers[0].PointOfView = EPointOfView.FirstPerson;

            isSnapped = !isSnapped;

            return;
        }
    }
}
