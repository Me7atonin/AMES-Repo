using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform playerCamera;

    private PlayerMovement.PlayerMovement playerInput;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isGrounded;
    public bool isSprinting = false;
    public bool isCrouching = false;
    private float xRotation = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = new PlayerMovement.PlayerMovement();
    }

    private void OnEnable()
    {
        playerInput.player.Enable();
        playerInput.player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInput.player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInput.player.Look.canceled += ctx => lookInput = Vector2.zero;
        playerInput.player.Jump.performed += ctx => Jump();
        playerInput.player.Sprint.performed += ctx => isSprinting = true;
        playerInput.player.Sprint.canceled += ctx => isSprinting = false;
        playerInput.player.Crouch.performed += ctx => ToggleCrouch();
    }

    private void OnDisable()
    {
        playerInput.player.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Get raw camera directions
        Vector3 rawForward = playerCamera.forward;
        Vector3 rawRight = playerCamera.right;

        // Calculate camera angle to vertical
        float forwardAngle = Vector3.Angle(rawForward, Vector3.up);
        float rightAngle = Vector3.Angle(rawRight, Vector3.up);

        // Project onto horizontal plane unless too steep
        Vector3 flatForward = (forwardAngle > 5f && forwardAngle < 175f)
            ? Vector3.ProjectOnPlane(rawForward, Vector3.up).normalized
            : transform.forward;

        Vector3 flatRight = (rightAngle > 5f && rightAngle < 175f)
            ? Vector3.ProjectOnPlane(rawRight, Vector3.up).normalized
            : transform.right;

        // Calculate movement direction
        Vector3 moveDirection = flatForward * moveInput.y + flatRight * moveInput.x;
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        float currentSpeed = isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed;

        Vector3 fullMovement = moveDirection * currentSpeed + Vector3.up * velocity.y;
        velocity.y += gravity * Time.deltaTime;

        controller.Move(fullMovement * Time.deltaTime);

        Debug.DrawRay(playerCamera.position, playerCamera.forward * 100f, Color.red);
    }





    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Clamp the vertical rotation to avoid extreme angles
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -5f, 5f);  // Reduce max look angle range even more
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }


    private void Jump()
    {
        if (isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        controller.height = isCrouching ? 1f : 2f;
    }
}