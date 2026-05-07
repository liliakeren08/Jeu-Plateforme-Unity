using UnityEngine;
public class Fireball : MonoBehaviour
{
    [SerializeField] private float _homingRange = 0.5f;
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
        // Cherche l'ennemi le plus proche dans le range
        GameObject nearest = FindNearestEnemy();
        if (nearest != null)
        {
            Vector3 toEnemy = (nearest.transform.position - transform.position).normalized;
            _direction = toEnemy;
        }
        transform.position += _direction * _speed * Time.deltaTime;
    }
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = _homingRange;
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
}