using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Mirror;
public class GoalDetector : NetworkBehaviour
{
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string firstGoal = "FirstGoal";
    [SerializeField] private string secondGoal = "SecondGoal";
    [SerializeField] private string whoScored = "";
    [SerializeField] private UnityEngine.Events.UnityEvent OnGoalScored;

    private bool goalScored = false;
    private Vector2 ballPos;
    private Vector2[] respawns;
    [SerializeField] private GameObject[] players;
    [SerializeField] private NetworkGameUI goalTextObejctManager;
    private Rigidbody2D ballRb;
    [SerializeField] PlayerMethods playerMethods;

    void Start()
    {
        players = playerMethods.GetPlayers();
        ballPos = GameObject.FindWithTag("Ball").transform.position;

        Vector2 player1Pos = new Vector2(6.3F, -3.7F);
        Vector2 player2Pos = new Vector2(-5.6F, -3.7F);
        
        respawns = new Vector2[]
        {
            player1Pos,
            player2Pos
        };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ballTag) && !goalScored)
        {
            ballRb = collision.GetComponent<Rigidbody2D>();
            if (ballRb != null && ballRb.linearVelocity.x > 0 && gameObject.CompareTag(firstGoal)) // Перевірка напрямку (для 2D гри з горизонтальними воротами)
            {
                whoScored = "Player1";

            }
            else if (ballRb != null && ballRb.linearVelocity.x < 0 && gameObject.CompareTag(secondGoal))
            {
                whoScored = "Player2";
                
            }
            Goal();
            goalScored = true;
        }
    }

    private void ResetPosition()
    {
        GameObject.FindWithTag("Ball").transform.position = ballPos;
        ballRb.linearVelocity = Vector2.zero;
        playerMethods.SetStandartPosition();

        if (whoScored == "Player1")
            ballRb.AddForce(new Vector2(100f, 0f));
        else
            ballRb.AddForce(new Vector2(-100f, 0f));

    }
    private async void Goal()
    {
        OnGoalScored?.Invoke();
//        StartCoroutine(ResetGoalScored());
        if (whoScored == "Player1") { goalTextObejctManager?.PlayerScored("Player1"); }
        if (whoScored == "Player2") { goalTextObejctManager?.PlayerScored("Player2"); }
        await Task.Delay(2000);
        ResetPosition();
        goalScored = false;
     }
}
