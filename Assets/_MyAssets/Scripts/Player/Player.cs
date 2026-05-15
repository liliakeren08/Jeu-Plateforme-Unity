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
    private bool _isShooting = false;

    public event EventHandler<OnPlayerUpEventArgs> OnPlayerUp;
    public event EventHandler OnPlayerDeath;

    public float PlayerCurentXp => _PlayerCurentXp;
    public float PlayerSpeed => _playerSpeed;
    public Transform FirePoint => _firePoint;
    public bool IsShooting => _isShooting;

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
        PlayerShooting();
        PlayerLvlUp();
    }

    private void PlayerMovement()
    {
        Vector2 input = _inputSystem_Actions.Player.Move.ReadValue<Vector2>();

        _animator.SetBool("isWalking", input != Vector2.zero);

        if (input.x > 0)
            _spriteRenderer.flipX = false;
        else if (input.x < 0)
            _spriteRenderer.flipX = true;

        if (input.x != 0)
            _lastDirection = new Vector2(input.x, 0).normalized;

        Vector3 movement = new Vector3(input.x, input.y, 0f);
        transform.position += movement * _playerSpeed * Time.deltaTime;

        ClampMovement();
    }

    private void PlayerShooting()
    {
        // 1 appui = 1 tir (Semi-Automatique)
        _isShooting = _inputSystem_Actions.Player.Attack.IsPressed();
        
        if (_isShooting)
        {
            // Déclenche l'animation
            _animator.SetTrigger("attack"); 
        }
    }

    private void ClampMovement()
    {
        if (_collider == null || _cam == null) return;

        Bounds b = _collider.bounds;
        float halfW = b.extents.x;
        float halfH = b.extents.y;

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
        StartCoroutine(FlashRed());

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
        if (_inputSystem_Actions != null)
            _inputSystem_Actions.Player.Disable();
    }
}