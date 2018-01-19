using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour, IHurtable {
    public Ragdoll ragdoll;

	public void Hurt(float amount)
    {
        ragdoll.SetEnabled(true);
    }
}
