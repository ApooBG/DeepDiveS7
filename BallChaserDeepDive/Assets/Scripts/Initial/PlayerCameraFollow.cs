using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraFollow : MonoBehaviour
{
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

    public void FollowPlayer(Transform transform)
    {
        cinemachineVirtualCamera.Follow = transform;
    }

    public Transform GetCameraTransform()
    {
        return cinemachineVirtualCamera.gameObject.transform;
    }
}
