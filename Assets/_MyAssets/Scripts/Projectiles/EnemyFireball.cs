using UnityEngine;
public class EnemyFireball : MonoBehaviour
{
    private float _speed = 6f;
    private Vector3 _direction = Vector3.right;
    private static readonly Vector3 BASE_SCALE = new Vector3(0.3f, 0.3f, 0.3f);
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    public void Init(float speed, Vector3 direction, float scale = 1f)
    {
        _speed = speed;
        _direction = direction.normalized;
        transform.localScale = BASE_SCALE * scale;
    }

    private void Update()
    {
        // Détruit hors caméra
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.1f || viewPos.x > 1.1f ||
            viewPos.y < -0.1f || viewPos.y > 1.1f)
        {
            Destroy(gameObject);
        }

        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>()?.TakeDamage(1f);
            Destroy(gameObject);
        }
    }

}