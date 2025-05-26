using UnityEngine;
using System.Collections; // Додаємо для IEnumerator
using Mirror;

public class GoalDetector : NetworkBehaviour
{
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string firstGoalTag = "FirstGoal";
    [SerializeField] private string secondGoalTag = "SecondGoal";
    [SerializeField] private UnityEngine.Events.UnityEvent OnGoalScored;
    [SerializeField] private NetworkGameUI goalTextObejctManager;

    // Затримки тепер в секундах для зручності з WaitForSeconds
    private const float BallResetDelaySeconds = 2f; // 2000ms = 2s
    private const float PlayerResetDelaySeconds = 0.1f; // 100ms = 0.1s

    private const float BallForceMagnitude = 100f; 
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
        else
        {
             ballRb = ballObject.GetComponent<Rigidbody2D>(); // Отримуємо Rigidbody2D м'яча на старті сервера
             if (ballRb == null)
             {
                 Debug.LogError("Ball object has no Rigidbody2D component.");
             }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Логіка виявлення голу має бути тільки на сервері
        if (!isServer) return;

        if (collision.CompareTag(ballTag) && !goalScored)
        {
            // Rigidbody2D м'яча тепер отримуємо в OnStartServer
            if (ballRb == null)
            {
                 Debug.LogError("Ball Rigidbody2D is not initialized.");
                 return;
            }

            string potentialScoringPlayer = null;
            if (ballRb.linearVelocity.x < 0 && gameObject.CompareTag(secondGoalTag))
            {
                potentialScoringPlayer = "Player1";
            }
            else if (ballRb.linearVelocity.x > 0 && gameObject.CompareTag(firstGoalTag))
            {
                potentialScoringPlayer = "Player2";
            }

            // Якщо це справді гол
            if (potentialScoringPlayer != null)
            {
                 scoringPlayer = potentialScoringPlayer;
                 goalScored = true; // Встановлюємо прапор, щоб уникнути повторних викликів

                 // Запускаємо корутину для обробки події голу та скидання стану
                 StartCoroutine(GoalSequence_Coroutine());
            }
        }
    }

    // Корутина, яка керує всією послідовністю після голу
    [Server] // Ця корутина виконується лише на сервері
    private IEnumerator GoalSequence_Coroutine()
    {
        OnGoalScored?.Invoke(); // Викликаємо UnityEvent (якщо потрібно на сервері)

        // Повідомляємо клієнтів про гол для оновлення UI та звуків
        RpcShowGoalMessage(scoringPlayer);

        // Чекаємо основну затримку перед скиданням
        yield return new WaitForSeconds(BallResetDelaySeconds);

        // Скидаємо позиції гравців (це RPC)
        RpcResetPlayerPositions();

        // Чекаємо коротку затримку перед скиданням м'яча (щоб гравці встигли стати на місце)
        yield return new WaitForSeconds(PlayerResetDelaySeconds);

        // Скидаємо позицію та швидкість м'яча (серверна логіка)
        ResetBallPosition();

        // Дозволяємо забивати наступний гол
        goalScored = false;
    }

    // RPC, який викликається на клієнтах для оновлення UI (виклик PlayerScored на клієнті)
    [ClientRpc]
    private void RpcShowGoalMessage(string player)
    {
        if (goalTextObejctManager != null)
        {
            goalTextObejctManager.PlayerScored(player);
        }
    }

    // RPC, який викликається на клієнтах для скидання позицій гравців
    [ClientRpc]
    private void RpcResetPlayerPositions()
    {
        if (PlayerMethods.Instance != null)
        {
             PlayerMethods.Instance.SetStandartPosition();
        }
        else
        {
             Debug.LogError("PlayerMethods.Instance is null on client.");
        }
    }

    // Серверний метод для скидання позиції та швидкості м'яча
    [Server]
    private void ResetBallPosition()
    {
        if (ballObject != null && ballRb != null)
        {
            ballObject.transform.position = ballStartPos;
            ballRb.linearVelocity = Vector2.zero; // Скидаємо швидкість

            // Надаємо м'ячу початковий імпульс в залежності від того, хто забив
            if (scoringPlayer == "Player1")
                ballRb.AddForce(new Vector2(BallForceMagnitude, 0f)); // Використовуйте ForceMode2D.Impulse для миттєвого поштовху
            else
                ballRb.AddForce(new Vector2(-BallForceMagnitude, 0f));
        }
    }
}
