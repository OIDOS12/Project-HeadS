using UnityEngine;
using System.Collections;

public class LocalGoalDetector : MonoBehaviour
{
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string firstGoal = "FirstGoal";
    [SerializeField] private string secondGoal = "SecondGoal";

    private bool goalScored = false;
    private Vector2 ballStartPos;
    private Vector2[] playerRespawns;
    private GameObject[] players;
    [SerializeField] private LocalGameUI goalTextObjectManager; 
    private Rigidbody2D ballRb;
    private GameObject ball;
    private string whoScored = "";

    void Start()
    {
        // Find and cache references
        players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null || players.Length < 2)
        {
            Debug.LogError("Not enough players found with tag 'Player'.");
            return;
        }

        ball = GameObject.FindWithTag(ballTag);
        if (ball == null)
        {
            Debug.LogError("Ball not found with tag '" + ballTag + "'.");
            return;
        }
        ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb == null)
        {
            Debug.LogError("Ball Rigidbody2D not found.");
            return;
        }

        ballStartPos = ball.transform.position;
        playerRespawns = new Vector2[]
        {
            players[0].transform.position,
            players[1].transform.position
        };

        if (goalTextObjectManager == null)
            Debug.LogWarning("goalTextObjectManager is not assigned in the inspector.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (goalScored) return;
        if (!collision.CompareTag(ballTag)) return;

        ballRb = collision.GetComponent<Rigidbody2D>();
        if (ballRb == null) return;

        // Direction check: ensure ball is moving towards the goal
        if (gameObject.CompareTag(firstGoal) && ballRb.linearVelocity.x > 0.1f)
        {
            whoScored = "Player1";
        }
        else if (gameObject.CompareTag(secondGoal) && ballRb.linearVelocity.x < -0.1f)
        {
            whoScored = "Player2";
        }
        else
        {
            // Ball entered goal but not moving in expected direction, ignore
            return;
        }

        goalScored = true;
        StartCoroutine(GoalCoroutine());
    }

    private IEnumerator GoalCoroutine()
    {

        if (goalTextObjectManager != null)
        {
            goalTextObjectManager.PlayerScored(whoScored);
        }
        yield return new WaitForSeconds(2f);
        ResetPosition();
        goalScored = false;
    }

    private void ResetPosition()
    {
        if (ball == null || ballRb == null || players == null || playerRespawns == null) return;

        ball.transform.position = ballStartPos;
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;

        for (int i = 0; i < players.Length && i < playerRespawns.Length; i++)
        {
            players[i].transform.position = playerRespawns[i];
        }

        // Apply force only if whoScored is valid
        if (whoScored == "Player1")
            ballRb.AddForce(new Vector2(100f, 0f));
        else if (whoScored == "Player2")
            ballRb.AddForce(new Vector2(-100f, 0f));
    }
}