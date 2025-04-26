using UnityEngine;
using UnityEngine.Events;
using Mirror;
using UnityEngine.SceneManagement;
using Mirror.Examples.Basic;

public class PlayerController2D : NetworkBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    [SerializeField] private float kickForce = 10f;
    [SerializeField] private float upwardForce = 5f;
    [SerializeField] private bool m_AirControl = false;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_CeilingCheck;
    private bool m_Grounded;
    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 groundCheckMinusHeight = new Vector3(0, 0.1f, 0);
    private Vector3 groundCheckminusSize = new Vector3(0.03f, 0.2f, 0);

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnCrouchEvent;
    private BoxCollider2D boxCollider;
    private CircleCollider2D circleCollider;
    private Animator animator;
    private NetworkAnimator networkAnimator;
    private Vector3 positiveAreaSettings = new Vector3(0.8f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.8f, -0.8f, 0);
    private Vector3 areaSettings;

    // New variables for player force input
    public string playerForceAxis = "Horizontal"; // The input axis for player force.
    public float playerForceMagnitude = 10f; // Maximum force magnitude.
    [SerializeField] private NetworkIdentity ballIdentity;
    public GameObject PlayerModel;

    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "SampleScene")
        {
            Debug.Log(PlayerModel.activeSelf);
            if(PlayerModel.activeSelf == false)
            {
                PlayerModel.SetActive(true);
                m_Rigidbody2D = GetComponent<Rigidbody2D>();
                boxCollider = GetComponent<BoxCollider2D>();
                circleCollider = GetComponent<CircleCollider2D>();
                if (OnLandEvent == null)
                    OnLandEvent = new UnityEvent();
                if (OnCrouchEvent == null)
                    OnCrouchEvent = new BoolEvent();
                animator = GetComponent<Animator>();
                networkAnimator = GetComponent<NetworkAnimator>();
                if (animator == null)
                {
                    Debug.LogError("Animator component not found on the player!");
                }
                float yRotation = transform.eulerAngles.y;
                if (yRotation == 0) { areaSettings = positiveAreaSettings; }
                if (yRotation == 180) { areaSettings = negativeAreaSettings; }
                ballIdentity = GameObject.FindGameObjectWithTag("Ball").GetComponent<NetworkIdentity>();

            }

        }
    }
    [Client]
    private void FixedUpdate()
    {
        if(SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (!isLocalPlayer) { return; }
            bool wasGrounded = m_Grounded;
            m_Grounded = false;
            Collider2D[] collider = Physics2D.OverlapBoxAll(boxCollider.bounds.center - groundCheckMinusHeight, boxCollider.bounds.size - groundCheckminusSize, 0f, m_WhatIsGround);
            for (int i = 0; i < collider.Length; i++)
            {
                if (collider[i].gameObject != gameObject)
                {
                    m_Grounded = true;
                    if (!wasGrounded)
                        OnLandEvent.Invoke();
                    break;
                }
            }
        }
    }

    [Command]
    public void CmdRequestAuthority()
    {
        if (connectionToClient == ballIdentity.connectionToClient) return; // Already owned
        if (ballIdentity.connectionToClient != null)
        {
            ballIdentity.RemoveClientAuthority();
        }
        ballIdentity.AssignClientAuthority(connectionToClient);
    }

    [Command]
    public void ReciveCmdMove(float move, bool jump, bool kick, NetworkConnectionToClient newOwner)
    {
        RpcMove(move, jump, kick);
        if (kick)
        {
            animator.SetTrigger("Kick");
            animator.SetBool("IsKicking", true);
            networkAnimator.SetTrigger("Kick");
        }
    }

    [ClientRpc]
    public void RpcMove(float move, bool jump, bool kick)
    {
        if (m_Grounded || m_AirControl)
        {
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
            m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        }

        if (m_Grounded && jump)
        {
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
        if (kick && m_Grounded)
        {
            bool in_Air = false;
            Kick(areaSettings, in_Air);
        }
        if (kick && !m_Grounded)
        {
            bool in_Air = true;
            Kick(areaSettings, in_Air);
        }
    }


    public void Kick(Vector3 areaSettings, bool in_Air)
    {
        Collider2D[] hitColliders = Physics2D.OverlapAreaAll(transform.position, transform.position + areaSettings);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball"))
            {
                Rigidbody2D ballRb = hitCollider.GetComponent<Rigidbody2D>();
                if (ballRb != null)
                {
                    Vector2 kickDirection = hitCollider.transform.position - transform.position;
                    kickDirection.Normalize();
                    if (!in_Air)
                    {
                        kickDirection = new Vector2(kickDirection.x, upwardForce).normalized;
                        Debug.Log("Kick up");
                    }

                    ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
                    Debug.Log("Regular kick");
                }
            }
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
