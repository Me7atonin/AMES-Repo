using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float walkSpeed = 3.0f;               // Walking speed
    public float sprintSpeed = 6.0f;             // Sprinting speed
    public float lookSpeedX = 2.0f;              // Mouse look speed (left and right)
    public float lookSpeedY = 2.0f;              // Mouse look speed (up and down)

    // View bobbing variables
    public float bobbingAmount = 0.1f;           // How much the camera bobs up and down
    public float bobbingSpeed = 10.0f;           // Speed of bobbing (how fast it oscillates)
    public float sideBobbingAmount = 0.025f;     // Side to side bobbing amount (for horror)
    public float sideBobbingSpeed = 1.5f;        // Speed of side to side bobbing (for tension)

    // Camera tilt variables
    public float tiltAmount = 2.0f;              // Maximum tilt in degrees (side to side)
    public float tiltSpeed = 2.5f;               // Speed of tilt transition

    private Camera playerCamera;
    private Transform playerTransform;
    private float rotationX = 0;
    private float timer = 0f;                    // Timer to control the bobbing frequency
    private bool isWalking = false;              // Is player currently walking
    private bool isSprinting = false;            // Is player sprinting?

    private float currentTilt = 0.0f;            // Current tilt angle
    private float targetTilt = 0.0f;             // Target tilt angle

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>(); // Find camera attached to player
        playerTransform = transform;
        Cursor.lockState = CursorLockMode.Locked;  // Lock the cursor
        Cursor.visible = false; // Hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        // Check for sprint input (Shift key or whatever you choose)
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Set movement speed based on sprinting
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Player movement
        float moveDirectionX = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime;
        float moveDirectionZ = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;
        playerTransform.Translate(moveDirectionX, 0, moveDirectionZ);

        // Check if player is walking or sprinting
        isWalking = Mathf.Abs(Input.GetAxis("Vertical")) > 0 || Mathf.Abs(Input.GetAxis("Horizontal")) > 0;

        // View bobbing effect
        HandleBobbing();

        // Handle camera tilt based on movement
        HandleCameraTilt();

        // Looking around
        float rotationY = Input.GetAxis("Mouse X") * lookSpeedX;
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Prevent camera from rotating too far

        // Apply rotations
        playerTransform.Rotate(0, rotationY, 0); // Rotate the player body (left/right)
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Rotate the camera (up/down)
    }

    // Handles view bobbing (side-to-side and up-and-down movements)
    void HandleBobbing()
    {
        if (isWalking)
        {
            // If player is moving, apply the bobbing
            timer += Time.deltaTime * bobbingSpeed;

            // Calculate up/down (vertical) motion smoothly
            float newY = Mathf.Sin(timer) * bobbingAmount;

            // Calculate side-to-side (horizontal) motion smoothly
            float newX = Mathf.Sin(timer * sideBobbingSpeed) * sideBobbingAmount;

            // Double the effect while sprinting
            if (isSprinting)
            {
                newY *= 2f;  // Double vertical bobbing while sprinting
                newX *= 2f;  // Double horizontal bobbing while sprinting
            }

            // Apply both vertical and horizontal bobbing to the camera's local position
            Vector3 targetPosition = new Vector3(newX, newY, playerCamera.transform.localPosition.z);

            // Smoothly interpolate the camera's position to avoid jitter
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
        else
        {
            // If the player is not moving, reset the camera's position smoothly
            Vector3 targetPosition = new Vector3(0, 0, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
    }

    // Handles camera tilt effect based on player movement (left and right)
    void HandleCameraTilt()
    {
        // Get horizontal movement input
        float moveInput = Input.GetAxis("Horizontal");

        // Calculate target tilt based on movement direction
        if (moveInput > 0)
            targetTilt = -tiltAmount; // Tilt left
        else if (moveInput < 0)
            targetTilt = tiltAmount;  // Tilt right
        else
            targetTilt = 0.0f;       // No movement

        // Double the tilt effect while sprinting for more intensity
        if (isSprinting)
        {
            targetTilt *= 2f;  // Double the tilt while sprinting
        }

        // Smoothly transition to the target tilt
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Apply the tilt to the camera's Z-axis rotation
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, currentTilt);
    }
}
