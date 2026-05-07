using System;
using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 10; 
    //[SerializeField] private GameObject _explosionAnim; 
    [SerializeField] private GameObject _xpOrbPrefab; 
    
    [Header("Mouvement")]
    [SerializeField] private float _enemySpeed = 3f; 
    [SerializeField] private float _knockbackForce = 5f; 
    [SerializeField] private float _knockbackDuration = 0.2f; 

    [Header("Santé")]
    [SerializeField] private float _maxHealth = 1f; 
    [SerializeField] private float _damageOnContact = 1f;

    private float _currentHealth;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
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

        if (_maxHealth > 0)
        {
            ChaseMovement();
        }
    }

    private void ChaseMovement()
    {
        // Déplacement de l'ennemi en direction du joueur
        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * _enemySpeed;
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