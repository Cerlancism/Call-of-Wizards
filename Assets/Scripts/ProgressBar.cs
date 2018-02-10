using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    public Image progressBar;

    private void Start()
    {
        SetProgress(0);
    }

    public void SetProgress(float amount)
    {
        progressBar.fillAmount = amount;
    }
}
