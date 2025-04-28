using UnityEngine;
using Cinemachine;

public class CameraNoiseSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;  // Virtual Camera reference
    public NoiseSettings idleNoiseProfile;          // Idle noise profile (not used for direct changes)
    public NoiseSettings movementNoiseProfile;      // Movement noise profile (not used for direct changes)
    public CharacterController playerController;    // Player character controller

    private CinemachineBasicMultiChannelPerlin perlin; // Perlin noise component of the virtual camera
    private float transitionSpeed = 5f;              // Speed at which the transition happens

    private float currentMovementWeight = 0f;        // Current weight of movement noise
    private float targetMovementWeight = 0f;         // Target weight based on movement

    void Start()
    {
        // Fetch the Perlin noise component from the Cinemachine virtual camera
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin component is missing.");
        }
    }

    void Update()
    {
        // Calculate the player's speed
        float playerSpeed = playerController.velocity.magnitude;
        bool isSprinting = playerController.velocity.magnitude > 8.5f; // Adjust threshold to fit your sprint speed
        bool isGrounded = playerController.isGrounded;  // Check if the player is grounded

        // Set target weight based on movement (higher speed means more movement noise)
        targetMovementWeight = Mathf.Clamp01(playerSpeed / 4f); // Adjust speed divisor to fit your needs

        // Smoothly transition between idle and movement noise using Lerp
        currentMovementWeight = Mathf.Lerp(currentMovementWeight, targetMovementWeight, Time.deltaTime * transitionSpeed);

        // Ensure that the perlin noise component exists before modifying it
        if (perlin != null)
        {
            float maxAmplitude = isSprinting ? 3.0f : 2.0f; // More intense when sprinting
            float maxFrequency = isSprinting ? 1.5f : 1.0f; // Faster shake during sprint

            // Apply Lerp for smooth transition between idle and movement noise intensity
            perlin.m_AmplitudeGain = Mathf.Lerp(0f, 1.5f, currentMovementWeight); // Transition from 0 (idle) to 1 (movement)
            perlin.m_FrequencyGain = Mathf.Lerp(0.2f, 0.8f, currentMovementWeight); // Adjust these values to suit your needs

            // Ensure that the correct noise profile is active based on movement state
            perlin.m_NoiseProfile = (currentMovementWeight > 0f && isGrounded) ? movementNoiseProfile : idleNoiseProfile;
        }
    }
}
