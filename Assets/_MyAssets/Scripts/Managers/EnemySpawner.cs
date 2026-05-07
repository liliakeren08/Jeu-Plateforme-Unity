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
        if (elapsed < 20f)
            return 2.0f;

        if (elapsed < 45f)
            return 1.6f;

        if (elapsed < 75f)
            return 1.3f;

        if (elapsed < 120f)
            return 1.1f;

        if (elapsed < 180f)
            return 0.95f;

        return 0.8f;
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
        // Seulement des petits ennemis au début
        if (elapsed < 60f)
        {
            return chaserPrefab;
        }

        // Quelques tanks après 1 minute
        if (elapsed < 180f)
        {
            return Random.value < 0.85f
                ? chaserPrefab
                : tankPrefab;
        }

        // Late game
        return Random.value < 0.65f
            ? chaserPrefab
            : tankPrefab;
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