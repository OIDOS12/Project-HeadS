using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{   
    private static MusicManager instance;
    [SerializeField] private AudioMixer mixer; // Reference to the AudioMixer
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundFXVolumeKey = "SoundFXVolume";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load events
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
    }

    void Start()
    {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f); // Default to 75% if not set
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f); // Default to 75% if not set
            float soundFXVolume = PlayerPrefs.GetFloat(SoundFXVolumeKey, 0.75f); // Default to 75% if not set

            mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20); // Convert to decibels
            mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20); // Convert to decibels
            mixer.SetFloat("SoundFXVolume", Mathf.Log10(soundFXVolume) * 20); // Convert to decibels
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene") // Replace with your game scene name
        {
            GetComponent<AudioSource>().Stop(); // Stop the music when entering the game scene
        }
        else
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play(); // Resume playing menu music
            }
        }
    }
}