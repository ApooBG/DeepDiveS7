using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
public class SpawnerControl : NetworkBehaviour
{
    private static SpawnerControl _instance;

    [SerializeField]
    private GameObject objectPrefab;

    public static SpawnerControl Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SpawnerControl>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            NetworkObjectPool.Singleton.InitializePool();
        };
    }

    // Update is called once per frame
    void Update()
    {
        // Your update logic here
    }

    public GameObject SpawnObject(Vector3 position)
    {
        if (!IsServer) return null;

        GameObject go = NetworkObjectPool.Singleton.GetNetworkObject(objectPrefab).gameObject;
        go.transform.position = position;
        go.transform.rotation = Quaternion.identity;
        go.GetComponent<NetworkObject>().Spawn();
        return go;
    }
}