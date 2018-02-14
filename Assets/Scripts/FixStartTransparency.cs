using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixStartTransparency : MonoBehaviour {
    public float alpha = 0;

    private void Start()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = alpha;
    }
}
