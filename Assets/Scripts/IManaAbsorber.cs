using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManaAbsorber {
    void AbsorbMana(float amount);
    bool CanAbsorb();
}
