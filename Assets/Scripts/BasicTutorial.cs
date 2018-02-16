using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTutorial : MonoBehaviour {
    public Message message;
    public bool done = false;

    private void Start()
    {
        StartCoroutine(DelayTutorial());
    }

    private IEnumerator DelayTutorial()
    {
        yield return new WaitForSeconds(3);
        if (!done)
        {
            message.ShowMessage("WASD move - M1 attack");
        }
    }
}
