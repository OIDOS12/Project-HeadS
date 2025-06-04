using UnityEngine;


public class LocalBallAndPlayerMethods : MonoBehaviour
{
    public static LocalBallAndPlayerMethods Instance;

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

        GetPlayers(); // Get all players in the scene
        GetBall(); // Get the ball in the scene
        SetStandartPosition(); // Set the players to their respawn positions at the start
    }

    public GameObject[] GetPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        return players;
    }

    public GameObject GetBall()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        return ball;
    }

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