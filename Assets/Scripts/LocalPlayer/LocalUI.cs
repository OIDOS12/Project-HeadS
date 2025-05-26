using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Додаємо для Coroutines

public class LocalGameUI : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    private int player1Goals = 0; // Більше не SyncVar
    private int player2Goals = 0; // Більше не SyncVar
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;
    [SerializeField] private GameObject winnerTextGameObject;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private int goalLimit = 2;
    private string player1Name;
    private string player2Name;
    [SerializeField] private AudioClip refereeWhistle;
    private bool gameEnded = false;

    void Awake()
    {
        PauseSetup();
        winnerTextGameObject.SetActive(false);
    }

    void Start()
    {
        // Відтворюємо свисток одразу на старті гри
        if (SoundFXManager.instance != null && refereeWhistle != null)
        {
            SoundFXManager.instance.PlaySoundFX(refereeWhistle, transform);
        }
    }

    void Update()
    {
        PauseMenu();
    }

    // Цей метод викликається, коли гравець забиває гол
    public void PlayerScored(string whoScored)
    {
        if (gameEnded) { return; }

        if (whoScored == "Player1")
        {
            player1Goals += 1;
        }
        else if (whoScored == "Player2")
        {
            player2Goals += 1;
        }

        // Оновлюємо рахунок безпосередньо, оскільки немає мережі
        UpdateScoreboard(player1Goals, player2Goals);

        // Перевіряємо умову перемоги
        if (player1Goals >= goalLimit || player2Goals >= goalLimit)
        {
            GameOver(player1Goals >= goalLimit ? "Player 1" : "Player 2");
            return;
        }
    }

    // Метод для оновлення табло
    private void UpdateScoreboard(int p1Goals, int p2Goals)
    {
        // Запускаємо корутину для відображення тексту "Goal!" та відтворення звуку
        StartCoroutine(PlayGoalEffects_Coroutine(p1Goals, p2Goals));
    }

    // Корутина для відображення тексту "Goal!" та відтворення свистка після затримки
    private IEnumerator PlayGoalEffects_Coroutine(int p1Goals, int p2Goals)
    {
        ScoreText.text = "Goal!"; // Встановлюємо текст "Goal!"

        yield return new WaitForSeconds(2f); // Чекаємо 2 секунди
        ScoreText.text = p1Goals.ToString() + "|" + p2Goals.ToString();

        if (SoundFXManager.instance != null && refereeWhistle != null)
        {
            SoundFXManager.instance.PlaySoundFX(refereeWhistle, transform);
        }
    }

    // Метод, який викликається, коли гра закінчується
    private void GameOver(string winner)
    {
        // Запускаємо корутину для відображення тексту про переможця та завершення гри
        StartCoroutine(EndGame_Coroutine(winner));
    }

    // Корутина для відображення тексту про переможця та очікування перед виходом
    private IEnumerator EndGame_Coroutine(string winner)
    {
        player1Name = "Player 1";
        player2Name = "Player 2";

        string winnerName = winner == "Player 1" ? player1Name : player2Name;
        winnerText.text = "Game Over! " + winnerName + " wins!";
        winnerTextGameObject.SetActive(true); // Показуємо текст "Game Over"

        gameEnded = true; // Встановлюємо прапор, що гра закінчена
        yield return new WaitForSeconds(10f); // Чекаємо 10 секунд

        // Після очікування викликаємо кнопку виходу
        QuitButton();
    }

    // --- Методи паузи та виходу ---

    private void PauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            {
                PauseSetup();
            }
            else
            {
                Time.timeScale = 0; // Зупиняємо гру
                pauseMenu.SetActive(true);
            }
        }
    }

    public void ResumeButton()
    {
        PauseSetup();
    }

    public void MainMenuButton()
    {
        confirmQuit.SetActive(true);
    }

    public void QuitButton()
    {
        // Для офлайн-сцени просто завантажуємо головне меню або виходимо з програми
        SceneManager.LoadScene("MainMenu"); // Замініть "MainMenu" на назву вашої сцени головного меню
        // Або, якщо потрібно вийти з програми: Application.Quit();
    }

    public void DontQuitButton()
    {
        confirmQuit.SetActive(false);
    }

    public void PauseSetup()
    {
        Time.timeScale = 1; 
        pauseMenu.SetActive(false);
        confirmQuit.SetActive(false);
    }
}