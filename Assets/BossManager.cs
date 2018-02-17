using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour {
    public Boss bossPrefab;
    public BossWall bossWallPrefab;
    public AudioSource music;

    public void StartBoss(Vector2 bossPosition)
    {
        music.Play();
        Instantiate(bossWallPrefab, new Vector3(), Quaternion.identity);
    }
}
