using Mirror;
using UnityEngine;

public class PlayerMoves : NetworkBehaviour
{
    float horizontalMove = 0f;
    [SerializeField] private string HorizontalInput = "Horizontal";
    [SerializeField] private string JumpKeyInput = "Jump";
    [SerializeField] private float runSp = 16f;
    private bool jump = false;
    private PlayerController2D controller;
    
    [SerializeField] private bool usePositiveArea;
    public KeyCode kickKey = KeyCode.Space;
    private bool kick = false;
    
    private void Start()
    {
        controller = GetComponent<PlayerController2D>();

    }

    [Client]
    private void Update()
    {
        if (!isLocalPlayer) { return; }
        
        horizontalMove = Input.GetAxisRaw(HorizontalInput) * runSp;
        if (Input.GetKeyDown(kickKey))
        {
            controller.CmdRequestAuthority();
            kick = true;
        }
        if (Input.GetButtonDown(JumpKeyInput))
        {
            controller.CmdRequestAuthority();
            jump = true;
        }
    }

    [Client]
    void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }
        // if (horizontalMove == 0 && jump != true) { return; } - no warnings if included
        controller.ReciveCmdMove(horizontalMove * Time.fixedDeltaTime, jump, kick, connectionToClient);
        jump = false;
        kick = false;
        
    }
}