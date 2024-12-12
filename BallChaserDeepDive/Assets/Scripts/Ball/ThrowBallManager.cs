using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowBallManager : NetworkBehaviour
{
    /* 
        The script is attached to a gameObject called ThrowBallManager. This script handles the whole game logic of the ThrowBall game.
    */
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
            gameStarted.Value = false; //make sure that the gameStarted is false for every client at first
        }
    }

    void Update()
    {
        if (IsServer && currentChaser == null && ThrowBallManager.Instance != null) //if chaser is null, make the server to set one
        {
            SetChaser(ChooseRandomPlayer());
        }

        if (IsClient) //through the client check if the chaser is changed
        {
            CheckIfChaserIsChanged();
        }
        if (IsServer) //game logic that the server handles
            Game();
    }

    public void SetChaser(ulong clientId) //set new chaser
    {
        if (currentChaser != null) //if current chaser is not null, make him to be a runner
        {
            UpdateToRunnerServerRpc();
        }

        UpdateVariablesServerRpc(clientId); //make the passed client to be a chaser
        GameObject player = PlayersManager.Instance.GetPlayerPrefab(clientId); //if you need to do anything with that player here you can get the game object of the new chaser
    }

    public ulong ChooseRandomPlayer() //chooses a random player and makes them the chaser when the game starts
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

    public List<GameObject> GetAllPlayers() //gets a list of all players that are in the lobby
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


    private void CheckIfChaserIsChanged() //checks if the chaser is changed and sets the correct roles through the server
    {
        GameObject player = PlayersManager.Instance.GetPlayerPrefab(currentChaserulong.Value);

        if (player != currentChaser)
            currentChaser = player;

        else if (player.GetComponent<PlayerControl>().playerRole == PlayerControl.PlayerRole.Runner)
        {
            player.GetComponent<PlayerControl>().playerRole = PlayerControl.PlayerRole.Chaser;
        }
    }

    public void StartGame() //starts game when called
    {
        UpdateGameStartedServerRpc(true);
    }

    public void SetGame() //sets a new game when called
    {
        UpdateGameStartedServerRpc(true);
        timerRunning = false; 
        secondLeft.Value = 60;

        foreach (GameObject player in GetAllPlayers())
        {
            player.GetComponent<PlayerControl>().RestartGame(); //restart the game 
        }
    }

    void Game()
    {
        if (gameStarted.Value && !timerRunning) //if the game is running and the timer hasn't started
        {
            timerRunning = true; //make the timer to start
            StartCoroutine(TimerGoesDownLoop()); //actually start the timer
        }
    }

    private IEnumerator TimerGoesDownLoop() //for the chaser reduce 1 point and timer too
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

    public string GetLeaderboard() //get leaderboard string by ordering the players by points
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
    private void UpdateVariablesServerRpc(ulong clientId) //set a new chaser
    {
        currentChaserulong.Value = clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateToRunnerServerRpc() //set a new runner
    {
        currentChaser.GetComponent<PlayerControl>().playerRole = PlayerControl.PlayerRole.Runner;
        ProjectileThrow pt = currentChaser.GetComponentInChildren<ProjectileThrow>();
        PlayerHud playerHud = currentChaser.GetComponentInChildren<PlayerHud>();
        pt.enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGameStartedServerRpc(bool value) //update game started value 
    {
        gameStarted.Value = value;
        if (value)
            UpdateSecondsLeftValueServerRpc(60);

    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateSecondsLeftValueServerRpc(int seconds) //update the seconds left value
    {
        secondLeft.Value = seconds;
    }
}