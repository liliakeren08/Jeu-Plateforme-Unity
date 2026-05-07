using System;
using System.Collections;
using UnityEngine;

public class EnemyBoss : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 50;
    [SerializeField] private GameObject _enemyAttackPrefab;
    //[SerializeField] private GameObject _explosionAnim;
    [SerializeField] private GameObject _xpOrbPrefab;

    [Header("Mouvement")]
    [SerializeField] private float _enemySpeed = 1.5f;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.2f;

    [Header("Attaque")]
    [SerializeField] private bool _canAttack = true;
    [SerializeField] private int _pointsMinToStartAttack = 100;
    [SerializeField] private float _fireRateMin = 0.5f;
    [SerializeField] private float _fireRateMax = 1f;

    [Header("Téléportation")]
    [SerializeField] private float _teleportCooldown = 2f; 
    [SerializeField] private float _teleportRange = 2f;    

    [Header("Santé")]
    [SerializeField] private float _maxHealth = 6f;
    [SerializeField] private float _damageOnContact = 5f;

    private float _currentHealth;
    private float _canFire = 0f;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private Transform _player;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        StartCoroutine(TeleportLoop());
        StartCoroutine(AttackAfterDelay()); // attend 3s avant de commencer à tirer
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

        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * _enemySpeed;
    }

    private IEnumerator AttackAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        _canAttack = true;
    }

    private void EnemyAttack()
    {
        if (_enemyAttackPrefab == null) return;
        if (GameManager.Instance == null) return;
        if (Time.time < _canFire) return;

        // Direction aléatoire
        float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        GameObject proj = Instantiate(_enemyAttackPrefab, transform.position, Quaternion.identity);
        EnemyFireball fireScript = proj.GetComponent<EnemyFireball>();
        if (fireScript != null)
            fireScript.Init(8f, direction);

        float fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        _canFire = Time.time + fireRate;
    }

    private IEnumerator TeleportLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_teleportCooldown);
            EnemyTeleport();
        }
    }

    private void EnemyTeleport()
    {
        if (_player == null) return;

        _spriteRenderer.enabled = false;
        _rb.linearVelocity = Vector2.zero;

        float angle = UnityEngine.Random.Range(150f, 210f) * Mathf.Deg2Rad; // derrière
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _teleportRange;
        transform.position = (Vector2)_player.position + offset;

        _spriteRenderer.enabled = true;
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
        if (collision.CompareTag("Enemy") || collision.CompareTag("EnemyAttack")
            || collision.CompareTag("Xp") || collision.CompareTag("Power")) return;

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