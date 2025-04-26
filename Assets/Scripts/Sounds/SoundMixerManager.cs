using System;
using UnityEngine;
using UnityEngine.Audio;

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


    void Start()
    {
        LoadSliders(); // Load volume settings at the start
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20); // Convert linear volume to decibels
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume",  Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public void SetSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFXVolume",  Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(SoundFXVolumeKey, volume);
        SoundFXManager.instance.PlaySoundFX(testSoundFX, transform, volume); // Play test sound effect
    }

    public void LoadSliders()
    {
        MasterVolumeSlider.GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        MusicVolumeSlider.GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        SoundFXVolumeSlider.GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat(SoundFXVolumeKey, 0.75f);
    }
}