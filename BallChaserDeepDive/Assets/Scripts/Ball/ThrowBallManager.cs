using System;
using Unity.Netcode;
using UnityEngine;

public class ThrowBallManager : NetworkBehaviour
{
    private static ThrowBallManager _instance;
    public GameObject currentChaser;
    public NetworkVariable<ulong> currentChaserulong;

    public static ThrowBallManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<ThrowBallManager>();
            }
            return _instance;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (IsServer && currentChaser == null && ThrowBallManager.Instance != null)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            SetChaser(clientId);
        }

        if (IsClient)
        {
            CheckIfChaserIsChanged();
        }
    }

    public void SetChaser(ulong clientId)
    {
        if (currentChaser != null)
        {
            UpdateToRunnerServerRpc();
        }

        UpdateVariablesServerRpc(clientId);
        GameObject player = PlayersManager.Instance.GetPlayerPrefab(clientId);
    }


    private void CheckIfChaserIsChanged()
    {
        GameObject player = PlayersManager.Instance.GetPlayerPrefab(currentChaserulong.Value);

        if (player != currentChaser)
            currentChaser = player;

        else if (player.GetComponent<PlayerControl>().playerRole == PlayerControl.PlayerRole.Runner)
        {
            player.GetComponent<PlayerControl>().playerRole = PlayerControl.PlayerRole.Chaser;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateVariablesServerRpc(ulong clientId)
    {
        currentChaserulong.Value = clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateToRunnerServerRpc()
    {
        currentChaser.GetComponent<PlayerControl>().playerRole = PlayerControl.PlayerRole.Runner;
        ProjectileThrow pt = currentChaser.GetComponentInChildren<ProjectileThrow>();
        pt.enabled = false;
    }
}