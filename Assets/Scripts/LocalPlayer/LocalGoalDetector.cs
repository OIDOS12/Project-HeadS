using UnityEngine;
using System.Threading.Tasks;

public class LocalGoalDetector : MonoBehaviour
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
    [SerializeField] private LocalGameUI goalTextObejctManager;
    private Rigidbody2D ballRb;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        ballPos = GameObject.FindWithTag("Ball").transform.position;

        respawns = new Vector2[]
        {
            new Vector2(players[0].transform.position.x, players[0].transform.position.y),
            new Vector2(players[1].transform.position.x, players[1].transform.position.y)
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
                goalTextObejctManager?.Player1Scored();

            }
            else if (ballRb != null && ballRb.linearVelocity.x < 0 && gameObject.CompareTag(secondGoal))
            {
                whoScored = "Player2";
                goalTextObejctManager?.Player2Scored();
            }
            Goal();
            goalScored = true;
        }
    }


    private void ResetPosition()
    {
        GameObject.FindWithTag("Ball").transform.position = ballPos;
        ballRb.linearVelocity = Vector2.zero;
        for (int i = 0; i < players.Length; i++)
        {
          players[i].transform.position = respawns[i];
        }

        if (whoScored == "Player1")
            ballRb.AddForce(new Vector2(100f, 0f));
        else
            ballRb.AddForce(new Vector2(-100f, 0f));

    }
    
    private async void Goal()
    {
        OnGoalScored?.Invoke();
//        StartCoroutine(ResetGoalScored());
        await Task.Delay(2000);
        ResetPosition();
        goalScored = false;
     }
}