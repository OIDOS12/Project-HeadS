using UnityEngine;
using System.Threading.Tasks;
using Mirror;

public class GoalDetector : NetworkBehaviour
{
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string firstGoalTag = "FirstGoal";
    [SerializeField] private string secondGoalTag = "SecondGoal";
    [SerializeField] private UnityEngine.Events.UnityEvent OnGoalScored;
    [SerializeField] private NetworkGameUI goalTextObejctManager;

    private const float BallResetDelay = 2000f; // Delay in milliseconds before resetting the ball
    private const float BallForceMagnitude = 100f;
    private const float BallResetXForce = 100f;
    private Vector2 ballStartPos = new(0f, -1.27f);

    private bool goalScored = false;
    private string scoringPlayer;
    private GameObject ballObject;
    private Rigidbody2D ballRb;

    public override void OnStartServer()
    {
        base.OnStartServer();
        ballObject = GameObject.FindWithTag(ballTag);
        if (ballObject == null)
        {
            Debug.LogError("Ball not found. Ensure the ball has the correct tag.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer) return;

        if (collision.CompareTag(ballTag) && !goalScored)
        {
            ballRb = collision.GetComponent<Rigidbody2D>();
            if (ballRb == null)
            {
                Debug.LogError("Ball has no Rigidbody2D component.");
                return;
            }

            if (ballRb.linearVelocity.x > 0 && gameObject.CompareTag(firstGoalTag))
            {
                scoringPlayer = "Player1";
            }
            else if (ballRb.linearVelocity.x < 0 && gameObject.CompareTag(secondGoalTag))
            {
                scoringPlayer = "Player2";
            }
            else
            {
                return; // Exit if no goal is scored
            }

            Goal();
            goalScored = true;
        }
    }

    [Server]
    private async void Goal()
    {
        OnGoalScored?.Invoke();
        RpcShowGoalMessage(scoringPlayer);
        await Task.Delay((int)BallResetDelay);
        ResetBallAndPlayers();
        goalScored = false;
    }

    [ClientRpc]
    private void RpcShowGoalMessage(string player)
    {
        if (goalTextObejctManager != null)
        {
            goalTextObejctManager.PlayerScored(player);
        }
    }

    [Server]
    private async void ResetBallAndPlayers()
    {
        PlayerMethods.Instance.SetStandartPosition();
        await Task.Delay(100);
        ResetBallPosition();
    }

    [Server]
    private void ResetBallPosition()
    {
        if (ballObject != null)
        {
            ballObject.transform.position = ballStartPos;
            ballRb.linearVelocity = Vector2.zero; // Reset velocity

            if (scoringPlayer == "Player1")
                ballRb.AddForce(new Vector2(BallResetXForce, 0f));
            else
                ballRb.AddForce(new Vector2(-BallResetXForce, 0f));

            // Optionally, sync the ball's new state to clients
            RpcSyncBallState(ballObject.transform.position, ballRb.linearVelocity);
        }
    }

    [ClientRpc]
    private void RpcSyncBallState(Vector2 position, Vector2 velocity)
    {
        if (ballObject != null)
        {
            ballObject.transform.position = position;
            ballRb.linearVelocity = velocity;
        }
    }
}
