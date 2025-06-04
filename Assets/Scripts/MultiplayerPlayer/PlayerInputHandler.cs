using UnityEngine;
using Mirror;

/// <summary>
/// Handles and centralizes player input for a local player.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string jumpKey = "Jump";
    [SerializeField] private string kickKey = "Kick";

    public event System.Action OnJumpPressed;

    public event System.Action OnKickPressed;

    /// <summary>
    /// Checks for player input and invokes events.
    /// </summary>
    private void Update()
    {

        if (Input.GetButtonDown(jumpKey))
        {
            OnJumpPressed?.Invoke();
        }

        if (Input.GetButtonDown(kickKey))
        {
            OnKickPressed?.Invoke();
        }
    }

    /// <summary>
    /// Gets the current horizontal input value.
    /// </summary>
    /// <returns>The horizontal input value, between -1 and 1.</returns>
    public float GetHorizontalInput()
    {
        return Input.GetAxis(horizontalAxis);
    }
}