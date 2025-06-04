using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// SoundMixerManager class handles the sound volume settings in the game.
/// </summary>
public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioClip testSoundFX;

    [SerializeField] private GameObject MasterVolumeSlider;
    [SerializeField] private GameObject MusicVolumeSlider;
    [SerializeField] private GameObject SoundFXVolumeSlider;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundFXVolumeKey = "SoundFXVolume";

    /// <summary>
    /// Loads the volume settings from PlayerPrefs on start.
    /// </summary>
    void Start()
    {
        LoadSliders();
    }

    /// <summary>
    /// Sets the master volume.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20); // Convert linear volume to decibels
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    /// <summary>
    /// Sets the music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20); // Convert linear volume to decibels
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    /// <summary>
    /// Sets the sound effects volume.
    /// </summary>
    public void SetSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume) * 20); // Convert linear volume to decibels
        PlayerPrefs.SetFloat(SoundFXVolumeKey, volume);
        SoundFXManager.Instance.PlaySoundFX(testSoundFX, transform, volume); // Play test sound effect
    }

    /// <summary>
    /// Loads the volume settings from PlayerPrefs and sets the sliders accordingly.
    /// </summary>
    public void LoadSliders()
    {
        MasterVolumeSlider.GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        MusicVolumeSlider.GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        SoundFXVolumeSlider.GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat(SoundFXVolumeKey, 0.75f);
    }
}