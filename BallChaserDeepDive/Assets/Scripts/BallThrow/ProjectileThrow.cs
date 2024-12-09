using System.Collections;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ProjectileThrow : NetworkBehaviour
{
    TrajectoryPredictor trajectoryPredictor;

    [SerializeField]
    GameObject objectToThrow;

    [SerializeField, Range(0.0f, 100.0f)]
    public float force = 10f;

    [SerializeField]
    Transform startPosition;

    public Rigidbody nextBall;
    void OnEnable()
    {
        trajectoryPredictor = GetComponent<TrajectoryPredictor>();

        if (startPosition == null)
            startPosition = transform;
    }

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (nextBall == null)
                CreateNextBall();
        };
    }

    void Update()
    {
        if (IsClient && IsOwner && nextBall != null)
        {
            Predict();
        }
    }


    public void Predict()
    {
        trajectoryPredictor.PredictTrajectory(ProjectileData());
    }

    ProjectileProperties ProjectileData()
    {
        ProjectileProperties properties = new ProjectileProperties();

        properties.direction = startPosition.forward;
        properties.initialPosition = startPosition.position;

        // Adjust initial speed to account for mass, mirroring Unity's ForceMode.Impulse
        properties.initialSpeed = force / nextBall.mass;

        properties.mass = nextBall.mass;
        properties.drag = nextBall.linearDamping;

        return properties;
    }

    public void ThrowObject()
    {
        nextBall.gameObject.transform.parent = null;
        nextBall.gameObject.SetActive(true);
        nextBall.isKinematic = false;
        nextBall.AddForce(startPosition.forward * force, ForceMode.Impulse);

        // Call a ClientRpc to sync the active state across all clients
        SetActiveOnServerRpc(nextBall.GetComponent<NetworkObject>().NetworkObjectId, 0, true);

        // Create the next ball
        CreateNextBall();
    }

    void CreateNextBall()
    {
        if (IsClient)
        {
            GameObject nextBallObject = SpawnerControl.Instance.SpawnObject(startPosition.position);

            // Assign the nextBall reference locally
            nextBall = nextBallObject.GetComponent<Rigidbody>();

            // Call a ServerRpc to sync the active state and parent across all clients
            ulong parentId = gameObject.transform.parent.GetComponent<NetworkObject>().NetworkObjectId;
            SetActiveOnServerRpc(nextBall.GetComponent<NetworkObject>().NetworkObjectId, parentId, false);
        }
    }

    [ServerRpc]
    void SetActiveOnServerRpc(ulong networkObjectId, ulong parentNetworkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            // Set the active state
            networkObject.gameObject.SetActive(isActive);

            // Assign the parent if the parent exists
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetworkObjectId, out var parentNetworkObject))
            {
                networkObject.transform.parent = parentNetworkObject.transform;
            }

            // Broadcast to all clients
            SetActiveOnClientRpc(networkObjectId, parentNetworkObjectId, isActive);
        }
    }

    [ClientRpc]
    void SetActiveOnClientRpc(ulong networkObjectId, ulong parentNetworkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            // Set the active state
            networkObject.gameObject.SetActive(isActive);

            // Assign the parent if the parent exists
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetworkObjectId, out var parentNetworkObject))
            {
                networkObject.transform.parent = parentNetworkObject.transform;
            }
        }
    }
}