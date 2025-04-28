using UnityEngine;
using UnityEngine.InputSystem;

public class DebugConsole : MonoBehaviour
{
    private PlayerController playerController;

    private void Start()
    {
        // Find the PlayerController component in the scene
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        // Check for input to toggle noclip mode
        if (Keyboard.current.f3Key.wasPressedThisFrame)  // Example: Press 'F3' to toggle noclip
        {
            if (playerController != null)
            {
// playerController.ToggleNoclip();  // Call ToggleNoclip on PlayerController
                Debug.Log("Noclip toggled");
            }
        }
    }
}
