using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowBallManager : NetworkBehaviour
{
    private static ThrowBallManager _instance;
    public GameObject currentChaser;
    public NetworkVariable<ulong> currentChaserulong;
    public NetworkVariable<bool> gameStarted;
    public NetworkVariable<int> secondLeft;
    private bool timerRunning = false;

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
        if (IsClient)
        {
            gameStarted.Value = false;
        }
    }

    void Update()
    {
        if (IsServer && currentChaser == null && ThrowBallManager.Instance != null)
        {
            SetChaser(ChooseRandomPlayer());
        }

        if (IsClient)
        {
            CheckIfChaserIsChanged();
        }
        if (IsServer)
            Game();
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

    public ulong ChooseRandomPlayer()
    {
        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        if (connectedClients.Count == 0)
        {
            Debug.LogWarning("No players are currently connected to the server.");
            return 0; // or handle this scenario appropriately (e.g., return a special value or throw an exception).
        }

        int randomIndex = UnityEngine.Random.Range(0, connectedClients.Count);
        ulong chosenClientId = connectedClients[randomIndex].ClientId;
        return chosenClientId;
    }

    public List<GameObject> GetAllPlayers()
    {
        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        if (connectedClients.Count == 0)
        {
            Debug.LogWarning("No players are currently connected to the server.");
            return null; // or handle this scenario appropriately (e.g., return a special value or throw an exception).
        }

        List<GameObject> listOfPlayers = new List<GameObject>();
        foreach (var client in connectedClients)
        {
            listOfPlayers.Add(PlayersManager.Instance.GetPlayerPrefab(client.ClientId));
        }

        return listOfPlayers;
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

    public void StartGame()
    {
        UpdateGameStartedServerRpc(true);
    }

    public void SetGame()
    {
        UpdateGameStartedServerRpc(true);
        timerRunning = false;
        secondLeft.Value = 60;

        foreach (GameObject player in GetAllPlayers())
        {
            player.GetComponent<PlayerControl>().RestartGame();
        }
    }

    void Game()
    {
        if (gameStarted.Value && !timerRunning)
        {
            timerRunning = true;
            StartCoroutine(TimerGoesDownLoop());
        }
    }

    // A single coroutine that runs the entire countdown
    private IEnumerator TimerGoesDownLoop()
    {
        while (secondLeft.Value >= 0)
        {
            yield return new WaitForSeconds(1f);

            // Decrement the countdown timer
            int value = secondLeft.Value - 1;
            UpdateSecondsLeftValueServerRpc(value);

            // Decrement points if currentChaser is indeed a Chaser
            if (currentChaser != null)
            {
                PlayerControl chaserControl = currentChaser.GetComponent<PlayerControl>();
                if (chaserControl != null && chaserControl.playerRole == PlayerControl.PlayerRole.Chaser)
                {
                    chaserControl.points.Value -= 1;
                }
            }
        }

        // Once the timer hits zero, end the game
        UpdateGameStartedServerRpc(false);
        timerRunning = false;
    }

    public string GetLeaderboard()
    {
        string s = "";
        List<GameObject> playerList = GetAllPlayers();
        if (playerList == null || playerList.Count == 0)
            return "No players connected.";

        // Sort players by points in descending order using points.Value
        playerList.Sort((a, b) =>
        {
            int aPoints = a.GetComponent<PlayerControl>().points.Value;
            int bPoints = b.GetComponent<PlayerControl>().points.Value;
            return bPoints.CompareTo(aPoints); // descending order
        });

        // Build the leaderboard string
        foreach (var player in playerList)
        {
            PlayerControl pControl = player.GetComponent<PlayerControl>();
            PlayerHud pHud = player.GetComponentInChildren<PlayerHud>();

            string playerName = pHud != null ? pHud.playerNetworkName.Value : "Unknown";
            int playerPoints = pControl != null ? pControl.points.Value : 0;

            s += $"{playerName} - {playerPoints}\n";
        }

        return s;
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
        PlayerHud playerHud = currentChaser.GetComponentInChildren<PlayerHud>();
        pt.enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGameStartedServerRpc(bool value)
    {
        gameStarted.Value = value;
        if (value)
            UpdateSecondsLeftValueServerRpc(60);

    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateSecondsLeftValueServerRpc(int seconds)
    {
        secondLeft.Value = seconds;
    }
}