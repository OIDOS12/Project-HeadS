using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Mirror;
using System;

public class GameUI : NetworkBehaviour
{
    public TextMeshProUGUI ScoreText;
    [SyncVar]
    private int player1Goals = 0;
    [SyncVar]
    private int player2Goals = 0;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;
    private String score;

    void Awake()
    {

        PauseSetup();

    }
    void Start()
    {
        score = player1Goals.ToString() + "|" + player2Goals.ToString();
        UpdateScoreboard(score);
    }
    void Update()
    {
        PauseMenu();
    }

    public void Player1Scored()
    {
        player1Goals += 1;
        score = player1Goals.ToString() + "|" + player2Goals.ToString();
        UpdateScoreboard(score);
    }

    public void Player2Scored()
    {
        player2Goals += 1;
        score = player1Goals.ToString() + "|" + player2Goals.ToString();
        UpdateScoreboard(score);
    }

    private void UpdateScoreboard(string score)
    {
        ScoreText.text = score;
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