using UnityEngine;

/// <summary>
/// SoundFXManager class handles the sound effects functionality of the game.
/// </summary>
public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }
    [SerializeField] private AudioSource audioSourcePrefab;

    /// <summary>
    /// Singleton instance of SoundFXManager to ensure only one instance exists.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Plays a sound effect at the specified position with the given volume.
    /// </summary>
    public void PlaySoundFX(AudioClip clip, Transform spawnTransform, float volume = 1f)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
    }
}
