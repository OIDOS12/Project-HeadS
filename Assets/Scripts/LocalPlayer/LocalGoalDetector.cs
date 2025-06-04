using UnityEngine;

/// <summary>
/// Detects when a goal has been scored and triggers the goal sequence via the GoalManager.
/// </summary>
public class LocalGoalDetector : MonoBehaviour
{
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string firstGoalTag = "FirstGoal";
    [SerializeField] private string secondGoalTag = "SecondGoal";
    [SerializeField] private LocalGoalManager goalManager;

    private bool goalScored = false;
    private Rigidbody2D ballRb;

    /// <summary>
    /// Initializes references to the ball's Rigidbody2D on the server.
    /// </summary>
    public void Start()
    {
        var ball = GameObject.FindWithTag(ballTag);
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Triggered when the ball enters the goal. Notifies GoalManager.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (goalScored || !collision.CompareTag(ballTag)) return;

        string scoringPlayer = null;
        if (ballRb.linearVelocity.x < 0 && gameObject.CompareTag(secondGoalTag))
            scoringPlayer = "Player1";
        else if (ballRb.linearVelocity.x > 0 && gameObject.CompareTag(firstGoalTag))
            scoringPlayer = "Player2";

        if (scoringPlayer != null)
        {
            goalScored = true;
            goalManager.HandleGoal(scoringPlayer, () => goalScored = false);
        }
    }
}
