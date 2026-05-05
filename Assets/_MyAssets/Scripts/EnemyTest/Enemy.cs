using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    //[SerializeField] private int _enemyPoints = 10;
    [SerializeField] private float _enemySpeed = 3f;
    [SerializeField] private GameObject _enemyAttackPrefab;
    [SerializeField] private GameObject _explosionAnim;
    [SerializeField] private int _PointsMinToStartAttack = 500;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.2f;

    private Transform player;
    //private SpriteRenderer _spriteRenderer; 
    private float _fireRateMin = 2f;
    private float _fireRateMax = 4f;
    private float _canFire = 0f;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private Rigidbody2D rb;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        EnemyAttack();
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        if (_isKnockback)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            if (_knockbackTimer <= 0)
            {
                _isKnockback = false;
            }
            return; 
        }

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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject); 
        }

        if (collision.CompareTag("Player"))
        {
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;

            rb.linearVelocity = knockbackDir * _knockbackForce;

            _isKnockback = true;
            _knockbackTimer = _knockbackDuration;
        }
    }

}
