using System.Collections;
using System.Globalization;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]

public class PlayerControl : NetworkBehaviour
{
    /* 
        Attached to the player prefab gameObject.
        This script is used to catch the player's information and sync it with the server. That includes position, rotation, animation
        and anything else that you might need in the process of creating your game.
    */

    //To check which animation has to be played
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

    //What's the role of the player
    public enum PlayerRole
    {
        Runner,
        Chaser
    }


    [SerializeField]
    private float walkSpeed = 3.5f;

    [SerializeField]
    private GameObject fakeBall; //a ball gameObject that's attached to the player's hand and does nothing.

    [SerializeField]
    private float runSpeedOffset = 2.0f; //additional speed when running

    [SerializeField]
    private float rotationSpeed = 3.5f; //rotation speed

    [SerializeField]
    public bool isAiming = false; //a player variable to check if the player is aiming

    [SerializeField]
    private Vector2 defaultInitialPlanePosition = new Vector2(-4, 4); //player spawn position

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>(); //player state that will be given across the server

    private CharacterController characterController; //character controller component attached to this gameObject

    public Animator animator; //animator component attached to this gameObject

    private PlayerState lastPlayerState; //last player state

    public PlayerRole playerRole; //player role

    public NetworkVariable<int> points = new NetworkVariable<int>(60, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); //points which will be synced across the server

    public NetworkVariable<Color> imageColor = new NetworkVariable<Color>(Color.white); //nametag image color which will be synced across the server

    public Image image; //actual object of the nametag image

    private bool isThrowing = false; //a check to know when the player can throw the ball again

    private ProjectileThrow projectileThrow; //a script that is attached to the child of this gameObject

    private void Awake()
    {
        Cursor.visible = true;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (IsClient && IsOwner) //checks if the executor is the client and if he owns this object, so no one else can execute this besides the owner of this object
        {
            //setting the default role to runner, the default spawning position randomized, camera to follow this gameObject and assigning projectileThrow
            playerRole = PlayerRole.Runner;
            transform.position = new Vector3(Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y), 0, Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y));
            PlayerCameraFollow.Instance.FollowPlayer(transform);

