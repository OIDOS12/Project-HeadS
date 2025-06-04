using UnityEngine;
using TMPro;
using Steamworks;

/// <summary>
/// Represents an entry in the lobby data list, displaying lobby information and allowing players to join.
/// </summary>
public class LobbyData : MonoBehaviour
{
    public CSteamID LobbyID;
    public TMP_Text LobbyNameText;
    public string LobbyName;

    /// <summary>
    /// Sets the lobby data for display in the UI.
    /// </summary>
    public void SetLobbyData()
    {
        LobbyNameText.text = LobbyName;
    }

    /// <summary>
    /// Joins the lobby associated with this entry.
    /// </summary>
    public void JoinLobbby()
    {
        SteamLobby.Instance.JoinLobby(LobbyID);
    }
}
