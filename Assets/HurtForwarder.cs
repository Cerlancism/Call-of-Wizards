using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtForwarder : MonoBehaviour, IHurtable {
    public void Hurt(float amount, bool createsMana = false, Transform sender = null)
    {
        IHurtable parentHurtable = transform.parent.GetComponent<IHurtable>();
        if (parentHurtable != null)
        {
            parentHurtable.Hurt(amount, createsMana, sender);
        }
    }

    public void Kill(bool createsMana = false, Transform sender = null)
    {
        IHurtable parentHurtable = transform.parent.GetComponent<IHurtable>();
        if (parentHurtable != null)
        {
            parentHurtable.Kill(createsMana, sender);
        }
    }
}
