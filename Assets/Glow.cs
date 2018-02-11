using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glow : MonoBehaviour {
    public Material[] materials;

    private void Start()
    {
        foreach (Material material in materials)
        {
            material.EnableKeyword("_EMISSION");
        }
    }

    public void SetGlow(Color emissionColor)
    {
        foreach (Material material in materials)
        {
            material.SetColor("_EmissionColor", emissionColor);
        }
    }
}
