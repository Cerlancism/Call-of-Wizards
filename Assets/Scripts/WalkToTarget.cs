using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToTarget : PatrolStep {
    public Transform target;
    public float nextRadius = 0.2f;

    public override Vector2 Direction()
    {
        Vector3 displacement = target.position - transform.position;
        Vector2 direction = new Vector2(displacement.x, displacement.z);
        return direction;
    }

    public override bool Next()
    {
        Vector3 displacement = target.position - transform.position;
        return displacement.magnitude <= nextRadius;
    }
}
