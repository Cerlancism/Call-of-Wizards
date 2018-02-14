using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {
    public SpellCollected message;
    public Player target;
    public ParticleSystem effect;
    public Transform destination;
    public bool triggered = false;
    public CheckpointManager checkpointManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target.gameObject && !triggered)
        {
            triggered = true;
            StartCoroutine(DelayTeleport());
        }
    }

    private IEnumerator DelayTeleport()
    {
        message.ShowMessage("10s to teleport");
        yield return new WaitForSeconds(5);
        message.ShowMessage("5s to telport");
        yield return new WaitForSeconds(5);
        target.transform.position = destination.position;
        effect.Play();
        checkpointManager.SetSpawn("Level 3");
    }
}
