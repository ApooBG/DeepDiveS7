using System.Collections;
using System.Globalization;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]

public class PlayerControl : NetworkBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walk,
        ReverseWalk,
        Run,
        Throw,
        Aim,
        ReverseAim
    }

    public enum PlayerRole
    {
        Runner,
        Chaser
    }


    [SerializeField]
    private float walkSpeed = 3.5f;

    [SerializeField]
    private float runSpeedOffset = 2.0f;

    [SerializeField]
    private float rotationSpeed = 3.5f;

    [SerializeField]
    private bool isAiming = false;

    [SerializeField]
    private Vector2 defaultInitialPlanePosition = new Vector2(-4, 4);

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private CharacterController characterController;

    public Animator animator;

    private PlayerState lastPlayerState;

    public PlayerRole playerRole;

    private ProjectileThrow projectileThrow;
    private void Awake()
    {
        Cursor.visible = true;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (IsClient && IsOwner)
        {
            playerRole = PlayerRole.Runner;
            transform.position = new Vector3(Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y), 0, Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y));
            PlayerCameraFollow.Instance.FollowPlayer(transform);

            projectileThrow = gameObject.GetComponentInChildren<ProjectileThrow>();
            //projectileThrow.enabled = true;
        }
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
            CheckRole();
            Debug.Log(playerRole);
        }

        ClientVisuals();
    }

    void CheckRole()
    {
        if (ThrowBallManager.Instance.currentChaser != this.gameObject)
        {
            SetToRunner();
        }

        else if (ThrowBallManager.Instance.currentChaser == this.gameObject)
            SetToChaser();
    }

    private void FixedUpdate()
    {

    }

    private void ClientInput()
    {
        // y axis client rotation
        Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);

        // forward & backward direction
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        Vector3 inputPosition = direction * forwardInput;

        transform.Rotate(inputRotation * rotationSpeed, Space.World);

        if (playerRole == PlayerRole.Chaser)
        {
            if (ActiveAimingActionkey() && !isAiming)
            {
                UpdatePlayerStateServerRpc(PlayerState.Aim);
                isAiming = true;
            }

            if (ActiveThrowingActionkey() && isAiming)
            {
                projectileThrow.ThrowObject();
                UpdatePlayerStateServerRpc(PlayerState.Throw);
                StartCoroutine(HandleThrowAnimation());
            }

            if (StopAimingActionKey() && isAiming)
            {
                UpdatePlayerStateServerRpc(PlayerState.ReverseAim);
                StartCoroutine(HandleThrowCancelAnimation());
            }

            //don't allow movement while aiming
            if (isAiming)
                return;
        }

        if (lastPlayerState == PlayerState.Aim && playerRole == PlayerRole.Runner)
            UpdatePlayerStateServerRpc(PlayerState.Throw);

        if (lastPlayerState == PlayerState.Idle && playerRole == PlayerRole.Runner)
            UpdatePlayerStateServerRpc(PlayerState.Idle);

        // change animation states
        if (forwardInput == 0)
            UpdatePlayerStateServerRpc(PlayerState.Idle);
        else if (!ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1)
            UpdatePlayerStateServerRpc(PlayerState.Walk);
        else if (ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1)
        {
            inputPosition = direction * runSpeedOffset;
            UpdatePlayerStateServerRpc(PlayerState.Run);
        }
        else if (forwardInput < 0)
            UpdatePlayerStateServerRpc(PlayerState.ReverseWalk);

        characterController.SimpleMove(inputPosition * walkSpeed);
    }



    private static bool ActiveRunningActionKey()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    private static bool ActiveAimingActionkey()
    {
        return Input.GetKeyDown(KeyCode.Mouse0);
    }
    private static bool StopAimingActionKey()
    {
        return Input.GetKeyDown(KeyCode.Mouse1);
    }

    private static bool ActiveThrowingActionkey()
    {
        return Input.GetKeyUp(KeyCode.Mouse0);
    }

    private IEnumerator HandleThrowAnimation()
    {
        yield return null;

        isAiming = false;
        UpdatePlayerStateServerRpc(PlayerState.Idle);
    }

    private IEnumerator HandleThrowCancelAnimation()
    {
        yield return null;

        isAiming = false;
        UpdatePlayerStateServerRpc(PlayerState.Idle);
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
         networkPlayerState.Value = state;
         animator.speed = 1;
    }

    private void ClientVisuals()
    {
        if (networkPlayerState.Value == lastPlayerState)
            return; // Skip if the state hasn't changed.

        ResetAllTriggers();

        switch (networkPlayerState.Value)
        {
            case PlayerState.Walk:
                animator.SetTrigger("Walk");
                break;
            case PlayerState.ReverseWalk:
                animator.SetTrigger("ReverseWalk");
                break;
            case PlayerState.Run:
                animator.SetTrigger("Run");
                break;
            case PlayerState.Throw:
                animator.SetTrigger("Throw");
                break;
            case PlayerState.Aim:
                animator.SetTrigger("Aim");
                break;
            case PlayerState.ReverseAim:
                animator.SetTrigger("ReverseAim");
                break;
            default:
                animator.SetTrigger("Idle");
                break;
        }
        lastPlayerState = networkPlayerState.Value;
    }

    private void ResetAllTriggers()
    {
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("ReverseWalk");
        animator.ResetTrigger("Run");
        animator.ResetTrigger("Throw");
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Aim");
        animator.ResetTrigger("ReverseAim");
    }


    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "Ball" && playerRole != PlayerRole.Chaser)
        {
            ulong playerulong = gameObject.GetComponent<NetworkObject>().OwnerClientId;
            playerRole = PlayerRole.Chaser;
            NotifyHitOnServerRpc(playerulong); // Call the ServerRpc
            ThrowBallManager.Instance.SetChaser(playerulong);
        }
    }

    void SetToChaser()
    {
        this.projectileThrow.enabled = true;
        this.playerRole = PlayerRole.Chaser;
    }

    void SetToRunner()
    {
        this.projectileThrow.enabled = false;
        this.playerRole = PlayerRole.Runner;
        isAiming = false;

        if(lastPlayerState == PlayerState.Aim)
            UpdatePlayerStateServerRpc(PlayerState.ReverseAim);
    }

    [ServerRpc(RequireOwnership = false)]
    void NotifyHitOnServerRpc(ulong hitClientId)
    {
        // Log the hit information
        Debug.Log($"NotifyHitOnServerRpc called for Player: {hitClientId}");

        string playerName = $"Player{hitClientId}";

        // Notify all clients about the hit
        NotifyClientsOfHitClientRpc(hitClientId, playerName);
    }


    [ClientRpc]
    void NotifyClientsOfHitClientRpc(ulong hitClientId, string playerName)
    {
        if (NetworkManager.Singleton.LocalClientId == hitClientId)
        {
            // Log a message for the player who was hit
            Logger.Instance.LogInfo("You were hit");
        }
        else
        {
            // Log a message for everyone else
            Logger.Instance.LogInfo($"{playerName} has been hit");
        }
    }

}
