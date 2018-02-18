using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {
    public Message message;
    public Player target;
    public ParticleSystem effect;
    public Transform destination;
    public bool triggered = false;
    public CheckpointManager checkpointManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target.gameObject)
        {
            GameObject[] goldenGuardObjects = GameObject.FindGameObjectsWithTag("Golden Guard");
            bool allGuardsDead = true;

            foreach (GameObject goldenGuardObject in goldenGuardObjects)
            {
                GoldenGuard goldenGuard = goldenGuardObject.GetComponent<GoldenGuard>();
                if (goldenGuard.Alive)
                {
                    allGuardsDead = false;
                }
            }

            if (allGuardsDead)
            {
                StartCoroutine(DelayTeleport());
            }
            else
            {
                message.ShowMessage("Kill with fire");
            }
        }
    }

    private IEnumerator DelayTeleport()
    {
        message.ShowMessage("5s to telport");
        yield return new WaitForSeconds(5);
        target.transform.position = destination.position;
        effect.Play();
        checkpointManager.SetSpawn("Level 3");
    }
}
