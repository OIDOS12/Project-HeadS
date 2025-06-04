using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenu class handles the main menu functionality of the game.
/// It allows players to start the game, access settings, choose game modes, and quit the game.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject selectMode;
    [SerializeField] private GameObject playButton;

    /// <summary>
    /// Initializes the main menu by setting the time scale to 1 and hiding the mode selection UI.
    /// </summary>
    void Awake()
    {
        Time.timeScale = 1;
        selectMode.SetActive(false);
    }

    /// <summary>
    /// Displaying the mode selection UI and hiding the play button.
    /// </summary>
    public void PlayGame()
    {
        selectMode.SetActive(true);
        playButton.SetActive(false);
    }

    /// <summary>
    /// Loads the settings screen scene.
    /// </summary>
    public void SettingsScreen() => SceneManager.LoadScene("SettingsMenu");
    
    /// <summary>
    /// Loads the local mode scene.
    /// </summary>
    public void LocalMode() => SceneManager.LoadScene("LocalScene");

    /// <summary>
    /// Loads the multiplayer mode scene.
    /// </summary>
    public void MultiplayerMode() => SceneManager.LoadScene("HostOrJoinScene");

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame() => Application.Quit();
}
