using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomlessPit : MonoBehaviour {
    private void OnTriggerEnter(Collider other)
    {
        IHurtable hurtable = other.GetComponent<IHurtable>();
        if (hurtable != null)
        {
            hurtable.Kill();
        }
    }
}