            projectileThrow = gameObject.GetComponentInChildren<ProjectileThrow>();
        }
    }

    private void Update()
    {
        if (IsClient && IsOwner)//checks if the executor is the client and if he owns this object, so no one else can execute this besides the owner of this object
        {
            ClientInput(); //if owner then they can listen for inputs
            CheckRole(); //if owner then they can check if the given role is correct one
        }

        ClientVisuals(); //without IsClient && IsOwner, this will get triggered by other players as well, so they can sync that player in the scene
    }

    public void RestartGame() //when restarting the game, reset the points
    {
        points.Value = 60;
    }

    void CheckRole()
    {
        if (ThrowBallManager.Instance == null) //if the game manager isn't created yet, return
            return;

        if (ThrowBallManager.Instance.currentChaser != this.gameObject) //checks if this gameObject is the current chaser
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

        transform.Rotate(inputRotation * rotationSpeed, Space.World); //rotate the player based on input

        //checks if game has started and if this player is the chaser
        if (playerRole == PlayerRole.Chaser && ThrowBallManager.Instance.gameStarted.Value) 
        {
            if (ActiveAimingActionkey() && !isAiming) //when aiming
            {
                UpdatePlayerStateServerRpc(PlayerState.Aim); //update the player's animation to aiming
                isAiming = true;
            }

            if (ActiveThrowingActionkey() && isAiming && !isThrowing) //when throwing the ball
            {
                isThrowing = true;
                UpdatePlayerStateServerRpc(PlayerState.Throw); //update the player's animation to throwing
                StartCoroutine(HandleThrow()); //throw the object
            }

            if (isThrowing) //if the player is throwing, set that fake ball to not be seen
                fakeBall.SetActive(false);

            if (StopAimingActionKey() && isAiming) //if aiming and the right button of the mouse is clicked, stop aiming
            {
                UpdatePlayerStateServerRpc(PlayerState.ReverseAim); //update player's animation to reverse aim
                StartCoroutine(HandleThrowCancelAnimation()); //cancel throwing
            }

            //don't allow movement while aiming
            if (isAiming)
                return;
        }

        //two additional checks for the animation just to make sure that they don't get stuck into a throwing animation
        if (lastPlayerState == PlayerState.Aim && playerRole == PlayerRole.Runner)
            UpdatePlayerStateServerRpc(PlayerState.Throw);

        if (lastPlayerState == PlayerState.Idle && playerRole == PlayerRole.Runner)
            UpdatePlayerStateServerRpc(PlayerState.Idle);

        // change animation states depending on the forward input
        if (forwardInput == 0)
            UpdatePlayerStateServerRpc(PlayerState.Idle);

        //if the player is a chaser even when holding shift he will walk
        else if (!ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1 || ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1 && playerRole == PlayerRole.Chaser)
            UpdatePlayerStateServerRpc(PlayerState.Walk);
        else if (ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1 && playerRole == PlayerRole.Runner)
        {
            inputPosition = direction * runSpeedOffset;
            UpdatePlayerStateServerRpc(PlayerState.Run);
        }
        else if (forwardInput < 0)
            UpdatePlayerStateServerRpc(PlayerState.ReverseWalk);

        //update movement
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

    private IEnumerator HandleThrow()
    {
        yield return new WaitForSeconds(0.25f); //waits 0.25 seconds before throwing a ball
        projectileThrow.ThrowObject(); //throws the ball
        isAiming = false;
        isThrowing = false;
        UpdatePlayerStateServerRpc(PlayerState.Idle);
    }

    private IEnumerator HandleThrowCancelAnimation()
    {
        yield return null;

        isAiming = false;
        UpdatePlayerStateServerRpc(PlayerState.Idle);
    }

    //in order to get the playerstate for this player across all clients and sync it in, we need to update the networkPlayerState as a server
    //these kind of methods need ServerRpc affix
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
         networkPlayerState.Value = state;
         animator.speed = 1;
    }

    //updates the animation of the player
    private void ClientVisuals()
    {
        //makes the image playtag color to red if chaser and black if runner
        image.color = imageColor.Value;

        //if the last playerstate equals to the current one, then don't do anything
        if (networkPlayerState.Value == lastPlayerState)
            return; // Skip if the state hasn't changed.

        //reset all animation triggers
        ResetAllTriggers();

        //find the correct trigger
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

    //this method is used to check when the player is hit by a ball
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Ball" && playerRole != PlayerRole.Chaser) //if the player role is not chaser and is hit from a ball
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();

            // If the ball has already hit a player, stop here
            if (ball.hasHitPlayer.Value == true)
            {
                return;
            }

            // Process the hit since hasHitPlayer was false
            ulong playerulong = gameObject.GetComponent<NetworkObject>().OwnerClientId; //get the hit player just in case
            playerRole = PlayerRole.Chaser;
            NotifyHitOnServerRpc(playerulong); // Call the ServerRpc
            ThrowBallManager.Instance.SetChaser(playerulong); //set that player to be the chaser

            // Set hasHitPlayer to true so the ball can�t trigger another hit
            ball.hasHitPlayer.Value = true;
        }
    }

    //this method is used to set the player to a chaser
    void SetToChaser()
    {
        this.projectileThrow.enabled = true; //enable the projectileThrow
        this.playerRole = PlayerRole.Chaser; //change the role of the player
        fakeBall.SetActive(true); //make the fake ball active
        ChangeImageColorServerRpc(new Color(255f, 0f, 0f, 155f / 255f)); //change the color of the nameTag
    }

    //this method is used to set the player to a runner
    void SetToRunner()
    {
        this.projectileThrow.enabled = false; //disable projectileThrow
        this.playerRole = PlayerRole.Runner; //make the player to be a runner
        isAiming = false; //cant aim
        fakeBall.SetActive(false); //fake ball deactivate
        ChangeImageColorServerRpc(new Color(0f, 0f, 0f, 155f / 255f)); //change the color of the name tag image

        if (lastPlayerState == PlayerState.Aim) //in case that the player was still aiming when the chaser was updated, go to reverse aim
            UpdatePlayerStateServerRpc(PlayerState.ReverseAim);
    }

    //each client comes into this method and sets the image color for this player, that is why we disable the ownership
    [ServerRpc(RequireOwnership = false)]
    void ChangeImageColorServerRpc(Color color)
    {
        imageColor.Value = color;
    }

    //notifies clients who hit who, it is just for debugging purposes in this case
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
