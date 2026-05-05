using UnityEngine;

public class Laser : MonoBehaviour
{
    private float _speed = 6f;
    private Vector3 _direction = Vector3.right;
    private static readonly Vector3 BASE_SCALE = new Vector3(0.3f, 0.3f, 0.3f);

    public void Init(float speed, Vector3 direction, float scale = 1f)
    {
        _speed = speed;
        _direction = direction.normalized;

        transform.localScale = BASE_SCALE * scale;
    }

    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
}
