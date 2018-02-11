using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [Header("Enemy Settings")]
    public EnemyManager enemyManager;

    private void Start()
    {
        enemyManager.RegisterEnemy(this);
    }
}
