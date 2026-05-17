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
        float interval;
        if (elapsed < 20f) interval = 6.0f;      // Début très calme pour s'équiper
        else if (elapsed < 45f) interval = 5.0f;
        else if (elapsed < 75f) interval = 4.0f;
        else if (elapsed < 120f) interval = 3.5f;
        else if (elapsed < 180f) interval = 3.0f;
        else if (elapsed < 210f) interval = 2.5f;
        else if (elapsed < 260f) interval = 2.0f;
        else interval = 1.5f;                    // Fin de partie dynamique mais faisable

        // Si le boss est actif, on ralentit drastiquement l'apparition des autres ennemis (x3)
        // pour laisser la vedette au combat de boss et éviter d'étouffer le joueur !
        if (IsBossActive())
        {
            interval *= 3f;
        }

        return interval;
    }

    private bool IsBossActive()
    {
        // Recherche si le boss est en vie dans la scène
        return FindFirstObjectByType<EnemyBoss>() != null;
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

        // Après 1 minute — uniquement les ennemis normaux (Chaser et Tank).
        // Le Boss ne doit apparaître QUE via son timer dédié (firstBossTime) pour rester unique et mémorable !
        float r = Random.value;
        if (r < 0.60f) return chaserPrefab;
        return tankPrefab;
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