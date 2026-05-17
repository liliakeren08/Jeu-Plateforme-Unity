using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Contrôleur principal du joueur. 
/// Gère les mouvements, la santé, l'XP et détecte quand le joueur veut tirer.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _playerSpeed = 6.0f; // Vitesse de base augmentée à 6.0 pour une meilleure réactivité
    [SerializeField] private float _maxHeight = 10f;
    [SerializeField] private float _minHeight = -10f;

    [Header("Stats")]
    [SerializeField] private float _PlayerXpCap = 6f; // XP nécessaire pour le prochain niveau
    [SerializeField] private float _PlayerCurentXp = 0f;
    [SerializeField] private float _PlayerCurentLvl = 1f;
    [SerializeField] private float _playerLife = 20f;

    [Header("Weapons")]
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private Weapons _startingWeapon;
    [SerializeField] private Transform _firePoint;

    [Header("Configuration Audio & FX")]
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private GameObject _hitParticlesPrefab;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private GameObject _deathParticlesPrefab;
    [SerializeField] private AudioClip _levelUpSound;

    private Vector2 _lastDirection = Vector2.right; // Sauvegarde la direction (gauche/droite) pour le tir
    private bool _isShooting = false; // Indique si le joueur maintient le bouton de tir enfoncé

    // Événements pour informer les autres scripts (ex: montée de niveau, mort)
    public event EventHandler<OnPlayerUpEventArgs> OnPlayerUp;
    public event EventHandler OnPlayerDeath;

    public float PlayerCurentXp => _PlayerCurentXp;
    public float PlayerSpeed => _playerSpeed;
    public Transform FirePoint => _firePoint;
    public bool IsShooting => _isShooting;
    public Animator PlayerAnimator => _animator; // Permet à l'arme de déclencher l'animation d'attaque

    private InputSystem_Actions _inputSystem_Actions; // Nouveau système d'Input (requis par la borne)
    private PolygonCollider2D _collider;
    private Camera _cam;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isDead = false;
    private float _maxLife;

    public class OnPlayerUpEventArgs : EventArgs
    {
        public float newLevel;
    }

    private void Start()
    {
        // Initialisation de l'arme de départ
        _weaponManager.AddWeapon(_startingWeapon);

        // Récupération des composants visuels du joueur
        Transform playerVisual = transform.Find("PlayerVisual");
        _animator = playerVisual.GetComponent<Animator>();
        _spriteRenderer = playerVisual.GetComponent<SpriteRenderer>();

        // Initialisation et activation du système d'inputs (clavier/borne)
        _inputSystem_Actions = new InputSystem_Actions();
        _inputSystem_Actions.Player.Enable();

        _collider = GetComponentInChildren<PolygonCollider2D>();
        _cam = Camera.main;

        // Initialisation de la barre de vie
        _maxLife = _playerLife;
        if (UIGame.Instance != null)
        {
            UIGame.Instance.UpdateHealthBar(_playerLife, _maxLife);
        }
    }

    private void Update()
    {
        if (_isDead) return;

        PlayerMovement();
        PlayerShooting();
        PlayerLvlUp();
    }

    /// <summary>
    /// Gère le déplacement du joueur avec le joystick ou le clavier.
    /// </summary>
    private void PlayerMovement()
    {
        // Lit les valeurs de déplacement envoyées par Unity (ex: WASD ou le Joystick de la borne)
        Vector2 input = _inputSystem_Actions.Player.Move.ReadValue<Vector2>();

        // Active l'animation de marche si le joueur bouge
        _animator.SetBool("isWalking", input != Vector2.zero);

        // Oriente le visuel du personnage à gauche ou à droite selon le mouvement
        if (input.x > 0)
            _spriteRenderer.flipX = false;
        else if (input.x < 0)
            _spriteRenderer.flipX = true;

        // On sauvegarde UNIQUEMENT la direction horizontale (X) pour que les boules de feu 
        // partent toujours tout droit (gauche ou droite), même si on marche en diagonale.
        if (input.x != 0)
            _lastDirection = new Vector2(input.x, 0).normalized;

        // Applique le mouvement sur le Transform du personnage
        Vector3 movement = new Vector3(input.x, input.y, 0f);
        transform.position += movement * _playerSpeed * Time.deltaTime;

        ClampMovement();
    }

    /// <summary>
    /// Détecte l'appui sur le bouton de tir. 
    /// L'arme elle-même gérera la cadence (cooldown).
    /// </summary>
    private void PlayerShooting()
    {
        // IsPressed() est VRAI tant que le bouton est enfoncé (permet le tir automatique/continu)
        _isShooting = _inputSystem_Actions.Player.Attack.IsPressed();
    }

    /// <summary>
    /// Empêche le joueur de sortir du cadre de la caméra et de dépasser sous la barre d'UI du haut.
    /// </summary>
    private void ClampMovement()
    {
        if (_cam == null) return;

        float camZ = Mathf.Abs(_cam.transform.position.z);

        // Calcule les 4 bords du cadre de la caméra en coordonnées monde
        float minX = _cam.ViewportToWorldPoint(new Vector3(0, 0, camZ)).x;
        float maxX = _cam.ViewportToWorldPoint(new Vector3(1, 0, camZ)).x;
        float minY = _cam.ViewportToWorldPoint(new Vector3(0, 0, camZ)).y;
        
        // Bloque le joueur sous la barre d'UI du haut (qui occupe le haut de l'écran, soit environ 85% de la hauteur disponible)
        float maxY = _cam.ViewportToWorldPoint(new Vector3(0, 0.85f, camZ)).y;

        // Marges de décalage (rayon du joueur) pour éviter qu'il dépasse à moitié de l'écran sur les côtés
        float marginX = 0.5f;
        float marginY = 0.5f;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX + marginX, maxX - marginX);
        pos.y = Mathf.Clamp(pos.y, minY + marginY, maxY - marginY);
        transform.position = pos;
    }

    /// <summary>
    /// Vérifie si le joueur a assez d'XP pour monter de niveau.
    /// Déclenche l'événement OnPlayerUp qui dit aux armes de s'améliorer !
    /// </summary>
    private void PlayerLvlUp()
    {
        if (_PlayerCurentXp >= _PlayerXpCap)
        {
            _PlayerCurentLvl++;
            _playerSpeed += 0.35f; // Augmente dynamiquement la vitesse du joueur pour suivre le rythme des ennemis !
            _PlayerXpCap += 5f; // On rend le prochain niveau plus long à atteindre
            _PlayerCurentXp = 0f;

            // Joue le son de niveau supérieur s'il est configuré
            if (_levelUpSound != null)
            {
                AudioSource.PlayClipAtPoint(_levelUpSound, Camera.main != null ? Camera.main.transform.position : transform.position);
            }

            // Alerte tous les scripts abonnés (ex: l'arme) que le joueur vient de monter de niveau
            OnPlayerUp?.Invoke(this, new OnPlayerUpEventArgs
            {
                newLevel = _PlayerCurentLvl
            });
        }
    }

    public void AddXP(float amount)
    {
        _PlayerCurentXp += amount;
    }

    public Vector2 GetLastDirection()
    {
        return _lastDirection;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _playerLife -= damage;
        StartCoroutine(FlashRed()); // Fait clignoter le joueur en rouge

        // Effets visuels et sonores de dégâts
        if (_hitParticlesPrefab != null)
        {
            Instantiate(_hitParticlesPrefab, transform.position, Quaternion.identity);
        }

        if (_hitSound != null)
        {
            AudioSource.PlayClipAtPoint(_hitSound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        // Atténue fortement la musique de fond pendant l'impact de dégât
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.DuckMusic(0.2f, 0.4f);
        }

        if (UIGame.Instance != null)
        {
            UIGame.Instance.UpdateHealthBar(_playerLife, _maxLife);
        }

        if (_playerLife <= 0)
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
        if (_isDead) return;
        _isDead = true;

        // Effets visuels et sonores d'explosion de mort
        if (_deathParticlesPrefab != null)
        {
            Instantiate(_deathParticlesPrefab, transform.position, Quaternion.identity);
        }

        if (_deathSound != null)
        {
            AudioSource.PlayClipAtPoint(_deathSound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        // Empêche le joueur de tomber sous l'effet de la gravité quand les colliders sont désactivés
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Désactive tous les colliders pour éviter que d'autres ennemis ne le touchent
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        _animator.SetTrigger("isDead");
        OnPlayerDeath?.Invoke(this, EventArgs.Empty);

        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();

        StartCoroutine(LoadSceneAfterAnimation());
    }

    private IEnumerator LoadSceneAfterAnimation()
    {
        // L'animation Titan_Death dure exactement 0.6 secondes.
        // On attend 0.55 secondes pour masquer le visuel juste avant la toute dernière image
        // afin de garantir qu'on ne revoit jamais la pose Idle/Walk du début !
        yield return new WaitForSeconds(0.55f);

        // Désactive complètement le visuel du joueur juste avant qu'il ne reboucle sur l'idle/walk
        Transform playerVisual = transform.Find("PlayerVisual");
        if (playerVisual != null)
        {
            playerVisual.gameObject.SetActive(false);
        }
        else if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }

        // Une mini pause finale de 0.1 seconde pour finir proprement la transition, puis fin de jeu
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.EndGame();
    }

    private void OnDestroy()
    {
        // Toujours désactiver le système d'inputs quand l'objet est supprimé
        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();
    }
}