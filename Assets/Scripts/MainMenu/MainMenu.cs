using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject selectMode;
    [SerializeField] private GameObject playButton;
    void Awake()
    {
        Time.timeScale = 1;
        selectMode.SetActive(false);
    }
    public void PlayGame()
    {
        selectMode.SetActive(true);
        playButton.SetActive(false);
    }

    public void SettingsScreen()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void LocalMode()
    {
        SceneManager.LoadScene("LocalScene");
    }

    public void MultiplayerMode()
    {
        SceneManager.LoadScene("HostOrJoinScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
