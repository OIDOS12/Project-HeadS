using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages the Steam lobby functionality, including creating, joining, and listing lobbies.
/// </summary>
public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;
    private NetworkManager networkManager;
    [SerializeField] private HostOrJoinSceneManager HostSceneManager;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequest;
    protected Callback<LobbyEnter_t> lobbyEntered;

    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdate;

    public List<CSteamID> lobbyIds = new List<CSteamID>();
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    public TMP_Text SteamInitializationText;

    /// <summary>
    /// Ensures that this is a singleton instance.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Initializes the lobby manager and sets up callbacks.
    /// </summary>
    private void Start()
    {
        if (SteamInitializationText == null)
        {
            Debug.LogError("SteamInitializationText is not assigned in the inspector.");
            return;
        }

        if (!SteamManager.Initialized)
        {
            SteamInitializationText.gameObject.SetActive(true);
            SteamInitializationText.text = "Steam not initialized";
            SteamInitializationText.color = Color.red;
            return;
        }

        networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager component not found on SteamLobby GameObject.");
            return;
        }

        if (HostSceneManager == null)
        {
            HostSceneManager = FindFirstObjectByType<HostOrJoinSceneManager>();
            if (HostSceneManager == null)
            {
                Debug.LogError("HostOrJoinSceneManager reference not set and not found in scene.");
            }
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
    }

    /// <summary>
    /// Creates a new Steam lobby and starts hosting the game.
    /// This method is called when the player chooses to host a lobby.
    /// </summary>
    public void HostLobby()
    {
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager is null in HostLobby.");
            return;
        }
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
    }

    /// <summary>
    /// Called when a lobby is created successfully.
    /// </summary>
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        Debug.Log("Lobby created");
        networkManager.StartHost();

        var lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyId, "name", SteamFriends.GetPersonaName() + "'s LOBBY");
    }

    /// <summary>
    /// Called when a player requests to join a lobby.
    /// </summary>
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    /// <summary>
    /// Called when the player successfully enters a lobby. 
    /// It sets the current lobby ID and starts the client.
    /// </summary>
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (NetworkServer.active) { return; }

        var lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        networkManager.networkAddress = SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey);
        networkManager.StartClient();
    }

    /// <summary>
    /// Joins an existing Steam lobby using the provided lobby ID.
    /// This method is called when the player selects a lobby to join from the lobby list.
    /// </summary>
    /// <param name="lobbyID"></param>
    public void JoinLobby(CSteamID lobbyID)
    {
        Debug.Log("Joining lobby: " + lobbyID);
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    /// <summary>
    /// Requests a list of available Steam lobbies.
    /// </summary>
    public void GetLobbiesList()
    {
        if (lobbyIds.Count > 0) { lobbyIds.Clear(); }
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(100);
        SteamMatchmaking.RequestLobbyList();
    }

    /// <summary>
    /// Callback method that is called when the lobby list is received from Steam.
    /// It processes the list of lobbies and requests their data.
    /// </summary>
    private void OnGetLobbyList(LobbyMatchList_t result)
    {
        if (HostSceneManager == null)
        {
            HostSceneManager = FindFirstObjectByType<HostOrJoinSceneManager>();
            if (HostSceneManager == null)
            {
                Debug.LogWarning("HostSceneManager is null in OnGetLobbyList. Cannot destroy lobbies.");
                return;
            }
        }

        if (HostSceneManager.ListOfLobbies.Count > 0)
        {
            HostSceneManager.DestroyLobbies();
        }

        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIds.Add(lobbyId);
            SteamMatchmaking.RequestLobbyData(lobbyId);
        }
    }

    /// <summary>
    /// Callback method that is called when the lobby data is updated. 
    /// </summary>
    private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
    {
        if (HostSceneManager == null)
        {
            HostSceneManager = FindFirstObjectByType<HostOrJoinSceneManager>();
            if (HostSceneManager == null)
            {
                Debug.LogWarning("HostSceneManager is null in OnLobbyDataUpdate. Cannot display lobbies.");
                return;
            }
        }
        HostSceneManager.DisplayLobbies(lobbyIds, result);
    }

    public void ToMainMenu() => SceneManager.LoadScene("MainMenu");

    /// <summary>
    /// Leaves the current Steam lobby and stops the network session.
    /// Handles scene transitions appropriately.
    /// </summary>
    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));

        if (networkManager != null)
        {
            if (networkManager.isNetworkActive)
            {
                if (NetworkServer.active)
                {
                    networkManager.StopHost();
                }
                else
                {
                    networkManager.StopClient();
                }
            }
        }
        else
        {
            Debug.LogWarning("NetworkManager is null in LeaveLobby.");
        }
    }
}
