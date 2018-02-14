using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {
    bool showLoadingScreen = true;

    public void Play()
    {
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("Checkpoint");
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }

    public void Credits()
    {
        
    }
}
