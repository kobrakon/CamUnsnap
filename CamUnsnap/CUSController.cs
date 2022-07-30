using UnityEngine;
using Comfort.Common;
using EFT;
using EFT.UI;

namespace CamUnSnap
{
    public class CUSController : MonoBehaviour
    {
        public static bool isSnapped { get; set; } = false;
        public static bool CamViewInControl { get; set; } = false;
        public static float MovementSpeed;
        public static GameObject gameCamera;
        public void Update()
        {
            MovementSpeed = Plugin.CameraMoveSpeed.Value;

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

            if (isSnapped && Ready())
            {
                gameCamera = GameObject.Find("FPS Camera");

                if (Input.GetKey(KeyCode.LeftArrow)) //basic movement controls, might rework to be Bep configurable
                {
                    gameCamera.transform.position += (-gameCamera.transform.right * MovementSpeed * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    gameCamera.transform.position += (gameCamera.transform.right * MovementSpeed * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    gameCamera.transform.position += (gameCamera.transform.forward * MovementSpeed * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    gameCamera.transform.position += (-gameCamera.transform.forward * MovementSpeed * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    gameCamera.transform.position += (gameCamera.transform.up * MovementSpeed * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    gameCamera.transform.position += (-gameCamera.transform.up * MovementSpeed * Time.deltaTime);
                }

                if (CamViewInControl) //logic for slaving the camera viewport to your mouse
                {
                    float newRotationX = gameCamera.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * Plugin.CameraSensitivity.Value;
                    float newRotationY = gameCamera.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * Plugin.CameraSensitivity.Value;
                    gameCamera.transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
                }
                
            }
        }

        private static void SnapCam()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.AllPlayers == null)
            {
                if (isSnapped) isSnapped = !isSnapped;
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

        private static bool Ready()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld.AllPlayers == null || gameWorld.AllPlayers[0] == null)
            {
                return false;
            }
            return true;
        }
    }
}
