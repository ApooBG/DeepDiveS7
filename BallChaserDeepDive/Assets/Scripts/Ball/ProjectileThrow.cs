using System.Collections;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ProjectileThrow : NetworkBehaviour
{
    /*
        This script is attached to the player's child object which position is used to spawn balls on throw. 
        The script is used to call the ball prediction and also to change the throw's force. It also handles spawning new balls.
    */
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

    //if the player is the one that owns this object and is a client, then check if their role is chaser and if they are aiming, and show the prediction, also enable the force change
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

    //predicting the ball's movement based on the data
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
        if (IsClient) //if its a client and this method was called, then request the server to spawn a ball on this gameObject's position
        {
            RequestBallSpawnServerRpc(gameObject.transform.position, force);
        }
    }

    //request the server to spawn a ball, adding it force and changing the ownership of the ball's network object just to make sure that it works as intended
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

    //notifying the clients that the ball is ready for launch
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

    //make the ball visible through the server
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

    //make the ball visible through the client
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