using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _playerSpeed = 5f;
    [SerializeField] private float _maxHeight = 10f;
    [SerializeField] private float _minHeight = -10f;

    [Header("Stats")]
    [SerializeField] private float _PlayerXpCap = 6f;
    [SerializeField] private float _PlayerCurentXp = 0f;
    [SerializeField] private float _PlayerCurentLvl = 1f;
    [SerializeField] private float _playerLife = 20f;

    [Header("Weapons")]
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private Weapons _startingWeapon;
    [SerializeField] private Transform _firePoint;

    private Vector2 _lastDirection = Vector2.right;

    public event EventHandler<OnPlayerUpEventArgs> OnPlayerUp;
    public event EventHandler OnPlayerDeath;

    public float PlayerCurentXp => _PlayerCurentXp;
    public float PlayerSpeed => _playerSpeed;
    public Transform FirePoint => _firePoint;

    private InputSystem_Actions _inputSystem_Actions;
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
        _weaponManager.AddWeapon(_startingWeapon);

        // On cible explicitement PlayerVisual pour éviter de prendre
        // l'Animator d'un enfant comme Player_Fireball
        Transform playerVisual = transform.Find("PlayerVisual");
        _animator = playerVisual.GetComponent<Animator>();
        _spriteRenderer = playerVisual.GetComponent<SpriteRenderer>();

        _inputSystem_Actions = new InputSystem_Actions();
        _inputSystem_Actions.Player.Enable();

        _collider = GetComponent<PolygonCollider2D>();
        _cam = Camera.main;
    }

    private void Update()
    {
        PlayerMovement();
        PlayerLvlUp();
    }

    private void PlayerMovement()
    {
        Vector2 input = _inputSystem_Actions.Player.Move.ReadValue<Vector2>();

        _animator.SetBool("isWalking", input != Vector2.zero);

        if (input.x > 0)
            _spriteRenderer.flipX = true;
        else if (input.x < 0)
            _spriteRenderer.flipX = false;

        if (input.x != 0)
            _lastDirection = new Vector2(input.x, 0).normalized;

        Vector3 movement = new Vector3(input.x, input.y, 0f);
        transform.position += movement * _playerSpeed * Time.deltaTime;

        ClampMovement();
    }

    private void ClampMovement()
    {
        if (_collider == null || _cam == null) return;

        Bounds b = _collider.bounds;

        float halfW = b.extents.x;
        float halfH = b.extents.y;

        // Fix : on passe la distance Z de la caméra pour que
        // ViewportToWorldPoint calcule correctement les bords de l'écran
        float camZ = Mathf.Abs(_cam.transform.position.z);

        float minX = _cam.ViewportToWorldPoint(new Vector3(0, 0, camZ)).x + halfW;
        float maxX = _cam.ViewportToWorldPoint(new Vector3(1, 0, camZ)).x - halfW;

        float minY = _minHeight + halfH;
        float maxY = _maxHeight - halfH;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    private void PlayerLvlUp()
    {
        if (_PlayerCurentXp >= _PlayerXpCap)
        {
            _PlayerCurentLvl++;
            _PlayerXpCap += 5f;
            _PlayerCurentXp = 0f;

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
        StartCoroutine(FlashRed()); // Clignotement rouge quand le joueur prend des dégâts

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
        _animator.SetTrigger("isDead"); // Trigger au lieu de SetBool

        OnPlayerDeath?.Invoke(this, EventArgs.Empty);

        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnDestroy()
    {
        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();
    }
}