using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] public float runSp = 16f;
    float horizontalMove = 0f;
    bool jump = false;
    private bool kick = false;
    public KeyCode kickKey = KeyCode.Space;

    [SerializeField] private LocalPlayerController2D controller;
    [SerializeField] private bool usePositiveArea;
    [SerializeField] private string HorizontalInput = "Horizontal";
    [SerializeField] private string JumpKeyInput = "Jump";
    private Vector3 positiveAreaSettings = new Vector3(0.78f, -0.8f, 0);
    private Vector3 negativeAreaSettings = new Vector3(-0.78f, -0.8f, 0);


    public Vector3 areaSettings
    {
        get
        {
            return usePositiveArea ? positiveAreaSettings : negativeAreaSettings;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        horizontalMove = Input.GetAxisRaw(HorizontalInput) * runSp;

        if (Input.GetButtonDown(JumpKeyInput))
        {
            jump = true;

        }

        if (horizontalMove != 0)
        {

        }

        if (Input.GetKeyDown(kickKey))
        {
            kick = true;
        }
    }

    void FixedUpdate()
    {
        // Move player
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }

}
