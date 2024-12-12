using UnityEngine;

public class CanvasFaceCamera : MonoBehaviour
{

    /*
        Attached to the player canvas, this script finds the PlayerCameraFollow script and makes this canvas to always look towards the camera. 
        No need to ask for any server updates as this is a feature that each player will have to implement for their own scene.
    */
    void Update()
    {
        Transform cameraTransform = PlayerCameraFollow.Instance.GetCameraTransform();
        if (cameraTransform != null)
        {
            // Rotate the canvas so it always faces the camera
            transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
                             cameraTransform.rotation * Vector3.up);
        }
    }
}
