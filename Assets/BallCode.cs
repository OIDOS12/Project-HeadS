using Mirror;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private Rigidbody2D rb;
    private NetworkIdentity ballIdentity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ballIdentity = GetComponent<NetworkIdentity>();
    }

    [Server]
    public void AssignAuthority(NetworkConnectionToClient newOwner)
    {
        if (ballIdentity.connectionToClient == newOwner) return; // Already owned

        // Remove previous authority if there was one
        if (ballIdentity.connectionToClient != null)
        {
            ballIdentity.RemoveClientAuthority();
        }

        // Assign new authority
        ballIdentity.AssignClientAuthority(newOwner);
        Debug.Log($"Ball authority given to {newOwner}");
    }

    [Server]
    public void KickBall(Vector2 force, NetworkConnectionToClient playerConn)
    {
        AssignAuthority(playerConn); // Give authority to the player
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}
