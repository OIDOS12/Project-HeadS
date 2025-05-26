using UnityEngine;
using Steamworks;
using System.Collections.Generic;

public class HostOrJoinSceneManager : MonoBehaviour
{
    public static HostOrJoinSceneManager Instance;

    [SerializeField] private SteamLobby steamLobby;
    public GameObject LobbiesMenu;
    public GameObject LobbyDataItemPrefab;
    public GameObject LobbiesListContent;

    public GameObject lobbiesButton, hostButton, MainMenuButton;
    public List<GameObject> ListOfLobbies = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (steamLobby == null)
        {
            steamLobby = FindFirstObjectByType<SteamLobby>();
            if (steamLobby == null)
            {
                Debug.LogError("SteamLobby reference not set in HostOrJoinSceneManager.");
            }
        }
    }

    public void DisplayLobbies(List<CSteamID> lobbyIds, LobbyDataUpdate_t result)
    {
        if (LobbyDataItemPrefab == null || LobbiesListContent == null)
        {
            Debug.LogError("LobbyDataItemPrefab or LobbiesListContent is not assigned.");
            return;
        }

        // Remove any existing entry with the same ID
        for (int i = ListOfLobbies.Count - 1; i >= 0; i--)
        {
            var entry = ListOfLobbies[i].GetComponent<LobbyDataEntry>();
            if (entry != null && entry.LobbyID.m_SteamID == result.m_ulSteamIDLobby)
            {
                Destroy(ListOfLobbies[i]);
                ListOfLobbies.RemoveAt(i);
            }
        }

        // Only add if the lobby is in the list and has a valid name
        for (int i = 0; i < lobbyIds.Count; i++)
        {
            if (lobbyIds[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData(lobbyIds[i], "name");
                if (string.IsNullOrEmpty(lobbyName))
                    continue;

                GameObject createdItem = Instantiate(LobbyDataItemPrefab, LobbiesListContent.transform);
                var entry = createdItem.GetComponent<LobbyDataEntry>();
                if (entry == null)
                {
                    Debug.LogError("LobbyDataItemPrefab does not have a LobbyDataEntry component.");
                    Destroy(createdItem);
                    continue;
                }
                entry.LobbyID = lobbyIds[i];
                entry.LobbyName = lobbyName;
                entry.SetLobbyData();

                createdItem.transform.localScale = Vector3.one;
                ListOfLobbies.Add(createdItem);
            }
        }
    }

    public void GetListOfLobbies()
    {
        lobbiesButton?.SetActive(false);
        hostButton?.SetActive(false);
        MainMenuButton?.SetActive(false);

        LobbiesMenu?.SetActive(true);
        if (steamLobby != null)
            steamLobby.GetLobbiesList();
        else
            Debug.LogError("SteamLobby reference missing in HostOrJoinSceneManager.");
    }

    public void DestroyLobbies()
    {
        foreach (GameObject lobby in ListOfLobbies)
        {
            if (lobby != null)
            {
                Destroy(lobby);
            }
        }
        ListOfLobbies.Clear();
    }

    public void BackButton()
    {
        lobbiesButton?.SetActive(true);
        hostButton?.SetActive(true);
        MainMenuButton?.SetActive(true);

        LobbiesMenu?.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && steamLobby != null)
        {
            steamLobby.ToMainMenu();
        }
    }

    public void HostLobby()
    {
        if (steamLobby != null)
            steamLobby.HostLobby();
        else
            Debug.LogError("SteamLobby reference missing in HostOrJoinSceneManager.");
    }

    public void ToMainMenu()
    {
        if (steamLobby != null)
            steamLobby.ToMainMenu();
        else
            Debug.LogError("SteamLobby reference missing in HostOrJoinSceneManager.");
    }
}