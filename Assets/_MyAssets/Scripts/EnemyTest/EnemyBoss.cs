using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 30; 
    [SerializeField] private GameObject _enemyAttackPrefab; 
    //[SerializeField] private GameObject _explosionAnim; 
    [SerializeField] private GameObject _xpOrbPrefab; 
    
    [Header("Mouvement")]
    [SerializeField] private float _enemySpeed = 3f; 
    [SerializeField] private float _knockbackForce = 5f; 
    [SerializeField] private float _knockbackDuration = 0.2f; 

    [Header("Attaque")]
    [SerializeField] private bool _canAttack = true; 
    [SerializeField] private int _pointsMinToStartAttack = 500; 
    [SerializeField] private float _fireRateMin = 2f; 
    [SerializeField] private float _fireRateMax = 4f; 

    [Header("Santé")]
    [SerializeField] private float _maxHealth = 1f; 
    [SerializeField] private float _damageOnContact = 1f; 

    [Header("Orbite")]
    [SerializeField] private bool _orbitsPlayer = true; 
    [SerializeField] private float _orbitRadius = 3f; 
    [SerializeField] private float _orbitSpeed = 90f; 
    [SerializeField] private float _orbitCloseSpeed = 0.3f; 

    private float _currentHealth;
    private float _canFire = 0f;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private float _orbitAngle = 0f;
    private Transform _player;
    private Rigidbody2D _rb;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_canAttack)
            EnemyAttack();
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        if (_isKnockback)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            if (_knockbackTimer <= 0f)
                _isKnockback = false;
            return;
        }

        if (_orbitsPlayer)
            OrbitMovement();
        else
            ChaseMovement();
    }

    private void ChaseMovement()
    {
        // Déplacement de l'ennemi en direction du joueur
        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * _enemySpeed;
    }

    private void OrbitMovement()
    {
        // L'ennemi orbite autour du joueur en resserrant progressivement son rayon
        _orbitAngle += _orbitSpeed * Time.fixedDeltaTime;
        _orbitRadius = Mathf.Max(1.2f, _orbitRadius - _orbitCloseSpeed * Time.fixedDeltaTime);

        float rad = _orbitAngle * Mathf.Deg2Rad;
        Vector2 targetPos = (Vector2)_player.position + new Vector2(
            Mathf.Cos(rad) * _orbitRadius,
            Mathf.Sin(rad) * _orbitRadius
        );

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        _rb.linearVelocity = dir * _enemySpeed;
    }

    private void EnemyAttack()
    {
        if (_enemyAttackPrefab == null) return;
        if (GameManager.Instance.PlayerScore < _pointsMinToStartAttack) return;
        if (Time.time < _canFire) return;

        Instantiate(_enemyAttackPrefab, transform.position + new Vector3(0f, -1.1f, 0f), Quaternion.identity);
        float fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        _canFire = Time.time + fireRate;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        //if (_explosionAnim != null)
        //    Instantiate(_explosionAnim, transform.position, Quaternion.identity);

        if (_xpOrbPrefab != null)
            Instantiate(_xpOrbPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.EnemyDestroyed(_enemyPoints, "Bullet");

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[Enemy] Collision avec: {collision.gameObject.tag}");

        if (collision.CompareTag("Enemy") || collision.CompareTag("EnemyAttack")) return;

        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            TakeDamage(1f);
        }

        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.GetComponent<Player>();
            if (playerScript != null)
                playerScript.TakeDamage(_damageOnContact);

            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            _rb.linearVelocity = knockbackDir * _knockbackForce;
            _isKnockback = true;
            _knockbackTimer = _knockbackDuration;
        }
    }
}