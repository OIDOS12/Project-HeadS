using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// MusicManager class handles the music functionality of the game.
/// It manages the music volume, stops music when entering the game scene, and resumes it in other scenes.
/// </summary>
public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    [SerializeField] private AudioMixer mixer;
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundFXVolumeKey = "SoundFXVolume";

    /// <summary>
    /// Singleton instance of MusicManager to ensure only one instance exists.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load events
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Initializes the music volume settings based on player preferences.
    /// </summary>
    void Start()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f); // Default volume to 75% if not set
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f); 
        float soundFXVolume = PlayerPrefs.GetFloat(SoundFXVolumeKey, 0.75f); 

        mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20); // Convert linear volume to decibels
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20); 
        mixer.SetFloat("SoundFXVolume", Mathf.Log10(soundFXVolume) * 20); 
    }

    /// <summary>
    /// Handles scene loading events to manage music playback.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "OnlineGameScene")
        {
            GetComponent<AudioSource>().Stop();
        }

        else
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            }
        }
    }
}