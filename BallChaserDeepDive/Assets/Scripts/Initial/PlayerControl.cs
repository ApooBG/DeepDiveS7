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
        Throw
    }


    [SerializeField]
    private float walkSpeed = 3.5f;

    [SerializeField]
    private float runSpeedOffset = 2.0f;


    [SerializeField]
    private float rotationSpeed = 3.5f;

    [SerializeField]
    private Vector2 defaultInitialPlanePosition = new Vector2(-4, 4);

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private CharacterController characterController;

    public Animator animator;

    private PlayerState lastPlayerState;
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
            transform.position = new Vector3(Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y), 0, Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y));
            PlayerCameraFollow.Instance.FollowPlayer(transform);
        }
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
        }

        ClientVisuals();
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
        transform.Rotate(inputRotation * rotationSpeed, Space.World);
    }



    private static bool ActiveRunningActionKey()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private static bool ActiveAimingActionkey()
    {
        return Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.RightShift);
    }

    private static bool ActiveThrowingActionkey()
    {
        return Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKey(KeyCode.RightShift);
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
    }
}
