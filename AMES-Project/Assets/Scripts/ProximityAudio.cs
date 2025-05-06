using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    public Transform player;            // Reference to the player
    public float triggerDistance = 5f;  // Distance to trigger sound
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource missing on " + gameObject.name);
        }

        if (player == null)
        {
            Debug.LogError("Player transform not assigned.");
        }
    }

    void Update()
    {
        if (player == null || audioSource == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerDistance)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
