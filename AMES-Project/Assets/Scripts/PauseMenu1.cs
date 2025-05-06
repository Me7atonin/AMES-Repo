using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TestPauseMenu : MonoBehaviour
{
    // Input Action for Pause
    private InputAction pauseAction;

    // Use this for initialization
    void Awake()
    {
        // Create a new Input Action map (in this case, a simple Pause action)
        var playerInputActions = new InputActionMap("Player");

        // Set up the action for the "Pause" button (Escape key)
        pauseAction = playerInputActions.AddAction("Pause", binding: "<Keyboard>/escape");

        // Enable the action map
        playerInputActions.Enable();
    }

    void Start()
    {
        GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {
        // Check if the pause action was triggered
        if (pauseAction.triggered)
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                GetComponent<Canvas>().enabled = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Resume();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        GetComponent<Canvas>().enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitGame()
    {
        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1;
    }
}
