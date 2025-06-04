using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player movement, jumping, and kicking in a networked environment.
/// </summary>
public class PlayerMovementController : NetworkBehaviour
{
    private PlayerInputHandler inputHandler;
    
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float kickForce = 10f;
    [SerializeField] private float upwardForce = 0.9f;

    [Header("References")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private BoxCollider2D groundCheckCollider; 
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private AudioClip kickSoundClip;

    [Header("Area Settings")]
    private Vector3 positiveAreaSettings = new Vector3(0.8f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.8f, -0.8f, 0);
    private Vector3 areaSettings;

    [Header("Input States")]
    private bool jumpInputReceived;
    private bool kickInputReceived;
    private bool isGrounded;
    private int groundContactCount = 0;

    /// <summary>
    /// Caches references to components and logs errors.
    /// </summary>
    private void Awake()
    {
        // Cache references if not set in inspector
        rb = rb == null ? GetComponent<Rigidbody2D>() : rb;
        groundCheckCollider = groundCheckCollider == null ? GetComponent<BoxCollider2D>() : groundCheckCollider;

        // Log errors if critical references are missing
        if (playerModel == null) Debug.LogError("PlayerModel not assigned.");
        if (animator == null) Debug.LogError("Animator not assigned.");
        if (networkAnimator == null) Debug.LogError("NetworkAnimator not assigned.");
    }

    /// <summary>
    /// Disables the player model and Rigidbody2D simulation at the start.
    /// </summary>
    private void Start()
    {
        if (playerModel != null)
        {
            playerModel.SetActive(false);
            rb.simulated = false;
        }
    }

    /// <summary>
    /// Called when the local player starts.
    /// Initializes input handling and sets area settings based on player netId.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        // Get the PlayerInputHandler from player's game object
        inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler == null)
        {
            return;
        }

        inputHandler.OnJumpPressed += HandleJumpInput;
        inputHandler.OnKickPressed += HandleKickInput;

        SetAreaSettings();
    }

    /// <summary>
    /// Called when the local player stops.
    /// Unsubscribes from input events to prevent memory leaks.
    /// </summary>
    public override void OnStopLocalPlayer()
    {
        // Unsubscribe to prevent memory leaks when player is destroyed
        if (inputHandler != null)
        {
            inputHandler.OnJumpPressed -= HandleJumpInput;
            inputHandler.OnKickPressed -= HandleKickInput;
        }
    }

    /// <summary>
    /// Handles jump input from the PlayerInputHandler.
    /// </summary>
    private void HandleJumpInput()
    {
        jumpInputReceived = true;
    }

    /// <summary>
    /// Handles kick input from the PlayerInputHandler.
    /// </summary>
    private void HandleKickInput()
    {
        kickInputReceived = true;
    }

    /// <summary>
    /// Updates the player model and area settings when entering the game scene.
    /// </summary>
    private void Update()
    {
        // Activate player model and set area settings when entering the game scene
        // Assuming "OnlineGameScene" is the scene where active gameplay occurs
        if (SceneManager.GetActiveScene().name == "OnlineGameScene" && playerModel != null && !playerModel.activeSelf)
        {
            playerModel.SetActive(true);
            rb.simulated = true; // Enable physics simulation for the local player
        }

        if (!isServer) return;

        // Clamp player position if falling below threshold
        Vector3 position = transform.position;
        if (transform.position.y <= -3.704f) position.y = -3.705019f;

        if (position.x <= -10f) { position.x = -6f; }

        else if (position.x >= 10f) { position.x = 6f; }

        transform.position = position;
    }

    /// <summary>
    /// FixedUpdate is called at a fixed interval and is used for physics-related updates.
    /// It processes player input and sends movement commands to the server.
    /// </summary>
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return; // Only local player's FixedUpdate should process input and send commands

        if (groundCheckCollider == null) return;

        // Ensure playerModel is active before handling movement (relevant if not activated in OnStartLocalPlayer)
        if (playerModel != null && playerModel.activeSelf)
        {
            HandleMovement(jumpInputReceived, kickInputReceived, isGrounded);
        }

        // Reset input flags after processing
        jumpInputReceived = false;
        kickInputReceived = false;
    }

    /// <summary>
    /// Called when another collider enters the trigger.
    /// This method checks if the collider is on the ground layer.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsGround) != 0) // bitwise check if the layer is in whatIsGround
        {
            groundContactCount++;
            UpdateIsGrounded(); 
        }
    }

    /// <summary>
    /// Called when another collider exits the trigger.
    /// This method checks if the collider is on the ground layer and updates the ground contact count.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsGround) != 0)
        {
            groundContactCount--;
            groundContactCount = Mathf.Max(0, groundContactCount);
            UpdateIsGrounded();
        }
    }

    /// <summary>
    /// If groundContactCount is greater than 0, isGrounded is set to true, otherwise - false.
    /// </summary>
    private void UpdateIsGrounded()
    {
        isGrounded = groundContactCount > 0;
    }

    /// <summary>
    /// Sets area settings and player orientation based on netId.
    /// Used for team assignment.
    /// </summary>
    private void SetAreaSettings()
    {
        if (netId == 1)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
            areaSettings = negativeAreaSettings;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); 
            areaSettings = positiveAreaSettings;
        }
    }

    /// <summary>
    /// Handles local input and sends movement commands to the server.
    /// </summary>
    private void HandleMovement(bool jump, bool kick, bool isGrounded)
    {
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
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y); // horizontal movement

        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (kick)
        {
            CmdKick(areaSettings, isGrounded);
        }
    }

    /// <summary>
    /// Command sent from client to server to perform a kick action.
    /// </summary>
    /// <param name="areaSettings"></param>
    /// <param name="isGrounded"></param>
    [Command(requiresAuthority = false)]
    private void CmdKick(Vector3 areaSettings, bool isGrounded)
    {
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
                SoundFXManager.Instance.PlaySoundFX(kickSoundClip, transform);
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