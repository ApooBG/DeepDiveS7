using UnityEngine;
using TMPro;
using Unity.Netcode;
using static System.Net.Mime.MediaTypeNames;


public class ClientUIManager : NetworkBehaviour
{
    /* 
        Attached to the clientUI this script is used to make references to the client UI components. 
    */
    [SerializeField]
    TextMeshProUGUI leaderboard;

    [SerializeField]
    TextMeshProUGUI secondsLeft;

    [SerializeField]
    GameObject image;

    public void Update()
    {
        if (ThrowBallManager.Instance.gameStarted.Value)
        {
            secondsLeft.gameObject.SetActive(true);
            leaderboard.gameObject.SetActive(true);
            image.SetActive(true);

            secondsLeft.text = ThrowBallManager.Instance.secondLeft.Value.ToString();
            leaderboard.text = "";
        }

        leaderboard.text = ThrowBallManager.Instance.GetLeaderboard();
    }
}
