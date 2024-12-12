using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TextMeshProUGUI playersInGameText;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    [SerializeField]
    public TMP_InputField username;

    private bool hasServerStarted;

    private void Awake()
    {
        UnityEngine.Cursor.visible = true;
    }

    void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";

        if (PlayersManager.Instance.JoinCode.Value != "")
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame} \n {PlayersManager.Instance.JoinCode.Value}";
    }

    void Start()
    {
        // START HOST
        startHostButton?.onClick.AddListener(async () =>
        {
            // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
            // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
            // traffic through the relay, else it just uses a LAN type (UNET) communication.
            if (username.text.Length <= 0)
            {
                Logger.Instance.LogInfo("Enter your username first...");
                return;
            }
            if (RelayManager.Instance.IsRelayEnabled)
                await RelayManager.Instance.SetupRelay();

            if (NetworkManager.Singleton.StartHost())
            {
                HideUI();
                Logger.Instance.LogInfo("Host started...");
            }
            else
                Logger.Instance.LogInfo("Unable to start host...");
        });

        // START CLIENT
        startClientButton?.onClick.AddListener(async () =>
        {
            if (username.text.Length <= 0)
            {
                Logger.Instance.LogInfo("Enter your username first...");
                return;
            }
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);

            if (NetworkManager.Singleton.StartClient())
            {
                HideUI();
                Logger.Instance.LogInfo("Client started...");
            }
            else
                Logger.Instance.LogInfo("Unable to start client...");
        });

        // STATUS TYPE CALLBACKS
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            string username = PlayersManager.Instance.GetPlayerPrefab(id).GetComponentInChildren<PlayerHud>().playerNetworkName.Value;
            if (username.Length <= 0)
                Logger.Instance.LogInfo($"Someone just connected...");
            else
                Logger.Instance.LogInfo($"{username} just connected...");
        };

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };
    }

    void HideUI()
    {
        startHostButton.gameObject.SetActive(false);
        startClientButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
        username.gameObject.SetActive(false);
    }
}
