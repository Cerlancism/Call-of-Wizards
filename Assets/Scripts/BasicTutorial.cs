using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTutorial : MonoBehaviour {
    public Message message;

    public void StartTutorial()
    {
        StartCoroutine(DelayTutorial());
    }

    private IEnumerator DelayTutorial()
    {
        yield return new WaitForSeconds(10.9f);
        message.ShowMessage("WASD move - M1 attack");
        yield return new WaitForSeconds(3);
        message.ShowMessage("CTRL - sneak");
        yield return new WaitForSeconds(3);
        message.ShowMessage("ESC - Pause");
    }
}
