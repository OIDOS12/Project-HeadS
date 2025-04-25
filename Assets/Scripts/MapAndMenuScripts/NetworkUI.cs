using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Threading.Tasks;

public class NetworkGameUI : NetworkBehaviour
{
    public TextMeshProUGUI ScoreText;
    [SyncVar]
    private int player1Goals = 0;
    [SyncVar]
    private int player2Goals = 0;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;
    [SerializeField] private GameObject goalText;

    private string score;

    void Awake()
    {

        PauseSetup();
        goalText.SetActive(false);

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

    public void PlayerScored(string whoScored)
    {
        if (whoScored == "Player1")
        {
            player1Goals += 1;
        }
        else if (whoScored == "Player2")
        {
            player2Goals += 1;
        }

        SummonUpdateScoreboard(player1Goals, player2Goals);
    }

    public void SummonUpdateScoreboard(int player1Goals, int player2Goals)
    {
        score = player1Goals.ToString() + "|" + player2Goals.ToString();
        UpdateScoreboard(score);
        RpcUpdateScoreboard(score); // Update all clients
    }

    private void UpdateScoreboard(string score)
    {
        Debug.Log("regular update scoreboard");
        
    }

    [ClientRpc]
    private void RpcUpdateScoreboard(string score)
    {
        Debug.Log("RPC update scoreboard");
        ShowGoalText();
        ScoreText.text = score;
    }

    private async void ShowGoalText()
    {
        goalText.SetActive(true);
        await Task.Delay(2000);
        goalText.SetActive(false);
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
        SteamLobby.Instance.LeaveLobby();
    }
    public void NoButton()
    {
        confirmQuit.SetActive(false);
    }
    public void PauseSetup()
    {
        pauseMenu.SetActive(false);
        confirmQuit.SetActive(false);
    }
}

