using UnityEngine;
using System;
using UnityEngine.InputSystem;


public class EnemyTest : MonoBehaviour
{
    [Header("Proprietes joueur")]
    [SerializeField] private float _playerSpeed = 8f;
    public float PlayerSpeed => _playerSpeed;

    [SerializeField] private float _maxHeight = 4.5f;

    private InputSystem_Actions _inputSystem_Actions;

    private SpriteRenderer _spriteRenderer;

    private float _minX, _maxX;
    private float _minY, _maxY;


    private void Start()
    {
        _inputSystem_Actions = new InputSystem_Actions();
        _inputSystem_Actions.Player.Enable();

        //_inputSystem_Actions.Player.Attack.performed += Attack_performed;
        PlayerLimits();

        //GameManager.Instance.OnEnemyDestroyed += GameManager_OnEnemyDestroyed;
    }

    /*private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(_canFire < Time.time)
        {
            // Instancier dans le jeu le prefab du laser
            Instantiate(_laserPrefab, transform.position + new Vector3(0f, 0.9f, 0f), Quaternion.identity);
            _canFire = Time.time + _fireRate;
        }
    }*/

    private void OnDestroy()
    {
        _inputSystem_Actions.Player.Disable();
        // _inputSystem_Actions.Player.Attack.performed -= Attack_performed;
    }

    private void Update()
    {
        PlayerMovement();
    }


    private void PlayerLimits()
    {
        Camera mainCamera = Camera.main;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        float halfPlayerWidth = _spriteRenderer.bounds.extents.x;
        float halfPlayerHeight = _spriteRenderer.bounds.extents.y;



        _minX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + halfPlayerWidth;
        _maxX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - halfPlayerWidth;
        _minY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + halfPlayerHeight;
        _maxY = _maxHeight - halfPlayerHeight;
    }

    private void PlayerMovement()
    {
        Vector2 direction2D = _inputSystem_Actions.Player.Move.ReadValue<Vector2>();
        direction2D.Normalize();


        transform.Translate(direction2D * Time.deltaTime * _playerSpeed);


        if (direction2D != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction2D.y, direction2D.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }


        float clampedX = Mathf.Clamp(transform.position.x, _minX, _maxX);
        float clampedY = Mathf.Clamp(transform.position.y, _minY, _maxY);

        transform.position = new Vector2(clampedX, clampedY);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
    }
}
