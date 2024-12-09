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
}
