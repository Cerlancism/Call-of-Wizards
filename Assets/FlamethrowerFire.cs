using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerFire : MonoBehaviour {
    public float damagePerParticle = 1;

    private void OnParticleCollision(GameObject other)
    {
        IFlammable flammable = other.GetComponent<IFlammable>();
        if (flammable != null)
        {
            flammable.Flame(damagePerParticle);
        }
    }
}
