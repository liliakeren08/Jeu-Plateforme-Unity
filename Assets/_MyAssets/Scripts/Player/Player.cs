using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 5f;
    [SerializeField] private float _PlayerXpCap = 6f;
    [SerializeField] private float _PlayerCurentXp = 0f;
    [SerializeField] private float _PlayerCurentLvl = 1f;
    [SerializeField] private float _playerLife = 20f;
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private Weapons _startingWeapon;
    private Vector2 _lastDirection = Vector2.right;
    public event EventHandler<OnPlayerUpEventArgs> OnPlayerUp;
    public event EventHandler OnPlayerDeath;
    public float PlayerCurentXp => _PlayerCurentXp;
    public float PlayerSpeed => _playerSpeed;
    private SpriteRenderer _spriteRenderer;
    private InputSystem_Actions _inputSystem_Actions;
    [SerializeField] private float _maxHeight = 5f;

    private float _minX, _maxX;
    private float _minY, _maxY;

    private void Start()
    {
        _weaponManager.AddWeapon(_startingWeapon);
        Camera mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        float halfPlayerWithd = _spriteRenderer.bounds.extents.x;
        float halfPlayerHeight = _spriteRenderer.bounds.extents.y;
        _inputSystem_Actions = new InputSystem_Actions();
        _inputSystem_Actions.Player.Enable();

        _minX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + halfPlayerWithd;
        _maxX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - halfPlayerWithd;
        _minY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + halfPlayerHeight;
        _maxY = _maxHeight - halfPlayerHeight;
    }
    public class OnPlayerUpEventArgs : EventArgs
    {
        public float newLevel;
    }

    private void Update()
    {
        PlayerMovement();
        PlayerLvlUp();
    }

    private void PlayerLvlUp()
    {
        if (_PlayerCurentXp >= _PlayerXpCap)
        {

            LevelUp();
            
            Debug.Log("Lv: " + _PlayerCurentLvl);

        }
    }

    public void AddXP(float amount)
    {
        _PlayerCurentXp += amount;
    }

    private void OnDestroy()
    {
        _inputSystem_Actions.Player.Disable();
    }

    private void PlayerMovement()
    {
        Vector2 direction2D = _inputSystem_Actions.Player.Move.ReadValue<Vector2>();

        if (direction2D.x != 0)
            _lastDirection = new Vector2(direction2D.x, 0).normalized;

        Vector3 movement = new Vector3(direction2D.x, direction2D.y, 0f);
        transform.position += movement * _playerSpeed * Time.deltaTime;

        float clampedX = Mathf.Clamp(transform.position.x, _minX, _maxX);
        float clampedY = Mathf.Clamp(transform.position.y, _minY, _maxY);

        transform.position = new Vector2(clampedX, clampedY);

    }

    private void LevelUp()
    {
        _PlayerCurentLvl++;

        _PlayerXpCap += 5f;
        _PlayerCurentXp = 0f;
        OnPlayerUp?.Invoke(this, new OnPlayerUpEventArgs
        {
            newLevel = _PlayerCurentLvl
        }) ;
    }

    public Vector2 GetLastDirection()
    {
        return _lastDirection;
    }

    public void TakeDamage(float damage)
    {
        _playerLife -= damage;

        if (_playerLife <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player is dead");

        OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        _inputSystem_Actions.Player.Disable();

        // option simple pour l'instant
        gameObject.SetActive(false);
    }




}
