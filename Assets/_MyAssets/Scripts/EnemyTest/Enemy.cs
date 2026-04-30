using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    //[SerializeField] private int _enemyPoints = 10;
    [SerializeField] private float _enemySpeed = 3f;
    [SerializeField] private GameObject _enemyAttackPrefab;
    [SerializeField] private GameObject _explosionAnim;
    [SerializeField] private int _PointsMinToStartAttack = 500;

    private Transform player;
    //private SpriteRenderer _spriteRenderer; 
    private float _fireRateMin = 2f;
    private float _fireRateMax = 4f;
    private float _canFire = 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        EnemyAttack();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        GetComponent<Rigidbody2D>().linearVelocity = direction * _enemySpeed;
    }

    private void EnemyAttack()
    {
        if (GameManager.Instance.PlayerScore > _PointsMinToStartAttack)
        {
            if (Time.time > _canFire)
            {
                Instantiate(_enemyAttackPrefab, transform.position + new Vector3(0f, -1.1f, 0f), Quaternion.identity); 
                float fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax); 
                _canFire = Time.time + fireRate;
            }
        }
    }

}
