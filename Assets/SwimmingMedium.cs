using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingMedium : MonoBehaviour {
    public BoxCollider trigger;

    public float SurfaceHeight
    {
        get
        {
            return transform.position.y + trigger.center.y + trigger.size.y / 2;
        }
    }
}
