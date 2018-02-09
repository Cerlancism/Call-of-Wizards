using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {
    public bool showLoadingScreen = false;

    public void Play()
    {
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }
}
