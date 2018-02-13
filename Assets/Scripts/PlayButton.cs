using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {
    bool showLoadingScreen = true;
    static public bool reset;

    public void Play()
    {
        reset = false;
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }

    public void ResetProgress()
    {
        reset = true;
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }

    public void Credits()
    {
        
    }
}
