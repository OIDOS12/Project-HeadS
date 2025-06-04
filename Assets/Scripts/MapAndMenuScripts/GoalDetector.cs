using UnityEngine;
using Mirror;

/// <summary>
/// Detects when a goal has been scored and triggers the goal sequence via the GoalManager.
/// </summary>
public class GoalDetector : NetworkBehaviour
{
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string firstGoalTag = "FirstGoal";
    [SerializeField] private string secondGoalTag = "SecondGoal";
    [SerializeField] private GoalManager goalManager;

    private bool goalScored = false;
    private Rigidbody2D ballRb;

    /// <summary>
    /// Initializes references to the ball's Rigidbody2D on the server.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        var ball = BallAndPlayerMethods.Instance.GetBall();
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
        if (!isServer || goalScored || !collision.CompareTag(ballTag)) return;

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