using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordForwarder : MonoBehaviour {
    public void EnableSword()
    {
        transform.parent.SendMessage("EnableSword");
    }

    public void DisableSword()
    {
        transform.parent.SendMessage("DisableSword");
    }
}
