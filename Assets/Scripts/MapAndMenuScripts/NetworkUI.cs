using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Collections; // Додаємо для Coroutines

public class NetworkGameUI : NetworkBehaviour
{
    public TextMeshProUGUI ScoreText;
    [SyncVar]
    private int player1Goals = 0;
    [SyncVar]
    private int player2Goals = 0;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;
    [SerializeField] private GameObject winnerTextGameObject;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private int goalLimit = 10;
    private string player1Name;
    private string player2Name;
    [SerializeField] private AudioClip refereeWhistle;
    private string score;
    private bool gameEnded = false;

    void Awake()
    {
        PauseSetup();
        winnerTextGameObject.SetActive(false);
    }

    void Start()
    {
        // Цей код виконується одразу на старті
        score = player1Goals.ToString() + "|" + player2Goals.ToString();
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

    public void PlayerScored(string whoScored)
    {
        if(gameEnded){ return; }

        if (whoScored == "Player1"){ player1Goals += 1;}
        else if (whoScored == "Player2"){ player2Goals += 1;}

        // Оновлюємо рахунок на всіх клієнтах через RPC
        SummonUpdateScoreboard(player1Goals, player2Goals);

        // Перевіряємо умову перемоги тільки на сервері
        if (isServer && (player1Goals >= goalLimit || player2Goals >= goalLimit))
        {
            // Викликаємо RPC для завершення гри на всіх клієнтах
            RpcGameOver(player1Goals >= goalLimit ? "Player 1" : "Player 2");
            return; // Завершуємо виконання серверної логіки після перемоги
        }
    }

    [Server]
    public void SummonUpdateScoreboard(int player1Goals, int player2Goals)
    {
        // На сервері оновлюємо SyncVar player1Goals та player2Goals. Mirror синхронізує їх.
        // Рядок score теж можна оновлювати, але для RPC потрібен актуальний стан.
        // score = player2Goals.ToString() + "|" + player1Goals.ToString(); // Можливо, тут помилка? Порядок гравців?
        // Викликаємо RPC для візуального оновлення на клієнтах
        RpcUpdateScoreboard(player1Goals, player2Goals); // Передаємо актуальні бали
    }

    [ClientRpc]
    private void RpcUpdateScoreboard(int p1Goals, int p2Goals)
    {
        // Запускаємо корутину для відображення тексту "Goal!" та відтворення звуку
        StartCoroutine(PlayGoalEffects_Coroutine(p1Goals, p2Goals));
    }

    // Корутина для відображення тексту "Goal!" та відтворення свистка після затримки
    private IEnumerator PlayGoalEffects_Coroutine(int p1Goals, int p2Goals)
    {
        // winnerText.text = "Goal!"; // Можливо, тут треба текст "Goal!"?
        
        ScoreText.text = "Goal!"; 

        yield return new WaitForSeconds(2f); // Чекаємо 2 секунди

        ScoreText.text = p1Goals.ToString() + "|" + p2Goals.ToString();

        // Відтворюємо свисток після затримки
        if (SoundFXManager.instance != null && refereeWhistle != null)
        {
             SoundFXManager.instance.PlaySoundFX(refereeWhistle, transform);
        }
    }


    // RPC, який викликається на клієнтах, коли гра закінчується
    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        // Запускаємо корутину для відображення тексту про переможця та завершення гри
        StartCoroutine(EndGame_Coroutine(winner));
    }

    // Корутина для відображення тексту про переможця та очікування перед виходом
    private IEnumerator EndGame_Coroutine(string winner)
    {
        string[] names = PlayerMethods.Instance.GetPlayerNames();
        Debug.Log($"Player1Name: {names[0]}, Player2Name: {names[1]}");

        player1Name = names[0];
        player2Name = names[1];

        string winnerName = winner == "Player 1" ? player1Name : player2Name;
        winnerText.text = "Game Over! " + winnerName + " wins!";
        winnerTextGameObject.SetActive(true); // Показуємо текст "Game Over"

        gameEnded = true; // Встановлюємо прапор, що гра закінчена
        yield return new WaitForSeconds(10f); // Чекаємо 10 секунд

        // Після очікування викликаємо кнопку виходу
        QuitButton();
    }

    // --- Методи паузи та виходу залишаються без змін ---

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
        // Важливо: Логіка виходу з лобі повинна бути реалізована правильно для клієнта/сервера
        // Якщо це клієнт, він має відключитися. Якщо це сервер, він має зупинити хост.

        if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
        {
            // Код для виходу, коли ви є Хостом (зупинити сервер і відключити клієнта)
            SteamLobby.Instance.LeaveLobby(); 
        }
        // Виправлення: використовувати ClientOnly для режиму клієнта
        else if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
        {
            // Код для виходу, коли ви є тільки Клієнтом (відключитися від сервера)
            SteamLobby.Instance.LeaveLobby(); 
        }
    }

    public void DontQuitButton()
    {
        confirmQuit.SetActive(false);
    }

    public void PauseSetup()
    {
        pauseMenu.SetActive(false);
        confirmQuit.SetActive(false);
    }
}