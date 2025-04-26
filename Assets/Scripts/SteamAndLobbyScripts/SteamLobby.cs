using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;
    private NetworkManager networkManager;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequest;
    protected Callback<LobbyEnter_t> lobbyEntered;

    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdate;

    public List<CSteamID> lobbyIds = new List<CSteamID>();
    public ulong CurrentLobbyID;
    private const string HostAddresKey = "HostAddress";
    public TMP_Text SteamInitializationText;
    // public TMP_Text lobbyNameText;
    // public GameObject hostButton;

    private void Start()
    {
        if(Instance == null) { Instance = this; }

        if (!SteamManager.Initialized) 
        { 
            SteamInitializationText.gameObject.SetActive(true);
            SteamInitializationText.text = "Steam not initialized"; 
            SteamInitializationText.color = Color.red;
            return; 
        }

        networkManager = GetComponent<NetworkManager>();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
    }
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        Debug.Log("Lobby created");
        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddresKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName() + "'s LOBBY");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {

        CurrentLobbyID = callback.m_ulSteamIDLobby;
        
        if (NetworkServer.active) { return; }

        networkManager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddresKey);
    
        networkManager.StartClient();
    }

    public void JoinLobby(CSteamID lobbyID)
    {
        Debug.Log("Joining lobby: " + lobbyID);
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    public void GetLobbiesList()
    {
        if(lobbyIds.Count > 0) { lobbyIds.Clear(); }
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList(); 
    }
    
    private void OnGetLobbyList(LobbyMatchList_t result)
    {
        if(HostOrJoinSceneManager.Instance.ListOfLobbies.Count > 0) { HostOrJoinSceneManager.Instance.DestroyLobbies(); }
        
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIds.Add(lobbyId);
            SteamMatchmaking.RequestLobbyData(lobbyId);
        }
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
    {
        HostOrJoinSceneManager.Instance.DisplayLobbies(lobbyIds, result);
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;
        if (networkManager.isNetworkActive)
        {
            networkManager.StopHost();
        }
        else
        {
            networkManager.StopClient();
        }

        if (SceneManager.GetActiveScene().name == "Lobby" || SceneManager.GetActiveScene().name == "SampleScene")
        {
            SceneManager.LoadScene("HostOrJoinScene");
            return;
        }
        ToMainMenu();
    }
}
