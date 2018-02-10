using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleForDuration : PatrolStep
{
    public float duration = 1;
    private bool done;

    private void OnEnable()
    {
        done = false;
        StartCoroutine(DoneAfterDuration());
    }

    private IEnumerator DoneAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        done = true;
    }

    public override Vector2 Direction()
    {
        return Vector2.zero;
    }

    public override bool Next()
    {
        return done;
    }
}
