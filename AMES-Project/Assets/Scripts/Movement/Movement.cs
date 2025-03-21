using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    // Movement speeds
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 12.0f;
    public float crouchSpeed = 1.5f;
    public float lookSpeedX = 2.0f;
    public float lookSpeedY = 2.0f;

    // View bobbing
    public float bobbingAmount = 0.1f;
    public float bobbingSpeed = 10.0f;
    public float sideBobbingAmount = 0.025f;
    public float sideBobbingSpeed = 1.5f;

    public float swayAmount = 0.02f;   // Amount of sway
    public float swaySpeed = 0.5f;     // Speed of sway

    // Camera tilt
    public float tiltAmount = 2.0f;
    public float tiltSpeed = 2.5f;

    // Crouching settings
    public float crouchHeight = 0.5f;
    public float standingHeight = 2.0f;
    private float targetHeight;

    // Jump settings
    public float jumpForce = 3.0f;
    public float gravity = -9.81f;
    private float velocityY = 0f;

    // Stamina settings
    public float maxStamina = 20f;
    public float currentStamina = 20f;
    public float staminaDepletionRate = 20f; // How fast stamina depletes while sprinting
    public float staminaRegenerationRate = 10f; // How fast stamina regenerates when not sprinting
    private bool isExhausted = false;

    // Internal variables
    private Camera playerCamera;
    private Transform playerTransform;
    private Rigidbody rb;
    private float rotationX = 0;
    private float timer = 0f;

    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isJumping = false;
    private bool isWalking = false;

    private Vector3 originalCameraPosition; // To store the initial camera position

    private float currentTilt = 0.0f;
    private float targetTilt = 0.0f;

    // Vignette Post-Processing Variables
    private Volume postProcessVolume;
    private Vignette vignette;
    private float sprintVignetteIntensity = 0.6f; // Max intensity during sprinting
    private float normalVignetteIntensity = 0.275f; // Normal intensity when not sprinting
    private float vignetteLerpSpeed = 5.0f; // Speed of transitioning vignette intensity

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        playerTransform = transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent the Rigidbody from rotating
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevent going through objects

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetHeight = standingHeight; // Set to standing height initially

        // Find the PostProcessVolume and get the Vignette effect
        postProcessVolume = GetComponentInChildren<Volume>();
        if (postProcessVolume != null)
        {
            // Attempt to get the Vignette component from the volume
            postProcessVolume.profile.TryGet(out vignette);
        }
        else
        {
            Debug.LogWarning("PostProcessVolume not found in the scene.");
        }
    }

    void FixedUpdate()
    {
        // Handle movement and jumping (FixedUpdate is better for physics-based movement)
        HandleMovement();
        HandleJumping();
    }

    void Update()
    {
        // Handle input-based actions like crouching, sprinting, and looking around
        HandleCrouching();
        HandleSprinting();
        HandleLookingAround();
        HandleBobbing();
        HandleCameraTilt();

        // Smoothly transition vignette intensity based on sprinting state
        float targetIntensity = isSprinting ? sprintVignetteIntensity : normalVignetteIntensity;
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * vignetteLerpSpeed);

        // Regenerate stamina when not sprinting
        if (!isSprinting && currentStamina < maxStamina)
        {
            RegenerateStamina();
        }
    }

    void HandleMovement()
    {
        // Get movement input (WASD or arrow keys)
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        float moveDirectionX = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime;
        float moveDirectionZ = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;

        // Combine inputs into a Vector3 (for movement)
        Vector3 move = new Vector3(moveDirectionX, 0, moveDirectionZ);
        move = playerCamera.transform.TransformDirection(move); // Make movement relative to camera's facing
        move.y = 0; // Keep movement on the ground plane

        // Check if the player is walking
        isWalking = move.magnitude > 0 && !isSprinting; // Player is walking if there's movement and not sprinting

        // Apply movement using Rigidbody's MovePosition (helps prevent clipping and works with physics)
        Vector3 newPosition = rb.position + move;
        rb.MovePosition(newPosition);
    }

    void HandleJumping()
    {
        // Check if the player is grounded
        bool isGrounded = Physics.Raycast(playerTransform.position, Vector3.down, 1.1f);

        if (isGrounded)
        {
            if (velocityY < 0)
            {
                velocityY = 0f; // Stop downward movement if grounded
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Apply jump force if the player presses Space and is grounded
                velocityY = jumpForce;
            }
        }
        else
        {
            // If not grounded, apply gravity
            velocityY += gravity * Time.deltaTime;
        }

        // Apply the Y velocity (for jumping/falling)
        Vector3 velocity = rb.velocity;
        velocity.y = velocityY;
        rb.velocity = velocity;
    }

    void HandleCrouching()
    {
        // Handle crouching input (LeftControl key)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : standingHeight; // Toggle crouch height
        }

        // Smoothly adjust height when crouching or standing
        playerTransform.localScale = new Vector3(1, Mathf.Lerp(playerTransform.localScale.y, targetHeight, Time.deltaTime * 8f), 1);
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, Mathf.Lerp(playerCamera.transform.localPosition.y, targetHeight / 2f, Time.deltaTime * 8f), playerCamera.transform.localPosition.z);
    }

    void HandleSprinting()
    {
        // Check if the player is sprinting (LeftShift key)
        if (Input.GetKey(KeyCode.LeftShift) && isWalking && !isCrouching && currentStamina > 0f)
        {
            isSprinting = true;
            // Deplete stamina while sprinting
            currentStamina -= staminaDepletionRate * Time.deltaTime;
            isExhausted = currentStamina <= 0f;
        }
        else
        {
            isSprinting = false;
        }
    }

    void RegenerateStamina()
    {
        if (!isSprinting && currentStamina < maxStamina)
        {
            // Regenerate stamina over time when not sprinting
            currentStamina += staminaRegenerationRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina; // Ensure it doesn't exceed max stamina
            }
        }
    }

    void HandleLookingAround()
    {
        // Handle mouse look input (for looking around)
        float rotationY = Input.GetAxis("Mouse X") * lookSpeedX;
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Prevent camera from rotating too far up/down

        // Apply rotations
        playerTransform.Rotate(0, rotationY, 0); // Rotate player body
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Rotate camera up/down
    }

    void HandleBobbing()
    {
        if (isWalking && !isSprinting || isSprinting) // Bobbing applies to walking and sprinting
        {
            // Adjust bobbing speed based on whether the player is sprinting
            float currentBobbingSpeed = isSprinting ? bobbingSpeed * 2.5f : bobbingSpeed;  // Increase by 1.5x while sprinting

            // Bob the camera up and down while walking or sprinting
            timer += Time.deltaTime * currentBobbingSpeed;
            float newY = Mathf.Sin(timer) * bobbingAmount;
            float newX = Mathf.Sin(timer * sideBobbingSpeed) * sideBobbingAmount;

            // Increase the effect further during sprinting
            if (isSprinting)
            {
                newY *= 6f;  // Increased multiplier for stronger bobbing
                newX *= 6f;  // Increased multiplier for stronger side-bobbing
            }

            Vector3 targetPosition = new Vector3(newX, newY, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
        else
        {
            Vector3 targetPosition = new Vector3(0, 0, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
    }

    void HandleCameraTilt()
    {
        // Apply the tilt effect if the player is walking or sprinting
        if (isWalking && !isSprinting || isSprinting)
        {
            // Define a multiplier to speed up the sway when sprinting
            float sprintMultiplier = isSprinting ? 2f : 1f;  // Increased multiplier

            // Use a dynamic tilt amount for stronger sway while sprinting
            float dynamicTiltAmount = isSprinting ? tiltAmount * 3f : tiltAmount;

            // Create a smooth back-and-forth sway effect with a sine wave function
            float sway = Mathf.Sin(Time.time * tiltSpeed * sprintMultiplier) * dynamicTiltAmount;

            // Apply the sway to the camera's local rotation
            currentTilt = Mathf.Lerp(currentTilt, sway, Time.deltaTime * tiltSpeed);
        }
        else
        {
            // Smoothly reset the tilt when the player is not moving
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * tiltSpeed);
        }

        // Update vignette intensity based on sprinting state
        if (vignette != null)
        {
            float targetVignetteIntensity = isSprinting ? 0.085f : 0.075f;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * 5f); // Smoother lerp speed
        }

        // Apply the calculated tilt to the camera
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, currentTilt);
    }
    void HandleIdleSway()
    {
        if (!isWalking && !isSprinting) // When idle
        {
            // Idle sway should only happen when the player is not moving
            // Swaying motion (randomized direction for more natural feel)
            float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            float swayY = Mathf.Cos(Time.time * swaySpeed) * swayAmount;

            // Apply the sway effect smoothly
            Vector3 swayPosition = originalCameraPosition + new Vector3(swayX, swayY, 0);
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, swayPosition, Time.deltaTime * 2f);
        }
        else
        {
            // Reset camera position when the player starts moving
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, originalCameraPosition, Time.deltaTime * 5f);
        }
    }
}