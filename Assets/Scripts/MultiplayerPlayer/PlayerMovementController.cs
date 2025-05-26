using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player movement, jumping, and kicking in a networked environment.
/// </summary>
public class PlayerMovementController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float kickForce = 10f;
    [SerializeField] private float upwardForce = 0.9f;

    [Header("References")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private AudioClip kickSoundClip;

    private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    private Vector3 groundCheckSizeReduction = new Vector3(0.03f, 0.2f, 0);
    private Vector3 positiveAreaSettings = new Vector3(0.8f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.8f, -0.8f, 0);
    private Vector3 areaSettings;

    private bool jumpInputReceived;
    private bool kickInputReceived;
    private bool isGrounded;

    private PlayerInputHandler inputHandler; // Will now be a reference to *this player's* input handler

    private void Awake()
    {
        // Cache references if not set in inspector
        rb = rb == null ? GetComponent<Rigidbody2D>() : rb;
        boxCollider = boxCollider == null ? GetComponent<BoxCollider2D>() : boxCollider;

        // Log errors if critical references are missing
        if (playerModel == null) Debug.LogError("PlayerModel not assigned.");
        if (animator == null) Debug.LogError("Animator not assigned.");
        if (networkAnimator == null) Debug.LogError("NetworkAnimator not assigned.");
    }

    private void Start()
    {
        if (playerModel != null)
        {
            playerModel.SetActive(false); // Player model starts inactive
        }
    }

    public override void OnStartLocalPlayer()
    {
        // Get the PlayerInputHandler from *this* player's game object
        inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler == null)
        {
            Debug.LogError("PlayerMovementController: PlayerInputHandler component not found on this player's GameObject.", this);
            return;
        }

        // Only local player handles input
        inputHandler.OnJumpPressed += HandleJumpInput;
        inputHandler.OnKickPressed += HandleKickInput;

        // Immediately set area settings for the local player when they start
        SetAreaSettings();
    }

    public override void OnStopLocalPlayer()
    {
        // Unsubscribe to prevent memory leaks when player is destroyed
        if (inputHandler != null)
        {
            inputHandler.OnJumpPressed -= HandleJumpInput;
            inputHandler.OnKickPressed -= HandleKickInput;
        }
    }

    private void HandleJumpInput()
    {
        jumpInputReceived = true;
    }

    private void HandleKickInput()
    {
        kickInputReceived = true;
    }

    private void Update()
    {
        // Activate player model and set area settings when entering the game scene
        // Assuming "OnlineGameScene" is the scene where active gameplay occurs
        if (SceneManager.GetActiveScene().name == "OnlineGameScene" && playerModel != null && !playerModel.activeSelf)
        {
            playerModel.SetActive(true);
        }

        if (!isLocalPlayer) return;

        // Clamp player position if falling below threshold
        // This threshold might need to be adjusted based on your game's level design
        if (transform.position.y <= -3.69f)
        {
            transform.position = new Vector3(transform.position.x, -3.705019f, 0);
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return; // Only local player's FixedUpdate should process input and send commands

        if (boxCollider == null) return;

        PerformGroundCheck();

        // Ensure playerModel is active before handling movement (relevant if not activated in OnStartLocalPlayer)
        if (playerModel != null && playerModel.activeSelf)
        {
            HandleMovement(jumpInputReceived, kickInputReceived, isGrounded);
        }

        // Reset input flags after processing
        jumpInputReceived = false;
        kickInputReceived = false;
    }

    // Performs a ground check using an OverlapBoxAll to detect ground colliders.
    private void PerformGroundCheck()
    {
        isGrounded = false;
        // Adjust the ground check box based on the player's collider
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            boxCollider.bounds.center - groundCheckOffset,
            boxCollider.bounds.size - groundCheckSizeReduction,
            0f, whatIsGround);

        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                isGrounded = true;
                break;
            }
        }
    }

    /// <summary>
    /// Sets area settings and player orientation based on netId.
    /// Used for team assignment.
    /// </summary>
    private void SetAreaSettings()
    {
        // This logic will run on the server and then be reflected on clients.
        // For visual changes based on netId (like rotation), you might use a SyncVar
        // or ensure this is called on the client for the local player.
        // Since netId is assigned by Mirror, it's reliable.
        if (netId == 1) // Assuming netId 1 is the first player, and netId 2 is the second, etc.
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
            areaSettings = negativeAreaSettings;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Ensure default rotation for other players
            areaSettings = positiveAreaSettings;
        }
    }

    /// <summary>
    /// Handles local input and sends movement commands to the server.
    /// </summary>
    private void HandleMovement(bool jump, bool kick, bool isGrounded)
    {
        // No need to check playerModel.activeSelf here if it's set in OnStartLocalPlayer and FixedUpdate is only for local player
        if (!isLocalPlayer) return;

        float horizontal = inputHandler.GetHorizontalInput();
        CmdMove(horizontal, jump, kick, isGrounded);
    }

    /// <summary>
    /// Command sent from client to server to synchronize player movement and actions.
    /// </summary>
    /// <param name="horizontalInput">The horizontal input value.</param>
    /// <param name="jumpPressed">True if the jump button was pressed.</param>
    /// <param name="kick">True if the kick button was pressed.</param>
    /// <param name="isGrounded">True if the player is currently grounded.</param>
    [Command]
    private void CmdMove(float horizontalInput, bool jumpPressed, bool kick, bool isGrounded)
    {
        RpcMove(horizontalInput, jumpPressed, kick, isGrounded);

        if (kick && animator != null && networkAnimator != null)
        {
            animator.SetTrigger("Kick");
            animator.SetBool("IsKicking", true);
            networkAnimator.SetTrigger("Kick"); // Synchronize animation across the network
        }
        
    }

    /// <summary>
    /// ClientRpc called from server to all clients to apply movement and actions.
    /// </summary>
    /// <param name="horizontalInput">The horizontal input value.</param>
    /// <param name="jumpPressed">True if the jump button was pressed.</param>
    /// <param name="kick">True if the kick button was pressed.</param>
    /// <param name="isGrounded">True if the player is currently grounded.</param>
    [ClientRpc]
    private void RpcMove(float horizontalInput, bool jumpPressed, bool kick, bool isGrounded)
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        // Apply jump force if jump button was pressed and player is grounded
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Perform kick action
        if (kick)
        {
            CmdKick(areaSettings, isGrounded);
        }
    }

    [Command]
    private void CmdKick(Vector3 areaSettings, bool isGrounded)
    {
        // Call the kick action on all clients
        RpcKick(areaSettings, isGrounded);
    }
    /// <summary>
    /// Performs the kick action, applying force to the ball.
    /// </summary>
    /// <param name="areaSettings">The area settings for detecting the ball.</param>
    /// <param name="isGrounded">True if the player is currently grounded.</param>
    [ClientRpc]
    private void RpcKick(Vector3 areaSettings, bool isGrounded)
    {
        // OverlapAreaAll is used to find all colliders within a specified 2D area
        Collider2D[] hitColliders = Physics2D.OverlapAreaAll(transform.position, transform.position + areaSettings);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball") && hitCollider.TryGetComponent<Rigidbody2D>(out var ballRb))
            {
                Vector2 kickDirection = hitCollider.transform.position - transform.position;
                kickDirection.Normalize();

                if (isGrounded)
                {
                    kickDirection = new Vector2(kickDirection.x, upwardForce).normalized;
                }
                ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
                // Play kick sound effect
                SoundFXManager.instance.PlaySoundFX(kickSoundClip, transform);
            }
        }
    }

    /// <summary>
    /// Called by an animation event at the end of the kick animation.
    /// Resets the "IsKicking" animation parameter.
    /// </summary>
    public void OnKickAnimationEnd()
    {
        if (animator != null)
        {
            animator.SetBool("IsKicking", false);
        }
    }
}
