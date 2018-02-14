using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glow : MonoBehaviour {
    public SkinnedMeshRenderer[] renderers;

    private void Start()
    {
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            renderer.material.EnableKeyword("_EMISSION");
        }
    }

    public void SetGlow(Color emissionColor)
    {
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            renderer.material.SetColor("_EmissionColor", emissionColor);
        }
    }
}
