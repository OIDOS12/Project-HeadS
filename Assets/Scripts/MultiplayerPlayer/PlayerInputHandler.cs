using UnityEngine;
using Mirror; // Add Mirror namespace for NetworkBehaviour

/// <summary>
/// Handles and centralizes player input for a *single* local player.
/// This component should be attached to the local player's game object.
/// </summary>
public class PlayerInputHandler : MonoBehaviour // Make it a NetworkBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string jumpKey = "Jump";
    [SerializeField] private string kickKey = "Kick";

    /// <summary>
    /// Event fired when the jump button is pressed down.
    /// </summary>
    public event System.Action OnJumpPressed;

    /// <summary>
    /// Event fired when the kick button is pressed down.
    /// </summary>
    public event System.Action OnKickPressed;

    private void Update()
    {
        // Check for jump input
        if (Input.GetButtonDown(jumpKey))
        {
            OnJumpPressed?.Invoke(); // Invoke the jump event
        }

        // Check for kick input
        if (Input.GetButtonDown(kickKey))
        {
            OnKickPressed?.Invoke(); // Invoke the kick event
        }
    }

    /// <summary>
    /// Gets the current horizontal input value.
    /// </summary>
    /// <returns>The horizontal input value, typically between -1 and 1.</returns>
    public float GetHorizontalInput()
    {
        return Input.GetAxis(horizontalAxis);
    }
}