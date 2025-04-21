using Mirror;
using UnityEngine;

public class EndAnimation : NetworkBehaviour
{
    [SerializeField] PlayerMovementController playerMovementController;
    public void CallEndAnimation()
    {
        playerMovementController.OnKickAnimationEnd();
    }
}