using UnityEngine;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Handles the lobby functionality, including player management and lobby state.
/// </summary>
public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;
    public TMP_Text lobbyNameText;

    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerObjectController;
    private CustomNetworkManager networkManager;

    [SerializeField] private Button StartGameButton;
    public TMP_Text ReadyButtonText;

    private CustomNetworkManager Manager
    {
        get
        {
            if (networkManager != null) { return networkManager; }
            return networkManager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    /// <summary>
    /// Ensures that this is a singleton instance and initializes the LocalPlayerObjectController reference.
    /// </summary>
    void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    /// <summary>
    /// Calls @ChangeReadyState on the LocalPlayerObjectController to toggle the ready state of the player.
    /// </summary>
    public void ReadyPlayer()
    {
        LocalPlayerObjectController.ChangeReadyState();
    }

    /// <summary>
    /// Updates the text of the Ready button based on the player's ready state.
    /// </summary>
    public void UpdateButton()
    {
        if (LocalPlayerObjectController.isReady)
        {
            ReadyButtonText.text = "Unready";
        }
        else
        {
            ReadyButtonText.text = "Ready";
        }
    }

    /// <summary>
    /// Checks if all players in the lobby are ready and updates the StartGameButton interactability accordingly.
    /// </summary>
    public void CheckIfAllReady()
    {
        if (Manager.PlayerList.All(player => player.isReady))
        {
            if (LocalPlayerObjectController.PlayerID == 1)
            {
                StartGameButton.interactable = true;
            }
            else
            {
                StartGameButton.interactable = false;
            }
        }
        else
        {
            StartGameButton.interactable = false;
        }
    }

    /// <summary>
    /// Updates the lobby name text based on the current lobby ID.
    /// </summary>
    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    /// <summary>
    /// Updates the player list by checking the current player list against the existing items.
    /// If the player item has not been created, it creates the host player item.
    /// If there are more players in the current list than in the existing items, it creates new client player items.
    /// If there are fewer players in the current list than in the existing items, it removes the excess player items.
    /// If the number of players matches, it updates the existing player items.
    /// </summary>
    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated) { CreateHostPlayerItem(); }
        if (playerListItems.Count < Manager.PlayerList.Count) { CreateClientPlayerItem(); }
        if (playerListItems.Count > Manager.PlayerList.Count) { RemovePlayerItem(); }
        if (playerListItems.Count == Manager.PlayerList.Count) { UpdatePlayerItem(); }
    }

    /// <summary>
    /// Finds the local player object and its controller.
    /// </summary>
    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerObjectController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    /// <summary>
    /// Creates player items for each player in the lobby when hosting.
    /// This method instantiates a new player item prefab for each player in the player list,
    /// sets the player name, connection ID, Steam ID, and ready state,
    /// and adds the item to the player list view content.
    /// </summary>
    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.PlayerList)
        {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab);
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.isReady = player.isReady;
            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            NewPlayerItemScript.transform.localScale = Vector3.one;

            playerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    /// <summary>
    /// Simmilary to @CreateHostPlayerItem, this method creates player items for each client player in the lobby.
    /// It checks if the player already exists in the player list items before creating a new item.
    /// </summary>
    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.PlayerList)
        {
            if (!playerListItems.Any(b => b.ConnectionID == player.ConnectionID))
            {
                GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab);
                PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

                NewPlayerItemScript.PlayerName = player.PlayerName;
                NewPlayerItemScript.ConnectionID = player.ConnectionID;
                NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
                NewPlayerItemScript.isReady = player.isReady;
                NewPlayerItemScript.SetPlayerValues();

                NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                NewPlayerItemScript.transform.localScale = Vector3.one;

                playerListItems.Add(NewPlayerItemScript);
            }
        }
    }

    /// <summary>
    /// Updates the player items in the lobby by iterating through the player list
    /// and updating the corresponding player list items with the current player information.
    /// </summary>
    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.PlayerList)
        {
            foreach (PlayerListItem PlayerListItemScript in playerListItems)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.isReady = player.isReady;
                    PlayerListItemScript.SetPlayerValues();
                    if (player == LocalPlayerObjectController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckIfAllReady();

    }

    /// <summary>
    /// Removes player items from the lobby that no longer exist in the current player list.
    /// It checks each player list item against the current player list and removes any items
    /// </summary>
    public void RemovePlayerItem()
    {
        List<PlayerListItem> itemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem PlayerListItem in playerListItems)
        {
            if (PlayerListItem == null) { continue; }
            if (!Manager.PlayerList.Any(b => b.ConnectionID == PlayerListItem.ConnectionID))
            {
                itemsToRemove.Add(PlayerListItem);
            }
        }
        if (itemsToRemove.Count > 0)
        {
            foreach (PlayerListItem PlayerListItemToRemove in itemsToRemove)
            {
                GameObject ObjectToRemove = PlayerListItemToRemove.gameObject;
                playerListItems.Remove(PlayerListItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }


    /// <summary>
    /// Starts the game by calling the LocalPlayerObjectController's CanStartGame method with the scene name.
    /// </summary>
    /// <param name="sceneName"></param>
    public void StartGame(string sceneName)
    {
        LocalPlayerObjectController.CanStartGame(sceneName);
    }

    /// <summary>
    /// Leaves the current Steam lobby.
    /// </summary>
    public void LeaveLobby()
    {
        SteamLobby.Instance.LeaveLobby();
    }
}