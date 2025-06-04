// using UnityEngine;
// using Mirror;

// /// <summary>
// /// Handles network authority transfer for the ball in a Mirror networking environment.
// /// When a player (client) interacts with this ball, this script, running on the server,
// /// assigns network authority over the ball's NetworkIdentity to that player's connection.
// /// This enables the client to control the ball's Rigidbody2D and physics authoritatively,
// /// reducing latency for the interacting player.
// /// </summary>
// [RequireComponent(typeof(NetworkIdentity))] // Ensures the ball has a NetworkIdentity
// [RequireComponent(typeof(Rigidbody2D))]    // Ensures the ball has a Rigidbody2D
// public class BallAuthority : NetworkBehaviour
// {
//     /// <summary>
//     /// The tag used to identify player GameObjects.
//     /// Ensure your player prefabs have this tag set in the Unity Inspector.
//     /// </summary>
//     [SerializeField] private string playerTag = "Player";

//     private NetworkIdentity ballNetworkIdentity;
//     private Rigidbody2D ballRigidbody;

//     /// <summary>
//     /// Called when the script instance is being loaded.
//     /// This method retrieves references to the ball's <see cref="NetworkIdentity"/>
//     /// and <see cref="Rigidbody2D"/> components. It also includes error logging
//     /// if these essential components are not found.
//     /// </summary>
//     void Awake()
//     {
//         ballNetworkIdentity = GetComponent<NetworkIdentity>();
//         ballRigidbody = GetComponent<Rigidbody2D>();

//         if (ballNetworkIdentity == null)
//         {
//             Debug.LogError("BallAuthority: NetworkIdentity component not found on this GameObject. Please add one.", this);
//         }
//         if (ballRigidbody == null)
//         {
//             Debug.LogError("BallAuthority: Rigidbody2D component not found on this GameObject. Please add one.", this);
//         }
//     }

//     /// <summary>
//     /// Called when another collider enters this trigger collider.
//     /// This method is used to detect when a player (identified by <c>playerTag</c>)
//     /// interacts with the ball.
//     /// <para>
//     /// IMPORTANT: This logic must only execute on the server, as network authority
//     /// changes are server-authoritative operations. If the ball already has an owner,
//     /// its authority is removed before being assigned to the new interacting player.
//     /// </para>
//     /// <remarks>
//     /// For this method to be called, the ball's Collider2D must be set to 'Is Trigger'
//     /// in the Unity Inspector.
//     /// </remarks>
//     /// </summary>
//     /// <param name="other">The <see cref="Collider2D"/> that entered the trigger.</param>
   
//     [ServerCallback] // Ensures this method is only called on the server
//     void OnTriggerEnter2D(Collider2D other)
//     {
//         // Check if the colliding object has the designated player tag.
//         if (other.CompareTag(playerTag))
//         {
//             // Get the NetworkIdentity of the interacting player.
//             NetworkIdentity playerNetworkIdentity = other.GetComponent<NetworkIdentity>();

//             if (playerNetworkIdentity != null)
//             {
//                 // Remove authority from the current owner if there is one.
//                 // This ensures a clean transfer of ownership.
//                 if (ballNetworkIdentity.connectionToClient != null)
//                 {
//                     ballNetworkIdentity.RemoveClientAuthority();
//                 }

//                 // Assign network authority of this ball to the interacting player's connection.
//                 // This allows the client associated with 'playerNetworkIdentity' to send
//                 // authoritative physics updates for this ball to the server.
//                 ballNetworkIdentity.AssignClientAuthority(playerNetworkIdentity.connectionToClient);
//                 Debug.Log($"BallAuthority: Authority assigned to player connection ID: {playerNetworkIdentity.connectionToClient.connectionId}");
//             }
//             else
//             {
//                 Debug.LogWarning($"BallAuthority: Collided with object tagged '{playerTag}' but it does not have a NetworkIdentity component. Cannot assign authority.", other.gameObject);
//             }
//         }
//     }
// }