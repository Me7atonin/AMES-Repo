using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TestPauseMenu : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        GetComponent<Canvas>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 1)
        {
            Time.timeScale = 0;
            GetComponent<Canvas>().enabled = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 0)
        {
            Resume();
            Cursor.lockState = CursorLockMode.Locked;
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
