using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostUIManager : NetworkBehaviour
{
    [SerializeField]
    private Button startGame;

    [SerializeField]
    private Button triggerConsole;

    [SerializeField]
    private GameObject logger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startGame?.onClick.AddListener(() =>
        {
            if (IsServer)
            {
                ThrowBallManager.Instance.SetChaser(ThrowBallManager.Instance.ChooseRandomPlayer());
                ThrowBallManager.Instance.SetGame();
                ThrowBallManager.Instance.StartGame();

                startGame.gameObject.SetActive(false);
            }
        });

        triggerConsole?.onClick.AddListener(() =>
        {
            if (IsServer)
            {
                float value = logger.GetComponent<CanvasGroup>().alpha;
                if (value == 1)
                    logger.GetComponent<CanvasGroup>().alpha = 0;
                else
                    logger.GetComponent<CanvasGroup>().alpha = 1;
            }
        });
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Send username to server once ready
            startGame.gameObject.SetActive(true);
            triggerConsole.gameObject.SetActive(true);
            logger.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if (ThrowBallManager.Instance.gameStarted.Value)
                startGame.gameObject.SetActive(false);
            else
                startGame.gameObject.SetActive(true);
        }
    }
}
