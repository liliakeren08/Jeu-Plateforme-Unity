using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject chaserPrefab;
    public GameObject tankPrefab;
    public GameObject bossPrefab;

    [Header("Boss Settings")]
    public float firstBossTime = 90f;
    public float bossInterval = 120f;

    private Camera cam;
    private float elapsed;
    private float nextBossTime;

    private void Start()
    {
        cam = Camera.main;
        nextBossTime = firstBossTime;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            elapsed = Time.timeSinceLevelLoad;

            float interval = GetSpawnInterval();

            
            SpawnEnemy();

            
            if (elapsed >= nextBossTime)
            {
                SpawnBoss();
                nextBossTime += bossInterval;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private float GetSpawnInterval()
    {
        if (elapsed < 20f) return 5.0f;
        if (elapsed < 45f) return 4.0f;
        if (elapsed < 75f) return 3.5f;
        if (elapsed < 120f) return 3.0f;
        if (elapsed < 180f) return 2.5f;
        if (elapsed < 210f) return 2.0f;
        if (elapsed < 260f) return 1.5f;
        return 1.0f;
    }

    private void SpawnEnemy()
    {
        GameObject prefab = ChooseEnemyType();
        Vector2 pos = GetPerimeterSpawnPoint();

        Instantiate(prefab, pos, Quaternion.identity);
    }

    private void SpawnBoss()
    {
        Vector2 pos = GetPerimeterSpawnPoint();

        Instantiate(bossPrefab, pos, Quaternion.identity);
    }

    private GameObject ChooseEnemyType()
    {
        if (elapsed < 20f)
            return chaserPrefab;

        if (elapsed < 60f)
        {
            float roll = Random.value;
            if (roll < 0.7f) return chaserPrefab;
            return tankPrefab;
        }

        // Après 1 minute — les 3 types
        float r = Random.value;
        if (r < 0.55f) return chaserPrefab;
        if (r < 0.85f) return tankPrefab;
        return bossPrefab;
    }

    private Vector2 GetPerimeterSpawnPoint()
    {
        float camH = cam.orthographicSize + 0.5f;
        float camW = camH * cam.aspect + 0.5f;

        int edge = Random.Range(0, 4);

        return edge switch
        {
            0 => new Vector2(Random.Range(-camW, camW), camH),
            1 => new Vector2(Random.Range(-camW, camW), -camH),
            2 => new Vector2(-camW, Random.Range(-camH, camH)),
            _ => new Vector2(camW, Random.Range(-camH, camH)),
        };
    }
}