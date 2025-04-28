using UnityEngine;
using UnityEngine.Rendering;  // For Volume and PostProcessing
using System.Collections;
using UnityEngine.Rendering.Universal;  // For IEnumerator and coroutines

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 10f;
    public float maxHealth = 10f;
    public float lowHealthThreshold = 3f; // Below this = Low Health state

    [Header("Regen Settings")]
    public float regenRate = 1f; // Health regenerated per second
    public float regenDelay = 20f; // Time before regen starts after taking damage

    [Header("Vignette Settings")]
    public Volume postProcessVolume;  // Reference to the Post Process Volume
    private Vignette vignette;         // Vignette effect from PostProcessing
    private float transitionSpeed = 5f;  // Speed at which vignette intensity changes
    private float maxVignetteIntensity = 0.5f;  // Max intensity of vignette

    private float timeSinceDamage = 0f;
    private bool isCollidingWithEnemy = false;
    private Coroutine damageCoroutine; // Hold reference to coroutine for stopping it

    [Header("Damage Cooldown")]
    public float damageCooldown = 1.5f; // Cooldown in seconds
    private float lastDamageTime = -Mathf.Infinity; // Timestamp of last damage taken

    void Start()
    {
        // Fetch the Vignette effect from the PostProcess Volume
        if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.Log("Vignette effect found.");
            vignette.intensity.value = 0f;  // Start with no vignette effect
        }
        else
        {
            Debug.LogError("Vignette effect not found in the Post Process Profile.");
        }
    }

    void Update()
    {
        if (health <= 0)
        {
            // Handle player death
            Debug.Log("Player is dead.");
        }

        // Update vignette intensity based on health
        UpdateVignetteIntensity();

        if (timeSinceDamage >= regenDelay)
        {
            RegenerateHealth();
        }

        timeSinceDamage += Time.deltaTime;
    }

    public void TakeDamage(float damageAmount)
    {
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return; // On cooldown, skip
        }

        lastDamageTime = Time.time; // Update last damage timestamp
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        timeSinceDamage = 0f;
        Debug.Log("Player took damage. Current health: " + health);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(ApplyDamageOverTime(collision.gameObject.GetComponent<Enemy>().damageAmount));
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator ApplyDamageOverTime(float damageAmount)
    {
        while (true)
        {
            TakeDamage(damageAmount);
            yield return new WaitForSeconds(0.25f); // Frequent check, but cooldown guards it
        }
    }

    void RegenerateHealth()
    {
        if (health < maxHealth)
        {
            health += regenRate * Time.deltaTime;
            health = Mathf.Clamp(health, 0, maxHealth);
            Debug.Log("Player Health: " + health);
        }
    }

    // Public check for low health
    public bool IsLowHealth()
    {
        return health <= lowHealthThreshold;
    }

    // Adjust vignette intensity based on health
    void UpdateVignetteIntensity()
    {
        if (vignette != null)
        {
            float healthFactor = Mathf.Clamp01((maxHealth - health) / maxHealth);  // Increases as health decreases
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, healthFactor * maxVignetteIntensity, Time.deltaTime * transitionSpeed);
        }
        else
        {
            Debug.LogError("Vignette is not initialized properly.");
        }
    }
}
