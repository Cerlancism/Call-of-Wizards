using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour {
    bool showLoadingScreen = true;
    public Canvas pauseCanvas;
	// Use this for initialization
	void Start ()
    {
        pauseCanvas.enabled = false;
	}

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            pauseCanvas.enabled = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Resume()
    {
        pauseCanvas.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Menu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
