using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;
using Steamworks;
using Edgegap;

public class PlayerObjectController : NetworkBehaviour
{
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerID;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool isReady = false;
    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null) { return manager; }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
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

    [Command]
    public void CmdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.isReady, !this.isReady);
    }

    public void ChangeReadyState()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStartClient()
    {
        Manager.PlayerList.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.PlayerList.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string name)
    {
        this.PlayerNameUpdate(this.PlayerName, name);
    }
    public void PlayerNameUpdate(string oldName, string newName)
    {
        if (isServer)
        {
            this.PlayerName = newName;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    public void CanStartGame(string sceneName)
    {
        if (isServer)
        {
            CmdCanStartGame(sceneName);
        }
    }

    [Command]
    public void CmdCanStartGame(string sceneName)
    {
        manager.StartGame(sceneName);
    }
}
