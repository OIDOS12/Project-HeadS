using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

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

    void Awake()
    {
        if(Instance == null) { Instance = this; }
    }

    public void ReadyPlayer()
    {
        LocalPlayerObjectController.ChangeReadyState();
    }

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

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated) { CreateHostPlayerItem(); }
        if (playerListItems.Count < Manager.PlayerList.Count) { CreateClientPlayerItem(); }
        if (playerListItems.Count > Manager.PlayerList.Count) { RemovePlayerItem(); }
        if (playerListItems.Count == Manager.PlayerList.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerObjectController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

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

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.PlayerList)
        {
            if(!playerListItems.Any(b => b.ConnectionID == player.ConnectionID))
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

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.PlayerList)
        {
            foreach (PlayerListItem PlayerListItemScript in playerListItems)
            {
                if(PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.isReady = player.isReady;
                    PlayerListItemScript.SetPlayerValues();
                    if(player == LocalPlayerObjectController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckIfAllReady();

    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> itemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem PlayerListItem in playerListItems)
        {
            if (!Manager.PlayerList.Any(b => b.ConnectionID == PlayerListItem.ConnectionID))
            {
                itemsToRemove.Add(PlayerListItem);
            }
        }
        if(itemsToRemove.Count > 0)
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
    
    public void StartGame(string sceneName)
    {
        LocalPlayerObjectController.CanStartGame(sceneName);
    }

    public void LeaveLobby()
    {
        SteamLobby.Instance.LeaveLobby();
    }
}