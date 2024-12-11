using UnityEngine;
using Unity.Netcode;

public class Ball : NetworkBehaviour
{
    public NetworkVariable<bool> hasHitPlayer = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
