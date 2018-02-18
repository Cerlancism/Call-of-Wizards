using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceChunk : MonoBehaviour {
    public void DestroyIce()
    {
        Debug.Log("Ice destroy");
        Destroy(gameObject);
    }
}
