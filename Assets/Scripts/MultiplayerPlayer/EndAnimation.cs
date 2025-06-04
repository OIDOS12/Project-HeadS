using Mirror;
using UnityEngine;

/// <summary>
/// Handles the end of a kick animation.
/// </summary>
public class EndAnimation : NetworkBehaviour
{
    [SerializeField] PlayerMovementController playerMovementController;
    public void CallEndAnimation() => playerMovementController.OnKickAnimationEnd();
}