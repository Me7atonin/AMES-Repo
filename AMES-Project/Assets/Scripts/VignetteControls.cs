using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEffects : MonoBehaviour
{
    public Volume postProcessVolume; // Reference to the Post-process Volume component
    private Vignette vignette;       // Reference to the Vignette effect
    private float transitionSpeed = 5f;  // Speed of vignette change
    private float maxVignetteIntensity = 0.5f;  // Max intensity of vignette effect
    private float currentVignetteIntensity = 0f; // Current vignette intensity

    public float playerHealth = 10f;  // Player's health

    void Start()
    {
        // Try to get the Vignette effect from the volume profile
        if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
        {
            // Initialize vignette intensity
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogError("Vignette effect not found in the Post Process Profile.");
        }
    }

    void Update()
    {
        // Calculate vignette intensity based on player's health
        float healthFactor = Mathf.Clamp01((10f - playerHealth) / 10f);  // Increases as health decreases

        // Smoothly transition vignette intensity
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, healthFactor, Time.deltaTime * transitionSpeed);
        vignette.intensity.value = Mathf.Clamp(currentVignetteIntensity, 0f, maxVignetteIntensity);  // Cap intensity to max value
    }
}
