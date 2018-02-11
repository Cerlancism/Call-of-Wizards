using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public AudioSource combatMusicSource;
    public AudioClip combatMusicBegin;
    public AudioClip combatMusicLoop;
    public AudioClip combatMusicEnd;

    private List<Enemy> enemies;

    public void RegisterEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    private void Update()
    {
        
    }
}
