using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private AudioSource audioSourcePrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
    
    public void PlaySoundFX(AudioClip clip, Transform spawnTransform, float volume=1f)
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
