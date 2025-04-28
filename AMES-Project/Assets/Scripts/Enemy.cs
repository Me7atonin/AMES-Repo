using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float damageAmount = 10f;  // The amount of damage the enemy deals
    public float damageInterval = 1f; // How often the enemy deals damage while colliding

    private float timeSinceLastDamage = 0f;

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Apply damage over time while colliding with the player
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= damageInterval)
            {
                // Call the PlayerHealth method to apply damage
                collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageAmount);
                timeSinceLastDamage = 0f;
                Debug.Log("Damage applied to player");
            }
        }
    }
}
