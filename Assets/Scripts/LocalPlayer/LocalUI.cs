using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LocalGameUI : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    private int player1Goals = 0;
    private int player2Goals = 0;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;


    void Awake()
    {

        UpdateScoreboard();
        PauseSetup();

    }
    void Update()
    {
        PauseMenu();
    }

    public void Player1Scored()
    {
        player1Goals += 1;
        UpdateScoreboard();
    }


    public void Player2Scored()
    {
        player2Goals += 1;
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        ScoreText.text = player1Goals.ToString() + "|" + player2Goals.ToString();
    }


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
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
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
        SceneManager.LoadScene("MainMenu");
    }
    public void NoButton()
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