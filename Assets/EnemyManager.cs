using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MetaEnemyState { Idle, Combat, Dead };

// aka the HiveMind
public class EnemyManager : MonoBehaviour {
    private class MetaEnemy
    {
        public Enemy enemy;
        public MetaEnemyState metaEnemyState = MetaEnemyState.Idle;
        public bool active = true;
    }

    public AudioSource combatMusicBegin;
    public AudioSource combatMusicLoop;
    public AudioSource combatMusicEnd;
    private DynamicMusicState dynamicMusicState = DynamicMusicState.Stopped;
    private enum DynamicMusicState { Playing, Stopped };

    private List<MetaEnemy> metaEnemies = new List<MetaEnemy>();

    public Player player;

    public void RegisterEnemy(Enemy enemy)
    {
        metaEnemies.Add(new MetaEnemy
        {
            enemy = enemy
        });
    }

    private MetaEnemy GetMetaEnemy(Enemy enemy)
    {
        foreach (MetaEnemy metaEnemy in metaEnemies)
        {
            if (metaEnemy.enemy == enemy)
            {
                return metaEnemy;
            }
        }

        return null;
    }

    public void SetMetaEnemyState(Enemy enemy, MetaEnemyState metaEnemyState)
    {
        GetMetaEnemy(enemy).metaEnemyState = metaEnemyState;
    }

    public void SetMetaEnemyActive(Enemy enemy, bool active)
    {
        GetMetaEnemy(enemy).active = active;
    }

    private void Update()
    {
        // Dynamic music
        if (player.Alive)
        {
            bool combating = false;
            foreach (MetaEnemy metaEnemy in metaEnemies)
            {
                if (metaEnemy.metaEnemyState == MetaEnemyState.Combat && metaEnemy.active)
                {
                    combating = true;
                }
            }
            DynamicMusicState previousDynamicMusicState = dynamicMusicState;
            dynamicMusicState = combating ? DynamicMusicState.Playing : DynamicMusicState.Stopped;
            // Switched music states
            if (previousDynamicMusicState != dynamicMusicState)
            {
                switch (dynamicMusicState)
                {
                    case DynamicMusicState.Playing:
                        combatMusicEnd.Stop();
                        combatMusicBegin.Play();
                        combatMusicLoop.PlayDelayed(combatMusicBegin.clip.length);
                        break;

                    case DynamicMusicState.Stopped:
                        combatMusicEnd.Play();
                        combatMusicBegin.Stop();
                        combatMusicLoop.Stop();
                        break;
                }
            }
        }
        else
        {
            combatMusicBegin.Stop();
            combatMusicLoop.Stop();
            combatMusicEnd.Stop();
        }
    }
}
