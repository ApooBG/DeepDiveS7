using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
public class SpawnerControl : NetworkBehaviour
{
    /* 
        This script is attached to a scene gameObject called NetworkObjectPool.
        It makes it possible for the server to initialize pool of the objectPrefab and spawn it when needed.
    */
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
        //initialize the pool
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

    //only the server can spawn from the pool.
    public GameObject SpawnObject(Vector3 position)
    {
        if (!IsServer) return null;

        GameObject go = NetworkObjectPool.Singleton.GetNetworkObject(objectPrefab).gameObject;
        go.transform.position = position;
        go.transform.rotation = Quaternion.identity;
        go.GetComponent<NetworkObject>().Spawn(); //without this, the pool doesn't work
        return go;
    }
}