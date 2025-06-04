using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;

/// <summary>
/// Custom Network Manager for handling player connections.
/// </summary>
public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public List<PlayerObjectController> PlayerList { get; } = new List<PlayerObjectController>();

    /// <summary>
    /// When players are added to the server, this method is called.
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerObjectController GamePlayerInstance = Instantiate(GamePlayerPrefab);
            GamePlayerInstance.ConnectionID = conn.connectionId;
            GamePlayerInstance.PlayerID = PlayerList.Count + 1;
            GamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, PlayerList.Count);

            NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
        }
    }
    
    /// <summary>
    /// Starts the game by changing the scene to the specified scene name.
    /// </summary>
    /// <param name="sceneName"></param>
    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }
}
