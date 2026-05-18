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
    public float firstBossTime = 30f; // MODIFIÉ : Réduit à 30s (au lieu de 90s) pour que le prof puisse voir le Boss rapidement !
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

            // SÉCURITÉ / ÉQUILIBRAGE : Si le boss est actif, on met en pause l'apparition des petits ennemis !
            if (IsBossActive())
            {
                yield return new WaitForSeconds(1.5f); // Attend 1.5s avant de revérifier
                continue;
            }

            int enemyCount = GetEnemyCount();
            float interval = GetSpawnInterval();

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy();
            }

            if (elapsed >= nextBossTime)
            {
                SpawnBoss();
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private float GetSpawnInterval()
    {
        // Early game 
        if (elapsed < 30f) return 4.0f;

       
        if (elapsed < 60f) return 3.5f;

       
        if (elapsed < 120f) return 2.9f;

        // Mid game
        if (elapsed < 180f) return 2.0f;

        // Late game
        return 1.6f;
    }

    private int GetEnemyCount()
    {
        if (elapsed < 45f) return 1;
        if (elapsed < 90f) return 2;
        if (elapsed < 180f) return 3;

        return 4;
    }

    private bool IsBossActive()
    {
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

        // Repousser la prochaine apparition de Boss pour laisser respirer le joueur
        nextBossTime = Time.timeSinceLevelLoad + bossInterval;

        // NETTOYAGE : Détruire tous les petits ennemis présents à l'arrivée du Boss pour un vrai duel épique !
        EnemyChaser[] chasers = FindObjectsByType<EnemyChaser>(FindObjectsSortMode.None);
        foreach (var chaser in chasers)
        {
            Destroy(chaser.gameObject);
        }

        EnemyTank[] tanks = FindObjectsByType<EnemyTank>(FindObjectsSortMode.None);
        foreach (var tank in tanks)
        {
            Destroy(tank.gameObject);
        }

        Debug.Log("[EnemySpawner] Arène nettoyée de ses petits ennemis pour le duel contre le Boss !");
    }

    private GameObject ChooseEnemyType()
    {
        if (elapsed < 30f)
            return chaserPrefab;

        float roll = Random.value;

        // Early game
        if (elapsed < 90f)
        {
            if (roll < 0.80f) return chaserPrefab;
            return tankPrefab;
        }

        // Mid game
        if (elapsed < 180f)
        {
            if (roll < 0.70f) return chaserPrefab;
            return tankPrefab;
        }

        // Late game
        if (roll < 0.60f) return chaserPrefab;
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