using System.Collections;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ProjectileThrow : NetworkBehaviour
{
    TrajectoryPredictor trajectoryPredictor;
    LineRenderer lineRenderer;

    [SerializeField]
    GameObject objectToThrow;

    [SerializeField, Range(0.0f, 100.0f)]
    public float force = 10f;

    [SerializeField, Range(0.0f, 100.0f)]
    float forceIncrementSpeed = 70f; // Speed at which force increases

    public Rigidbody nextBall;

    private void OnEnable()
    {
        trajectoryPredictor = GetComponent<TrajectoryPredictor>();
        lineRenderer = GetComponent<LineRenderer>();
        trajectoryPredictor.enabled = true;
        lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        trajectoryPredictor.enabled = false;
        lineRenderer.enabled = false;
    }

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            //
        };
    }

    void Update()
    {
        if (IsClient && IsOwner)
        {
            lineRenderer.enabled = false;
            if (gameObject.transform.parent.GetComponent <PlayerControl>().playerRole == PlayerControl.PlayerRole.Chaser)
            {
                if (gameObject.transform.parent.GetComponent<PlayerControl>().isAiming)
                {
                    lineRenderer.enabled = true;
                    HandleForceChange();
                    Predict();
                }
            }
        }
    }


    public void Predict()
    {
        trajectoryPredictor.PredictTrajectory(ProjectileData());
    }

    ProjectileProperties ProjectileData()
    {
        ProjectileProperties properties = new ProjectileProperties();
        Rigidbody currentBall;
        if (nextBall == null)
            currentBall = objectToThrow.GetComponent<Rigidbody>();
        else
            currentBall = nextBall;

        properties.direction = gameObject.transform.forward;
        properties.initialPosition = gameObject.transform.position;

        // Adjust initial speed to account for mass, mirroring Unity's ForceMode.Impulse
        properties.initialSpeed = force / currentBall.mass;

        properties.mass = currentBall.mass;
        properties.drag = currentBall.linearDamping;

        return properties;
    }

    void HandleForceChange()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // Get the scroll wheel input

        if (scrollInput != 0) // Check if there is any scroll input
        {
            // Adjust the force value based on scroll direction
            force += scrollInput * forceIncrementSpeed;

            // Clamp the force value between 0 and 100
            force = Mathf.Clamp(force, 0.0f, 100.0f);

            Debug.Log($"Scroll Input Detected. New Force: {force}");
        }
        else
        {
            Debug.Log("No Scroll Input Detected");
        }
    }
    public void ThrowObject()
    {
        if (IsClient)
        {
            RequestBallSpawnServerRpc(gameObject.transform.position, force);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBallSpawnServerRpc(Vector3 spawnPosition, float clientForce, ServerRpcParams rpcParams = default)
    {
        GameObject nextBallObject = SpawnerControl.Instance.SpawnObject(spawnPosition);
        Rigidbody nextBallRigidbody = nextBallObject.GetComponent<Rigidbody>();

        nextBallRigidbody.GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        nextBallRigidbody.transform.position = spawnPosition;
        nextBallRigidbody.transform.rotation = Quaternion.identity;

        // Use the client's passed-in force value
        nextBallRigidbody.AddForce(gameObject.transform.forward * clientForce, ForceMode.Impulse);

        NotifyClientBallReadyClientRpc(nextBallObject.GetComponent<NetworkObject>().NetworkObjectId, rpcParams.Receive.SenderClientId);
    }

    // Method called once the ball is ready
    private void OnBallReady(GameObject spawnedBall)
    {
        // Activate and configure the ball
        nextBall = spawnedBall.GetComponent<Rigidbody>();
        nextBall.gameObject.SetActive(true);
        nextBall.isKinematic = false;

        // Sync activation with all clients
        SetActiveOnServerRpc(nextBall.GetComponent<NetworkObject>().NetworkObjectId, true);
    }

    [ClientRpc]
    private void NotifyClientBallReadyClientRpc(ulong ballNetworkId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballNetworkId, out var ballNetworkObject))
        {
            OnBallReady(ballNetworkObject.gameObject);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void SetActiveOnServerRpc(ulong networkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            networkObject.gameObject.SetActive(isActive);

            // Broadcast to all clients
            SetActiveOnClientRpc(networkObject.NetworkObjectId, isActive);
        }
    }

    [ClientRpc]
    void SetActiveOnClientRpc(ulong networkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            // Set the active state
            networkObject.gameObject.SetActive(isActive);
        }
    }
}