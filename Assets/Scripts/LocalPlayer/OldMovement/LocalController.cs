using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Collections;

public class LocalPlayerController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
	[SerializeField] private float kickForce = 10f; 

	[SerializeField] private float upwardForce = 5f; 	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	const float k_GroundedRadius = .1f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .1f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	private Vector3 m_Velocity = Vector3.zero;

    private Vector3 minusHeight = new Vector3(0, 0.01f, 0);
    private Vector3 centerPoint;
	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
    
	private BoxCollider2D boxCollider; 
	private Animator animator;


	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
		
		 animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on the player!");
            }
	}

	private void FixedUpdate()
	{

		bool wasGrounded = m_Grounded;
        m_Grounded = false;

		Collider2D[] collider = Physics2D.OverlapBoxAll(boxCollider.bounds.center - minusHeight, boxCollider.bounds.size, 0f, m_WhatIsGround);

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


	public void Move(float move, bool jump)
	{


		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{
			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}
	}
	public void Kick(Vector3 areaSettings)
    {
		
        Collider2D[] hitColliders = Physics2D.OverlapAreaAll(transform.position, transform.position + areaSettings);
		PlayKickAnimation();
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball"))
            {
                Rigidbody2D ballRb = hitCollider.GetComponent<Rigidbody2D>();

                if (ballRb != null)
                {
                    // Розраховуємо напрямок удару (горизонтальний)
                    Vector2 kickDirection = hitCollider.transform.position - transform.position;
                    kickDirection.Normalize();
                    if (m_Grounded)
                    {
                        kickDirection = new Vector2(kickDirection.x, upwardForce).normalized; // 1f - сила підйому, можна налаштувати

                    }
                    ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
                }
            }
        }
    }
    private void PlayKickAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Kick"); // Запускаємо тригер анімації
                animator.SetBool("IsKicking", true); // Встановлюємо булеву змінну в true

            }
        }

        public void CallEndAnimation()
        {
            animator.SetBool("IsKicking", false); // Встановлюємо булеву змінну в false
        }




	    private void OnDrawGizmos2()
        {
            if (boxCollider == null)
                return;

            Gizmos.color = Color.yellow; // Колір області
            Gizmos.DrawWireCube(boxCollider.bounds.center - minusHeight, boxCollider.bounds.size);
        }
}