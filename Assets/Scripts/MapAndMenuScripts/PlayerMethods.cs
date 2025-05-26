using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMethods : NetworkBehaviour
{
    public static PlayerMethods Instance;

    //Respawn positions for players
    // 0 - player1, 1 - player2
    private Vector2[] respawns;
    private GameObject[] players;

    Vector2 player1Pos = new(6.3F, -3.7F);
    Vector2 player2Pos = new(-5.6F, -3.7F);

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        respawns = new Vector2[]
        {
            player1Pos,
            player2Pos
        };

        players = GetPlayers(); // Get all players in the scene
        SetStandartPosition(); // Set the players to their respawn positions at the start
    }

    public GameObject[] GetPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        return players;
    }

    public string[] GetPlayerNames()
    {
        string[] names = new string[2] { "Player 1", "Player 2" };
        var playerControllers = FindObjectsByType<PlayerObjectController>(FindObjectsSortMode.None);
        foreach (var player in playerControllers)
        {
            if (player.PlayerID == 1)
                names[0] = player.PlayerName;
            else if (player.PlayerID == 2)
                names[1] = player.PlayerName;
        }
        return names;
    }

    public void SetStandartPosition()
    {
        for (int i = 0; i < players.Length; i++)
        {
          players[i].transform.position = respawns[i];
        }
        Debug.Log("SetStandartPosition() called. Players respawned to their positions.");
    }
}