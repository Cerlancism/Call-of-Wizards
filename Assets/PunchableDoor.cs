using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PunchableDoor : MonoBehaviour, IHurtable {
    public PlayableDirector playableDirector;

    public void Hurt(float amount, bool createsMana = false)
    {
        playableDirector.Play();
    }

    public void Kill(bool createsMana = false)
    {
        Hurt(1, createsMana);
    }
}
