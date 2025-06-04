using Mirror;
using UnityEngine;

/// <summary>
/// Handles player respawn positions, ball management, and goal tracking.
/// </summary>
public class BallAndPlayerMethods : NetworkBehaviour
{
    public static BallAndPlayerMethods Instance;

    //Respawn positions for players
    // 0 - player1, 1 - player2
    private Vector2[] respawns;
    private GameObject[] players;

    Vector2 player1Pos = new(5.6F, -3.76F);
    Vector2 player2Pos = new(-5.6F, -3.76F);

    [SerializeField] private GameObject ball;
    private Rigidbody2D ballRb;
    [SerializeField] private Vector2 startBallPosition = new(0f, -1.27f);
    [SerializeField] private float forceMagnitudeForBallRelease = 100f;

    public int Player1Goals { get; private set; }
    public int Player2Goals { get; private set; }

    /// <summary>
    /// Ensures a single instance of BallAndPlayerMethods exists in the scene.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// Initializes respawn positions, retrieves players and ball, and sets players to their respawn positions.
    /// </summary>
    void Start()
    {
        respawns = new Vector2[]
        {
            player1Pos,
            player2Pos
        };

        GetPlayers(); // Get all players in the scene
        GetBall(); // Get the ball in the scene
        SetStandartPosition(); // Set the players to their respawn positions at the start
    }

    /// <summary>
    /// Retrieves all players in the scene with the "Player" tag.
    /// </summary>
    /// <returns>players</returns>
    public GameObject[] GetPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        return players;
    }

    /// <summary>
    /// Retrieves the ball in the scene with the "Ball" tag.
    /// </summary>
    /// <returns>ball</returns>
    public GameObject GetBall()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        return ball;
    }

    /// <summary>
    /// Retrieves the names of the players based on their PlayerObjectController components.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Sets the players to their standard respawn positions.
    /// </summary>
    public void SetStandartPosition()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = respawns[i];
        }
        Debug.Log("SetStandartPosition() called. Players respawned to their positions.");
    }

    /// <summary>
    /// Resets the ball's position and applies a force depending on the scoring player.
    /// </summary>
    public void ResetBall(string scoringPlayer)
    {
        ballRb = ball.GetComponent<Rigidbody2D>();
        ballRb.linearVelocity = Vector2.zero;
        ballRb.position = startBallPosition;

        if (scoringPlayer == "Player1") Player1Goals++;
        else if (scoringPlayer == "Player2") Player2Goals++;

        Vector2 force = (scoringPlayer == "Player1")
            ? Vector2.left * forceMagnitudeForBallRelease
            : Vector2.right * forceMagnitudeForBallRelease;

        ballRb.AddForce(force);
    }
}