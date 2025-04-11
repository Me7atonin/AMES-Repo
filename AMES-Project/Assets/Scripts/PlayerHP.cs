using UnityEngine;
using System.Collections;  // Add this to allow IEnumerator and coroutines

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    private bool isCollidingWithEnemy = false;

    // Regen variables
    public float regenRate = 2f; // Health regenerated per second
    private float timeSinceDamage = 0f; // Time since last damage was taken
    public float regenDelay = 5f; // Time (in seconds) before regen starts after taking damage

    void Update()
    {
        if (health <= 0)
        {
            // Handle player death (e.g., trigger death animation, game over screen)
            Debug.Log("Player is dead.");
        }

        // If the player hasn't taken damage for a while, start regenerating health
        if (timeSinceDamage >= regenDelay)
        {
            RegenerateHealth();
        }

        // Keep track of time since the last damage
        timeSinceDamage += Time.deltaTime;
    }

    // Take damage from the enemy
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);  // Prevent health from going below 0
        timeSinceDamage = 0f; // Reset the timer when damage is taken
        Debug.Log("Player took damage. Current health: " + health);
    }

    // Handle collision with the enemy
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isCollidingWithEnemy = true;
            StartCoroutine(ApplyDamageOverTime(collision.gameObject.GetComponent<Enemy>().damageAmount));
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isCollidingWithEnemy = false;
        }
    }

    private IEnumerator ApplyDamageOverTime(float damageAmount)
    {
        while (isCollidingWithEnemy)
        {
            TakeDamage(damageAmount); // Apply damage to the player
            yield return new WaitForSeconds(2f); // Apply damage every 2 seconds while colliding
        }
    }

    // Slow health regeneration after not taking damage for a while
    void RegenerateHealth()
    {
        if (health < maxHealth)
        {
            health += regenRate * Time.deltaTime; // Regenerate health over time
            health = Mathf.Clamp(health, 0, maxHealth); // Ensure health doesn't exceed max
            Debug.Log("Player Health: " + health);
        }
    }
}
