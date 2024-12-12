using UnityEngine;
using Unity.Netcode;

public class Ball : NetworkBehaviour
{
    /*
        Attached to the ball prefab, this is a simple script that checks with a boolean if the ball has hit a player.
    */
    //network variable is needed to save the boolean of the ball for all clients, with starting value false
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
