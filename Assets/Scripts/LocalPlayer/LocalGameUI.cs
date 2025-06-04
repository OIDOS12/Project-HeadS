using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles displaying score, goal messages, and game-over UI in a networked game.
/// </summary>
public class LocalGameUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;
    [SerializeField] private GameObject winnerTextGameObject;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI ScoreboardText; 

    void Start()
    {
        // Initialize UI elements if needed
        winnerTextGameObject.SetActive(false);
        PauseSetup();
    }

    /// <summary>
    /// Called once per frame.
    /// Checks for input to toggle the pause menu.
    /// </summary>
    void Update()
    {
        PauseMenu();
    }

    /// <summary>
    /// Updates the scoreboard with the latest goal counts.
    /// </summary>


    public void UpdateScoreboardText(string text)
    {
        ScoreboardText.text = text;
    }

    /// <summary>
    /// Displays the winning message on game over.
    /// </summary>
    public void ShowWinner(string winner)
    {
        string winnerName = winner == "Player 1" ? "Player1" : "Player2";
        winnerText.text = $"Game Over! {winnerName} wins!\n Returning to menu in 5 seconds...";
        winnerTextGameObject.SetActive(true);
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