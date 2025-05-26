using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class LobbyDataEntry : MonoBehaviour
{
    public CSteamID LobbyID;
    public TMP_Text LobbyNameText;
    public string LobbyName;


    public void SetLobbyData()
    {
        LobbyNameText.text = LobbyName;
    }
    
    public void JoinLobbby()
    {
        SteamLobby.Instance.JoinLobby(LobbyID);
    }
}
