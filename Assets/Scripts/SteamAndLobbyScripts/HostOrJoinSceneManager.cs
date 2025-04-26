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
        if (Instance == null) { Instance = this; }
        steamLobby = FindFirstObjectByType<SteamLobby>();
    }
    
    public void DestroyLobbies()
    {
        foreach (GameObject lobby in ListOfLobbies)
        {
            Destroy(lobby);
        }
        ListOfLobbies.Clear();
    }

    public void DisplayLobbies(List<CSteamID> lobbyIds, LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbyIds.Count; i++)
        {
            if(lobbyIds[i].m_SteamID == result.m_ulSteamIDLobby) 
            { 
                GameObject createdItem = Instantiate(LobbyDataItemPrefab);
                
                createdItem.GetComponent<LobbyDataEntry>().LobbyID = (CSteamID)lobbyIds[i].m_SteamID;
                createdItem.GetComponent<LobbyDataEntry>().LobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIds[i].m_SteamID, "name");
                createdItem.GetComponent<LobbyDataEntry>().SetLobbyData();

                createdItem.transform.SetParent(LobbiesListContent.transform);
                createdItem.transform.localScale = Vector3.one;
                
                foreach (GameObject lobbie in ListOfLobbies)
                {
                    if (lobbie.GetComponent<LobbyDataEntry>().LobbyID == createdItem.GetComponent<LobbyDataEntry>().LobbyID)
                    {
                        Destroy(createdItem);
                        ListOfLobbies.Remove(lobbie);
                        break;
                    }
                }
                ListOfLobbies.Add(createdItem);

            }
        }
    }

    public void GetListOfLobbies()
    {
        lobbiesButton.SetActive(false);
        hostButton.SetActive(false);
        MainMenuButton.SetActive(false);

        LobbiesMenu.SetActive(true);

        SteamLobby.Instance.GetLobbiesList();
    }

    public void BackButton()
    {
        lobbiesButton.SetActive(true);
        hostButton.SetActive(true);
        MainMenuButton.SetActive(true);

        LobbiesMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            steamLobby.ToMainMenu();
        }
    }

    public void HostLobby()
    {
        steamLobby.HostLobby();
    }
    public void ToMainMenu()
    {
        steamLobby.ToMainMenu();
    }
}