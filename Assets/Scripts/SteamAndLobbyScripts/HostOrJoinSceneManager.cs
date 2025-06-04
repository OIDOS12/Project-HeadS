using UnityEngine;
using Steamworks;
using System.Collections.Generic;

/// <summary>
/// Manages the host or join lobby functionality.
/// </summary>
public class HostOrJoinSceneManager : MonoBehaviour
{
    public static HostOrJoinSceneManager Instance;

    [SerializeField] private SteamLobby steamLobby;
    public GameObject LobbiesMenu;
    public GameObject LobbyDataItemPrefab;
    public GameObject LobbiesListContent;

    public GameObject lobbiesButton, hostButton, MainMenuButton;
    public List<GameObject> ListOfLobbies = new List<GameObject>();

    /// <summary>
    /// Ensures that this is a singleton instance and initializes the SteamLobby reference.
    /// </summary>
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

    /// <summary>
    /// Displays the list of lobbies in the UI.
    /// </summary>
    /// <param name="lobbyIds"></param>
    /// <param name="result"></param>
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
            var entry = ListOfLobbies[i].GetComponent<LobbyData>();
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

                // Ensure the created item has a LobbyData component
                if (!createdItem.TryGetComponent<LobbyData>(out var entry))
                {
                    Debug.LogError("LobbyDataItemPrefab does not have a LobbyData component.");
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

    /// <summary>
    /// Initiates the process to get a list of lobbies.
    /// </summary>
    public void GetListOfLobbies()
    {
        lobbiesButton.SetActive(false);
        hostButton.SetActive(false);
        MainMenuButton.SetActive(false);

        LobbiesMenu.SetActive(true);
        steamLobby.GetLobbiesList();

    }

    /// <summary>
    /// Destroys all lobby entries in the UI and clears the list of lobbies.
    /// </summary>
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

    /// <summary>
    /// Handles the back button functionality to return to the previous menu.
    /// </summary>
    public void BackButton()
    {
        lobbiesButton.SetActive(true);
        hostButton.SetActive(true);
        MainMenuButton.SetActive(true);

        LobbiesMenu.SetActive(false);
    }

    /// <summary>
    /// Hosts a new lobby using the SteamLobby instance.
    /// </summary>
    public void HostLobby()
    {
        if (steamLobby != null)
            steamLobby.HostLobby();
        else
            Debug.LogError("SteamLobby reference missing in HostOrJoinSceneManager.");
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void ToMainMenu()
    {
        if (steamLobby != null)
            steamLobby.ToMainMenu();
        else
            Debug.LogError("SteamLobby reference missing in HostOrJoinSceneManager.");
    }
}