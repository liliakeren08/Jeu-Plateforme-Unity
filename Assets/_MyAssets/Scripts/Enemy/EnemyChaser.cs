using System;
using System.Collections;
using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 10;
    [SerializeField] private GameObject _xpOrbPrefab;

    [Header("Mouvement")]
    [SerializeField] private float _enemySpeed = 2f;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.2f;

    [Header("Sant")]
    [SerializeField] private float _maxHealth = 1f;
    [SerializeField] private float _damageOnContact = 1f;

    private float _currentHealth;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private bool _isDead = false;
    private Transform _player;
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();

        // On rcupre l'Animator et SpriteRenderer sur l'enfant ChaserVisual
        Transform chaserVisual = transform.Find("ChaserVisual");
        _animator = chaserVisual.GetComponent<Animator>();
        _spriteRenderer = chaserVisual.GetComponent<SpriteRenderer>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        // Dmarre directement en mode walking ds l'apparition
        _animator.SetBool("isWalking", true);
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

        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * _enemySpeed;

        // Retourne le sprite selon la direction
        // Animation de base regarde vers la droite
        if (direction.x > 0)
            _spriteRenderer.flipX = false; // De face vers la droite
        else if (direction.x < 0)
            _spriteRenderer.flipX = true;  // De dos vers la gauche

        // Active l'animation de marche quand il fonce vers le joueur
        _animator.SetBool("isWalking", true);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        _currentHealth -= amount;

        // Flash rouge quand il prend des dgts
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
        _rb.linearVelocity = Vector2.zero;

        // Trigger mort  l'animation joue jusqu'au bout
        _animator.SetTrigger("isDead");

        if (_xpOrbPrefab != null)
            Instantiate(_xpOrbPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.EnemyDestroyed(_enemyPoints, "Bullet");

        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // On attend que l'Animator soit sur isDead avant de lire sa dure
        yield return null;
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;
        // On ignore les zones utilitaires du joueur (MagnetZone, PickUpZone)
        // pour ne déclencher des dégâts que sur le vrai corps du joueur
        if (collision.GetComponent<MagnetZone>() != null) return;
        if (collision.name == "PickUpZone") return;


        if (collision.CompareTag("Enemy") || collision.CompareTag("EnemyAttack")
            || collision.CompareTag("Xp") || collision.CompareTag("Power")) return;

        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            TakeDamage(1f);
        }

        if (collision.GetComponentInParent<Player>() != null)
        {
            // GetComponentInParent : le script Player est sur le parent (ex: Player_Fireball)
            // mais le PolygonCollider2D peut etre sur l'enfant (PlayerVisual). On remonte donc la hierarchie.
            Player playerScript = collision.GetComponentInParent<Player>();
            if (playerScript != null)
                playerScript.TakeDamage(_damageOnContact);

            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            _rb.linearVelocity = knockbackDir * _knockbackForce;
            _isKnockback = true;
            _knockbackTimer = _knockbackDuration;
        }
    }
}