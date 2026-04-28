using System;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject chaserPrefab;
    public GameObject tankPrefab;
    public GameObject bossPrefab;

    [Header("Difficulty Settings")]
    public float initialSpawnInterval = 3f;
    public float minSpawnInterval = 0.6f;
    public float difficultyRampTime = 60f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        float elapsed = 0f;

        while (true)
        {
            float t = Mathf.Clamp01(elapsed / difficultyRampTime);
            float interval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, t);

            SpawnEnemy(elapsed);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void SpawnEnemy(float elapsed)
    {
        GameObject prefab = ChooseEnemyType(elapsed);
        Vector2 pos = GetPerimeterSpawnPoint();
        Instantiate(prefab, pos, Quaternion.identity);
    }

    private GameObject ChooseEnemyType(float elapsed)
    {
        if (elapsed < 20f) return chaserPrefab;
        if (elapsed < 40f) return Random.value < 0.7f ? chaserPrefab : tankPrefab;

        float roll = Random.value;
        if (roll < 0.5f) return chaserPrefab;
        if (roll < 0.75f) return tankPrefab;
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
