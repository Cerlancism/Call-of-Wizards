using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class FinalAreaTrigger : MonoBehaviour {
    public PlayableDirector doorCloseDirector;
    public PlayableDirector doorOpenDirector;
    public GameObject target;

    public Guard[] deactivateGuards;
    public Guard[] alertGuards;
    public EnemyManager enemyManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target)
        {
            doorCloseDirector.Play();

            foreach (Guard deactivateGuard in deactivateGuards)
            {
                enemyManager.SetMetaEnemyActive(deactivateGuard, false);
            }

            foreach (Guard alertGuard in alertGuards)
            {
                alertGuard.StartCombat();
            }
        }
    }

    private void Update()
    {
        bool allDead = true;
        foreach (Guard alertGuard in alertGuards)
        {
            if (alertGuard.Alive)
            {
                allDead = false;
            }
        }

        if (allDead)
        {
            doorOpenDirector.Play();
        }
    }
}
