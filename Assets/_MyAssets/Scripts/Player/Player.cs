using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 5f;

    [SerializeField] private float _PlayerXpCap = 6f;
    [SerializeField] private float _PlayerCurentXp = 0f;
    [SerializeField] private float _PlayerCurentLvl = 1f;
    [SerializeField] private float _playerLife = 20f;

    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private Weapons _startingWeapon;

    [SerializeField] private float _maxHeight = 5f;
    [SerializeField] private float _minHeight = -5f;

    private Vector2 _lastDirection = Vector2.right;

    public event EventHandler<OnPlayerUpEventArgs> OnPlayerUp;
    public event EventHandler OnPlayerDeath;

    public float PlayerCurentXp => _PlayerCurentXp;
    public float PlayerSpeed => _playerSpeed;

    private InputSystem_Actions _inputSystem_Actions;
    private PolygonCollider2D _collider;

    public class OnPlayerUpEventArgs : EventArgs
    {
        public float newLevel;
    }

    private void Start()
    {
        _weaponManager.AddWeapon(_startingWeapon);

        _inputSystem_Actions = new InputSystem_Actions();
        _inputSystem_Actions.Player.Enable();

        _collider = GetComponent<PolygonCollider2D>();
    }

    private void Update()
    {
        PlayerMovement();
        PlayerLvlUp();
    }

    private void PlayerMovement()
    {
        Vector2 direction2D = _inputSystem_Actions.Player.Move.ReadValue<Vector2>();

        if (direction2D.x != 0)
            _lastDirection = new Vector2(direction2D.x, 0).normalized;

        Vector3 movement = new Vector3(direction2D.x, direction2D.y, 0f);
        transform.position += movement * _playerSpeed * Time.deltaTime;

        // ✅ bounds du collider (toujours à jour)
        Bounds b = _collider.bounds;

        float halfWidth = b.extents.x;
        float halfHeight = b.extents.y;

        Camera mainCamera = Camera.main;

        float minX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + halfWidth;
        float maxX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - halfWidth;

        // ✅ FIX demandé : limite bas + haut
        float minY = _minHeight + halfHeight;
        float maxY = _maxHeight - halfHeight;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector2(clampedX, clampedY);
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

    private void LevelUp()
    {
        _PlayerCurentLvl++;

        _PlayerXpCap += 5f;
        _PlayerCurentXp = 0f;

        OnPlayerUp?.Invoke(this, new OnPlayerUpEventArgs
        {
            newLevel = _PlayerCurentLvl
        });
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
        OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        _inputSystem_Actions.Player.Disable();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnDestroy()
    {
        _inputSystem_Actions.Player.Disable();
    }
}