using UnityEngine;

/// <summary>
/// Handles and centralizes player input.
/// This is no longer a singleton. An instance of this class should be placed
/// on a GameObject in your scene where input is needed.
/// </summary>
public class LocalPlayerInputHandler : MonoBehaviour
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
            OnJumpPressed?.Invoke(); 
        }

        // Check for kick input
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