using UnityEngine;
using UnityEngine.UI;

public class PlayerMethods : MonoBehaviour
{
    //Respawn positions for players
    // 0 - player1, 1 - player2
    private Vector2[] respawns;
    Vector2 player1Pos = new(6.3F, -3.7F);
    Vector2 player2Pos = new(-5.6F, -3.7F);

    void Start()
    {
        respawns = new Vector2[]
        {
            player1Pos,
            player2Pos
        };
    }

    public GameObject[] GetPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        return players;
    }

    public void SetStandartPosition()
    {
        GameObject[] players = GetPlayers(); // Get all players in the scene
        for (int i = 0; i < players.Length; i++)
        {
          players[i].transform.position = respawns[i];
        }
        Debug.Log("SetStandartPosition() called. Players respawned to their positions.");
    }
}