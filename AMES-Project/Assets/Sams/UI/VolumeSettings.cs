using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private const string VolumePrefKey = "VolumeLevel";

    void Awake()
    {
        // Prevent this GameObject from being destroyed when switching scenes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Load saved volume or default to full volume
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // Add listener to handle volume changes
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();  // Save the setting immediately
    }

    // Ensure the volume is set when a new scene is loaded
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Apply saved volume after scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        AudioListener.volume = savedVolume;
    }
}
