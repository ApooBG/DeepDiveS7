using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraFollow : MonoBehaviour
{
    /*
        Attched to the camera component, this script makes use of a cinemachine and makes it possible for the cinemachine to follow the player. 
    */
    private static PlayerCameraFollow _instance;
    private CinemachineCamera cinemachineVirtualCamera;
    public static PlayerCameraFollow Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerCameraFollow>();
            }
            return _instance;
        }
    }
    void Start()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineCamera>();
    }

    // Call this function to make it possible for the camera to follow the player
    // No need for server update as each player will have its own scene with its own camera.
    public void FollowPlayer(Transform transform)
    {
        cinemachineVirtualCamera.Follow = transform;
    }

    public Transform GetCameraTransform()
    {
        return cinemachineVirtualCamera.gameObject.transform;
    }
}
