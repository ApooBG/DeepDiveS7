using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    private static PlayersManager _instance;
    NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    public static PlayersManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayersManager>();
            }
            return _instance;
        }
    }

    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (IsServer)
                playersInGame.Value++;
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (IsServer)
                playersInGame.Value--;
        };
    }

    /// <summary>
    /// Gets the playerPrefab (PlayerObject) for the specified client ID.
    /// </summary>
    /// <param name="clientId">The client ID of the owner.</param>
    /// <returns>The playerPrefab's GameObject, or null if not found.</returns>
    public GameObject GetPlayerPrefab(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            // Return the PlayerObject associated with the client
            return networkClient.PlayerObject?.gameObject;
        }

        Debug.LogWarning($"PlayerPrefab not found for client ID: {clientId}");
        return null;
    }
}
