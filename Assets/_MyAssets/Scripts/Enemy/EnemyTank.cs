using System;
using System.Collections;
using UnityEngine;

public class EnemyTank : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 20;
    //[SerializeField] private GameObject _explosionAnim;
    [SerializeField] private GameObject _powerOrbPrefab;

    [Header("Mouvement")]
    [SerializeField] private float _enemySpeed = 1f;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.2f;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 18f;       
    [SerializeField] private float _dashCooldown = 3f;     
    [SerializeField] private float _windUpDuration = 0.8f; 
    [SerializeField] private float _dashDuration = 0.25f;  

    [Header("Santé")]
    [SerializeField] private float _maxHealth = 3f;
    [SerializeField] private float _damageOnContact = 3f;

    private float _currentHealth;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private bool _isDashing = false;
    private bool _isWindingUp = false;
    private Transform _player;
    private Rigidbody2D _rb;
    //private Animator _animator; // décommenter quand les animations seront prêtes

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();
        //_animator = GetComponent<Animator>(); // décommenter quand les animations seront prêtes

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        StartCoroutine(DashLoop());
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

        // Pendant wind-up ou dash, on ne bouge pas normalement
        if (_isDashing || _isWindingUp) return;

        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * _enemySpeed;
    }

    private IEnumerator DashLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_dashCooldown);
            yield return StartCoroutine(DashSequence());
        }
    }

    private IEnumerator DashSequence()
    {
        _isWindingUp = true;
        _rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(_windUpDuration);
        _isWindingUp = false;

        _isDashing = true;
        if (_player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
            if (distanceToPlayer >= _dashSpeed * 0.3f)
            {
                Vector2 direction = (_player.position - transform.position).normalized;
                _rb.linearVelocity = direction * _dashSpeed;
            }
        }

        yield return new WaitForSeconds(_dashDuration);
        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;
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

        if (_powerOrbPrefab != null)
            Instantiate(_powerOrbPrefab, transform.position, Quaternion.identity);

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

            // Stop le dash si frappe le joueur
            _isDashing = false;
            _rb.linearVelocity = Vector2.zero;

            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            _rb.linearVelocity = knockbackDir * _knockbackForce;
            _isKnockback = true;
            _knockbackTimer = _knockbackDuration;
        }
    }
}