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
    private bool _canAttack = false;
    private Transform _player;
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();

        if (_rb == null)
        {
            Debug.LogError("Rigidbody2D manquant sur le Boss ! Ajout automatique.");
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        Transform bossVisual = transform.Find("BossVisual");
        if (bossVisual != null)
        {
            _animator = bossVisual.GetComponent<Animator>();
            _spriteRenderer = bossVisual.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("BossVisual introuvable !");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

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
            Vector2 direction = (_player.position - transform.position).normalized;
            _rb.linearVelocity = direction * _enemySpeed;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = _player.position.x <= transform.position.x;
    }

    private IEnumerator AttackAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        _canAttack = true;
    }

    // Calcule la position du firepoint en tenant compte
    // du sens du sprite (gauche/droite)
    private Vector3 GetFirePointPosition()
    {
        if (_firePoint == null)
            return transform.position;

        bool facingRight = _player.position.x > transform.position.x;
        Vector3 offset = _firePoint.localPosition;

        if (!facingRight)
            offset.x = -Mathf.Abs(offset.x);
        else
            offset.x = Mathf.Abs(offset.x);

        return transform.position + offset;
    }

    private void EnemyAttack()
    {
        if (_enemyAttackPrefab == null) return;
        if (GameManager.Instance == null) return;
        if (_player == null) return;
        if (Time.time < _canFire) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer > _attackRange) return;

        Vector3 spawnPos = GetFirePointPosition();
        Vector2 direction = (_player.position - spawnPos).normalized;

        if (_animator != null)
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
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0f)
            Die();
    }

    private IEnumerator FlashRed()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            _spriteRenderer.color = Color.white;
        }
    }

    private void Die()
    {
        _isDead = true;

        if (_animator != null)
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
        yield return null;
        if (_animator != null)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }
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