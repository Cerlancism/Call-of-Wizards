using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenGuardTrigger : MonoBehaviour {
    public GoldenGuard goldenGuardPrefab;
    public SpawnArea[] spawnAreas;
    public int numberOfGuards = 20;
    public Player target;
    public Message message;
    public bool triggered = false;

    [Header("Scene References")]
    public EnemyManager enemyManager;
    public Player player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target.gameObject && !triggered)
        {
            triggered = true;
            message.ShowMessage("The chase is on...");

            // Spawn
            for (int i = 0; i < numberOfGuards; i++)
            {
                SpawnArea randomSpawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];
                Vector3 position = randomSpawnArea.GetRandomSpawnPosition();
                GoldenGuard guard = Instantiate(goldenGuardPrefab, position, Quaternion.identity);

                guard.enemyManager = enemyManager;
                guard.player = player;
            }
        }
    }
}
