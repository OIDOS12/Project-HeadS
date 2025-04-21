using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    HashSet<float> validRefreshRates = new HashSet<float>() {60f, 120f, 144f, 240f, 360f};
    
    public TMPro.TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    Resolution[] filtredResolutionsArray;
    public Toggle toggle;

    private string toggleFullscreenKey = "toggleFullscreenKey";

    void Awake()
    {
        resolutions = Screen.resolutions;
        var filtredResolutions = resolutions
            .Where(res => res.width >= 1024 && res.height >= 768) // Фільтруємо за розміром
            .GroupBy(res => new { res.width, res.height }) // Групуємо за розширенням
            .Select(group => group.OrderByDescending(res => res.refreshRateRatio).FirstOrDefault()); // Вибираємо найбільшу частоту для кожного розширення

        filtredResolutionsArray = filtredResolutions.ToArray();

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < filtredResolutionsArray.Length; i++)
        {
            string option = filtredResolutionsArray[i].width + " x " + filtredResolutionsArray[i].height;
            options.Add(option);

            if(filtredResolutionsArray[i].width == Screen.currentResolution.width
            && filtredResolutionsArray[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        toggle.isOn = PlayerPrefs.GetInt(toggleFullscreenKey, 0) == 1;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filtredResolutionsArray[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(toggleFullscreenKey, isFullscreen ? 1 : 0);
    }

    public void Save()
    {
        //User.Settings
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}