using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour {
    public Boss bossPrefab;
    public float bossHeight;
    public BossWall bossWallPrefab;
    public float bossWallHeight;
    public AudioSource music;
    public bool triggered = false;
    public Vector2 bossPositionOffset;
    public Cinematic endCinematic;
    private BossWall bossWall;
    private Boss boss;
    public GameObject healthGroup;

    public HealthWheel healthWheel;

    private bool bossHappening;
    public bool BossHappening
    {
        get
        {
            return bossHappening;
        }
    }

    [Header("Pass to Boss")]
    public Player player;

    public void StartBoss(Vector2 bossPosition)
    {
        bossPosition += bossPositionOffset;
        if (!triggered)
        {
            triggered = true;
            music.Play();
            bossWall = Instantiate(bossWallPrefab, new Vector3(bossPosition.x, bossWallHeight, bossPosition.y), Quaternion.identity);
            boss = Instantiate(bossPrefab, new Vector3(bossPosition.x, bossHeight, bossPosition.y), Quaternion.identity);
            boss.player = player;
            boss.bossManager = this;
            healthGroup.SetActive(true);
            healthWheel.health = boss.GetComponent<Health>();
            bossHappening = true;
        }
    }

    public void StopBoss()
    {
        music.Stop();
        Destroy(bossWall);
        healthGroup.SetActive(false);
        endCinematic.StartCinematic();
        bossHappening = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Vector3 position = player.transform.position;
            StartBoss(new Vector2(position.x, position.z));
        }
    }
}
