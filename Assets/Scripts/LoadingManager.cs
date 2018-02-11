using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour {
    public ProgressBar progressBar;
    private AsyncOperation loadingAsync;

    private void Start()
    {
        loadingAsync = SceneManager.LoadSceneAsync("Game");
    }

    private void Update()
    {
        if (loadingAsync != null)
        {
            if (!loadingAsync.isDone)
            {
                progressBar.SetProgress(loadingAsync.progress);
            }
        }
    }
}
