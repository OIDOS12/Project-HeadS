using UnityEngine;

/// <summary>
/// Handles player movement, jumping, and kicking for offline gameplay.
/// </summary>
public class OfflinePlayerMovementController : MonoBehaviour
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
    // No NetworkAnimator here
    [SerializeField] private AudioClip kickSoundClip;

    private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    private Vector3 groundCheckSizeReduction = new Vector3(0.03f, 0.2f, 0);
    private Vector3 positiveAreaSettings = new Vector3(0.8f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.8f, -0.8f, 0);
    private Vector3 areaSettings;

    private bool jumpInputReceived;
    private bool kickInputReceived;
    private bool isGrounded;

    // We no longer rely on 'Instance'. Instead, we need a direct reference.
    // [SerializeField] allows you to drag and drop it in the Inspector.
    // private LocalPlayerInputHandler inputHandler; // Original declaration
    [SerializeField] private LocalPlayerInputHandler inputHandler; // New: Allow Inspector assignment

    private void Awake()
    {
        rb = rb == null ? GetComponent<Rigidbody2D>() : rb;
        boxCollider = boxCollider == null ? GetComponent<BoxCollider2D>() : boxCollider;

        if (playerModel == null) Debug.LogError("PlayerModel not assigned.");
        if (animator == null) Debug.LogError("Animator not assigned.");

        // If 'inputHandler' isn't assigned in the Inspector, try to get it from the same GameObject
        // or find it in the scene as a fallback.
        if (inputHandler == null)
        {
            inputHandler = GetComponent<LocalPlayerInputHandler>();
        }
    }

    private void Start()
    {
        if (playerModel != null)
        {
            playerModel.SetActive(true);

            float yRotation = transform.eulerAngles.y;
        
            if (yRotation == 0) { areaSettings = positiveAreaSettings; }
            if (yRotation == 180) { areaSettings = negativeAreaSettings; }
        }

        // Subscribe to input events only if the inputHandler was successfully found/assigned
        if (inputHandler != null)
        {
            inputHandler.OnJumpPressed += HandleJumpInput;
            inputHandler.OnKickPressed += HandleKickInput;
        }
    }

    private void OnDestroy() // Unsubscribe when this component is destroyed
    {
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
        // Clamp player position if falling below threshold
        if (transform.position.y <= -3.69f)
        {
            transform.position = new Vector3(transform.position.x, -3.705019f, 0);
        }
    }

    private void FixedUpdate()
    {
        if (boxCollider == null) return;

        PerformGroundCheck();

        // Ensure inputHandler is valid before getting horizontal input
        float horizontal = 0f;
        if (inputHandler != null)
        {
            horizontal = inputHandler.GetHorizontalInput();
        }
        else
        {
            // Fallback: If inputHandler is missing, player won't move horizontally
            // or you could use Input.GetAxis("Horizontal") directly here for simple cases.
        }

        ApplyMovement(horizontal, jumpInputReceived, kickInputReceived, isGrounded);

        jumpInputReceived = false;
        kickInputReceived = false;
    }

    private void PerformGroundCheck()
    {
        isGrounded = false;
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

    private void ApplyMovement(float horizontalInput, bool jump, bool kick, bool isGrounded)
    {
        if (playerModel == null || !playerModel.activeSelf) return;

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (jump && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (kick)
        {
            if (animator != null)
            {
                animator.SetTrigger("Kick");
                animator.SetBool("IsKicking", true);
            }
            Kick(areaSettings, isGrounded);
        }
    }

    private void Kick(Vector3 currentAreaSettings, bool isGrounded)
    {
        Collider2D[] hitColliders = Physics2D.OverlapAreaAll(transform.position, transform.position + currentAreaSettings);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball") && hitCollider.TryGetComponent<Rigidbody2D>(out var ballRb))
            {
                Vector2 kickDirection = (hitCollider.transform.position - transform.position).normalized;
                if (isGrounded)
                {
                    kickDirection = new Vector2(kickDirection.x, upwardForce).normalized;
                }
                ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
                if (SoundFXManager.Instance != null && kickSoundClip != null)
                {
                    SoundFXManager.Instance.PlaySoundFX(kickSoundClip, transform);
                }
                break;
            }
        }
    }

    public void CallEndAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsKicking", false);
        }
    }
}