using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : NetworkBehaviour
{
    [SerializeField]
    public NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();

    [SerializeField]
    private TextMeshProUGUI usernameTextBox;

    private bool overlaySet = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsClient)
        {
            // Send username to server once ready
            SubmitUsernameToServerRpc(GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>().username.text);
        }
    }

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