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
    [SerializeField] private float _playerSpeed = 5f;
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
    }

    private void Update()
    {
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
    /// Empêche le joueur de sortir du cadre de la caméra.
    /// </summary>
    private void ClampMovement()
    {
        if (_cam == null) return;

        float camZ = Mathf.Abs(_cam.transform.position.z);

        // Calcule les 4 bords exacts du cadre de la caméra en coordonnées monde
        float minX = _cam.ViewportToWorldPoint(new Vector3(0, 0, camZ)).x;
        float maxX = _cam.ViewportToWorldPoint(new Vector3(1, 0, camZ)).x;
        float minY = _cam.ViewportToWorldPoint(new Vector3(0, 0, camZ)).y;
        float maxY = _cam.ViewportToWorldPoint(new Vector3(0, 1, camZ)).y;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
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
            _PlayerXpCap += 5f; // On rend le prochain niveau plus long à atteindre
            _PlayerCurentXp = 0f;

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
        _playerLife -= damage;
        StartCoroutine(FlashRed()); // Fait clignoter le joueur en rouge

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
        _animator.SetTrigger("isDead");
        OnPlayerDeath?.Invoke(this, EventArgs.Empty);

        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();

        StartCoroutine(LoadSceneAfterAnimation());
    }

    private IEnumerator LoadSceneAfterAnimation()
    {
        yield return null;
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnDestroy()
    {
        // Toujours désactiver le système d'inputs quand l'objet est supprimé
        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();
    }
}