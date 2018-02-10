using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PatrolStep : MonoBehaviour {
    public abstract Vector2 Direction();
    public abstract bool Next();
}
