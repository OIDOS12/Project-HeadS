using UnityEngine;


public class LocalMovement : MonoBehaviour
{
    [SerializeField] public float runSp = 16f;
    float horizontalMove = 0f;
    bool jump = false;
    public KeyCode kickKey = KeyCode.Space;
    
    [SerializeField] private LocalPlayerController2D controller;
    [SerializeField] private bool usePositiveArea;
    [SerializeField] private string HorizontalInput = "Horizontal";
    [SerializeField] private string JumpKeyInput = "Jump";
    private Vector3 positiveAreaSettings = new Vector3(0.8f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.8f, -0.8f, 0);


    public Vector3 areaSettings;

    private void Start()
    {
        controller = GetComponent<LocalPlayerController2D>();
        float yRotation = transform.eulerAngles.y;
        if (yRotation == 0) {areaSettings = positiveAreaSettings; }
        if (yRotation == 180) {areaSettings = negativeAreaSettings; }
    }
    void Update()
    {
       horizontalMove = Input.GetAxisRaw(HorizontalInput) * runSp;

        if (Input.GetButtonDown(JumpKeyInput))
        {
            jump = true;
        }
        if (Input.GetKeyDown(kickKey))
        {
            controller.Kick(areaSettings);
        }

    }

    void FixedUpdate()
    {
        //Move player
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;

    }
        private void OnDrawGizmos()
    {
        Vector2 pointA = transform.position;
        Vector2 pointB = transform.position + areaSettings;
        Color gizmoColor = Color.green;
        Gizmos.color = gizmoColor;

        Vector2 topLeft = new Vector2(Mathf.Min(pointA.x, pointB.x), Mathf.Max(pointA.y, pointB.y));
        Vector2 topRight = new Vector2(Mathf.Max(pointA.x, pointB.x), Mathf.Max(pointA.y, pointB.y));
        Vector2 bottomLeft = new Vector2(Mathf.Min(pointA.x, pointB.x), Mathf.Min(pointA.y, pointB.y));
        Vector2 bottomRight = new Vector2(Mathf.Max(pointA.x, pointB.x), Mathf.Min(pointA.y, pointB.y));

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}