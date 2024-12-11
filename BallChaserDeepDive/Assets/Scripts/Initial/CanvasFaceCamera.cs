using UnityEngine;

public class CanvasFaceCamera : MonoBehaviour
{
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
