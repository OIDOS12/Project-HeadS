using Mirror;
using Steamworks;

/// <summary>
/// Controls the player object in the multiplayer game, managing player data and interactions.
/// </summary>
public class PlayerObjectController : NetworkBehaviour
{
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerID;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool isReady = false;
    private CustomNetworkManager manager;

    /// <summary>
    /// Gets the CustomNetworkManager instance, ensuring it is initialized.
    /// </summary>
    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null) { return manager; }
            return manager = NetworkManager.singleton as CustomNetworkManager;
        }
    }
    
    /// <summary>
    /// Don't destroy this object on scene load, allowing it to persist across scenes.
    /// </summary>
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Updates the player's ready state and notifies clients of the change.
    /// </summary>
    /// <param name="oldReady"></param>
    /// <param name="newReady"></param>
    private void PlayerReadyUpdate(bool oldReady, bool newReady)
    {
        if (isServer)
        {
            isReady = newReady;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    /// <summary>
    /// Command to set the player's ready state. This is called by the client and executed on the server.
    /// </summary>
    [Command]
    public void CmdSetPlayerReady()
    {
        PlayerReadyUpdate(isReady, !isReady);
    }

    /// <summary>
    /// Changes the player's ready state by calling the command to set it. This is typically called when the player clicks a "Ready" button.
    /// </summary>
    public void ChangeReadyState()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    /// <summary>
    /// Called when the player object gains authority on the client. It sets the player's name and updates the lobby.
    /// </summary>
    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdatePlayerList();
    }

    /// <summary>
    /// Called when the player object is started on the client. It adds the player to the player list and updates the lobby name and player list.
    /// </summary>
    public override void OnStartClient()
    {
        Manager.PlayerList.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    /// <summary>
    /// Called when the player object stops being a client. It removes the player from the player list and updates the lobby.
    /// </summary>
    public override void OnStopClient()
    {
        Manager.PlayerList.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    /// <summary>
    /// Command to set the player's name. This is called by the client and executed on the server.
    /// </summary>
    /// <param name="name"></param>
    [Command]
    private void CmdSetPlayerName(string name)
    {
        PlayerNameUpdate(PlayerName, name);
    }

    /// <summary>
    /// Updates the player's name and notifies clients of the change.
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    public void PlayerNameUpdate(string oldName, string newName)
    {
        if (isServer)
        {
            PlayerName = newName;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    /// <summary>
    /// Checks if the player can start the game.
    /// </summary>
    /// <param name="sceneName"></param>
    public void CanStartGame(string sceneName)
    {
        if (isServer)
        {
            CmdCanStartGame(sceneName);
        }
    }

    /// <summary>
    /// Command to start the game by changing the scene.
    /// </summary>
    /// <param name="sceneName"></param>
    [Command]
    public void CmdCanStartGame(string sceneName)
    {
        manager.StartGame(sceneName);
    }
}
