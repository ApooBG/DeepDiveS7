using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : NetworkBehaviour
{
    /*
        Attached to the UIComponent of the player.
        This script stores the username that the player has chosen and updates the textbox across all clients.
    */
    [SerializeField]
    public NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>(); //the stored username string using NetworkString

    [SerializeField]
    private TextMeshProUGUI usernameTextBox; //the textbox component

    private bool overlaySet = false;

    public override void OnNetworkSpawn() //once the player with this object spawns
    {
        if (IsOwner && IsClient) //if this is the client and the player that owns this object
        {
            // Send username to server once ready
            SubmitUsernameToServerRpc(GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>().username.text); //set the username to the server
        }
    }

    //update the server username for all clients
    [ServerRpc]
    private void SubmitUsernameToServerRpc(string chosenName, ServerRpcParams serverRpcParams = default) 
    {
        playerNetworkName.Value = chosenName;
    }

    public void SetOverlay()
    {
        //var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        usernameTextBox.text = $"{playerNetworkName.Value}";
    }

    public void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }
}