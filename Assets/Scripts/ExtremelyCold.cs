using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtremelyCold : MonoBehaviour {
    public float amount = 30; // out of 100 per second

    private void OnTriggerEnter(Collider other)
    {
        IColdable coldable = other.GetComponent<IColdable>();
        if (coldable != null)
        {
            coldable.EnterCold(amount);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IColdable coldable = other.GetComponent<IColdable>();
        if (coldable != null)
        {
            coldable.ExitCold();
        }
    }
}
