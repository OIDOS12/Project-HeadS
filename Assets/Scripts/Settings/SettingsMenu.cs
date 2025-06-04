using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// SettingsMenu class handles the settings menu functionality of the game.
/// It allows players to change the screen resolution and toggle fullscreen mode.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    public TMPro.TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    Resolution[] filtredResolutionsArray;
    public Toggle toggle;

    private string toggleFullscreenKey = "toggleFullscreenKey";

    /// <summary>
    /// Initializes the settings menu by filtering available resolutions and setting up the dropdown options.
    /// It also sets the current resolution and fullscreen toggle based on player preferences.
    /// </summary>
    void Awake()
    {
        resolutions = Screen.resolutions;
        var filtredResolutions = resolutions
            .Where(res => res.width >= 1024 && res.height >= 768)
            .GroupBy(res => new { res.width, res.height })
            .Select(group => group.OrderByDescending(res => res.refreshRateRatio).FirstOrDefault());

        filtredResolutionsArray = filtredResolutions.ToArray();

        resolutionDropdown.ClearOptions();
        List<string> options = new();

        int currentResolutionIndex = 0;
        for (int i = 0; i < filtredResolutionsArray.Length; i++)
        {
            string option = filtredResolutionsArray[i].width + " x " + filtredResolutionsArray[i].height;
            options.Add(option);

            if (filtredResolutionsArray[i].width == Screen.currentResolution.width
            && filtredResolutionsArray[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        toggle.isOn = PlayerPrefs.GetInt(toggleFullscreenKey, 0) == 1;
    }

    /// <summary>
    /// Sets the resolution based on the selected index from the dropdown.
    /// </summary>
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filtredResolutionsArray[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
    }

    /// <summary>
    /// Toggles fullscreen mode based on the toggle state.
    /// </summary>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(toggleFullscreenKey, isFullscreen ? 1 : 0);
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    public void BackToMenu() => SceneManager.LoadScene("MainMenu");
}