using System;
using System.Collections;
using UnityEngine;

public class EnemyBoss : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 50;
    [SerializeField] private GameObject _enemyAttackPrefab;
    [SerializeField] private GameObject _xpOrbPrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Mouvement")]
    [SerializeField] private float _enemySpeed = 1.5f;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.2f;

    [Header("Attaque")]
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private float _fireRateMin = 0.5f;
    [SerializeField] private float _fireRateMax = 1f;

    [Header("Santé")]
    [SerializeField] private float _maxHealth = 6f;
    [SerializeField] private float _damageOnContact = 5f;

    private float _currentHealth;
    private float _canFire = 0f;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private bool _isDead = false;
    private bool _canAttack = false; // Devient true après le délai de 3s
    private Transform _player;
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();

        // On récupère l'Animator et SpriteRenderer sur l'enfant BossVisual
        Transform bossVisual = transform.Find("BossVisual");
        _animator = bossVisual.GetComponent<Animator>();
        _spriteRenderer = bossVisual.GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        // Attend 3s avant de commencer à attaquer
        // la téléportation est désactivée pour l'instant
        StartCoroutine(AttackAfterDelay());
    }

    private void Update()
    {
        if (_canAttack && !_isDead)
            EnemyAttack();
    }

    private void FixedUpdate()
    {
        if (_player == null || _isDead) return;

        if (_isKnockback)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            if (_knockbackTimer <= 0f)
                _isKnockback = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer > _attackRange)
        {
            // Trop loin — il marche vers le joueur
            Vector2 direction = (_player.position - transform.position).normalized;
            _rb.linearVelocity = direction * _enemySpeed;
            _animator.SetBool("isWalking", true);
        }
        else
        {
            // Dans la zone de tir — il s'arrête et tire
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool("isWalking", false);
        }
    }

    private IEnumerator AttackAfterDelay()
    {
        // Le boss marche d'abord pendant 3s avant de pouvoir attaquer
        yield return new WaitForSeconds(3f);
        _canAttack = true;
    }

    private void EnemyAttack()
    {
        if (_enemyAttackPrefab == null) return;
        if (GameManager.Instance == null) return;
        if (Time.time < _canFire) return;

        // Tire seulement si dans la zone d'attaque
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer > _attackRange) return;

        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;

        // Tire vers le joueur
        Vector2 direction = (_player.position - spawnPos).normalized;

        // Trigger animation attaque quand il tire
        _animator.SetTrigger("attack");

        GameObject proj = Instantiate(_enemyAttackPrefab, spawnPos, Quaternion.identity);
        EnemyFireball fireScript = proj.GetComponent<EnemyFireball>();
        if (fireScript != null)
            fireScript.Init(8f, direction);

        float fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        _canFire = Time.time + fireRate;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        _currentHealth -= amount;

        // Flash rouge quand le Boss prend des dégâts
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0f)
            Die();
    }

    private IEnumerator FlashRed()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        _spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        _isDead = true;

        // Trigger mort — l'animation joue jusqu'au bout
        _animator.SetTrigger("isDead");
        _rb.linearVelocity = Vector2.zero;

        if (_xpOrbPrefab != null)
            Instantiate(_xpOrbPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.EnemyDestroyed(_enemyPoints, "Bullet");

        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // On attend que l'Animator soit sur isDead avant de lire sa durée
        yield return null;
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;

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