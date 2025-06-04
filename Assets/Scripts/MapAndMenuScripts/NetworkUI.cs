using UnityEngine;
using TMPro;
using Mirror;

/// <summary>
/// Handles displaying score, goal messages, and game-over UI in a networked game.
/// </summary>
public class NetworkGameUI : NetworkBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject confirmQuit;
    [SerializeField] private GameObject winnerTextGameObject;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI ScoreboardText;

    /// <summary>
    /// Checks for input to toggle the pause menu.
    /// </summary>
    void Update()
    {
        PauseMenu();
    }

    /// <summary>
    /// Updates the scoreboard with the latest goal counts.
    /// </summary>

    [ClientRpc]
    public void RpcUpdateScoreboardText(string text)
    {
        ScoreboardText.text = text;
    }

    /// <summary>
    /// Displays the winning message on game over.
    /// </summary>
    [ClientRpc]
    public void RpcShowWinner(string winner)
    {
        string[] names = BallAndPlayerMethods.Instance.GetPlayerNames();
        string winnerName = winner == "Player 1" ? names[0] : names[1];
        winnerText.text = $"Game Over! {winnerName} wins!\n Returning to menu in 5 seconds...";
        winnerTextGameObject.SetActive(true);
    }

    public void PauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            confirmQuit.SetActive(false);
        }
    }
    /// <summary>
    /// Called when the player presses the resume button.
    /// </summary>
    public void ResumeButton()
    {
        pauseMenu.SetActive(false);
        confirmQuit.SetActive(false);
    }

    /// <summary>
    /// Opens the quit confirmation dialog.
    /// </summary>
    public void MainMenuButton() => confirmQuit.SetActive(true);

    /// <summary>
    /// Leaves the lobby and ends the game session.
    /// </summary>
    public void QuitButton() => SteamLobby.Instance.LeaveLobby();

    /// <summary>
    /// Hides the quit confirmation dialog.
    /// </summary>
    public void DontQuitButton() => confirmQuit.SetActive(false);
}
