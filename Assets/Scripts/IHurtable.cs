using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHurtable {
    void Hurt(float amount, bool createsMana = false, Transform sender = null);
    void Kill(bool createsMana = false, Transform sender = null);
}
