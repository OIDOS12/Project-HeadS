using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioSource audioSourcePrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
            PlayMainMenuMusic();
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load events
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
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

    private void PlayMainMenuMusic()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = menuMusicClip;
        audioSource.loop = true; // Loop the menu music
        audioSource.playOnAwake = false;
        audioSource.volume = 0.1f; // Adjust volume as needed
        audioSource.Play(); // Start playing menu music
    }
    
    public void PlaySoundFX(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
    }

    // private void OnDestroy()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from scene load events
    // }
}
