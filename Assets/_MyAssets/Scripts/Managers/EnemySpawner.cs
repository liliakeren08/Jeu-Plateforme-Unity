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
        if (elapsed < 25f) interval = 4.5f;      // Début immédiatement actif et engageant
        else if (elapsed < 60f) interval = 4.0f;  // Période intermédiaire agréable
        else if (elapsed < 120f) interval = 3.5f; // Légère montée d'adrénaline
        else if (elapsed < 180f) interval = 3.0f; // Progression finale stabilisée et saine
        else interval = 2.2f;                    // Reste dynamique sans jamais saturer l'arène de jeu

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
        // Si le boss est actif, on ne fait apparaître QUE des Chasers (fantômes légers et rapides)
        // pour que le joueur puisse se concentrer à 100% sur l'esquive des tirs du boss sans être bloqué par des sacs à PV !
        if (IsBossActive())
        {
            return chaserPrefab;
        }

        if (elapsed < 20f)
            return chaserPrefab;

        if (elapsed < 60f)
        {
            float roll = Random.value;
            if (roll < 0.85f) return chaserPrefab; // 85% Chasers (ennemis légers)
            return tankPrefab;                     // 15% Tanks (ennemis lourds rares)
        }

        // Après 1 minute
        float r = Random.value;
        if (r < 0.80f) return chaserPrefab;       // 80% Chasers
        return tankPrefab;                        // 20% Tanks (excellent ratio d'action !)
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