using UnityEngine;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine.SceneManagement;
public class PlayerMovementController : NetworkBehaviour
{
    //Player settings
    [SerializeField] private float Speed = 5f; // Speed of the player movement
    [SerializeField] private float JumpForce = 10f; // Force of the jump
    [SerializeField] private float kickForce = 10f; // Force of the kick
    [SerializeField] private float upwardForce = 0.9f; // Upward force for the kick
    
    public GameObject PlayerModel; // Reference to the player model
    public Rigidbody2D rb; // Reference to the Rigidbody2D component
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private Transform CeilingCheck;
    private BoxCollider2D boxCollider;
    
    private Vector3 groundCheckMinusHeight = new Vector3(0, 0.1f, 0);
    private Vector3 groundCheckminusSize = new Vector3(0.03f, 0.2f, 0);

    private Vector3 positiveAreaSettings = new Vector3(0.8f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.8f, -0.8f, 0);
    [SerializeField] private Vector3 areaSettings;

    //Player movement variables    
    public string HorizontalAxis = "Horizontal"; // Name of the horizontal input axis
    public string jumpKey = "Jump"; // Name of the jump key
    public string KickKey = "Kick";

    private bool jump = false; // Flag to check if jump is pressed
    private bool kick = false; // Flag to check if kick is pressed
    private bool isGrounded;
    [SerializeField] private Animator animator; // Reference to the Animator component
    [SerializeField] private NetworkAnimator networkAnimator; // Reference to the Animator component

    //Sounds
    [SerializeField] private AudioClip kickSoundClip; // Reference to the jump sound effect

    private void Start()
    {
        PlayerModel.SetActive(false); // Deactivate the player model at the start
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "SampleScene")
        {
            if(PlayerModel.activeSelf == false)
            {
                SetAreaSettings();
                PlayerModel.SetActive(true); // Activate the player model
                boxCollider = GetComponent<BoxCollider2D>();
            }
        }
        if (!isLocalPlayer) return;
        if(Input.GetButtonDown(jumpKey))
        {
            jump = true; // Check if the jump key is pressed
        }

        if(Input.GetButtonDown(KickKey))
        {
            kick = true; // Check if the kick key is pressed
        }
        if (transform.position.y <= -3.69f)
        {
            transform.position = new Vector3(transform.position.x, -3.705019f, 0); // Reset the player's position if it falls below a certain point
        }
    }
    

    private void FixedUpdate()
    {
        if(isLocalPlayer && SceneManager.GetActiveScene().name == "SampleScene") // Check if the player has authority over this object
        {
            //Ground check
            isGrounded = false;
            Collider2D[] collider = Physics2D.OverlapBoxAll(boxCollider.bounds.center - groundCheckMinusHeight, boxCollider.bounds.size - groundCheckminusSize, 0f, WhatIsGround);
            for (int i = 0; i < collider.Length; i++)
            {
                if (collider[i].gameObject != gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }

            Movement(jump, kick, isGrounded); // Call the movement function
            jump = false; // Reset the jump flag
            kick = false; // Reset the kick flag
        }
    }
    
    private void SetAreaSettings()
    {

        if (netId == 1)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0); // Set the rotation of each player to (0, 0, 0)
            areaSettings = negativeAreaSettings; // Set the area settings for the kick
        }
        else
        {
            areaSettings = positiveAreaSettings; // Set the area settings for the kick
        }
    }

    void Movement(bool jump, bool kick, bool isGrounded)
    {
        if (!isLocalPlayer) return; // Only process movement on local player

        if(PlayerModel.activeSelf == true)
        {
            CmdMove(Input.GetAxis(HorizontalAxis), jump, kick, isGrounded); // Send the movement command to the server
        }
    }

    private void Kick(Vector3 areaSettings, bool isGrounded)
    {
        Collider2D[] hitColliders = Physics2D.OverlapAreaAll(transform.position, transform.position + areaSettings);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball"))
            {
                if (hitCollider.TryGetComponent<Rigidbody2D>(out var ballRb))
                {
                    Vector2 kickDirection = hitCollider.transform.position - transform.position;
                    kickDirection.Normalize();
                    if (isGrounded)
                    {
                        kickDirection = new Vector2(kickDirection.x, upwardForce).normalized;
                        Debug.Log("Kick up");
                    }

                    ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
                    Debug.Log("Regular kick");
                    SoundFXManager.instance.PlaySoundFX(kickSoundClip, transform); // Play the kick sound effect
                }
            }
        }
    }
    
    [Command]
    void CmdMove(float horizontalInput, bool jumpPressed, bool kick, bool isGrounded)
    {
        RpcMove(horizontalInput, jumpPressed, kick, isGrounded); // Call the client RPC to move the player
        if (kick)
        {
            animator.SetTrigger("Kick"); // Trigger the kick animation
            animator.SetBool("IsKicking", true); // Set the kick animation state
            networkAnimator.SetTrigger("Kick");
        }
    }

    [ClientRpc]
    public void RpcMove(float horizontalInput, bool jumpPressed, bool kick, bool isGrounded)
    {
        if (!NetworkClient.ready) return; // Ensure the client is ready before executing the command

        rb.linearVelocity = new Vector2(horizontalInput * Speed, rb.linearVelocity.y); // Set the Rigidbody2D velocity based on the input
        
        if (jumpPressed && isGrounded) // If jump is true, set the vertical direction to jump force
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse); // Add force to the Rigidbody2D for jumping
        }

        if (kick)
        {
            Kick(areaSettings, isGrounded);
        }
    }

    public void OnKickAnimationEnd()
    {
        animator.SetBool("IsKicking", false);
    }

    void OnDrawGizmos()
    {
        if (boxCollider == null) return;

        Vector3 overlapCenter = boxCollider.bounds.center - groundCheckMinusHeight;
        Vector3 overlapSize = boxCollider.bounds.size - groundCheckminusSize;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(overlapCenter, overlapSize);
    }
}
