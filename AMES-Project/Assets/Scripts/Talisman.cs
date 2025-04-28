using UnityEngine;

public class Talisman : MonoBehaviour
{
    public bool isPlaced = false;  // Tracks if the talisman is placed correctly
    private Collider talismanCollider;

    void Start()
    {
        talismanCollider = GetComponent<Collider>();
    }

    // When the talisman enters a designated placement zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlacementZone") && !isPlaced)
        {
            isPlaced = true;
            transform.position = other.transform.position;  // Move the talisman to its placement spot
            transform.SetParent(other.transform);  // Optional: Parent it to the placement zone for better management
            talismanCollider.enabled = false;  // Disable collision if needed
            Debug.Log("Talisman placed in the waterfall!");
        }
    }
}
