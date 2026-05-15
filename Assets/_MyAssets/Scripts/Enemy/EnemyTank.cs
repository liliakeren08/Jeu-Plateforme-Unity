using System;
using System.Collections;
using UnityEngine;

public class EnemyTank : MonoBehaviour
{
    [SerializeField] private int _enemyPoints = 20;
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

    [Header("Sant")]
    [SerializeField] private float _maxHealth = 3f;
    [SerializeField] private float _damageOnContact = 3f;

    private float _currentHealth;
    private bool _isKnockback = false;
    private float _knockbackTimer = 0f;
    private bool _isDashing = false;
    private bool _isWindingUp = false;
    private bool _isDead = false;
    private Transform _player;
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer; // Pour le flash rouge

    private void Start()
    {
        _currentHealth = _maxHealth;
        _rb = GetComponent<Rigidbody2D>();

        // On rcupre l'Animator et le SpriteRenderer sur l'enfant TankVisual
        Transform tankVisual = transform.Find("TankVisual");
        _animator = tankVisual.GetComponent<Animator>();
        _spriteRenderer = tankVisual.GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        StartCoroutine(DashLoop());
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

        if (_isDashing || _isWindingUp) return;

        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * _enemySpeed;

        // Active l'animation de marche quand il se dplace vers le joueur
        _animator.SetBool("isWalking", true);
    }

    private IEnumerator DashLoop()
    {
        // La boucle s'arrte ds que le Tank est mort
        while (!_isDead)
        {
            yield return new WaitForSeconds(_dashCooldown);
            if (!_isDead)
                yield return StartCoroutine(DashSequence());
        }
    }

    private IEnumerator DashSequence()
    {
        // Scurit : on ne dash pas si mort
        if (_isDead) yield break;

        _isWindingUp = true;
        _rb.linearVelocity = Vector2.zero;

        // Wind-up : arrt de marche + trigger attaque
        _animator.SetBool("isWalking", false);
        _animator.SetTrigger("attack");

        yield return new WaitForSeconds(_windUpDuration);
        _isWindingUp = false;

        if (_isDead) yield break;

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

    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        _currentHealth -= damage;

        // Clignotement rouge quand le Tank prend des dgts
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

        // Trigger mort  l'animation joue jusqu'au bout
        _animator.SetTrigger("isDead");
        _rb.linearVelocity = Vector2.zero;

        if (_powerOrbPrefab != null)
            Instantiate(_powerOrbPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.EnemyDestroyed(_enemyPoints, "Bullet");

        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // On attend que l'Animator soit bien sur l'tat isDead avant de lire sa dure
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

            _isDashing = false;
            _rb.linearVelocity = Vector2.zero;

            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            _rb.linearVelocity = knockbackDir * _knockbackForce;
            _isKnockback = true;
            _knockbackTimer = _knockbackDuration;
        }
    }
}